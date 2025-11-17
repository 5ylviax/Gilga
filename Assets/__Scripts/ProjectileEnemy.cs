using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEnemy : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 20f;
    public float lifetime = 5f;
    public int damage = 1;
    public bool destroyableByPlayer = true;  // red: true, black: false
    public Transform platform;               // assign in inspector or auto-find

    private Rigidbody rb;
    private Bounds platformBounds;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.velocity = transform.forward * speed;

        // backup auto-destroy
        Destroy(gameObject, lifetime);

        // Find platform if not assigned
        if (platform == null)
        {
            GameObject platformObj = GameObject.FindWithTag("Platform");
            if (platformObj != null)
                platform = platformObj.transform;
        }

        if (platform != null)
        {
            Renderer rend = platform.GetComponent<Renderer>();
            if (rend != null)
                platformBounds = rend.bounds;
        }
    }

    void Update()
    {
        // Despawn when leaving map (similar to player projectile)
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
        // Hit player: deal 1 damage and disappear
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        // Future cover: enemy bullets disappear on cover
        else if (other.CompareTag("Cover"))
        {
            Destroy(gameObject);
        }
        // Ignore enemies / platform here (handled by bounds)
    }
}
