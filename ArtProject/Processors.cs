using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Collections;

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

        public static void PixelSort(ref Color[,] texture, float valueThreshhold, bool aboveThreshhold, bool reverseSort)
        {
            int width = texture.GetLength(0);
            int height = texture.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                Stack<Point> sections = new Stack<Point>();
                Point current = new Point(-1, -1);
                for (int x = 0; x < width; x++)
                {
                    var color = texture[x, y];
                    if (aboveThreshhold ? color.R + color.G + color.B > valueThreshhold * 255 :
                        color.R + color.G + color.B < valueThreshhold * 255)
                    {
                        if (current.X == -1) current.X = x;
                        else current.Y = x;
                    }
                    else if (current.Y != -1)
                    {
                        sections.Push(current);
                        current.X = -1;
                        current.Y = -1;
                    }
                }
                foreach (Point p in sections)
                {
                    var list = new List<Color>();
                    for (int i = p.X; i <= p.Y; i++)
                        list.Add(texture[i, y]);
                    if (reverseSort) list = list.OrderBy(color => color.R + color.G + color.B).ToList();
                    else list = list.OrderByDescending(color => color.R + color.G + color.B).ToList();
                    for (int i = p.X; i <= p.Y; i++)
                        texture[i, y] = list[i - p.X];
                }
            }
        }
    }
}
