using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Readiness.ASPNET
{
    public class WebReadinessOptions
    {
        public string ReadinessPath { get; set; } = "Readiness";
    }
}
