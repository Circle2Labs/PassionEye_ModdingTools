using Animancer;
using Code.Components;
using Code.Frameworks.Animation.Enums;
using Code.Frameworks.Animation;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.InteractionSystem.Interactions.Base.H.Enums;
using Code.Frameworks.InteractionSystem.Interactions.Base.H.Interfaces;
using Code.Frameworks.InteractionSystem.Interactions.Base.H.Structs;
using Code.Managers;
using Code.Tools;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Frameworks.InteractionSystem.Interactions.Base.H.Locations
{
    public class HLocation : MonoBehaviour, IHLocation
    {
        [field: SerializeField]
        public string Name { get; set; }

        [field: SerializeField]
        public ELocationType Type { get; set; }
        
        [field: SerializeField]
        public Image Icon { get; set; }
    }
}
