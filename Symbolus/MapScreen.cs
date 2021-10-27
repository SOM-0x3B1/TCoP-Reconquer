using System;
using System.Linq;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Media;
using System.Runtime.InteropServices;

namespace Symbolus
{
    /// <summary>
    /// Kezelőfelület (térkép + mobilis objektumok).
    /// A felülnézetes játék.
    /// </summary>
    public class MapScreen
    {
        public Map map;

        /// <summary>
        /// Kezelőfelület kirajzolása, majd utasítás várása
        /// </summary>
        public void Display()
        {
            Console.Clear();
            char c;
            int n = 0;
            for (int j = 0; j < map.maxY; j++) //térkép
            {
                for (int i = 0; i < Map.centerSpace.Length; i++)
                {
                    if (j >= Program.stats.Length || i >= Program.stats[0].Length)
                    {
                        Console.Write(Map.centerSpace[i]);
                    }
                    else
                    {
                        c = Program.stats[j][i];
                        if (c == '1')
                        {
                            int k = 0;
                            string hp = Program.player.HP.ToString();
                            int hplen = hp.ToString().Length;
                            for (k = 0; k < (8 - hplen); k++)
                            {
                                hp += ' ';
                            }
                            double scale = (double)Program.player.HP / Program.player.MaxHP;
                            k = 0;
                            for (; k < Math.Floor(scale * 8); k++)
                            {
                                Console.BackgroundColor = ConsoleColor.DarkRed;
                                Console.Write(hp[k]);
                            }
                            Console.BackgroundColor = ConsoleColor.Red;
                            for (; k < hp.Length; k++)
                            {
                                Console.Write(hp[k]);
                            }
                            i += hp.Length - 1;
                            Console.BackgroundColor = ConsoleColor.Black;
                        }
                        else if (c == '2')
                        {
                            int k = 0;
                            string s = Program.player.S.ToString();
                            int slen = s.ToString().Length;
                            for (k = 0; k < (8 - slen); k++)
                            {
                                s += ' ';
                            }
                            double scale = (double)Program.player.S / Program.player.MaxS;
                            k = 0;
                            for (; k < Math.Floor(scale * 8); k++)
                            {
                                Console.BackgroundColor = ConsoleColor.DarkBlue;
                                Console.Write(s[k]);
                            }
                            Console.BackgroundColor = ConsoleColor.Blue;
                            for (; k < s.Length; k++)
                            {
                                Console.Write(s[k]);
                            }
                            i += s.Length - 1;
                            Console.BackgroundColor = ConsoleColor.Black;
                        }
                        else if (c == '3')
                        {
                            int k = 0;
                            string s;
                            if (Program.player.sprint)
                                s = " Futás";
                            else
                                s = " Séta";

                            int slen = s.ToString().Length;
                            for (k = 0; k < s.Length; k++)
                            {
                                Console.Write(s[k]);
                            }

                            i += s.Length-1;
                            Console.BackgroundColor = ConsoleColor.Black;
                        }
                        else
                        {
                            if (c == '#')
                            {
                                Console.BackgroundColor = ConsoleColor.Gray;
                                Console.ForegroundColor = ConsoleColor.Gray;
                            }
                            else
                            {
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            Console.Write(c);
                        }
                    }
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                for (int i = 0; i < map.matrix[0].Length; i++)
                {

                    c = map.matrix[j][i];
                    switch (c)
                    {
                        case '#':
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            break;
                        case '-':
                            Console.BackgroundColor = ConsoleColor.DarkGray;
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        case (char)39:
                            Console.BackgroundColor = ConsoleColor.DarkGray;
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        case '~':
                            Console.BackgroundColor = ConsoleColor.Blue;
                            Console.ForegroundColor = ConsoleColor.DarkBlue;
                            break;
                        case 'B':
                            Console.BackgroundColor = ConsoleColor.DarkGreen;
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            break;
                        case 'f':
                            Console.BackgroundColor = ConsoleColor.Yellow;
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            break;
                        case 'b':
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        case 'G':
                            Console.BackgroundColor = ConsoleColor.DarkGray;
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            break;
                        case 't':
                            Console.BackgroundColor = ConsoleColor.White;
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        default:
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                    }

                    if (i == Program.player.x && j == Program.player.y) //játékos
                    {
                        if (Tutorial.progress > 0 || Tutorial.textBoxIndex > 1)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write((char)3);
                        }
                        else
                            Console.Write(c);
                    }

                    #region Bűn
                    else if (map.enemies.Any(a => a.x == i && a.y == j)) //ellenség
                    {
                        if (Tutorial.progress > 0 || Tutorial.textBoxIndex > 1)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.Write((char)30);
                        }
                        else
                            Console.Write(c);
                    }
                    else if (map.npcs.Any(a => a.x == i && a.y == j && a.person)) //személy npc
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write((char)31);
                    }
                    else if (map.npcs.Any(a => a.x == i && a.y == j && !a.person)) //tárgy npc
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.Write((char)4);
                    }
                    #endregion

                    else //nem mobilis objektum
                        Console.Write(c);
                }

