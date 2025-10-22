using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class ShaderSwapper : MonoBehaviour
{
    private GameObject currentTargetObject;
    private CustomPassVolume assignedVolume;
    private PlayerCustomPass playerCustomPass;

    public int playerID = 1;

    public void SetCustomPassVolume(CustomPassVolume volume)
    {
        assignedVolume = volume;
        
        if (volume != null && volume.customPasses.Count > 0)
        {
            playerCustomPass = volume.customPasses[0] as PlayerCustomPass;
        }
    }

    public void RevertShader()
    {
        if (playerCustomPass != null)
        {
            playerCustomPass.ClearTarget();
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

        if (playerCustomPass != null && targetObject != null)
        {
            Debug.Log($"Player {playerID} applying shader to {targetObject.name}");
            playerCustomPass.SetTargetObject(targetObject);
        }
        else if (playerCustomPass != null && targetObject == null)
        {
            Debug.Log($"Player {playerID} clearing shader target");
            playerCustomPass.ClearTarget();
        }
        else if (playerCustomPass == null)
        {
            Debug.LogWarning($"Player {playerID} ShaderSwapper: PlayerCustomPass is null!");
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