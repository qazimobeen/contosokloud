using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bot_Application.Entities.AWS
{
    [Serializable]
    public class TicketFields
    {
        public string AccountId { get; set; }
        public int ChangeNumber { get; set; }
        public string InstanceId { get; set; }
        public string InstanceType { get; set; }
        public string OperationName { get; set; }
        /// <summary>
        /// In UTC time
        /// </summary>
        public string ScheduledTimestamp { get; set; }
    }
}