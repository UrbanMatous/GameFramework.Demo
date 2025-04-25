using System;
using System.Collections.Generic;
using System.Linq;
using MandatoryUrban;
using MandatoryUrban.Actions;
using MandatoryUrban.Items;

class Program
{
    static void Main(string[] args)
    {
        var config = GameConfiguration.Load();
        GameLogger.Log($"Game started. World size: {config.WorldWidth}x{config.WorldHeight}, Level: {config.GameLevel}");

        // Create characters
        var aragorn = new Warrior { Name = "Aragorn", HitPoints = 50, X = 0, Y = 0 };
        var goblin = new Warrior { Name = "Goblin", HitPoints = 60, X = 2, Y = 2 };
        var goblinKing = new Warrior { Name = "Goblin King", HitPoints = 100, X = 2, Y = 2 };

        // Observer setup
        var hitTracker = new HitTracker();
        aragorn.Attach(hitTracker);
        goblin.Attach(hitTracker);
        goblinKing.Attach(hitTracker);

        // Setup items and loot
        var sword = new AttackItem { HitPoints = 10, Description = "Sword" };
        var fireball = new AttackItem { HitPoints = 8, Description = "Fireball" };
        var defenseItem = new DefenseItem { Protection = 3, Description = "Shield" };
        var rustySword = new AttackItem { HitPoints = 2, Description = "Rusty Sword" };

        var rustedSword = new BonusDamageDecorator(rustySword, 3, "Rusted");
        GameLogger.Log("Goblin has " + rustedSword.Description);
        goblin.Attacks.Add( rustedSword );
        var lootBox = new WorldObject("Treasure Chest",0,0)
        {
            ContainedAttackItem = fireball,
            ContainedDefenseItem = defenseItem,
            BonusHitPoints = 10
        };

        // Thor loots the treasure chest
        aragorn.Loot(lootBox);

        // Equip basic sword
        var swordLoot = new WorldObject("Sword of Olympus",0,0)
        {
            ContainedAttackItem = sword
        };
        aragorn.Loot(swordLoot);
 

        // Game loop with simple AI
        var creatures = new List<Creature> { aragorn, goblin };

        for (int turn = 1; goblin.IsAlive;turn++)
        {
            Console.WriteLine($"--- Turn {turn} ---");

            foreach (var c in creatures.Where(c => c.IsAlive))
            {
                c.Move(1, -1);

                var target = creatures.FirstOrDefault(x => x != c && x.IsAlive);
                if (target != null && target.IsAlive)
                {
                    c.Attack(target);
                }
            }
        }

        Console.WriteLine($"--- Continue to Goblin King ---");

        // Apply boosted weapon
        var boostedSword = new LowHealthBoostDecorator(sword, aragorn);
        aragorn.Attacks.Clear();
        aragorn.Attacks.Add(boostedSword);
        aragorn.Attacks.Add(fireball);
        aragorn.HitPoints = 20; // simulate low HP
        GameLogger.Log("Thor is now below 25HP");
        GameLogger.Log("Low Health Boost Added!(+5 HitPoints)");

        // Attack using boosted weapon
        if (goblinKing.IsAlive)
        {
            aragorn.Attack(goblinKing);
            // Do composite attacks or strategy switch
        }

        // Composite weapon example
        var attackGroup = new AttackGroup();
        attackGroup.Add(new AttackItem { HitPoints = 3, Description = "Lightning Strike" });
        attackGroup.Add(new AttackItem { HitPoints = 4, Description = "Ice Blade" });

        aragorn.Attacks.Clear();
        aragorn.Attacks.Add(attackGroup);
        if (goblinKing.IsAlive)
        {
            aragorn.Attack(goblinKing);
            // Do composite attacks or strategy switch
        }

        // Strategy switch
        aragorn.AttackStrategy = new AggressiveStrategy();
        GameLogger.Log("Thor switched to Aggresive Strategy(+5 hitPoints)");
        if (goblinKing.IsAlive)
        {
            aragorn.Attack(goblinKing);
            // Do composite attacks or strategy switch
        }

        Console.WriteLine("Full demo complete. Check log for detailed output.");
    }
}