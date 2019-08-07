using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

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
        
        public static void PixelSort(ref ColorHSV[,] texture, float valueThreshhold, bool aboveThreshhold, bool xAxis, bool reverseSort)
        {
            int width = texture.GetLength(0);
            int height = texture.GetLength(1);
            if(xAxis)
            {
                if(aboveThreshhold)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Stack<Point> sections = new Stack<Point>();
                        Point current = new Point(-1, -1);
                        for (int x = 0; x < width; x++)
                        {
                            if (texture[x, y].V > valueThreshhold)
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
                        foreach(Point p in sections)
                        {
                            var list = new List<ColorHSV>();
                            var delta = p.Y - p.X;
                            for (int i = p.X; i <= p.Y; i++)
                                list.Add(texture[i, y]);
                            list.OrderBy(color => color.V);
                            if (reverseSort) list.Reverse();
                            for (int i = p.X; i <= p.Y; i++)
                                texture[i, y] = list[i - p.X];
                        }
                    }
                }
                else
                {
                    for (int y = 0; y < height; y++)
                    {
                        Stack<Point> sections = new Stack<Point>();
                        Point current = new Point(-1, -1);
                        for (int x = 0; x < width; x++)
                        {
                            if (texture[x, y].V < valueThreshhold)
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
                            var list = new List<ColorHSV>();
                            var delta = p.Y - p.X;
                            for (int i = p.X; i <= p.Y; i++)
                                list.Add(texture[i, y]);
                            list.OrderBy(color => color.V);
                            if (reverseSort) list.Reverse();
                            for (int i = p.X; i <= p.Y; i++)
                                texture[i, y] = list[i - p.X];
                        }
                    }
                }
            }
            else
            {
                if (aboveThreshhold)
                {
                    for (int x = 0; x < height; x++)
                    {
                        Stack<Point> sections = new Stack<Point>();
                        Point current = new Point(-1, -1);
                        for (int y = 0; y < width; y++)
                        {
                            if (texture[x, y].V > valueThreshhold)
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
                            var list = new List<ColorHSV>();
                            var delta = p.Y - p.X;
                            for (int i = p.X; i <= p.Y; i++)
                                list.Add(texture[x, i]);
                            list.OrderBy(color => color.V);
                            if (reverseSort) list.Reverse();
                            for (int i = p.X; i <= p.Y; i++)
                                texture[x, i] = list[i - p.X];
                        }
                    }
                }
                else
                {
                    for (int x = 0; x < height; x++)
                    {
                        Stack<Point> sections = new Stack<Point>();
                        Point current = new Point(-1, -1);
                        for (int y = 0; y < width; y++)
                        {
                            if (texture[x, y].V < valueThreshhold)
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
                            var list = new List<ColorHSV>();
                            var delta = p.Y - p.X;
                            for (int i = p.X; i <= p.Y; i++)
                                list.Add(texture[x, i]);
                            list.OrderBy(color => color.V);
                            if (reverseSort) list.Reverse();
                            for (int i = p.X; i <= p.Y; i++)
                                texture[x, i] = list[i - p.X];
                        }
                    }
                }
            }
        }
    }
}
