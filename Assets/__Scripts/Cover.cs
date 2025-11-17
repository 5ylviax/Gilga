using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cover : MonoBehaviour
{
    void Reset()
    {
        // Auto-setup when you first add this script

        // Make sure this object is tagged correctly
        gameObject.tag = "Cover";

        // Ensure there is a solid collider
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null) col = gameObject.AddComponent<BoxCollider>();
        col.isTrigger = false;   // solid – player/enemy cannot pass through
    }
}
