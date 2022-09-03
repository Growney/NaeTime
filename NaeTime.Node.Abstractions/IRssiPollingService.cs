﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Abstractions
{
    public interface IRssiPollingService
    {
        Task PausePolling();
        void ResumePolling();
    }
}
