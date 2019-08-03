using Microsoft.XNA.Framework;

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

        public static Color[,] PixelSort(Color[,] texture, byte valueThreshhold, bool aboveThreshhold, bool xAxis)
        {
            return;
        }
    }
}
