using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Serializable]
    public class GemCount
    {
        public GemType gemType;
        public int count;
    }

    public enum GemType { RedGem, BlueGem, GreenGem }

    [SerializeField]
    private List<GemCount> gemCounts = new List<GemCount>();

    private Dictionary<GemType, int> gems = new Dictionary<GemType, int>();
    private List<DoorInteraction> interactableDoors = new List<DoorInteraction>(); // Added list of interactable doors
    private DoorInteraction currentInteractableDoor;

    void Start()
    {
        InitializeGems();
    }

    void Update()
    {
        UpdateCurrentInteractableDoor();
        CheckForGemDeposit();
    }

    private void InitializeGems()
    {
        gems.Clear();
        foreach (var gemCount in gemCounts)
        {
            gems[gemCount.gemType] = gemCount.count;
        }
        InventoryUI.UpdateDisplay(gems); // Update UI with initial gem counts
    }

    private void CheckForGemDeposit()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TryDepositGem(GemType.RedGem, 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TryDepositGem(GemType.BlueGem, 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TryDepositGem(GemType.GreenGem, 1);
        }
    }

    public void SetCurrentInteractableDoor(DoorInteraction door)
    {
        currentInteractableDoor = door;
        Debug.Log("Interacting with door: " + (door != null ? door.gameObject.name : "None"));
    }
    private void UpdateCurrentInteractableDoor()
    {
        float minDistance = float.MaxValue;
        DoorInteraction nearestDoor = null;

        foreach (var door in interactableDoors)
        {
            float distance = Vector3.Distance(transform.position, door.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestDoor = door;
            }
        }

        if (nearestDoor != currentInteractableDoor)
        {
            currentInteractableDoor = nearestDoor;
            Debug.Log("Current Interactable Door updated to: " + (nearestDoor != null ? nearestDoor.gameObject.name : "None"));
        }
    }

    public void RegisterInteractableDoor(DoorInteraction door)
    {
        if (!interactableDoors.Contains(door))
        {
            interactableDoors.Add(door);
            Debug.Log("Registered door: " + door.gameObject.name);
        }
    }

    public void UnregisterInteractableDoor(DoorInteraction door)
    {
        if (interactableDoors.Contains(door))
        {
            interactableDoors.Remove(door);
            if (currentInteractableDoor == door)
            {
                currentInteractableDoor = null;
            }
            Debug.Log("Unregistered door: " + door.gameObject.name);
        }
    }

    private void TryDepositGem(GemType gemType, int amount)
    {
        if (currentInteractableDoor == null)
        {
            Debug.Log("No interactable door is currently set.");
            return;
        }

        if (!gems.ContainsKey(gemType) || gems[gemType] < amount)
        {
            Debug.Log($"Insufficient {gemType} gems. Available: {gems[gemType]}, Required: {amount}.");
            return;
        }

        bool depositSuccessful = currentInteractableDoor.DepositGems(gemType.ToString(), amount);
        if (depositSuccessful)
        {
            gems[gemType] -= amount;
            InventoryUI.UpdateDisplay(gems); // Update UI after deposit
            Debug.Log($"Deposited {amount} {gemType} gem(s). Remaining: {gems[gemType]}");
        }
        else
        {
            Debug.Log($"Failed to deposit {amount} {gemType} gem(s).");
        }
    }

    void OnValidate()
    {
        InitializeGems();
    }
    public List<GemCount> GetGemCounts()
    {
        List<GemCount> currentGemCounts = new List<GemCount>();
        foreach (var gem in gems)
        {
            currentGemCounts.Add(new GemCount { gemType = gem.Key, count = gem.Value });
        }
        return currentGemCounts;
    }
    public void SetGemCounts(List<GemCount> newGemCounts)
    {
        gems.Clear();
        foreach (var gemCount in newGemCounts)
        {
            gems[gemCount.gemType] = gemCount.count;
        }
        InventoryUI.UpdateDisplay(gems); // Update UI with new gem counts
    }

}
