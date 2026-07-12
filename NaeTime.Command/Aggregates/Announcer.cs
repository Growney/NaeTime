using EventDbLite.Aggregates;
using NaeTime.Events.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Command.Aggregates;
public class Announcer : AggregateRoot<Guid>
{
    public Announcer(Guid id,string name)
    {
        Raise(new AnnouncerConfigurationCreated(id, name));

    }

    private void When(AnnouncerConfigurationCreated created)
    {
        Id = created.AnnouncerId;
    }

    public void Rename(string newName)
    {
        Raise(new AnnouncerConfigurationRenamed(Id, newName));
    }

}
