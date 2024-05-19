using Code.Frameworks.Character.Enums;
using UnityEngine;

namespace Code.Frameworks.Character.Interfaces
{
    /// <summary>
    /// Interface defining common functionality for clothing items.
    /// </summary>
    public interface IClothing : ICharacterObject
    {
        /// <summary>
        /// The clothing category this item belongs to.
        /// </summary>
        public EClothingType ClothingType { get; set; }

        /// <summary>
        /// Clothing state to gameobject map
        /// </summary>
        public Transform[] ClothingStates { get; set; }
        
        /// <summary>
        /// The state the current clothing item is being weared.
        /// </summary>
        public EClothingState ClothingState { get; }

        /// <summary>
        /// Set the clothing wearing state.
        /// </summary>
        public void SetState(EClothingState state);

        /// <summary>
        /// Get the GameObject for a specific state.
        /// </summary>
        public GameObject GetStateObject(EClothingState state);
    }
}