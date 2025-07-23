using Code.Frameworks.InteractionSystem.Interactions.Base.H.Enums;
using Code.Frameworks.InteractionSystem.Interactions.Base.H.Structs;
using UnityEngine.UI;

namespace Code.Frameworks.InteractionSystem.Interactions.Base.H.Interfaces
{
    public interface IHLocation
    {
        public string Name { get; set; }
        public ELocationType Type { get; set; }
    }
}
