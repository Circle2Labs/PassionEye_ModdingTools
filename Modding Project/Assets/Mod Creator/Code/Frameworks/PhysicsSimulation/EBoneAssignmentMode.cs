namespace Code.Frameworks.PhysicsSimulation
{
    public enum EBoneAssignmentMode : byte
    {

        /// <summary>
        /// Do nothing.
        /// 
        /// e.g. non skinned accessories (no armature)
        /// </summary>
        None = 0,

        /// <summary>
        /// Attach the objects armature to a specific bone from another armature
        /// 
        /// 
        /// e.g. skinned accessories (yes armature)
        /// </summary>
        AttachOnly = 1,

        /// <summary>
        /// The objects armature follows the structure of the target armature.
        /// Reassign all bones of this armature to the bones of the target armature.
        /// Adds missing bones to the target armature as well.
        /// 
        /// e.g. clothing
        /// </summary>
        Full = 2,
    }
}
