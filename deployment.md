Run the following script on the Azure Portal to deploy:

```bash
wget https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh; `
chmod +x dotnet-install.sh; `
./dotnet-install.sh -version 6.0.417; `
$ENV:PATH="$HOME/.dotnet:$ENV:PATH"; `
dotnet tool install --global dotnet-ef --version 6.0.1; `
git clone https://github.com/jinaga/Commercial-Marketplace-SaaS-Accelerator.git -b jinaga-landing-page --depth 1; `
cd ./Commercial-Marketplace-SaaS-Accelerator/deployment; `
.\Upgrade.ps1 `
 -WebAppNamePrefix "raas" `
 -ResourceGroupForDeployment "raas-marketplace" `
 ```