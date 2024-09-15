using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace Symbolus
{
    /// <summary>
    /// Harc közben választott cselekedet (ütés, védekezés, képesség)
    /// </summary>
    public class Action
    {
        public string type;
        public int targetIndex;
        public TextBox description;       
    }

    public class BasicAction : Action
    {
        public string name;

        public BasicAction() { }

        public BasicAction(string type, int targetIndex)
        {
            this.type = type;
            this.targetIndex = targetIndex;
        }

        public BasicAction(string name, string type, string description)
        {
            this.name = name;
            this.type = type;
            this.description = new TextBox(description);

            allBasicActions.Add(type, this);
        }

        public static Dictionary<string, BasicAction> allBasicActions = new Dictionary<string, BasicAction>();
    }


    /// <summary>
    /// Speciális képességek (pl. gyújtás, fagasztás, kábítás).
    /// Egy 'Action' objektum 'type' paramétere mutathat a statikus lista elemeire.
    /// </summary>
    public class Skill : Action
    {
        public string name;
        public string effect; //az utóhatás (pl. égés)
        public int effectDuration; //az utóhatás ideje
        public int damage; //képesség kezdő sebzése
        public int area; //képesség hatásának területe (hány ellenfélre hat)
        public int Scost;
        public int cost;
        //public bool self; //a játékos önmagára használja-e

        public Skill() { }

        public Skill(Skill skill, int targetIndex)
        {
            this.type = skill.name;
            this.targetIndex = targetIndex;
        }

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="name">Képesség neve</param>
        /// <param name="damage">Képesség kezdő sebzése</param>
        /// <param name="area">Képesség hatásának területe (hány ellenfélre hat)</param>
        /// <param name="effect">Utóhatás</param>
        /// <param name="effectDuration">Utóhatás ideje</param>
        /// <param name="self">A játékos önmagára használja-e</param>
        public Skill(string name, int damage, int area, string effect, int effectDuration, int Scost, string description)
        {
            this.name = name;
            this.damage = damage;
            this.area = area;
            this.effect = effect;
            this.effectDuration = effectDuration;
            this.Scost = Scost;

            this.cost = Convert.ToInt32(description[0].ToString());

            this.description = new TextBox(description);
        }

        public static List<Skill> allSkills = new List<Skill>();
    }


    public class Effect
    {
        public string type;
        public int timeRemaining;

        public static Dictionary<string, string> typeToName = new Dictionary<string, string> {
            { "fire", "Égés" },
            { "stun", "Kábítás"},
            { "confusion", "Zavarodottság"},
            { "rage", "Düh"},
            { "poison", "Mérgezés"}
        };

        public Effect(string type, int timeRemaining)
        {
            this.type = type;
            this.timeRemaining = timeRemaining;
        }
    }


    public class ActionMenu : Displayable
    {
        protected int cursorPos = 0; //"kurzor" veritkális pozíciója
        protected int firstListed = 0; //a kilistázás kezdete
        protected WaveEngine waveEngine;

        protected TextBox description = new TextBox();        
    }


    /// <summary>
    /// Az inventory-ho
    /// </summary>
    public class BasicActionMenu : ActionMenu
    {
        private BasicAction selectedAction = new BasicAction();

        public BasicActionMenu()
        {
            using (StreamReader r = new StreamReader(@"assets\menu\action_menu.txt", Encoding.UTF8)) //grafika beolvasása
            {
                int i = 0;
                BasicMatrixBuilder(r, ref i);
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

            description = BasicAction.allBasicActions.ElementAt(cursorPos + firstListed).Value.description;

            for (int j = 0; j <= this.maxY; j++)
            {
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
                        int index = firstListed + listed;

                        if (listed == cursorPos)
                        {
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = ConsoleColor.Gray;
                        }

                        if (BasicAction.allBasicActions.Count > index)
                        {
                            Console.Write(index + 1);
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            if ((index + 1).ToString().Length < 2)
                            {
                                Console.Write("  ");
                            }
                            else
                            {
                                Console.Write(" ");
                            }
                        }
                        else
                        {
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.Write("   ");
                        }



                        c = ' ';
                        if (index < BasicAction.allBasicActions.Count)
                        {
                            selectedAction = BasicAction.allBasicActions.ElementAt(index).Value;
                            WriteListItem(selectedAction, 18, false);
                        }
                        else
                        {
                            Console.Write("                  ");
                        }

                        listed++;
                        i += 21;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    Console.Write(c);
                    Console.BackgroundColor = ConsoleColor.Black;
                }

                if (description != null && description.maxY >= j)
                {
                    Console.Write("  ");
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
        protected void WriteListItem(BasicAction item, int length, bool writeEquippedSign)
        {
            string output = item.name;

            while (output.Length < length)
                output += " ";

            if (item.type == "heal")
                Console.ForegroundColor = ConsoleColor.Green;
            else if (item.type == "shield" || item.type == "dodge")
                Console.ForegroundColor = ConsoleColor.Blue;
            else if (item.type == "skip")
                Console.ForegroundColor = ConsoleColor.Gray;
            else
                Console.ForegroundColor = ConsoleColor.Red;

            Console.Write(output);
        }

        static public void AddViaShortcut(char c)
        {
            Program.PlaySound("action_selected");
            string selectedType = BasicAction.allBasicActions.ElementAt(int.Parse(c.ToString())-1).Value.type;

            if (selectedType == "heavy_attack")
            {
                if (Program.cEnemy.actions.Count < 2)
                {
                    Program.cEnemy.actions.Add(new BasicAction(selectedType, Program.cEnemy.selectedMember));
                    Program.cEnemy.actions.Add(new BasicAction(selectedType, Program.cEnemy.selectedMember));
                }
                else
                    Program.PlaySound("error");
            }
            else
                Program.cEnemy.actions.Add(new BasicAction(selectedType, Program.cEnemy.selectedMember));
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
                    if (BasicAction.allBasicActions.Count - 1 > cursorPos + firstListed)
                    {
                        cursorPos++;
                        Program.PlaySound("menumove");
                    }
                    if (cursorPos > 5 && BasicAction.allBasicActions.Count > cursorPos + firstListed)
                    {
                        cursorPos--;
                        firstListed++;
                    }
                }
                else if (keyinfo.KeyChar >= '1' && keyinfo.KeyChar <= '9')
                {
                    int newPos = int.Parse(keyinfo.KeyChar.ToString()) - 1;
                    if (newPos < BasicAction.allBasicActions.Count)
                        cursorPos = newPos;
                }
                else if (keyinfo.Key == ConsoleKey.Escape)
                {
                    Program.PlaySound("inv2");
                    waveEngine.Stop();
                    Program.nextDisplayed = Program.Screen.Combat;
                    cursorPos = 0;
                }
            }
            else
            {
                Program.PlaySound("action_selected");
                string selectedType = BasicAction.allBasicActions.ElementAt(cursorPos + firstListed).Value.type;


                if (selectedType == "heavy_attack")
                {
                    if (Program.cEnemy.actions.Count < 2)
                    {
                        Program.cEnemy.actions.Add(new BasicAction(selectedType, Program.cEnemy.selectedMember));
                        Program.cEnemy.actions.Add(new BasicAction(selectedType, Program.cEnemy.selectedMember));
                    }
                    else
                        Program.PlaySound("error");
                }
                else
                    Program.cEnemy.actions.Add(new BasicAction(selectedType, Program.cEnemy.selectedMember));

                waveEngine.Stop();
                Program.nextDisplayed = Program.Screen.Combat;

                cursorPos = 0;
            }
        }
    }

    public class SkillActionMenu : ActionMenu
    {
        private Skill selectedSkill = new Skill();

        public SkillActionMenu()
        {
            using (StreamReader r = new StreamReader(@"assets\menu\skill_menu.txt", Encoding.UTF8)) //grafika beolvasása
            {
                int i = 0;
                BasicMatrixBuilder(r, ref i);
            }
        }

        /// <summary>
        /// Megjelenítés
        /// </summary>
        public void Display()
        {
            Console.Clear();
            char c;
            int listed = 0;

            if (Program.player.skills.Count > 0)
                description = Program.player.skills[cursorPos + firstListed].description;
            else
                description = null;

            for (int j = 0; j <= this.maxY; j++)
            {
                for (int i = 0; i < this.maxX; i++)
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
                    else if (c == '-')
                    {
                        int index = firstListed + listed;

                        if (listed == cursorPos)
                        {
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = ConsoleColor.Gray;
                        }
                       

                        if (Program.player.skills.Count > index)
                        {
                            Console.Write(index + 1);
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            if ((index + 1).ToString().Length < 2)
                            {
                                Console.Write("  ");
                            }
                            else
                            {
                                Console.Write(" ");
                            }
                        }
                        else
                        {
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.Write("   ");
                        }

                        c = ' ';
                        if (index < Program.player.skills.Count)
                        {
                            selectedSkill = Program.player.skills[index];
                            WriteListItem(selectedSkill, 18);
                        }
                        else
                        {
                            Console.Write("                  ");
                        }

                        listed++;
                        i += 21;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    Console.Write(c);
                    Console.BackgroundColor = ConsoleColor.Black;
                }

                if (description != null && description.maxY >= j)
                {
                    Console.Write("  ");
                    description.WriteAtLine(j);
                }

                Console.WriteLine();
            }
            Controls();
        }

        protected void WriteListItem(Skill item, int length)
        {
            string output = item.name;

            while (output.Length < length)
                output += " ";

            Console.Write(output);
        }       


        /// <summary>
        /// Utasítás várása, fogadása, végrehajtása
        /// </summary>
        protected void Controls()
        {
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
                    if (Program.player.skills.Count - 1 > cursorPos + firstListed)
                    {
                        cursorPos++;
                        Program.PlaySound("menumove");
                    }
                    if (cursorPos > 5 && Program.player.skills.Count > cursorPos + firstListed)
                    {
                        cursorPos--;
                        firstListed++;
                    }
                }
                else if (keyinfo.KeyChar >= '1' && keyinfo.KeyChar <= '9')
                {
                    int newPos = int.Parse(keyinfo.KeyChar.ToString()) - 1;
                    if (newPos < Program.player.skills.Count)
                        cursorPos = newPos;
                }
                else if (keyinfo.Key == ConsoleKey.Escape)
                {
                    Program.PlaySound("inv2");
                    Program.nextDisplayed = Program.Screen.Combat;
                    cursorPos = 0;
                }
            }
            else
            {                
                if (Program.player.skills.Count > 0 && Program.player.S >= Program.player.skills[firstListed+cursorPos].Scost)
                {
                    Skill skill = Program.player.skills[firstListed + cursorPos];
                    Program.PlaySound("action_selected");

                    Program.cEnemy.actions.Add(new Skill(skill, Program.cEnemy.selectedMember));
                    Program.player.S -= skill.Scost;

                    Program.nextDisplayed = Program.Screen.Combat;
                    cursorPos = 0;
                }
                else
                {
                    Program.PlaySound("error");
                }
            }
        }
    }
}