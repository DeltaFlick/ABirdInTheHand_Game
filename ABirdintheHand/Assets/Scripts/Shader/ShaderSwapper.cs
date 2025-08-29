using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderSwapper : MonoBehaviour
{
    private Material[] originalMaterials;
    private GameObject currentIObject;
    private MeshRenderer targetMeshRenderer;
    
    //private Color originalColor;

    public Material appliedShaderMaterial;
    public int targetMaterialIndex = 1; 

    public void RevertShader()
    {
        if (currentIObject != null && originalMaterials != null)
        {
            targetMeshRenderer.materials = originalMaterials;
        }
        currentIObject = null;
        originalMaterials = null;
        targetMeshRenderer = null;
    }

   public void ChangeShader(GameObject targetIObject)
{
    if (currentIObject == targetIObject) return;
    if (currentIObject != null)
    {
        RevertShader();
    }
    if (targetIObject == null || appliedShaderMaterial == null)
        return;

    targetMeshRenderer = targetIObject.GetComponent<MeshRenderer>();
    if (targetMeshRenderer == null || targetMeshRenderer.materials.Length <= targetMaterialIndex)
        return;

    currentIObject = targetIObject;
    
    Material[] currentMaterials = targetMeshRenderer.materials;
    originalMaterials = new Material[currentMaterials.Length];
    for (int i = 0; i < currentMaterials.Length; i++)
    {
        originalMaterials[i] = currentMaterials[i];
    }

    Material[] materials = new Material[currentMaterials.Length];
    for (int i = 0; i < materials.Length; i++)
    {
        materials[i] = (i == targetMaterialIndex) ? appliedShaderMaterial : currentMaterials[i];
    }
    
    if (materials.Length > targetMaterialIndex)
    {
        targetMeshRenderer.materials = materials;
    }
}
//  DebugCheckRenderer();
    public void DebugCheckRenderer()
    {

        if (targetMeshRenderer == null)
        {
            Debug.Log("No Mesh Renderer found on object");
        }else
        {
            Debug.Log(targetMeshRenderer.materials.Length);
        }
    }
    
}
