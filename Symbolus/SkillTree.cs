using System;
using System.IO;
using System.Text;

namespace Symbolus
{
    /// <summary>
    /// Kezelőfelület.
    /// Képességfa
    /// </summary>
    public class SkillTree
    {
        public int freePoints = Program.player.level; //elkölthető pontok
        private int usedPoints = 0; //foglalt pontok

        public string[] matrix = new string[40]; //karakter-rajz
        public int maxY; //matrix utolsó sora
        public WaveEngine waveEngine;

        private int cursorPosX = 3;
        private int cursorPosY = 0;
        private TextBox description = new TextBox(); //a kijelölt képesség grafikus leírása

        private bool onResetButton = true; //a reset gombon van-e a kurzor


        public SkillTree()
        {
            using (StreamReader r = new StreamReader(@"assets\menu\skill_tree.txt", Encoding.UTF8))
            {
                int i = 0;
                while (!r.EndOfStream)
                {
                    matrix[i] = r.ReadLine();
                    i++;
                }
                maxY = i - 1;
            }

            waveEngine = new WaveEngine(matrix, maxY, maxY / 2);
        }

        /// <summary>
        /// Képességfa kirajzolása
        /// </summary>
        public void Display()
        {
            int y = 0;
            int listed = 0;
            int k = -2;
            description = Skill.allSkills[(cursorPosX + cursorPosY * 3)-1].description;

            Console.Clear();
            char c;
            for (int j = 0; j <= maxY; j++)
            {
                for (int i = 0; i < matrix[0].Length; i++)
                {
                    c = matrix[j][i];

                    switch (c)
                    {
                        case '#':
                            Console.BackgroundColor = ConsoleColor.DarkGray;
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            break;
                        case 'f':
                            Console.BackgroundColor = ConsoleColor.DarkGreen;
                            Console.ForegroundColor = ConsoleColor.Green;
                            c = 'J';
                            break;
                        case '~':
                            Console.BackgroundColor = ConsoleColor.Blue;
                            Console.ForegroundColor = ConsoleColor.DarkBlue;
                            break;
                        case '%':
                            string output = (freePoints + "/" + (freePoints + usedPoints));
                            Console.Write(output);
                            i += output.Length;
                            c = ' ';
                            break;
                        case '*':
                            if (Program.player.skills.Contains(Skill.allSkills[listed]))
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            else
                            {
                                if (listed % 3 == 0)
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                else if (listed % 3 == 1)
                                    Console.ForegroundColor = ConsoleColor.Green;
                                else if (listed % 3 == 2)
                                    Console.ForegroundColor = ConsoleColor.Red;
                            }
                            Console.Write(Skill.allSkills[listed].name);
                            i += Skill.allSkills[listed].name.Length;
                            c = ' ';
                            listed++;
                            break;
                        case '"':
                            if (onResetButton)
                            {
                                Console.BackgroundColor = ConsoleColor.Gray;
                                Console.ForegroundColor = ConsoleColor.Gray;
                            }
                            else
                            {
                                Console.BackgroundColor = ConsoleColor.DarkGray;
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                            }
                            break;

                        default:
                            if (c >= '4' && c <= '9' && Convert.ToInt32(c.ToString()) - 3 + (y * 3) == cursorPosX + cursorPosY * 3 && !onResetButton)
                            {
                                Console.BackgroundColor = ConsoleColor.Gray;
                                Console.ForegroundColor = ConsoleColor.Gray;
                            }
                            else if (c >= '4' && c <= '9')
                            {
                                Console.BackgroundColor = ConsoleColor.DarkGray;
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                            }
                            else if (c >= '2' && c <= '3')
                                y++;
                            else
                            {
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            break;
                    }          

                    Console.Write(c);
                    Console.BackgroundColor = ConsoleColor.Black;
                }

                if (description != null && description.maxY >= j && !onResetButton)
                {
                    Console.Write("  ");
                    description.WriteAtLine(j);
                }
                else if (k < 0)
                {
                    k++;
                }
                else if (k < Program.player.skillList.maxY + 1 && Program.player.skills.Count>0)
                {
                    Console.Write("  ");
                    Program.player.skillList.WriteAtLine(k);
                    k++;
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
            ConsoleKeyInfo keyinfo;
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
            keyinfo = Console.ReadKey(true);

            if (keyinfo.Key != ConsoleKey.Spacebar && keyinfo.Key != ConsoleKey.Enter)
            {
                if (keyinfo.Key == ConsoleKey.W && this.cursorPosY > 0)
                {
                    this.cursorPosY--;
                    Program.PlaySound("menumove");
                }
                else if(keyinfo.Key == ConsoleKey.W)
                {
                    onResetButton = true;
                    cursorPosX = 3;
                    cursorPosY = 0;
                    Program.PlaySound("menumove");
                }
                else if (keyinfo.Key == ConsoleKey.S && this.cursorPosY < 2)
                {
                    if (!onResetButton)
                        this.cursorPosY++;
                    else
                        onResetButton = false;
                    Program.PlaySound("menumove");
                }
                else if (keyinfo.Key == ConsoleKey.A && this.cursorPosX > 1)
                {
                    this.cursorPosX--;
                    Program.PlaySound("menumove");
                }
                else if (keyinfo.Key == ConsoleKey.D && this.cursorPosX < 3)
                {
                    this.cursorPosX++;
                    Program.PlaySound("menumove");
                }
                else if(keyinfo.Key == ConsoleKey.R)
                {
                    Reset();
                }
                else if (keyinfo.Key == ConsoleKey.Escape  || keyinfo.Key == ConsoleKey.T)
                {
                    Program.PlaySound("inv2");
                    Program.nextDisplayed = Program.Screen.Map;
                }
            }
            else
            {
                if (!onResetButton)
                {
                    Skill selected = Skill.allSkills[(cursorPosX + cursorPosY * 3) - 1];

                    if (selected.cost <= freePoints && !Program.player.skills.Contains(selected))
                    {
                        Program.PlaySound("action_selected");
                        Program.player.skills.Add(selected);
                        freePoints -= selected.cost;
                        usedPoints += selected.cost;
                        Program.player.RefreshSkillList();

                        if (Tutorial.progress == 7)
                        {
                            Program.mapScreen.map.enemies.Add(new EnemyGroup(0, 3, "2", 2, "forest", 0));
                            Tutorial.Next();
                        }
                    }
                    else if(Program.player.skills.Contains(selected))
                    {
                        Program.player.skills.Remove(selected);
                        Program.PlaySound("reset");
                        freePoints += selected.cost;
                        usedPoints -= selected.cost;
                    }
                    else
                        Program.PlaySound("error");
                }
                else
                    Reset();                
            }            
        }

        /// <summary>
        /// A lefoglalt képességek feloldása pénzért cserébe
        /// </summary>
        public void Reset()
        {
            Program.PlaySound("select");
            Program.player.skills.Clear();
            freePoints = Program.player.level;
            usedPoints = 0;
            Program.player.RefreshSkillList();
        }
    }
}
