using System;
using Utilities.Pathfinding;
namespace Models
{
  public class Tile : IPathObject
  {
    public EventHandler TileChanged;

    public int X { get; set; }

    public int Y { get; set; }

    public TileType Type { get; set; }

    public Facing Facing { get; set; }

    public TileEdge[] Edges { get; set; }

    // Tiles are always the same size, so we can leave this as 1;
    public int Width { get { return 1; } }
    public int Height { get { return 1; } }
  }
}
