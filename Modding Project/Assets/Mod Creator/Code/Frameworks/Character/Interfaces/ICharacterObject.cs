using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Flags;
using Code.Interfaces;
using UnityEngine;

namespace Code.Frameworks.Character.Interfaces
{
    /// <summary>
    /// Interface defining common functionality for generic items in the character editor and gameplay.
    /// </summary>
    public interface ICharacterObject : IMaterialCustomizable
    {
        /// <summary>
        /// Allow only one character object of this ECharacterObjectType
        /// </summary>
        public bool IsSingular { get; }

        /// <summary>
        /// The character object type this object belongs to
        /// </summary>
        public ECharacterObjectType ObjectType { get; set; }
        
        /// <summary>
        /// Icon of the item inside the character editor.
        /// </summary>
        public Sprite Icon { get; set; }

        /// <summary>
        /// Mame assigned to the item.
        /// </summary>   
        public string Name { get; set; }

        /// <summary>
        /// Array of tags assigned to the item.
        /// </summary>
        public string[] Tags { get; set; }

        /// <summary>
        /// Description string assigned to the item.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Is this item Not safe for work.
        /// </summary>   
        public bool IsNSFW { get; set; }
        
        /// <summary>
        /// Supported character Genders for this Item
        /// </summary>
        public ESupportedGendersFlags SupportedGendersFlags { get; set; }

        public Character Character { get; }
        
        /// <summary>
        /// Assigns the object for rendering on the character.
        /// </summary>
        public void Assign(Character chara);
        
        /// <summary>
        /// Removes the object from rendering on the character.
        /// </summary>
        public void Remove(Character chara);

        /// <summary>
        /// GameObject for this character object
        /// </summary>
        public GameObject GetGameObject();

        //TODO: maybe add material and meshes related methods
    }
}
