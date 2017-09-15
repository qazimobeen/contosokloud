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
    public class MachineResizeConfirmDialog : IDialog<string>
    {
        private int nullattempts = 3;
        private int confirmattempts = 3;
        private string vmName;
        private const string YesOption = "Yes";
        private const string NoOption = "No";

        public MachineResizeConfirmDialog(string vmName)
        {
            this.vmName = vmName;
        }

        public async Task StartAsync(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OnOptionsSelected, new List<string> { YesOption, NoOption }, $"Are you sure you want to Resize - {this.vmName} ?", "I'm sorry, I don't understand your response. Please reply with 'Yes' or 'No'?", 3);
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
    }
}