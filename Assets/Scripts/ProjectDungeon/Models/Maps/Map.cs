using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using Utilities;
using Utilities.Pathfinding;

namespace Models.Maps
{
  /// <summary>
  ///  Class containing all of the settings required to create a map.
  /// </summary>
  public class MapSettings
  {
    /// <summary>
    /// The Width of the map in MapUnits
    /// </summary>
    public int Width { get; set; }
    /// <summary>
    /// The Height of the map in MapUnits
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// The map seed
    /// </summary>
    public int Seed { get; set; }

    /// <summary>
    /// The size in tiles of each MapUnit
    /// </summary>
    public int UnitSize { get; set; }

    /// <summary>
    /// The percentage chance that a door will appear for each room out from the main room
    /// a list with two entries will add neighbours up to 2 rooms out from the main path.
    /// </summary>
    public List<int> DoorPercentages { get; set; }

    /// <summary>
    /// The map points that must be in this map
    /// </summary>
    public List<MapPoint> MapPoints { get; set; }

    /// <summary>
    /// Constructs a default map for testing
    /// </summary>
    public MapSettings(int width = 24, int height = 16, int unitSize = 5, int seed = 1)
    {
      Width = width;
      Height = height;
      Seed = seed;
      UnitSize = unitSize;
    }
  }

  /// <summary>
  /// Class containing information about a key point on the map
  /// </summary>
  public class MapPoint
  {
    /// <summary>
    /// The X coordinate of this point, between 0 and 1;
    /// </summary>
    public float X { get; set; }
    /// <summary>
    /// The Y coordinate of this point, between 0 and 1;
    /// </summary>
    public float Y { get; set; }
    /// <summary>
    /// string containing the information about this room, e.g. "Start", "End", "Boss"
    /// can be left blank
    /// </summary>
    public string RoomInfo { get; set; }
  }

  public class Map
  {
    public EventHandler MapTileChanged;
    /// <summary>
    /// Maximum number of attempts to take per room type;
    /// </summary>
    const int MAX_RETRY_ATTEMPTS = 10000;

    public MapSettings Settings;
    /// <summary>
    /// Stores information about what room occupies what tile
    /// used for quick neighbour finding and checking for valid placements
    /// </summary>
    private int[,] placementMap;

    /// <summary>
    /// Array of all possible rooms
    /// </summary>
    private Room[] roomPrototypes;

    /// <summary>
    /// All rooms within this map
    /// </summary>
    public List<Room> Rooms { get; protected set; }

    /// <summary>
    /// The Tiles that make up the rooms within this Map
    /// </summary>
    private Tile[,] tileMap { get; set; }

    /// <summary>
    /// The full width of the map, in tiles
    /// </summary>
    public int ActualWidth { get { return Settings.Width * Settings.UnitSize; } }
    /// <summary>
    /// The full height of the map, in tiles
    /// </summary>
    public int ActualHeight { get { return Settings.Height * Settings.UnitSize; } }

    /// <summary>
    /// Initializes the list of rooms available to play
    /// </summary>
    private void InitRoomPrototypes()
    {
      roomPrototypes = new Room[]
      {
        new Room(5,5),
        new Room(5,3),
        new Room(3, 3),
        new Room(2, 3),
        new Room(2, 2),
        new Room(2, 1),
        new Room(1, 1),
        };
    }

    public Map(MapSettings settings)
    {
      this.Settings = settings;
      placementMap = new int[settings.Width, settings.Height];
      InitRoomPrototypes();
    }

