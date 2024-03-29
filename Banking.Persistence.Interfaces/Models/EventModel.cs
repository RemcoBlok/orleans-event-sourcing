﻿namespace Banking.Persistence.Interfaces.Models
{
    public class EventModel<TEventBase> where TEventBase : notnull
    {
        public required TEventBase Data { get; set; }

        public required EventMetadata Metadata { get; set; }
    }
}
