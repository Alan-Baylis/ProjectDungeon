using System;
using Utilities.Pathfinding;
namespace Models
{
  public enum TileType : int
  {
    FLOOR, WALL, DOOR, DEBUG
  }

  public enum TileFacing
  {
    NORTH,
    NORTHEAST,
    EAST,
    SOUTHEAST,
    SOUTH,
    SOUTHWEST,
    WEST,
    NORTHWEST
  }

  public class Tile : IPathObject
  {
    public EventHandler TileChanged;

    public int X { get; set; }

    public int Y { get; set; }

    public TileType Type { get; set; }

    public TileFacing Facing { get; set; }

    // Tiles are always the same size, so we can leave this as 1;
    public int Width { get { return 1; } }
    public int Height { get { return 1; } }
  }
}
