Here are the steps to gather values from Azure and configure your GitHub Actions secrets and variables:

---

### 🔐 1. **Create a GitHub Secret for Azure Authentication**

First, create a service principal with `Contributor` access to the subscription and export credentials:

```bash
az ad sp create-for-rbac --name "github-actions-automation" --sdk-auth
```

Copy the entire JSON output and add it as a GitHub Secret named:

```
AZURE_CREDENTIALS
```

---

### 🔎 2. **Get Your App Registration Object ID**

```bash
az ad app list --display-name "Jinaga Replicator Portal" --query "[0].id" -o tsv
```

Add the result to GitHub Secrets as:

```
APP_REGISTRATION_ID
```

---

### 🔐 3. **Get Key Vault and Secret Names**

You probably already know these, but to look them up:

```bash
az keyvault list --query "[?contains(name,'raas')].name" -o tsv
az keyvault secret list --vault-name "raas-kv" --query "[].name" -o tsv
```

Add these to GitHub Secrets:

```
KEYVAULT_NAME=raas-kv
SECRET_NAME=ADApplicationSecret
```

---

### 🌐 4. **Get App Service Names and Resource Group**

```bash
az webapp list --query "[?contains(name, 'raas')].[name, resourceGroup]" -o table
```

Add the results to GitHub Secrets:

```
CUSTOMER_APP_NAME=raas-portal
ADMIN_APP_NAME=raas-admin
RESOURCE_GROUP=raas-marketplace
```

---

### ✅ Optional: Add a Health Check Secret

If you implement a `/api/system-health` endpoint that uses a shared secret:

Add the secret value to GitHub Secrets:

```
HEALTH_CHECK_SECRET=<your random string>
```

And have your health check use it in an `Authorization` header.
