# Bookstore Infrastructure

## Getting Started

The easiest way to get started is to read up on [ARM Templates](https://docs.microsoft.com/sv-se/azure/azure-resource-manager/templates/overview) to get a basic understanding of structure and syntax.

### Installation

1. Download & Install [Visual Studio Code](https://code.visualstudio.com/)
2. **Optional\*** Enable [Azure Resource Manager (ARM) Tools](https://marketplace.visualstudio.com/items?itemName=msazurermtools.azurerm-vscode-tools) in Visual Studio Code
3. **Optional\*** Install [Azure Az Powershell Module](https://docs.microsoft.com/en-us/powershell/azure/install-az-ps?view=azps-4.8.0)
4. **Optional\*** Install [PS ARM WhatIf](https://docs.microsoft.com/sv-se/azure/azure-resource-manager/templates/template-deploy-what-if?tabs=azure-powershell)

**Azure Resource Manager (ARM) Tools** is not required but recommended to get some syntax intellisense/snippets when editing the ARM template.

**Azure Az Powershell Module** is not required but recommended to be able to run your ARM templates local validation och deployment

**PS ARM WhatIf** is not required but is very useful to execute the -WhatIf switch on the deployment to get a actual 'dryrun' against Azure cloud.

## Build and Test

Build pipelines is defined in yaml in the folder of the Templates.
Each of the infrastructure parts(sub folders) have their own documentation in the readme for Build and Release.

### Local Template Validation and Deployment

For each Arm-Template there is possibilities to Validate templates structure and deploy templates from your local machine.
For this it is required to install the **Azure Az Powershell Module** (See installation section)

#### Steps

1. Open a Powershell terminal and connect the instance to the Azure environment.

   ```Powershell
   Connect-AzAccount
   ```

   A login window should pop up. If it doesn't, search for it (It can hide behind other windows).

2. Get & Select a subscription this instance will work against.

   1. Get all subscriptions (Save the Guid of the relevant subscription id to the next step)

      ```Powershell
      Get-AzSubscription
      ```

   2. Set subscription

      ```Powershell
      Set-AzContext  "[Insert subscription id]"
      ```

3. **Optional:** Use [New-AzResourceGroupDeployment](https://docs.microsoft.com/en-us/powershell/module/az.resources/new-azresourcegroupdeployment?view=azps-4.8.0) to execute -WhatIf on template.

   **This step requires you to run the Installation of WhatIf (See Installation)**

   Examples:
   1.WhatIf Validation of ARM Template

   ```Powershell
   New-AzResourceGroupDeployment -ResourceGroupName "rg-bookstore-dev" -TemplateFile "bookstore.json" -TemplateParameterFile "bookstore.parameters.dev.json" -WhatIf
   ```

   Output of this command will display delta to the actual azure cloud resources

4. Use [New-AzResourceGroupDeployment](https://docs.microsoft.com/en-us/powershell/module/az.resources/new-azresourcegroupdeployment?view=azps-4.8.0) to validate or deploy template.

   Examples:
   1.Validate ARM Template

   ```Powershell
   Test-AzResourceGroupDeployment -ResourceGroupName "rg-bookstore-dev" -TemplateFile "bookstore.json" -TemplateParameterFile "bookstore.parameters.dev.json"  -Verbose
   ```

   2.Deploy ARM Template

   ```Powershell
   New-AzResourceGroup "rg-bookstore-dev"
   New-AzResourceGroupDeployment -ResourceGroupName "rg-bookstore-dev" -TemplateFile "bookstore.json" -TemplateParameterFile "bookstore.parameters.dev.json"  -Verbose
   ```
