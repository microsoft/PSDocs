# AKS deployment with a virtual network

This template creates a AKS cluster using a virtual network in the same resource group

## Parameters

Parameter name | Description
-------------- | -----------
resourceName   | The name of the Managed Cluster resource.
environment    | The environment that the resource will be deployed to. Either production or internal.
location       | The location of AKS resource.
dnsPrefix      | Optional DNS prefix to use with hosted Kubernetes API server FQDN.
agentCount     | The number of agent nodes for the cluster.
agentVMSize    | The size of the Virtual Machine.
servicePrincipalClientId | Client ID (used by cloudprovider)
servicePrincipalClientSecret | The Service Principal Client Secret.
osType         | The type of operating system.
kubernetesVersion | The version of Kubernetes.
enableOmsAgent | boolean flag to turn on and off of omsagent addon
workspaceRegion | Specify the region for your OMS workspace
workspaceName  | Specify the name of the OMS workspace
omsWorkspaceId | Specify the resource id of the OMS workspace
omsSku         | Select the SKU for your workspace
enableHttpApplicationRouting | boolean flag to turn on and off of http application routing
networkPlugin  | Network plugin used for building Kubernetes network.
maxPods        | Maximum number of pods that can run on a node.
vnetSubnetID   | Resource ID of virtual network subnet used for nodes and/or pods IP assignment.
serviceCidr    | A CIDR notation IP range from which to assign service cluster IPs.
dnsServiceIP   | Containers DNS server IP address.
dockerBridgeCidr | A CIDR notation IP for Docker bridge.

## Use the template

### PowerShell

```powershell
New-AzResourceGroupDeployment -Name <deployment-name> -ResourceGroupName <resource-group-name> -TemplateFile <path-to-template>
```

### Azure CLI

```text
az group deployment create --name <deployment-name> --resource-group <resource-group-name> --template-file <path-to-template>
```
