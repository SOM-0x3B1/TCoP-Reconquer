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
        private int width;
        private int height;
        private int firstX = int.MaxValue;
        private int n;
        private int pacing;
        private int countOfWaves;
        public enum WaveCell { none, wheat, water };
        

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
        private void BuildCells(string[] map, int pacing)
        {
            width = map[0].Length;

            for (int i = 0; i < height && map[i] != null; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    switch (map[i][j])
                    {
                        case 'f':
                            cells[i, j] = WaveCell.wheat;
                            if (j < firstX)
                                firstX = j;
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
        }

        public void Render()
        {
            while (true)
            {
                for (int i = -height; i < 0; i++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = firstX; x < width; x++)
                        {
                            n = y;
                            for (int j = 0; j < countOfWaves * 2; j++)
                            {
                                if (cells[y, x] == WaveCell.wheat)
                                {
                                    if (n == i || n == i + 1)
                                    {
                                        Console.SetCursorPosition(x, y);
                                        Console.ForegroundColor = ConsoleColor.Black;
                                        Console.BackgroundColor = ConsoleColor.Yellow;
                                        Console.Write('f');
                                    }
                                    if (n == i - 1 || n == i + 2)
                                    {
                                        Console.SetCursorPosition(x, y);
                                        Console.ForegroundColor = ConsoleColor.DarkGray;
                                        Console.BackgroundColor = ConsoleColor.Yellow;
                                        Console.Write('f');
                                    }
                                    else if (n == i - 2 || (j == countOfWaves - 1 && n == i + 13))
                                    {
                                        Console.SetCursorPosition(x, y);
                                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                                        Console.BackgroundColor = ConsoleColor.Yellow;
                                        Console.Write('f');
                                    }
                                }
                                n -= pacing;
                            }
                        }
                    }
                    if(i >- height)
                        Thread.Sleep(200);
                }
            }
        }
    }
}
