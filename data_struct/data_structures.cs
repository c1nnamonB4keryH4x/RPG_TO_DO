using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClickerRPG : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statsText; // Displays player's stats
    [SerializeField] private TMP_InputField taskInputField;
    [SerializeField] private TMP_InputField difficultyInputField; // New input field for difficulty
    [SerializeField] private Button buyItemButton, completeTaskButton, addTaskButton, gachaPullButton;
    [SerializeField] private GameObject monsterImage; // Reference to the monster image UI
    [SerializeField] private GameObject taskListContainer; // This should be a ScrollRect in the UI
    [SerializeField] private GameObject taskPrefab;
    [SerializeField] private GameObject itemPrefab; // Prefab for displaying items in the shop
    [SerializeField] private GameObject inventoryContainer; // Container for the inventory UI
    [SerializeField] private TextMeshProUGUI equippedItemText; // Text to display equipped item
    [SerializeField] private TextMeshProUGUI taskCountText; // Text to display the number of tasks

    public PlayerProfile player;
    public List<Monster> monsters;
    public List<Task> tasks;
    public Shop shop;
    public int currentLevel = 1;

    private List<Monster> monsterPool = new List<Monster>
    {
        new Monster("Goblin", 100, 10, "Sprites/Goblin"), // Path to the monster sprite
        new Monster("Orc", 150, 20, "Sprites/Orc"),
        new Monster("Troll", 200, 30, "Sprites/Troll"),
        new Monster("Skeleton", 120, 15, "Sprites/Skeleton")
    };

    void Start()
    {
        player = new PlayerProfile();
        monsters = new List<Monster> { GetRandomMonster() };
        tasks = new List<Task>();
        shop = new Shop();

        buyItemButton.onClick.AddListener(BuyItem);
        completeTaskButton.onClick.AddListener(CompleteTask);
        addTaskButton.onClick.AddListener(AddTask);
        gachaPullButton.onClick.AddListener(GachaPull);
        monsterImage.GetComponent<Button>().onClick.AddListener(AttackMonster);

        UpdateUI();
        DisplayShopItems(); // Display items in the shop at the start
        DisplayInventoryItems(); // Display items in the inventory at the start
    }

    private void AttackMonster()
    {
        if (monsters.Count > 0)
        {
            monsters[0].TakeDamage(player.GetTotalDamage());
            if (monsters[0].health <= 0)
            {
                player.xp += monsters[0].GetScaledXP(currentLevel);
                monsters.RemoveAt(0);
                monsters.Add(GetRandomMonster());
            }
        }
        UpdateUI();
    }

    private void BuyItem()
    {
        if (shop.itemsForSale.Count > 0 && player.gold >= shop.itemsForSale[0].price)
        {
            player.BuyItem(shop.itemsForSale[0]);
            shop.itemsForSale.RemoveAt(0);
        }
        UpdateUI();
    }

    private void CompleteTask()
    {
        if (tasks.Count > 0)
        {
            Task completedTask = tasks[0];
            tasks.RemoveAt(0);

            // Remove the task UI when completed
            DestroyTaskUI(completedTask);

            // Add the gold reward and XP for the completed task
            completedTask.CompleteTask(player);

            // Update the stats UI to reflect the new gold and XP
            UpdateUI();
        }
    }

    private void AddTask()
    {
        string taskName = taskInputField.text;
        if (int.TryParse(difficultyInputField.text, out int difficulty) && !string.IsNullOrEmpty(taskName))
        {
            Task newTask = new Task(taskName, Random.Range(10, 50), Random.Range(5, 20), difficulty);
            tasks.Add(newTask);
            taskInputField.text = "";
            difficultyInputField.text = ""; // Clear difficulty input field
            DisplayTask(newTask);
        }
        UpdateUI();
    }

    private void DisplayTask(Task task)
    {
        GameObject newTaskUI = Instantiate(taskPrefab, taskListContainer.transform);
        newTaskUI.GetComponentInChildren<TextMeshProUGUI>().text = $"{task.name} (Gold: {task.goldReward * task.difficulty}, XP: {task.xpReward})";
        newTaskUI.GetComponent<Button>().onClick.AddListener(() => CompleteTask());
    }

    private void DestroyTaskUI(Task task)
    {
        // Find and destroy the task's UI element when the task is completed
        foreach (Transform child in taskListContainer.transform)
        {
            if (child.GetComponentInChildren<TextMeshProUGUI>().text.Contains(task.name))
            {
                Destroy(child.gameObject);
                break;
            }
        }
    }

    private void UpdateUI()
    {
        // Update the UI with the player's current gold, XP, and level, and the number of tasks
        statsText.text = $"Gold: {player.gold} | XP: {player.xp} | Level: {currentLevel} | Tasks: {tasks.Count}";

        // Update the monster image based on the current monster
        if (monsters.Count > 0)
        {
            monsterImage.GetComponent<Image>().sprite = Resources.Load<Sprite>(monsters[0].spritePath);
        }

        // Update equipped item text
        equippedItemText.text = player.equippedItem != null ? $"Equipped: {player.equippedItem.name}" : "Equipped: None";

        // Update task count text
        taskCountText.text = $"Tasks: {tasks.Count}";

        // Level up check if XP exceeds threshold
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        int requiredXP = 25 * currentLevel * currentLevel + (currentLevel - 1);
        if (player.xp >= requiredXP)
        {
            currentLevel++;
            player.xp -= requiredXP; // Adjust XP for next level.
        }
    }

    private Monster GetRandomMonster()
    {
        return new Monster(monsterPool[Random.Range(0, monsterPool.Count)]);
    }

    // Implement Gacha Pull Mechanic
    private void GachaPull()
    {
        int cost = 100; // Cost per pull
        if (player.gold >= cost)
        {
            player.gold -= cost;
            Item pulledItem = GetRandomGachaItem();
            player.BuyItem(pulledItem);
            DisplayGachaItem(pulledItem);
            UpdateUI();
        }
        else
        {
            Debug.Log("Not enough gold for a Gacha pull!");
        }
    }

    // Get a random item from the gacha pool
    private Item GetRandomGachaItem()
    {
        int roll = Random.Range(0, 100);
        Item item;

        if (roll < 50)
        {
            // Common item
            item = new Item("Basic Sword", 1.1f, 50);
        }
        else if (roll < 80)
        {
            // Uncommon item
            item = new Item("Silver Sword", 1.3f, 150);
        }
        else
        {
            // Rare item
            item = new Item("Golden Sword", 1.5f, 300);
        }

        return item;
    }

    // Display the gacha item
    private void DisplayGachaItem(Item item)
    {
        GameObject itemUI = Instantiate(itemPrefab, taskListContainer.transform);
        itemUI.GetComponentInChildren<TextMeshProUGUI>().text = $"Gacha Pull: {item.name} (Damage: {item.damageMultiplier}, Price: {item.price})";
    }

    // Display items in the shop
    private void DisplayShopItems()
    {
        foreach (var item in shop.itemsForSale)
        {
            GameObject itemUI = Instantiate(itemPrefab, taskListContainer.transform);
            itemUI.GetComponentInChildren<TextMeshProUGUI>().text = $"{item.name} (Damage: {item.damageMultiplier}, Price: {item.price})";
            itemUI.GetComponent<Button>().onClick.AddListener(() => player.BuyItem(item));
        }
    }

    // Display items in the inventory
    private void DisplayInventoryItems()
    {
        foreach (var item in player.inventory)
        {
            GameObject itemUI = Instantiate(itemPrefab, inventoryContainer.transform);
            itemUI.GetComponentInChildren<TextMeshProUGUI>().text = $"{item.name} (Damage: {item.damageMultiplier}, Price: {item.price})";
            itemUI.GetComponent<Button>().onClick.AddListener(() => player.EquipItem(item));
        }
    }
}

