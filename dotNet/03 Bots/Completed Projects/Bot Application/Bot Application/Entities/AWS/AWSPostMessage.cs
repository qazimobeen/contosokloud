using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bot_Application.Entities.AWS
{
    [Serializable]
    public class AWSPostMessage
    {
        public int CompanyId { get; set; }
        public string ServiceDescription { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string TicketDescription { get; set; }
        public TicketFields TicketFields { get; set; }
        public int TicketId { get; set; }
        public string TicketRequesterEmail { get; set; }
        public string TicketRequesterName { get; set; }
        public string TicketRequesterPhone { get; set; }
        public string TicketSubject { get; set; }
    }
}