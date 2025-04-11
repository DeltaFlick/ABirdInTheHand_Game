using UnityEngine;
using UnityEngine.InputSystem;

public class InputDebuggerAction : MonoBehaviour
{
    public InputAction exampleAction;
    
    void OnEnable()
    {
       exampleAction.Enable();
    }
}
