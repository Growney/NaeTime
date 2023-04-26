using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EventSourcingCore.Persistence.EF.Abstractions.Model
{
    [NonCustomerEntity]
    public class StorePosition
    {
        [Key]
        public string Key { get; set; }
        public ulong CommitPosition { get; set; }
        public ulong PreparePosition { get; set; }
    }
}
