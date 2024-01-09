using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    AttackDamage, CriticalChance, Armor, MagicResistance, MovementSpeed, ResourceRegeneration, HealthBoost, ManaBoost, CooldownReduction, AttackSpeed, ArmorPenetration
}

public enum Quadrant
{
    NorthEast, EastSouth, SouthWest, NorthWest
}

public class StatBoost
{
    public StatType type;
    public float minValue;
    public float maxValue;

    public StatBoost(StatType type, float minValue, float maxValue)
    {
        this.type = type;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }

    // Generate a unique identifier for the StatBoost
    public string GetUniqueId()
    {
        return $"{type}-{minValue}-{maxValue}";
    }
}
public class StatBoostLibrary : MonoBehaviour
{
    public List<StatBoost> northEastStats; // Defense focused
    public List<StatBoost> eastSouthStats; // Magic Damage focused
    public List<StatBoost> southWestStats; // Speed focused
    public List<StatBoost> northWestStats; // Physical Damage focused
    public static StatBoostLibrary Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Make this object persistent across scenes

        InitializeStats(); // Initialize stats
    }
    void Start()
    {
        InitializeStats();
    }

    void InitializeStats()
    {
        // Defense focused stats for the NorthEast quadrant
        northEastStats = new List<StatBoost>
    {
        new StatBoost(StatType.Armor, 1.0f, 3.0f),
        new StatBoost(StatType.MagicResistance, 1.0f, 3.0f),
        new StatBoost(StatType.HealthBoost, 5.0f, 10.0f),
        new StatBoost(StatType.ResourceRegeneration, 0.5f, 1.5f),
        new StatBoost(StatType.MovementSpeed, 0.1f, 0.3f)
    };

        // Magic Damage focused stats for the EastSouth quadrant
        eastSouthStats = new List<StatBoost>
    {
        new StatBoost(StatType.ManaBoost, 2.0f, 5.0f),
        new StatBoost(StatType.CooldownReduction, 0.5f, 1.5f),
        new StatBoost(StatType.AttackSpeed, 0.1f, 0.3f),
        new StatBoost(StatType.MagicResistance, 1.0f, 2.0f),
        new StatBoost(StatType.ResourceRegeneration, 0.5f, 1.0f)
    };

        // Speed focused stats for the SouthWest quadrant
        southWestStats = new List<StatBoost>
    {
        new StatBoost(StatType.MovementSpeed, 0.1f, 0.5f),
        new StatBoost(StatType.AttackSpeed, 0.1f, 0.3f),
        new StatBoost(StatType.HealthBoost, 3.0f, 6.0f),
        new StatBoost(StatType.CooldownReduction, 0.2f, 1.0f),
        new StatBoost(StatType.Armor, 0.5f, 1.5f)
    };

        // Physical Damage focused stats for the NorthWest quadrant
        northWestStats = new List<StatBoost>
    {
        new StatBoost(StatType.AttackDamage, 1.0f, 4.0f),
        new StatBoost(StatType.CriticalChance, 0.5f, 2.0f),
        new StatBoost(StatType.ArmorPenetration, 1.0f, 3.0f),
        new StatBoost(StatType.MovementSpeed, 0.1f, 0.3f),
        new StatBoost(StatType.ManaBoost, 3.0f, 7.0f)
    };
    }

    public StatBoost GetRandomBoost(Quadrant quadrant, int ring)
    {
        List<StatBoost> selectedList = GetStatList(quadrant);
        if (selectedList != null && selectedList.Count > 0)
        {
            StatBoost chosenBoost = selectedList[Random.Range(0, selectedList.Count)];
            float rolledValue = Random.Range(chosenBoost.minValue, chosenBoost.maxValue) * ring;
            return new StatBoost(chosenBoost.type, rolledValue, rolledValue);
        }
        return null;
    }

    public StatBoost GetRandomBoostWithRoll(Quadrant quadrant, int ring, string gemType)
    {
        StatBoost boost = GetRandomBoost(quadrant, ring);
        if (boost != null)
        {
            ApplyGemModifier(boost, gemType);
        }
        return boost;
    }
    private List<StatBoost> GetStatList(Quadrant quadrant)
    {
        switch (quadrant)
        {
            case Quadrant.NorthEast: return northEastStats;
            case Quadrant.EastSouth: return eastSouthStats;
            case Quadrant.SouthWest: return southWestStats;
            case Quadrant.NorthWest: return northWestStats;
            default: return null;
        }
    }

    private void ApplyGemModifier(StatBoost boost, string gemType)
    {
        // Define gem-specific effects or modifiers
        switch (gemType)
        {
            case "RedGem":
                // Apply Red Gem effect
                break;
            case "BlueGem":
                // Apply Blue Gem effect
                break;
            case "GreenGem":
                // Apply Green Gem effect
                break;
                // Add more cases for different gem types
        }
    }
    public StatBoost GetStatBoostById(string id)
    {
        // Combine all stat lists
        var allStats = new List<StatBoost>();
        allStats.AddRange(northEastStats);
        allStats.AddRange(eastSouthStats);
        allStats.AddRange(southWestStats);
        allStats.AddRange(northWestStats);

        // Search for the matching stat boost
        foreach (var statBoost in allStats)
        {
            if (statBoost.GetUniqueId() == id)
            {
                return statBoost;
            }
        }

        return null; // Return null if not found
    }


}