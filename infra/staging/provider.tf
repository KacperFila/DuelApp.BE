terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }

    keycloak = {
      source  = "mrparkers/keycloak"
      version = "~> 4.0"
    }
  }

  backend "azurerm" {
    resource_group_name  = "rg-duelapp-tfstate"
    storage_account_name = "duelappstate25105"
    container_name       = "tfstate-be"
    key                  = "terraform.tfstate"
  }
}

provider "azurerm" {
  features {}
}

provider "keycloak" {
  client_id     = var.keycloak_client_id
  client_secret = var.keycloak_client_secret
  url           = var.keycloak_url
  realm         = "master"
}
