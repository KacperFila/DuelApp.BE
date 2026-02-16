# =====================================================
# Random values
# =====================================================

resource "random_password" "postgres_admin_password" {
  length  = 24
  special = true
}

# =====================================================
# Resource Group
# =====================================================
resource "azurerm_resource_group" "rg_duelapp_be_staging" {
  location = "polandcentral"
  name     = "rg-duelapp-be-staging"

  tags = {
    environment = "staging"
    project     = "duelapp"
  }
}

# =====================================================
# Azure Container Registry
# =====================================================
resource "azurerm_container_registry" "duelapp_acr" {
  name                = "stagingduelappacr"
  resource_group_name = azurerm_resource_group.rg_duelapp_be_staging.name
  location            = azurerm_resource_group.rg_duelapp_be_staging.location
  sku                 = "Basic"
  admin_enabled       = false

  tags = {
    environment = "staging"
    project     = "duelapp"
    component   = "backend"
  }
}

# =====================================================
# Container Apps Environment
# =====================================================
resource "azurerm_container_app_environment" "duelapp_env" {
  name                = "staging-duelapp-env"
  location            = azurerm_resource_group.rg_duelapp_be_staging.location
  resource_group_name = azurerm_resource_group.rg_duelapp_be_staging.name

  tags = {
    environment = "staging"
    project     = "duelapp"
    component   = "backend"
  }
}

# =====================================================
# Key Vault
# =====================================================
data "azurerm_client_config" "current" {}

resource "azurerm_key_vault" "duelapp_kv" {
  name                     = "staging-duelapp-kv"
  location                 = azurerm_resource_group.rg_duelapp_be_staging.location
  resource_group_name      = azurerm_resource_group.rg_duelapp_be_staging.name
  tenant_id                = data.azurerm_client_config.current.tenant_id
  sku_name                 = "standard"
  purge_protection_enabled = false
  enable_rbac_authorization = true

  tags = {
    environment = "staging"
    project     = "duelapp"
    component   = "backend"
  }
}

# =====================================================
# PostgreSQL Flexible Server
# =====================================================
resource "azurerm_postgresql_flexible_server" "postgres" {
  name                = "staging-duelapp-psql"
  resource_group_name = azurerm_resource_group.rg_duelapp_be_staging.name
  location            = azurerm_resource_group.rg_duelapp_be_staging.location

  administrator_login    = "psqladmin"
  administrator_password = random_password.postgres_admin_password.result

  sku_name   = "B_Standard_B1ms"
  version    = "15"
  storage_mb = 32768

  backup_retention_days         = 7
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
    component   = "backend"
  }
}

# =====================================================
# PostgreSQL Firewall Rules
# =====================================================
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

  depends_on = [
    azurerm_role_assignment.terraform_kv_secret_officer
  ]

  lifecycle {
    ignore_changes = [value]
  }
}

resource "azurerm_key_vault_secret" "postgres_admin_password" {
  name         = "postgres--admin-password"
  value        = random_password.postgres_admin_password.result
  key_vault_id = azurerm_key_vault.duelapp_kv.id

  depends_on = [
    azurerm_role_assignment.terraform_kv_secret_officer
  ]

  lifecycle {
    ignore_changes = [value]
  }
}

# =====================================================
# User Assigned Managed Identity
# =====================================================
resource "azurerm_user_assigned_identity" "duelapp_uami" {
  name                = "uami-duelapp-staging"
  resource_group_name = azurerm_resource_group.rg_duelapp_be_staging.name
  location            = azurerm_resource_group.rg_duelapp_be_staging.location

  tags = {
    environment = "staging"
    project     = "duelapp"
    component   = "backend"
  }
}

# =====================================================
# Permissions
# =====================================================
resource "azurerm_role_assignment" "duelapp_uami_acr_pull" {
  scope                = azurerm_container_registry.duelapp_acr.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.duelapp_uami.principal_id
}

resource "azurerm_role_assignment" "duelapp_uami_kv_access" {
  scope                = azurerm_key_vault.duelapp_kv.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.duelapp_uami.principal_id
}

data "azuread_service_principal" "github_actions" {
  display_name = "github-actions-oidc"
}

resource "azurerm_role_assignment" "github_actions_kv_secret_officer" {
  scope                = azurerm_key_vault.duelapp_kv.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = data.azuread_service_principal.github_actions.object_id
}

resource "azurerm_role_assignment" "terraform_kv_secret_officer" {
  scope                = azurerm_key_vault.duelapp_kv.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = data.azurerm_client_config.current.object_id
}

# =====================================================
# Container App
# =====================================================
resource "azurerm_container_app" "duelapp_be" {
  name                         = "staging-duelapp-be"
  resource_group_name          = azurerm_resource_group.rg_duelapp_be_staging.name
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

  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "auto"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
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
      image  = var.image_tag
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
    azurerm_role_assignment.duelapp_uami_acr_pull,
    azurerm_role_assignment.duelapp_uami_kv_access
  ]

  tags = {
    environment = "staging"
    project     = "duelapp"
    component   = "backend"
  }
}