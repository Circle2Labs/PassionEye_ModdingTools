using Animancer;
using Code.Frameworks.InteractionSystem.Interactions.Base.H.Enums;
using System;
using System.Collections.Generic;
using Code.Frameworks.Animation.Enums;
using UnityEngine;

namespace Code.Frameworks.InteractionSystem.Interactions.Base.H.Structs
{
    public struct SActivityAnimation
    {
        public Guid Id;
        public string Name;
        public string Description;
        public Sprite Icon;
        public EHAnimationType Type;
        public bool IsAffectingActiveArousal;
        public bool IsAffectingPassiveArousal;
        public float ArousalModifier;
        public Vector3 CameraPositionOffset;
        public Vector3 CameraAnglesOffset;
        public float CameraDistance;
        public EClimaxType[] ClimaxTypes;

        public bool[] RaycastDown;

        public ITransition[] Idles;                                         //index = participant index
        public ITransition[] Activities;                                    //index = participant index

        public Dictionary<EClimaxType, ITransition[]> ClimaxIdles;          //index (value) = participant index
        public Dictionary<EClimaxType, ITransition[]> Climaxes;             //index (value) = participant index
        public Dictionary<EClimaxType, ITransition[]> ClimaxTransitions;    //index (value) = participant index

        public ITransition[] NonClimaxIdles;                                //index (value) = participant index
        public ITransition[] NonClimaxes;                                   //index (value) = participant index
        public ITransition[] NonClimaxTransitions;                          //index (value) = participant index
    }
}
