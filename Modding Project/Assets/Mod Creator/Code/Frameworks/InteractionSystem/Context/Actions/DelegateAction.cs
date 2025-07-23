using Code.Frameworks.InteractionSystem.Context.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace Code.Frameworks.InteractionSystem.Context.Actions
{
    public class DelegateAction : IGameAction
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Sprite Icon { get; set; }
        public GameObject Target { get; set; }
        
        //delegate specific properties
        public UnityAction Action { get; set; }
        
        public void Run()
        {
        }
    }
}