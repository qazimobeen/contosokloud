﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bot_Application.Entities
{
    public class Ticket
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Text { get; set; }
        public string recordType { get; set; }
        public string dateEntered { get; set; }

        public string CompanyName {get;set;}

        
    }
}