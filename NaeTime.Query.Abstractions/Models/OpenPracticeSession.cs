using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Query.Abstractions.Models;
public record OpenPracticeSession(Guid Id, string Name, Guid TrackId, IReadOnlyList<OpenPracticeLane> Lanes);