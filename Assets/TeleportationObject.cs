using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportationObject : MonoBehaviour
{
    private string targetScene;
    private Collider playerCollider;

    private void Start()
    {
        // Set the target scene based on the current scene
        if (SceneManager.GetActiveScene().name == "RoomScene")
        {
            targetScene = "Arena";
        }
        else
        {
            targetScene = "RoomScene";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player enters the teleportation trigger
        if (other.gameObject.CompareTag("Player"))
        {
            playerCollider = other;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the player exits the teleportation trigger
        if (other.gameObject.CompareTag("Player"))
        {
            playerCollider = null;
        }
    }

    private void Update()
    {
        // If the player is inside the trigger and presses 'E', teleport to the target scene
        if (playerCollider != null && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(TeleportToScene());
        }
    }

    private IEnumerator TeleportToScene()
    {
        RoomManager.Instance.SaveGame(); // Save the current game state before transitioning
        yield return SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Single);

        // Call a method in RoomManager to handle post-load processing
        RoomManager.Instance.OnSceneLoaded(targetScene);
    }
}
