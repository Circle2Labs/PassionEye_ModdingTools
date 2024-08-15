using UnityEngine;

[ExecuteInEditMode]
public class ToonShaderHelper : MonoBehaviour
{
    [SerializeField]
    public Transform FaceTransform;

    [SerializeField] 
    private Transform Eye_L;
    
    [SerializeField] 
    private Transform Eye_R;
    
    private Material faceMaterial;
    public Material FaceMaterial
    {
        get
        {
            if (faceMaterial != null)
                return faceMaterial;

            var materials = GetComponent<Renderer>().sharedMaterials;
            foreach (var material in materials)
            {
                if (!material.name.StartsWith("Face"))
                    continue;

                faceMaterial = material;
                return faceMaterial;
            }

            return null;
        }
        set => faceMaterial = value;
    }

    private Material eyeMaterialL;
    public Material EyeMaterialL
    {
        get
        {
            if (eyeMaterialL != null)
                return eyeMaterialL;

            var materials = Eye_L.GetComponent<Renderer>().sharedMaterials;
            foreach (var material in materials)
            {
                if (!material.name.StartsWith("Eye") || material.name.StartsWith("Eye_Socket"))
                    continue;

                eyeMaterialL = material;
                return eyeMaterialL;
            }

            return null;
        }
        set => eyeMaterialL = value;
    }
    
    private Material eyeMaterialR;
    public Material EyeMaterialR
    {
        get
        {
            if (eyeMaterialR != null)
                return eyeMaterialR;

            var materials = Eye_R.GetComponent<Renderer>().sharedMaterials;
            foreach (var material in materials)
            {
                if (!material.name.StartsWith("Eye") || material.name.StartsWith("Eye_Socket"))
                    continue;
                
                eyeMaterialR = material;
                return eyeMaterialR;
            }

            return null;
        }
        set => eyeMaterialR = value;
    }

    private static readonly int FaceCenter = Shader.PropertyToID("_FaceCenter");
    private static readonly int FaceFwdVec = Shader.PropertyToID("_FaceFwdVec");
    private static readonly int FaceRightVec = Shader.PropertyToID("_FaceRightVec");

    [ExecuteAlways]
    public void Update()
    {
        var rotation = FaceTransform.rotation;
        var position = FaceTransform.position;

        FaceMaterial.SetVector(FaceCenter, position);
        FaceMaterial.SetVector(FaceFwdVec, rotation * Vector3.forward);
        FaceMaterial.SetVector(FaceRightVec, rotation * Vector3.right);
    }

    public void SetEyesForwardVector(Vector3 forward_L, Vector3 forward_R)
    {
        var position = FaceTransform.position;
        
        EyeMaterialL.SetVector(FaceCenter, position);
        EyeMaterialL.SetVector(FaceFwdVec, forward_L);
        EyeMaterialR.SetVector(FaceCenter, position);
        EyeMaterialR.SetVector(FaceFwdVec, forward_R);
    }
}
