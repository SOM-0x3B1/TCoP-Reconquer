using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace Symbolus
{
    /// <summary>
    /// Eszköztár (kezelőfelület + tároló)
    /// A játék során "E"-vel meghívható, a játékos fegyvert és páncélt szerelhet fel, illetve főzeteket (gyógyító, erősítő, stb.) fogyaszthat (fogyócikk) el.
    /// </summary>
    public class Inventory : Displayable
    {        
        public WaveEngine waveEngine;

        private int cursorPos = 0; //"kurzor" veritkális pozíciója
        private int page = 1; //"kurzor" horizntális pozíciója (lap)
        private int firstListed = 0; //a kilistázás kezdete

        public List<Weapon> weapons = new List<Weapon>(); 
        public Weapon equippedWeapon = new Weapon("Ököl", "normal", 2, "Ehhez szerintem nem kell leírás.", 0); 
        public List<Armor> armors = new List<Armor>();
        public Armor equippedArmor = new Armor("Semmi", "normal", 0, "A játékos fanáziájára bízom.", 0);
        public Dictionary<string, Potion> potions = new Dictionary<string, Potion>();        

        public Inventory()
        {
            using (StreamReader r = new StreamReader(@"assets\menu\inventory.txt", Encoding.UTF8)) //grafika beolvasása
            {
                int i = 0;
                string line = "";

                while (!r.EndOfStream)
                {
                    line = r.ReadLine();
                    matrix[i] = line;
                    i++;                    
                }
                maxY = i-2;
            }
            waveEngine = new WaveEngine(matrix, maxY, maxY / 2);
        }


        /// <summary>
        /// Megjelenítés
        /// </summary>
        public void Display()
        {
            waveEngine.Stop();
            Console.Clear();
            char c;
            int listed = 0;
            TextBox description = new TextBox();

            if (page == 1 && weapons.Count > 0)
                description = weapons[cursorPos + firstListed].description;
            else if (page == 2 && armors.Count > 0)
                description = armors[cursorPos + firstListed].description;
            else if (page == 3 && potions.Count > 0)
                description = potions.ToList()[cursorPos + firstListed].Value.description;
            else
                description = null;


            for (int j = 0; j <= this.maxY; j++)
            {
                for (int i = 0; i < this.matrix[0].Length; i++)
                {
                    c = this.matrix[j][i];
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
                    else if (c == page.ToString()[0])
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    else if (c >= '0' && c <= '9')
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    else if (c == '-')
                    {
                        int index = firstListed + listed;

                        if (listed == cursorPos)
                        {
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = ConsoleColor.Gray;
                        }

                        if ((page == 1 && weapons.Count > index) || (page == 2 && armors.Count > index) || (page == 3 && potions.Count > index))
                        {
                            Console.Write(index + 1);
                            Console.BackgroundColor = ConsoleColor.Black;
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
                        if (page == 1 && (index < weapons.Count()))
                            WriteListItem(weapons[index], 20, false);
                        else if (page == 2 && (firstListed + listed < armors.Count()))
                            WriteListItem(armors[index], 20, false);
                        else if (page == 3 && (firstListed + listed < potions.Count()))
                            WriteListItem(potions.ElementAt(index).Value, 20);
                        else
                            Console.Write("                    ");

                        i += 23;
                        listed++;
                    }
                    else if (c == 'W')
                    {
                        c = ' ';
                        if (equippedWeapon != null)
                            WriteListItem(equippedWeapon, 11, true);
                        else
                            Console.Write("           ");
                        i += 11;
                    }
                    else if (c == 'A')
                    {
                        c = ' ';                        
                        if (equippedArmor != null)
                            WriteListItem(equippedArmor, 11, true);
                        else
                            Console.Write("           ");
                        i += 11;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    Console.Write(c);
                    Console.BackgroundColor = ConsoleColor.Black;
                }

                if (description!=null && j <= description.maxY && page != 4) 
                {
                    Console.Write(" ");
                    description.WriteAtLine(j);
                }                

                Console.WriteLine();
            }
            Controls();
        }


        /// <summary>
        /// Eszközök kiírása a megfelelő hosszúsággal
        /// </summary>
        /// <param name="item">kiírandó eszköz</param>
        /// <param name="length">kívánt hossz</param>
        /// <param name="writeEquippedSign">Ki kell-e írni mellé, ha fel van szerelve</param>
        protected void WriteListItem(Equipment item, int length, bool writeEquippedSign) {
            string output = item.name;
            if (item.equipped && !writeEquippedSign)
                output += " [E]";

            Console.ForegroundColor = item.rarity;

            while (output.Length < length)
                output += " ";
            Console.Write(output);
        }
        protected void WriteListItem(Potion item, int length)
        {
            string output = item.name;
            output += $" [{item.count}]";

            Console.ForegroundColor = ConsoleColor.White;

            while (output.Length < length)
                output += " ";
            Console.Write(output);
        }


        /// <summary>
        /// Utasítás várása, fogadása, végrehajtása
        /// </summary>
        protected void Controls()
        {
            waveEngine.Start();
            ConsoleKeyInfo keyinfo;
            while (Console.KeyAvailable)
            {
                keyinfo = Console.ReadKey(true);
            }
            keyinfo = Console.ReadKey(true);

            if (keyinfo.Key != ConsoleKey.Spacebar && keyinfo.Key != ConsoleKey.Enter)
            {
                if (keyinfo.Key == ConsoleKey.W)
                {
                    if (cursorPos > 0)
                    {
                        cursorPos--;
                    }
                    else
                    {
                        cursorPos = 0;
                        if(firstListed>0)
                            firstListed--;
                    }
                    Program.PlaySound("menumove");
                }
                else if (keyinfo.Key == ConsoleKey.S)
                {
                    if ((page == 1 && weapons.Count - 1 > cursorPos + firstListed) || (page == 2 && armors.Count - 1 > cursorPos + firstListed) || (page == 3 && potions.Count - 1 > cursorPos + firstListed))
                    {
                        cursorPos++;
                        Program.PlaySound("menumove");
                    }
                    if (cursorPos > 5 && ((page == 1 && weapons.Count > cursorPos + firstListed) || (page == 2 && armors.Count > cursorPos + firstListed) || (page == 3 && potions.Count - 1 > cursorPos + firstListed)))
                    {
                        cursorPos--;
                        firstListed++;                        
                    }
                }
                else if (keyinfo.Key == ConsoleKey.A)
                {
                    if (page > 1)
                    {
                        page--;                        
                    }
                    firstListed = 0;
                    cursorPos = 0;
                    Program.PlaySound("menumove");
                }
                else if (keyinfo.Key == ConsoleKey.D)
                {
                    if (page < 4)
                    {
                        page++;                        
                    }
                    cursorPos = 0;
                    firstListed = 0;
                    Program.PlaySound("menumove");
                }
                else if (keyinfo.Key == ConsoleKey.E)
                {
                    waveEngine.Stop();
                    Program.PlaySound("inv2");
                    if(Program.combat)
                        Program.nextDisplayed = Program.Screen.Combat;
                    else
                        Program.nextDisplayed = Program.Screen.Map;
                    page = 1;
                }
                else if (keyinfo.KeyChar >= '1' && keyinfo.KeyChar <= '9')
                {
                    int newPos = int.Parse(keyinfo.KeyChar.ToString()) - 1;
                    if (page == 1 && newPos < weapons.Count) 
                        cursorPos = newPos;
                    else if (page == 2 && newPos < armors.Count)
                        cursorPos = newPos;
                    else if (page == 3 && newPos < potions.Count)
                        cursorPos = newPos;
                }
                else if (keyinfo.Key == ConsoleKey.Escape)
                {
                    waveEngine.Stop();
                    Program.PlaySound("pause");
                    Program.menu.backFromPause = true;
                    Program.menu.menuPage = new MenuPage("pause");
                    Program.nextDisplayed = Program.Screen.Menu;
                }
            }
            else
            {
                if (page == 1 && weapons.Count > 0)
                {
                    Program.PlaySound("action_selected");

                    equippedWeapon.equipped = false;
                    equippedWeapon = weapons[cursorPos + firstListed];
                    weapons[cursorPos+firstListed].equipped = !weapons[cursorPos + firstListed].equipped;

                    if (Tutorial.progress == 4)
                    {                       
                        Tutorial.Next();
                        Program.mapScreen.map.enemies.Add(new EnemyGroup(0, 3, "1", 2, "forest", 0));
                    }
                }
                else if (page == 2 && armors.Count > 0)
                {
                    Program.PlaySound("action_selected");

                    equippedArmor.equipped = false;
                    equippedArmor = armors[cursorPos + firstListed];
                    armors[cursorPos + firstListed].equipped = !armors[cursorPos + firstListed].equipped;
                }
                else if (page == 3 && potions.Count > 0)
                {            
                    Potion potion = potions.ElementAt(cursorPos + firstListed).Value;
                    if (Program.combat) 
                    {
                        Program.PlaySound("action_selected");                        

                        Program.player.effects.Add(potion.effect);
                        potion.count--;

                        if (potion.count == 0)
                        {
                            potions.Remove(potion.name);                           
                            if (potions.Count > 0 && cursorPos>0)
                                cursorPos--;
                        }
                    }
                    else
                        Program.PlaySound("error");
                }
                else if (page == 4)
                {
                    waveEngine.Stop();
                    Program.PlaySound("inv2");
                    if (Program.combat)
                        Program.nextDisplayed = Program.Screen.Combat;
                    else
                        Program.nextDisplayed = Program.Screen.Map;
                    page = 1;
                }
                else
                {
                    Program.PlaySound("error");
                }                
            }
        }
    }
}
