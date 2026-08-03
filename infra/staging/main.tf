# =====================================================
# Random values
# =====================================================

resource "random_password" "postgres_admin_password" {
  length  = 24
  special = true
}

resource "random_password" "keycloak_admin_password" {
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
# Service Bus
# =====================================================
resource "azurerm_servicebus_namespace" "duelapp" {
  name                = "staging-duelapp-messaging"
  location            = azurerm_resource_group.rg_duelapp_be_staging.location
  resource_group_name = azurerm_resource_group.rg_duelapp_be_staging.name
  sku                 = "Standard"

  tags = {
    environment = "staging"
    project     = "duelapp"
    component   = "question-imports"
  }
}

resource "azurerm_servicebus_queue" "question_imports" {
  name         = "question-imports"
  namespace_id = azurerm_servicebus_namespace.duelapp.id

  lock_duration                           = "PT5M"
  max_delivery_count                      = 5
  default_message_ttl                     = "P14D"
  dead_lettering_on_message_expiration    = true
  requires_duplicate_detection            = true
  duplicate_detection_history_time_window = "PT10M"
}

# =====================================================
# Event Grid
# =====================================================
resource "azurerm_eventgrid_system_topic" "question_imports" {
  name                   = "staging-duelapp-question-imports-events"
  location               = azurerm_resource_group.rg_duelapp_be_staging.location
  resource_group_name    = azurerm_resource_group.rg_duelapp_be_staging.name
  source_arm_resource_id = azurerm_storage_account.question-imports.id
  topic_type             = "Microsoft.Storage.StorageAccounts"

  identity {
    type = "SystemAssigned"
  }

  tags = {
    environment = "staging"
    project     = "duelapp"
    component   = "question-imports"
  }
}

resource "azurerm_eventgrid_system_topic_event_subscription" "question_imports_blob_created" {
  name                = "question-imports-blob-created"
  system_topic        = azurerm_eventgrid_system_topic.question_imports.name
  resource_group_name = azurerm_resource_group.rg_duelapp_be_staging.name

  service_bus_queue_endpoint_id = azurerm_servicebus_queue.question_imports.id
  event_delivery_schema         = "EventGridSchema"
  included_event_types          = ["Microsoft.Storage.BlobCreated"]

  subject_filter {
    subject_begins_with = "/blobServices/default/containers/question-imports/blobs/imports/"
    subject_ends_with   = "/questions.json"
    case_sensitive      = false
  }

  delivery_identity {
    type = "SystemAssigned"
  }

  storage_blob_dead_letter_destination {
    storage_account_id          = azurerm_storage_account.question-imports.id
    storage_blob_container_name = azurerm_storage_container.question_imports_eventgrid_deadletters.name
  }

  dead_letter_identity {
    type = "SystemAssigned"
  }

  retry_policy {
    event_time_to_live    = 1440
    max_delivery_attempts = 30
  }

  depends_on = [
    azurerm_role_assignment.question_imports_eventgrid_servicebus_sender,
    azurerm_role_assignment.question_imports_eventgrid_deadletter_contributor
  ]
}

# =====================================================
# Key Vault
# =====================================================
data "azurerm_client_config" "current" {}

resource "azurerm_key_vault" "duelapp_kv" {
  name                       = "staging-duelapp-kv"
  location                   = azurerm_resource_group.rg_duelapp_be_staging.location
  resource_group_name        = azurerm_resource_group.rg_duelapp_be_staging.name
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = "standard"
  purge_protection_enabled   = false
  rbac_authorization_enabled = true

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
# Keycloak database
# =====================================================
resource "azurerm_postgresql_flexible_server_database" "keycloak_db" {
  name      = "keycloak_db"
  server_id = azurerm_postgresql_flexible_server.postgres.id
  collation = "en_US.utf8"
  charset   = "UTF8"

  lifecycle {
    prevent_destroy = true
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
# Keycloak Web App Service
# =====================================================
resource "azurerm_linux_web_app" "keycloak" {
  name                = "appkeycloak"
  location            = azurerm_resource_group.rg_duelapp_be_staging.location
  resource_group_name = azurerm_resource_group.rg_duelapp_be_staging.name

  service_plan_id = azurerm_service_plan.spkeycloak.id
  https_only      = true

  site_config {
    always_on                               = false
    container_registry_use_managed_identity = true

    application_stack {
      docker_image_name   = "keycloak:latest"
      docker_registry_url = "https://${azurerm_container_registry.duelapp_acr.login_server}"
    }
  }

  identity {
    type = "SystemAssigned"
  }

  app_settings = {
    "KC_DB" : "postgres"
    "KC_DB_URL_HOST" : azurerm_postgresql_flexible_server.postgres.fqdn
    "KC_DB_URL_PORT" : 5432
    "KC_DB_URL_DATABASE" : azurerm_postgresql_flexible_server_database.keycloak_db.name
    "KC_DB_USERNAME" : "psqladmin"
    "KC_DB_PASSWORD" = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.postgres_admin_password.id})"
    "KC_PROXY" : "edge"
    "WEBSITES_PORT" : 8080
    "KEYCLOAK_ADMIN"          = "admin"
    "KEYCLOAK_ADMIN_PASSWORD" = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.keycloak_admin_password.id})"
  }
}

