using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    static public Player S { get; private set; } // Singleton property 
    [Header("Inscribed")]

    // Controls the movement of the Player
    public float speed = 25;
    
    [Header("Dynamic")]
    [Range(0, 2)]
    public float shieldLevel = 1;
    
    void Update()
    {
        //PUll info from Input class 
        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");
        float mouseX = Input.GetAxis("Mouse X");

        // Change tranform.position based on the axes 
        Vector3 pos = transform.position;
        pos.x += hAxis * speed * Time.deltaTime;
        pos.z += vAxis * speed * Time.deltaTime;
        transform.position = pos;
        // Fix hardpoint aiming position whre the character should rotate to face toward the 
        //in order to aim 
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            Vector3 lookPos = hit.point;
            lookPos.y = hardpoint.position.y;
            hardpoint.LookAt(lookPos);
        }
    }
}
