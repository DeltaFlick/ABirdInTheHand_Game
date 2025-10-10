using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <Summary>
/// 1st Person Camera holder
/// </Summary>

public class CameraHolder : MonoBehaviour
{
    public Transform cameraPosition;

    private void Update()
    {
        transform.position = cameraPosition.position;
    }
}
