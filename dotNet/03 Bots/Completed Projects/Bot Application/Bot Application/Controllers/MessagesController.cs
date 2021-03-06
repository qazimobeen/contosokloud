﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json.Linq;

namespace Bot_Application
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                //ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                //Activity reply;


                //if (activity.Text.ToLower().Contains("contact"))
                //{
                //    reply = activity.CreateReply(GetAccountManagerDetails());
                //}
                //else if (activity.Text.ToLower().Contains("status"))
                //{ 
                //    reply = activity.CreateReply("The status of your ticket is : " + GetTicketStatus(""));
                //}
                //else if (activity.Text.ToLower().Contains("tickets"))
                //{
                //    reply = activity.CreateReply(GetContractDetails());
                //}
                //else
                //{
                //    reply = activity.CreateReply($"your word is not defined");
                //}

                //await connector.Conversations.ReplyToActivityAsync(reply);
                activity.Text = activity.GetTextWithoutMentions();
                await Conversation.SendAsync(activity, () => new Bot_Application.Dialogs.RootLuisDialog());

                //await Conversation.SendAsync(activity, () => new Bot_Application.RootDialog());
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