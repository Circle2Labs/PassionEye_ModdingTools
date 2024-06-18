using Code.Frameworks.Character.Enums;
using UnityEngine;

namespace Code.Frameworks.Character.Interfaces
{
	public interface ITexture : ICharacterObject
	{
		public ETextureType TextureType { get; set; }
		
		public EOverlayTarget OverlayTarget { get; set; }
		
		public EOverlayMode OverlayMode { get; set; }

		public Texture2D Texture { get; set; }
		
		/// <summary>
		/// If used, texture is assigned on runtime and saved into bytes of the character card
		/// instead of being a mod. Using a mod is much better for performance and reliability, this
		/// should really only be used for testing before you turn it into a mod.
		/// </summary>
		public bool IsCustom { get; set; }
		
		public bool IsOverlay { get; set; }
		
		public Color OverlayColor { get; set; }
		
		/// <summary>
		/// Texture might have changed, if it's an overlay ask the overlay controller to reapply them, if it's not an overlay - apply the Texture (optionally also update all overlays)
		/// </summary>
		public void UpdateTexture(bool updateOverlays = true);
		
		/// <summary>
		/// Color might have changed, if it's an overlay ask the overlay controller to reapply them 
		/// </summary>
		public void UpdateColor(bool reset = false);
		
		/// <summary>
		/// Apply given texture to the material
		/// </summary>
		public void ApplyTexture(Texture2D texture);
	}
}