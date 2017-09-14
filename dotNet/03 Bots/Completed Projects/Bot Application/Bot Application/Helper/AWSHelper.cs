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
        /// <param name="operationName">The operation name</param>
        /// <param name="scheduledTime">the scheduled time</param>
        /// <returns></returns>
        public static bool RunOperation(string instanceId, string operationName, DateTime? scheduledTime = null)
        {
            HttpResponseMessage response = GetAWSResponse(CreateAWSPostMessage(instanceId, operationName, scheduledTime));
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Creates a POST <see cref="AWSPostMessage"/> that will invoke the supplied <paramref name="operationName"/> 
        /// based on the <paramref name="instanceId"/> provided, by the <paramref name="scheduledTime"/> supplied.
        /// </summary>
        /// <param name="instanceId">The target VM</param>
        /// <param name="operationName">The operation name</param>
        /// <param name="scheduledTime">the scheduled time</param>
        /// <returns></returns>
        private static AWSPostMessage CreateAWSPostMessage(string instanceId, string operationName, DateTime? scheduledTime = null)
        {
            return new AWSPostMessage
            {
                CompanyId = 250,
                ServiceDescription = "Restart, start, or stop an existing Amazon EC2 instance",
                ServiceId = 2,
                ServiceName = @"Restart/start/stop Amazon EC2 instance",
                TicketDescription = CapitaliseOperationName(operationName) + " an existing Amazon virtual machine",
                TicketFields = new TicketFields
                {
                    AccountId = "411808402448",
                    ChangeNumber = 1,
                    InstanceId = instanceId,
                    OperationName = operationName.ToLower(),
                    ScheduledTimestamp = scheduledTime.HasValue 
                        ? scheduledTime.Value.AddMinutes(1).ToString("s") 
                        : DateTime.UtcNow.AddMinutes(1).ToString("s")
                },
                TicketId = 1,
                TicketRequesterEmail = "39fd55d0.kloud.com.au@apac.teams.ms",
                TicketRequesterName = "Contoso Managed Services",
                TicketRequesterPhone = "+61439391813",
                TicketSubject = CapitaliseOperationName(operationName) + " Amazon Virtual Machine"
            };
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
    }
}