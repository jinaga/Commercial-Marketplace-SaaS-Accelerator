Here’s how to find each value for the script using the Azure CLI:

---

### 🔐 **APP\_ID** (App Registration ID)

Search by display name (replace `"SaaS Accelerator"` with yours):

```bash
az ad app list --display-name "Jinaga Replicator Portal" --query "[0].appId" -o tsv
```

If you’re unsure of the name, list recent apps:

```bash
az ad app list --query "[].{name:displayName,id:appId}" -o table
```

---

### 🔑 **KEY\_VAULT\_NAME**

List Key Vaults in your subscription:

```bash
az keyvault list --query "[].name" -o table
```

Or narrow it down by resource group:

```bash
az keyvault list --resource-group <your-rg> --query "[].name" -o tsv
```

---

### 🔐 **SECRET\_NAME**

List secrets in a known Key Vault:

```bash
az keyvault secret list --vault-name raas-kv --query "[].name" -o table
```

Look for one named `ADApplicationSecret` or similar.

---

### 🌐 **CUSTOMER\_APP** and **ADMIN\_APP**

List all Web Apps in a resource group:

```bash
az webapp list --resource-group raas-marketplace --query "[].name" -o table
```

Look for names like `raas-portal` and `raas-admin`.

---

### 📦 **RESOURCE\_GROUP**

If you only know the app names:

```bash
az webapp show --name raas-portal --query resourceGroup -o tsv
```

Or use the portal to confirm it from the app's overview.
