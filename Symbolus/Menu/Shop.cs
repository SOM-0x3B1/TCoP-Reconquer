using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace Symbolus
{
    /// <summary>
    /// Bizonyos NPC-k boltokkal rendelkeznek. A játékos pénzért vehet ki belőlük felszerelést
    /// </summary>
    public class Shop : Displayable
    {
        List<Good> goods = new List<Good>(); //áru
        string type; // milyen árukat ad (weapon/armor/potion)

        public WaveEngine waveEngine;

        private int cursorPos = 0; //"kurzor" veritkális pozíciója
        private int firstListed = 0; //a kilistázás kezdete

        private TextBox description = new TextBox();


        public Shop(string npc)
        {
            using (StreamReader r = new StreamReader($@"assets\shops\{npc}.txt", Encoding.UTF8))
            {
                this.type = r.ReadLine();
                while (!r.EndOfStream)
                {
                    this.goods.Add(new Good(type, r.ReadLine().Split(';')));
                }
            }

            using (StreamReader r = new StreamReader(@"assets\shops\art.txt", Encoding.UTF8))
            {
                int i = 0;
                BasicMatrixBuilder(r, ref i);
            }

            waveEngine = new WaveEngine(matrix, maxY, maxY / 2);
        }

        /// <summary>
        /// Bolt kirajzolása
        /// </summary>
        public void Display()
        {
            waveEngine.Stop();
            Console.Clear();
            char c;
            int listed = 0;
            int index;
            int x = 0;

            for (int j = 0; j <= this.maxY; j++)
            {
                index = firstListed + listed;

                if (firstListed + cursorPos < goods.Count)
                    description = goods[firstListed + cursorPos].description;
                else
                    description = null;

                for (int i = 0; i < this.maxX; i++)
                {
                    SkipSpace(j, ref i);
                    if (i < maxX)
                        c = matrix[j][i];
                    else
                        break;

                    if (c == '#')
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    else if (c == 'f')
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                    }
                    else if (c == '~')
                    {
                        Console.BackgroundColor = ConsoleColor.Blue;
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                    }
                    else if (c == '-')
                    {
                        if (listed == cursorPos)
                        {
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = ConsoleColor.Gray;
                        }

                        if (goods.Count > index)
                        {
                            Console.Write(index + 1);
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            if ((index + 1).ToString().Length < 2)
                                Console.Write("  ");
                            else
                                Console.Write(" ");
                        }
                        else
                        {
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.Write("   ");
                        }

                        c = ' ';
                        if (type == "weapon" && (index < goods.Count))
                            WriteListItemLevel(goods[index].weapon, 18, false, goods[index].weapon.level);
                        else if (type == "armor" && (firstListed + listed < goods.Count()))
                            WriteListItem(goods[index].armor, 18, false);
                        else if (type == "potion" && (firstListed + listed < goods.Count()))
                        {
                            string name = goods[index].potion.name;
                            Console.Write(name);
                            string quantity = " [0]";
                            if (Program.player.inventory.potions.ContainsKey(name))
                            {
                                quantity = " [" + Program.player.inventory.potions.First(a => a.Key == name).Value.count.ToString() + "]";
                            }
                            Console.Write(quantity);
                            i += goods[index].potion.name.Length + 3 + quantity.Length;
                        }
                        else
                        {
                            Console.Write("                  ");
                            if (type == "potion")
                                i += 21;
                        }

                        if (type != "potion")
                            i += 21;
                        listed++;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    Console.Write(c);
                    Console.BackgroundColor = ConsoleColor.Black;
                }

                if (description != null && description.maxY >= x && j > 7)
                {
                    Console.Write("   ");
                    description.WriteAtLine(x);
                    x++;
                }

                if (j <= 6)
                {
                    char c2;
                    for (int k = 0; k < Program.wallet[0].Length; k++)
                    {
                        c2 = Program.wallet[j][k];
                        switch (c2)
                        {
                            case '#':
                                Console.BackgroundColor = ConsoleColor.Gray;
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                            case '1':
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write(Program.player.money.ToString() + " Ft");
                                k += Program.player.money.ToString().Length + 2;
                                c2 = ' ';
                                break;
                            case '2':
                                Console.ForegroundColor = ConsoleColor.Blue;
                                string s = Program.player.xp.ToString() + "/" + Program.player.levelsteps[Program.player.level].ToString();
                                Console.Write(s);
                                k += s.Length - 1;
                                c2 = ' ';
                                break;
                            /*case '2':
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.Write();
                                k += Program.player.levels[Program.player.level].ToString().Length - 1;
                                c2 = ' ';
                                break;*/
                            case '3':
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write(Program.player.level.ToString());
                                k += Program.player.level.ToString().Length - 1;
                                c2 = ' ';
                                break;
                            default:
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = ConsoleColor.White;
                                break;
                        }
                        Console.Write(c2);
                    }
                }

                Console.WriteLine();
            }
            Controls();
        }

        /// <summary>
        /// Irányítás
        /// </summary>
        public void Controls()
        {
            waveEngine.Start();
            ConsoleKeyInfo keyinfo;
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
            keyinfo = Console.ReadKey(true);

            if (keyinfo.Key != ConsoleKey.Spacebar && keyinfo.Key != ConsoleKey.Enter)
            {
                if (keyinfo.Key == ConsoleKey.W)
                {
                    if (cursorPos > 0)
                    {
                        cursorPos--;
                        Program.PlaySound("menumove");
                    }
                    else
                    {
                        cursorPos = 0;
                        if (firstListed > 0)
                        {
                            firstListed--;
                            Program.PlaySound("menumove");
                        }
                    }
                }
                else if (keyinfo.Key == ConsoleKey.S)
                {
                    if (goods.Count - 1 > cursorPos + firstListed)
                    {
                        cursorPos++;
                        Program.PlaySound("menumove");
                    }
                    if (cursorPos > 5 && goods.Count > cursorPos + firstListed)
                    {
                        cursorPos--;
                        firstListed++;
                    }
                }
                else if (keyinfo.KeyChar >= '1' && keyinfo.KeyChar <= '9')
                {
                    int newPos = int.Parse(keyinfo.KeyChar.ToString()) - 1;
                    if (newPos < goods.Count)
                        cursorPos = newPos;
                }
                else if (keyinfo.Key == ConsoleKey.Escape)
                {
                    waveEngine.Stop();
                    Program.PlaySound("inv2");
                    Program.nextDisplayed = Program.Screen.Map;
                    if (Program.musicOn)
                    {
                        Program.musics[Program.cmusic].Stop();
                        if (!Program.newGame)
                        {
                            Program.musics[Program.mapScreen.map.music].PlayLooping();
                            Program.cmusic = Program.mapScreen.map.music;
                        }
                        else
                        {
                            Program.musics["confusion"].PlayLooping();
                            Program.cmusic = "confusion";
                        }
                    }
                    cursorPos = 0;
                }
            }
            else
            {
                if (goods.Count > 0)
                {
                    Good good = goods[firstListed + cursorPos];

                    if ((good.type == "potion" && good.potion.cost <= Program.player.money) || (good.type != "potion" && good.level <= Program.player.level))
                    {
                        good.Buy();
                        if (good.type != "potion")
                        {
                            goods.RemoveAt(firstListed + cursorPos);

                            if (cursorPos + firstListed > 0)
                            {
                                if (firstListed == 0)
                                    cursorPos--;
                                else
                                    firstListed--;
                            }
                        }
                    }
                    else
                        Program.PlaySound("error");
                }
            }
        }

        /// <summary>
        /// Listaelemek kírása a megfelelő hosszúságban
        /// </summary>
        protected void WriteListItem(Equipment item, int length, bool writeEquippedSign)
        {
            WLI(item, length, writeEquippedSign, -1);
        }
        protected void WriteListItemLevel(Equipment item, int length, bool writeEquippedSign, int level)
        {
            WLI(item, length, writeEquippedSign, level);
        }
        private void WLI(Equipment item, int length, bool writeEquippedSign, int level)
        {
            string output = item.name;

            if (level > -1)
                output += $" - lvl.{level}";

            if (item.equipped && !writeEquippedSign)
                output += " [E]";

            Console.ForegroundColor = item.rarity;

            while (output.Length < length)
                output += " ";
            Console.Write(output);
        }
    }

    /// <summary>
    /// Áru, lehet fegyver, páncél, vagy fogyócikk (potion)
    /// </summary>
    public class Good
    {
        public Weapon weapon;
        public Armor armor;
        public Potion potion;
        public string type;
        public TextBox description;
        public int level = 0;

        public Good(string type, string[] line)
        {
            if (type == "weapon")
            {
                weapon = new Weapon(line[0], line[1], int.Parse(line[2]), line[3], int.Parse(line[4]));
                description = weapon.description;
                level = weapon.level;
            }
            else if (type == "armor")
            {
                armor = new Armor(line[0], line[1], int.Parse(line[2]), line[3], int.Parse(line[4]));
                description = armor.description;
                level = armor.level;
            }
            else
            {
                potion = new Potion(line[0], line[1], line[2], int.Parse(line[3]), int.Parse(line[4]));
                description = potion.description;
            }

            this.type = type;
        }

        /// <summary>
        /// áru megvásálása
        /// </summary>
        public void Buy()
        {
            Program.PlaySound("buy");
            Inventory inventory = Program.player.inventory;

            if (type == "weapon")
            {
                inventory.weapons.Add(weapon);
                if (Tutorial.progress == 3)
                    Tutorial.Next();
            }
            else if (type == "armor")
                inventory.armors.Add(armor);
            else
            {
                if (!inventory.potions.ContainsKey(potion.name))
                    inventory.potions.Add(potion.name, potion);
                else
                    inventory.potions[potion.name].count++;
                Program.player.money -= potion.cost;
            }
        }
    }
}
