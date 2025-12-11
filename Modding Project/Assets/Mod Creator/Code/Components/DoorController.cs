using Code.Components.Enums;
using Code.Components.Events;
using System;
using System.Threading;
using UnityEngine;

namespace Code.Components
{
    public interface IDoorController
    {
        public EDoorType DoorType { get; set; }
        public DoorStateChangedEvent DoorStateChangedEvent { get; }
        public EDoorState DoorState { get; }
        public void Open();
        public void Close();
        public void Toggle();
        public void SetValue(float amount);
        public float GetValue();
    }

    public class DoorController : MonoBehaviour, IDoorController
    {
        [field: SerializeField]
        public EDoorType DoorType { get; set; }

        [field: SerializeField]
        public float Distance { get; set; }

        [field: SerializeField]
        public AnimationCurve DoorCurve { get; set; }

        [field: SerializeField]
        public float AnimationDuration { get; set; }

        [field: SerializeField]
        public Rigidbody DualRigidbody { get; set; }
        
        public DoorStateChangedEvent DoorStateChangedEvent => doorStateChangedEvent;
        public EDoorState DoorState => doorState;

        private DoorStateChangedEvent doorStateChangedEvent;
        private EDoorState doorState;
        
        public void Open()
        {

        }

        public void Close()
        {

        }

        public void Toggle()
        {

        }

        public void SetValue(float amount)
        {
            
        }

        public float GetValue()
        {
            return 0f;
        }
    }
}
