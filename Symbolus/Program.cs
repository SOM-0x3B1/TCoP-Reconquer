using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Media;
using System.Runtime.InteropServices;

/// <summary> 
/// Pusztaszentistván Krónikái:
///     Pusztaszentistvánt (kisfalu) a Mendéről érkező suttyók megtámadják, és elfoglalják.
///     A játékos a játék készítőjének háza előtt találja magát, célként a falu felszabadítását kapja.
///     A faluban küldetéseket és itemeket kap a lakosoktól.
///     A falun kívül ellenségekkel (suttyókkal) kell leszámolnia, illetve akadályokat kell áttörnie.
///     A játék végén 5 (bad, neutral, default, secret, true) ending várja, amiket a döntéseivel, teljesítményével érhet el.
///     
/// Class-ok előfordulása (példa):
///     1. A 'Program' betölti az alapvető játékelemeket ==> pl. a 'Player'-t (, aki egy 'MobileObject'):
///         a) Megkapja az 'Inventory'-ját 
///     2. A 'Menu' megjeleníti a "main" menüoldalt
///     3. A start után betölt a 'MapScreen' ==> betölt egy új 'Map'-ot:
///         a) A 'Player' megkapja kezdőpozícióját
///         b) Betöltődnek a 'Gate'-ek 
///         c) Betöltődnek az NPC-k ==> 'Background'-ot kapnak
///         d) Betöltődnek az 'EnemyGroup'-ok
///     4. A játékos átlép egy másik pályára ==> új térkép, új 'MobileObject'-ek
///     5. Előhívja a PAUSE menüt ('Menu')
///     6. Előhívja az 'Inventory'-t, az 'Equipment'-ek között böngészik
///         a) Vehet 'Weapon'-t, 'Armor'-t és 'Potion'-t
///         b) Az kiválaszott 'Equipment' 'TextBox'-os leírása oldalt megjelenik.
///     7. Előhívja a 'SkillTree'-t, ahol a szintjeiből kapott pontokért választhat 'Skill'-eket
///     8. Beszél egy NPC-vel
///         a) Betöltődik az NPC rajza
///         b) Betöltődik a hozzá tartozó 'Background'
///         c) Betöltődnek a dialógusok és választási lehetőségek
///         d) Ha az NPC rendelkezik 'Shop'-pal, a játékos vehet tőle felszerelést
///     9. Ütközik egy 'EnemyGroup'-pal
///         a) Betöltődnek a csoportban lévő 'EnemyMember'-ek ==> 'Background'-ot kapnak
///         b) A játékos átlép a combat ciklusba, 'Action'-öket választhat a kijelölt ellefelekre, vagy magára
///         c) Az szintekkel megnyitott 'Skill'-eket 'Action'-ök formájában, Stamina árán használhatja                | Ezek 'Effect'-eket adhatnak a játékoshoz,
///         d) Az 'Inventory'-ban harc közben is lecseréheti páncélját, fegyverét; 'Potion'-öket ('Equipmnet') ihat   | vagy az ellenfelekhez; a lépések előtt játszódnak le.
///         e) Egyes események végrehajtása után 'Sticker'-eket kapnak az ellenfelek
///         f) Legyőzi az ellenfelet ==> törlődik az ellenfél; vagy veszít ==> újratöltődik az adott pálya
///     
/// Irányítás:
///     1. Általános:
///         - W,A,S,D ==> fel, balra, le, jobbra
///         - ESC ==> vissza
///         - Space ==> kiválasztás
///     2. Felülnézetes játék:
///         - Space ==> séta/sprint
///         - E ==> eszköztár megnyitása, majd bezárása
///         - T ==> képességfa megnyitása, majd bezárása
///     3. Harc:
///         - TAB ==> ellefelek közti váltás
///         - 1-6 ==> alap cselekedetek shotrcutjai
///         
/// Hiányosságok:
///     - K.O. után fura effekt-clear
///     - Ellenfél ütközési sorrend
///     - Ellenfél képességei
///     - Potionok árának kiírása
///     - 1 körös effektek listájának eltűnése
///     - random sebzés
///     - pajzs
///     - progress
///     - abilities maction menu is broken
///     - add external dialoge jumps
///     - enemy doesn't load dead sprite after retry
///     - shield/armor?
///     - rewrite enemy ai
///     - add wave effect to actionmenu
///     - remaster npc dialoge
///     
/// </summary>


