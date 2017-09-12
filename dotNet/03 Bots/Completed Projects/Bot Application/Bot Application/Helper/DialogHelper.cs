using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams; //Teams bot extension SDK
using Microsoft.Bot.Connector.Teams.Models;
//using TeamsSampleTaskApp.Utils;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Bot_Application.Entities;
using Bot_Application.Helper;

using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace Bot_Application.Helper
{
    public static class DialogHelper
    {
        public static Ticket EntertainIntentStatus(IDialogContext context, LuisResult result)
        {
            return (Ticket)ConnectWiseHelper.GetTicketStatus(result.Entities[0].Entity);
        }
    }
}