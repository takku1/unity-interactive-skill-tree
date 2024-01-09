using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Ensure this namespace is included

public class DoorInteraction : MonoBehaviour
{
    public Room room;
    public int wallIndexToDisable;
    public int doorIndex;
    public PlayerInventory playerInventory;
    public TextMeshPro gemCounterText; // Reference to the TextMeshPro component

    [SerializeField]
    public int totalRequiredGems;
    [SerializeField]
    private int depositedGems = 0;
    [SerializeField]
    private PlayerInventory.GemType? requiredGemType = null;

    public Vector3 adjacentRoomPosition;
    private bool isPlayerNearby = false; // Flag to check if player is nearby
    private bool isUnlocked = false; // Track if the door is unlocked
    private bool isAdjacentRoomActive = false; // Track if the adjacent room is active

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerInventory>() != null)
        {
            isPlayerNearby = true;
            other.gameObject.GetComponent<PlayerInventory>().RegisterInteractableDoor(this);
            Debug.Log($"Player entered door collider: {gameObject.name}, Door Index: {doorIndex}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerInventory>() != null)
        {
            isPlayerNearby = false;  // Player has exited the collider
            other.gameObject.GetComponent<PlayerInventory>().UnregisterInteractableDoor(this);
            Debug.Log($"Player exited door collider: {gameObject.name}");
        }
    }

    private void Start()
    {
        // Check the state of the adjacent room at the start
        Room adjacentRoom = RoomManager.Instance.GetRoomByPosition(adjacentRoomPosition);
        if (adjacentRoom != null && adjacentRoom.IsActive)
        {
            isAdjacentRoomActive = true;
            UnlockDoorWithoutGems(); // Automatically unlock if adjacent room is active
        }
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"Player pressed E over door: {gameObject.name}, Door Index: {doorIndex}");

            if (depositedGems >= totalRequiredGems && !isUnlocked)
            {
                UnlockDoor();
            }
            else if (!isUnlocked)
            {
                Debug.Log("Not enough gems deposited to unlock the door.");
            }
        }
    }
    public bool DepositGems(string gemTypeString, int amount)
    {
        // Retrieve the adjacent room within this method
        Room adjacentRoom = RoomManager.Instance.GetRoomByPosition(adjacentRoomPosition);

        // Check if the door is already unlocked or the adjacent room is active
        if (isUnlocked || (adjacentRoom != null && adjacentRoom.IsActive))
        {
            Debug.Log("Cannot deposit gems: Door is already unlocked or leads to an active room.");
            return false;
        }

        Debug.Log($"Attempting to deposit {amount} of {gemTypeString} into door: {gameObject.name}");
        if (!System.Enum.TryParse(gemTypeString, out PlayerInventory.GemType gemType))
        {
            Debug.LogError("Invalid gem type: " + gemTypeString);
            return false;
        }

        if (requiredGemType == null)
        {
            requiredGemType = gemType;

            if (adjacentRoom != null)
            {
                adjacentRoom.SetGemType(gemTypeString);
                Debug.Log($"Required gem type set to {gemType} for adjacent room at {adjacentRoomPosition}.");
            }
            else
            {
                Debug.LogError($"Adjacent room not found at position: {adjacentRoomPosition}");
                return false;
            }
        }
        else if (requiredGemType != gemType)
        {
            Debug.Log($"Gem type mismatch for door: {gameObject.name}. Required: {requiredGemType}, Provided: {gemType}");
            return false;
        }

        if (depositedGems + amount <= totalRequiredGems)
        {
            depositedGems += amount;
            UpdateGemCounterText();
            Debug.Log($"Deposited {amount} of {gemType} into door: {gameObject.name}. Total now: {depositedGems}");
            return true;
        }
        else
        {
            Debug.Log($"Exceeds required gems for door: {gameObject.name}. Required: {totalRequiredGems}, Attempted deposit: {depositedGems + amount}");
            return false;
        }
    }



    private void UpdateGemCounterText()
    {
        if (depositedGems >= totalRequiredGems && !isUnlocked)
        {
            gemCounterText.text = "Press E to unlock door";
        }
        else if (!isUnlocked)
        {
            gemCounterText.text = $"Gems: {depositedGems}/{totalRequiredGems}";
        }
    }

    public void UnlockDoorWithoutGems()
    {
        if (!isUnlocked)
        {
            Debug.Log("Unlocking door without gems: " + gameObject.name);
            isUnlocked = true;

            // Deactivate the wall associated with this door
            room.DeactivateWall(wallIndexToDisable);

            // Optionally, deactivate the door itself or perform other actions
            gameObject.SetActive(false);

            if (gemCounterText != null)
            {
                gemCounterText.gameObject.SetActive(false);
            }
        }
    }

    private void UnlockDoor()
    {
        if (isUnlocked)
        {
            Debug.Log("Door already unlocked: " + gameObject.name);
            return;
        }

        if (depositedGems < totalRequiredGems)
        {
            Debug.Log("Not enough gems deposited to unlock the door.");
            return;
        }

        // Get the adjacent room
        Room adjacentRoom = RoomManager.Instance.GetRoomByPosition(adjacentRoomPosition);
        if (adjacentRoom != null)
        {
            // Update isAdjacentRoomActive based on the current state of the adjacent room
            isAdjacentRoomActive = adjacentRoom.IsActive;

            if (isAdjacentRoomActive)
            {
                Debug.LogError("Cannot unlock door: Adjacent room is already active.");
                return;
            }

            Debug.Log("Unlocking door with gems: " + gameObject.name);
            isUnlocked = true;

            // Deactivate the wall in the current room
            room.DeactivateWall(wallIndexToDisable);

            // Deactivate the corresponding wall in the adjacent room
            int correspondingWallIndex = adjacentRoom.GetCorrespondingWallIndex(doorIndex);
            adjacentRoom.DeactivateWall(correspondingWallIndex);

            // Activate the adjacent room
            adjacentRoom.SetRoomActive(true);

            // Check and spawn adjacent rooms if necessary
            RoomManager.Instance.SpawnAdjacentRoomsIfNecessary(adjacentRoomPosition);
        }
        else
        {
            Debug.LogError("Adjacent room not found at position: " + adjacentRoomPosition);
        }

        // Optionally, deactivate the door itself or perform other actions
        gameObject.SetActive(false);

        if (gemCounterText != null)
        {
            gemCounterText.gameObject.SetActive(false);
        }
    }
}