namespace Symbolus
{
    /// <summary>
    /// A globális, alapvető elemek nagy része itt található; a fő ciklus is itt fut.
    /// </summary>
    public class Program
    {
        //eszközök
        [STAThread]
        [DllImport("winmm.dll")]
        static extern int mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);
        public static Random rnd = new Random();
        public static int width = 120;
        //alap
        public static Dictionary<string,string> settings = new Dictionary<string, string>();
        public static string assetPath = "HU";
        public static Player player = new Player(10, 2); //mobilis objektum (pozícióval rendelkezik)
        public static MapScreen mapScreen = new MapScreen(); //kezelőfelület (térkép + mobilis objektumok)
        public static Menu menu = new Menu(); //kezelőfelület (gombok)
        public static WinScreen winScreen = new WinScreen();
        public static BasicActionMenu basicActionMenu = new BasicActionMenu();
        public static SkillActionMenu skillActionMenu = new SkillActionMenu();
        public static SkillTree skillTree = new SkillTree();
        //rajzok
        public static string[] title = new string[22]; //Cím
        public static string[] stats = new string[7]; //HP és stamina sáv
        public static string[] combatStats = new string[3]; //harci HP és stamina sáv
        public static string[] fightmenu = new string[3]; //HP és stamina sáv
        public static string[] koEnemy = new string[40]; //kiütött ellenfél
        public static string[] actionSlots = new string[3]; //cselekvés-foglalatok
        public static string[] wallet = new string[7];
        //átmeneti kezelőfelületes objektumok
        public static NPC cNPC; //az NPC, akivel jelenleg beszélünk
        public static EnemyGroup cEnemy; //az ellenség, akivel jelenleg harcolunk
        //hangok és zene
        public static string cmusic = "main_menu"; //aktuális zene
        public static bool musicOn = true;
        public static bool soundOn = true;
        public static Dictionary<string, SoundPlayer> musics = new Dictionary<string, SoundPlayer>(); //zenék
        //játékparaméterek
        public static bool newGame = false; //új játék?
        public static bool NPCRefresh = true;
        public static bool mapRefresh = false;//NPC-nél gombváltás-e (igen -> nem beszél; nem -> beszél)
        public static bool combat = false; //folyamatban van-e harc?        
        public static List<Position> pixelsToRefresh = new List<Position>();
        public enum Screen { Menu, Map, NPC, Combat, Inventory, ActionMenu, SkillMenu, WinScreen, Shop, SkillTree }
        public static Screen nextDisplayed = Screen.Menu; //következő képernyő


        /// <summary>
        /// Hangok (nem zenék) lejátszása
        /// </summary>
        /// <param name="sound">A .wav fájl neve</param>


        public static void PlaySound(string sound)
        {
            if (soundOn)
                PlaySND(sound);
        }
        public static void PlaySound(string sound, bool bypass)
        {
            if (bypass)
                PlaySND(sound);
        }
        private static void PlaySND(string sound)
        {
            try
            {
                mciSendString("close " + sound, null, 0, IntPtr.Zero);
                mciSendString($@"open assets\sfx\{sound}.wav type waveaudio alias " + sound, new StringBuilder(), 0, IntPtr.Zero);
                mciSendString("play " + sound, null, 0, IntPtr.Zero);
            }
            catch { }
        }

        #region indk2
        /*public static ConsoleKey ReadInput()
        {
            ConsoleKey key;
            while (Console.KeyAvailable)
            {
                key = Console.ReadKey(true).Key;
            }
            key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    key = ConsoleKey.W;
                    break;
                case ConsoleKey.LeftArrow:
                    key = ConsoleKey.A;
                    break;
                case ConsoleKey.DownArrow:
                    key = ConsoleKey.S;
                    break;
                case ConsoleKey.RightArrow:
                    key = ConsoleKey.D;
                    break;
            }

            return key;
        }*/
        #endregion


