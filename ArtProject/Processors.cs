using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Extended.Generic;

namespace ArtProject
{
    public static class Processors
    {
        /// <summary>
        /// Each Point[] corresponds to its Y position
        /// </summary>
        public static Queue<Point[]> GetSortQueue(Color[,] texture, float valueThreshhold, bool aboveThreshhold)
        {
            int width = texture.GetLength(0);
            int height = texture.GetLength(1);
            var returns = new Queue<Point[]>();

            for (int y = 0; y < height; y++)
            {
                List<Point> sections = new List<Point>();
                Point current = new Point(-1, -1);
                for (int x = 0; x < width; x++)
                {
                    var color = texture[x, y];
                    if (aboveThreshhold ? color.R + color.G + color.B > valueThreshhold * 765 :
                        color.R + color.G + color.B < valueThreshhold * 255)
                    {
                        if (current.X == -1) current.X = x;
                        else current.Y = x;
                    }
                    else if (current.Y != -1)
                    {
                        sections.Add(current);
                        current.X = -1;
                        current.Y = -1;
                    }
                }
                returns.Enqueue(sections.ToArray());
            }
            return returns;
        }

        public static void PixelSort(ref Color[,] texture, Point section, int y, bool reverse)
        {
            var list = new List<Color>();
            for (int i = section.X; i <= section.Y; i++)
                list.Add(texture[i, y]);
            if (!reverse) list = list.OrderBy(color => color.R + color.G + color.B).ToList();
            else list = list.OrderByDescending(color => color.R + color.G + color.B).ToList();
            for (int i = section.X; i <= section.Y; i++)
                texture[i, y] = list[i - section.X];
        }
        public static void ArrayPixelSort(ref Color[,] texture, Point[] sections, int y, bool reverse)
        {
            foreach (Point p in sections)
            {
                var list = new List<Color>();
                for (int i = p.X; i <= p.Y; i++)
                    list.Add(texture[i, y]);
                if (reverse) list = list.OrderByDescending(color => color.R + color.G + color.B).ToList();
                else list = list.OrderBy(color => color.R + color.G + color.B).ToList();
                for (int i = p.X; i <= p.Y; i++)
                    texture[i, y] = list[i - p.X];
            }
        }
        public static void QueueStackPixelSort(ref Color[,] texture, Queue<Point[]> sections, bool reverse)
        {
            int y = 0;
            while (sections.Count > 0)
            {
                foreach (Point p in sections.Dequeue())
                {
                    var list = new List<Color>();
                    for (int i = p.X; i <= p.Y; i++)
                        list.Add(texture[i, y]);
                    if (!reverse) list = list.OrderBy(color => color.R + color.G + color.B).ToList();
                    else list = list.OrderByDescending(color => color.R + color.G + color.B).ToList();
                    for (int i = p.X; i <= p.Y; i++)
                        texture[i, y] = list[i - p.X];
                }
                y++;
            }
        }

        public static void ColorSplit(ref Color[,] texture, int distance)
        {
            var width = texture.GetLength(0);
            var height = texture.GetLength(1);
            var returns = new Color[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var color = texture[x, y];
                    returns[MathExtended.Modulo(x - distance, width), y].R = color.R;
                    returns[x, MathExtended.Modulo(y - distance, height)].G = color.G;
                    returns[MathExtended.Modulo(x + distance, width), MathExtended.Modulo(y + distance, height)].B = color.B;
                    returns[x, y].A = color.A;
                }
            }
            texture = returns;
        }

        public static void TextureScramble(ref Color[,] texture)
        {
            // cache width and height
            var width = texture.GetLength(0);
            var height = texture.GetLength(1);

            // "incorrectly" translate the data into a single-dimension array
            var array = new Color[width * height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    array[x + y * width] = texture[x, y];

            // set data back into a 2d array (correctly, to retain the scramble)
            for (int i = 0; i < width * height; i++)
                texture[i / height, i % height] = array[i];
        }

        public static void TextureDescramble(ref Color[,] texture)
        {
            // cache width and height
            var width = texture.GetLength(0);
            var height = texture.GetLength(1);

            // "incorrectly" translate the data into a single-dimension array
            var array = new Color[width * height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    array[x * height + y] = texture[x, y];

            // set data back into a 2d array (correctly, to retain the descramble)
            for (int i = 0; i < width * height; i++)
                texture[i % width, i / width] = array[i];
        }

        public static void Pixelate(ref Color[,] texture, int pixel_size)
        {
            var width = texture.GetLength(0);
            var height = texture.GetLength(1);

            for (int x = 0; x < width / pixel_size; x++)
                for (int y = 0; y < height / pixel_size; y++)
                {
                    float sumR = 0;
                    float sumG = 0;
                    float sumB = 0;
                    float sqre = pixel_size * pixel_size;
                    for (int _x = 0; _x < pixel_size; _x++)
                        for (int _y = 0; _y < pixel_size; _y++)
                        {
                            sumR += texture[x * pixel_size + _x, y * pixel_size + _y].R / sqre;
                            sumG += texture[x * pixel_size + _x, y * pixel_size + _y].G / sqre;
                            sumB += texture[x * pixel_size + _x, y * pixel_size + _y].B / sqre;
                        }
                    for (int _x = 0; _x < pixel_size; _x++)
                        for (int _y = 0; _y < pixel_size; _y++)
                        {
                            texture[x * pixel_size + _x, y * pixel_size + _y].R = (byte)sumR;
                            texture[x * pixel_size + _x, y * pixel_size + _y].G = (byte)sumG;
                            texture[x * pixel_size + _x, y * pixel_size + _y].B = (byte)sumB;
                        }
                }
        }

        public static void ColorFloor(ref Color[,] texture, byte shift)
        {   
            // for every pixel
            for (int x = 0; x < texture.GetLength(0); x++)
            {
                for (int y = 0; y < texture.GetLength(1); y++)
                {
                    // grab the colour
                    var colour = texture[x, y];
                    // bitshift
                    colour.R = (byte)((colour.R >> shift) << shift);
                    colour.G = (byte)((colour.G >> shift) << shift);
                    colour.B = (byte)((colour.B >> shift) << shift);
                    // set the colour to the texture
                    texture[x, y] = colour;
                }
            }
        }
    }
}
