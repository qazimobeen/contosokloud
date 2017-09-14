﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Bot_Application.Dialogs
{
    [Serializable]
    public class MachineBootInquireDialog : IDialog<string>
    {
        private int attempts = 3;
        Dictionary<string, string> allVms = new Dictionary<string, string>();

        public async Task StartAsync(IDialogContext context)
        {
            allVms = Helper.AWSHelper.GetVMs();

            List<string> vmList = allVms.Keys.ToList();

            PromptDialog.Choice(context, this.OnOptionsSelected, vmList, "What machine do you want to boot?  Below is a list of possible VMs.", "I'm sorry, I don't understand your reply. Please choose from the list or with the name of a VM.", 3);
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
    }
}