// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System.Collections.Generic;
using UnityEngine;

namespace Animancer
{
    /// <summary>A set of up/right/down/left animations with diagonals as well.</summary>
    /// <remarks>
    /// Consider using <c>DirectionalSet&lt;AnimationClip&gt;</c> in code instead of this class
    /// to allow <see cref="DirectionalAnimationSet4"/> and <see cref="DirectionalAnimationSet8"/>
    /// to be used interchangeably.
    /// <para></para>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/playing/directional-sets">
    /// Directional Animation Sets</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/DirectionalAnimationSet8
    /// 
    [CreateAssetMenu(
        menuName = Strings.MenuPrefix + "Directional Animation Set/8 Directions",
        order = Strings.AssetMenuOrder + 6)]
    [AnimancerHelpUrl(typeof(DirectionalAnimationSet8))]
    public class DirectionalAnimationSet8 : DirectionalSet8<AnimationClip>,
        IAnimationClipSource
    {
        /************************************************************************************************************************/

        /// <summary>[<see cref="IAnimationClipSource"/>] Adds all animations from this set to the `clips`.</summary>
        void IAnimationClipSource.GetAnimationClips(List<AnimationClip> clips)
            => AddTo(clips);

        /************************************************************************************************************************/
    }
}

