using System;
using System.Collections.Generic;
using Code.Frameworks.InteractionSystem.Context.Actions;
using Code.Frameworks.InteractionSystem.Context.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace Code.Frameworks.InteractionSystem.Context
{
    public class ActionCategory
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Sprite Icon { get; set; }
        public ActionCategory? Parent { get; set; }
        public List<IGameAction> Actions { get; set; } = new();
        public List<ActionCategory> SubCategories { get; set; } = new();
        public GameObject Owner { get; set; }
        
        public void DisplayOnActionWheel()
        {
        }

        public void AddAction(IGameAction action)
        {
        }

        public void RemoveAction(IGameAction action)
        {
        }
        
        public void ClearActions()
        {
        }

        public void JoinOrAddSubCategory(ActionCategory subCategory)
        {
        }
    }
}