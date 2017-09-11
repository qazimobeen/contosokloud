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

    }
}