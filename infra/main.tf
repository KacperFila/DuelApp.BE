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
    max_replicas    = 1
    revision_suffix = null

    container {
      name   = "duelapp-be"
      image = "duelapp.azurecr.io/duelapp:c99a38ee4c91ce76dedbeaaa34666d8c485518b5"
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
