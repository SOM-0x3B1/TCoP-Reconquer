using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Threading;

namespace Symbolus
{
    /// <summary>
    /// Aktív objektum kezelőfelülettel ellátva. Dilaógusok, küldetések és eszközök adására szolgál.
    /// Interakció a játékos oldalán lévő karakterekkel.
    /// </summary>
    public class NPC : MobileObject
    {
        private string name;
        public bool person = false;

        private string[] matrix = new string[40];
        private int maxY;
        private Background background; //az npc-hez tartozó háttér
        private int currentDialog = 0; //jelenlegi dialógus
        private List<Dialog> dialogs = new List<Dialog>(); //dialógusok
        private int cursorPos = 0; //a "kurzor" pozíciója

        private int delay = 15; //szöveg kirajzolásának gyorsasága

        public Shop shop;

        #region Folyamatban...
        //public static bool flowStopped = false; // aszöveg kirajzolásának vége?
        //public static ConsoleKey key = ConsoleKey.NoName;
        //private Thread skipChecker = new Thread(SkipChecker);
        #endregion


        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="name">NPC neve</param>
        /// <param name="x">Pozíció, oszlop</param>
        /// <param name="y">Pozíció, sor</param>
        public NPC(string name, int x, int y, int person)
        {
            this.name = name;
            this.x = x;
            this.y = y;

            if(person == 1)            
                this.person = true;            

            using (StreamReader r = new StreamReader(@"assets\characters\" + name + ".txt", Encoding.UTF8)) //karakter (személy) rajzának betöltése
            {
                string line = r.ReadLine();
                int i = 0;
                while (line[0] != '=')
                {
                    matrix[i] = line;
                    i++;
                    line = r.ReadLine();
                }
                maxY = i;
                background = Background.backgrounds[r.ReadLine()]; //háttér hozzárendelése

                while (!r.EndOfStream) //dialógusok beolvasása
                {
                    string[] input = r.ReadLine().Split(';');

                    dialogs.Add(new Dialog(input));
                }
                //NPCs.Add(name, this);
            }

            try
            {
                this.shop = new Shop(name);
            }
            catch { }
        }


        /// <summary>
        /// Kezelőfelület kirajzolása, utasítás várása
        /// </summary>
        public void Display()
        {
            Console.Clear();
            char c;
            for (int j = 0; j <= background.maxY; j++)
            {
                Console.Write(Map.centerSpace);
                for (int i = 0; i < background.matrix[0].Length; i++)
                {
                    if(this.matrix[j][i] == ' ')
                        c = background.matrix[j][i];
                    else
                        c = this.matrix[j][i];

                    switch (c)
                    {
                        case '#':
                            Console.BackgroundColor = ConsoleColor.DarkGray;
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            break;
                        case 'B':
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                            Console.ForegroundColor = ConsoleColor.DarkBlue;
                            break;
                        case 'P':
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        case 'p':
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            break;
                        case 'S':
                            Console.BackgroundColor = ConsoleColor.Yellow;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                        case 's':
                            Console.BackgroundColor = ConsoleColor.DarkYellow;
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            break;
                        case 'z':
                            Console.BackgroundColor = ConsoleColor.DarkGreen;
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            break;
                        case 'Z':
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.ForegroundColor = ConsoleColor.Green;
                            break;
                        case 'K':
                            Console.BackgroundColor = ConsoleColor.Blue;
                            Console.ForegroundColor = ConsoleColor.Blue;
                            break;
                        case 'f':
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.Black;
                            break;
                        case 'F':
                            Console.BackgroundColor = ConsoleColor.White;
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        case 'G':
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.ForegroundColor = ConsoleColor.Gray;
                            break;
                        case 'g':
                            Console.BackgroundColor = ConsoleColor.DarkGray;
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            break;
                    }

                    Console.Write(c);                    
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                Console.WriteLine();
            }
            Console.WriteLine();


            Console.ForegroundColor = ConsoleColor.White;
            //int steps = 0;

            if (Program.NPCRefresh)
            {
                //skipChecker.Start();
                /*foreach (char c2 in dialogs[currentDialog].message)
                {

                    if (c2 == '/')
                        Console.Write("\n");
                    else
                        Console.Write(c2);

                    Thread.Sleep(delay);
                    //steps++;
                }*/
                //steps = 0;
                foreach (string line in dialogs[currentDialog].message)
                {
                    for (int i = 0; i < Program.width / 2 - line.Length / 2 - 6; i++)
                        Console.Write(' ');

                    foreach (char ch in line)
                    {
                        Console.Write(ch);
                        Thread.Sleep(5);
                    }
                    Console.Write("\n");
                }
            }
            else
            {
                /*foreach (char c2 in dialogs[currentDialog].message)
                {
                    if (c2 == '/')
                        Console.Write("\n");
                    else
                        Console.Write(c2);
                }*/
                foreach (string line in dialogs[currentDialog].message)
                {
                    for (int i = 0; i < Program.width / 2 - line.Length / 2 - 6; i++)
                        Console.Write(' ');

                    foreach (char ch in line)
                        Console.Write(ch);

                    Console.Write("\n");
                }
            }
            Console.WriteLine();

            if (dialogs[currentDialog].choise)
            {
                Console.WriteLine();
                for (int i = 0; i < dialogs[currentDialog].choises.Count; i++)
                {
                    if (i == cursorPos)
                        Console.ForegroundColor = ConsoleColor.White;
                    else
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"[{dialogs[currentDialog].choises[i]}]");
                }
            }
            else
            {
                for (int i = 0; i < Program.width / 2 - 6; i++)
                    Console.Write(' ');
                Console.WriteLine("...");
            }


            /*skipChecker.Join();
            skipChecker = new Thread(SkipChecker);
            delay = 30;*/

            Controls();
        }


        /// <summary>
        /// Utasítás várása, fogadása, végrehajtása.
        /// </summary>
        private void Controls()
        {

            /*if (key == ConsoleKey.Enter)
            {*/
            ConsoleKeyInfo keyinfo = new ConsoleKeyInfo();

            while (Console.KeyAvailable)
            {
                keyinfo = Console.ReadKey(true);
            }
            keyinfo = Console.ReadKey(true);

            if (keyinfo.Key != ConsoleKey.Spacebar && keyinfo.Key != ConsoleKey.Enter)
            {
                if (keyinfo.Key == ConsoleKey.W && cursorPos > 0)
                {
                    cursorPos--;
                    Program.PlaySound("menumove");
                }
                else if (keyinfo.Key == ConsoleKey.S && cursorPos < dialogs[currentDialog].choises.Count - 1)
                {
                    cursorPos++;
                    Program.PlaySound("menumove");
                }
                Program.nextDisplayed = Program.Screen.NPC;
                Program.NPCRefresh = false;
            }
            else
            {
                Program.PlaySound("select");

                if (!dialogs[currentDialog].choise)
                {
                    if (dialogs[currentDialog].destinations[0] == -1)
                    {
                        Program.nextDisplayed = Program.Screen.Map;
                    }
                    else
                    {
                        currentDialog = dialogs[currentDialog].destinations[0];
                    }
                }
                else
                {
                    if (dialogs[currentDialog].destinations[cursorPos] == -1)
                    {
                        Program.nextDisplayed = Program.Screen.Map;
                    }
                    else if (dialogs[currentDialog].destinations[cursorPos] == -2)
                    {
                        if (Tutorial.progress > 2)
                        {
                            Program.nextDisplayed = Program.Screen.Shop;

                            Program.PlaySound("shop");
                            if (Program.musicOn)
                            {
                                Program.musics[Program.cmusic].Stop();
                                Program.cmusic = "shop";
                                Program.musics["shop"].PlayLooping();
                            }
                        }
                        else
                            Program.PlaySound("error");
                    }
                    else
                    {
                        currentDialog = dialogs[currentDialog].destinations[cursorPos];
                    }                    
                }               
                
                Program.NPCRefresh = true;
            }

            #region Folyamatban...
            /*else
            {
                Space();
            }
        }
        else
        {
            key = ConsoleKey.NoName;
            Space();                
        }


        void Space()
        {
            Program.PlaySound("select");

            if (!dialogs[currentDialog].choise)
            {
                currentDialog = dialogs[currentDialog].destinations[0];
            }
            else
            {
                currentDialog = dialogs[currentDialog].destinations[cursorPos];
            }

            if (currentDialog == -1)
            {
                currentDialog = 0;
                Program.nextDisplayed = "game";
                Program.refresh = true;
            }
        }*/
            #endregion
        }

        #region Folyamatban... (skip funkció)
        /*protected static void SkipChecker()
        {
            ConsoleKeyInfo keyinfo;
            while (Console.KeyAvailable)
            {
                keyinfo = Console.ReadKey(true);
            }
            keyinfo = Console.ReadKey(true);

            if (keyinfo.Key == ConsoleKey.Spacebar)
            {
                Program.cNPC.delay = 0;                
            }

            if (!flowStopped)
            {
                key = ConsoleKey.Enter;
            }
            else
            {
                key = ConsoleKey.NoName;
            }
        }*/
        #endregion 
    }


    /// <summary>
    /// NPC-knek szánt beszédrészletek választásokkal
    /// </summary>
    public class Dialog
    {
        public bool choise; //választásos-e
        public int index;
        public string[] message; //szöveg
        public List<string> choises = new List<string>(); //választások
        public List<int> destinations = new List<int>(); //következő dialógusok indexei

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="input">beolvasott sor</param>
        public Dialog(string[] input)
        {
            index = int.Parse(input[0]);
            string[] msg = input[1].Split('%');
            if (msg.Length > 1)
            {
                choise = true;
                message = msg[0].Split('/');
                foreach(string choise in msg[1].Split('/'))
                {
                    choises.Add(choise);
                }
                foreach (string dest in input[2].Split(','))
                {
                    destinations.Add(int.Parse(dest));
                }
            }
            else
            {
                choise = false;
                message = input[1].Split('/');
                destinations.Add(int.Parse(input[2]));
            }
        }
    }
}