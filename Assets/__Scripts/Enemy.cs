using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Health")]
    [Range(1, 5)]
    public int maxHealth = 3;   // set this 1–5 in Inspector
    private int currentHealth;

    [Header("Movement Settings")]
    [Range(0f, 30f)]
    public float speed = 15f;           // overall movement speed
    public float bounceOffset = 0.2f;   // how far inside the bounds to bounce
    public bool debugBounds = false;

    private Vector3 moveDir;           
    private BoundsCheck bndCheck;
    private Renderer platformRenderer;
    private float xMin, xMax, zMin, zMax;
    private bool boundsReady = false;

    [Header("Shooting")]
    public GameObject redProjectilePrefab;
    public GameObject blackProjectilePrefab;
    public float fireIntervalMin = 1.5f;
    public float fireIntervalMax = 3f;
    public float projectileSpeed = 25f;
    public float projectileSpawnHeight = 0.5f;

    private float nextShotTime;

    void Start()
    {
        currentHealth = maxHealth;

        bndCheck = GetComponent<BoundsCheck>();
        SetupBounds();

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;

        // NEW: first shot time
        nextShotTime = Time.time + Random.Range(fireIntervalMin, fireIntervalMax);
    }

    void Update()
    {
        if (!boundsReady)
            SetupBounds();

        if (boundsReady)
        {
            Move();
            TryShootAtPlayer();     // NEW
        }
    }

    // ---- NEW: called by projectiles ----
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // TODO: play VFX / SFX here if you want
        Destroy(gameObject);
    }
    // ------------------------------------

    void SetupBounds()
    {
        if (bndCheck == null || bndCheck.platform == null) return;

        platformRenderer = bndCheck.platform.GetComponent<Renderer>();
        if (platformRenderer == null) return;

        Bounds platformBounds = platformRenderer.bounds;

        Renderer rend = GetComponentInChildren<Renderer>();
        float halfW = rend != null ? rend.bounds.extents.x : 0.5f;
        float halfD = rend != null ? rend.bounds.extents.z : 0.5f;

        xMin = platformBounds.min.x + halfW + bounceOffset;
        xMax = platformBounds.max.x - halfW - bounceOffset;
        zMin = platformBounds.min.z + halfD + bounceOffset;
        zMax = platformBounds.max.z - halfD - bounceOffset;

        if (xMax > xMin && zMax > zMin)
        {
            boundsReady = true;
            if (debugBounds)
                Debug.Log($"Enemy bounds ready: X({xMin:F1},{xMax:F1}) Z({zMin:F1},{zMax:F1})");
        }
    }

    void Move()
    {
        Vector3 pos = transform.position;
        pos += moveDir * speed * Time.deltaTime;

        if (pos.x <= xMin || pos.x >= xMax)
        {
            moveDir.x *= -1;
            pos.x = Mathf.Clamp(pos.x, xMin, xMax);
        }

        if (pos.z <= zMin || pos.z >= zMax)
        {
            moveDir.z *= -1;
            pos.z = Mathf.Clamp(pos.z, zMin, zMax);
        }

        transform.position = pos;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(1);   // lose 1 HP per bump
            }
        }
    }

    void TryShootAtPlayer()
    {
        if (Time.time < nextShotTime) return;
        if (Player.S == null) return;   // no player yet

        Vector3 dir = Player.S.transform.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f) return;  // too close / invalid

        dir.Normalize();
        Quaternion rot = Quaternion.LookRotation(dir);

        // Choose red or black projectile
        bool useRed = (Random.value < 0.5f);
        GameObject prefabToUse = null;

        if (useRed && redProjectilePrefab != null)
            prefabToUse = redProjectilePrefab;
        else if (!useRed && blackProjectilePrefab != null)
            prefabToUse = blackProjectilePrefab;

        if (prefabToUse == null) return;

        Vector3 spawnPos = transform.position + dir * 1f + Vector3.up * projectileSpawnHeight;
        GameObject projGO = Instantiate(prefabToUse, spawnPos, rot);

        ProjectileEnemy proj = projGO.GetComponent<ProjectileEnemy>();
        if (proj != null)
        {
            proj.speed = projectileSpeed;
            proj.damage = 1;                   // enemy shots always do 1 dmg
            proj.destroyableByPlayer = useRed; // red = destroyable, black = not
        }

        nextShotTime = Time.time + Random.Range(fireIntervalMin, fireIntervalMax);
    }
}
