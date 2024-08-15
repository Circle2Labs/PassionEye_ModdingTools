using Code.Frameworks.Character.Enums;
using UnityEngine;

namespace Code.Frameworks.Character.Interfaces
{
	/// <summary>
	/// Interface defining common functionality for clothing accessories.
	/// </summary>
	public interface IAccessory : ICharacterObject
	{
		/// <summary>
		/// The accessory category this item belongs to.
		/// </summary>
		public EAccessoryType AccessoryType { get; set; }

		/// <summary>
		/// The current accessory state this item currently is. (Weared or not)
		/// </summary>
		public EAccessoryState AccessoryState { get; }

		/// <summary>
		/// Visibility rules for this accessory item.
		/// </summary>
		public EAccessoryVisibility AccessoryVisibility { get; }

		/// <summary>
		/// Get if the accessory item is bound to an <see cref="IClothing"/> item.
		/// </summary>
		/// <returns>The <see cref="IClothing"/> item this item is bound to, <see langword="null"/> otherwise.</returns>
		public IClothing GetAccessoryBoundClothing();

		/// <summary>
		/// Binds the accessory item to an <see cref="IClothing"/> item.
		/// </summary>
		/// <param name="clothing">The <see cref="IClothing"/> item the accessory will bind to.</param>
		public void SetAccessoryBoundClothing(IClothing clothing);

		public void SetAccessoryVisibility(EAccessoryVisibility visibility);
		
		/// <summary>
		/// Get the parent <see cref="Transform"/> this accessory is connected to.
		/// </summary>
		/// <returns><see cref="Transform"/> this accessory item is connected to, <see langword="null"/> otherwise.</returns>
		public Transform GetAccessoryParent();

		/// <summary>
		/// Set the parent <see cref="Transform"/> this accessory is connected to.
		/// </summary>
		/// <param name="parent">The <see cref="Transform"/> that will be the accessories parent.</param>
		public void SetAccessoryParent(Transform parent);
		
		/// <summary>
		/// Set the accessory wearing state.
		/// </summary>
		public void SetState(EAccessoryState state);
	}
}