                Console.BackgroundColor = ConsoleColor.Black;


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


                int x = Tutorial.progress;
                if (x != 2 && j > 7 && map.cDialogIndex == map.cutsceneDialogs.Count && !Tutorial.completed)
                {
                    int y = Tutorial.textBoxIndex;
                    if (y < Tutorial.listOfTextBoxes[x].Count)
                    {
                        TextBox tut = Tutorial.listOfTextBoxes[x][y];
                        if (n < tut.maxY + 1)
                        {
                            Console.Write("   ");
                            tut.WriteAtLine(n);
                            n++;
                        }
                    }
                }

                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine();
            }


            Console.WriteLine();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            if (map.cDialogIndex != map.cutsceneDialogs.Count) 
            {
                Console.Write(Map.smallCenterSpace);
                foreach (char ch in map.cutsceneDialogs[map.cDialogIndex])
                {
                    if (ch == '/')
                    {                        
                        Console.Write("\n");
                        Console.Write(Map.smallCenterSpace);
                    }
                    else
                        Console.Write(ch);
                    Thread.Sleep(5);
                }
                map.cDialogIndex++;

                while (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                }
                Console.ReadKey(true);
                Program.PlaySound("action_selected");
            }
            else
            {
                Controls();
            }
            Console.BackgroundColor = ConsoleColor.Black;
        }


        /// <summary>
        /// Néhány pixel frissítése a consol-ban.
        /// </summary>
        /// <param name="positions">Frissítendő pozíciók</param>
        public void RefreshPixels(List<Position> positions)
        {
            int lastx = Console.CursorLeft;
            int lasty = Console.CursorTop;
            foreach (Position pos in positions)
            {
                int x = pos.x;
                int y = pos.y;
                char c = map.matrix[y][x];
                Console.SetCursorPosition(x+Map.centerSpaceLength, y);
                switch (c)
                {
                    case '#':
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                    case '-':
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case (char)39:
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case '~':
                        Console.BackgroundColor = ConsoleColor.Blue;
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        break;
                    case 'B':
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        break;
                    case 'f':
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        break;
                    case 'b':
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case 'G':
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                    case 't':
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    default:
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }

                if (x == Program.player.x && y == Program.player.y) //játékos
                {
                    if (Tutorial.progress > 0 || Tutorial.textBoxIndex > 1)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write((char)3);
                    }
                    else
                        Console.Write(c);
                }
                #region Bűn
                else if (map.enemies.Any(a => a.x == x && a.y == y)) //ellenség
                {
                    if (Tutorial.progress > 0 || Tutorial.textBoxIndex > 1)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write((char)30);
                    }
                    else
                        Console.Write(c);
                }
                else if (map.npcs.Any(a => a.x == x && a.y == y && a.person)) //személy npc
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write((char)31);
                }
                else if (map.npcs.Any(a => a.x == x && a.y == y && !a.person)) //tárgy npc
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write((char)4);
                }
                #endregion
                else //nem mobilis objektum
                    Console.Write(c);

                Console.BackgroundColor = ConsoleColor.Black;
            }

            char c2;
            for (int j = 0; j < Program.stats.Length; j ++)
            {
                for (int i = 0; i < Program.stats[j].Length; i++)
                {
                    Console.SetCursorPosition(i, j);
                    c2 = Program.stats[j][i];

                    if (c2 == '1')
                    {
                        int k = 0;
                        string hp = Program.player.HP.ToString();
                        int hplen = hp.ToString().Length;
                        for (k = 0; k < (8 - hplen); k++)
                        {
                            hp += ' ';
                        }
                        double scale = (double)Program.player.HP / Program.player.MaxHP;
                        k = 0;
                        for (; k < Math.Floor(scale * 8); k++)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                            Console.Write(hp[k]);
                        }
                        Console.BackgroundColor = ConsoleColor.Red;
                        for (; k < hp.Length; k++)
                        {
                            Console.Write(hp[k]);
                        }
                        i += hp.Length - 1;
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    else if (c2 == '2')
                    {
                        int k = 0;
                        string s = Program.player.S.ToString();
                        int slen = s.ToString().Length;
                        for (k = 0; k < (8 - slen); k++)
                        {
                            s += ' ';
                        }
                        double scale = (double)Program.player.S / Program.player.MaxS;
                        k = 0;
                        for (; k < Math.Floor(scale * 8); k++)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                            Console.Write(s[k]);
                        }
                        Console.BackgroundColor = ConsoleColor.Blue;
                        for (; k < s.Length; k++)
                        {
                            Console.Write(s[k]);
                        }
                        i += s.Length - 1;
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    else if (c2 == '3')
                    {
                        int k = 0;
                        string s;
                        if (Program.player.sprint)
                            s = " Futás";
                        else
                            s = " Séta";

                        int slen = s.ToString().Length;
                        for (k = 0; k < s.Length; k++)
                        {
                            Console.Write(s[k]);
                        }

                        i += s.Length - 1;
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    else
                    {
                        if (c2 == '#')
                        {
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        else
                        {
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        Console.Write(c2);
                    }
                }
                Console.BackgroundColor = ConsoleColor.Black;
            }

            Program.pixelsToRefresh.Clear();
            Controls();
        }

        /// <summary>
        /// Új Map betöltése
        /// </summary>
        /// <param name="name">A Gate által megadott cél</param>
        public void LoadMap(string name)
        {
            if (!Map.maps.ContainsKey(name)) //teljesen új
            {
                this.map = new Map(name);
                Map.maps.Add(name, this.map);
            }
            else //betöltve
            {
                this.map = Map.maps[name];
            }

            if (map.music != Program.cmusic && Program.musicOn)
            {
                if (!Program.newGame)
                {
                    Program.musics[Program.cmusic].Stop();
                    Program.musics[map.music].PlayLooping();
                    Program.cmusic = map.music;
                }
            }
            Program.mapRefresh = false;
        }


        /// <summary>
        /// Utasítás várása, fogadása és végrehajtása
        /// </summary>
        private void Controls()
        {
            Thread.Sleep(100);

            ConsoleKeyInfo keyinfo;
            while (Console.KeyAvailable)
            {
                keyinfo = Console.ReadKey(true);
            }
            keyinfo = Console.ReadKey(true);

            if (Tutorial.completed || (Tutorial.textBoxIndex > Tutorial.listOfTextBoxes[Tutorial.progress].Count - 2 && Tutorial.progress != 9))
            {
                if (keyinfo.Key != ConsoleKey.Spacebar && keyinfo.Key != ConsoleKey.Escape && keyinfo.Key != ConsoleKey.E && keyinfo.Key != ConsoleKey.T)
                {
                    #region specialstepsound
                    /*char c = Program.screen.map.matrix[Program.player.y][Program.player.x];
                    if (c == '-' || c == (char)39)
                    {
                        Program.PlaySound("step2");
                    }
                    else if (c == ' ')
                    {*/
                    #endregion
                    if (keyinfo.Key == ConsoleKey.W || keyinfo.Key == ConsoleKey.A || keyinfo.Key == ConsoleKey.S || keyinfo.Key == ConsoleKey.D)
                    {
                        Program.pixelsToRefresh.Add(new Position(Program.player.x, Program.player.y));
                        Program.mapRefresh = true;

                        Program.player.move(keyinfo);
                        foreach (EnemyGroup enemy in map.enemies)
                            enemy.MoveOnMap(map.enemies);

                        if (!Program.player.InteractionCheck() && Program.player.sprint) //itt a duplalépést hajtom végre; megvizsgálom, hogy végre kell-e hajtani, illetve lefuttatom az interakció keresőt
                        {
                            Program.player.move(keyinfo);
                            Program.player.S -= 2;
                            Program.player.InteractionCheck();
                        }

                        Program.pixelsToRefresh.Add(new Position(Program.player.x, Program.player.y));
                        Program.PlaySound("step");
                    }
                }
                else if (keyinfo.Key == ConsoleKey.Spacebar)
                {
                    Program.player.sprint = !Program.player.sprint;
                    if (Program.player.sprint)
                        Program.PlaySound("faster");
                    else
                        Program.PlaySound("slower");
                    Program.mapRefresh = true;
                }
                else if (keyinfo.Key == ConsoleKey.Escape)
                {
                    Program.PlaySound("pause");
                    Program.menu.backFromPause = true;
                    Program.menu.menuPage = new MenuPage("pause");
                    Program.nextDisplayed = Program.Screen.Menu;
                    Program.mapRefresh = false;
                }
                else if (keyinfo.Key == ConsoleKey.E)
                {
                    Program.PlaySound("inv1");
                    Program.nextDisplayed = Program.Screen.Inventory;
                    Program.mapRefresh = false;
                }
                else if (keyinfo.Key == ConsoleKey.T)
                {
                    Program.PlaySound("inv1");
                    Program.nextDisplayed = Program.Screen.SkillTree;
                    Program.mapRefresh = false;
                }
            }
            else
            {
                if (keyinfo.Key == ConsoleKey.Enter)
                {
                    Program.PlaySound("action_selected");
                    Tutorial.textBoxIndex++;
                    Program.mapRefresh = false;

                    if (Tutorial.textBoxIndex > Tutorial.listOfTextBoxes[Tutorial.progress].Count - 1)
                        Tutorial.completed = true;
                }
                else if (keyinfo.Key == ConsoleKey.Escape)
                {
                    Program.PlaySound("pause");
                    Program.menu.backFromPause = true;
                    Program.menu.menuPage = new MenuPage("pause");
                    Program.nextDisplayed = Program.Screen.Menu;
                    Program.mapRefresh = false;
                }
                else
                {
                    Program.PlaySound("blocked");
                    Program.mapRefresh = true;
                }
            }
        }
    }
}
