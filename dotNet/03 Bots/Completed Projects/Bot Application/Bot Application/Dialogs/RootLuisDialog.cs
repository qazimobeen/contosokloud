using System;
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

namespace Bot_Application.Dialogs
{
    [LuisModel("5681482a-bcd0-4d95-8c89-c42e56786555", "7d6dc4f76ea647e395108fe2c3b77110")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
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
            string message = $"LUIS - reboot - Are you sure?";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
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
            string message = $"LUIS - contact - It appears to be your Account Manager is";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("tickets")]
        public async Task tickets(IDialogContext context, LuisResult result)
        {
            string message = $"LUIS - tickets - Ok, I will tell you how many hours left";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("status")]
        public async Task status(IDialogContext context, LuisResult result)
        {
            string message = $"LUIS - status - {result.Query} Let me find out the status of your ticket";

            Ticket ticket = DialogHelper.EntertainIntentStatus(context, result);

            //await context.PostAsync(ticket.Text);

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
            var companyName = "kloudtraining";
            card.Buttons = new List<CardAction>()
                {
                    new CardAction("openUrl", "View ticket", null, string.Format("https://aus.myconnectwise.net/v4_6_release/services/system_io/Service/fv_sr100_request.rails?service_recid={0}&companyName={1}",ticket.SubTitle, companyName))
                };

            reply.Attachments.Add(card.ToAttachment());

            await context.PostAsync(reply);

            context.Wait(this.MessageReceived);
        }
    }
}