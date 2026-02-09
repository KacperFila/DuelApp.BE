variable "image_tag" {
  type    = string
  default = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"
  description = "Docker image tag to deploy to Container App"
}