public class PlayerProfile
{
    public int gold;
    public int xp;
    public int level;
    private float baseDamage = 10f;
    public List<Item> inventory;
    public Item equippedItem; // New field for equipped item

    public PlayerProfile()
    {
        gold = 0;
        xp = 0;
        level = 1;
        inventory = new List<Item>();
    }

    public float GetTotalDamage()
    {
        float multiplier = 1.0f;
        foreach (var item in inventory)
        {
            multiplier += item.damageMultiplier;
        }
        if (equippedItem != null)
        {
            multiplier += equippedItem.damageMultiplier; // Add equipped item damage
        }
        return baseDamage * multiplier;
    }

    public void EquipItem(Item item)
    {
        if (inventory.Contains(item))
        {
            equippedItem = item; // Equip the item
        }
    }

    public void BuyItem(Item item)
    {
        if (gold >= item.price)
        {
            gold -= item.price;
            inventory.Add(item);
        }
    }
}

public class Item
{
    public string name;
    public float damageMultiplier;
    public int price;

    public Item(string name, float damageMultiplier, int price)
    {
        this.name = name;
        this.damageMultiplier = damageMultiplier;
        this.price = price;
    }
}

public class Monster
{
    public string spritePath; // Path to the monster sprite
    public int health;
    private int baseXP;

    public Monster(string spritePath, int health, int baseXP)
    {
        this.spritePath = spritePath;
        this.health = health;
        this.baseXP = baseXP;
    }

    public Monster(Monster other)
    {
        this.spritePath = other.spritePath;
        this.health = other.health;
        this.baseXP = other.baseXP;
    }

    public void TakeDamage(float damage)
    {
        health -= (int)damage;
    }

    public int GetScaledXP(int playerLevel)
    {
        return (int)(baseXP * (1.2 * playerLevel));
    }
}

public class Task
{
    public string name;
    public int goldReward;
    public int xpReward;
    public int difficulty;

    public Task(string name, int goldReward, int xpReward, int difficulty)
    {
        this.name = name;
        this.goldReward = goldReward;
        this.xpReward = xpReward;
        this.difficulty = difficulty;
    }

    public void CompleteTask(PlayerProfile player)
    {
        player.gold += goldReward * difficulty; // Adjust gold based on task difficulty.
        player.xp += xpReward; // Add XP reward to player.
    }
}

public class Shop
{
    public List<Item> itemsForSale;

    public Shop()
    {
        itemsForSale = new List<Item>
        {
            new Item("Sword", 1.3f, 100),
            new Item("Shield", 2f, 150),
            new Item("Potion", 1.2f, 50)
        };
    }
}
