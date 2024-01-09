using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string savePath = Path.Combine(Application.persistentDataPath, "playerHomeBase.save");

    [Serializable]
    public class RoomState
    {
        public Vector3 position;
        public bool isActive;
        public string gemType;
        public string statBoostId; // Unique identifier for the stat boost

        public RoomState(Vector3 position, bool isActive, string gemType, string statBoostId)
        {
            this.position = position;
            this.isActive = isActive;
            this.gemType = gemType;
            this.statBoostId = statBoostId;
        }
    }

    [Serializable]
    public class SaveData
    {
        public RoomState[] roomStates;
        public PlayerInventoryData playerInventoryData;
        public PlayerStatsData playerStatsData;

        [Serializable]
        public class PlayerInventoryData
        {
            public List<PlayerInventory.GemCount> gems;

            public PlayerInventoryData(PlayerInventory inventory)
            {
                gems = inventory.GetGemCounts();
            }
        }

        [Serializable]
        public class PlayerStatsData
        {
            public Dictionary<StatType, float> stats;

            public PlayerStatsData(PlayerStats stats)
            {
                this.stats = new Dictionary<StatType, float>(stats.currentStats);
            }
        }
    }

    public static void SaveGame(RoomManager roomManager, PlayerInventory playerInventory, PlayerStats playerStats)
    {
        var roomStates = new List<RoomState>();
        foreach (var roomEntry in roomManager.rooms)
        {
            Room room = roomEntry.Value;
            string gemType = room.GetGemType();
            string statBoostId = room.GetCurrentStatBoost()?.GetUniqueId();
            roomStates.Add(new RoomState(roomEntry.Key, room.IsActive, gemType, statBoostId));
            Debug.Log($"Saving Room: Position={roomEntry.Key}, IsActive={room.IsActive}, GemType={gemType}, StatBoostId={statBoostId}");
        }

        SaveData saveData = new SaveData
        {
            roomStates = roomStates.ToArray(),
            playerInventoryData = new SaveData.PlayerInventoryData(playerInventory),
            playerStatsData = new SaveData.PlayerStatsData(playerStats)
        };

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
    }

    public static SaveData LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return null;
    }
}
