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
        
#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            var offset = Vector3.zero;
            
            switch (Type)
            {
                case ELocationType.None:
                    break;
                case ELocationType.Flat:
                    Gizmos.DrawWireCube(Vector3.zero, new Vector3(1f, 0f, 1f));
                    break;
                case ELocationType.Seated:
                    Gizmos.DrawWireCube(new Vector3(0f, 0f, -0.35f), new Vector3(1f, 0f, 1f));
                    Gizmos.DrawWireCube(new Vector3(0f, -0.25f, 0.15f), new Vector3(1f, -0.5f, 0f));
                    Gizmos.DrawWireCube(new Vector3(0f, -0.5f, 0.65f), new Vector3(1f, 0f, 1f));
                    break;
                case ELocationType.Wall:
                    Gizmos.DrawWireCube(new Vector3(0f, 1f, 0f), new Vector3(1f, 2f, 0f));
                    Gizmos.DrawWireCube(new Vector3(0f, 0f, -0.5f), new Vector3(1f, 0f, 1f));

                    offset = new Vector3(0f, 0f, -0.15f);
                    break;
            }
            
            Gizmos.DrawLine(Vector3.zero + offset, new Vector3(0f, 0f, 0.15f) + offset);
			
            Gizmos.DrawLine(new Vector3(0f, 0f, 0.15f) + offset, new Vector3(0.15f, 0f, 0f) + offset);
            Gizmos.DrawLine(new Vector3(0f, 0f, 0.15f) + offset, new Vector3(-0.15f, 0f, 0f) + offset);
        }
#endif
    }
}
