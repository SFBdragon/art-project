using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Extended.Generic;

namespace ArtProject
{
    public static class Processors
    {
        public static Stack<Vector2> IterateAngles(Vector2[] lsAngleLength, float iterateInRadians)
        {
            for (int i = 0; i < lsAngleLength.Length; i++)
                lsAngleLength[i].X += iterateInRadians;
            var returns = new Stack<Vector2>();
            foreach (Vector2 i in lsAngleLength)
                returns.Push(new Vector2((float)Math.Cos(i.X) * i.Y, (float)Math.Sin(i.X) * i.Y));
            return returns;
        }

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
                    returns[ExtendedMath.Modulus(x - distance, width), y].R = color.R;
                    returns[x, ExtendedMath.Modulus(y - distance, height)].G = color.G;
                    returns[ExtendedMath.Modulus(x + distance, width), ExtendedMath.Modulus(y + distance, height)].B = color.B;
                    returns[x, y].A = color.A;
                }
            }
            texture = returns;
        }
    }
}
