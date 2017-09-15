using Bot_Application.Entities.AWS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Script.Serialization;

namespace Bot_Application.Helper
{
    public class AWSHelper
    {
        public const string awsURI = @"https://kloud-thunderhead-func-dev-aue.azurewebsites.net/api/zendesk/tickets/aws/ec2/instances/operations?code=9W1egedDlQRviJXGtM4BEHfFGdXvN9K9WPQPlSd0fTjj5kzl72bCKA==";

        /// <summary>
        /// Posts an <see cref="AWSPostMessage"/> to the <see cref="awsURI"/> 
        /// to schedule an operation to be performed via AWS on a VM.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private static HttpResponseMessage GetAWSResponse(AWSPostMessage postMessage)
        {
            HttpClient httpClient = new HttpClient()
            {
                BaseAddress = new Uri(awsURI)
            };
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var json = new JavaScriptSerializer().Serialize(postMessage);
            var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync(awsURI, content);
            response.Wait();
            return response.Result;
        }

        /// <summary>
        /// Returns a list of available VMs.
        /// </summary>
        /// <returns><see cref="Dictionary{TKey, TValue}"/></returns>
        public static Dictionary<string, string> GetVMs()
        {
            return new Dictionary<string, string>
            {
                { "KloudMSHackfestInstance", "i-074c35e1a36b0ae3d" }
            };
        }

        /// <summary>
        /// Gets the VM based on the name provided.
        /// </summary>
        /// <param name="vmName"></param>
        /// <returns>instanceID of the VM</returns>
        public static string GetVMInstanceId(string vmName)
        {
            var resolvedVmInstanceId = string.Empty;
            if (!string.IsNullOrEmpty(vmName))
            {
                var result = GetVMs()
                    .Where(x => x.Key.ToUpper() == vmName.ToUpper())
                    .Select(x => (KeyValuePair<string, string>?)x)
                    .FirstOrDefault();
                resolvedVmInstanceId = result.HasValue ? result.Value.Value : string.Empty;
            }
            return resolvedVmInstanceId;
        }

        /// <summary>
        /// Runs the requested action against the target VM.
        /// </summary>
        /// <param name="instanceId">The target VM</param>
        /// <param name="serviceMessage">The service message to parse</param>
        /// <param name="scheduledTime">the scheduled time</param>
        /// <returns></returns>
        public static bool RunOperation(string instanceId, ServiceMessage serviceMessage, DateTime? scheduledTime = null)
        {
            HttpResponseMessage response = GetAWSResponse(CreateAWSPostMessage(instanceId, serviceMessage, scheduledTime));
            return response.IsSuccessStatusCode;
        }

        public static List<string> GetResizeFamilies()
        {
            return Enum.GetNames(typeof(InstanceFamilyType)).ToList();
        }

        public static List<string> GetStorageInstanceTypes()
        {
            return Enum.GetNames(typeof(StorageInstanceType)).ToList();
        }

        /// <summary>
        /// Creates a POST <see cref="AWSPostMessage"/> that will parse the <paramref name="serviceMessage"/> and determine
        /// what actions to do for the <paramref name="instanceId"/> provided, by the <paramref name="scheduledTime"/> supplied.
        /// </summary>
        /// <param name="instanceId">The target VM</param>
        /// <param name="serviceMessage">The service message to parse</param>
        /// <param name="scheduledTime">the scheduled time</param>
        /// <returns></returns>
        private static AWSPostMessage CreateAWSPostMessage(string instanceId, ServiceMessage serviceMessage, DateTime? scheduledTime = null)
        {
            if (serviceMessage != null)
            {
                // default descriptions
                var operationTypeName = GetNameOfOperationType(serviceMessage.OperationType);
                var serviceDescription = operationTypeName + " an existing Amazon EC2 instance";
                var serviceName = operationTypeName + " Amazon EC2 instance";
                var ticketDescription = operationTypeName + " an existing Amazon virtual machine";
                var ticketSubject = operationTypeName + " Amazon Virtual Machine";
                // format descriptions for different actions
                switch (serviceMessage.OperationType)
                {
                    case OperationType.Start:
                    case OperationType.Stop:
                    case OperationType.Restart:
                    {
                        serviceDescription = "Restart, start, or stop an existing Amazon EC2 instance";
                        serviceName = @"Restart/start/stop Amazon EC2 instance";
                        ticketDescription = operationTypeName + " an existing Amazon virtual machine";
                        ticketSubject = operationTypeName + " Amazon Virtual Machine";
                    }
                    break;
                }

                // compose the post message
                return new AWSPostMessage
                {
                    CompanyId = 250,
                    ServiceDescription = serviceDescription,
                    ServiceId = serviceMessage.CurrentServiceId,
                    ServiceName = serviceName,
                    TicketDescription = ticketDescription,
                    TicketFields = new TicketFields
                    {
                        AccountId = "411808402448",
                        ChangeNumber = 1,
                        InstanceId = instanceId,
                        OperationName = operationTypeName.ToLower(),
                        InstanceType = serviceMessage.ServiceConfiguration != null
                            ? serviceMessage.ServiceConfiguration.SelectedInstanceType
                            : string.Empty,
                        ScheduledTimestamp = scheduledTime.HasValue
                        ? scheduledTime.Value.ToString("s")
                        : DateTime.UtcNow.ToString("s")
                    },
                    TicketId = 1,
                    TicketRequesterEmail = "39fd55d0.kloud.com.au@apac.teams.ms",
                    TicketRequesterName = "Contoso Managed Services",
                    TicketRequesterPhone = "+61439391813",
                    TicketSubject = ticketSubject
                };
            }
            // just return an empty message if the serviceMessage provided was invalid
            return new AWSPostMessage();
        }

        /// <summary>
        /// Capitalises the first letter of the <paramref name="operationName"/> supplied.
        /// This should be used for pretty text formatting.
        /// </summary>
        /// <param name="operationName"></param>
        /// <returns></returns>
        private static string CapitaliseOperationName(string operationName)
        {
            var result = new StringBuilder();
            result.Append(operationName.First().ToString().ToUpper());
            result.Append(operationName.Substring(1));
            return result.ToString();
        }

        /// <summary>
        /// Gets the enum name of the <see cref="OperationType"/> provided.
        /// </summary>
        /// <param name="type">the type to get the name of</param>
        /// <returns>The name of the enum value</returns>
        private static string GetNameOfOperationType(OperationType type)
        {
            return Enum.GetName(typeof(OperationType), type);
        }
    }
}