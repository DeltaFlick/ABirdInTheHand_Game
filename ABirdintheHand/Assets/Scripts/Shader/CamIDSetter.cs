using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Rendering;

public class CamIDSetter : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private int cameraIDExposed;
    private int cameraID;
    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CamIDSetter requires a Camera component on the same GameObject.");
            enabled = false;

        }
                cameraID = cam.GetInstanceID();
                cameraIDExposed = cameraID;
    }

  private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == cam)
        {
            Shader.SetGlobalInt("_CurrentCameraID", cameraID);
        }
    }
}