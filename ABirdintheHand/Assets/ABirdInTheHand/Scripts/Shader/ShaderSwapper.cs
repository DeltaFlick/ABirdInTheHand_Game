using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class ShaderSwapper : MonoBehaviour
{
    private GameObject currentTargetObject;
    private CustomPassVolume assignedVolume;
    private OutlineEffect outlineEffect;

    public int playerID = 1;

    public void SetCustomPassVolume(CustomPassVolume volume)
    {
        assignedVolume = volume;
        
        if (volume != null && volume.customPasses.Count > 0)
        {
            outlineEffect = volume.customPasses[0] as OutlineEffect;
        }
    }

    public void RevertShader()
    {
        if (outlineEffect != null)
        {
            outlineEffect.ClearTarget();
        }
        
        currentTargetObject = null;
    }

    public void ChangeShader(GameObject targetObject, Camera targetCamera = null)
    {
        if (currentTargetObject != targetObject)
        {
            if (currentTargetObject != null)
            {
                RevertShader();
            }

            currentTargetObject = targetObject;
        }

        if (outlineEffect != null && targetObject != null)
        {
            Debug.Log($"Player {playerID} applying shader to {targetObject.name}");
            outlineEffect.SetTargetObject(targetObject);
        }
        else if (outlineEffect != null && targetObject == null)
        {
            Debug.Log($"Player {playerID} clearing shader target");
            outlineEffect.ClearTarget();
        }
        else if (outlineEffect == null)
        {
            Debug.LogWarning($"Player {playerID} ShaderSwapper: OutlineEffect is null!");
        }
    }

    public void ResetVisualReference(GameObject newVisual)
    {
        if (newVisual != null)
            currentTargetObject = newVisual;
    }

    public void RemoveCamera(Camera targetCamera)
    {
        //deprecated
        RevertShader();
    }
}