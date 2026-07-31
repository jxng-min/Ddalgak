using System.Collections.Generic;
using UnityEngine;

namespace Ddalgak
{
    public sealed class EventRepository : EventRepositoryBase
    {
        [SerializeField] private List<EventData> events = new();

        public override IReadOnlyList<EventData> GetAllEvents()
        {
            return events;
        }
    }
}
