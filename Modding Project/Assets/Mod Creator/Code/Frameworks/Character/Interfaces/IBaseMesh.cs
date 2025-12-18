using System.Collections.Generic;
using Animancer;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Flags;
using Code.Frameworks.Character.Structs;
using Code.Frameworks.ClippingFix.Enums;
using Code.Frameworks.ForwardKinematics.Interfaces;
using UnityEngine;

namespace Code.Frameworks.Character.Interfaces
{
	/// <summary>
	/// Interface defining common functionality for character base meshes.
	/// </summary>
	public interface IBaseMesh : ICharacterObject, IFKCompatible
	{
		#region Serialized Game Data

		/// <summary>
		/// Avatar for this base mesh
		/// </summary>
        public SAvatarData AvatarData { get; set; }
        
		/// <summary>
		/// What character gender this base mesh is made for
		/// </summary>
        public ESupportedGendersFlags SupportedGendersFlags { get; set; }
        
		/// <summary>
		/// Presets for blendshap groups, shown in the character maker
		/// </summary>
        public SBlendshapePreset[] BlendshapePresets { get; set; }
        
		/// <summary>
		/// Valid accessory parents for this base mesh
		/// </summary>
		public List<Tools.Tuple.SerializableTuple<Transform, string>> AccessoryParents { get; set; }
		/// <summary>
		/// Map of textures to their specific renderers and submesh indexes
		/// </summary>
		public List<Tools.Tuple.SerializableTuple<ETextureType, Renderer, int>> TextureMaterialMap { get; set; }
		
		/// <summary>
		/// Blendshapes that are shown and are editable in the character maker
		/// </summary>
		public List<SBlendShape> Blendshapes { get; set; }
		/// <summary>
		/// Body parts for parenting objects to in the studio
		/// </summary>
		public List<SBodyPart> BodyParts { get; set; }
        
		/// <summary>
		/// Body renderers containing blendshapes
		/// </summary>
		public List<SkinnedMeshRenderer> BlendshapeRenderers { get; set; }
        
		/// <summary>
		/// Collider that is activated in SFW mode to block the camera clipping into the body
		/// </summary>
		public List<Collider> SFWColliders { get; set; }
        
		/// <summary>
		/// Transform which the POV camera mode will follow
		/// </summary>
		public Transform POV { get; set; }
		/// <summary>
		/// Transform which is used for the face vectors
		/// </summary>
		public Transform FaceTransform { get; set; }
		
		/// <summary>
		/// Blendshapes used for eye control
		/// </summary>
		public SEyeData EyeData { get; set; }
		/// <summary>
		/// Blendshapes used for mouth control
		/// </summary>
		public SMouthData MouthData { get; set; }
		
		public Transform Cock { get; set; }
		public Transform HideVag { get; set; }
		public Transform BodyRootBone { get; set; }
		public Transform HeadRootBone { get; set; }
		public Transform PrivatesRootBone { get; set; }
		
		public Transform MergedEyes { get; set; }
		public bool InvertMergedEyes { get; set; }
		
		public Transform[] Eyes { get; set; }
		public Transform[] Breasts { get; set; }
        public Transform[] Buttocks { get; set; }
        public Transform[] Balls { get; set; }
        
        public bool EyeControl { get; set; }
        public bool ExpressionControl { get; set; }
        public bool PoseControl { get; set; }
			
        /// <summary>
        /// Internal IDs of the base mesh rays to be grabbed from the mod file
        /// </summary>
        public List<Tools.Tuple.SerializableTuple<ERaysResolution, byte>> RaysIDs { get; set; }
        
        #endregion
        
        #region Runtime Game Data

        public Avatar Avatar { get; set; }
        
        #endregion
        
        #region Runtime Game Methods

        /// <summary>
        /// Returns the body renderer and body submesh
        /// </summary>
        public (SkinnedMeshRenderer, int)? GetBodyRenderer();
        
        #endregion
	}
}