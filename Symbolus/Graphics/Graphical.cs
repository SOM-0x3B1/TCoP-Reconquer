using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Symbolus
{
    /// <summary>
    /// Statikus karakter-rajzok háttérnek
    /// </summary>
    public class Background : Displayable
    {
        public string name;

        public Background(string name)
        {
            this.name = name;

            using (StreamReader r = new StreamReader(@"assets\bg\" + name + ".txt", Encoding.UTF8)) //beolvasás egy lista alapján
            {
                int i = 0;
                BasicMatrixBuilder(r, ref i);

                backgrounds.Add(name, this);
            }
        }

        public static Dictionary<string, Background> backgrounds = new Dictionary<string, Background>(); //betöltött hátterek
    }


    /// <summary>
    /// Ellenfeleken megjenlő (1 képkockás) animáció; pl kard, heal, pajzs
    /// </summary>
    public class Sticker : Displayable
    {
        public string name;

        public Sticker() { }
        public Sticker(string name)
        {
            this.name = name;

            using (StreamReader r = new StreamReader(@"assets\stickers\" + name + ".txt", Encoding.UTF8)) //beolvasás egy lista alapján
            {
                int i = 0;
                BasicMatrixBuilder(r, ref i);
            }
        }

        /// <summary>
        /// Matrica lehívása menet közben
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Sticker GetSticker(string name)
        {
            if (!stickers.ContainsKey(name))            
                stickers.Add(name, new Sticker(name));            

            return stickers[name];
        }

        public static Sticker GetVariableSticker(string name, string x)
        {
            Sticker temp = GetSticker(name);
            Sticker result = new Sticker();

            result.maxY = temp.maxY;
            result.name = "temp";

            for (int j = 0; j <= temp.maxY; j++)
            {
                for (int i = 0; i < temp.maxX; i++)
                {
                    if (temp.matrix[j][i] == '&')
                    {
                        for (int k = 0; k < x.Length; k++)
                        {
                            result.matrix[j] += x[k];
                            i++;
                        }
                        i--;
                    }
                    else
                        result.matrix[j] += temp.matrix[j][i];
                }
            }
            return result;
        }

        public static Dictionary<string, Sticker> stickers = new Dictionary<string, Sticker>(); //betöltött matricák
    }   
}
