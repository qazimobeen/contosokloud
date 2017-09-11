using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams; //Teams bot extension SDK
using Microsoft.Bot.Connector.Teams.Models;
using TeamsSampleTaskApp.Utils;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TeamsSampleTaskApp.Dialogs
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

            //Strip out all mentions.  As all channel messages to a bot must @mention the bot itself, you must strip out the bot name at minimum.
            // This uses the extension SDK function GetTextWithoutMentions() to strip out ALL mentions
            var text = activity.GetTextWithoutMentions().ToLower();

            //Supports 5 commands:  Help, Welcome (sent from HandleSystemMessage when bot is added), Create, Find, Assign, and Link 
            //  This simple text parsing assumes the command is the first string, and an optional parameter is the second.
            var split = text.ToLower().Split(' ');
            if (split.Length < 2)
            {
                if (text.Contains("help"))
                {
                    await SendHelpMessage(context, "Sure, I can provide help info about me.");
                }
                else if (text.Contains("welcome"))
                {
                    await SendHelpMessage(context, "## Hi, I'm the Teams Sample Task app bot, in C#.");
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
                if (cmd.Contains("create") || cmd.Contains("find"))
                {
                    await SendTaskMessage(context, string.Join(" ", q));
                }
                else if (cmd.Contains("assign"))
                {
                    string guid = split[1];
                    await UpdateMessage(context, guid);
                }
                else if (cmd.Contains("link"))
                {
                    await SendDeeplink(context, activity, string.Join(" ", q));
                }
                else
                {
                    await SendHelpMessage(context, "I'm sorry, I did not understand you :(");
                }
            }

            context.Wait(MessageReceivedAsync);
        }

        /// <summary>
        /// Helper method to create a simple task card and send it back as a message.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="taskItemTitle"></param>
        /// <returns></returns>
        private async Task SendTaskMessage(IDialogContext context, string taskItemTitle)
        {
            var taskItem = Utils.Utils.CreateTaskItem();
            taskItem.Title = taskItemTitle;

            IMessageActivity reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();

            var random = new Random();

            ThumbnailCard card = new ThumbnailCard()
            {
                Title = $"Task created: {taskItem.Title}",
                Subtitle = $"Assigned to: {taskItem.Assigned}",
                Text = taskItem.Description,
                Images = new List<CardImage>()
                {
                    new CardImage()
                    {
                        Url = $"../images/image{random.Next(1, 9)}.png",
                    }
                }
            };

            card.Buttons = new List<CardAction>()
            {
                new CardAction("openUrl", "View task", null, "https://www.microsoft.com"),
                new CardAction("imBack", "Assign to me", null, $"assign {taskItem.Guid}")
            };
            
            reply.Attachments.Add(card.ToAttachment());

            ConnectorClient client = new ConnectorClient(new Uri(context.Activity.ServiceUrl));
            ResourceResponse resp = await client.Conversations.ReplyToActivityAsync((Activity)reply);

            // Cache the response activity ID and previous task card.
            string activityId = resp.Id.ToString();
            context.ConversationData.SetValue("task " + taskItem.Guid, new Tuple<string, ThumbnailCard>(activityId, card));
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
        private async Task SendDeeplink(IDialogContext context, Activity activity, string tabName)
        {
            var teamsChannelData = activity.GetChannelData<TeamsChannelData>();
            var teamId = teamsChannelData.Team.Id;
            var channelId = teamsChannelData.Channel.Id;

            // The app ID, stored in the web.config file, should be the appID from your manifest.json file.
            var appId = System.Configuration.ConfigurationManager.AppSettings["TeamsAppId"];
            var entity = $"todotab-{tabName}-{teamId}-{channelId}"; // Match the entity ID we setup when configuring the tab
            var tabContext = new TabContext()
            {
                ChannelId = channelId,
                CanvasUrl = "https://teams.microsoft.com"
            };

            var url = $"https://teams.microsoft.com/l/entity/{HttpUtility.UrlEncode(appId)}/{HttpUtility.UrlEncode(entity)}?label={HttpUtility.UrlEncode(tabName)}&context={HttpUtility.UrlEncode(JsonConvert.SerializeObject(tabContext))}";

            var text = $"I've created a deep link to {tabName}! Click [here]({url}) to navigate to the tab.";
            await context.PostAsync(text);
        }

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
    }
}