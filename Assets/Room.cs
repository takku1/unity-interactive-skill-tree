using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Room : MonoBehaviour
{
    public GameObject[] invisibleWalls;
    public Renderer floorRenderer;
    public Color defaultLockedColor = Color.gray;
    public Color unlockedColor = Color.white;
    private DoorInteraction[] doors;
    private Dictionary<string, Color> gemTypeToColor = new Dictionary<string, Color>()
    {
        {"RedGem", Color.red},
        {"BlueGem", Color.blue},
        {"GreenGem", Color.green}
        // Add more gem types and their corresponding colors if needed
    };
    private Room[] adjacentRooms = new Room[4]; // 0: forward, 1: back, 2: right, 3: left
    public bool IsActive { get; private set; }
    private string gemType;
    private StatBoost currentStatBoost;
    private Quadrant roomQuadrant;
    private int ringDistanceFromCenter;
    public TextMeshPro roomStatText;
    private string statBoostId;

    void Start()
    {
        if (floorRenderer == null)
        {
            Debug.LogError("Floor Renderer not assigned in Room: " + gameObject.name);
        }

        if (invisibleWalls == null || invisibleWalls.Length == 0)
        {
            Debug.LogError("Invisible walls not assigned or empty in Room: " + gameObject.name);
        }

        doors = GetComponentsInChildren<DoorInteraction>();
        DetermineQuadrantAndRing();

        // Assign a random stat boost if it's not the initial room

        if (!RoomManager.Instance.IsInitialRoom(transform.position))
        {
            if (!string.IsNullOrEmpty(statBoostId))
            {
                RestoreStatBoost(statBoostId);
            }

            // Only assign a random stat boost if it's not the initial room
            if (string.IsNullOrEmpty(gemType))
            {
                StatBoost randomStatBoost = StatBoostLibrary.Instance.GetRandomBoost(roomQuadrant, ringDistanceFromCenter);
                AssignStatBoost(randomStatBoost);
            }
        }

        SetRoomActive(IsActive);
    }

    // Method to activate or deactivate the room
    public void SetRoomActive(bool state)
    {
        IsActive = state;
        UpdateFloorColor();

        if (invisibleWalls != null)
        {
            foreach (var wall in invisibleWalls)
            {
                if (wall != null)
                {
                    wall.SetActive(!IsActive);
                }
            }
        }

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.RecalculateTotalStats(); // Recalculate player stats whenever room state changes
        }

        if (!IsActive && !RoomManager.Instance.IsInitialRoom(transform.position))
        {
            // Reset gem type for non-initial inactive rooms
            gemType = null;
        }
    }

    private void UpdateFloorColor()
    {
        if (floorRenderer == null || RoomManager.Instance.IsInitialRoom(transform.position)) return;

        if (IsActive)
        {
            floorRenderer.material.color = !string.IsNullOrEmpty(gemType) ? gemTypeToColor[gemType] : unlockedColor;
        }
        else
        {
            floorRenderer.material.color = !string.IsNullOrEmpty(gemType) ? Color.Lerp(gemTypeToColor[gemType], defaultLockedColor, 0.5f) : defaultLockedColor;
        }
    }

    // Call this method when a gem is deposited in the room's door
    public void SetGemType(string type)
    {
        if (gemType == null)
        {
            gemType = type;
            UpdateFloorColor();
        }
    }


    // Method to deactivate a specific wall
    public void DeactivateWall(int wallIndex)
    {
        if (wallIndex >= 0 && wallIndex < invisibleWalls.Length)
        {
            invisibleWalls[wallIndex].SetActive(false);
        }
    }

    // Method to activate a specific wall
    public void ActivateWall(int wallIndex)
    {
        if (wallIndex >= 0 && wallIndex < invisibleWalls.Length)
        {
            invisibleWalls[wallIndex].SetActive(true);
        }
    }


    // Method to set an adjacent room reference
    public void SetAdjacentRoom(int index, Room room)
    {
        if (index >= 0 && index < adjacentRooms.Length)
        {
            adjacentRooms[index] = room;
        }
    }


    // Method to get an adjacent room reference
    public Room GetAdjacentRoom(int index)
    {
        return index >= 0 && index < adjacentRooms.Length ? adjacentRooms[index] : null;
    }

    // Method to get the corresponding wall index in an adjacent room
    public int GetCorrespondingWallIndex(int doorIndex)
    {
        return (doorIndex + 2) % 4;
    }

    public void UnlockCorrespondingDoor(int doorIndex)
    {
        int correspondingIndex = GetCorrespondingWallIndex(doorIndex);
        if (correspondingIndex >= 0 && correspondingIndex < doors.Length)
        {
            doors[correspondingIndex].UnlockDoorWithoutGems();
        }
    }
    public void UnlockDoor(int doorIndex)
    {
        if (doorIndex >= 0 && doorIndex < doors.Length)
        {
            doors[doorIndex].UnlockDoorWithoutGems();

            // Log and apply stat boost
            Debug.Log($"Unlocked door at index {doorIndex}. Applying stat boost.");
            ApplyStatBoost();
        }
    }

    // Method to get the color associated with a gem type
    public Color GetGemColor(string gemType)
    {
        if (gemTypeToColor.TryGetValue(gemType, out Color color))
        {
            return color;
        }
        else
        {
            Debug.LogError("Gem type not found: " + gemType);
            return Color.white;
        }
    }

    public void DetermineQuadrantAndRing()
    {
        Vector3 position = transform.position;
        roomQuadrant = CalculateQuadrant(position);
        ringDistanceFromCenter = CalculateRingDistance(position);
    }

    private Quadrant CalculateQuadrant(Vector3 position)
    {
        if (position.x >= 0 && position.z >= 0) return Quadrant.NorthEast;
        if (position.x < 0 && position.z >= 0) return Quadrant.NorthWest;
        if (position.x >= 0 && position.z < 0) return Quadrant.EastSouth;
        return Quadrant.SouthWest;
    }

    private int CalculateRingDistance(Vector3 position)
    {
        Vector3 relativePosition = position - Vector3.zero; // Assuming initial room is at Vector3.zero
        return Mathf.Max(Mathf.Abs((int)relativePosition.x / (int)RoomManager.Instance.roomSize.x), Mathf.Abs((int)relativePosition.z / (int)RoomManager.Instance.roomSize.z));
    }

    public void UnlockWithGem(string gemType)
    {
        if (currentStatBoost != null)
        {
            currentStatBoost = StatBoostLibrary.Instance.GetRandomBoostWithRoll(roomQuadrant, ringDistanceFromCenter, gemType);
        }
        SetRoomActive(true);
        ApplyStatBoost();
    }

    private void ApplyStatBoost()
    {
        if (currentStatBoost != null)
        {
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.AddStatBoost(currentStatBoost);
                Debug.Log($"Applied {currentStatBoost.type} boost of {currentStatBoost.minValue} to player.");

                // Trigger UI update
                PlayerMenu playerMenu = FindObjectOfType<PlayerMenu>();
                if (playerMenu != null)
                {
                    playerMenu.ForceUpdateUI();
                }
            }
            else
            {
                Debug.LogError("PlayerStats not found.");
            }
        }
        else
        {
            Debug.Log("No stat boost to apply.");
        }
    }


    public void AssignStatBoost(StatBoost statBoost)
    {
        currentStatBoost = statBoost;
        if (roomStatText != null && statBoost != null)
        {
            roomStatText.text = $"{statBoost.type}: {statBoost.minValue} - {statBoost.maxValue}";
        }
        else if (roomStatText != null)
        {
            roomStatText.text = "No Boost";
        }
    }
    public void RestoreStatBoost(string savedStatBoostId)
    {
        StatBoost restoredStatBoost = StatBoostLibrary.Instance.GetStatBoostById(savedStatBoostId);

        if (restoredStatBoost != null)
        {
            currentStatBoost = restoredStatBoost;
            UpdateStatBoostText();
        }
        else
        {
            // Log an error message indicating the stat boost was not found.
            // This should not happen under normal circumstances if the game state is saved correctly.
            Debug.LogError($"StatBoost with ID {savedStatBoostId} not found. This indicates a potential issue in the game state saving or loading process.");
        }
    }
    private void UpdateStatBoostText()
    {
        if (roomStatText != null && currentStatBoost != null)
        {
            roomStatText.text = $"{currentStatBoost.type}: {currentStatBoost.minValue} - {currentStatBoost.maxValue}";
        }
        else if (roomStatText != null)
        {
            roomStatText.text = "No Boost";
        }
    }
    public Quadrant GetRoomQuadrant()
    {
        return roomQuadrant;
    }

    public int GetRingDistance()
    {
        return ringDistanceFromCenter;
    }

    public StatBoost GetCurrentStatBoost()
    {
        return currentStatBoost;
    }
    // Public method or property to expose gemType
    public string GetGemType()
    {
        return gemType;
    }
    // Additional methods for room-specific functionality can be added here
}