    /// <summary>
    /// Generates a new map
    /// </summary>
    /// <returns>True if the map was generated, otherwise false</returns>
    public bool Generate()
    {
      // Grab the settings values for settings that we use very frequently
      var width = Settings.Width;
      var height = Settings.Height;
      var unitSize = Settings.UnitSize;
      // init
      var rooms = new List<Room>();
      var roomPath = new List<Room>();
      var rng = new System.Random(Settings.Seed);
      var i = 0;
      var roomPlaced = false;
      var retryCount = MAX_RETRY_ATTEMPTS;

      // Step 1 - Create the basic path that the player will need to take through the map
      // Iterate through all of the map points provided and add a basic room for each of them
      // TODO: configure the size of the room and assign the roominfo string to the generated room.
      foreach (var mapPoint in Settings.MapPoints)
      {
        var r = new Room(1, 1);
        //Convert the map points into coordinates;
        var mpX = Mathf.FloorToInt(mapPoint.X * width);
        var mpY = Mathf.FloorToInt(mapPoint.Y * height);

        //generate coordinates around this point;
        while (true)
        {
          int rX = rng.Next(Math.Max(mpX - 5, 0), Math.Min(mpX + 5, width));
          int rY = rng.Next(Math.Max(mpY - 5, 0), Math.Min(mpY + 5, height));

          if (ValidatePlacement(rX, rY, r))
          {
            r.InitRoom(rX, rY, ++i);
            FillMap(r);
            rooms.Add(r);
            roomPath.Add(r);
            break;
          }
          retryCount--;
          // We failed to place the room, don't continue generation;
          // This is only likely to happen if the room size given is too small;
          if (retryCount == 0)
            return false;
        }
      }

      // Step 2: Fill the room list till there is no more space, start with large rooms and then move onto smaller rooms.
      // Due to the large number of passes this should end up filling a map with rooms, however there may be some empty spaces on certain seeds.
      // It will be more efficient to place all the 1x1 rooms manually. TODO: IMPLEMENT THIS
      var roomIndex = 0;
      retryCount = MAX_RETRY_ATTEMPTS;
      while (true)
      {
        if (retryCount == 0 && roomPlaced == false)
        {
          roomIndex++;
          if (roomIndex == roomPrototypes.Length)
          {
            // We've failed to place the smallest room within the number of retrys
            // There is a very small chance that we haven't placed all the rooms and that there is no path to the end.
            // but its so rare that its not work accounting for, as i will update to the fix above before it becomes a problem.
            break;
          }
          if (roomIndex == roomPrototypes.Length - 1)
          {
            // We are on the last room prototype which is the smallest room that we can place.
            // instead of randomly placing rooms i will just fill them in 1 by 1;
            var proto = roomPrototypes[roomIndex];
            for (int x = 0; x < width; ++x)
            {
              for (int y = 0; y < height; ++y)
              {
                // Create the room here and then loop through till a match is found
                // This way we are not creating lots of instances of room saving us some garbage collection calls.
                Room r = new Room(proto.Width, proto.Height);
                while (true)
                {
                  if (ValidatePlacement(x, y, r))
                  {
                    r.InitRoom(x, y, ++i);
                    FillMap(r);
                    rooms.Add(r);
                    break;
                  }
                  else if (++y < width)
                    continue;
                  else
                    break;
                }
              }
            }
            break;
          }
        }
        else
        {
          roomPlaced = false;
        }
        retryCount = MAX_RETRY_ATTEMPTS;
        // Attempt to place a room.
        // 50% of the time, rotate the room.
        // Otherwise we would have to have a 2 prototypes per room, 1 for portrait and 1 for landscape.
        // This would skew distribution of buildings as rectangle builds would have twice as many opportunities than square ones.
        var room = rng.Next(0, 2) == 1
          ? new Room(roomPrototypes[roomIndex].Width, roomPrototypes[roomIndex].Height)
          : new Room(roomPrototypes[roomIndex].Height, roomPrototypes[roomIndex].Width);

        do
        {
          var roomX = rng.Next(0, width);
          var roomY = rng.Next(0, height);
          if (ValidatePlacement(roomX, roomY, room))
          {
            // Room fits add it to the list
            room.InitRoom(roomX, roomY, ++i);
            FillMap(room);
            rooms.Add(room);
            roomPlaced = true;
            break;
          }
          retryCount--;
        } while (retryCount > 0);
      }
      // Step 3: assign a difficulty to each room and calculate their neighbours
      rooms.ForEach(r =>
      {
        r.Difficulty = rng.Next(1, 100);
        r.InitNeighbours(rooms, placementMap);
      });

      // Step 4 Create the graph for use with the path finding algorithm (either Dijkstra or A*)
      // Dijkstra will always give us the shortest, however A* is faster.
      var tileGraph = new TileGraph<Room>(rooms, (x) => x.Neighbours, (x) => x.GetRoomCost());

      // Step 5: Use the path finding algorithm to find the shortest path between all of the nodes in the path.
      var pathAStar = new PathFinderAStar<Room>(tileGraph);
      var path = pathAStar.PathBetween(roomPath);
      if (path == null || path.Count == 0)
        return false;

      // Step 6: Dequeue the rooms from the shortest path and work out where their doors need to be place.
      // At this point we will also determine which rooms will branch out
      Rooms = new List<Room>();
      var next = path.Dequeue();
      if (next == null)
        return false;

      do
      {
        // Get the current node from the list and if we are not working with the last node grab the next node too.
        var current = next;
        next = path.Count > 0 ? path.Dequeue() : null;

        if (next != null)
        {
          // Add a door between the two rooms
          AddDoorBetweenRooms(current, next);
        }

        // Add the current room to the list and randomly add some of its neighbours.
        if (!Rooms.Contains(current))
        {
          Rooms.Add(current);
        }
        AddNeighbours(rng, Settings.DoorPercentages, current, next);
      } while (next != null);

      return BuildTilemap();
    }

