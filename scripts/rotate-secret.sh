#!/bin/bash

set -euo pipefail

# CONFIGURATION
APP_ID="86add91b-2be1-4dae-9cd3-44c7a3eac02d"
KEY_VAULT_NAME="raas-kv"
SECRET_NAME="ADApplicationSecret"
CUSTOMER_APP="raas-portal"
ADMIN_APP="raas-admin"
RESOURCE_GROUP="raas-marketplace"
HEALTH_CHECK_URL="https://raas-admin.azurewebsites.net/api/system-health"

# LOGIN CHECK
az account show > /dev/null || { echo "Please run 'az login' first."; exit 1; }

echo "Rotating secret for App Registration: $APP_ID"

# Set expiration 6 months from now
END_DATE=$(date -u -v+180d '+%Y-%m-%dT%H:%M:%SZ')

# Create new secret
NEW_SECRET=$(az ad app credential reset \
  --id "$APP_ID" \
  --end-date "$END_DATE" \
  --query password -o tsv)

echo "✅ New secret created"

# Store in Key Vault
az keyvault secret set \
  --vault-name "$KEY_VAULT_NAME" \
  --name "$SECRET_NAME" \
  --value "$NEW_SECRET" \
  > /dev/null

echo "✅ Secret stored in Key Vault: $SECRET_NAME"

# Restart apps
az webapp restart --name "$CUSTOMER_APP" --resource-group "$RESOURCE_GROUP"
az webapp restart --name "$ADMIN_APP" --resource-group "$RESOURCE_GROUP"

echo "✅ Apps restarted"