# =====================================================
# Keycloak service plan
# =====================================================
resource "azurerm_service_plan" "spkeycloak" {
  name                = "sp-duelapp-keycloak"
  location            = azurerm_resource_group.rg_duelapp_be_staging.location
  resource_group_name = azurerm_resource_group.rg_duelapp_be_staging.name

  os_type  = "Linux"
  sku_name = "F1"
}

# =====================================================
# Key Vault Secrets
# =====================================================
resource "azurerm_key_vault_secret" "postgres_connection_string" {
  name         = "postgres-connection-string"
  value        = local.postgres_connection_string
  key_vault_id = azurerm_key_vault.duelapp_kv.id

  depends_on = [
    azurerm_role_assignment.terraform_kv_secret_officer
  ]

  lifecycle {
    ignore_changes = [value]
  }
}

resource "azurerm_key_vault_secret" "keycloak_admin_password" {
  name         = "keycloak-admin-password"
  value        = random_password.keycloak_admin_password.result
  key_vault_id = azurerm_key_vault.duelapp_kv.id

  depends_on = [
    azurerm_role_assignment.terraform_kv_secret_officer
  ]

  lifecycle {
    ignore_changes = [value]
  }
}

resource "azurerm_key_vault_secret" "postgres_admin_password" {
  name         = "postgres-admin-password"
  value        = random_password.postgres_admin_password.result
  key_vault_id = azurerm_key_vault.duelapp_kv.id

  depends_on = [
    azurerm_role_assignment.terraform_kv_secret_officer
  ]

  lifecycle {
    ignore_changes = [value]
  }
}

