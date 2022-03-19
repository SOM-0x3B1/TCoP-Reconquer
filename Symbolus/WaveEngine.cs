using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Symbolus
{
    public class WaveEngine
    {
        public WaveCell[,] cells;
        public bool enable = true;
        private int width;
        private int height;
        private int firstX = int.MaxValue;
        private int i;
        private int n;
        private bool m;
        private int pacing;
        private int countOfWaves;
        private bool fromRestart = false;        
        public enum WaveCell { none, wheat, water };

        public Thread renderer;


        public WaveEngine(string[] map, int pacing)
        {
            cells = new WaveCell[map.Length, map[0].Length];
            height = map.Length;
            BuildCells(map, pacing);
        }
        public WaveEngine(string[] map, int maxY, int pacing)
        {
            cells = new WaveCell[maxY + 1, map[0].Length];
            height = maxY + 1;
            BuildCells(map, pacing);
        }
        public WaveEngine(string[] map, int maxY, int pacing, int firstX)
        {
            cells = new WaveCell[maxY + 1, map[0].Length + firstX];
            height = maxY + 1;
            this.firstX = firstX;
            BuildCells(map, pacing);
        }

        private void BuildCells(string[] map, int pacing)
        {
            width = map[0].Length;

            if (firstX == int.MaxValue)
                m = true;           

            for (int i = 0; i < height && map[i] != null; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    switch (map[i][j])
                    {
                        case 'f':
                            if (m)
                            {
                                cells[i, j] = WaveCell.wheat;
                                if (j < firstX)
                                    firstX = j;
                            }
                            else
                                cells[i, j + firstX] = WaveCell.wheat;
                            break;
                            /*case '~':
                                cells[i, j] = WaveCell.water;
                                if (j < firstX)
                                    firstX = j;
                                break;*/
                    }
                }
            }

            this.pacing = pacing;
            this.countOfWaves = height / pacing * 2;
            if(!m)
                width = map[0].Length + firstX;

            renderer = new Thread(() => Render(this));
        }

        public void Start()
        {                      
            if (renderer.ThreadState == ThreadState.Unstarted)
                renderer.Start();
            else
                enable = true;
        }
        public void Stop()
        {
            renderer.Abort();
            renderer = new Thread(() => Render(this));
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            fromRestart = true;
        }
        public void Pause()
        {
            enable = false;
            fromRestart = true;
        }

        private static void Render(WaveEngine wE)
        {
            while (true)
            {
                if (!wE.fromRestart)
                    wE.i = -wE.height;  

                for (; wE.i < 0; wE.i++)
                {
                    for (int y = 0; y < wE.height && wE.enable; y++)
                    {
                        for (int x = wE.firstX; x < wE.width && wE.enable; x++)
                        {
                            wE.n = y;
                            for (int j = 0; j < wE.countOfWaves * 2 && wE.enable; j++)
                            {
                                if (wE.cells[y, x] == WaveCell.wheat)
                                {
                                    if (wE.n == wE.i || wE.n == wE.i + 1)
                                    {
                                        Console.SetCursorPosition(x, y);
                                        Console.ForegroundColor = ConsoleColor.Black;
                                        Console.BackgroundColor = ConsoleColor.Yellow;
                                        Console.Write('f');
                                    }
                                    if (wE.n == wE.i - 1 || wE.n == wE.i + 2)
                                    {
                                        Console.SetCursorPosition(x, y);
                                        Console.ForegroundColor = ConsoleColor.DarkGray;
                                        Console.BackgroundColor = ConsoleColor.Yellow;
                                        Console.Write('f');
                                    }
                                    else if (wE.n == wE.i - 2 || (j == wE.countOfWaves - 1 && wE.n == wE.i + 13))
                                    {
                                        Console.SetCursorPosition(x, y);
                                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                                        Console.BackgroundColor = ConsoleColor.Yellow;
                                        Console.Write('f');
                                    }
                                }
                                wE.n -= wE.pacing;
                            }
                        }
                    }
                    if (wE.i > -wE.height /*&& !wE.fromRestart*/)
                        try
                        {
                            Thread.Sleep(200);
                        }
                        catch { }
                }

                if (wE.fromRestart)
                    wE.fromRestart = false;
            }
        }
    }
}
