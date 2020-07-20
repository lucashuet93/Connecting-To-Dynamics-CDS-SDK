# Connecting To Dynamics With the CDS SDK

The repository contains a v3 .NET Azure Function that uses the CDS SDK to execute actions against Dynamics entities. The function app contains a single service bus queue triggered function that uses the message contents to create a new Account entity in a Dynamics environment, then perform read, update, and delete operations on the entity.

The README is designed to walk the user through configuration of a Dynamics environment and execution of the sample function.

## Pre-Requisites

- .NET Core ([Instructions](https://docs.microsoft.com/en-us/dotnet/core/install/))

- Azure Functions Core Tools ([Instructions](https://github.com/Azure/azure-functions-core-tools))

- Dynamics Environment with a Common Data Service Database ([Instructions](https://docs.microsoft.com/en-us/power-platform/admin/create-environment))

- Service Bus Namespace and Queue ([Instructions](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-quickstart-portal))

- Service Bus Explorer ([Instructions](https://github.com/paolosalvatori/ServiceBusExplorer))

## Set Up Client Secret Authentication

In order to connect to Dynamics via the CDS SDK, client secret authentication must be utilized. The flow essentially requires that a service principal be granted access to the Dynamics environment. An Azure Active Directory application must be created in the tenant that contains the pre-provisioned Dynamics environment, and that application must be added to the Dynamics environment as an 'Application User' by a Dynamics system administrator.

### Create an Azure Active Directory App

- Navigate to the [Azure Portal](https://portal.azure.com) and sign in with an account within the directory that contains the Dynamics environment.

- Search for 'Azure Active Directory' and select it.

- Select the 'App registrations' blade, then click '+ Add Registration'.

- Give the app a name. Under 'Supported account types' select 'Accounts in this organizational directory only' and ensure the 'Redirect Uri' is set to type 'Public Client/native (mobile & desktop)'.

- Register the app.

### Create an Application User

_Note: The following steps must be executed by a user with the 'System Administrator' role in Dynamics_

- Navigate to the [Power Platform Admin Center](https://admin.powerplatform.com).

- Find the 'Environments' blade and select the proper environment. Open the 'Environment URL'.

- Click the Settings icon and select 'Advanced Settings' from the dropdown, which loads the environment settings page.

- Find the Settings dropdown and select 'Security' under 'System'.

- Select the 'Users' link.

- Switch the view to 'Application Users' (default is 'Enabled Users'), then click '+ New'.

- Input the Application ID and Object ID from the Azure AD app in the 'Application ID' and 'Azure AD Object ID' fields in the form, respectively. Fill in the other fields with any value and click 'Save'.

## Run the Azure Function

The function app uses the ```v0.2.14-Alpha``` prerelease version of the ```Microsoft.PowerPlatform.Cds.Client``` package for interaction witt the CDS and Dynamics, ```Microsoft.Azure.Functions.Extensions``` for Azure Functions dependency injection, and ```Newtonsoft.Json``` for basic JSON deserialization.

The function app's single function, Function1, is designed to connect to Dynamics using the the CdsServiceClient class provided by the SDK. The function then uses the service client to execute WhoAmI and CRUD operations against the environment. The function gains access to a singleton CdsServiceClient through dependency injection so as to not recreate the service client on each invocation.

### Restore the Project Dependencies

Open the project in a cmd window and run ```dotnet restore```.

### Create the local.settings.json file

The sample.local.settings.json file serves as an example for the local.settings.json file that defines the environment variables for the function. Rename the file to local.settings.json and fill in the empty fields.

- ```DynamicsEnvironmentUrl``` is the url that was opened via the [Power Platform Admin Center](https://admin.powerplatform.com) (e.g. https://orga000c00a.crm.dynamics.com).

- ```DynamicsClientId``` and ```DynamicsClientSecret``` can be found on the Active Directory app registration on the Azure Portal.

- ```ServiceBusConnectionString``` can be found in the 'Shared access policies' blade of the Service Bus namespace in the Azure portal.

### Update the Trigger Attributes

The sample function expects the queue to be named **myqueue** by default. Open the [Function1.cs](./Function1.cs) file and update the name of the queue to trigger off of.

### Run the Function

The function can be run directly from Visual Studio or VS Code. Alternatively, open the project in a cmd window and run ```func host start```.

### Send Messages To The Service Bus Queue

- Open the Service Bus Explorer

- Click File -> Connect and input the Service Bus connection string

- When the namespace has been loaded, right click the queue and click 'Send Messages'

- Send a message with 'Message Text' conforming to the following structure:

```json
{"AccountName":"SampleAccount","ZipCode":"48103","Revenue":100}
```
