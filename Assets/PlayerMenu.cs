using UnityEngine;
using UnityEngine.UI;
using TMPro; // If using TextMeshPro

public class PlayerMenu : MonoBehaviour
{
    public GameObject menuCanvas;
    private PlayerStats playerStats;

    // This method initializes the PlayerMenu with a reference to PlayerStats
    public void Initialize(PlayerStats stats)
    {
        playerStats = stats;
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats reference not provided to PlayerMenu.");
            return;
        }

        UpdateUI(); // Update the UI as soon as it's initialized
    }


    void Start()
    {
        if (menuCanvas == null)
        {
            Debug.LogError("Menu Canvas is not set in PlayerMenu");
            return;
        }

        CreateUIElements();
        menuCanvas.SetActive(false);
    }

    void CreateUIElements()
    {
        // Create a background panel
        GameObject backgroundPanel = new GameObject("BackgroundPanel");
        backgroundPanel.transform.SetParent(menuCanvas.transform);
        Image bgImage = backgroundPanel.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.5f); // Semi-transparent black

        // Set the size of the background panel
        RectTransform bgRect = backgroundPanel.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Create the Text element
        GameObject textGameObject = new GameObject("PlayerStatText");
        textGameObject.transform.SetParent(backgroundPanel.transform); // Parent to the background panel

        // Add TextMeshPro component
        TextMeshProUGUI textComponent = textGameObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = "Initial Text";

        // Position and style the textComponent
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontSize = 24;

        // Adjust the RectTransform of the text
        RectTransform rectTransform = textGameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero; // Center in parent
        rectTransform.sizeDelta = new Vector2(400, 600); // Width and height
    }

    public void UpdateUI()
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats reference not set or found. Cannot update UI.");
            return;
        }

        TextMeshProUGUI textComponent = GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            string statsText = "Player Stats:\n";
            foreach (var stat in playerStats.currentStats)
            {
                statsText += $"{stat.Key}: {stat.Value}\n";
            }
            textComponent.text = statsText;
            Debug.Log("PlayerMenu UI updated with: " + statsText);
        }
        else
        {
            Debug.LogError("TextMeshPro component not found in PlayerMenu.");
        }
    }

    // Call this method to update the UI from other scripts
    public void ForceUpdateUI()
    {
        UpdateUI();
    }

    public void ToggleMenu()
    {
        bool isMenuBeingOpened = !menuCanvas.activeSelf;
        menuCanvas.SetActive(isMenuBeingOpened);

        if (isMenuBeingOpened)
        {
            UpdateUI(); // Update UI only when the menu is opened
        }
    }

    void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.onStatsChanged += UpdateUI;
        }
    }

    void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.onStatsChanged -= UpdateUI;
        }
    }
    // Additional methods...
}

