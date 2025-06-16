using Code.Components.Enums;
using UnityEngine.Events;

namespace Code.Components.Events
{
    public class DoorStateChangedEvent : UnityEvent<EDoorState, EDoorState>
    { }
}
