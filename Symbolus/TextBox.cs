using System;
using System.IO;

namespace Symbolus
{
    /// <summary>
    /// Grafikus lista, melyben egy keretben helyezkedik a tagolt szöveg
    /// </summary>
    public class TextBox : Displayable
    {
        private string[] text = new string[4];        
        public string title = "";

        private static string[] baseMatrix = new string[4]; //négy soros alapkeret


        public TextBox() {}

        public TextBox(string text)
        {            
            this.title = "Leírás";
            BasicKonstruktor(text);
        }
        public TextBox(string text, string title)
        {
            this.title = title;
            BasicKonstruktor(text);            
        }


        public void BasicKonstruktor(string text)
        {
            this.text = text.Split(' ');
            maxY = 3;

            if (baseMatrix[0] == null)
                LoadGraphics();

            char c;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < baseMatrix[0].Length; j++)
                {
                    c = baseMatrix[i][j];
                    if (c == '%')
                    {
                        matrix[i] += this.title;
                        j += this.title.Length-1;
                    }
                    else
                    {
                        matrix[i] += c;
                    }
                }
            }

            matrix[maxY] += "#";
            for (int i = 0; i < this.text.Length; i++)
            {
                if (matrix[maxY].Length + this.text[i].Length + 1 > matrix[0].Length - 2 || this.text[i] == "$")
                {
                    while (matrix[maxY].Length < matrix[0].Length - 1)                        
                            matrix[maxY] += " ";
                    matrix[maxY] += "#";
                    maxY++;
                    matrix[maxY] += "#";
                }

                if (this.text[i] != "$")
                    matrix[maxY] += (" " + this.text[i]);
                else
                    matrix[maxY] += "";
            }
            while (matrix[maxY].Length < matrix[0].Length - 1)
                matrix[maxY] += " ";
            matrix[maxY] += "#";

            maxY++;
            matrix[maxY] = baseMatrix[3];
        }


        /// <summary>
        /// A mátrix egy sorának kiírása
        /// </summary>
        public void WriteAtLine(int i)
        {
            foreach (char c in matrix[i])
            {
                switch (c)
                {
                    case '#':
                        if (this.title == "Tutorial")
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
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }
                Console.Write(c);
            }
        }


        /// <summary>
        /// Az alaprajz betöltése
        /// </summary>
        private void LoadGraphics()
        {
            using (StreamReader r = new StreamReader(@"assets\screen_modules\textbox.txt"))
            {
                int i = 0;
                while (!r.EndOfStream)
                {
                    baseMatrix[i] = r.ReadLine();
                    i++;
                }
            }
        }
    }
}
