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
using Bot_Application.Entities;
using Bot_Application.Helper;

namespace Bot_Application
{
    /// <summary>
    /// Basic dialog implemention showing how to create an interactive chat bot.
    /// </summary>
    [Serializable]
    public class RootDialog : IDialog<object>
    {
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

            var teamsChannelData = activity.GetChannelData<TeamsChannelData>();
            var teamId = (teamsChannelData != null && teamsChannelData.Team != null) ? teamsChannelData.Team.Id : "";
            var channelId = (teamsChannelData != null && teamsChannelData.Channel != null) ? teamsChannelData.Channel.Id : "";

            //TODO: Get Customer ID from SharePoint configuration list.
            var customerId = "19321";

            var text = activity.GetTextWithoutMentions().ToLower();

            //Supports 5 commands:  Help, Welcome (sent from HandleSystemMessage when bot is added), Create, Find, Assign, and Link 
            //  This simple text parsing assumes the command is the first string, and an optional parameter is the second.
            var split = text.ToLower().Split(' ');
            if (split.Length < 2)
            {
                var cmd = split[0].Trim().ToLower();
                if (cmd == "contact")
                {
                    Company companyDetails = ConnectWiseHelper.GetContactDetails(customerId);
                    await SendContactDetailsMessage(context, companyDetails);
                }
                else if (cmd == "status")
                {
                    await SendHelpMessage(context, "Sure, I can provide info about the status of a ticket");
                }
                else if (cmd == "ticket")
                {
                    await SendHelpMessage(context, "Sure, I can provide info about a ticket");
                }
                else if (cmd == "tickets")
                {
                    //TODO: Get 5 most recent service tickets
                    await SendHelpMessage(context, "Sure, I can provide help info about this ticket");
                }
                else
                {
                    await SendHelpMessage(context, "I'm sorry, I did not understand you :(");
                }
            }
            else
            {
                var cmd = split[0].Trim().ToLower();

                // Parse the command and go do the right thing
                if (cmd == "status")
                {
                    var ticketNumber = split.Skip(1);
                    Ticket ticketDetails = ConnectWiseHelper.GetTicketStatus(string.Join(" ", ticketNumber));
                    await SendStatusMessage(context, ticketDetails);
                }
                else if (cmd == "ticket")
                {
                    var ticketNumber = split.Skip(1);
                    Ticket ticketDetails = ConnectWiseHelper.GetTicketStatus(string.Join(" ", ticketNumber));
                    await SendStatusMessage(context, ticketDetails);
                }
                else if (cmd.Contains("tickets"))
                {
                    var CustomerId = "19373";
                    var validParams = false;
                    DateTime fromDate = DateTime.MinValue;
                    DateTime toDate = DateTime.MinValue;
                    if (split.Length == 3)
                    {

                        validParams = DateTime.TryParse(split[1], out fromDate);
                        validParams = DateTime.TryParse(split[2], out toDate);
                    }
                    if(validParams)
                    {
                        // TODO: Get service tickets between from/to date range
                        JArray ticketDetails = ConnectWiseHelper.GetTicketsFromAndTo(CustomerId,fromDate, toDate);
                        await SendTicketDetailsMessage(context, ticketDetails);

                    }
                    else
                    {
                        await SendHelpMessage(context, "I'm sorry, you must enter a valid from date and to date :(");
                    }
                }
                else
                {
                    await SendHelpMessage(context, "I'm sorry, I did not understand you :(");
                }
            }

            context.Wait(MessageReceivedAsync);
        }

