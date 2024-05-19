using Code.Frameworks.Character.Enums;

namespace Code.Frameworks.Character.Interfaces
{
    /// <summary>
    /// Interface defining common functionality for Hair items.
    /// </summary>
    public interface IHair : ICharacterObject
    {
        /// <summary>
        /// The hair category this item belongs to.
        /// </summary>
        public EHairType HairType { get; set; }
    }
}