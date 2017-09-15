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
    public class MachineStoragePickDialog : IDialog<string>
    {
        private int nullattempts = 3;
        private int confirmattempts = 3;
        private string vmName;
        Dictionary<string, string> storageChoices;

        public MachineStoragePickDialog(string vmName)
        {
            this.vmName = vmName;
            storageChoices = new Dictionary<string, string>();

            storageChoices.Add("d2.xlarge", "d2.xlarge");
            storageChoices.Add("d2.2xlarge", "d2.2xlarge");
            storageChoices.Add("d2.4xlarge", "d2.4xlarge");
            storageChoices.Add("d2.8xlarge", "d2.8xlarge");
            storageChoices.Add("i2.xlarge", "i2.xlarge");
            storageChoices.Add("i2.2xlarge", "i2.2xlarge");
            storageChoices.Add("i2.4xlarge", "i2.4xlarge");
            storageChoices.Add("i2.8xlarge", "i2.8xlarge");
            storageChoices.Add("i3.large", "i3.large");
            storageChoices.Add("i3.xlarge", "i3.xlarge");
            storageChoices.Add("i3.2xlarge", "i3.2xlarge");
            storageChoices.Add("i3.4xlarge", "i3.4xlarge");
            storageChoices.Add("i3.8xlarge", "i3.8xlarge");
            storageChoices.Add("i3.16xlarge", "i3.16xlarge");
        }

        public async Task StartAsync(IDialogContext context)
        {
            //await context.PostAsync($"Do you want to Reboot - {this.vmName} ?");
            //context.Wait(this.MessageReceivedAsync);
            
            List<string> storageOptions = storageChoices.Keys.ToList<string>();
            PromptDialog.Choice(context, this.onStorageProvided, storageOptions, "Choose from the list below the storage type", "I'm sorry, I don't understand your reply. Please choose from the list", 3);
        }

        private async Task onStorageProvided(IDialogContext context, IAwaitable<string> result)
        {
            string optionSelected = await result;
            context.Done(storageChoices[optionSelected].ToString());
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