using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderSwapper : MonoBehaviour
{
    private GameObject currentIObject;
    private MeshRenderer targetMeshRenderer;
    private Material originalMaterial;
    
    private HashSet<Camera> activeCameras = new HashSet<Camera>();
    private Dictionary<Camera, MaterialPropertyBlock> cameraPropertyBlocks = new Dictionary<Camera, MaterialPropertyBlock>();

    private Material[] cachedMaterials;

    public Material appliedShaderMaterial;
    public int targetMaterialIndex = 1;
    public int playerID = 1;

    public void RevertShader()
    {
        if (currentIObject != null && targetMeshRenderer != null && originalMaterial != null && cachedMaterials != null)
        {
            foreach (var kvp in cameraPropertyBlocks)
            {
                var propertyBlock = kvp.Value;
                propertyBlock.SetFloat("_IsVisible", 0f);
                targetMeshRenderer.SetPropertyBlock(propertyBlock);
            }

            cachedMaterials[targetMaterialIndex] = originalMaterial;
            targetMeshRenderer.materials = cachedMaterials;
        }

        activeCameras.Clear();
        cameraPropertyBlocks.Clear();
        cachedMaterials = null;
        currentIObject = null;
        targetMeshRenderer = null;
        originalMaterial = null;
    }

    public void ChangeShader(GameObject targetIObject, Camera targetCamera = null)
    {
        if (currentIObject != targetIObject)
        {
            if (currentIObject != null)
            {
                RevertShader();
            }
            
            if (targetIObject == null || targetCamera == null)
            {
                return;
            }

            targetMeshRenderer = targetIObject.GetComponent<MeshRenderer>();
            if (targetMeshRenderer == null) return;

            cachedMaterials = targetMeshRenderer.materials;
            if (cachedMaterials.Length <= targetMaterialIndex)
            {
                cachedMaterials = null;
                return;
            }

            originalMaterial = cachedMaterials[targetMaterialIndex];
            currentIObject = targetIObject;
            
            cachedMaterials[targetMaterialIndex] = appliedShaderMaterial;
            targetMeshRenderer.materials = cachedMaterials;
        }

        if (targetCamera != null && !activeCameras.Contains(targetCamera))
        {
            activeCameras.Add(targetCamera);
            
            if (!cameraPropertyBlocks.ContainsKey(targetCamera))
            {
                cameraPropertyBlocks[targetCamera] = new MaterialPropertyBlock();
            }

            var propertyBlock = cameraPropertyBlocks[targetCamera];
            targetMeshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetInt("_PlayerCameraID", targetCamera.GetInstanceID());
          //  propertyBlock.SetInt("_PlayerID", playerID); // Add player ID to property block
            propertyBlock.SetFloat("_IsVisible", 1f);
            propertyBlock.SetFloat("_TimeStamp", Time.time);
            targetMeshRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    public void RemoveCamera(Camera targetCamera)
    {
        if (activeCameras.Contains(targetCamera))
        {
            activeCameras.Remove(targetCamera);
            
            if (cameraPropertyBlocks.ContainsKey(targetCamera))
            {
                var propertyBlock = cameraPropertyBlocks[targetCamera];
                propertyBlock.SetFloat("_IsVisible", 0f);
                targetMeshRenderer.SetPropertyBlock(propertyBlock);
                cameraPropertyBlocks.Remove(targetCamera);
            }

            if (activeCameras.Count == 0)
            {
                RevertShader();
            }
        }
    }
}