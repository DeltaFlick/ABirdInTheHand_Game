using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Experimental.Rendering;

public class OutlineEffect : CustomPass
{
    public LayerMask outlineLayer;
    public Color outlineColor = Color.cyan;
    public float thickness = 1.5f;
    [SerializeField] private Shader outlineShader;
    [SerializeField] private Shader maskShader;

    private Material outlineMaterial;
    private Material maskMaterial;
    private GameObject targetObject;

    private RTHandle tempRT;
    private int lastWidth = -1;
    private int lastHeight = -1;

    private Renderer[] cachedRenderers;
    private bool renderersValid = false;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        try
        {
            Debug.Log("OutlineEffect: Setup started");

            if (outlineShader != null)
            {
                outlineMaterial = new Material(outlineShader);
                Debug.Log("OutlineEffect: Outline material created successfully");
            }
            else
            {
                Debug.LogError("OutlineEffect: Outline shader is not assigned!");
            }

            if (maskShader != null)
            {
                maskMaterial = new Material(maskShader);

                if (maskMaterial.HasProperty("_Color"))
                {
                    maskMaterial.SetColor("_Color", Color.white);
                }
                if (maskMaterial.HasProperty("_UnlitColor"))
                {
                    maskMaterial.SetColor("_UnlitColor", Color.white);
                }

                Debug.Log($"OutlineEffect: Mask material created with {maskShader.name}");
            }
            else
            {
                Debug.LogError("OutlineEffect: Mask shader is not assigned in inspector!");
            }

            Debug.Log("OutlineEffect: Setup complete");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"OutlineEffect Setup Error: {e.Message}\n{e.StackTrace}");
        }
    }

protected override void Execute(CustomPassContext ctx)
    {
        try
        {
            if (outlineMaterial == null || maskMaterial == null)
            {
                return;
            }

            if (targetObject == null || !renderersValid)
                return;

            var camera = ctx.hdCamera.camera;
            var cmd = ctx.cmd;

            if ((outlineLayer & (1 << targetObject.layer)) == 0)
            {
                return;
            }

            var viewport = camera.pixelRect;
            int viewportWidth = Mathf.RoundToInt(viewport.width);
            int viewportHeight = Mathf.RoundToInt(viewport.height);

            if (viewportWidth <= 0 || viewportHeight <= 0)
            {
                return;
            }

            if (tempRT == null || lastWidth != viewportWidth || lastHeight != viewportHeight)
            {
                if (tempRT != null)
                {
                    RTHandles.Release(tempRT);
                }

                tempRT = RTHandles.Alloc(
                    viewportWidth, viewportHeight, TextureXR.slices,
                    dimension: TextureXR.dimension,
                    colorFormat: GraphicsFormat.R8G8B8A8_UNorm,
                    useDynamicScale: true,
                    name: "OutlineMask");

                lastWidth = viewportWidth;
                lastHeight = viewportHeight;
            }

            CoreUtils.SetRenderTarget(cmd, tempRT, ClearFlag.Color, Color.black);
            cmd.SetViewport(new Rect(0, 0, viewportWidth, viewportHeight));
            cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);

            foreach (var renderer in cachedRenderers)
            {
                if (renderer != null && renderer.gameObject.activeInHierarchy &&
                    ((outlineLayer & (1 << renderer.gameObject.layer)) != 0))
                {
                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    {
                        cmd.DrawRenderer(renderer, maskMaterial, i);
                    }
                }
            }

            outlineMaterial.SetColor("_OutlineColor", outlineColor);
            outlineMaterial.SetFloat("_Thickness", thickness);
            outlineMaterial.SetTexture("_ObjectMask", tempRT);
            outlineMaterial.SetVector("_ViewportParams", new Vector4(viewportWidth, viewportHeight, 1.0f / viewportWidth, 1.0f / viewportHeight));

            CoreUtils.SetRenderTarget(cmd, ctx.cameraColorBuffer);
            cmd.SetViewport(viewport);
            HDUtils.DrawFullScreen(cmd, outlineMaterial, ctx.cameraColorBuffer);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"OutlineEffect Execute Error: {e.Message}\n{e.StackTrace}");
        }
    }

    protected override void Cleanup()
    {
        try
        {
            Debug.Log("OutlineEffect: Cleanup started");

            if (tempRT != null)
            {
                RTHandles.Release(tempRT);
                tempRT = null;
            }

            CoreUtils.Destroy(outlineMaterial);
            CoreUtils.Destroy(maskMaterial);

            outlineMaterial = null;
            maskMaterial = null;
            cachedRenderers = null;
            renderersValid = false;

            Debug.Log("OutlineEffect: Cleanup complete");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"OutlineEffect Cleanup Error: {e.Message}\n{e.StackTrace}");
        }
    }

    public void SetTargetObject(GameObject obj)
    {
        try
        {
            targetObject = obj;

            if (obj != null)
            {
                cachedRenderers = obj.GetComponentsInChildren<Renderer>();
                renderersValid = cachedRenderers != null && cachedRenderers.Length > 0;

                if (!renderersValid)
                {
                    Debug.LogWarning($"OutlineEffect: No renderers found on target object {obj.name}");
                }
                else
                {
                    Debug.Log($"OutlineEffect: Cached {cachedRenderers.Length} renderers for {obj.name}");
                }
            }
            else
            {
                cachedRenderers = null;
                renderersValid = false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"OutlineEffect SetTargetObject Error: {e.Message}\n{e.StackTrace}");
        }
    }

    public void ClearTarget()
    {
        targetObject = null;
        cachedRenderers = null;
        renderersValid = false;
    }
}