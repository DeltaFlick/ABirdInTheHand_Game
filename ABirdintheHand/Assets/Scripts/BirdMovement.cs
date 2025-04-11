using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
 
public class BirdMovement : MonoBehaviour
{
   BirdControls controls;
   Vector2 move;
   public float speed = 10;
 
   void Awake()
   {
       controls = new BirdControls();
       controls.Bird.Move.performed += ctx => 
                                      SendMessage(ctx.ReadValue<Vector2>());
       controls.Bird.Move.performed += ctx => move = 
                                      ctx.ReadValue<Vector2>();
       controls.Bird.Move.canceled += ctx => move = Vector2.zero;
   }
 
   private void OnEnable()
   {
       controls.Bird.Enable();
   }
   private void OnDisable()
   {
       controls.Bird.Disable();
   }
 
   void SendMessage(Vector2 coordinates)
   {
       Debug.Log("Thumb-stick coordinates = " + coordinates);
   }
 
   void FixedUpdate()
   {
       Vector3 movement = new Vector3(move.x, 0.0f, move.y) * speed 
                                                            * Time.deltaTime;
       transform.Translate(movement, Space.World);
   }
}

