using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public TextMeshProUGUI redGemText;
    public TextMeshProUGUI blueGemText;
    public TextMeshProUGUI greenGemText;

    public static InventoryUI Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    public static void UpdateDisplay(Dictionary<PlayerInventory.GemType, int> gems)
    {
        if (Instance == null) return;

        // Update the UI text based on the gem counts
        Instance.redGemText.text = "Red Gems: " + gems.GetValueOrDefault(PlayerInventory.GemType.RedGem, 0);
        Instance.blueGemText.text = "Blue Gems: " + gems.GetValueOrDefault(PlayerInventory.GemType.BlueGem, 0);
        Instance.greenGemText.text = "Green Gems: " + gems.GetValueOrDefault(PlayerInventory.GemType.GreenGem, 0);
    }
}
