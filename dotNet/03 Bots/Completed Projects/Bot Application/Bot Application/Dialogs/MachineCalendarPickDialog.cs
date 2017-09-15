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
    public class MachineCalendarPickDialog : IDialog<string>
    {
        private int nullattempts = 3;
        private int confirmattempts = 3;
        private string vmName;
        Dictionary<string, int> timeChoices;
        private string actionName;

        public MachineCalendarPickDialog(string vmName, string actionName)
        {
            this.vmName = vmName;
            this.actionName = actionName;
            timeChoices = new Dictionary<string, int>();
            timeChoices.Add("now", 0);
            timeChoices.Add("5 minutes", 5);
            timeChoices.Add("15 minutes", 15);
            timeChoices.Add("30 minutes", 30);
            timeChoices.Add("An hour", 60);
            timeChoices.Add("6 hours", 360);
            timeChoices.Add("In 12 hours from now", 720);
            timeChoices.Add("This time tomorrow (24 hours)", 1440);
        }

        public async Task StartAsync(IDialogContext context)
        {
            //await context.PostAsync($"Do you want to Reboot - {this.vmName} ?");
            //context.Wait(this.MessageReceivedAsync);
            
            List<string> timeOptions = timeChoices.Keys.ToList<string>();
            PromptDialog.Choice(context, this.onTimeProvided, timeOptions, $"Choose from the list below when to {this.actionName}", "I'm sorry, I don't understand your reply. Please choose from the list", 3);
        }

        private async Task onTimeProvided(IDialogContext context, IAwaitable<string> result)
        {
            string optionSelected = await result;
            context.Done(timeChoices[optionSelected].ToString());
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