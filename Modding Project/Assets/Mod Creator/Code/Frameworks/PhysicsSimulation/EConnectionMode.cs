using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Frameworks.PhysicsSimulation
{
    public enum EConnectionMode : byte
    {
        None = 0,
        Line = 1,
        AutomaticMesh = 2,
        SequentialLoopMesh = 3,
        SequentialNonLoopMesh = 4,
    }
}
