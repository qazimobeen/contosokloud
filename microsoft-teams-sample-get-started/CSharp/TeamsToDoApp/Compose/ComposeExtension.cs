using Bogus;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;
using System;
using System.Collections.Generic;

namespace TeamsSampleTaskApp
{
    /// <summary>
    /// Simple class that processes an activity and responds with with set of compose extension results.
    /// </summary>
    public class ComposeExtension
    {
        private Activity activity;

        /// <summary>
        /// Used to generate image index.
        /// </summary>
        private Random random;

        public ComposeExtension(Activity activity)
        {
            this.activity = activity;
            random = new Random();
        }

        /// <summary>
        /// Help method to generate a compose extension
        /// 
        /// Note that for this sample, we are returning random tasks, for illustration purposes only.
        /// </summary>
        /// <returns></returns>
        public ComposeExtensionResponse CreateComposeExtensionResponse()
        {
            ComposeExtensionResponse response = null;
            const int numResults = 10;

            var query = activity.GetComposeExtensionQueryData();

            //Check to make sure a query was actually made:
            if (query.CommandId == null || query.Parameters == null)
            {
                return null;
            }
            else if (query.Parameters.Count > 0)
            {
                // query.Parameters has the parameters sent by client
                var results = new ComposeExtensionResult()
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = new List<ComposeExtensionAttachment>(),
                };

                // Generate cards for the response.
                for (var i = 0; i < numResults; i++)
                {
                    var card = GenerateResultCard();
                    // Add content to the response title.
                    if (query.Parameters[0].Name != "initialRun")
                    {
                        card.Title += " " + query.Parameters[0].Value;
                    }
                    var composeExtensionAttachment = card.ToAttachment().ToComposeExtensionAttachment();
                    results.Attachments.Add(composeExtensionAttachment);
                }

                response = new ComposeExtensionResponse()
                {
                    ComposeExtension = results
                };
            }

            return response;
        }

        /// <summary>
        /// Helper method to generate a result card for this activity.
        /// </summary>
        /// <returns></returns>
        private ThumbnailCard GenerateResultCard()
        {
            var faker = new Faker();

            ThumbnailCard card = new ThumbnailCard()
            {
                Title = $"Task 8120801: {faker.Commerce.ProductName()}",
                Subtitle = $"Assigned to {faker.Name.FirstName()} {faker.Name.LastName()}",
                Text = faker.Lorem.Lines(2),
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
                new CardAction("imBack", "Assign to me", null, $"assign task")
            };

            return card;
        }
    }
}