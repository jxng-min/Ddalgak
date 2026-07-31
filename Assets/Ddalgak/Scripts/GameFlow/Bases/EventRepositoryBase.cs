using System.Collections.Generic;
using UnityEngine;

namespace Ddalgak
{
    public abstract class EventRepositoryBase : MonoBehaviour
    {
        public abstract IReadOnlyList<EventData> GetAllEvents();
    }
}
