using System;

namespace Assets.Code.Frameworks.Animation.Enums
{
    /// <summary>
    /// This enums values are identical to the ones in ESupportedGendersFlags in the beginning.
    /// The gap is for supporting additional flags in the future.
    /// </summary>
    [Flags]
    public enum EClipUsageFlags
    {
        None = 0,
        Male = 1,
        Female = 2,
        Active = 32,
        Passive = 64
    }
}
