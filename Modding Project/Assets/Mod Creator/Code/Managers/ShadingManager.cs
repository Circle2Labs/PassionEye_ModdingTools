using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Managers
{
    [ExecuteAlways]
    public class ShadingManager : MonoBehaviour
    {
        [Header("Shading Properties")] [SerializeField] [Range(0.0001f, 1f)]
        private float _LightSmooth = 0.25f;

        public float LightSmooth
        {
            get => _LightSmooth;
            set
            {
                if (value > 1f || value < 0.0001f)
                {
                    Debug.LogWarning("LightSmooth must be between 0.0001 and 1");
                    return;
                }
                _LightSmooth = value;
                _isDirty = true;
            }
        }

        [SerializeField] [Range(0, 0.999f)] private float _LightMin = 0.2f;
        
        public float LightMin
        {
            get => _LightMin;
            set
            {
                if (value < 0 || value > 0.999f)
                {
                    Debug.LogWarning("LightMin must be between 0 and 0.999");
                    return;
                }
                _LightMin = value;
                _isDirty = true;
            }
        }
        
        [SerializeField] [Range(0, 1f)] private float _MidPoint = 0.35f;
        
        public float MidPoint
        {
            get => _MidPoint;
            set
            {
                if (value < 0 || value > 1f)
                {
                    Debug.LogWarning("MidPoint must be between 0 and 1");
                    return;
                }
                _MidPoint = value;
                _isDirty = true;
            }
        }

        [Header("Day Night Cycle")] [SerializeField] [Range(0.01f, 0.5f)]
        private float _ShiftAmount = 0.01f;
        
        public float ShiftAmount
        {
            get => _ShiftAmount;
            set
            {
                if (value < 0.01f || value > 0.5f)
                {
                    Debug.LogWarning("ShiftAmount must be between 0.01 and 0.5");
                    return;
                }
                _ShiftAmount = value;
                _isDirty = true;
            }
        }

        [SerializeField] [Range(0, 1f)] private float _kelvinTemp = .25f;
        
        public float KelvinTemp
        {
            get => _kelvinTemp;
            set
            {
                if (value < 0 || value > 1f)
                {
                    Debug.LogWarning("KelvinTemp must be between 0 and 1");
                    return;
                }
                _kelvinTemp = value;
                _isDirty = true;
            }
        }
        
        [SerializeField] private Texture2D _GradientTexture;
        [FormerlySerializedAs("DayNightTintStrength")] [SerializeField] [Range(0, 1f)] private float _DayNightTintStrength = 0f;
        
        public float DayNightTintStrength
        {
            get => _DayNightTintStrength;
            set
            {
                if (value < 0 || value > 1f)
                {
                    Debug.LogWarning("DayNightTintStrength must be between 0 and 1");
                    return;
                }
                _DayNightTintStrength = value;
                _isDirty = true;
            }
        }
        

        public Texture2D GradientTexture
        {
            get => _GradientTexture;
            set
            {
                _GradientTexture = value;
                _isTextureDirty = true;
            }
        }

        private float[] previousLightSmoothVals = new float[3] { 0.1f, 0.5f, 1f };
        private float[] previousDNVals = new float[3] { 0.1f, 0.5f, 1f };

        private bool _isDirty = true;
        private bool _isTextureDirty = true;

        public void OnDestroy()
        {
            // If the current one gets destroyed, make the parent one dirty so it applies again

            var shadingManagers = FindObjectsOfType<ShadingManager>();
            for (var i = 0; i < shadingManagers.Length; i++)
            {
                var shadingManager = shadingManagers[i];
                if (shadingManager == null || shadingManager == this)
                    continue;

                shadingManager.MarkDirty();
                break;
            }
        }

        // Update is called once per frame
        public void Update()
        {
            if (!Mathf.Approximately(previousLightSmoothVals[0], _LightSmooth) ||
                !Mathf.Approximately(previousLightSmoothVals[1], _LightMin) ||
                !Mathf.Approximately(previousLightSmoothVals[2], _MidPoint))
            {
                _isDirty = true;
            }

            if (!Mathf.Approximately(previousDNVals[0], _ShiftAmount) ||
                !Mathf.Approximately(previousDNVals[1], _kelvinTemp) ||
                !Mathf.Approximately(previousDNVals[2], DayNightTintStrength))
            {
                _isDirty = true;
            }

            if (_isDirty) UpdateValues();
            if (_isTextureDirty) UpdateTexture();
        }

        public void MarkDirty()
        {
            _isDirty = true;
            _isTextureDirty = true;
        }

        private void UpdateValues()
        {
            Shader.SetGlobalFloat("_LightSmooth", _LightSmooth);
            Shader.SetGlobalFloat("_LightMin", _LightMin);
            Shader.SetGlobalFloat("_MidPoint", _MidPoint);
            Shader.SetGlobalFloat("_ShiftAmount", _ShiftAmount);
            Shader.SetGlobalFloat("_kelvinTemp", _kelvinTemp);
            Shader.SetGlobalFloat("_DNTintStr", DayNightTintStrength);
            previousLightSmoothVals[0] = _LightSmooth;
            previousLightSmoothVals[1] = _LightMin;
            previousLightSmoothVals[2] = _MidPoint;
            previousDNVals[0] = _ShiftAmount;
            previousDNVals[1] = _kelvinTemp;
            previousDNVals[2] = DayNightTintStrength;
            _isDirty = false;
        }

        private void UpdateTexture()
        {
            Shader.SetGlobalTexture("_DayNightRamp", _GradientTexture);
            _isTextureDirty = false;
        }
    }
}