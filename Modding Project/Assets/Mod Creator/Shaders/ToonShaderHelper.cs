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

    [ExecuteAlways]
    public void Update()
    {
        if (FaceMaterial == null || FaceTransform == null)
            return;
        
        FaceMaterial.SetVector("_FaceCenter", FaceTransform.position);
        FaceMaterial.SetVector("_FaceFwdVec", FaceTransform.forward);
        FaceMaterial.SetVector("_FaceRightVec", FaceTransform.right);
    }
}
