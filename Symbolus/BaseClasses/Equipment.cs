using System;

namespace Symbolus
{
    /// <summary>
    /// Az eszköztárban tárolható fegyver/páncél/főzet
    /// </summary>
    public class Equipment
    {
        public string name;
        public ConsoleColor rarity; //az eszköz ritkasága (sima, jó, ritka, epic, legendás)
        public bool equipped = false; //fel van-e szerelve
        public TextBox description;

        /// <summary>
        /// Stringben tárolt ritkaságot ConsoleColor-rá alakít
        /// </summary>
        /// <param name="rarity">Stringes ritkaság</param>
        protected void RarityChecker(string rarity)
        {
            switch (rarity)
            {
                case "normal":
                    this.rarity = ConsoleColor.Gray;
                    break;
                case "good":
                    this.rarity = ConsoleColor.Green;
                    break;
                case "rare":
                    this.rarity = ConsoleColor.Blue;
                    break;
                case "epic":
                    this.rarity = ConsoleColor.Magenta;
                    break;
                case "legendary":
                    this.rarity = ConsoleColor.DarkYellow;
                    break;
            }
        }
    }

    /// <summary>
    /// Fegyver: alapsebzés
    /// </summary>
    public class Weapon : Equipment
    {
        public int baseDamage;
        public int level;

        public Weapon(string name, string rarity, int damage, string description, int level)
        {
            this.baseDamage = damage;
            this.name = name;
            RarityChecker(rarity);
            this.description = new TextBox(description);
            this.level = level;
        }
    }

    /// <summary>
    /// Páncél: védelem
    /// </summary>
    public class Armor : Equipment
    {
        public int protection;
        public int level;

        public Armor(string name, string rarity, int defense, string description, int level)
        {
            this.name = name;
            this.protection = defense;
            RarityChecker(rarity);
            this.description = new TextBox(description);
            this.level = level;
        }
    }

    public class Potion
    {
        public int count = 1;
        public string name;
        public TextBox description;
        public Effect effect;
        public int cost;

        public Potion() { }

        public Potion(string name, string description, string effect, int effectDuration, int cost) 
        {
            this.name = name;
            this.description = new TextBox(description);
            this.effect = new Effect(effect, effectDuration);
            this.cost = cost;
        }
    }
}