using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Symbolus
{

    /// <summary>
    /// Pozícióval rendelkező, térképen elhelyezhető (mobilis) objektum.
    /// </summary>
    public class MobileObject
    {
        public int x;
        public int y;

        public bool Free(int xChange, int yChange)
        {
            int i = y + yChange;
            int j = x + xChange;

            if (i > -1 && i < Program.mapScreen.map.maxY && j > -1 && j < Program.mapScreen.map.matrix[0].Length)
            {
                char c = Program.mapScreen.map.matrix[i][j]; // út/föld van-e alatta
                if (c != ' ' && c != '-' && c != (char)39)
                    return false;
                else
                    return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// A játékos (a GameScreen kezelőfelületen keresztül mozgatható)
    /// </summary>
    public class Player : MobileObject
    {
        public int MaxHP = 50;
        public int HP = 50;
        public int MaxS = 20;
        public int S = 20; //stamina
        public bool sprint = false; //2-szeres gyorsaság
        public Inventory inventory = new Inventory();
        public double defenseModifier = 1;
        public int defMissProbability = 10;
        public int offMissProbability = 10;
        public int damageModifier = 1;
        public List<Skill> skills = new List<Skill>();
        public List<Effect> effects = new List<Effect>();        

        public TextBox skillList = new TextBox();
        public TextBox effectList = new TextBox();        

        public int level = 0;
        public int xp = 0;
        public int money = 200;

        public List<int> levelsteps = new List<int> {1, 10, 20, 30, 40, 50};


        public Player(int x, int y)
        {
            this.x = x;
            this.y = y;
        }        

        /// <summary>
        /// Megnézi, hogy a játékos nekiment-e MobileObject-nek
        /// </summary>
        /// <returns>Ütközözz-e valamivel</returns>
        public bool InteractionCheck()
        {
            if (Program.mapScreen.map.npcs.Any(a => a.x == x && a.y == y)) //NPC-be ütközött
            {
                Program.PlaySound("npc");
                Program.mapRefresh = false;
                Program.cNPC = Program.mapScreen.map.npcs.Where(a => a.x == x && a.y == y).ToList()[0];
                Program.NPCRefresh = true;
                Program.nextDisplayed = Program.Screen.NPC;

                if (Tutorial.progress == 0)
                    Tutorial.Next();

                return true;
            }
            else if (Program.mapScreen.map.enemies.Any(a => a.x == x && a.y == y) && Tutorial.progress != 0)
            {
                Program.mapRefresh = false;
                Program.cEnemy = Program.mapScreen.map.enemies.Where(a => a.x == x && a.y == y).First();
                Program.nextDisplayed = Program.Screen.Combat;
                Program.combat = true;

                if (Tutorial.progress == 1)
                    Tutorial.Next();
                else if (Tutorial.progress == 5)
                    Tutorial.Next();

                if (Program.musicOn)
                    Program.musics[Program.cmusic].Stop();
                Program.PlaySound("engage");

                Console.BackgroundColor = ConsoleColor.Black;
                Console.Clear();

                Thread.Sleep(1000);
                if (Program.musicOn)
                {
                    Program.cmusic = "fight";
                    Program.musics[Program.cmusic].PlayLooping();
                }

                return true;
            }
            else if (Program.mapScreen.map.gates.Any(a => a.x == this.x && a.y == this.y) && Tutorial.completed) 
            {
                Program.mapRefresh = false;
                Program.PlaySound("newmap");
                Gate gate = Program.mapScreen.map.gates.First(a => a.x == this.x && a.y == this.y);
                Program.mapScreen.LoadMap(gate.destination);
                this.x = gate.teleportToX;
                this.y = gate.teleportToY;
            }

            return false;
        }

        /// <summary>
        /// Mozgás végrehejtása
        /// </summary>
        /// <param name="keyinfo">Billentyű</param>
        public void move(ConsoleKeyInfo keyinfo)
        {            
            switch (keyinfo.Key)
            {
                case ConsoleKey.W:
                    if (Program.player.Free(0, -1))
                        Program.player.y--;
                    else
                        Program.PlaySound("blocked");
                    break;
                case ConsoleKey.A:
                    if (Program.player.Free(-1, 0))
                        Program.player.x--;
                    else
                        Program.PlaySound("blocked");
                    break;
                case ConsoleKey.S:
                    if (Program.player.Free(0, 1))
                        Program.player.y++;
                    else
                        Program.PlaySound("blocked");
                    break;
                case ConsoleKey.D:
                    if (Program.player.Free(1, 0))
                        Program.player.x++;
                    else
                        Program.PlaySound("blocked");
                    break;
            }

            if (HP < MaxHP)
                HP++;
            if (S < MaxS)
                S++;
        }

        /// <summary>
        /// Game over
        /// </summary>
        public void Die()
        {
            Console.Clear();
            Program.musics[Program.cmusic].Stop();
            Thread.Sleep(1500);
            Program.menu.menuPage = new MenuPage("game_over");
            Program.nextDisplayed = Program.Screen.Menu;
            if (Program.musicOn)
                Program.musics["game_over"].Play();
            Program.cmusic = "game_over";

            Program.combat = false;
        }

        public void AddXP(int x)
        {
            this.xp += x;
            if (xp >= levelsteps[level])
            {
                level++;
                Program.skillTree.freePoints++;
                Program.PlaySound("levelup");
            }
        }

        public void RefreshSkillList()
        {
            string temp = "";
            foreach(Skill skill in skills)
            {
                temp += " - " + skill.name + " $";
            }
            skillList = new TextBox(temp,"Képességek");
        }
        public void RefreshEffectList()
        {
            string temp = "";
            foreach (Effect effect in effects)
            {
                temp += " - " + effect.type + " $";
            }
            effectList = new TextBox(temp, "Ellenség:");
        }

        public void Heal(int x)
        {
            Program.player.HP += Program.player.MaxHP / x;
            if (Program.player.HP >= Program.player.MaxHP)
                Program.player.HP = Program.player.MaxHP;
            Program.PlaySound("heal");
        }
        public void AddStamina(int x)
        {
            Program.player.S += Program.player.MaxS / x;
            if (Program.player.S >= Program.player.MaxS)
                Program.player.S = Program.player.MaxS;
            Program.PlaySound("faster");
        }
    }


    /// <summary>
    /// Új mapokra vezető "átjárók"
    /// </summary>
    public class Gate : MobileObject
    {
        public string destination;
        public bool open;
        public int teleportToX;
        public int teleportToY;

        public Gate(string[] pos, string name, string open, string[] pos2)
        {
            this.destination = name;
            this.x = int.Parse(pos[0]);
            this.y = int.Parse(pos[1]);

            if (open == "1")
                this.open = true;
            else
                this.open = false;

            teleportToX = int.Parse(pos2[0]);
            teleportToY = int.Parse(pos2[1]);
        }
    }
}
