using EventDbLite.Abstractions;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Query;
public class OpenPracticeQueryHandler : IOpenPracticeQueryHandler
{
    private readonly IProjectionProvider _projectionProvider;

    public OpenPracticeQueryHandler(IProjectionProvider projectionProvider)
    {
        _projectionProvider = projectionProvider;
    }

    public async Task<OpenPracticeSession?> GetByIdAsync(Guid id)
    {
        OpenPracticeList openPracticeList = await _projectionProvider.Load<OpenPracticeList>();
        return openPracticeList.GetSession(id);
    }
}
