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

        public static JArray GetTickets(string ClientID)
        {
            ClientID = "19373";
            JArray Obj = null;
            //string accessToken = "S2xvdWRUcmFpbmluZytxcHBWZkFNZlVWMXJaZ0tKOk1vU1RCdURzMG5MRlp5b3A=";
            // HttpClient client = new HttpClient();
            // string ticketInfoUri = "https://api-aus.myconnectwise.net/v4_6_release/apis/3.0/service/tickets?orderby=dateEntered desc&pageSize=5";
            string ticketInfoUri = string.Format("{0}/service/tickets?conditions=company/id={1}&orderby=dateEntered desc&pageSize=5", cwURI, ClientID);

            https://api-aus.myconnectwise.net/v4_6_release/apis/3.0/service/tickets/61295?orderby=dateEntered desc&pageSize=5
            //19321
            //client.BaseAddress = new Uri(url);
            //client.DefaultRequestHeaders.Add("Authorization", "Basic " + accessToken);

            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = GetCWResponse(ticketInfoUri);

            if (response.IsSuccessStatusCode)
            {
                using (HttpContent content = response.Content)
                {
                    Task<string> result = content.ReadAsStringAsync();

                    Obj = JArray.Parse(result.Result);

                    foreach (JObject o in Obj)
                    {
                        Ticket tickDetails = null;
                        //tickDetails = new TicketDetails { Title = o["summary"].ToString() };

                        tickDetails = new Ticket { Title = o["summary"].ToString(), SubTitle = ClientID, Text = o["status"]["name"].ToString(), Id = o["id"].ToString(),
                            recordType = o["recordType"].ToString(), dateEntered = o["dateEntered"].ToString() };

                    }
                }
            }
            else
            {
                //Display unable to receive
                //Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
            return Obj;
        }

        /// <summary>
        /// This function get tickets for to and from date time
        /// </summary>
        /// <param name="ClientID"></param>
        /// <returns></returns>
        public static JArray GetTicketsFromAndTo(string ClientID, DateTime From, DateTime To)
        {
            // TODO: Get Client Id form somewhere else. 
            DateTime.TryParse("2015-02-20T17:04:26Z", out From);
            DateTime.TryParse("2016-02-20T17:04:26Z", out To);

            //From.ToString(isoDateTimeFormat.SortableDateTimePattern);
            From.ToString("yyyy-MM-ddTHH:mm:ssZ");

            JArray Obj = null;
            //string accessToken = "S2xvdWRUcmFpbmluZytxcHBWZkFNZlVWMXJaZ0tKOk1vU1RCdURzMG5MRlp5b3A=";
            // HttpClient client = new HttpClient();
            // string ticketInfoUri = "https://api-aus.myconnectwise.net/v4_6_release/apis/3.0/service/tickets?orderby=dateEntered desc&pageSize=5";
            string ticketInfoUri = string.Format("{0}/service/tickets?conditions=company/id={1} and dateEntered > {2} and dateEntered <  {3} &orderby=dateEntered desc&orderby=dateEntered desc&pageSize=5", cwURI, ClientID, From, To);

            https://api-aus.myconnectwise.net/v4_6_release/apis/3.0/service/tickets/61295?orderby=dateEntered desc&pageSize=5
            //19321
            //client.BaseAddress = new Uri(url);
            //client.DefaultRequestHeaders.Add("Authorization", "Basic " + accessToken);

            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = GetCWResponse(ticketInfoUri);

            if (response.IsSuccessStatusCode)
            {
                using (HttpContent content = response.Content)
                {
                    Task<string> result = content.ReadAsStringAsync();

                    Obj = JArray.Parse(result.Result);

                    foreach (JObject o in Obj)
                    {
                        Ticket tickDetails = null;
                        //tickDetails = new TicketDetails { Title = o["summary"].ToString() };

                        tickDetails = new Ticket
                        {
                            Title = o["summary"].ToString(),
                            SubTitle = ClientID,
                            Text = o["status"]["name"].ToString(),
                            Id = o["id"].ToString(),
                            recordType = o["recordType"].ToString(),
                            dateEntered = o["dateEntered"].ToString()
                        };

                    }
                }
            }
            else
            {
                //Display unable to receive
                //Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
            return Obj;
        }
        public static Company GetHoursDetails(string ClientID)
        {

            Company cDetails = new Company();

            Random rnd1 = new Random();
            cDetails.Id = ClientID;

            Random random = new Random();
            int randomNumber = random.Next(0, 100);

            cDetails.TotalHoursUsed = randomNumber.ToString();
            cDetails.Name = "Qantas";

            return cDetails;
        }
    }
}