resource "azurerm_key_vault_secret" "profile_pictures_connection_string" {
  name         = "profile-pictures-connection-string"
  value        = azurerm_storage_account.profile_pictures.primary_connection_string
  key_vault_id = azurerm_key_vault.duelapp_kv.id

  depends_on = [
    azurerm_role_assignment.terraform_kv_secret_officer,
    azurerm_storage_account.profile_pictures
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

resource "azurerm_role_assignment" "duelapp_uami_question_imports_receiver" {
  scope                = azurerm_servicebus_queue.question_imports.id
  role_definition_name = "Azure Service Bus Data Receiver"
  principal_id         = azurerm_user_assigned_identity.duelapp_uami.principal_id
}

resource "azurerm_role_assignment" "duelapp_uami_question_imports_blob_contributor" {
  scope                = azurerm_storage_account.question-imports.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.duelapp_uami.principal_id
}

resource "azurerm_role_assignment" "duelapp_uami_profile_pictures_blob_contributor" {
  scope                = azurerm_storage_account.profile_pictures.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.duelapp_uami.principal_id
}

resource "azurerm_role_assignment" "question_imports_eventgrid_servicebus_sender" {
  scope                = azurerm_servicebus_namespace.duelapp.id
  role_definition_name = "Azure Service Bus Data Sender"
  principal_id         = azurerm_eventgrid_system_topic.question_imports.identity[0].principal_id
}

resource "azurerm_role_assignment" "question_imports_eventgrid_deadletter_contributor" {
  scope                = azurerm_storage_account.question-imports.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_eventgrid_system_topic.question_imports.identity[0].principal_id
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

resource "azurerm_role_assignment" "keycloak_kv_access" {
  scope                = azurerm_key_vault.duelapp_kv.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_linux_web_app.keycloak.identity[0].principal_id
}

resource "azurerm_role_assignment" "keycloak_acr_pull" {
  scope                = azurerm_container_registry.duelapp_acr.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_linux_web_app.keycloak.identity[0].principal_id
}

# =====================================================
# API Service Plan
# =====================================================
resource "azurerm_service_plan" "duelapp_plan" {
  name                = "staging-duelapp-plan"
  location            = azurerm_resource_group.rg_duelapp_be_staging.location
  resource_group_name = azurerm_resource_group.rg_duelapp_be_staging.name

  os_type  = "Linux"
  sku_name = "B1"
}

# =====================================================
# API App Service
# =====================================================
resource "azurerm_linux_web_app" "duelapp_be" {
  name                            = "staging-duelapp-be"
  location                        = azurerm_resource_group.rg_duelapp_be_staging.location
  resource_group_name             = azurerm_resource_group.rg_duelapp_be_staging.name
  service_plan_id                 = azurerm_service_plan.duelapp_plan.id
  key_vault_reference_identity_id = azurerm_user_assigned_identity.duelapp_uami.id

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.duelapp_uami.id]
  }


  site_config {
    container_registry_use_managed_identity       = true
    container_registry_managed_identity_client_id = azurerm_user_assigned_identity.duelapp_uami.client_id

    application_stack {
      docker_image_name   = "duelapp:${var.image_tag}"
      docker_registry_url = "https://${azurerm_container_registry.duelapp_acr.login_server}"
    }

    always_on = true
  }

  app_settings = {
    WEBSITES_PORT = "8080"

    ASPNETCORE_ENVIRONMENT = "Staging"
    AZURE_CLIENT_ID        = azurerm_user_assigned_identity.duelapp_uami.client_id

    KEYVAULT_NAME = azurerm_key_vault.duelapp_kv.name

    Cors__AllowedOrigins__0 = "https://staging-duelapp-fe98179.azurewebsites.net"

    Keycloak__Authority            = "https://appkeycloak.azurewebsites.net/realms/duelapp-realm"
    Keycloak__ClientId             = "duelapp-be-keycloak-client"
    Keycloak__Audience             = "account"
    Keycloak__Issuer               = "https://appkeycloak.azurewebsites.net/realms/duelapp-realm"
    Keycloak__MetadataAddress      = "https://appkeycloak.azurewebsites.net/realms/duelapp-realm/.well-known/openid-configuration"
    Keycloak__RequireHttpsMetadata = "true"

    Postgres__ConnectionString = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.postgres_connection_string.versionless_id})"

    Azure__Storage__ProfilePictures__ConnectionString = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.profile_pictures_connection_string.versionless_id})"
  }

  tags = {
    environment = "staging"
    project     = "duelapp"
    component   = "backend"
  }

  depends_on = [
    azurerm_role_assignment.duelapp_uami_acr_pull,
    azurerm_role_assignment.duelapp_uami_kv_access
  ]
}

# =====================================================
# Storage Account
# =====================================================
resource "azurerm_storage_account" "profile_pictures" {
  name                = "stgduelappprofpic"
  resource_group_name = azurerm_resource_group.rg_duelapp_be_staging.name
  location            = azurerm_resource_group.rg_duelapp_be_staging.location

  account_tier             = "Standard"
  account_replication_type = "LRS"
  account_kind             = "StorageV2"

  min_tls_version = "TLS1_2"

  https_traffic_only_enabled = true

  allow_nested_items_to_be_public = false

  public_network_access_enabled = true

  shared_access_key_enabled = true

  cross_tenant_replication_enabled = false

  access_tier = "Hot"

  blob_properties {
    delete_retention_policy {
      days = 7
    }

    container_delete_retention_policy {
      days = 7
    }
  }

  network_rules {
    default_action = "Allow"
    bypass         = ["AzureServices"]
  }

  identity {
    type = "SystemAssigned"
  }

  tags = {
    environment = "staging"
    component   = "profile-pictures"
  }
}

resource "azurerm_storage_container" "profile_pictures" {
  name                  = "profile-pictures"
  storage_account_name  = azurerm_storage_account.profile_pictures.name
  container_access_type = "private"
}

resource "azurerm_storage_account" "question-imports" {
  name                = "stgduelappqimports"
  resource_group_name = azurerm_resource_group.rg_duelapp_be_staging.name
  location            = azurerm_resource_group.rg_duelapp_be_staging.location

  account_tier             = "Standard"
  account_replication_type = "LRS"
  account_kind             = "StorageV2"

  min_tls_version = "TLS1_2"

  https_traffic_only_enabled = true

  allow_nested_items_to_be_public = false

  public_network_access_enabled = true

  shared_access_key_enabled = true

  cross_tenant_replication_enabled = false

  access_tier = "Hot"

  blob_properties {
    delete_retention_policy {
      days = 7
    }

    container_delete_retention_policy {
      days = 7
    }
  }

  network_rules {
    default_action = "Allow"
    bypass         = ["AzureServices"]
  }

  identity {
    type = "SystemAssigned"
  }

  tags = {
    environment = "staging"
    component   = "question-imports"
  }
}

