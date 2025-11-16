using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class ShaderSwapper : MonoBehaviour
{
    private GameObject currentTargetObject;
    private CustomPassVolume assignedVolume;
    private OutlineEffect outlineEffect;
    public int playerID = 1;
    private bool isShaderActive = false;

    public void SetCustomPassVolume(CustomPassVolume volume)
    {
        try
        {
            Debug.Log($"ShaderSwapper Player {playerID}: Setting CustomPassVolume");

            assignedVolume = volume;

            if (volume == null)
            {
                Debug.LogError($"ShaderSwapper Player {playerID}: Volume is null");
                return;
            }

            if (volume.customPasses == null)
            {
                Debug.LogError($"ShaderSwapper Player {playerID}: Volume.customPasses is null");
                return;
            }

            if (volume.customPasses.Count == 0)
            {
                Debug.LogError($"ShaderSwapper Player {playerID}: Volume.customPasses is empty");
                return;
            }

            outlineEffect = volume.customPasses[0] as OutlineEffect;

            if (outlineEffect == null)
            {
                Debug.LogError($"ShaderSwapper Player {playerID}: First custom pass is not an OutlineEffect. Type: {volume.customPasses[0]?.GetType().Name ?? "null"}");
                return;
            }

            Debug.Log($"ShaderSwapper Player {playerID}: OutlineEffect assigned successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ShaderSwapper Player {playerID} SetCustomPassVolume Error: {e.Message}\n{e.StackTrace}");
        }
    }

    public void RevertShader()
    {
        try
        {
            if (!isShaderActive)
                return;

            if (outlineEffect != null)
            {
                outlineEffect.ClearTarget();
            }

            currentTargetObject = null;
            isShaderActive = false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ShaderSwapper Player {playerID} RevertShader Error: {e.Message}\n{e.StackTrace}");
        }
    }

    public void ChangeShader(GameObject targetObject, Camera targetCamera = null)
    {
        try
        {
            if (currentTargetObject == targetObject && isShaderActive && targetObject != null)
            {
                return;
            }

            if (currentTargetObject != targetObject)
            {
                if (currentTargetObject != null && isShaderActive)
                {
                    RevertShader();
                }
                currentTargetObject = targetObject;
            }

            if (outlineEffect != null && targetObject != null)
            {
                if (!isShaderActive)
                {
                    Debug.Log($"Player {playerID} applying shader to {targetObject.name}");
                }
                outlineEffect.SetTargetObject(targetObject);
                isShaderActive = true;
            }
            else if (outlineEffect != null && targetObject == null)
            {
                outlineEffect.ClearTarget();
                isShaderActive = false;
            }
            else if (outlineEffect == null)
            {
                Debug.LogWarning($"Player {playerID} ShaderSwapper: OutlineEffect is null!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ShaderSwapper Player {playerID} ChangeShader Error: {e.Message}\n{e.StackTrace}");
        }
    }

    public void ResetVisualReference(GameObject newVisual)
    {
        if (newVisual != null)
        {
            currentTargetObject = newVisual;
        }
    }

    public void RemoveCamera(Camera targetCamera)
    {
        RevertShader();
    }
}