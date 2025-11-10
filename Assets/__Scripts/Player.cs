using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    
    public static Player S { get; private set; }

    [Header("Inscribed")]
    public float speed = 15;
    public LayerMask groundMask;   // Assign "Ground" layer in Inspector
    public Camera cam;             // Assign Main Camera
    public Transform hardpoint;    // Assign the aiming pivot point (child)
    public GameObject projectilePrefab;   // ← assign your ProjectilePlayer prefab here
    public float fireRate = 0.25f;        // bullets per second
    public float projectileSpeed = 40f;
    float rotationOffsetY = 180f;

    [Header("Dynamic")]
    [Range(0, 2)]
    public float shieldLevel = 1;
    private float nextFireTime = 0f;

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleShooting();
    }

    void HandleMovement()
    {
        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");

        // Move the player using input axes
        Vector3 pos = transform.position;
        pos.x += hAxis * speed * Time.deltaTime;
        pos.z += vAxis * speed * Time.deltaTime;
        transform.position = pos;
    }

    void HandleRotation()
    {
        // Cast a ray from the camera to the ground where the mouse points
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            Vector3 desiredDirection = hit.point - transform.position;
            desiredDirection.y = 0; // fixed (flat)

            if (desiredDirection.sqrMagnitude > 0.001f)
            {
                // Calculate and apply rotation toward that direction
                Quaternion targetRotation = Quaternion.LookRotation(desiredDirection);

                // Adjust this angle until the black face points correctly
                targetRotation *= Quaternion.Euler(0, rotationOffsetY, 0);

                // Smoothly rotate
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
            // Aim hardpoint toward hit point
            if (hardpoint != null)
            {
                Vector3 lookPos = hit.point;
                lookPos.y = hardpoint.position.y;
                hardpoint.LookAt(lookPos);
            }
        }
    }

    void HandleShooting()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;

            if (projectilePrefab != null && hardpoint != null)
            {
                // spawn a bit ahead of hardpoint
                Vector3 spawnPos = hardpoint.position + hardpoint.forward * 1f;
                Debug.Log("Shooting projectile...");
                GameObject proj = Instantiate(projectilePrefab, spawnPos, hardpoint.rotation);

                // Ignore collision with player
                Collider projCol = proj.GetComponent<Collider>();
                Collider playerCol = GetComponent<Collider>();
                if (projCol != null && playerCol != null)
                    Physics.IgnoreCollision(projCol, playerCol);

                //velocity
                Rigidbody rb = proj.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.velocity = hardpoint.forward * projectileSpeed;
                }
        }
    }
}
