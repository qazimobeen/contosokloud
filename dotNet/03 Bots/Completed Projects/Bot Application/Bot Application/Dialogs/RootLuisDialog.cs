using Bot_Application.Entities;
using Bot_Application.Entities.AWS;
using Bot_Application.Helper;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Bot_Application.Dialogs
{
    [LuisModel("5681482a-bcd0-4d95-8c89-c42e56786555", "7d6dc4f76ea647e395108fe2c3b77110")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
        private string vmName = string.Empty;
        private string vmAWSID = string.Empty;
        private string confirmMessage = string.Empty;
        private string confirmStorage = string.Empty;
        private int numOfMins = 0;

        private const string bootString = "boot";
        private const string RebootString = "reboot";
        private const string ResizeString = "resize";
        private const string SnapshotString = "snapshot";
        private const string StopString = "stop";


        [LuisIntent("thanks")]
        public async Task Thanks(IDialogContext context, LuisResult result)
        {
            string message = "Oh no, thank you!";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("")]
        [LuisIntent("none")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"LUIS - None - Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("boot")]
        public async Task boot(IDialogContext context, LuisResult result)
        {
            string message = "Certainly!  Let me help you boot a Virtual Machine (VM).";

            await context.PostAsync(message);

            context.Call(new MachineActionInquireDialog(bootString), this.BootInquireResumeAfter);
        }

        private async Task BootInquireResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                this.vmName = await result;
                context.Call(new MachineActionConfirmDialog(this.vmName,bootString), this.BootConfirmResumeAfter);
            }
            catch (Exception)
            {
                await context.PostAsync("Boot Aborted!");
                throw;
            }
        }

        private async Task BootConfirmResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                confirmMessage = await result;
                if (confirmMessage.ToLower().Equals("yes"))
                {
                    Dictionary<string, string> allVms = new Dictionary<string, string>();
                    allVms = Helper.AWSHelper.GetVMs();
                    var myIdx = allVms.Keys.ToList().IndexOf(this.vmName);
                    this.vmAWSID = allVms.Values.ElementAt(myIdx);        
                    var serviceMessage = new ServiceMessage(OperationType.Start);
                    Helper.AWSHelper.RunOperation(this.vmAWSID, serviceMessage);
                    await context.PostAsync("Sounds good, I'll attempt to boot the " + this.vmName);
                }
                else
                {
                    await context.PostAsync("Boot Aborted - Sorry, the time could not be understood.");
                }
            }
            catch (Exception)
            {
                await context.PostAsync("Boot Unsuccessful :(");
                throw;
            }
        }

        [LuisIntent("stop")]
        public async Task stop(IDialogContext context, LuisResult result)
        {
            string message = "Certainly!  Let me help you stop a Virtual Machine (VM)...";

            await context.PostAsync(message);

            context.Call(new MachineActionInquireDialog(StopString), this.StopInquireResumeAfter);
        }

        private async Task StopInquireResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                this.vmName = await result;
                context.Call(new MachineActionConfirmDialog(this.vmName, StopString), this.StopConfirmResumeAfter);
            }
            catch (Exception)
            {
                await context.PostAsync("Stop Aborted!");
                throw;
            }
        }

        private async Task StopConfirmResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                confirmMessage = await result;
                if (confirmMessage.ToLower().Equals("yes"))
                {
                    context.Call(new MachineCalendarPickDialog(this.vmName, StopString), this.StopConfirmTimeResumeAfter);
                }
                else
                {
                    await context.PostAsync("Stop Aborted!");
                }
            }
            catch (Exception)
            {
                await context.PostAsync("Stop Unsuccessful :(");
                throw;
            }
        }

        private async Task StopConfirmTimeResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                confirmMessage = await result;
                numOfMins = Convert.ToInt32(confirmMessage);
                if (numOfMins >= 0)
                {
                    Dictionary<string, string> allVms = new Dictionary<string, string>();
                    allVms = Helper.AWSHelper.GetVMs();
                    var myIdx = allVms.Keys.ToList().IndexOf(this.vmName);
                    this.vmAWSID = allVms.Values.ElementAt(myIdx);
                    DateTime dt = DateTime.UtcNow.AddMinutes(numOfMins);
                    var serviceMessage = new ServiceMessage(OperationType.Stop);
                    Helper.AWSHelper.RunOperation(this.vmAWSID, serviceMessage, dt);
                    await context.PostAsync("Sounds good, I'll attempt to stop the " + this.vmName + " VM in " + numOfMins + " minutes.");
                }
                else
                {
                    await context.PostAsync("Reboot Aborted - Sorry, the time could not be understood.");
                }
            }
            catch (Exception)
            {
                await context.PostAsync("Reboot Unsuccessful :(");
                throw;
            }
        }

        [LuisIntent("reboot")]
        public async Task reboot(IDialogContext context, LuisResult result)
        {
            string message = "Great!  Let me help you reboot a Virtual Machine (VM).";

            await context.PostAsync(message);

            context.Call(new MachineActionInquireDialog(RebootString), this.RebootInquireResumeAfter);
        }

        private async Task RebootInquireResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                this.vmName = await result;
                context.Call(new MachineActionConfirmDialog(this.vmName, RebootString), this.RebootConfirmResumeAfter);
            }
            catch (Exception)
            {
                await context.PostAsync("Reboot Aborted!");
                throw;
            }
        }

        private async Task RebootConfirmResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                confirmMessage = await result;
                if (confirmMessage.ToLower().Equals("yes"))
                {
                    context.Call(new MachineCalendarPickDialog(this.vmName, RebootString), this.RebootConfirmTimeResumeAfter);
                }
                else
                {
                    await context.PostAsync("Reboot Aborted!");
                }
            }
            catch (Exception)
            {
                await context.PostAsync("Reboot Unsuccessful :(");
                throw;
            }
        }

        private async Task RebootConfirmTimeResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                confirmMessage = await result;
                numOfMins = Convert.ToInt32(confirmMessage);
                if (numOfMins >= 0)
                {
                    Dictionary<string, string> allVms = new Dictionary<string, string>();
                    allVms = Helper.AWSHelper.GetVMs();
                    var myIdx = allVms.Keys.ToList().IndexOf(this.vmName);
                    this.vmAWSID = allVms.Values.ElementAt(myIdx);
                    DateTime dt = DateTime.UtcNow.AddMinutes(numOfMins);
                    var serviceMessage = new ServiceMessage(OperationType.Restart);
                    Helper.AWSHelper.RunOperation(this.vmAWSID, serviceMessage, dt);
                    await context.PostAsync("Sounds good, I'll attempt to reboot the " + this.vmName + " VM in " + numOfMins + " minutes.");
                }
                else
                {
                    await context.PostAsync("Reboot Aborted - Sorry, the time could not be understood.");
                }
            }
            catch (Exception)
            {
                await context.PostAsync("Reboot Unsuccessful :(");
                throw;
            }
        }

        [LuisIntent("resize")]
        public async Task resize(IDialogContext context, LuisResult result)
        {
            string message = "Great!  Let me help you resize a Virtual Machine (VM).";

            await context.PostAsync(message);

            context.Call(new MachineActionInquireDialog(ResizeString), this.ResizeInquireResumeAfter);
        }


        private async Task ResizeInquireResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                this.vmName = await result;
                context.Call(new MachineStoragePickDialog(this.vmName), this.ResizeConfirmStorageResumeAfter);
            }
            catch (Exception)
            {
                await context.PostAsync("Resize Aborted!");
                throw;
            }
        }

        private async Task ResizeConfirmStorageResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                confirmStorage = await result;
                if (confirmStorage != string.Empty)
                {
                    context.Call(new MachineActionConfirmDialog(this.vmName, ResizeString), this.ResizeConfirmResumeAfter);
                }
                else
                {
                    await context.PostAsync("Resize Aborted - Sorry, the storage could not be understood.");
                }
            }
            catch (Exception)
            {
                await context.PostAsync("Resize Unsuccessful :(");
                throw;
            }
        }

        private async Task ResizeConfirmResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                confirmMessage = await result;
                if (confirmMessage.ToLower().Equals("yes"))
                {

                    Dictionary<string, string> allVms = new Dictionary<string, string>();
                    allVms = Helper.AWSHelper.GetVMs();
                    var myIdx = allVms.Keys.ToList().IndexOf(this.vmName);
                    this.vmAWSID = allVms.Values.ElementAt(myIdx);
                    var serviceMessage = new ServiceMessage(OperationType.Restart, confirmStorage, InstanceFamilyType.Storage);
                    Helper.AWSHelper.RunOperation(this.vmAWSID, serviceMessage);
                    await context.PostAsync("Sounds good, I'll attempt to resize the " + this.vmName);

                }
                else
                {
                    await context.PostAsync("Resize Aborted!");
                }
            }
            catch (Exception)
            {
                await context.PostAsync("Resize Unsuccessful :(");
                throw;
            }
        }

        [LuisIntent("snapshot")]
        public async Task snapshot(IDialogContext context, LuisResult result)
        {
            string message = $"Snapshot - Yes, I can do it for you. What you want?";

            await context.PostAsync(message);

            context.Call(new MachineActionInquireDialog(SnapshotString), this.SnapshotInquireResumeAfter);
        }

        private async Task SnapshotInquireResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                this.vmName = await result;
                context.Call(new MachineActionConfirmDialog(this.vmName, SnapshotString), this.SnapshotConfirmResumeAfter);
            }
            catch (Exception)
            {
                await context.PostAsync("Snapshot Aborted!");
                throw;
            }
        }

        private async Task SnapshotConfirmResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                confirmMessage = await result;
                if (confirmMessage.ToLower().Equals("yes"))
                {

                    Dictionary<string, string> allVms = new Dictionary<string, string>();
                    allVms = Helper.AWSHelper.GetVMs();
                    var myIdx = allVms.Keys.ToList().IndexOf(this.vmName);
                    this.vmAWSID = allVms.Values.ElementAt(myIdx);
                    var serviceMessage = new ServiceMessage(OperationType.Snapshot);
                    Helper.AWSHelper.RunOperation(this.vmAWSID, serviceMessage);
                    await context.PostAsync("Sounds good, I'll attempt to snapshot the " + this.vmName);

                }
                else
                {
                    await context.PostAsync("Snapshot Aborted!");
                }
            }
            catch (Exception)
            {
                await context.PostAsync("Snapshot Unsuccessful :(");
                throw;
            }
        }

        [LuisIntent("contact")]
        public async Task contact(IDialogContext context, LuisResult result)
        {
            string message = $"LUIS - contact - Your Account Manager details are as follows:";

            await context.PostAsync(message);

            IMessageActivity reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();

            //TODO: get company details dynamically
            Company company = ConnectWiseHelper.GetContactDetails("19321");

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

            await context.PostAsync(reply);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("tickets")]
        public async Task tickets(IDialogContext context, LuisResult result)
        {
            string message = $"LUIS - tickets - Ok, I will tell you how many hours left";

            await context.PostAsync(message);

            await context.PostAsync(new Random().Next().ToString());

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("status")]
        public async Task status(IDialogContext context, LuisResult result)
        {
            string message = $"LUIS - status - Let me find out the status of your tickets...";

            await context.PostAsync(message);

            if (result.Entities.Count == 1)
            {
                //SCENARIO: a single ticket ID is passed to LUIS

                Ticket ticket = DialogHelper.EntertainIntentStatus(context, result);

                if (ticket != null)
                {
                    IMessageActivity reply = context.MakeMessage();
                    reply.Attachments = new List<Attachment>();

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
                    var companyName = ticket.CompanyName;
                    card.Buttons = new List<CardAction>()
                    {
                        new CardAction("openUrl", "View ticket", null, string.Format("https://aus.myconnectwise.net/v4_6_release/services/system_io/Service/fv_sr100_request.rails?service_recid={0}&companyName={1}",ticket.SubTitle, companyName)),
                    };

                    reply.Attachments.Add(card.ToAttachment());

                    await context.PostAsync(reply);
                }
                else
                {
                    await context.PostAsync("LUIS - status - I could not find this ticket :(");
                }
            }
            else 
            {
                //SCENARIO: all tickets or 5 tickets
                //TODO: get customer ID dynamically
                JArray ticketDetails = ConnectWiseHelper.GetTickets("1234");

                IMessageActivity reply = context.MakeMessage();
                reply.Attachments = new List<Attachment>();
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                foreach (JObject o in ticketDetails)
                {
                    var text = $"Ticket Id: { o["id"].ToString()}" + $" Ticket Type: { o["recordType"].ToString()}";
                    ThumbnailCard card = new ThumbnailCard()
                    { 
                        Title = $"{o["summary"].ToString()}",
                        Subtitle = $"Status: { o["status"]["name"].ToString()}," + $"  Date Entered: { o["dateEntered"].ToString()}",
                        Text = text
                    };

                    card.Buttons = new List<CardAction>()
                    {
                        new CardAction("openUrl", "View ticket in new tab", null, SendDeeplink(context, context.Activity, text))
                    };

                    reply.Attachments.Add(card.ToAttachment());
                }

                await context.PostAsync(reply);
            }

            context.Wait(this.MessageReceived);
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
        private string SendDeeplink(IDialogContext context, IActivity activity, string tabName)
        {
            var teamsChannelData = activity.GetChannelData<TeamsChannelData>();
            var teamId = teamsChannelData.Team.Id;
            var channelId = teamsChannelData.Channel.Id;

            // The app ID, stored in the web.config file, should be the appID from your manifest.json file.
            var appId = System.Configuration.ConfigurationManager.AppSettings["TeamsAppId"];
            var entity = $"kloud-{tabName}-{teamId}-{channelId}"; // Match the entity ID we setup when configuring the tab
            var tabContext = new TabContext()
            {
                ChannelId = channelId,
                CanvasUrl = "https://teams.microsoft.com"
            };

            var url = $"https://teams.microsoft.com/l/entity/{HttpUtility.UrlEncode(appId)}/{HttpUtility.UrlEncode(entity)}?label={HttpUtility.UrlEncode(tabName)}&context={HttpUtility.UrlEncode(JsonConvert.SerializeObject(tabContext))}";

            var text = $"I've created a deep link to {tabName}! Click [here]({url}) to navigate to the tab.";
            return text;
        }
    }
}