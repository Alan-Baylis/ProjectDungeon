using Utilities.Pathfinding;
using System;

namespace Utilities
{
  public class Rectangle : IPathObject
  {
    public int X { get; set; }
    public int Y { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }

    public Rectangle() { }

    public Rectangle(int x, int y, int width, int height)
    {
      X = x;
      Y = y;
      Width = width;
      Height = height;
    }

    public static Rectangle Intersect(Rectangle r1, Rectangle r2)
    {
      Rectangle r3 = new Rectangle();
      if (((r1.X >= r2.X && r1.X <= r2.X + r2.Width) || (r2.X >= r1.X && r2.X <= r1.X + r1.Width))
        &&
        ((r1.Y >= r2.Y && r1.Y <= r2.Y + r2.Height) || (r2.Y >= r1.Y && r2.Y <= r1.Y + r1.Height)))
      {
        int x1 = Math.Min(r1.X + r1.Width, r2.X + r2.Width);
        int x2 = Math.Max(r1.X, r2.X);
        int y1 = Math.Min(r1.Y + r1.Height, r2.Y + r2.Height);
        int y2 = Math.Max(r1.Y, r2.Y);
        r3.X = Math.Min(x1, x2);
        r3.Y = Math.Min(y1, y2);
        r3.Width = Math.Max(0, x1 - x2);
        r3.Height = Math.Max(0, y1 - y2);
      }
      return r3;
    }

    public bool Intersects(int x, int y)
    {
      return ((x >= X && x <= X + Width) && (y >= Y && y <= Y + Height));

    }
  }
}
