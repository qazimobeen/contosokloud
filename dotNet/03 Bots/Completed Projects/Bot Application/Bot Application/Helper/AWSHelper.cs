using Bot_Application.Entities.AWS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Bot_Application.Helper
{
    public class AWSHelper
    {
        public const string awsURI = @"https://kloud-thunderhead-func-dev-aue.azurewebsites.net/api/zendesk/tickets/aws/ec2/instances/operations?code=9W1egedDlQRviJXGtM4BEHfFGdXvN9K9WPQPlSd0fTjj5kzl72bCKA==";

        /// <summary>
        /// HTTPResponseMessage
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
            return httpClient.PostAsJsonAsync(awsURI, postMessage).Result;
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
        /// <returns></returns>
        public static bool RunOperation(string instanceId, string operationName)
        {
            HttpResponseMessage response = GetAWSResponse(CreateAWSPostMessage(instanceId, operationName));
            return response.IsSuccessStatusCode;
        }

        private static AWSPostMessage CreateAWSPostMessage(string instanceId, string operationName)
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
                    ScheduledTimestamp = DateTime.UtcNow.AddMinutes(1).ToString("s")
                },
                TicketId = 1,
                TicketRequesterEmail = "39fd55d0.kloud.com.au@apac.teams.mu",
                TicketRequesterName = "Contoso Managed Services",
                TicketRequesterPhone = "+61439391813",
                TicketSubject = CapitaliseOperationName(operationName) + " Amazon Virtual Machine"
            };
        }

        private static string CapitaliseOperationName(string operationName)
        {
            var result = new StringBuilder();
            result.Append(operationName.First().ToString().ToUpper());
            result.Append(operationName.Substring(1));
            return result.ToString();
        }
    }
}