variable "image_tag" {
  type        = string
  default     = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"
  description = "Docker image tag to deploy to Container App"
}

variable "allowed_ips" {
  type = list(string)
  default = [
    "0.0.0.0" # Azure services
  ]
  description = "Allowed IPs by PostgreSQL Firewall"
}

variable "keycloak_client_id" {}

variable "keycloak_client_secret" {
  sensitive = true
}

variable "keycloak_url" {}
