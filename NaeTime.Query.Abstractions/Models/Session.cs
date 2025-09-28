using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Query.Abstractions.Models;
public record Session(Guid Id, string Name, SessionType Type);
