using System.Collections.Generic;

namespace Models.Maps
{
  public class RoomPrototype
  {

    public int Width { get; set; }
    public int Height { get; set; }

    public Tile[,] Tiles { get; set; }

    public static List<RoomPrototype> CreateTestRoomPrototypes()
    {
      var temp = new List<RoomPrototype>();

      var p1 = new RoomPrototype()
      {
        Height = 15,
        Width = 15,
      };
      return temp;
    }
  }
}
