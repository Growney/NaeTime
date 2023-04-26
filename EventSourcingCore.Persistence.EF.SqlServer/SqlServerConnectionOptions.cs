using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Persistence.EF.SqlServer
{
    public class SqlServerConnectionOptions
    {
        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Timeout { get; set; }
        public string DatabaseName { get; set; }
    }
}
