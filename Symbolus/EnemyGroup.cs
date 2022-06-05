using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text;

namespace Symbolus
{
    /// <summary>
    /// Ellenséges csapat (mobilis objektum kezelőfelülettel).
    /// A térképen egy pont, de a valóságban lehet több ellenfél.
    /// </summary>
    public class EnemyGroup : MobileObject
    {
        private string type; //Rang, vagy boss (főgonosz)        
        private Background background; //az npc-hez tartozó háttér
        public bool moving = true;

        private string voice;
        private List<string> dialogs = new List<string>(); //
        private int cDialogIndex = 0; //jelenlegi dialógus indexe
        private List<int> dialogPercentages = new List<int>(); //az ellenfél hány % HP-nál folytassa a beszédét

        public  List<EnemyMember> members = new List<EnemyMember>();
        public int selectedMember = 0;
        private int lastSelectedMember = 0;
        public int selectedAction = 0;
        public List<Action> actions = new List<Action>();
        private bool turns = false; //automatikus lépések vannak-e
        private string lastAction = "";

        public bool defeated = false;
        public int XP;
        public int hits = 0;
        public int summedDamage = 0;

        public static string centerSpace = "                              ";
        public static string smallCenterSpace = "                             ";
        private int centerSpaceLength;

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="x">X pozíció</param>
        /// <param name="y">Y pozíció</param>
        /// <param name="type">Rang, vagy boss (főgonosz)</param>
        /// <param name="count">Ellenfelek száma</param>
        /// <param name="background">Ellefelek háttere</param>
        public EnemyGroup(int x, int y, string type, int count, string background, int dialog)
        {
            this.x = x;
            this.y = y;
            this.type = type;
            this.background = Background.backgrounds[background];

            for (int i = 0; i < count; i++)
            {
                members.Add(new EnemyMember(type));
            }

            if (dialog == 1) //rendelkezik szöveggel, tehát van saját karaktere
            {
                using (StreamReader r = new StreamReader($@"assets\enemies\{type}.txt", Encoding.UTF8))
                {
                    string[] line;
                    while (r.ReadLine() != "=") { }
                    this.voice = r.ReadLine();
                    while (!r.EndOfStream)
                    {
                        line = r.ReadLine().Split(';');
                        this.dialogs.Add(line[0]);
                        this.dialogPercentages.Add(int.Parse(line[1]));
                    }
                }
            }

            if (type[0] >= '0' && type[0] <= '9')
                this.XP = count * int.Parse(type);
            else
            {
                if (this.type == "pityu_bacsi")
                {
                    XP = 0;
                    moving = false;
                }
            }

            centerSpaceLength = Program.width / 2 - (31 * members.Count) / 2 + (6 * (members.Count - 1)) - 4;
        }


