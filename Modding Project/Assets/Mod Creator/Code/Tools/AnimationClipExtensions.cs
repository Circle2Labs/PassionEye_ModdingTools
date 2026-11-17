using Animancer;
using UnityEngine;

namespace Code.Tools
{
    public static class AnimationClipExtensions
    {
        public static ClipTransition ToClipTransition(this AnimationClip clip, float fadeDuration)
        {
            if (clip == null)
                return null;

            var clipTransition = new ClipTransition();
            clipTransition.FadeDuration = fadeDuration;
            clipTransition.Clip = clip;

            return clipTransition;
        }
    }
}
