using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Transform target;            // Reference to _Player or Player
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 15f, -15f);

    void LateUpdate()
    {
        if (!target) return;
        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
    }
}