        private async Task SendContactDetailsMessage(IDialogContext context, Company company)
        {
            IMessageActivity reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();

            if (company != null)
            {
                ThumbnailCard card = new ThumbnailCard()
                {

                    Title = $"{company.AccountManager.Name}",
                    Subtitle = $"{company.AccountManager.Email}",
                    Text = company.AccountManager.PhoneNumber,
                    Images = new List<CardImage>()
                {
                    new CardImage()
                    {
                        Url = $"https://contosomanagedservices.azurewebsites.net/images/kloud88.png"
                    }
                }
                };

                card.Buttons = new List<CardAction>()
                {
                    new CardAction("openUrl", "Skype", null, string.Format("sip:{0}",company.AccountManager.Email)),
                    new CardAction("openUrl", "Call", null, string.Format("tel:{0}",company.AccountManager.PhoneNumber)),
                    new CardAction("openUrl", "Email", null, string.Format("mailto:{0}",company.AccountManager.Email))
                };

                reply.Attachments.Add(card.ToAttachment());
            }
            else
            {
                reply.Text = $"I could not find the Account Manager for your company :(";
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
        private async Task SendStatusMessage(IDialogContext context, Ticket ticket)
        {
            IMessageActivity reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();

            if (ticket != null)
            {
                ThumbnailCard card = new ThumbnailCard()
                {
                    Title = $"{ticket.Title}",
                    Subtitle = $"#{ticket.SubTitle}",
                    Text = ticket.Text,
                    Images = new List<CardImage>()
                {
                    new CardImage()
                    {
                        Url = $"https://contosomanagedservices.azurewebsites.net/images/kloud88.png"
                    }
                }
                };

                // TODO: Retrieve Company Name from SharePoint configuration list.
                var companyName = "kloudtraining";
                card.Buttons = new List<CardAction>()
                {
                    new CardAction("openUrl", "View ticket", null, string.Format("https://aus.myconnectwise.net/v4_6_release/services/system_io/Service/fv_sr100_request.rails?service_recid={0}&companyName={1}",ticket.SubTitle, companyName))
                };

                reply.Attachments.Add(card.ToAttachment());
            }
            else
            {
                reply.Text = $"I could not find this ticket :(";
            }

            ConnectorClient client = new ConnectorClient(new Uri(context.Activity.ServiceUrl));
            ResourceResponse resp = await client.Conversations.ReplyToActivityAsync((Activity)reply);

            // Cache the response activity ID and previous task card.
            //string activityId = resp.Id.ToString();
            //context.ConversationData.SetValue("task " + taskItem.Guid, new Tuple<string, ThumbnailCard>(activityId, card));
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
                + "* To find out my account manager contact information, you can type **contact**\n"
                + "* To find out remaining hours in my service agreement this month, you can type **hours**\n"
                + "* To get latest status of service request or incident, you can type **status** followed by the ticket number\n"
                + "* To find most recent service requests or incidents, you can type **tickets**\n"
                + "* To find service requests or incidents for a specific period, you can type **tickets** followed by from date and to date"
                + "* To perform actions on a VM, you can type **reboot, start, stop or resize** followed by the name of the VM";

            await context.PostAsync(helpMessage);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ticketDetails"></param>
        /// <returns></returns>
        private async Task SendCompanyDetailsMessage(IDialogContext context, Company cDetails)
        {
            //var taskItem = Utils.Utils.CreateTaskItem();
            //taskItem.Title = taskItemTitle;

            IMessageActivity reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();

            ThumbnailCard card = new ThumbnailCard()
            {
                Title = $"{cDetails.Name}",
                Subtitle = $"Total Hours: {cDetails.TotalHoursUsed}",
                Text = cDetails.Id,
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

            ConnectorClient client = new ConnectorClient(new Uri(context.Activity.ServiceUrl));
            ResourceResponse resp = await client.Conversations.ReplyToActivityAsync((Activity)reply);

            // Cache the response activity ID and previous task card.
            //string activityId = resp.Id.ToString();
            //context.ConversationData.SetValue("task " + taskItem.Guid, new Tuple<string, ThumbnailCard>(activityId, card));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cDetails"></param>
        /// <returns></returns>
        private async Task SendTicketDetailsMessage(IDialogContext context, JArray tDetails)
        {
            //var taskItem = Utils.Utils.CreateTaskItem();
            //taskItem.Title = taskItemTitle;

            IMessageActivity reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();

            var random = new Random();

            foreach (JObject o in tDetails)
            {
                ThumbnailCard card = new ThumbnailCard()
                {
                    Title = $"{o["summary"].ToString()}",
                    Subtitle = $"Status: { o["status"]["name"].ToString()}," + $"  Date Entered: { o["dateEntered"].ToString()}",
                    Text = $"Ticket Id: { o["id"].ToString()}" + $" Ticket Type: { o["recordType"].ToString()}"
                    //Subtitle = $"Total Hours: {cDetails.totalHours}",
                    //Text = cDetails.CompanyId,
                    //    Images = new List<CardImage>()
                    //{
                    //    new CardImage()
                    //    {
                    //        Url = $"https://contosomanagedservices.azurewebsites.net/images/kloud88.png"
                    //    }
                    //}
                };

                reply.Attachments.Add(card.ToAttachment());
            }

            //card.Buttons = new List<CardAction>()
            //{
            //    new CardAction("openUrl", "View task", null, "https://www.microsoft.com"),
            //    new CardAction("imBack", "Assign to me", null, $"assign {taskItem.Guid}")
            //};

            ConnectorClient client = new ConnectorClient(new Uri(context.Activity.ServiceUrl));
            ResourceResponse resp = await client.Conversations.ReplyToActivityAsync((Activity)reply);

            // Cache the response activity ID and previous task card.
            //string activityId = resp.Id.ToString();
            //context.ConversationData.SetValue("task " + taskItem.Guid, new Tuple<string, ThumbnailCard>(activityId, card));
        }

    }
}