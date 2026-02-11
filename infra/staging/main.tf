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
  name                = "stagingduelappacr${random_integer.suffix.result}"
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
  name                     = "staging-duelapp-kv${random_integer.suffix.result}"
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
  name                = "staging-duelapp-psql${random_integer.suffix.result}"
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

resource "azurerm_key_vault_access_policy" "duelapp_uami_kv_access" {
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
    azurerm_role_assignment.duelapp_uami_acr_pull
  ]

  tags = {
    environment = "staging"
    project     = "duelapp"
    component   = "backend"
  }
}

# =====================================================
# App registrations
# =====================================================
resource "azuread_application" "github_actions_ar" {
  display_name     = "github_actions_ar"
  sign_in_audience = "AzureADMyOrg"

  required_resource_access {
    resource_app_id = "00000003-0000-0000-c000-000000000000"

    resource_access {
      id   = "e1fe6dd8-ba31-4d61-89e7-88639da4683d" # User.Read
      type = "Scope"
    }
  }
}

# =====================================================
# Federated Credentials
# =====================================================
resource "azuread_application_federated_identity_credential" "github_actions_fe" {
  application_id = azuread_application.github_actions_ar.id
  display_name   = "github-actions-fe"
  description    = "Deployments for DuelApp.FE"
  audiences      = ["api://AzureADTokenExchange"]
  issuer         = "https://token.actions.githubusercontent.com"
  subject        = "repo:KacperFila/DuelApp.FE:ref:refs/heads/main"
}

resource "azuread_application_federated_identity_credential" "github_actions_be" {
  application_id = azuread_application.github_actions_ar.id
  display_name   = "github-actions-be"
  description    = "Deployments for DuelApp.BE"
  audiences      = ["api://AzureADTokenExchange"]
  issuer         = "https://token.actions.githubusercontent.com"
  subject        = "repo:KacperFila/DuelApp.BE:ref:refs/heads/main"
}

# =====================================================
# Output suffix for FE infra
# =====================================================
output "suffix" {
  description = "Random numeric suffix used across BE infra"
  value       = random_integer.suffix.result
}
