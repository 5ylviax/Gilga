using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [Range(0f, 30f)]
    public float speed = 15f;           // overall movement speed
    public float bounceOffset = 0.2f;  // how far inside the bounds to bounce
    public bool debugBounds = false;

    private Vector3 moveDir;           // current direction of motion
    private BoundsCheck bndCheck;
    private Renderer platformRenderer;
    private float xMin, xMax, zMin, zMax;
    private bool boundsReady = false;

    void Start()
    {
        bndCheck = GetComponent<BoundsCheck>();
        SetupBounds();

        // start moving at a random diagonal angle
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;
    }

    void Update()
    {
        if (!boundsReady)
            SetupBounds();

        if (boundsReady)
            Move();
    }

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

        // Check for collisions with bounds and reflect
        if (pos.x <= xMin || pos.x >= xMax)
        {
            moveDir.x *= -1; // reflect horizontally
            pos.x = Mathf.Clamp(pos.x, xMin, xMax);
        }

        if (pos.z <= zMin || pos.z >= zMax)
        {
            moveDir.z *= -1; // reflect vertically (front/back)
            pos.z = Mathf.Clamp(pos.z, zMin, zMax);
        }

        transform.position = pos;
    }
}
