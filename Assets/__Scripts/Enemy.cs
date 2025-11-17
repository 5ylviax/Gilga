using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // ============================
    // 1. VARIABLES
    // ============================
    [Header("Health")]
    [Range(1, 5)]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Movement Settings")]
    [Range(0f, 30f)]
    public float speed = 15f;
    public float bounceOffset = 0.2f;
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
    private Rigidbody rb;

    // ============================
    // 2. START()
    // ============================
    void Start()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        bndCheck = GetComponent<BoundsCheck>();
        SetupBounds();

        // Random initial direction
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;

        nextShotTime = Time.time + Random.Range(fireIntervalMin, fireIntervalMax);
    }

    // ============================
    // 3. UPDATE()
    // ============================
    void Update()
    {
        if (!boundsReady)
            SetupBounds();

        if (boundsReady)
        {
            TryShootAtPlayer();
        }
    }

    // ============================
    // 4. HEALTH & DEATH
    // ============================
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        // tell LevelManager this enemy died
        if (LevelManager.S != null)
        {
            LevelManager.S.OnEnemyKilled(this);
        }

        Destroy(gameObject);
    }

    // ============================
    // 5. BOUND SETUP
    // ============================
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

        boundsReady = true;
    }

    // ============================
    // 6. MOVEMENT + WALL BOUNCE
    // ============================
    // ============================
// 6. MOVEMENT + WALL BOUNCE
// ============================
void MovePhysics()
{
    // Start from rigidbody position
    Vector3 pos = rb.position;

    // Use fixedDeltaTime for physics step
    pos += moveDir * speed * Time.fixedDeltaTime;

    // Bounce off platform edges
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

    // Actually move via Rigidbody so collisions work
    rb.MovePosition(pos);
}



    // ============================
    // 7. COLLISIONS  (ONLY THIS)
    // ============================
    void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.collider;

        // Player bump → damage Player
        if (other.CompareTag("Player"))
        {
            Player p = other.GetComponent<Player>();
            if (p != null)
                p.TakeDamage(1);
        }
        // Cover hard bounce
        else if (other.CompareTag("Cover"))
        {
            ContactPoint cp = collision.contacts[0];
            moveDir = Vector3.Reflect(moveDir, cp.normal);
        }
    }

    // ============================
    // 8. SHOOTING AT PLAYER
    // ============================
    void TryShootAtPlayer()
    {
        if (Time.time < nextShotTime) return;
        if (Player.S == null) return;

        Vector3 dir = Player.S.transform.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f) return;

        dir.Normalize();
        Quaternion rot = Quaternion.LookRotation(dir);

        bool useRed = (Random.value < 0.5f);
        GameObject projPrefab =
            useRed ? redProjectilePrefab : blackProjectilePrefab;

        if (projPrefab == null) return;

        Vector3 spawnPos =
            transform.position + dir * 1f + Vector3.up * projectileSpawnHeight;

        GameObject projGO = Instantiate(projPrefab, spawnPos, rot);

        ProjectileEnemy proj = projGO.GetComponent<ProjectileEnemy>();
        if (proj != null)
        {
            proj.speed = projectileSpeed;
            proj.damage = 1;
            proj.destroyableByPlayer = useRed;
        }

        nextShotTime = Time.time + Random.Range(fireIntervalMin, fireIntervalMax);
    }
    void FixedUpdate()
{
    if (!boundsReady || rb == null) return;
    MovePhysics();
}

}
