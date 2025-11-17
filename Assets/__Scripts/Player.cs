using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;   // <-- NEW

public class Player : MonoBehaviour
{
    public static Player S { get; private set; }

    [Header("Inscribed")]
    public float speed = 15;
    public LayerMask groundMask;
    public Camera cam;
    public Transform hardpoint;
    public GameObject projectilePrefab;
    public float fireRate = 0.25f;
    public float projectileSpeed = 40f;
    float rotationOffsetY = 180f;

    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Dynamic")]
    [Range(0, 2)]
    public float shieldLevel = 1;
    private float nextFireTime = 0f;

    // NEW: track if the player is dead
    private bool isDead = false;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Update()
    {
        if (isDead) return;
        HandleMovement();
        HandleRotation();
        HandleShooting();
    }

        void FixedUpdate()
    {
        if (isDead)
        {
            // stop sliding after death
            if (rb != null) rb.velocity = Vector3.zero;
            return;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"Player took {amount} damage. HP = {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
    if (isDead) return;
    isDead = true;

    Debug.Log("Player died");

    // Stop movement immediately
    Rigidbody rb = GetComponent<Rigidbody>();
    if (rb != null) rb.velocity = Vector3.zero;

    // Start the reload coroutine (needs the object to stay active)
    StartCoroutine(ReloadSceneAfterDelay(1.5f));

    // HIDE the player visually
    foreach (Renderer r in GetComponentsInChildren<Renderer>())
    {
        r.enabled = false;
    }

    // Disable colliders so you can't interact anymore
    foreach (Collider c in GetComponentsInChildren<Collider>())
    {
        c.enabled = false;
    }
    }



    IEnumerator ReloadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    void HandleMovement()
    {
        float hAxis = Input.GetAxisRaw("Horizontal");
        float vAxis = Input.GetAxisRaw("Vertical");

        Vector3 pos = transform.position;

        pos.x += hAxis * speed * Time.deltaTime;
        pos.z += vAxis * speed * Time.deltaTime;

        transform.position = pos;
    }


    void HandleRotation()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            Vector3 desiredDirection = hit.point - transform.position;
            desiredDirection.y = 0;

            if (desiredDirection.sqrMagnitude > 0.001f)
            {
                // Keep 'up' strictly vertical so we don't get any tilt
                Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
                targetRotation *= Quaternion.Euler(0f, rotationOffsetY, 0f);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * 10f
                );
            }

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
                Vector3 spawnPos = hardpoint.position + hardpoint.forward * 1f;
                Debug.Log("Shooting projectile...");
                GameObject proj = Instantiate(projectilePrefab, spawnPos, hardpoint.rotation);

                Collider projCol = proj.GetComponent<Collider>();
                Collider playerCol = GetComponent<Collider>();
                if (projCol != null && playerCol != null)
                    Physics.IgnoreCollision(projCol, playerCol);

                Rigidbody rb = proj.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.velocity = hardpoint.forward * projectileSpeed;
            }
        }
    }
}
