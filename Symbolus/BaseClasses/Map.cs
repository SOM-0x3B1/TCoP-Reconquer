using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Symbolus
{
    /// <summary>
    /// Karakter-rajzzal alkotott pálya + mobilis objektumok + zene.
    /// </summary>
    public class Map : Displayable
    {
        public string name;
        //public WaveEngine waveEngine;
        public int darkness; //A játékos által látható terület sugara (használatlan)        
        public string music; //alap zene
        public List<Gate> gates = new List<Gate>(); //új mapokra vezető "átjárók"
        public List<EnemyGroup> enemies = new List<EnemyGroup>(); //a mapon álló ellenfelek
        public List<NPC> npcs = new List<NPC>(); //a mapon álló NPC-k

        public List<List<string>> cutsceneDialogs = new List<List<string>>(); //a map első betöltésénél megjelenő narráció
        public int cDialogIndex = 0; //jelenlegi dialógus indexe

        public static string centerSpace = "                                        ";
        public static int centerSpaceLength = centerSpace.Length;
        public static string smallCenterSpace = "                                    ";
        public static int smallCenterSpaceLength = centerSpace.Length;
        public enum MapObject { gate, enemy, npc, landmark};


        public Map(string name)
        {
            this.name = name;

            using (StreamReader r = new StreamReader(@"assets\maps\" + name + ".txt", Encoding.UTF8))
            {
                int i = 0;
                BasicMatrixBuilder(r, ref i);

                string[] entry;
                string[] p;
                string line = r.ReadLine();

                while (line != "=") //mobilis (pozícióval rendelkező) objektumok betöltése
                {
                    entry = line.Split(':');
                    p = entry[1].Split(';');
                    switch (entry[0])
                    {
                        case "e":
                            enemies.Add(new EnemyGroup(int.Parse(p[0].Split(',')[0]), int.Parse(p[0].Split(',')[1]), p[1], int.Parse(p[2]), p[3], int.Parse(p[4])));
                            break;
                        case "p":
                            Program.player.x = int.Parse(p[0].Split(',')[0]);
                            Program.player.y = int.Parse(p[0].Split(',')[1]);
                            break;
                        case "d":
                            darkness = int.Parse(p[0]);
                            break;
                        case "g":                           
                            gates.Add(new Gate(p[0].Split(','), p[1], p[2], p[3].Split(',')));
                            break;
                        case "m":
                            music = p[0];
                            break;
                        case "n":
                            npcs.Add(new NPC(p[0], int.Parse(p[1].Split(',')[0]), int.Parse(p[1].Split(',')[1]), int.Parse(p[2])));
                            break;
                    }
                    line = r.ReadLine();
                }
                int k = 0;
                while (!r.EndOfStream) //narráció betöltése
                {
                    line = r.ReadLine();
                    string[] s = line.Split('/');
                    cutsceneDialogs.Add(new List<string>());
                    foreach (var item in s)
                        cutsceneDialogs[k].Add(item);
                    k++;                    
                }

                //waveEngine = new WaveEngine(matrix, maxY, maxY / 2, centerSpaceLength);
            }
        }

        public bool CheckObject(int x, int y, MapObject mapObject)
        {
            switch (mapObject)
            {
                case MapObject.enemy:
                    for (int i = 0; i < enemies.Count; i++)
                    {
                        if (enemies[i].x == x && enemies[i].y == y)
                            return true;
                    }
                    return false;
                case MapObject.gate:
                    for (int i = 0; i < gates.Count; i++)
                    {
                        if (gates[i].x == x && gates[i].y == y)
                            return true;
                    }
                    return false;
                case MapObject.npc:
                    for (int i = 0; i < npcs.Count; i++)
                    {
                        if (npcs[i].person && npcs[i].x == x && npcs[i].y == y)
                            return true;
                    }
                    return false;
                case MapObject.landmark:
                    for (int i = 0; i < npcs.Count; i++)
                    {
                        if (!npcs[i].person && npcs[i].x == x && npcs[i].y == y)
                            return true;
                    }
                    return false;
            }
            return false;
        }

        public static Dictionary<string, Map> maps = new Dictionary<string, Map>(); //már betöltött map-ok
    }
}
