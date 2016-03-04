using Utilities.Pathfinding;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace Models.Maps
{
  public class Room : IPathObject
  {
    public List<Door> Doors { get; set; }
    public List<Room> Neighbours;

    public int Id { get; set; }

    /// <summary>
    /// The X co-ordinate of this room in Map Units
    /// </summary>
    public int X { get; set; }
    /// <summary>
    /// The Y co-ordinate of this room in Map Units
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// The width of this room in Map Units
    /// </summary>
    public int Width { get; set; }
    /// <summary>
    /// The height of tihs room in Map Units;
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// The max size of the X axis for this room
    /// </summary>
    public int MaxX { get { return X + Width; } }

    /// <summary>
    /// the max size of the Y axis for this room
    /// </summary>
    public int MaxY { get { return Y + Height; } }

    /// <summary>
    /// The cost to move through this room, between 0 and 1
    /// This will be used to create the path through the map
    /// </summary>
    public float Difficulty;

    public Rectangle BoundingRect
    {
      get { return new Rectangle(X, Y, Width, Height); }
    }

    public Room(int width, int height)
    {
      Width = width;
      Height = height;
      Doors = new List<Door>();
    }

    public Room(int x, int y, int width, int height)
      : this(width, height)
    {
      X = x;
      Y = y;
    }

    public void InitRoom(int x, int y, int id)
    {
      X = x;
      Y = y;
      Id = id;
    }

    public void InitNeighbours(List<Room> rooms, int[,] roomMap)
    {
      var mapHeight = roomMap.GetLength(1);
      var mapWidth = roomMap.GetLength(0);
      // We are not allowing diagonal travel, 
      // but if the width/height of a tile is more than 1 it may have more than 1 neighbour on that side
      var neighbourIds = new List<int>();
      // This is a 1x1 room so we can quite simply work out any neighbours.
      if (Width == 1)
      {
        var nX = X;
        var nY = Y + Height;
        if (nY < mapHeight)
          neighbourIds.Add(roomMap[nX, nY]);

        nY = Y - 1;
        if (nY >= 0)
          neighbourIds.Add(roomMap[nX, nY]);
      }
      else
      {
        var nY = Y + Height;
        if (nY < mapHeight)
        {
          for (var nX = X; nX < X + Width; ++nX)
          {
            neighbourIds.Add(roomMap[nX, nY]);
          }
        }
        nY = Y - 1;
        if (nY >= 0)
        {
          for (var nX = X; nX < X + Width; ++nX)
          {
            neighbourIds.Add(roomMap[nX, nY]);
          }
        }
      }
      if (Height == 1)
      {
        var nX = X + Width;
        var nY = Y;
        if (nX < mapWidth)
          neighbourIds.Add(roomMap[nX, nY]);

        nX = X - 1;
        if (nX >= 0)
          neighbourIds.Add(roomMap[nX, nY]);
      }
      else
      {
        var nX = X + Width;
        if (nX < mapWidth)
        {
          for (var y = Y; y < Y + Height; ++y)
          {
            neighbourIds.Add(roomMap[nX, y]);
          }
        }
        nX = X - 1;
        if (nX >= 0)
        {
          for (var nY = Y; nY < Y + Height; ++nY)
          {
            neighbourIds.Add(roomMap[nX, nY]);
          }
        }
      }

      neighbourIds = neighbourIds.Distinct().ToList();
      Neighbours = new List<Room>(rooms.Where(r => neighbourIds.Contains(r.Id)));
    }

    public float GetRoomCost()
    {
      return Difficulty;
    }
  }
}
