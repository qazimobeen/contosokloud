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
    public class MachineRebootInquireDialog : IDialog<string>
    {
        private int attempts = 3;
        Dictionary<string, string> allVms = new Dictionary<string, string>();

        public async Task StartAsync(IDialogContext context)
        {
            //await context.PostAsync("What machine do you want to reboot?  Below is a list of possible VMs.");
            //await context.PostAsync("What machine do you want to reboot2?");
            //await context.PostAsync("What machine do you want to reboot3?");
            //allVms.Add("bob", "bob123");
            //allVms.Add("sally", "sally123");
            allVms = Helper.AWSHelper.GetVMs();

            List<string> vmList = allVms.Keys.ToList();

            PromptDialog.Choice(context, this.OnOptionsSelected, vmList, "What machine do you want to reboot?  Below is a list of possible VMs.", "I'm sorry, I don't understand your reply. Please choose from the list or with the name of a VM.", 3);

            //context.Wait(this.MessageReceivedAsync);

        }

        private async Task OnOptionsSelected(IDialogContext context, IAwaitable<string> result)
        {
            string optionSelected = await result;

            List<string> vmList = allVms.Keys.ToList();

            if (vmList.Contains(optionSelected))
            {
                context.Done(optionSelected);
            }
        }


        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            if ((message.Text != null) && (message.Text.Trim().Length > 0))
            {
                context.Done(message.Text);
            }
            else
            {
                --attempts;
                if (attempts > 0)
                {
                    await context.PostAsync("I'm sorry, I don't understand your reply. What is your name (e.g. 'Bill', 'Melinda')?");

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