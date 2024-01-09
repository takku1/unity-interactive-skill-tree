using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    public delegate void OnStatsChanged();
    public event OnStatsChanged onStatsChanged;

    public Dictionary<StatType, float> currentStats = new Dictionary<StatType, float>();
    private Rigidbody rb;
    public PlayerMenu playerMenu; // Reference to the PlayerMenu script

    // Initilize stats
    private float maxHealth;
    private float currentHealth;
    private float attackDamage;
    private float criticalChance;
    private float armor;
    private float magicResistance;
    private float movementSpeed;
    private float resourceRegeneration;
    private float manaBoost;
    private float cooldownReduction;
    private float attackSpeed;
    private float armorPenetration;
    private float timeSinceLastAttack = 0f;
    public float attackRange = 5f; // Define the attack range

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple instances of PlayerStats found!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitializeDefaultStats();
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        InitializeDefaultStats();
        rb = GetComponent<Rigidbody>();
        UpdateMaxHealth();

        // Optionally, find the PlayerMenu script dynamically if it's not set in the Inspector
        if (playerMenu == null)
            playerMenu = FindObjectOfType<PlayerMenu>();
    }
    void Update()
    {
        UpdateStats();
        HandleMovement();
        HandleMenuToggle();
        HandleResourceRegeneration();
        HandleAttack(); // New method for handling attacks
    }

    //handle player stats
    private void HandleMovement()
    {
        movementSpeed = GetStatValue(StatType.MovementSpeed);
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rb.MovePosition(rb.position + movement * movementSpeed * Time.deltaTime);
    }
    private void HandleResourceRegeneration()
    {
        resourceRegeneration = GetStatValue(StatType.ResourceRegeneration);
        // Logic to regenerate resources (health, mana) based on resourceRegeneration
    }

    public void TakeDamage(float damage)
    {
        armor = GetStatValue(StatType.Armor);
        magicResistance = GetStatValue(StatType.MagicResistance);
        float effectiveDamage = damage - armor; // Example calculation, adjust as needed
        currentHealth -= effectiveDamage;
        CheckHealth();
    }

    private void HandleAttack()
    {

        // Cooldown between attacks based on attackSpeed
        if (timeSinceLastAttack < 1f / attackSpeed)
        {
            timeSinceLastAttack += Time.deltaTime;
            return; // Not ready to attack again
        }

        if (Input.GetMouseButtonDown(0)) // Left mouse button clicked
        {

            PerformAttack(); // Perform the attack
            // Reset timer
            timeSinceLastAttack = 0f;
        }
    }

    private void PerformAttack()
    {
        UpdateStats(); // Ensure stats are up-to-date

        // Assuming you have a method to identify the enemy
        Enemy enemy = FindTargetEnemy();
        if (enemy != null)
        {
            float finalDamage = CalculateDamage(enemy);
            enemy.TakeDamage(finalDamage); // Apply damage to the enemy
        }
    }

    private Enemy FindTargetEnemy()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        Enemy closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            Enemy enemy = hitCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }

        return closestEnemy;
    }
    private float CalculateDamage(Enemy enemy)
    {
        // Calculate damage considering armor penetration and enemy armor
        float enemyArmor = enemy.GetArmor() - armorPenetration;
        enemyArmor = Mathf.Max(enemyArmor, 0); // Armor can't be negative
        float finalDamage = attackDamage - enemyArmor;

        // Check for a critical hit
        if (Random.value < criticalChance / 100f)
        {
            finalDamage *= 2; // Double damage on a critical hit
            Debug.Log("Critical Hit!");
        }

        return finalDamage;
    }
    private void CheckHealth()
    {
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        // Handle player death (e.g., respawn, game over screen)
    }
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    // Call this when HealthBoost stat changes
    private void UpdateMaxHealth()
    {
        maxHealth = GetStatValue(StatType.HealthBoost);
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }
    private void HandleMenuToggle()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (playerMenu != null)
            {
                playerMenu.ToggleMenu();
            }
        }
    }

    private void InitializeDefaultStats()
    {
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            currentStats[type] = GetInitialValueForStatType(type);
            Debug.Log($"Initialized {type} with value {currentStats[type]}");
        }
        onStatsChanged?.Invoke(); // Notify UI update after initializing stats
    }


    public void AddStatBoost(StatBoost boost, bool updateUI = true)
    {
        if (currentStats.ContainsKey(boost.type))
        {
            currentStats[boost.type] += boost.minValue;
            Debug.Log($"Added {boost.minValue} to {boost.type}");

            if (updateUI)
            {
                // Notify UI to update
                onStatsChanged?.Invoke();
            }
        }
        else
        {
            Debug.LogError($"Stat type not found: {boost.type}");
        }
    }

    public float GetStatValue(StatType type)
    {
        if (currentStats.TryGetValue(type, out float value))
        {
            return value;
        }
        return 0f;
    }

    private float GetInitialValueForStatType(StatType type)
    {
        switch (type)
        {
            case StatType.AttackDamage: return 10f;
            case StatType.CriticalChance: return 5f;
            case StatType.Armor: return 5f;
            case StatType.MagicResistance: return 5f;
            case StatType.MovementSpeed: return 4f;
            case StatType.ResourceRegeneration: return 2f;
            case StatType.HealthBoost: return 100f;
            case StatType.ManaBoost: return 50f;
            case StatType.CooldownReduction: return 0f;
            case StatType.AttackSpeed: return 1f;
            case StatType.ArmorPenetration: return 0f;
            default: return 0f;
        }
    }

    private void UpdateStats()
    {
        // Update all stats based on current values in the dictionary
        maxHealth = GetStatValue(StatType.HealthBoost);
        attackDamage = GetStatValue(StatType.AttackDamage);
        criticalChance = GetStatValue(StatType.CriticalChance);
        armor = GetStatValue(StatType.Armor);
        magicResistance = GetStatValue(StatType.MagicResistance);
        movementSpeed = GetStatValue(StatType.MovementSpeed);
        resourceRegeneration = GetStatValue(StatType.ResourceRegeneration);
        manaBoost = GetStatValue(StatType.ManaBoost);
        cooldownReduction = GetStatValue(StatType.CooldownReduction);
        attackSpeed = GetStatValue(StatType.AttackSpeed);
        armorPenetration = GetStatValue(StatType.ArmorPenetration);
        // Ensure current health does not exceed max health
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }



    public void RecalculateTotalStats()
    {
        InitializeDefaultStats(); // Reset to base stats

        foreach (var room in RoomManager.Instance.GetAllActiveRooms())
        {
            if (room.GetCurrentStatBoost() != null)
            {
                AddStatBoost(room.GetCurrentStatBoost(), false); // Add boosts without invoking UI update
            }
        }

        onStatsChanged?.Invoke(); // Invoke UI update after all boosts are added
    }
    public void SetPlayerMenu(PlayerMenu menu)
    {
        playerMenu = menu;
    }

    // Additional methods to handle stat reduction, resetting stats, or other features can be added here
}