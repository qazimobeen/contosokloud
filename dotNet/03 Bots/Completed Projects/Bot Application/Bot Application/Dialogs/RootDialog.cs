using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams; //Teams bot extension SDK
using Microsoft.Bot.Connector.Teams.Models;
//using TeamsSampleTaskApp.Utils;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Bot_Application
{
    /// <summary>
    /// Basic dialog implemention showing how to create an interactive chat bot.
    /// </summary>
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public class TicketDetails
        {
            public string Title { get; set; }
            public string SubTitle { get; set; }
            public string Text { get; set; }
        }

        public class CompanyDetails
        {
            public string Name { get; set; }
            public string Address { get; set; }
            public AMContactDetails AMContactDetails { get; set; }
        }
        public class AMContactDetails
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
        }

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        /// <summary>
        /// This is where you can process the incoming user message and decide what to do.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            //Strip out all mentions.  As all channel messages to a bot must @mention the bot itself, you must strip out the bot name at minimum.
            // This uses the extension SDK function GetTextWithoutMentions() to strip out ALL mentions
            var text = activity.GetTextWithoutMentions().ToLower();

            //Supports 5 commands:  Help, Welcome (sent from HandleSystemMessage when bot is added), Create, Find, Assign, and Link 
            //  This simple text parsing assumes the command is the first string, and an optional parameter is the second.
            var split = text.ToLower().Split(' ');
            if (split.Length < 2)
            {
                if (text.Contains("contact"))
                {
                    //TODO: Return account manager info
                    await SendHelpMessage(context, "Sure, I can provide account manager info for this company");
                }
                else if (text.Contains("status"))
                {
                    //TODO : Prompt user for ticket number if not provided
                    await SendHelpMessage(context, "Sure, I can provide info about the status of this ticket");
                }
                else if (text.Contains("tickets"))
                {
                    await SendHelpMessage(context, "Sure, I can provide help info about this ticket");
                }
                else
                {
                    await SendHelpMessage(context, "I'm sorry, I did not understand you :(");
                }
            }
            else
            {
                var q = split.Skip(1);
                var cmd = split[0];

                // Parse the command and go do the right thing
                if (cmd.Contains("status"))
                {
                    TicketDetails ticketDetails =  GetTicketStatus(string.Join(" ", q));
                    await SendStatusMessage(context, ticketDetails);
                }
                else if (cmd.Contains("contact"))
                {
                    CompanyDetails companyDetails = GetContactDetails(string.Join(" ", q));
                     await SendContactDetailsMessage(context, companyDetails);
                }
                //else if (cmd.Contains("link"))
                //{
                //    // await SendDeeplink(context, activity, string.Join(" ", q));
                //}
                else
                {
                    await SendHelpMessage(context, "I'm sorry, I did not understand you :(");
                }
            }

            context.Wait(MessageReceivedAsync);
        }

        private async Task SendContactDetailsMessage(IDialogContext context, CompanyDetails companyDetails)
        {
            IMessageActivity reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();

            if (companyDetails != null)
            {
                ThumbnailCard card = new ThumbnailCard()
                {

                    Title = $"{companyDetails.AMContactDetails.Name}",
                    Subtitle = $"{companyDetails.AMContactDetails.Email}",
                    Text = companyDetails.AMContactDetails.PhoneNumber,
                    Images = new List<CardImage>()
                {
                    new CardImage()
                    {
                        Url = $"https://contosomanagedservices.azurewebsites.net/images/kloud88.png"
                    }
                }
                };

                //card.Buttons = new List<CardAction>()
                //{
                //    new CardAction("openUrl", "View task", null, "https://www.microsoft.com"),
                //    new CardAction("imBack", "Assign to me", null, $"assign {taskItem.Guid}")
                //};

                reply.Attachments.Add(card.ToAttachment());
            }
            else
            {
                reply.Text = $"Company could not be found";
            }
            

            ConnectorClient client = new ConnectorClient(new Uri(context.Activity.ServiceUrl));
            ResourceResponse resp = await client.Conversations.ReplyToActivityAsync((Activity)reply);
        }

        /// <summary>
        /// Helper method to create a simple task card and send it back as a message.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="taskItemTitle"></param>
        /// <returns></returns>
        private async Task SendStatusMessage(IDialogContext context, TicketDetails ticketDetails)
        {
            IMessageActivity reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();

            if (ticketDetails != null)
            {

                ThumbnailCard card = new ThumbnailCard()
                {
                    Title = $"{ticketDetails.Title}",
                    Subtitle = $"#{ticketDetails.SubTitle}",
                    Text = ticketDetails.Text,
                    Images = new List<CardImage>()
                {
                    new CardImage()
                    {
                        Url = $"https://contosomanagedservices.azurewebsites.net/images/kloud88.png"
                    }
                }
                };

                //card.Buttons = new List<CardAction>()
                //{
                //    new CardAction("openUrl", "View task", null, "https://www.microsoft.com"),
                //    new CardAction("imBack", "Assign to me", null, $"assign {taskItem.Guid}")
                //};

                reply.Attachments.Add(card.ToAttachment());
            }
            else
            {
                reply.Text = $"Ticket could not be found";
            }

            ConnectorClient client = new ConnectorClient(new Uri(context.Activity.ServiceUrl));
            ResourceResponse resp = await client.Conversations.ReplyToActivityAsync((Activity)reply);

            // Cache the response activity ID and previous task card.
            //string activityId = resp.Id.ToString();
            //context.ConversationData.SetValue("task " + taskItem.Guid, new Tuple<string, ThumbnailCard>(activityId, card));
        }

        /// <summary>
        /// Helper method to update an existing message for the given task item GUID.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="taskItemGuid"></param>
        /// <returns></returns>
        private async Task UpdateMessage(IDialogContext context, string taskItemGuid)
        {
            Tuple<string, ThumbnailCard> cachedMessage;

            //Retrieve passed task guid from the ConversationData cache
            if (context.ConversationData.TryGetValue("task " + taskItemGuid, out cachedMessage))
            {
                IMessageActivity reply = context.MakeMessage();
                reply.Attachments = new List<Attachment>();

                string activityId = cachedMessage.Item1;
                ThumbnailCard card = cachedMessage.Item2;

                card.Subtitle = $"Assigned to: {context.Activity.From.Name}";

                card.Buttons = new List<CardAction>()
                {
                    new CardAction("openUrl", "View task", null, "https://www.microsoft.com"),
                    new CardAction("openUrl", "Update details", null, "https://www.microsoft.com")
                };

                reply.Attachments.Add(card.ToAttachment());

                ConnectorClient client = new ConnectorClient(new Uri(context.Activity.ServiceUrl));
                ResourceResponse resp = await client.Conversations.UpdateActivityAsync(context.Activity.Conversation.Id, activityId, (Activity)reply);
            } else
            {
                System.Diagnostics.Debug.WriteLine($"Could not update task {taskItemGuid}");
            }
        }

        /// <summary>
        /// Helper method to create a deep link to a given tab name.  
        /// 
        /// For more information, see: https://msdn.microsoft.com/en-us/microsoft-teams/deeplinks#generating-a-deep-link-to-your-tab-for-use-in-a-bot-or-connector-message
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="activity"></param>
        /// <param name="tabName"></param>
        /// <returns></returns>
        //private async Task SendDeeplink(IDialogContext context, Activity activity, string tabName)
        //{
        //    var teamsChannelData = activity.GetChannelData<TeamsChannelData>();
        //    var teamId = teamsChannelData.Team.Id;
        //    var channelId = teamsChannelData.Channel.Id;

        //    // The app ID, stored in the web.config file, should be the appID from your manifest.json file.
        //    var appId = System.Configuration.ConfigurationManager.AppSettings["TeamsAppId"];
        //    var entity = $"todotab-{tabName}-{teamId}-{channelId}"; // Match the entity ID we setup when configuring the tab
        //    var tabContext = new TabContext()
        //    {
        //        ChannelId = channelId,
        //        CanvasUrl = "https://teams.microsoft.com"
        //    };

        //    var url = $"https://teams.microsoft.com/l/entity/{HttpUtility.UrlEncode(appId)}/{HttpUtility.UrlEncode(entity)}?label={HttpUtility.UrlEncode(tabName)}&context={HttpUtility.UrlEncode(JsonConvert.SerializeObject(tabContext))}";

        //    var text = $"I've created a deep link to {tabName}! Click [here]({url}) to navigate to the tab.";
        //    await context.PostAsync(text);
        //}

        /// <summary>
        /// Helper method to send a simple help message.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="firstLine"></param>
        /// <returns></returns>
        private async Task SendHelpMessage(IDialogContext context, string firstLine)
        {
            var helpMessage = $"{firstLine} \n\n Here's what I can help you do \n\n"
                + "* To create a new task, you can type **create** followed by the task name\n"
                + "* To find an existing task, you can type **find** followed by the task name\n"
                + "* To create a deep link, you can type **link** followed by the tab name";

            await context.PostAsync(helpMessage);
        }

        private CompanyDetails GetContactDetails(string companyId)
        {
            CompanyDetails cDetails = null;
            AMContactDetails amContactDetails = null;
            string accessToken = "S2xvdWRUcmFpbmluZytxcHBWZkFNZlVWMXJaZ0tKOk1vU1RCdURzMG5MRlp5b3A=";
            HttpClient clientCompany = new HttpClient();
            string urlCompany = string.Format("https://api-aus.myconnectwise.net/v2017_5/apis/3.0/company/companies/{0}", companyId);
            clientCompany.BaseAddress = new Uri(urlCompany);
            clientCompany.DefaultRequestHeaders.Add("Authorization", "Basic " + accessToken);

            clientCompany.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = clientCompany.GetAsync(urlCompany).Result;
            JObject companyObject = null;
            if (response.IsSuccessStatusCode)
            {
                using (HttpContent content = response.Content)
                {
                    Task<string> result = content.ReadAsStringAsync();
                    companyObject = JObject.Parse(result.Result);
                    cDetails = new CompanyDetails { Name = companyObject["name"].ToString(), Address = companyObject["addressLine1"].ToString() };
                }

                JObject defaultContactObject = JObject.Parse(companyObject["defaultContact"].ToString());
                string amId = defaultContactObject["id"].ToString();
                string amName = defaultContactObject["name"].ToString();

                string contactInfoUrl = string.Format("https://api-aus.myconnectwise.net/v4_6_release/apis/3.0//company/contacts/{0}/communications", amId);
                HttpClient clientContactInfo = new HttpClient();
                clientContactInfo.BaseAddress = new Uri(contactInfoUrl);
                clientContactInfo.DefaultRequestHeaders.Add("Authorization", "Basic " + accessToken);
                clientContactInfo.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage responsecontactInfo = clientContactInfo.GetAsync(contactInfoUrl).Result;

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
                            if(myElement.Value.ToString() == "Phone")
                            {
                                phoneNumber = itemProperties.FirstOrDefault(x => x.Name == "value").Value.ToString();
                            }
                            if (myElement.Value.ToString() == "Email")
                            {
                                email = itemProperties.FirstOrDefault(x => x.Name == "value").Value.ToString();
                            }
                        }

                        amContactDetails = new AMContactDetails { Name = amName, Email = email, PhoneNumber = phoneNumber};
                        cDetails.AMContactDetails = amContactDetails;
                    }
                }
            }
            return cDetails;
        }

        private TicketDetails GetTicketStatus(string ticketNumber)
        {
            TicketDetails tickDetails = null;
            string accessToken = "S2xvdWRUcmFpbmluZytxcHBWZkFNZlVWMXJaZ0tKOk1vU1RCdURzMG5MRlp5b3A=";
            HttpClient client = new HttpClient();
            string url = string.Format("https://api-aus.myconnectwise.net/v2017_5/apis/3.0/service/tickets/{0}", ticketNumber);
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + accessToken);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                using (HttpContent content = response.Content)
                {
                    Task<string> result = content.ReadAsStringAsync();

                    JObject o = JObject.Parse(result.Result);

                    tickDetails = new TicketDetails { Title = o["summary"].ToString(), SubTitle = ticketNumber, Text = o["status"]["name"].ToString() };
                }
            }

            return tickDetails;
        }
    }
}