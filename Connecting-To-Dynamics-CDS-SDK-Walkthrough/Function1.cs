using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.PowerPlatform.Cds.Client;
using Newtonsoft.Json;

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

            // deserialize message contents into useable class
            ServiceBusMessage serviceBusMessage = JsonConvert.DeserializeObject<ServiceBusMessage>(myQueueItem);

            // execute dynamics actions
            ExecuteDynamicsActions(_cdsServiceClient, serviceBusMessage);
        }

        static void ExecuteDynamicsActions(CdsServiceClient service, ServiceBusMessage serviceBusMessage)
        {
            try
            {
                if (service != null)
                {
                    if (service.IsReady)
                    {
                        // execute WhoAmI and CRUD operations against Dynamics environment
                        ExecuteWhoAmI(service);
                        ExecuteCRUD(service, serviceBusMessage);
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

        static void ExecuteCRUD(CdsServiceClient service, ServiceBusMessage serviceBusMessage)
        {
            // create
            Entity newAccount = new Entity("account");
            newAccount["name"] = serviceBusMessage.AccountName;
            newAccount["address1_postalcode"] = serviceBusMessage.ZipCode;
            Guid accountid = service.Create(newAccount);
            Console.WriteLine("Created {0} entity named {1}.", newAccount.LogicalName, newAccount["name"]);

            // read                      
            ColumnSet attributes = new ColumnSet("name", "ownerid");
            newAccount = service.Retrieve("account", accountid, attributes);
            Console.WriteLine("Retrieved Entity");

            // update
            Entity accountToUpdate = new Entity("account");
            accountToUpdate["accountid"] = newAccount.Id;
            accountToUpdate["revenue"] = new Money(serviceBusMessage.Revenue);
            accountToUpdate["creditonhold"] = false;
            service.Update(accountToUpdate);
            Console.WriteLine("Updated Entity");

            // delete
            service.Delete("account", accountid);
            Console.WriteLine("Deleted Entity");
            return;
        }
    }

    public class ServiceBusMessage { 
        public string AccountName { get; set; }
        public string ZipCode { get; set; }
        public int Revenue { get; set; }
    }
}
