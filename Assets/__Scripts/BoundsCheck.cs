using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsCheck : MonoBehaviour
{
    [Header("Dynamic")]
    public Transform platform;
    public float xMin;
    public float xMax;
    public float zMin;
    public float zMax; 

    void Awake()
    {
        Renderer rend = platform.GetComponent<Renderer>();
        Bounds bounds = rend.bounds;

        xMin = bounds.min.x;
        xMax = bounds.max.x;
        zMin = bounds.min.z;
        zMax = bounds.max.z;
    }
    void LateUpdate()
    {
        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, xMin, xMax);
        pos.z = Mathf.Clamp(pos.z, zMin, zMax);

        transform.position = pos;
    }
    void OnDrawGizmosSelected()
    {
        if (platform == null) return;
        Renderer rend = platform.GetComponent<Renderer>();
        Bounds b = rend.bounds;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(b.center, b.size);
    }
}