        /// <summary>
        /// A kirajzolás és az irányítás között számításokat kell elvégezni, ezt indítja a program fő ciklusa.
        /// </summary>
        public void EnemyMain()
        {
            if (actions.Count == 3)
            {
                turns = true;
                this.Display();
                Thread.Sleep(500);

                for (int j = 0; j < Program.player.effects.Count; j++)
                {
                    Effect effect = Program.player.effects[j];

                    switch (effect.type)
                    {
                        case "heal1":
                            Program.player.Heal(10);
                            lastAction = "heal";
                            this.Display();
                            Thread.Sleep(500);
                            break;
                        case "heal2":
                            Program.player.Heal(5);
                            lastAction = "heal";
                            this.Display();
                            Thread.Sleep(500);
                            break;
                        case "heal3":
                            Program.player.Heal(2);
                            lastAction = "heal";
                            this.Display();
                            Thread.Sleep(500);
                            break;
                        case "stamina1":
                            lastAction = "s";
                            Program.player.AddStamina(5);
                            this.Display();
                            Thread.Sleep(500);
                            break;
                        case "stamina2":
                            lastAction = "s";
                            Program.player.AddStamina(2);
                            this.Display();
                            Thread.Sleep(500);
                            break;
                        case "stamina3":
                            lastAction = "s";
                            Program.player.AddStamina(1);
                            this.Display();
                            Thread.Sleep(500);
                            break;
                        case "accuracy":
                            Program.player.damageModifier = 2;
                            Program.player.offMissProbability = -1;
                            break;
                    }

                    effect.timeRemaining--;
                    if (effect.timeRemaining == 0)
                    {
                        switch (effect.type)
                        {
                            case "accuracy":
                                Program.player.damageModifier = 1;
                                Program.player.offMissProbability = 10;
                                break;
                        }
                        Program.player.effects.RemoveAt(j);
                        j--;
                    }
                }

                for (int i = 0; i < 3 && !this.defeated; i++)
                {
                    EnemyMember member = members[actions[0].targetIndex];

                    ExecuteAction(actions[0], member);

                    lastAction = actions[0].type;
                    if (actions[0].type == "heavy_attack")
                    {
                        i++;
                        actions.RemoveAt(0);
                    }
                    actions.RemoveAt(0);

                    this.Display();
                    member.sticker = null;

                    if (!this.defeated)
                        Thread.Sleep(500);
                    else
                        Thread.Sleep(50);
                }
                lastAction = "";


                if (!this.defeated)
                {
                    Program.PlaySound("enemy_turn");
                    Thread.Sleep(600);
                    lastSelectedMember = selectedMember;

                    for (int i = 0; i < members.Count && Program.player.HP > 0 && !this.defeated; i++)
                    {
                        EnemyMember member = members[i];
                        selectedMember = i;

                        for (int j = 0; j < member.effects.Count; j++)
                        {
                            Effect effect = member.effects[j];
                            switch (effect.type)
                            {
                                case "fire":
                                    DealDamage(member, (int)(Math.Round((double)Program.player.inventory.equippedWeapon.baseDamage / 2)));
                                    break;
                                case "poison":
                                    DealDamage(member, (int)(Math.Round((double)Program.player.inventory.equippedWeapon.baseDamage / 4)));
                                    break;
                            }
                            if (effect.type != "stun")
                            {
                                member.specialSticker = Sticker.GetSticker(effect.type);
                                Program.PlaySound(effect.type);
                            }

                            this.Display();
                            Thread.Sleep(500);
                            member.sticker = null;
                            member.specialSticker = null;

                            effect.timeRemaining--;
                            if (effect.timeRemaining == 0)
                            {
                                switch (effect.type)
                                {
                                    //folyamatban
                                }
                                member.effects.RemoveAt(j);
                                j--;
                            }

                            member.specialSticker = null;
                            member.sticker = null;
                        }

                        if (!member.KO)
                        {
                            member.protection = member.basicProtection;
                            EnemyAi(member); ///////

                            if (Program.player.HP <= 0)
                            {
                                Program.PlaySound("engage");
                                Program.player.HP = 0;
                            }
                            else if (Program.player.HP > 0 && i != members.Count)
                            {
                                this.Display();
                                foreach (EnemyMember m in members)
                                    m.sticker = null;

                                member.specialSticker = null;
                                Thread.Sleep(500);
                            }
                        }
                    }
                    Program.player.defenseModifier = 1;
                    Program.player.defMissProbability = 10;

                    if (Program.player.HP == 0)
                        Program.player.Die();

                    selectedMember = lastSelectedMember;

                    if(this.defeated)
                        Win();
                }
                else
                    Win();

                actions.Clear();
                turns = false;
            }
            else
            {
                this.Display();
                this.Controls();
            }
        }