        static void Main(string[] args)
        {
            Console.Title = "The Chronicles of Pusztaszentistván";

            Console.OutputEncoding = Encoding.Unicode;
            Console.CursorVisible = false;
            //Console.ReadLine();

            #region Előkészületek

            string[] rawSettings = File.ReadAllLines(@"settings.txt", Encoding.UTF8);
            string[] param;
            for (int i = 0; i<rawSettings.Length; i++)
            {
                param = rawSettings[i].Split('=');
                switch (param[0])
                {
                    case "language":
                        assetPath = param[1];
                        break;
                    case "music":
                        musicOn = bool.Parse(param[1]);
                        break;
                    case "sound":
                        soundOn = bool.Parse(param[1]);
                        break;
                }
                settings.Add(param[0],param[1]);
            }
            
            //Cím betöltése
            using (StreamReader r = new StreamReader(@"assets\title.txt", Encoding.UTF8))
            {
                int i = 0;
                while (!r.EndOfStream)
                {
                    title[i] = r.ReadLine();
                    i++;
                }
            }
            //Tutorial lépések betöltése
            using (StreamReader r = new StreamReader(@"assets\tutorial.txt", Encoding.UTF8))
            {
                string[] line;
                while (!r.EndOfStream)
                {
                    List<TextBox> textBoxes = new List<TextBox>();
                    line = r.ReadLine().Split('/');
                    foreach (string text in line)
                        textBoxes.Add(new TextBox(text, "Tutorial"));
                    Tutorial.listOfTextBoxes.Add(new List<TextBox>(textBoxes));
                }
            }
            //Hátterek betöltése
            using (StreamReader r = new StreamReader(@"assets\bg\backgrounds.txt", Encoding.UTF8))
            {
                while (!r.EndOfStream)
                {
                    string line = r.ReadLine();
                    new Background(line);
                }
            }
            //Életerő és stamina sáv betöltése
            using (StreamReader r = new StreamReader(@"assets\screen_modules\stats.txt", Encoding.UTF8))
            {
                int i = 0;
                while (!r.EndOfStream)
                {
                    stats[i] = r.ReadLine();
                    i++;
                }
            }
            using (StreamReader r = new StreamReader(@"assets\screen_modules\combat_stats.txt", Encoding.UTF8))
            {
                int i = 0;
                while (!r.EndOfStream)
                {
                    combatStats[i] = r.ReadLine();
                    i++;
                }
            }
            //"Pénztárca"
            using (StreamReader r = new StreamReader(@"assets\screen_modules\wallet.txt", Encoding.UTF8))
            {
                int i = 0;
                while (!r.EndOfStream)
                {
                    wallet[i] = r.ReadLine();
                    i++;
                }
            }
            //Harci opciók betöltése
            using (StreamReader r = new StreamReader(@"assets\screen_modules\fight_menu.txt", Encoding.UTF8))
            {
                int i = 0;
                while (!r.EndOfStream)
                {
                    fightmenu[i] = r.ReadLine();
                    i++;
                }
            }
            //Kiütött ellenfél rajzának betöltése
            using (StreamReader r = new StreamReader(@"assets\enemies\ko.txt", Encoding.UTF8))
            {
                string line = r.ReadLine();
                int i = 0;
                while (!r.EndOfStream)
                {
                    koEnemy[i] = line;
                    i++;
                    line = r.ReadLine();
                }
                koEnemy[i] = line;
            }
            //Akciófoglalatok rajzának betöltése
            using (StreamReader r = new StreamReader(@"assets\screen_modules\action_slots.txt", Encoding.UTF8))
            {
                int i = 0;
                while (!r.EndOfStream)
                {
                    actionSlots[i] = r.ReadLine();
                    i++;
                }
            }
            //Akciók listájának betöltése
            using (StreamReader r = new StreamReader(@"assets\actions.txt", Encoding.UTF8))
            {
                int i = 0;
                while (!r.EndOfStream)
                {
                    string[] s = r.ReadLine().Split(';');
                    new BasicAction(s[0], s[1], s[2]);
                    i++;
                }
            }
            //Képességek betöltése
            using (StreamReader r = new StreamReader(@"assets\skills.txt", Encoding.UTF8))
            {
                int i = 0;
                while (!r.EndOfStream)
                {
                    string[] s = r.ReadLine().Split(';');
                    Skill.allSkills.Add(new Skill(s[0], int.Parse(s[1]), int.Parse(s[2]), s[3], int.Parse(s[4]), int.Parse(s[5]), s[6]));
                    i++;
                }
            }
            //Zenék betöltése
            using (StreamReader r = new StreamReader(@"assets\music\music.txt", Encoding.UTF8))
            {
                while (!r.EndOfStream)
                {
                    string name = r.ReadLine();
                    musics.Add(name, new SoundPlayer($@"assets\music\{name}.wav"));
                }
            }
            #endregion

            #region Teszt
            /*//Eszköztár feltöltése (kíséleti)
            #region Feltöltés
            player.inventory.weapons.Add(new Weapon("Bot", "normal", 5, "Mindenhol ott van. Mindenre használható. Midnenki szereti."));
            player.inventory.weapons.Add(new Weapon("Seprű", "good", 10, "Ha macskák ellen beválik, a suttyók ellen miért ne működne?"));            
            player.inventory.weapons.Add(new Weapon("Csúzli", "rare", 30, "Lufikról gyakran lepattan. Suttyókról soha."));
            player.inventory.weapons.Add(new Weapon("Farönk", "epic", 70, "A bot szteroidos nagybátyja. Multifunkcionális."));
            player.inventory.weapons.Add(new Weapon("Fűnyíró", "legendary", 100, "Két halálos penge. 3000 rpm. A tökéletes eszköz a suttyótlanításra."));

            player.inventory.armors.Add(new Armor("Pulóver", "normal", 10, "Talán a meggymagok lepatannak róla."));
            player.inventory.armors.Add(new Armor("Farönk", "rare", 20, "Mondtam, hogy multifunkcionális ;)."));

            player.inventory.potions.Add("Paprikapálinka", new Potion("Paprikapálinka", "Felfrissít.", "stamina1", 1, 50));
            player.inventory.potions.Add("Gyógyfű", new Potion("Gyógyfű", "Biztos segít...", "heal1", 1, 30));
            #endregion*/
            #endregion


            Thread.Sleep(1000); //Az előkészületek után valamiért időt kell hagyni a programnak.

            Console.SetWindowSize(width, 30);

            //Menüzene indul
            if (musicOn)
                musics[cmusic].PlayLooping();

            //Title screen
            for (int i = 0; i < title.Length; i++)
            {
                string line = title[i];

                Console.WriteLine(line);
                /*if (line != "")
                    Thread.Sleep(495);*/
            }
            ConsoleKey key = Console.ReadKey(true).Key;
            /*int p = 0;
            ConsoleColor[] colors = { ConsoleColor.Red, ConsoleColor.Yellow, ConsoleColor.Blue, ConsoleColor.White };*/
            while (key != ConsoleKey.Enter && key != ConsoleKey.Spacebar)
            {
                while (Console.KeyAvailable)
                    key = Console.ReadKey(true).Key;
                key = Console.ReadKey(true).Key;

                #region idk
                /* while (!Console.KeyAvailable)
                 {
                     Console.SetCursorPosition(0, 21);

                     Console.ForegroundColor = colors[p];




                     Console.WriteLine(title[21]);


                     p++;
                     if (p > 3)
                         p = 0;

                     Thread.Sleep(1000);
                 }*/
                #endregion

                key = Console.ReadKey(true).Key;
            }
            PlaySound("select");

            Console.SetWindowSize(120, 30);
            Console.CursorVisible = false;

            //Fő programciklus
            while (true)
            {
                switch (nextDisplayed)
                {
                    case Screen.Menu:
                        menu.Display();
                        break;
                    case Screen.Map:
                        if (!mapRefresh)
                            mapScreen.Display();
                        else
                            mapScreen.RefreshPixels(pixelsToRefresh);
                        break;
                    case Screen.NPC:
                        cNPC.Display();
                        break;
                    case Screen.Combat:
                        cEnemy.EnemyMain();
                        break;
                    case Screen.Inventory:
                        player.inventory.Display();
                        break;
                    case Screen.ActionMenu:
                        basicActionMenu.Display();
                        break;
                    case Screen.SkillMenu:
                        skillActionMenu.Display();
                        break;
                    case Screen.WinScreen:
                        winScreen.Display();
                        break;
                    case Screen.Shop:
                        cNPC.shop.Display();
                        break;
                    case Screen.SkillTree:
                        skillTree.Display();
                        break;
                }
            }
        }
    }

    public class Tutorial
    {
        public static List<List<TextBox>> listOfTextBoxes = new List<List<TextBox>>();
        public static int progress = 4;
        public static int textBoxIndex = 0;
        public static bool completed = false;
        //public static bool disabled = true;

        public static void Next()
        {
            progress++;
            textBoxIndex = 0;
        }
    }

    public class Position
    {
        public int x;
        public int y;

        public Position() { }

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class Displayable
    {
        public string[] matrix = new string[40];
        public int maxY;
        public int maxX;
        public int[,] starts;

        public void BasicMatrixBuilder(StreamReader r, ref int i)
        {
            string line = r.ReadLine();
            starts = new int[40, line.Length];
            maxX = line.Length;

            while (!r.EndOfStream && line != "=")
            {
                matrix[i] = line;
                CalcStart(line, i);
                i++;
                line = r.ReadLine();
            }
            if (line != "=")
                matrix[i] = line;
            maxY = i;
        }

        public void SkipSpace(int y, ref int x)
        {
            if (starts[y, x] != 0)
                Console.SetCursorPosition(x += starts[y, x], y);
        }

        public void CalcStart(string line, int i)
        {
            int j = 0;
            while (j < maxX)
            {
                int skips = 0;
                while (j + skips < maxX && line[j + skips] == ' ') { skips++; }
                if (skips > 0 && line[j] == ' ')
                    j += starts[i, j] = skips;
                else
                    j++;
            }
        }
    }
}
