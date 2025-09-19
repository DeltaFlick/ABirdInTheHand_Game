using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class PlayerCustomPass : CustomPass
{
    [SerializeField] private Material playerShaderMaterial;
    [SerializeField] private int targetMaterialIndex = 1;
    [SerializeField] private bool enableFrustumCulling = true;
    
    private int playerID;
    private Camera playerCamera;
    private GameObject targetObject;
    private bool isActive = false;
    private Material materialInstance; 

    public void SetPlayerData(int id, Camera camera)
    {
        playerID = id;
        playerCamera = camera;
        
        EnsureMaterialInstance();
        
        if (materialInstance != null)
        {
            materialInstance.SetInt("_PlayerID", playerID);
            Debug.Log($"Player {playerID} material instance created and PlayerID set");
        }
    }

    public void SetTargetObject(GameObject obj)
    {
        targetObject = obj;
        isActive = obj != null;
        
        EnsureMaterialInstance();
        
        if (materialInstance != null)
        {
            materialInstance.SetFloat("_IsVisible", isActive ? 1f : 0f);
        }
    }

    public void ClearTarget()
    {
        targetObject = null;
        isActive = false;
        
        if (materialInstance != null)
        {
            materialInstance.SetFloat("_IsVisible", 0f);
        }
    }

    private void EnsureMaterialInstance()
    {
        if (materialInstance == null && playerShaderMaterial != null)
        {
            materialInstance = new Material(playerShaderMaterial);
            materialInstance.name = $"{playerShaderMaterial.name}_Player{playerID}_Instance";
            Debug.Log($"Created material instance for Player {playerID}: {materialInstance.name}");
        }
    }

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (!isActive || targetObject == null || ctx.hdCamera.camera != playerCamera)
            return;

        MeshRenderer renderer = targetObject.GetComponent<MeshRenderer>();
        if (renderer == null)
            return;

        EnsureMaterialInstance();
        if (materialInstance == null)
            return;

        MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            if (enableFrustumCulling)
            {
                Bounds bounds = meshFilter.sharedMesh.bounds;
                bounds.center = targetObject.transform.TransformPoint(bounds.center);
                bounds.size = Vector3.Scale(bounds.size, targetObject.transform.lossyScale);
                if (!GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(playerCamera), bounds))
                    return;
            }

            materialInstance.SetFloat("_TimeStamp", Time.time);
            
            //materialInstance.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
           // materialInstance.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            //materialInstance.SetInt("_ZWrite", 0);

            Matrix4x4 matrix = targetObject.transform.localToWorldMatrix;

            for (int i = 0; i < meshFilter.sharedMesh.subMeshCount; i++)
            {
                ctx.cmd.DrawMesh(
                    meshFilter.sharedMesh,
                    matrix,
                    materialInstance,
                    i 
                );
            }
        }
    }

    protected override void Cleanup()
    {
        targetObject = null;
        isActive = false;
        
        if (materialInstance != null)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(materialInstance);
            }
            else
            {
                Object.DestroyImmediate(materialInstance);
            }
            materialInstance = null;
        }
    }
}