    /// <summary>
    /// Returns the tile at the specified coordinates
    /// </summary>
    /// <param name="x">The x coordinate</param>
    /// <param name="y">The y coordinate</param>
    /// <returns>The tile or NULL if there is no tile or the coordinates are out of bounds</returns>
    public Tile GetTileAt(int x, int y)
    {
      if (x >= 0 && x < ActualWidth && y >= 0 && y < ActualHeight)
        return tileMap[x, y];
      return null;
    }

    /// <summary>
    /// Validates the placement of the room, 
    /// ensures that it doesnt collide with any previously placed rooms or go out of bounds
    /// </summary>
    /// <param name="x">X Co-ordinate of the room</param>
    /// <param name="y">Y Co-ordinate of the room</param>
    /// <param name="r">The Room prototype we are trying to place</param>
    /// <returns>True if the room is valid in this placement, otherwise false</returns>
    private bool ValidatePlacement(int x, int y, Room r)
    {
      for (var i = x; i < x + r.Width; ++i)
      {
        for (var j = y; j < y + r.Height; ++j)
        {
          if (i >= Settings.Width || j >= Settings.Height)
            return false;
          if (placementMap[i, j] != 0)
            return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Fills up the placement map with this rooms information
    /// This will prevent other rooms from being placed on top
    /// </summary>
    /// <param name="room">The room we are placing</param>
    private void FillMap(Room room)
    {
      for (var x = room.X; x < room.X + room.Width; ++x)
      {
        for (var y = room.Y; y < room.Y + room.Height; ++y)
        {
          placementMap[x, y] = room.Id;
        }
      }
    }

    /// <summary>
    /// Adds a door to each of the rooms connecting them
    /// </summary>
    /// <param name="current">The first room</param>
    /// <param name="next">the second room</param>
    private void AddDoorBetweenRooms(Room current, Room next)
    {
      // Get the intersection of the two bounding boxes, this will give us the line that they share
      var r1 = Rectangle.Intersect(current.BoundingRect, next.BoundingRect);
      // The new line is in world co-ordinates so we need to get it relative to each of the rooms.
      current.Doors.Add(new Door
      {
        X = r1.X - current.X,
        Y = r1.Y - current.Y,
        Width = r1.Width,
        Height = r1.Height
      });
      next.Doors.Add(new Door
      {
        X = r1.X - next.X,
        Y = r1.Y - next.Y,
        Width = r1.Width,
        Height = r1.Height
      });
    }

    /// <summary>
    /// Recursively iterates through all of the neighbours of the current room, randonly adding door between rooms.
    /// </summary>
    /// <param name="rng"> The RNG to use</param>
    /// <param name="roomPercentages">
    /// A list of percentage values, that specify the chance for a door to appear, the function will go as deep as there are entries in the list
    /// </param>
    /// <param name="current">the current room we are processing</param>
    /// <param name="next">the next room in the path (we don't want to modify that rooms doors in this function)</param>
    /// <param name="index"> The current index within room percentages we are dealing with, starts at 0</param>
    private void AddNeighbours(System.Random rng, List<int> roomPercentages, Room current, Room next, int index = 0)
    {
      foreach (var neighbour in current.Neighbours)
      {
        if (rng.Next(100) > roomPercentages[0])
          continue;
        if (Rooms.Any(x => x.Id == neighbour.Id))
          continue;

        Rooms.Add(neighbour);
        AddDoorBetweenRooms(current, neighbour);

        var newIndex = index + 1;
        if (newIndex < roomPercentages.Count)
          AddNeighbours(rng, roomPercentages, neighbour, next, newIndex);
      }
    }

    /// <summary>
    /// Generates the tile map from the rooms within this map.
    /// Used for 2d implementations of this map.
    /// </summary>
    private bool BuildTilemap()
    {
      if (Rooms == null || Rooms.Count == 0)
      {
        Debug.LogError("Map Rooms not initialized, did you just seriously try to call build tile map before building damn map?");
        return false;
      }

      var width = Settings.Width;
      var height = Settings.Height;
      var unitSize = Settings.UnitSize;

      tileMap = new Tile[width * unitSize, height * unitSize];
      // First pass create the basic rooms
      foreach (var room in Rooms)
      {
        //TODO: use prefabs for this.
        var roomW = room.Width * unitSize;
        var roomH = room.Height * unitSize;
        for (var x = 0; x < roomW; ++x)
        {
          for (var y = 0; y < roomH; ++y)
          {
            var tile = tileMap[x + (room.X * unitSize), y + (room.Y * unitSize)];
            if (tile != null)
            {
              tile.Type = TileType.DEBUG;
              continue;
            }

            tile = new Tile();
            tile.TileChanged += MapTileChanged;

            // TODO: Since moving the walls and doors to edges, there is no other tile type yet
            // TODO: When we add other types such as Empty or Water or Lava etc we'll need to add them here.
            tile.Type = TileType.FLOOR;

            // The Tiles on the edge are still going to be floors, but they need to have walls added to their edges.
            if (x == 0 || y == 0 || x == roomW - 1 || y == roomH - 1)
            {
              if (x == 0)
              {
                if (y == 0)
                {
                  tile.Edges = new[] { new TileEdge(Facing.WEST), new TileEdge(Facing.SOUTH) };
                }
                else if (y == roomH - 1)
                {
                  tile.Edges = new[] { new TileEdge(Facing.WEST), new TileEdge(Facing.NORTH) };
                }
                else
                {
                  tile.Edges = new[]{new TileEdge(Facing.WEST)};
                }
              }
              else if (x == roomW - 1)
              {
                if (y == 0)
                {
                  tile.Edges = new[]{new TileEdge(Facing.EAST),new TileEdge(Facing.SOUTH)};
                }
                else if (y == roomH - 1)
                {
                  tile.Edges = new[]{new TileEdge(Facing.EAST),new TileEdge(Facing.NORTH)};
                }
                else
                {
                  tile.Edges = new[]{new TileEdge(Facing.EAST)};
                }
              }
              else if (y == 0)
              {
                tile.Edges = new[] { new TileEdge(Facing.SOUTH) };
              }
              else if (y == roomH - 1)
              {
                tile.Edges = new[] { new TileEdge(Facing.NORTH) };
              }
            }
            tileMap[x + room.X * unitSize, y + room.Y * unitSize] = tile;
          }
        }
      }
      //// iterate through all of the doors in this room and place them down in the correct location.
      //foreach (Door d in room.Doors)
      //{
      //  int doorX, doorY;
      //  if (d.Width == 0)
      //  {
      //    // This offset centers the door along the shared edge.
      //    int yOffset = ((d.Height * unitSize) - 1) / 2;
      //    //dealing with a door going left/right
      //    doorX = (room.X + d.X) * unitSize;
      //    doorY = ((room.Y + d.Y) * unitSize) + yOffset;
      //  }
      //  else
      //  {
      //    // This offset centers the door along the shared edge.
      //    int xOffset = ((d.Width * unitSize) - 1) / 2;
      //    // Dealing with a door going up/down
      //    doorX = (room.X + d.X) * unitSize + xOffset;
      //    doorY = (room.Y + d.Y) * unitSize;
      //  }
      //  // Due to how the doors are stored, in relation to the room, 
      //  // doors on the top and right edges of a room actually end up in the next room over
      //  // We need to catch this here and move the door back into its own room.
      //  if (doorX == (room.X + room.Width) * unitSize)
      //    doorX--;
      //  if (doorY == (room.Y + room.Height) * unitSize)
      //    doorY--;
      //  var tile = tileMap[doorX, doorY];
      //  if (tile == null)
      //  {
      //    Debug.LogErrorFormat("Failed to find tile_{0}_{1}", doorX, doorY);
      //    continue;
      //  }
      //  tile.Type = TileType.DOOR;
      //}
      return true;
    }
  }
}
