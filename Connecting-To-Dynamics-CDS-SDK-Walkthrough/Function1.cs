using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.PowerPlatform.Cds.Client;

namespace Connecting_To_Dynamics_CDS_SDK_Walkthrough
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([ServiceBusTrigger("myqueue", Connection = "ServiceBusConnectionString")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

            ConnectToDynamics();
        }

        static void ConnectToDynamics()
        {
            // build connection string
            string dynamicsEnvironmentUrl = Environment.GetEnvironmentVariable("DynamicsEnvironmentUrl");
            string dynamicsClientId = Environment.GetEnvironmentVariable("DynamicsClientId");
            string dynamicsClientSecret = Environment.GetEnvironmentVariable("DynamicsClientSecret");
            string connectionString = $@"AuthType=ClientSecret;Url={dynamicsEnvironmentUrl};ClientId={dynamicsClientId};ClientSecret={dynamicsClientSecret}";

            // connect to dynamics
            CdsServiceClient service = new CdsServiceClient(connectionString);
            try
            {
                if (service != null)
                {
                    if (service.IsReady)
                    {
                        // execute actions against Dynamics environment
                        Console.WriteLine("Connected to the CDS");
                        ExecuteWhoAmI(service);
                        ExecuteCRUD(service);
                        Console.WriteLine("The sample completed successfully");
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Failed to connect to the CDS");
                        throw new Exception(service.LastCdsError);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                // dispose of the CdsServiceClient instance
                if (service != null)
                    service.Dispose();
            }
        }

        static void ExecuteWhoAmI(CdsServiceClient service)
        {
            WhoAmIRequest request = new WhoAmIRequest();
            WhoAmIResponse response = (WhoAmIResponse)service.Execute(request);
            Console.WriteLine("Your UserID: {0}", response.UserId);
        }

        static void ExecuteCRUD(CdsServiceClient service)
        {
            // CREATE
            Entity newAccount = new Entity("account");
            newAccount["name"] = "LucasInc";
            newAccount["address2_postalcode"] = "48103";
            Guid accountid = service.Create(newAccount);
            Console.WriteLine("Created {0} entity named {1}.", newAccount.LogicalName, newAccount["name"]);

            // READ                      
            ColumnSet attributes = new ColumnSet("name", "ownerid");
            newAccount = service.Retrieve("account", accountid, attributes);
            Console.WriteLine("Retrieved Entity");

            // UPDATE
            Entity accountToUpdate = new Entity("account");
            accountToUpdate["accountid"] = newAccount.Id;
            accountToUpdate["address1_postalcode"] = "48103";
            accountToUpdate["address2_postalcode"] = null;
            accountToUpdate["revenue"] = new Money(5000000);
            accountToUpdate["creditonhold"] = false;
            service.Update(accountToUpdate);
            Console.WriteLine("Updated Entity");

            // DESTROY
            service.Delete("account", accountid);
            Console.WriteLine("Deleted Entity");
            return;
        }
    }
}