resource "azurerm_storage_container" "question_imports" {
  name                  = "question-imports"
  storage_account_name  = azurerm_storage_account.question-imports.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "question_imports_eventgrid_deadletters" {
  name                  = "eventgrid-deadletters"
  storage_account_name  = azurerm_storage_account.question-imports.name
  container_access_type = "private"
}

# =====================================================
# Question imports Azure Function
# =====================================================
resource "azurerm_storage_account" "question_imports_function" {
  name                = "stgduelappqifunc"
  resource_group_name = azurerm_resource_group.rg_duelapp_be_staging.name
  location            = azurerm_resource_group.rg_duelapp_be_staging.location

  account_tier             = "Standard"
  account_replication_type = "LRS"
  account_kind             = "StorageV2"

  min_tls_version                  = "TLS1_2"
  https_traffic_only_enabled       = true
  allow_nested_items_to_be_public  = false
  public_network_access_enabled    = true
  shared_access_key_enabled        = true
  cross_tenant_replication_enabled = false
  access_tier                      = "Hot"

  tags = {
    environment = "staging"
    project     = "duelapp"
    component   = "question-imports-functions"
  }
}

resource "azurerm_storage_container" "question_imports_function_deployments" {
  name                  = "function-releases"
  storage_account_id    = azurerm_storage_account.question_imports_function.id
  container_access_type = "private"
}

resource "azurerm_log_analytics_workspace" "question_imports_function" {
  name                = "staging-duelapp-question-imports-functions"
  resource_group_name = azurerm_resource_group.rg_duelapp_be_staging.name
  location            = azurerm_resource_group.rg_duelapp_be_staging.location
  sku                 = "PerGB2018"
  retention_in_days   = 30

  tags = {
    environment = "staging"
    project     = "duelapp"
    component   = "question-imports-functions"
  }
}

resource "azurerm_application_insights" "question_imports_function" {
  name                = "staging-duelapp-question-imports-functions"
  resource_group_name = azurerm_resource_group.rg_duelapp_be_staging.name
  location            = azurerm_resource_group.rg_duelapp_be_staging.location
  application_type    = "web"
  workspace_id        = azurerm_log_analytics_workspace.question_imports_function.id

  tags = {
    environment = "staging"
    project     = "duelapp"
    component   = "question-imports-functions"
  }
}

resource "azurerm_service_plan" "question_imports_function" {
  name                = "staging-duelapp-question-imports-functions"
  resource_group_name = azurerm_resource_group.rg_duelapp_be_staging.name
  location            = azurerm_resource_group.rg_duelapp_be_staging.location
  os_type             = "Linux"
  sku_name            = "FC1"

  tags = {
    environment = "staging"
    project     = "duelapp"
    component   = "question-imports-functions"
  }
}

resource "azurerm_function_app_flex_consumption" "question_imports" {
  name                = "staging-duelapp-question-imports"
  resource_group_name = azurerm_resource_group.rg_duelapp_be_staging.name
  location            = azurerm_resource_group.rg_duelapp_be_staging.location
  service_plan_id     = azurerm_service_plan.question_imports_function.id

  storage_container_type      = "blobContainer"
  storage_container_endpoint  = "${azurerm_storage_account.question_imports_function.primary_blob_endpoint}${azurerm_storage_container.question_imports_function_deployments.name}"
  storage_authentication_type = "StorageAccountConnectionString"
  storage_access_key          = azurerm_storage_account.question_imports_function.primary_access_key

  runtime_name           = "dotnet-isolated"
  runtime_version        = "10.0"
  maximum_instance_count = 1
  instance_memory_in_mb  = 2048

  app_settings = {
    APPLICATIONINSIGHTS_CONNECTION_STRING                        = azurerm_application_insights.question_imports_function.connection_string
    FUNCTIONS_WORKER_RUNTIME                                     = "dotnet-isolated"
    "AzureWebJobs.QuestionImportMessageLoggingFunction.Disabled" = "true"
  }

  identity {
    type = "SystemAssigned"
  }

  site_config {}

  tags = {
    environment = "staging"
    project     = "duelapp"
    component   = "question-imports-functions"
  }
}

resource "azurerm_role_assignment" "github_actions_question_imports_function_contributor" {
  scope                = azurerm_function_app_flex_consumption.question_imports.id
  role_definition_name = "Website Contributor"
  principal_id         = data.azuread_service_principal.github_actions.object_id
}
