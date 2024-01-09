using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArenaInitializer : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject teleportationPrefab; // Reference to the teleportation prefab
    public Vector3 teleportationPosition = new Vector3(-9, 0.5f, -9);


    private void Start()
    {
        PositionPlayerNear(teleportationPosition);
        SpawnTeleportationObject();
    }

    private void SpawnTeleportationObject()
    {
        if (teleportationPrefab != null)
        {
            Instantiate(teleportationPrefab, teleportationPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Teleportation prefab not set in ArenaInitializer");
        }
    }
    public void PositionPlayerNear(Vector3 position)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = position;
            // The CameraFollow target update is now handled by PlayerSceneUtility
        }
    }

}
