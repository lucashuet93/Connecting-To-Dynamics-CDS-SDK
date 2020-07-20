using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.PowerPlatform.Cds.Client;

namespace Connecting_To_Dynamics_CDS_SDK_Walkthrough
{
    public class Function1
    {
        private readonly CdsServiceClient _cdsServiceClient;

        public Function1(CdsServiceClient cdsServiceClient)
        {
            this._cdsServiceClient = cdsServiceClient;
        }

        [FunctionName("Function1")]
        public void Run([ServiceBusTrigger("myqueue", Connection = "ServiceBusConnectionString")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

            ConnectToDynamics(_cdsServiceClient);
        }

        static void ConnectToDynamics(CdsServiceClient service)
        {
            try
            {
                if (service != null)
                {
                    if (service.IsReady)
                    {
                        // execute actions against Dynamics environment
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
