# =====================================================
# Random values
# =====================================================

resource "random_integer" "suffix" {
  min = 10000
  max = 99999
}

resource "random_password" "postgres_admin_password" {
  length  = 24
  special = true
}

# =====================================================
# Resource Group
# =====================================================

resource "azurerm_resource_group" "duelapp_rg" {
  name     = "rg-duelapp-staging"
  location = "polandcentral"

  tags = {
    environment = "staging"
    project     = "duelapp"
  }
}

# =====================================================
# Azure Container Registry
# =====================================================

resource "azurerm_container_registry" "duelapp_acr" {
  name                = "stagingduelappacr${random_integer.suffix.result}"
  resource_group_name = azurerm_resource_group.duelapp_rg.name
  location            = azurerm_resource_group.duelapp_rg.location
  sku                 = "Basic"
  admin_enabled       = false

  tags = {
    environment = "staging"
    project     = "duelapp"
  }
}

# =====================================================
# Container Apps Environment
# =====================================================

resource "azurerm_container_app_environment" "duelapp_env" {
  name                = "staging-duelapp-env"
  location            = azurerm_resource_group.duelapp_rg.location
  resource_group_name = azurerm_resource_group.duelapp_rg.name

  tags = {
    environment = "staging"
    project     = "duelapp"
  }
}

# =====================================================
# Key Vault
# =====================================================

data "azurerm_client_config" "current" {}

resource "azurerm_key_vault" "duelapp_kv" {
  name                     = "staging-duelapp-kv${random_integer.suffix.result}"
  location                 = azurerm_resource_group.duelapp_rg.location
  resource_group_name      = azurerm_resource_group.duelapp_rg.name
  tenant_id                = data.azurerm_client_config.current.tenant_id
  sku_name                 = "standard"
  purge_protection_enabled = false

  tags = {
    environment = "staging"
    project     = "duelapp"
  }
}

# =====================================================
# PostgreSQL Flexible Server
# =====================================================

resource "azurerm_postgresql_flexible_server" "postgres" {
  name                = "staging-duelapp-psql${random_integer.suffix.result}"
  resource_group_name = azurerm_resource_group.duelapp_rg.name
  location            = azurerm_resource_group.duelapp_rg.location

  administrator_login    = "psqladmin"
  administrator_password = random_password.postgres_admin_password.result

  sku_name   = "B_Standard_B1ms"
  version    = "15"
  storage_mb = 32768

  backup_retention_days        = 7
  public_network_access_enabled = true

  lifecycle {
    ignore_changes = [
      zone,
      high_availability,
    ]
  }

  tags = {
    environment = "staging"
    project     = "duelapp"
  }
}

# =====================================================
# PostgreSQL Firewall Rules
# =====================================================

variable "allowed_ips" {
  default = [
    "0.0.0.0",        # Azure services
    "93.159.27.97"    # Kacper Warsaw
  ]
}

resource "azurerm_postgresql_flexible_server_firewall_rule" "allowed" {
  for_each  = toset(var.allowed_ips)
  name      = "allow-${replace(each.value, ".", "-")}"
  server_id = azurerm_postgresql_flexible_server.postgres.id

  start_ip_address = each.value
  end_ip_address   = each.value
}

# =====================================================
# PostgreSQL Connection String
# =====================================================

locals {
  postgres_connection_string = format(
    "Host=%s.postgres.database.azure.com;Database=postgres;Username=%s;Password=%s;Ssl Mode=Require;",
    azurerm_postgresql_flexible_server.postgres.name,
    "psqladmin",
    random_password.postgres_admin_password.result
  )
}

# =====================================================
# Key Vault Secrets
# =====================================================

resource "azurerm_key_vault_secret" "postgres_connection_string" {
  name         = "postgres--connection-string"
  value        = local.postgres_connection_string
  key_vault_id = azurerm_key_vault.duelapp_kv.id
}

resource "azurerm_key_vault_secret" "postgres_admin_password" {
  name         = "postgres--admin-password"
  value        = random_password.postgres_admin_password.result
  key_vault_id = azurerm_key_vault.duelapp_kv.id
}

# =====================================================
# User Assigned Managed Identity
# =====================================================

resource "azurerm_user_assigned_identity" "duelapp_uami" {
  name                = "uami-duelapp-staging"
  resource_group_name = azurerm_resource_group.duelapp_rg.name
  location            = azurerm_resource_group.duelapp_rg.location

  tags = {
    environment = "staging"
    project     = "duelapp"
  }
}

# =====================================================
# Permissions
# =====================================================

resource "azurerm_role_assignment" "acr_pull_uami" {
  scope                = azurerm_container_registry.duelapp_acr.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.duelapp_uami.principal_id
}

resource "azurerm_key_vault_access_policy" "duelapp_uami_policy" {
  key_vault_id = azurerm_key_vault.duelapp_kv.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_user_assigned_identity.duelapp_uami.principal_id

  secret_permissions = [
    "Get",
    "Set",
    "List",
    "Delete"
  ]
}

# =====================================================
# Container App
# =====================================================

resource "azurerm_container_app" "duelapp_be" {
  name                         = "staging-duelapp-be"
  resource_group_name          = azurerm_resource_group.duelapp_rg.name
  container_app_environment_id = azurerm_container_app_environment.duelapp_env.id
  revision_mode                = "Single"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.duelapp_uami.id]
  }

  registry {
    server   = azurerm_container_registry.duelapp_acr.login_server
    identity = azurerm_user_assigned_identity.duelapp_uami.id
  }

  secret {
    name                = "postgres--connection-string"
    key_vault_secret_id = azurerm_key_vault_secret.postgres_connection_string.id
    identity            = azurerm_user_assigned_identity.duelapp_uami.id
  }

  template {
    min_replicas = 1
    max_replicas = 1

    container {
      name   = "staging-duelapp-be"
      image  = "${azurerm_container_registry.duelapp_acr.login_server}/duelapp:fc5a7bcd1eea1b9bdd5f7d72c9beecf2d6693368"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name        = "Postgres__ConnectionString"
        secret_name = "postgres--connection-string"
      }

      env {
        name  = "Auth__IssuerSigningKey"
        value = "placeholder-for-testing"
      }

      env {
        name  = "KEYVAULT_NAME"
        value = azurerm_key_vault.duelapp_kv.name
      }

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Staging"
      }
    }
  }

  depends_on = [
    azurerm_role_assignment.acr_pull_uami
  ]

  tags = {
    environment = "staging"
    project     = "duelapp"
  }
}
