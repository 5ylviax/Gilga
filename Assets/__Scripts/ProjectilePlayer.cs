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
    // 1) Damage enemy
    if (other.CompareTag("Enemy"))
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);

            if(ScoreManager.Instance != null)
                {
                 ScoreManager.Instance.AddScore(10);   
                }
        }

        Destroy(gameObject);
        return;
    }

    // 2) Destroyable enemy projectile (red)
    ProjectileEnemy enemyProj = other.GetComponent<ProjectileEnemy>();
    if (enemyProj != null && enemyProj.destroyableByPlayer)
    {
        Destroy(enemyProj.gameObject);
        Destroy(gameObject);
        return;
    }

    // 3) Bounce off Cover
    if (other.CompareTag("Cover"))
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 v = rb.velocity;

            if (Mathf.Abs(v.x) > Mathf.Abs(v.z))
                v.x = -v.x;   // horizontal wall
            else
                v.z = -v.z;   // vertical wall

            rb.velocity = v;
        }
        return;
    }

    // 4) Default: destroy on anything else (except player/platform)
    if (!other.CompareTag("Player") && !other.CompareTag("Platform"))
    {
        Destroy(gameObject);
    }
}



}
