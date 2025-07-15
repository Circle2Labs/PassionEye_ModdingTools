using UnityEngine;

namespace Code.Frameworks.MovementSystem
{
    public class DeathPlaneController : MonoBehaviour
    {
        [field: SerializeField]
        public Collider Collider { get; private set; }

        [field: SerializeField]
        public Transform ReturnToTransform { get; private set; }
    }
}
