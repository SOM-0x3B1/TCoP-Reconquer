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


        public Map(string name)
        {
            this.name = name;

            using (StreamReader r = new StreamReader(@"assets\maps\" + name + ".txt", Encoding.UTF8))
            {
                string line = r.ReadLine();
                int i = 0;
                while (line != "=") //térkép betöltése
                {
                    matrix[i] = line;
                    i++;
                    line = r.ReadLine();
                }
                maxY = i;
                string type = "";

                line = r.ReadLine();
                while (line != "=") //mobilis (pozícióval rendelkező) objektumok betöltése
                {
                    type = line.Split(':')[0];
                    string[] parameters;
                    switch (type)
                    {
                        case "e":
                            parameters = line.Split(':')[1].Split(';');
                            enemies.Add(new EnemyGroup(int.Parse(parameters[0]), int.Parse(parameters[1]), parameters[2], int.Parse(parameters[3]), parameters[4], int.Parse(parameters[5])));
                            break;
                        case "p":
                            Program.player.x = int.Parse(line.Split(':')[1].Split(',')[0]);
                            Program.player.y = int.Parse(line.Split(':')[1].Split(',')[1]);
                            break;
                        case "d":
                            darkness = int.Parse(line.Split(':')[1]);
                            break;
                        case "g":
                            string input = line.Split(':')[1];
                            parameters = input.Split(';');
                            gates.Add(new Gate(parameters[0].Split(','), parameters[1], parameters[2], parameters[3].Split(',')));
                            break;
                        case "m":
                            music = line.Split(':')[1];
                            break;
                        case "n":
                            parameters = line.Split(':')[1].Split(',');
                            npcs.Add(new NPC(parameters[0], int.Parse(parameters[1]), int.Parse(parameters[2]), int.Parse(parameters[3])));
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

        public static Dictionary<string, Map> maps = new Dictionary<string, Map>(); //már betöltött map-ok
    }
}
