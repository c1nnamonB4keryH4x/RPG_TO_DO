using System.Collections.Generic;
using UnityEngine;

public class ClickerRPG : MonoBehaviour
{
    // Player Profile
    public class PlayerProfile
    {
        public int gold;
        public int xp;
        public float damageMultiplier;
        public List<Item> inventory;

        public PlayerProfile()
        {
            gold = 0;
            xp = 0;
            damageMultiplier = 1.0f;
            inventory = new List<Item>();
        }
    }

    // Item Class
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

    // Monster Class
    public class Monster
    {
        public string sprite;
        public int health;
        public int despawnClicks;

        public Monster(string sprite, int health, int despawnClicks)
        {
            this.sprite = sprite;
            this.health = health;
            this.despawnClicks = despawnClicks;
        }

        public void TakeDamage(float damageMultiplier)
        {
            health -= (int)(damageMultiplier * 10); // Assuming base damage is 10
            if (health <= 0)
            {
                despawnClicks--;
                if (despawnClicks <= 0)
                {
                    // Despawn logic
                    Debug.Log("Monster despawned!");
                }
                else
                {
                    health = (int)(100 * damageMultiplier); // Reset health based on damage multiplier
                }
            }
        }
    }

    // Task Class
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
            player.gold += goldReward * difficulty;
            player.xp += xpReward * difficulty;
            Debug.Log($"Task {name} completed! Gold: {goldReward * difficulty}, XP: {xpReward * difficulty}");
        }
    }

    // Shop Class
    public class Shop
    {
        public List<Item> itemsForSale;

        public Shop()
        {
            itemsForSale = new List<Item>
            {
                new Item("Sword", 1.2f, 100),
                new Item("Shield", 1.5f, 150),
                new Item("Potion", 0.8f, 50)
            };
        }

        public bool BuyItem(PlayerProfile player, Item item)
        {
            if (player.gold >= item.price)
            {
                player.gold -= item.price;
                player.inventory.Add(item);
                player.damageMultiplier += item.damageMultiplier;
                Debug.Log($"Item {item.name} purchased! Damage Multiplier: {player.damageMultiplier}");
                return true;
            }
            else
            {
                Debug.Log("Not enough gold to purchase the item.");
                return false;
            }
        }
    }

    // Game Manager
    public PlayerProfile player;
    public List<Monster> monsters;
    public List<Task> tasks;
    public Shop shop;
    public int currentLevel;

    void Start()
    {
        player = new PlayerProfile();
        monsters = new List<Monster>
        {
            new Monster("Goblin", 100, 5),
            new Monster("Orc", 150, 10)
        };
        tasks = new List<Task>
        {
            new Task("Kill Goblins", 50, 20, 1),
            new Task("Kill Orcs", 100, 30, 2)
        };
        shop = new Shop();
        currentLevel = 1;
    }

    void Update()
    {
        // Example: Click to attack the first monster
        if (Input.GetMouseButtonDown(0))
        {
            if (monsters.Count > 0)
            {
                monsters[0].TakeDamage(player.damageMultiplier);
            }
        }

        // Example: Check level and XP
        //leveling curve dynamically scales xp requirement per level
        int requiredXP = 25 * currentLevel * currentLevel + (currentLevel - 1);
        if (player.xp >= requiredXP)
        {
            currentLevel++;
            player.xp -= requiredXP;
            Debug.Log($"Level up! Current level: {currentLevel}");

            if (currentLevel == 2)
            {
                // Unlock new area or features
                Debug.Log("New area unlocked!");
            }
        }

        // Example: Complete a task
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (tasks.Count > 0)
            {
                tasks[0].CompleteTask(player);
                tasks.RemoveAt(0);
            }
        }

        // Example: Buy an item from the shop
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (shop.itemsForSale.Count > 0)
            {
                shop.BuyItem(player, shop.itemsForSale[0]);
            }
        }

        // Example: Create a new task
        if (Input.GetKeyDown(KeyCode.C))
        {
            string taskName = "New Task";
            int goldReward = 10;
            int xpReward = 5;
            int difficulty = 1;

            Task newTask = new Task(taskName, goldReward, xpReward, difficulty);
            tasks.Add(newTask);
            Debug.Log($"Task {taskName} created with difficulty {difficulty}!");
        }
    }
}
