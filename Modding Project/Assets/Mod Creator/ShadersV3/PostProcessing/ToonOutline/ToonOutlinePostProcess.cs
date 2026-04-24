using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Code.Frameworks.Outline
{
    [Serializable, VolumeComponentMenuForRenderPipeline("PostProcessing/ToonOutline", typeof(UniversalRenderPipeline))]
    public class ToonOutlinePostProcess : VolumeComponent, IPostProcessComponent
    {
        public BoolParameter EnableToonOutline = new BoolParameter(false);
        
        public FloatParameter DepthThreshold = new FloatParameter(1.0f);
        public FloatParameter NormalsThreshold = new FloatParameter(0.45f);
        public NoInterpColorParameter OutlineColor = new NoInterpColorParameter(Color.black, false, true, true);
        public IntParameter Radius = new IntParameter(1);
        public BoolParameter Use5x5 = new BoolParameter(false);
        public FloatParameter AngleFixScale = new FloatParameter(1.7f);
        public FloatParameter AngleFixPower = new FloatParameter(0.85f);
        public FloatParameter FadeStart = new FloatParameter(0.0f);
        public FloatParameter FadeEnd = new FloatParameter(30.0f);
        
        public FloatParameter BlurRadius = new FloatParameter(1.0f);
        
        public BoolParameter DebugOutline = new BoolParameter(false);
        public FloatParameter DebugTransparency = new FloatParameter(0.5f);
        public bool IsActive() => EnableToonOutline.value;
        public bool IsTileCompatible() => false;    
    }
}