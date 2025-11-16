using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePlayer : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 40f;
    public float lifetime = 5f;
    [Min(1)]
    public int damage = 1;          // how much damage this projectile does
    public Transform platform;      // assign your platform in inspector!

    private Rigidbody rb;
    private Bounds platformBounds;

    void Start()
    {
        Debug.Log("ProjectilePlayer started at " + transform.position);
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;

        // Auto-destroy after lifetime as a backup
        Destroy(gameObject, lifetime);

        // Find the platform automatically if not assigned
        if (platform == null)
        {
            GameObject platformObj = GameObject.FindWithTag("Platform");
            if (platformObj != null)
                platform = platformObj.transform;
            else
                Debug.LogWarning("ProjectilePlayer: No object with tag 'Platform' found in scene!");
        }

        // Cache platform bounds
        if (platform != null)
        {
            Renderer rend = platform.GetComponent<Renderer>();
            if (rend != null)
                platformBounds = rend.bounds;
        }
    }

    void Update()
    {
        // ignore y-axis when checking bounds, since projectile fly above the platform
        if (platform != null)
        {
            Vector3 flatPos = transform.position;
            Vector3 flatMin = platformBounds.min;
            Vector3 flatMax = platformBounds.max;

            flatPos.y = Mathf.Clamp(flatPos.y, flatMin.y, flatMax.y);

            if (!platformBounds.Contains(flatPos))
            {
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);   // apply damage
            }

            Destroy(gameObject);            // bullet disappears on hit
        }
        else if (!other.CompareTag("Player") && !other.CompareTag("Platform"))
        {
            Destroy(gameObject);
        }
    }
}
