using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Target to follow (player)
    public Vector3 offset; // Offset from the target, set this to something like new Vector3(0, 12, -12)

    void Awake()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.gameObject != gameObject)
        {
            Destroy(mainCamera.gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        Debug.Log("Camera target updated.");
    }

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;

        }
        else
        {
            Debug.LogError("Target not assigned to CameraFollow script");
        }
    }
}