        /// <summary>
        /// Kezelőfelület kirajzolása
        /// </summary>
        public void Display()
        {
            Console.Clear();
            int z = 0;

            char c;
            for (int j = 0; j <= members[0].maxY; j++)
            {
                /*if(members.Count==1)
                    Console.Write("\t");*/

                for (int i = 0; i < centerSpaceLength; i++)
                    Console.Write(' ');
                    
                for (int k = 0; k < members.Count; k++)
                {
                    EnemyMember m = members[k];

                    int starter = 5;
                    if (k == 0)
                        starter = 0;
                    int ender = -4;
                    if (k == members.Count - 1)
                        ender = 0;

                    for (int i = starter; i < background.maxX + ender; i++)
                    {
                        if (m.sticker != null && j < m.sticker.maxY && m.sticker.matrix[j][i] != ' ')
                            c = m.sticker.matrix[j][i];
                        else if (m.specialSticker != null && j < m.specialSticker.maxY && m.specialSticker.matrix[j][i] != ' ')
                            c = m.specialSticker.matrix[j][i];
                        else if (j < background.maxY && m.matrix[j][i] == ' ')
                            c = background.matrix[j][i];
                        else
                            c = m.matrix[j][i];

                        switch (c)
                        {
                            case '#':
                                if (turns)
                                {
                                    if (lastAction == "heal")
                                    {
                                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    }
                                    else if (lastAction == "shield" || lastAction == "dodge")
                                    {
                                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                                    }
                                    else if (lastAction == "fast_attack" || lastAction == "heavy_attack" || lastAction == "skip" || actions.Count==3 || actions.Count == 0)
                                    {
                                        Console.BackgroundColor = ConsoleColor.DarkGray;
                                        Console.ForegroundColor = ConsoleColor.DarkGray;
                                    }
                                    else if (lastAction == "equipment")
                                    {
                                        Console.BackgroundColor = ConsoleColor.DarkGray;
                                        Console.ForegroundColor = ConsoleColor.DarkGray;
                                    }
                                    else
                                    {
                                        Console.BackgroundColor = ConsoleColor.DarkYellow;
                                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                                    }
                                }
                                else
                                {
                                    Console.BackgroundColor = ConsoleColor.DarkGray;
                                    Console.ForegroundColor = ConsoleColor.DarkGray;
                                }
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
                            case 'f':
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = ConsoleColor.Black;
                                break;
                            case 'F':
                                Console.BackgroundColor = ConsoleColor.White;
                                Console.ForegroundColor = ConsoleColor.White;
                                break;
                            case 'k':
                                Console.BackgroundColor = ConsoleColor.DarkBlue;
                                Console.ForegroundColor = ConsoleColor.DarkBlue;
                                break;
                            case 'K':
                                Console.BackgroundColor = ConsoleColor.Blue;
                                Console.ForegroundColor = ConsoleColor.Blue;
                                break;
                            case 'd':
                                Console.BackgroundColor = ConsoleColor.DarkGray;
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                break;
                            case 'g':
                                Console.BackgroundColor = ConsoleColor.Gray;
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                            case 'G':
                                Console.BackgroundColor = ConsoleColor.Gray;
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                            case 'M':
                                Console.BackgroundColor = ConsoleColor.Magenta;
                                Console.ForegroundColor = ConsoleColor.Magenta;
                                break;
                            case 'V':
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = ConsoleColor.White;
                                if (k != selectedMember)
                                    c = ' ';
                                break;
                            case 'H':
                                int n = 0;
                                string hp = members[k].HP.ToString();
                                int hplen = hp.Length;
                                for (n = 0; n < (11 - hplen); n++)
                                {
                                    hp += ' ';
                                    i++;
                                }
                                if (hplen < 2)
                                    i--;
                                if (hplen > 2)
                                    i++;

                                if (k != members.Count)
                                    i += 2;
                                else if (k == 0)
                                    i += 4;

                                double scale = (double)members[k].HP / members[k].maxHP;
                                n = 0;
                                Console.ForegroundColor = ConsoleColor.White;
                                for (; n < Math.Floor(scale * 11); n++)
                                {
                                    Console.BackgroundColor = ConsoleColor.DarkRed;
                                    Console.Write(hp[n]);
                                }
                                Console.BackgroundColor = ConsoleColor.Red;
                                for (; n < hp.Length; n++)
                                {
                                    Console.Write(hp[n]);
                                }
                                Console.BackgroundColor = ConsoleColor.Black;

                                c = ' ';
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.BackgroundColor = ConsoleColor.Black;
                                break;
                        }

                        Console.Write(c);
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                }

                
                EnemyMember member = this.members[selectedMember];
                int x = Tutorial.progress;
                if (j < member.effectList.maxY+1 && member.effects.Count > 0)
                {
                    Console.Write("  ");
                    member.RefreshEffectList();
                    member.effectList.WriteAtLine(j);
                }
                /*else if (y < Program.player.effectList.maxY + 1 && Program.player.effects.Count > 0)
                {
                    Console.Write("  ");
                    Program.player.RefreshEffectList();
                    Program.player.effectList.WriteAtLine(y);
                    y++;
                }*/                
                else if ((x == 2 || x == 6) && (member.effects.Count == 0 || j > member.effectList.maxY) && !Tutorial.completed)
                {
                    int n = Tutorial.textBoxIndex;                    
                    if (n < Tutorial.listOfTextBoxes[x].Count)
                    {
                        TextBox tut = Tutorial.listOfTextBoxes[x][n];
                        if (z < tut.maxY+1)
                        {
                            Console.Write("   ");
                            tut.WriteAtLine(z);
                            z++;
                        }
                    }
                }

                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine();
            }
            Console.WriteLine();


            for (int m = 0; m < 3; m++) //HP és stamina sáv; cselekvés-foglalatok
            {
                Console.Write(centerSpace);

                string line = Program.combatStats[m];
                Console.Write("  ");
                foreach (char c2 in line)
                {
                    if (c2 == '1')
                    {
                        int i = 0;
                        string hp = Program.player.HP.ToString();
                        int hplen = hp.ToString().Length;
                        for (i = 0; i < (8 - hplen); i++)
                        {
                            hp += ' ';
                        }
                        double scale = (double)Program.player.HP / Program.player.MaxHP;
                        i = 0;
                        for (; i < Math.Floor(scale * 8); i++)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                            Console.Write(hp[i]);
                        }
                        Console.BackgroundColor = ConsoleColor.Red;
                        for (; i < hp.Length; i++)
                        {
                            Console.Write(hp[i]);
                        }
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    else if (c2 == '2')
                    {
                        int i = 0;
                        string s = Program.player.S.ToString();
                        int slen = s.ToString().Length;
                        for (i = 0; i < (8 - slen); i++)
                        {
                            s += ' ';
                        }
                        double scale = (double)Program.player.S / Program.player.MaxS;
                        i = 0;
                        for (; i < Math.Floor(scale * 8); i++)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                            Console.Write(s[i]);
                        }
                        Console.BackgroundColor = ConsoleColor.Blue;
                        for (; i < s.Length; i++)
                        {
                            Console.Write(s[i]);
                        }
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    else
                    {
                        if (c2 == '#')
                        {
                            Console.BackgroundColor = ConsoleColor.DarkGray;
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                        }
                        else
                        {
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        Console.Write(c2);
                    }
                }

                string line2 = Program.actionSlots[m];
                int x = 0;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write("  ");
                for (int i = 0; i < line2.Length; i++)
                {
                    char c3 = line2[i];
               
                    if (c3 == '1')
                    {
                        if (actions.Count > x)
                        {
                            if (actions[x].type == "fast_attack" || actions[x].type == "heavy_attack")
                            {
                                Console.BackgroundColor = ConsoleColor.DarkRed;

                                if (members.Count > 1)
                                {
                                    Console.ForegroundColor = ConsoleColor.White;
                                    c3 = (actions[x].targetIndex+1).ToString()[0];
                                }
                                else
                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                            }
                            else if (actions[x].type == "shield" || actions[x].type == "dodge")
                            {
                                Console.BackgroundColor = ConsoleColor.Blue;
                                Console.ForegroundColor = ConsoleColor.Blue;
                            }
                            else if (actions[x].type == "heal")
                            {
                                Console.BackgroundColor = ConsoleColor.Green;
                                Console.ForegroundColor = ConsoleColor.Green;
                            }
                            else if (actions[x].type == "skip")
                            {
                                Console.BackgroundColor = ConsoleColor.Gray;
                                Console.ForegroundColor = ConsoleColor.Gray;
                            }
                            else if (actions[x].type == "equipment")
                            {
                                Console.BackgroundColor = ConsoleColor.DarkYellow;
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                            }
                            else
                            {
                                Console.BackgroundColor = ConsoleColor.Magenta;
                                if (members.Count > 1)
                                {
                                    Console.ForegroundColor = ConsoleColor.White;
                                    c3 = (actions[x].targetIndex + 1).ToString()[0];
                                }
                                else
                                    Console.ForegroundColor = ConsoleColor.Magenta;
                            }
                        }
                        else
                        {
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.Black;
                        }
                        x++;
                    }
                    else if (c3 == '#')
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    Console.Write(c3);
                }

                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine();
            }
            Console.WriteLine();


            if (!turns)
            {
                foreach (string s in Program.fightmenu)
                {
                    Console.Write(smallCenterSpace);
                    foreach (char c2 in s)
                    {
                        if (c2 <= '9' && c2 >= '1')
                        {
                            if (int.Parse(c2.ToString()) == selectedAction + 1 && Tutorial.textBoxIndex == Tutorial.listOfTextBoxes[Tutorial.progress].Count -1)
                            {
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.BackgroundColor = ConsoleColor.Gray;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.BackgroundColor = ConsoleColor.DarkGray;
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.BackgroundColor = ConsoleColor.Black;
                        }
                        Console.Write(c2);
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        /// Utasítás várása, fogadása, végrehajtása
        /// </summary>
        protected void Controls()
        {
            ConsoleKeyInfo keyinfo = new ConsoleKeyInfo();

            while (Console.KeyAvailable)
            {
                keyinfo = Console.ReadKey(true);
            }
            keyinfo = Console.ReadKey(true);
            
            if (Tutorial.progress != 2 && Tutorial.progress != 6 || Tutorial.textBoxIndex > Tutorial.listOfTextBoxes[Tutorial.progress].Count - 2)
            {
                if (keyinfo.Key != ConsoleKey.Spacebar && keyinfo.Key != ConsoleKey.Enter)
                {
                    if (keyinfo.Key == ConsoleKey.A && selectedAction > 0)
                    {
                        selectedAction--;
                        Program.PlaySound("menumove");
                        Program.nextDisplayed = Program.Screen.Combat;
                    }
                    else if (keyinfo.Key == ConsoleKey.D && selectedAction < 3)
                    {
                        selectedAction++;
                        Program.PlaySound("menumove");
                        Program.nextDisplayed = Program.Screen.Combat;
                    }
                    else if (keyinfo.Key == ConsoleKey.Tab)
                    {
                        Program.PlaySound("menumove");
                        selectedMember++;
                        if (selectedMember == members.Count)
                            selectedMember = 0;
                        Program.nextDisplayed = Program.Screen.Combat;
                    }
                    else if (keyinfo.Key == ConsoleKey.Escape)
                    {
                        Program.PlaySound("pause");
                        Program.menu.backFromPause = true;
                        Program.menu.menuPage = new MenuPage("pause");
                        Program.nextDisplayed = Program.Screen.Menu;
                    }
                    else if (keyinfo.Key == ConsoleKey.E)
                    {
                        Program.PlaySound("inv1");
                        Program.nextDisplayed = Program.Screen.Inventory;
                    }
                    else if (keyinfo.KeyChar >= '1' && keyinfo.KeyChar <= '6')
                        BasicActionMenu.AddViaShortcut(keyinfo.KeyChar);
                    else if(keyinfo.KeyChar >= '0')
                        BasicActionMenu.AddViaShortcut('6');
                }
                else
                {
                    switch (selectedAction)
                    {
                        case 0:
                            Program.PlaySound("action_selected");
                            Program.nextDisplayed = Program.Screen.ActionMenu;
                            break;
                        case 1:
                            Program.PlaySound("action_selected");
                            Program.nextDisplayed = Program.Screen.SkillMenu;
                            break;
                        case 2:
                            Program.PlaySound("reset");
                            actions.Clear();
                            break;
                        case 3:
                            Program.PlaySound("inv1");
                            Program.nextDisplayed = Program.Screen.Inventory;
                            break;
                    }
                }
            }
            else
            {
                if (keyinfo.Key == ConsoleKey.Enter)
                {
                    Program.PlaySound("action_selected");
                    Tutorial.textBoxIndex++;
                }
                else if (keyinfo.Key == ConsoleKey.Escape)
                {
                    Program.PlaySound("pause");
                    Program.menu.backFromPause = true;
                    Program.menu.menuPage = new MenuPage("pause");
                    Program.nextDisplayed = Program.Screen.Menu;
                }
                else
                    Program.PlaySound("blocked");
            }   
            

            #region semmi
            /*else
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
                }
                Program.refresh = true;*/
            #endregion
        }

        private void Win()
        {
            Program.mapScreen.map.enemies.Remove(this);
            Program.nextDisplayed = Program.Screen.WinScreen;
            Program.combat = false;

            if (Tutorial.progress == 2)
                Tutorial.Next();

            if (Program.musicOn)
                Program.musics[Program.cmusic].Stop();

            Thread.Sleep(300);

            if (Program.musicOn)
            {
                Program.cmusic = "win";
                Program.musics["win"].Play();
            }
            Program.winScreen.Display(XP, XP * (hits - summedDamage), hits, summedDamage);
        }

        /// <summary>
        /// Ki van-e ütve (0 HP) az ellefél
        /// </summary>
        /// <param name="member">Vizsgálandó ellenfél</param>
        private void KOCheck(EnemyMember member)
        {
            if (member.HP <= 0)
            {
                Program.PlaySound("ko");
                member.KnockOut();
                member.HP = 0;

                if (members.All(a => a.KO))
                {
                    this.defeated = true;
                    if (this.type == "pityu_bacsi")
                        Tutorial.Next();
                    else if (Tutorial.progress == 6)
                        Tutorial.Next();
                    else if (Tutorial.progress == 8)
                    {
                        Program.newGame = false;
                        Tutorial.Next();
                    }
                }
            }
        }

        /// <summary>
        /// Az akciók végrehajtása
        /// </summary>
        private void ExecuteAction(Action action, EnemyMember member)
        {
            double baseDamage = Program.player.inventory.equippedWeapon.baseDamage;
            switch (action.type)
            {
                case "fast_attack":
                    DealDamage(member, (int)Math.Round(baseDamage + (double)Program.rnd.Next(-20, 20) / 100 * baseDamage));
                    break;
                case "heavy_attack":
                    double dhit = baseDamage * 2.3;
                    DealDamage(member, (int)Math.Round(dhit + (double)Program.rnd.Next(-10, 10) / 100 * dhit));
                    break;
                case "heal":
                    if (Program.player.HP < Program.player.MaxHP)
                    {
                        Program.PlaySound("heal");
                        Program.player.HP += Program.player.MaxHP / 10;

                        if (Program.player.HP > Program.player.MaxHP)
                            Program.player.HP = Program.player.MaxHP;
                    }
                    else
                        Program.PlaySound("error");
                    break;
                case "shield":
                    if (Program.player.defenseModifier == 1)
                    {
                        Program.player.defenseModifier = 2;
                        Program.PlaySound("shield");
                    }
                    else
                        Program.PlaySound("error");
                    break;
                case "dodge":
                    if (Program.player.defMissProbability == 10)
                    {
                        Program.player.defMissProbability += 55;
                        Program.PlaySound("shield");
                    }
                    else
                        Program.PlaySound("error");
                    break;
                case "Tejesvödör":
                    Program.player.effects.Clear();
                    Program.PlaySound("skill");
                    break;
                case "Csí áramlás":
                    ApplySkillOnSelf(action.type);
                    Program.PlaySound("skill");
                    break;
                case "Célzóvíz":
                    ApplySkillOnSelf(action.type);
                    Program.PlaySound("skill");
                    Program.player.offMissProbability = -1;
                    Program.player.damageModifier = 2;
                    break;
                case "skip":
                    Program.player.AddStamina(5);
                    break;
                case "equipment":
                    Program.player.AddStamina(2);
                    break;
                default:
                    Skill skill = Skill.allSkills.Where(a => a.name == action.type).First();
                    int j = action.targetIndex;

                    if (skill.name == "Petárda" || skill.name == "Molotov")
                        Program.PlaySound("explosion");
                    else if (skill.damage == 0)
                        Program.PlaySound("skill");

                    bool firstrun = true;
                    for (int i = 0; i < skill.area; i++)
                    {
                        if (j >= members.Count)
                            j = 0;
                        if (!firstrun)
                        {
                            if (j == action.targetIndex)           
                                break;
                            if (skill.damage != 0)
                            {
                                this.Display();
                                Thread.Sleep(300);
                            }
                            foreach (EnemyMember m in members)
                                m.sticker = null;
                        }
                        ApplySkillOnEnemy(members[j], skill);                        
                        j++;
                        firstrun = false;
                    }
                    if (skill.damage != 0)
                    {
                        this.Display();
                        Thread.Sleep(500);
                    }
                    foreach (EnemyMember m in members)
                        m.sticker = null;
                    break;
            }
        }


        /// <summary>
        /// Ellenfél sebzése
        /// </summary>
        /// <param name="member"></param>
        private void DealDamage(EnemyMember member, int hit)
        {
            hit *= Program.player.damageModifier;
            hit -= member.protection;
            if (member.HP > 0)
            {
                if (Program.rnd.Next(0, 101) > Program.player.offMissProbability && hit > 0)
                {
                    Program.PlaySound("hit");

                    member.HP -= hit;
                    this.hits += hit;

                    member.sticker = Sticker.GetVariableSticker("damage", hit.ToString());

                    KOCheck(member);
                }
                else
                {
                    Program.PlaySound("miss");
                    member.sticker = Sticker.GetSticker("offensive_miss");
                }
            }
            else
                Program.PlaySound("koHit");
        }


        /// <summary>
        /// Az ellenség lépésének megállapítása, végrehajtása
        /// </summary>
        /// <param name="member"></param>
        private void EnemyAi(EnemyMember member)
        {
            int damage;

            int defenseProbability = 100 - (int)(Math.Round((double)member.HP / member.maxHP * 100));

            if (!member.effects.Any(a => a.type == "stun"))
            {
                if (Program.rnd.Next(0, 200) > defenseProbability)
                {
                    damage = (int)Math.Round(member.baseDamage + ((double)Program.rnd.Next(-20, 10) / 100) * member.baseDamage);
                    damage = (int)Math.Round(damage - Program.player.inventory.equippedArmor.protection * Program.player.defenseModifier);                    

                    member.sticker = Sticker.GetVariableSticker("sword", damage.ToString());

                    if (Program.rnd.Next(0, 101) > Program.player.defMissProbability && damage > 0)
                    {
                        Program.PlaySound("damage");

                        if (member.effects.Any(a => a.type == "confusion"))
                        {
                            int randomTarget = Program.rnd.Next(-1, members.Count);
                            if (randomTarget == -1)
                            {
                                Program.player.HP -= damage;
                                this.summedDamage += damage;
                            }
                            else
                                DealDamage(members[randomTarget], damage);
                        }
                        else
                        {
                            Program.player.HP -= damage;
                            this.summedDamage += damage;
                        }
                    }
                    else
                    {
                        Program.PlaySound("miss");
                        member.sticker = Sticker.GetSticker("miss");
                    }
                }
                else
                {
                    switch (Program.rnd.Next(0, 2))
                    {
                        case 0:
                            member.sticker = Sticker.GetSticker("heal");
                            Program.PlaySound("heal");
                            member.HP += member.maxHP / 4;
                            if (member.HP > member.maxHP)
                                member.HP = member.maxHP;
                            break;
                        case 1:
                            member.sticker = Sticker.GetSticker("shield");
                            Program.PlaySound("shield");
                            member.protection *= 2;
                            break;
                    }
                }
            }
            else
            {
                Program.PlaySound("stun");
                member.sticker = null;
                member.specialSticker = Sticker.GetSticker("stun");
            }
        }


        /// <summary>
        /// Skill használata az ellenfélen
        /// </summary>
        private void ApplySkillOnEnemy(EnemyMember member, Skill skill)
        {
            Effect effect = new Effect(skill.effect, skill.effectDuration + 1);
            Effect effect2;
            if (!member.effects.Any(a => a.type == effect.type))
                member.effects.Add(effect);
            else
            {
                effect2 = member.effects.Where(a => effect.type == a.type).First();
                if (effect2.timeRemaining < effect.timeRemaining)
                    effect.timeRemaining = effect2.timeRemaining;
            }

            if (skill.damage != 0)
                DealDamage(member, (int)((double)skill.damage / 100 * Program.player.inventory.equippedWeapon.baseDamage));    
        }


        /// <summary>
        /// Skill használata magunkon
        /// </summary>
        private void ApplySkillOnSelf(string name)
        {
            Skill skill = Skill.allSkills.Where(a => a.name == name).First();
            Effect effect = new Effect(skill.effect, skill.effectDuration);

            if (!Program.player.effects.Contains(effect))
                Program.player.effects.Add(effect);
        }


        /// <summary>
        /// Az ellefél játékos felé való lépkedésének végrehajtása
        /// </summary>
        /// <param name="enemies">Melyik csoport mozogjon</param>
        public void MoveOnMap(List<EnemyGroup> enemies)
        {
            if (moving)
            {
                Program.pixelsToRefresh.Add(new Position(x, y));
                int targetX = Program.player.x;
                int targetY = Program.player.y;
                int lastX = this.x;
                int lastY = this.y;

                if (Program.rnd.Next(0, 2) == 0)
                {
                    if (this.x > targetX && Free(-1, 0))
                        this.x--;
                    else if (this.x < targetX && Free(1, 0))
                        this.x++;
                    else if (this.y > targetY && Free(0, -1))
                        this.y--;
                    else if (this.y < targetY && Free(0, 1))
                        this.y++;
                }
                else
                {
                    if (this.y > targetY && Free(0, -1))
                        this.y--;
                    else if (this.y < targetY && Free(0, 1))
                        this.y++;
                    else if (this.x > targetX && Free(-1, 0))
                        this.x--;
                    else if (this.x < targetX && Free(1, 0))
                        this.x++;
                }

                if (enemies.Any(a => a != this && a.x == x && a.y == y))
                {
                    x = lastX;
                    y = lastY;
                }

                Program.pixelsToRefresh.Add(new Position(x,y));
            }
        }
    }


    /// <summary>
    /// Egy ellenfél az ellefélcsomón belül
    /// </summary>
    public class EnemyMember : Displayable
    {
        public int HP;
        public int maxHP; //alapérték
        public int protection;
        public int basicProtection; //alapérték
        public bool KO = false; //le van-e győzve
        public int baseDamage;
        public Sticker sticker; //alap matrica
        public Sticker specialSticker = null; //effekt matrica
        public List<Effect> effects = new List<Effect>(); //ellenfélre ható effektek
        private string type = "";

        public TextBox effectList = new TextBox(); //grafikus lista az effektekről


        public EnemyMember() { }


        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="type">Az ellenfél szintje, vagy egyedi neve</param>
        public EnemyMember(string type)
        {
            this.type = type;

            using (StreamReader r = new StreamReader(@"assets\enemies\" + type + ".txt")) //karakter (személy) rajzának betöltése
            {
                int i = 0;
                BasicMatrixBuilder(r, ref i);
                maxY--;
            }

            switch (type)
            {
                case "1":
                    HP = 40;
                    protection = Program.rnd.Next(1, 3);
                    baseDamage = 10;
                    break;
                case "2":
                    HP = 50 + Program.rnd.Next(0,5);
                    protection = 1;
                    baseDamage = 10;
                    break;
                case "3":
                    HP = Program.rnd.Next(15, 31) * 10;
                    protection = Program.rnd.Next(60, 90);
                    baseDamage = Program.rnd.Next(2, 4) * 5;
                    break;
                case "4":
                    HP = Program.rnd.Next(40, 61) * 10;
                    protection = Program.rnd.Next(100, 200);
                    baseDamage = Program.rnd.Next(2, 4) * 5;
                    break;
                case "5":
                    HP = Program.rnd.Next(90, 131) * 10;
                    protection = Program.rnd.Next(300, 400);
                    baseDamage = Program.rnd.Next(2, 4) * 5;
                    break;
                case "pityu_bacsi":
                    HP = 20;
                    protection = 0;
                    baseDamage = 10;                    
                    break;
            }
            maxHP = HP;
            basicProtection = protection;
        }


        /// <summary>
        /// Ellenfél legyőzése
        /// </summary>
        public void KnockOut()
        {
            this.KO = true;
            Program.koEnemy.CopyTo(this.matrix, 0);
            this.effects.Clear();
        }

        /// <summary>
        /// Az ellenségre ható effektek grafikus listájának újralakotása
        /// </summary>
        public void RefreshEffectList()
        {
            string temp = "";
            foreach (Effect effect in effects)
            {
                temp += " - " + Effect.typeToName[effect.type] + " $";
            }
            effectList = new TextBox(temp, "Ellenség:");
        }

        /// <summary>
        /// Ellenfél eredeti állapotának visszállítása újrekezdés esetén
        /// </summary>
        public void Reset()
        {
            using (StreamReader r = new StreamReader(@"assets\enemies\" + type + ".txt")) //karakter (személy) rajzának betöltése
            {
                int i = 0;
                BasicMatrixBuilder(r, ref i);
                maxY--;
            }

            effects.Clear();
            KO = false;
            sticker = null;
            specialSticker = null;
            HP = maxHP;
            protection = basicProtection;
            RefreshEffectList();
        }
    }
}