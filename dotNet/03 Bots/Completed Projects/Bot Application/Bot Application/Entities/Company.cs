using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bot_Application.Entities
{
    public class Company
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public AccountManager AccountManager { get; set; }
    }
}