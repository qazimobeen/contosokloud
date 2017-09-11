using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json.Linq;

using Bot_Application.Dialogs;

namespace Bot_Application
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        //public class TicketDetails
        //{
        //    public string Title { get; set; }
        //    public string SubTitle { get; set; }
        //    public string Text { get; set; }
            
        //}
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                if (activity.Text.ToLower().Contains("luis"))
                {
                    // added LUIS 
                    await Conversation.SendAsync(activity, () => new RootLuisDialog());
                }
                else
                {
                    // regular chat
                    //await Conversation.SendAsync(activity, () => new RootDialog());
                    await Conversation.SendAsync(activity, () => new RootLuisDialog());
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }


        private string GetContractDetails()
        {
            return "contract-details";
        }

        //private TicketDetails GetTicketStatus(string ticketNumber)
        //{
        //    TicketDetails tickDetails = null;
        //    string accessToken = "S2xvdWRUcmFpbmluZytxcHBWZkFNZlVWMXJaZ0tKOk1vU1RCdURzMG5MRlp5b3A=";
        //    HttpClient client = new HttpClient();
        //    string url = string.Format("https://api-aus.myconnectwise.net/v2017_5/apis/3.0/service/tickets/{0}", ticketNumber);
        //    client.BaseAddress = new Uri(url);
        //    client.DefaultRequestHeaders.Add("Authorization", "Basic " + accessToken);
  
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //    HttpResponseMessage response = client.GetAsync(url).Result;

        //    if (response.IsSuccessStatusCode)
        //    {
        //        using (HttpContent content = response.Content)
        //        {
        //            Task<string> result = content.ReadAsStringAsync();

        //            JObject o = JObject.Parse(result.Result);

        //            tickDetails = new TicketDetails { Title = ticketNumber, SubTitle = o["Summary"].ToString(), Text = o["status"]["name"].ToString() };
        //        }
        //    }
        //    else
        //    {
        //        //Display unable to receive
        //        //Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
        //    }
        //    return tickDetails;
        //}

        private string GetAccountManagerDetails()
        {
            return "account-manager-details";
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}