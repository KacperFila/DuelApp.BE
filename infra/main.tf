# -----------------------------
# Managed Environment
# -----------------------------
resource "azurerm_container_app_environment" "duelapp_env" {
  name                = "duelapp"
  location            = "polandcentral"
  resource_group_name = "rg-duelapp-be"
}

# -----------------------------
# Container App
# -----------------------------
resource "azurerm_container_app" "duelapp_be" {
  name                        = "duelapp-be"
  resource_group_name          = "rg-duelapp-be"
  container_app_environment_id = azurerm_container_app_environment.duelapp_env.id
  revision_mode                = "Single"

  template {
    min_replicas    = 1
    max_replicas    = 10
    revision_suffix = null

    container {
      name   = "duelapp-be"
      image = "duelapp.azurecr.io/duelapp:af7e0cccfb5c79589cf05ba4932c04c39a2fac03"
      cpu    = 0.25
      memory = "0.5Gi"
      args   = []
      command = []
    }
  }

  lifecycle {
    ignore_changes = [
      identity,
      ingress,
      registry,
      workload_profile_name
    ]
  }
}
