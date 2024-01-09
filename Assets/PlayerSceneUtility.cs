using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneUtility : MonoBehaviour
{
    public static PlayerSceneUtility Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void PositionPlayerBasedOnScene()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found in scene.");
            return;
        }

        Vector3 spawnPosition = GetSpawnPositionBasedOnScene();
        player.transform.position = spawnPosition;

        // Update the camera target here instead of in RoomManager
        UpdateCameraTarget(player);
    }

    private Vector3 GetSpawnPositionBasedOnScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "RoomScene")
        {
            return RoomManager.Instance.GetSpawnPosition();
        }
        else if (currentScene == "Arena")
        {
            // Define the spawn position logic for the Arena scene
            // Example: return new Vector3(...); 
            return Vector3.zero; // Update this as needed
        }

        // Default spawn position if none of the above cases match
        return Vector3.zero;
    }

    private void UpdateCameraTarget(GameObject player)
    {
        CameraFollow cameraFollow = Camera.main?.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(player.transform);
        }
        else
        {
            Debug.LogError("CameraFollow script not found on the main camera.");
        }
    }

}