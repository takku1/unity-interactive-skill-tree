using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class RoomManager : MonoBehaviour
{
    public GameObject roomPrefab; // Add this line
    public GameObject playerMenuPrefab;
    public Dictionary<Vector3, Room> rooms = new Dictionary<Vector3, Room>();
    public Vector3 roomSize = new Vector3(10, 0, 10);

    public static RoomManager Instance { get; private set; }
    public GameObject teleportationPrefab;
    private GameObject teleportationObject;
    public static string roomSceneName = "RoomScene";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Vector3 initialPosition = Vector3.zero;
        if (SceneManager.GetActiveScene().name == roomSceneName)
        {
            InitializeOrRestoreRooms(initialPosition);
            HandlePlayer(initialPosition);
        }
        LoadGameState();
        Debug.Log("Save Path: " + Application.persistentDataPath);
    }

    public void InitializeOrRestoreRooms(Vector3 initialPosition)
    {
        Debug.Log("Initializing or Restoring Rooms in RoomScene");
        SaveSystem.SaveData saveData = SaveSystem.LoadGame();
        // Only spawn rooms if they do not exist
        if (rooms.Count == 0)
        {
            SpawnRoom(initialPosition, true);
            SpawnAdjacentRooms(initialPosition, true);
        }
        else
        {
            RestoreExistingRooms(saveData.roomStates); // Restore the state of existing rooms
        }
        SpawnOrReactivateTeleportationObject(initialPosition);
    }

    private void RestoreExistingRooms(SaveSystem.RoomState[] roomStates)
    {
        foreach (var roomState in roomStates)
        {
            Vector3 position = roomState.position;
            Room existingRoom;

            if (rooms.TryGetValue(position, out existingRoom))
            {
                // Reactivate existing room and update its state
                existingRoom.gameObject.SetActive(true);
                existingRoom.SetRoomActive(roomState.isActive);

                // Check if it's not the initial room before setting gem type and stat boost
                if (!IsInitialRoom(position))
                {
                    existingRoom.SetGemType(roomState.gemType);

                    StatBoost restoredStatBoost = StatBoostLibrary.Instance.GetStatBoostById(roomState.statBoostId);
                    if (restoredStatBoost != null)
                    {
                        existingRoom.AssignStatBoost(restoredStatBoost);
                    }
                }
            }
            else
            {
                // Spawn new room if it does not exist
                Room newRoom = SpawnRoom(position, roomState.isActive);

                // Check if it's not the initial room before setting gem type and stat boost
                if (!IsInitialRoom(position))
                {
                    newRoom.SetGemType(roomState.gemType);

                    StatBoost restoredStatBoost = StatBoostLibrary.Instance.GetStatBoostById(roomState.statBoostId);
                    if (restoredStatBoost != null)
                    {
                        newRoom.AssignStatBoost(restoredStatBoost);
                    }
                }

                rooms[position] = newRoom;
            }

            Debug.Log($"Loading Room: Position={position}, IsActive={roomState.isActive}, GemType={roomState.gemType}, StatBoostId={roomState.statBoostId}");
        }
    }


    private void SpawnOrReactivateTeleportationObject(Vector3 roomPosition)
    {
        if (IsInitialRoom(roomPosition))
        {
            Vector3 spawnPosition = roomPosition + new Vector3(5, 0, 5);
            if (teleportationObject == null)
            {
                teleportationObject = Instantiate(teleportationPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                teleportationObject.transform.position = spawnPosition;
                teleportationObject.SetActive(true);
            }
        }
    }
    private void HandlePlayer(Vector3 initialPosition)
    {
        GameObject player = GameObject.FindWithTag("Player");

        if (player == null)
        {
            // Instantiate or activate player if it doesn't exist
            // Example: player = Instantiate(playerPrefab, initialPosition, Quaternion.identity);
        }
        else
        {
            // Player already exists, just reposition it
            player.transform.position = initialPosition;
        }

        SetupCameraFollow(player);
    }



    public void RepositionExistingPlayer(Vector3 position)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = position;
            CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.SetTarget(player.transform);
            }
        }
    }
    public bool IsInitialRoom(Vector3 roomPosition)
    {
        // Assuming the initial room is at Vector3.zero
        return roomPosition == Vector3.zero;
    }

    private void InitializePlayerMenu(PlayerStats playerStats)
    {
        if (playerMenuPrefab != null)
        {
            // Check if the player menu already exists
            PlayerMenu existingMenu = FindObjectOfType<PlayerMenu>();
            if (existingMenu == null)
            {
                GameObject menuInstance = Instantiate(playerMenuPrefab);
                PlayerMenu playerMenu = menuInstance.GetComponent<PlayerMenu>();
                if (playerMenu != null)
                {
                    playerMenu.Initialize(playerStats);
                    playerStats.SetPlayerMenu(playerMenu);
                    DontDestroyOnLoad(menuInstance);
                }
                else
                {
                    Debug.LogError("PlayerMenu script not found on the PlayerMenu prefab.");
                }
            }
        }
        else
        {
            Debug.LogError("PlayerMenu prefab not set in RoomManager");
        }
    }

    private void SetupCameraFollow(GameObject player)
    {
        CameraFollow cameraFollow = Camera.main?.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(player.transform);
        }
        else
        {
            Debug.LogError("Main Camera with CameraFollow script not found.");
        }
    }


    public Vector3 GetSpawnPosition()
    {
        // Assuming the initial room is at Vector3.zero
        // You can modify this method to return different positions based on your game logic
        return Vector3.zero + new Vector3(0, 1, 0); // Spawn position at the initial room with some offset
    }

    private void SpawnTeleportationObject(Vector3 roomPosition)
    {
        if (IsInitialRoom(roomPosition))
        {
            Vector3 spawnPosition = roomPosition + new Vector3(5, 0, 5);
            // Check if the teleportation object already exists
            if (teleportationObject == null)
            {
                // Instantiate the teleportation object
                teleportationObject = Instantiate(teleportationPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                // Update position of the existing teleportation object
                teleportationObject.transform.position = spawnPosition;
            }
        }
    }

    public void UnlockDoorAndActivateRoom(Vector3 doorPosition, Vector3 adjacentRoomPosition, int doorIndex)
    {
        if (rooms.TryGetValue(adjacentRoomPosition, out Room adjacentRoom))
        {
            adjacentRoom.SetRoomActive(true);
            adjacentRoom.UnlockCorrespondingDoor(doorIndex);
            CheckAndSpawnAdjacentRooms(adjacentRoomPosition);
        }

        if (rooms.TryGetValue(doorPosition, out Room currentRoom))
        {
            currentRoom.UnlockDoor(doorIndex);
        }
    }

    // Make this method public so it can be accessed from other classes, like DoorInteraction
    public void SpawnAdjacentRoomsIfNecessary(Vector3 roomPosition)
    {
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        foreach (var direction in directions)
        {
            Vector3 newPosition = roomPosition + Vector3.Scale(direction, roomSize);
            if (!rooms.ContainsKey(newPosition))
            {
                SpawnRoom(newPosition);
            }
        }
    }

    private void CheckAndSpawnAdjacentRooms(Vector3 roomPosition)
    {
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        foreach (var direction in directions)
        {
            Vector3 adjacentPosition = roomPosition + Vector3.Scale(direction, roomSize);
            if (!rooms.ContainsKey(adjacentPosition))
            {
                SpawnRoom(adjacentPosition);
                Debug.Log($"Spawned new room at position {adjacentPosition} adjacent to {roomPosition}");
            }
            else
            {
                Debug.Log($"Room already exists at position {adjacentPosition} adjacent to {roomPosition}");
            }
        }
    }

    private Room SpawnRoom(Vector3 position, bool isActiveRoom = false)
    {
        // Check if a room already exists at the position
        if (rooms.TryGetValue(position, out Room existingRoom) && existingRoom != null)
        {
            // If the room already exists, just reactivate and update its active state
            existingRoom.gameObject.SetActive(true);
            existingRoom.SetRoomActive(isActiveRoom);
            SetupDoorInteractions(existingRoom.gameObject, position, isActiveRoom); // Setup door interactions again if needed
            return existingRoom; // Return the existing room
        }

        // Create a new room if it does not exist
        if (roomPrefab == null)
        {
            Debug.LogError("Room prefab not set in RoomManager");
            return null;
        }

        GameObject roomObject = Instantiate(roomPrefab, position, Quaternion.identity);
        Room newRoom = roomObject.GetComponent<Room>();

        if (newRoom == null)
        {
            Debug.LogError("Room component not found on the prefab.");
            return null;
        }

        // Setup the new room
        SetupRoom(newRoom, position, isActiveRoom);
        newRoom.SetRoomActive(isActiveRoom);
        SetupDoorInteractions(roomObject, position, isActiveRoom);

        rooms[position] = newRoom; // Add the new room to the dictionary
        return newRoom; // Return the newly created room
    }

    private void SetupRoom(Room room, Vector3 position, bool isActiveRoom)
    {
        // Assign a random stat boost to the room if it's not the initial room
        if (!isActiveRoom && StatBoostLibrary.Instance != null)
        {
            var statBoost = StatBoostLibrary.Instance.GetRandomBoost(room.GetRoomQuadrant(), room.GetRingDistance());
            room.AssignStatBoost(statBoost);
        }

        // Additional setup for the room can be placed here if needed
    }


    private void SetupDoorInteractions(GameObject roomObject, Vector3 roomPosition, bool isActiveRoom)
    {
        DoorInteraction[] doors = roomObject.GetComponentsInChildren<DoorInteraction>();
        PlayerInventory playerInventory = FindObjectOfType<PlayerInventory>();

        for (int i = 0; i < doors.Length; i++)
        {
            DoorInteraction door = doors[i];
            door.room = roomObject.GetComponent<Room>();
            door.doorIndex = i;
            door.wallIndexToDisable = i;
            Vector3 adjacentRoomPosition = roomPosition + Vector3.Scale(GetDirectionFromIndex(i), roomSize);
            door.adjacentRoomPosition = adjacentRoomPosition;

            int ringDistance = CalculateRingDistance(adjacentRoomPosition);
            int totalGemsRequired = 10 * ringDistance;
            door.totalRequiredGems = totalGemsRequired;

            if (isActiveRoom && playerInventory != null)
            {
                door.playerInventory = playerInventory;
            }

            Room adjacentRoom = GetRoomByPosition(adjacentRoomPosition);
            if (adjacentRoom != null && adjacentRoom.IsActive)
            {
                door.UnlockDoorWithoutGems();
            }
        }
    }

    private int CalculateRingDistance(Vector3 position)
    {
        Vector3 relativePosition = position - Vector3.zero;
        return Mathf.Max(Mathf.Abs((int)relativePosition.x / (int)roomSize.x), Mathf.Abs((int)relativePosition.z / (int)roomSize.z));
    }

    private void SpawnAdjacentRooms(Vector3 currentRoomPosition, bool isInitialRoom = false)
    {
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        foreach (var direction in directions)
        {
            Vector3 newPosition = currentRoomPosition + Vector3.Scale(direction, roomSize);
            if (!rooms.ContainsKey(newPosition))
            {
                if (isInitialRoom || ShouldSpawnRoom(currentRoomPosition, direction))
                {
                    Room newRoom = SpawnRoom(newPosition);
                    foreach (var wall in newRoom.invisibleWalls)
                    {
                        if (wall != null)
                        {
                            wall.SetActive(true); // Initialize the walls as locked for the new room
                        }
                    }
                }
            }
        }
    }
    private bool ShouldSpawnRoom(Vector3 currentRoomPosition, Vector3 direction)
    {
        Vector3 newPosition = currentRoomPosition + Vector3.Scale(direction, roomSize);

        // Check if there is already a room in the new position
        if (rooms.ContainsKey(newPosition))
        {
            return false;
        }

        // Additional logic can be added here for other conditions
        return true;
    }


    private Vector3 GetDirectionFromIndex(int index)
    {
        // Assuming index 0 is forward, 1 is back, 2 is right, and 3 is left
        switch (index)
        {
            case 0: return Vector3.forward;  // Door at index 0 leads to a room in the forward direction
            case 1: return Vector3.right;     // Door at index 1 leads to a room in the back direction
            case 2: return Vector3.back;    // Door at index 2 leads to a room in the right direction
            case 3: return Vector3.left;     // Door at index 3 leads to a room in the left direction
            default: return Vector3.zero;    // Error case
        }
    }
    public Room GetRoomByPosition(Vector3 position)
    {
        rooms.TryGetValue(position, out Room room);
        return room;
    }
    public IEnumerable<Room> GetAllActiveRooms()
    {
        foreach (var roomEntry in rooms)
        {
            if (roomEntry.Value.IsActive)
            {
                yield return roomEntry.Value;
            }
        }
    }
    private void RestoreRooms(SaveSystem.RoomState[] roomStates)
    {
        foreach (var roomState in roomStates)
        {
            // Check if the room already exists in the scene
            if (rooms.TryGetValue(roomState.position, out Room existingRoom))
            {
                // Update the existing room's state
                existingRoom.SetRoomActive(roomState.isActive);
                existingRoom.SetGemType(roomState.gemType);
            }
            else
            {
                // Spawn a new room if it doesn't exist
                Room newRoom = SpawnRoom(roomState.position, roomState.isActive);
                newRoom.SetGemType(roomState.gemType);
                newRoom.SetRoomActive(roomState.isActive);
            }
        }
    }

    public void SaveGame()
    {
        PlayerInventory playerInventory = FindObjectOfType<PlayerInventory>();
        PlayerStats playerStats = FindObjectOfType<PlayerStats>();
        SaveSystem.SaveGame(this, playerInventory, playerStats);
        Debug.Log("Game state saved successfully.");
    }
    public void SaveGameState()
    {
        SaveSystem.SaveGame(this, FindObjectOfType<PlayerInventory>(), FindObjectOfType<PlayerStats>());
    }
    public void LoadGame()
    {
        SaveSystem.SaveData saveData = SaveSystem.LoadGame();
        if (saveData != null)
        {
            RestoreRooms(saveData.roomStates);
            RestorePlayerState(saveData.playerInventoryData, saveData.playerStatsData);
            Debug.Log("Game state loaded successfully.");
        }
        else
        {
            Debug.LogWarning("No saved game found.");
        }
    }

    public void LoadGameState()
    {
        SaveSystem.SaveData saveData = SaveSystem.LoadGame();
        if (saveData != null)
        {
            // Restore the saved game state
            RestoreRooms(saveData.roomStates); // Pass the roomStates to the method
            RestorePlayerState(saveData.playerInventoryData, saveData.playerStatsData);
        }
        else
        {
            // If no saved state, initialize the rooms and player position
            Vector3 initialPosition = GetSpawnPosition();
            InitializeOrRestoreRooms(initialPosition);
            HandlePlayer(initialPosition);
        }
        Debug.Log("Game state (re)initialized.");
    }

    private void RestorePlayerState(SaveSystem.SaveData.PlayerInventoryData playerInventoryData, SaveSystem.SaveData.PlayerStatsData playerStatsData)
    {
        // Implement logic to restore player inventory and stats
        // Example: playerInventory.SetGemCounts(playerInventoryData.gems);
        // Example: playerStats.SetStats(playerStatsData.stats);
    }
    public IEnumerator HandleSceneTransition(string newSceneName)
    {
        SaveGame(); // Save before transitioning
        yield return StartCoroutine(LoadNewScene(newSceneName));
        if (newSceneName == RoomManager.roomSceneName)
        {
            LoadGameState(); // Load after transitioning back to RoomScene
        }
    }
    private IEnumerator LoadNewScene(string newSceneName)
    {
        // Deactivate rooms when leaving the RoomScene
        if (SceneManager.GetActiveScene().name == roomSceneName)
        {
            foreach (var roomEntry in rooms)
            {
                if (roomEntry.Value != null)
                {
                    roomEntry.Value.gameObject.SetActive(false); // Deactivate each room
                }
            }
        }

        // Begin loading the new scene
        Debug.Log($"Starting to load scene {newSceneName}");
        yield return SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Single);
        Debug.Log($"Scene {newSceneName} loaded.");

        // Reactivate rooms when returning to the RoomScene
        if (newSceneName == roomSceneName)
        {
            foreach (var roomEntry in rooms)
            {
                if (roomEntry.Value != null)
                {
                    roomEntry.Value.gameObject.SetActive(true); // Reactivate each room
                }
            }

            // Restoring the game state for the RoomScene
            Debug.Log($"Loading game state for {newSceneName}");
            LoadGameState();
        }

        // Additional logic after the new scene is loaded
        // You can add any other scene-specific initialization here
    }

    public void OnSceneLoaded(string loadedSceneName)
    {
        if (loadedSceneName == roomSceneName)
        {
            // Restore the game state when returning to the RoomScene
            LoadGameState();

            // Reposition the player at the initial position in the RoomScene
            Vector3 initialPosition = GetSpawnPosition();
            RepositionExistingPlayer(initialPosition);

            // Reactivate or spawn the teleportation object in the RoomScene
            SpawnOrReactivateTeleportationObject(initialPosition);

            // Additional logic to reinitialize the RoomScene environment
            // This could include restoring UI elements, player stats, inventory, etc.
            // Ensure all necessary components are reinitialized for a seamless transition

            // Example: Reinitialize Player's UI
            if (playerMenuPrefab != null)
            {
                InitializePlayerMenu(FindObjectOfType<PlayerStats>());
            }

            // Reactivate or reset other scene-specific elements if necessary
            // Example: Reactivating enemies, resetting puzzles, etc.
        }
        else
        {
            // Handle the loaded scene if it's not RoomScene, such as the Arena
            // Perform necessary actions based on the loaded scene

            // Example: Setup the player and environment for the Arena scene
            // InitializeArenaScene();
        }
    }
}