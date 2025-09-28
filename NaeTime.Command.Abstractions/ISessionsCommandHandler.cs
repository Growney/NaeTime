using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Command.Abstractions;
public interface ISessionsCommandHandler
{
    public Task ActivateOpenPracticeSession(Guid id);
    public Task DeactivateOpenPracticeSession(Guid id);
}
