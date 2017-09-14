﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Bot_Application.Helper;
using Bot_Application.Entities;
using Newtonsoft.Json.Linq;

namespace Bot_Application.Dialogs
{
    [LuisModel("5681482a-bcd0-4d95-8c89-c42e56786555", "7d6dc4f76ea647e395108fe2c3b77110")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
        private string vmName = string.Empty;
        private string vmAWSID = string.Empty;
        private string confirmMessage = string.Empty;
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"LUIS - None - Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("boot")]
        public async Task boot(IDialogContext context, LuisResult result)
        {
            string message = $"LUIS - boot - Booting it up for you!";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("reboot")]
        public async Task reboot(IDialogContext context, LuisResult result)
        {
            string message = $"LUIS - reboot - Great let me help you reboot";

            await context.PostAsync(message);

            context.Call(new MachineRebootInquireDialog(), this.RebootInquireResumeAfter);

            //context.Wait(this.MessageReceived);
        }

        private async Task RebootInquireResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                this.vmName = await result;
                context.Call(new MachineRebootConfirmDialog(this.vmName), this.RebootConfirmResumeAfter);
                //await context.PostAsync("Reboot Successfull");
            }
            catch (Exception)
            {
                await context.PostAsync("Reboot Aborted");
                throw;
            }
        }

        private async Task RebootConfirmResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
               confirmMessage = await result;
               if(confirmMessage.ToLower().Equals("yes"))
                {
                    Dictionary<string, string> allVms = new Dictionary<string, string>();
                    allVms.Add("bob", "bob123");
                    allVms.Add("sally", "sally123");
                    var myIdx = allVms.Keys.ToList().IndexOf(this.vmName);
                    this.vmAWSID = allVms.Values.ElementAt(myIdx);
                    await context.PostAsync("Attempting to reboot " + this.vmName + " (known as " + this.vmAWSID + " in AWS)...");
                }
               else
                {
                    await context.PostAsync("Reboot Aborted");
                }
            }
            catch (Exception)
            {
                await context.PostAsync("Reboot Unsuccessful");
                throw;
            }
        }

        [LuisIntent("resize")]
        public async Task resize(IDialogContext context, LuisResult result)
        {
            string message = $"LUIS - resize - Surely, how you want me to resize it?";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("snapshot")]
        public async Task snapshot(IDialogContext context, LuisResult result)
        {
            string message = $"LUIS - snapshot - Yes, I can do it for you. What you want?";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
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
                    new CardAction("openUrl", "View ticket", null, string.Format("https://aus.myconnectwise.net/v4_6_release/services/system_io/Service/fv_sr100_request.rails?service_recid={0}&companyName={1}",ticket.SubTitle, companyName))
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
                    ThumbnailCard card = new ThumbnailCard()
                    {
                        Title = $"{o["summary"].ToString()}",
                        Subtitle = $"Status: { o["status"]["name"].ToString()}," + $"  Date Entered: { o["dateEntered"].ToString()}",
                        Text = $"Ticket Id: { o["id"].ToString()}" + $" Ticket Type: { o["recordType"].ToString()}"
                    };

                    reply.Attachments.Add(card.ToAttachment());
                }

                await context.PostAsync(reply);
            }

            context.Wait(this.MessageReceived);
        }
    }
}