using UnityEngine;

[ExecuteInEditMode]
public class ToonShaderHelper : MonoBehaviour
{
    public Transform FaceTransform;

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

    private static readonly int FaceCenter = Shader.PropertyToID("_FaceCenter");
    private static readonly int FaceFwdVec = Shader.PropertyToID("_FaceFwdVec");
    private static readonly int FaceRightVec = Shader.PropertyToID("_FaceRightVec");
    
    [ExecuteAlways]
    public void Update()
    {
        var rotation = FaceTransform.rotation;
        var material = FaceMaterial;

        material.SetVector(FaceCenter, FaceTransform.position);
        material.SetVector(FaceFwdVec, rotation * Vector3.forward);
        material.SetVector(FaceRightVec, rotation * Vector3.right);
    }
}
