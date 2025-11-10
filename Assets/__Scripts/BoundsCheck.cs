using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsCheck : MonoBehaviour
{
    [Header("Inscribed")]
    public Transform platform;

    [Header("Dynamic")]
    private float xMin, xMax, zMin, zMax;
    private float playerHalfWidth, playerHalfDepth;

    void Start()
    {
        if (platform == null)
        {
            Debug.LogError("BoundsCheck: Platform not assigned!");
            enabled = false;
            return;
        }

        // Get platform bounds
        Renderer platRend = platform.GetComponent<Renderer>();
        if (platRend == null)
        {
            Debug.LogError("BoundsCheck: Platform has no Renderer!");
            enabled = false;
            return;
        }
        Bounds platBounds = platRend.bounds;

        // player’s Renderer from children instead
        Renderer playerRend = GetComponentInChildren<Renderer>();
        if (playerRend == null)
        {
            Debug.LogError("BoundsCheck: No Renderer found in children of " + gameObject.name);
            enabled = false;
            return;
        }

        Bounds playerBounds = playerRend.bounds;

        // Half-sizes of the player model
        playerHalfWidth = playerBounds.extents.x;
        playerHalfDepth = playerBounds.extents.z;

        // Shrink the platform bounds by player size
        xMin = platBounds.min.x + playerHalfWidth;
        xMax = platBounds.max.x - playerHalfWidth;
        zMin = platBounds.min.z + playerHalfDepth;
        zMax = platBounds.max.z - playerHalfDepth;
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;

        // Clamp edges instead of center
        pos.x = Mathf.Clamp(pos.x, xMin, xMax);
        pos.z = Mathf.Clamp(pos.z, zMin, zMax);

        transform.position = pos;
    }

    void OnDrawGizmosSelected()
    {
        if (platform == null) return;
        Renderer rend = platform.GetComponent<Renderer>();
        if (rend == null) return;

        Bounds b = rend.bounds;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(b.center, b.size);
    }
}
