using Bot_Application.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace Bot_Application.Helper
{
    public class ConnectWiseHelper
    {
        public const string cwURI = "https://api-aus.myconnectwise.net/v2017_5/apis/3.0/";

        /// <summary>
        /// HTTPResponseMessage
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private static HttpResponseMessage GetCWResponse(string uri)
        {
            HttpResponseMessage response = null;
            string accessToken = "S2xvdWRUcmFpbmluZytxcHBWZkFNZlVWMXJaZ0tKOk1vU1RCdURzMG5MRlp5b3A=";
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(uri);
            httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + accessToken);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            response = httpClient.GetAsync(uri).Result;

            return response;
        }

        /// <summary>
        /// Returns contact details of the Account Manager of the company
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public static Company GetContactDetails(string companyId)
        {
            Company company = null;
            JObject companyObject = null;
            string urlCompany = string.Format("{0}company/companies/{1}", cwURI, companyId);

            HttpResponseMessage responseCompany = GetCWResponse(urlCompany);

            if (responseCompany.IsSuccessStatusCode)
            {
                using (HttpContent content = responseCompany.Content)
                {
                    Task<string> result = content.ReadAsStringAsync();
                    companyObject = JObject.Parse(result.Result);
                    company = new Company
                    {
                        Name = companyObject["name"].ToString(),
                        Address = companyObject["addressLine1"].ToString()
                    };
                }

                JObject defaultContactObject = JObject.Parse(companyObject["defaultContact"].ToString());
                string accountManagerId = defaultContactObject["id"].ToString();
                string accountManagerName = defaultContactObject["name"].ToString();

                string contactInfoUri = string.Format("{0}company/contacts/{1}/communications", cwURI, accountManagerId);

                HttpResponseMessage responsecontactInfo = GetCWResponse(contactInfoUri);

                if (responsecontactInfo.IsSuccessStatusCode)
                {
                    using (HttpContent content1 = responsecontactInfo.Content)
                    {
                        Task<string> resultContactInfo = content1.ReadAsStringAsync();
                        JArray contactInfoObject = JArray.Parse(resultContactInfo.Result);
                        string phoneNumber = "";
                        string email = "";
                        foreach (var item in contactInfoObject.Children())
                        {
                            var itemProperties = item.Children<JProperty>();
                            var myElement = itemProperties.FirstOrDefault(x => x.Name == "communicationType");
                            if (myElement.Value.ToString() == "Phone")
                            {
                                phoneNumber = itemProperties.FirstOrDefault(x => x.Name == "value").Value.ToString();
                            }
                            if (myElement.Value.ToString() == "Email")
                            {
                                email = itemProperties.FirstOrDefault(x => x.Name == "value").Value.ToString();
                            }
                        }

                        company.AccountManager = new AccountManager
                        {
                            Name = accountManagerName,
                            Email = email,
                            PhoneNumber = phoneNumber
                        };
                    }
                }
            }
            return company;
        }

        /// <summary>
        /// Returns the status of the service ticket
        /// </summary>
        /// <param name="ticketNumber"></param>
        /// <returns></returns>
        public static Ticket GetTicketStatus(string ticketNumber)
        {
            Ticket tickDetails = null;
            string url = string.Format("{0}/service/tickets/{1}", cwURI, ticketNumber);

            HttpResponseMessage response = GetCWResponse(url);

            if (response.IsSuccessStatusCode)
            {
                using (HttpContent content = response.Content)
                {
                    Task<string> result = content.ReadAsStringAsync();
                    JObject ticketObject = JObject.Parse(result.Result);

                    tickDetails = new Ticket
                    {
                        Title = ticketObject["summary"].ToString(),
                        SubTitle = ticketNumber,
                        Text = ticketObject["status"]["name"].ToString()
                    };
                }
            }

            return tickDetails;
        }
    }
}