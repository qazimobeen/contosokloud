using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Bot_Application.Dialogs
{
    [Serializable]
    public class MachineActionConfirmDialog : IDialog<string>
    {
        private int nullattempts = 3;
        private int confirmattempts = 3;
        private string vmName;
        private string actionName;
        private const string YesOption = "Yes";
        private const string NoOption = "No";

        public MachineActionConfirmDialog(string vmName, string actionName)
        {
            this.vmName = vmName;
            this.actionName = actionName;
        }

        public async Task StartAsync(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OnOptionsSelected, new List<string> { YesOption, NoOption }, $"Do you want to {this.actionName} - {this.vmName} ?", "I'm sorry, I don't understand your reply. Please reply with 'Yes' or 'No'?", 3);
        }

        private async Task OnOptionsSelected(IDialogContext context, IAwaitable<string> result)
        {
            string optionSelected = await result;
            switch(optionSelected)
            {
                case YesOption:
                case NoOption:
                    context.Done(optionSelected);
                    break;
            }
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            if ((message.Text != null) && (message.Text.Trim().Length > 0))
            {
                if (message.Text.ToLower().Contains("yes") || message.Text.ToLower().Contains("no"))
                { 
                    context.Done(message.Text);
                }
                else
                {
                    --confirmattempts;
                    if (confirmattempts > 0)
                    {
                        await context.PostAsync("I'm sorry, I don't understand your reply. Please reply with 'Yes' or 'No'?");

                        context.Wait(this.MessageReceivedAsync);
                    }
                    else
                    {
                        context.Fail(new TooManyAttemptsException("Message was not a string or was an empty string."));
                    }
                }
            }
            else
            {
                --nullattempts;
                if (nullattempts > 0)
                {
                    await context.PostAsync("I'm sorry, I don't understand your reply. Please reply with 'Yes' or 'No'?");

                    context.Wait(this.MessageReceivedAsync);
                }
                else
                {
                    context.Fail(new TooManyAttemptsException("Message was not a string or was an empty string."));
                }
            }
        }

    }
}