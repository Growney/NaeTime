using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.OpenIdConnect.Configuration
{
    public class PathOptions
    {
        public string RegisterPath { get; set; }
        public string RemoteRegisterPath { get; set; }
        public string ProfilePath { get; set; }
        public string RemoteProfilePath { get; set; }
        public string LogInPath { get; set; }
        public string LogInCallbackPath { get; set; }
        public string LogInFailedPath { get; set; }
        public string LogOutPath { get; set; }
        public string LogOutCallbackPath { get; set; }
        public string LogOutFailedPath { get; set; }
        public string LogOutSucceededPath { get; set; }
    }
}
