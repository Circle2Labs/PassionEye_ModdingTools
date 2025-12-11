using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Structs;
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
        /// Clothing state to hide cock map
        /// If this is true, the cock gets hidden (like underwear)
        /// </summary>
        public bool[] HideCock { get; set; }
        
        /// <summary>
        /// The state the current clothing item is being weared.
        /// </summary>
        public EClothingState ClothingState { get; }

        /// <summary>
        /// Blendshape offsets from the default states used for modeling this item.
        /// 
        /// Can be either:
        /// - null
        /// - array of size of the number of blendshapes in the model
        /// </summary>
        public SBlendshapeOffset[] BlendshapeOffsets { get; set; }
        
        /// <summary>
        /// The distance at which the clothing item should be culling the body mesh. (In meters)
        /// </summary>
        public float ClippingDistance { get; set; }

        /// <summary>
        /// The BVH structure per-state for the clothing item.
        /// </summary>
        public byte[] BVHData { get; set; }
        
        /// <summary>
        /// Set the clothing wearing state.
        /// </summary>
        public void SetState(EClothingState state);

        /// <summary>
        /// Get the GameObject for a specific state.
        /// </summary>
        public GameObject GetStateObject(EClothingState state);
        
        /// <summary>
        /// Gets the hide cock value for a specific state.
        /// </summary>
        public bool GetHideCock(EClothingState state);
    }
}