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
        var thor = new Warrior { Name = "Thor", HitPoints = 50, X = 0, Y = 0 };
        var goblin = new Warrior { Name = "Goblin", HitPoints = 60, X = 2, Y = 2 };
        var goblinKing = new Warrior { Name = "Goblin King", HitPoints = 100, X = 2, Y = 2 };

        // Observer setup
        var hitTracker = new HitTracker();
        thor.Attach(hitTracker);
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
        var lootBox = new WorldObject("Treasure Chest")
        {
            ContainedAttackItem = fireball,
            ContainedDefenseItem = defenseItem,
            BonusHitPoints = 10
        };

        // Thor loots the treasure chest
        thor.Loot(lootBox);
        lootBox.ApplyTo(thor);

        // Equip basic sword
        var swordLoot = new WorldObject("Sword of Olympus")
        {
            ContainedAttackItem = sword
        };
        thor.Loot(swordLoot);
        swordLoot.ApplyTo(thor);

        // Game loop with simple AI
        var creatures = new List<Creature> { thor, goblin };

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

        // Apply boosted weapon
        var boostedSword = new LowHealthBoostDecorator(sword, thor);
        thor.Attacks.Clear();
        thor.Attacks.Add(boostedSword);
        thor.Attacks.Add(fireball);
        thor.HitPoints = 20; // simulate low HP
        GameLogger.Log("Thor is now below 25HP");
        GameLogger.Log("Low Health Boost Added!(+5 HitPoints)");

        // Attack using boosted weapon
        if (goblinKing.IsAlive)
        {
            thor.Attack(goblinKing);
            // Do composite attacks or strategy switch
        }

        // Composite weapon example
        var attackGroup = new AttackGroup();
        attackGroup.Add(new AttackItem { HitPoints = 3, Description = "Lightning Strike" });
        attackGroup.Add(new AttackItem { HitPoints = 4, Description = "Ice Blade" });

        thor.Attacks.Clear();
        thor.Attacks.Add(attackGroup);
        if (goblinKing.IsAlive)
        {
            thor.Attack(goblinKing);
            // Do composite attacks or strategy switch
        }

        // Strategy switch
        thor.AttackStrategy = new AggressiveStrategy();
        GameLogger.Log("Thor switched to Aggresive Strategy(+5 hitPoints)");
        if (goblinKing.IsAlive)
        {
            thor.Attack(goblinKing);
            // Do composite attacks or strategy switch
        }

        Console.WriteLine("Full demo complete. Check log for detailed output.");
    }
}