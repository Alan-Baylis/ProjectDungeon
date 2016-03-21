using Models;
using Models.Maps;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
public class Tile3dController : MonoBehaviour
{
  public GameObject cellPrefab;
  public List<WeightedPrefab> wallPrefabs;
  public GameObject wallCornerPrefab;
  public GameObject doorPrefab;

  public List<TileSet> TileSets;

  private Dictionary<Tile, GameObject> tileGameObjectMap;
  private Map _debugMap;
  private Map GameWorld
  {
    get
    {
      if (Application.isPlaying)
      {
        return WorldController.Instance.GameWorld;
      }
      else
      {
        if (_debugMap == null)
        {
          _debugMap = new Map(new MapSettings(16, 16, 3)
          {
            MapPoints = new List<MapPoint>()
            {
              new MapPoint { X = .2f, Y=.2f },
              new MapPoint { X = .8f, Y=.2f },
              new MapPoint { X = .8f, Y=.8f },
              new MapPoint { X = .2f, Y=.8f },
            },
            DoorPercentages = new List<int> { 50, 30, 20, 10 }
          });
          _debugMap.Generate();
        }
        return _debugMap;
      }
      
    }
  }
  // Use this for initialization
  void Start()
  {
    var total = wallPrefabs.Sum(x => x.Weight);
    wallPrefabs.ForEach(x => x.Percentage = Mathf.CeilToInt(((float)x.Weight / (float)total) * 100));
    wallPrefabs = wallPrefabs.OrderBy(x => x.Percentage).ToList();

    if (Application.isEditor)
    {
      BuildMap();
      return;
    }
    WorldController.Instance.MapUpdated += OnMapGenerated;

  }

  // Update is called once per frame
  void Update() { }

  private void OnMapGenerated(object sender, EventArgs e) { BuildMap(); }
  private void OnMapTileChanged(object sender, EventArgs e) { }

  private void BuildMap()
  {
    foreach (Transform child in transform)
    {
      GameObject.DestroyImmediate(child.gameObject);
    }

    if (tileGameObjectMap != null && tileGameObjectMap.Count > 0)
    {
      foreach (var k in tileGameObjectMap.Keys)
      {
        k.TileChanged -= OnMapTileChanged;
        Destroy(tileGameObjectMap[k]);
      }
      tileGameObjectMap.Clear();
    }
    var unitsize = GameWorld.Settings.UnitSize;
    tileGameObjectMap = new Dictionary<Tile, GameObject>();

    foreach (Room r in GameWorld.Rooms)
    {
      var tileSet = TileSets[UnityEngine.Random.Range(0, TileSets.Count)];

      GameObject roomObject = new GameObject();
      roomObject.name = "Room: " + r.Id;
      roomObject.transform.parent = transform;

      for (var x = r.X * unitsize; x < r.MaxX * unitsize; ++x)
      {
        for (var y = r.Y * unitsize; y < r.MaxY * unitsize; ++y)
        {
          var tile = GameWorld.GetTileAt(x, y);
          if (tile == null)
            continue;

          GameObject tileGameObject = new GameObject();
          tileGameObject.name = "Map Tile " + x + ", " + y;

          if (tile.Type == TileType.FLOOR)
          {
            GameObject floorGameObject = Instantiate(cellPrefab);
            floorGameObject.name = "Map Tile " + x + ", " + y + "_FLOOR";
            floorGameObject.transform.parent = tileGameObject.transform;
            floorGameObject.transform.localPosition = new Vector3(x - GameWorld.ActualWidth * 0.5f + 0.5f, 0f, y - GameWorld.ActualHeight * 0.5f + 0.5f);
            floorGameObject.GetComponentInChildren<MeshRenderer>().material = tileSet.FloorMaterial;
          }
          else if (tile.Type == TileType.WALL)
          {
            if (tile.Facing == TileFacing.NORTH || tile.Facing == TileFacing.EAST || tile.Facing == TileFacing.SOUTH || tile.Facing == TileFacing.WEST)
            {
              GameObject wallGameObject = Instantiate(GetWallPrefab());
              wallGameObject.name = "Map Wall " + x + ", " + y;
              wallGameObject.transform.parent = tileGameObject.transform;
              wallGameObject.transform.localPosition = new Vector3(x - GameWorld.ActualWidth * 0.5f + 0.5f, 0f, y - GameWorld.ActualHeight * 0.5f + 0.5f);
              wallGameObject.transform.localRotation = ToRotation(tile.Facing);
              var temp = wallGameObject.GetComponentsInChildren<MeshRenderer>();
              foreach (var mr in temp)
              {
                mr.material = tileSet.WallMaterial;
              }
            }
            else
            {
              GameObject wallGameObject = Instantiate(wallCornerPrefab);
              wallGameObject.name = "Map Wall Corner" + x + ", " + y;
              wallGameObject.transform.parent = tileGameObject.transform;
              wallGameObject.transform.localPosition = new Vector3(x - GameWorld.ActualWidth * 0.5f + 0.5f, 0f, y - GameWorld.ActualHeight * 0.5f + 0.5f);
              wallGameObject.transform.localRotation = ToRotation(tile.Facing);
              var temp = wallGameObject.GetComponentsInChildren<MeshRenderer>();
              foreach (var mr in temp)
              {
                mr.material = tileSet.WallMaterial;
              }
            }
          }
          else if (tile.Type == TileType.DOOR)
          {
            GameObject doorGameObject = Instantiate(doorPrefab);
            doorGameObject.name = "Map Door " + x + ", " + y;
            doorGameObject.transform.parent = tileGameObject.transform;
            doorGameObject.transform.localPosition = new Vector3(x - GameWorld.ActualWidth * 0.5f + 0.5f, 0f, y - GameWorld.ActualHeight * 0.5f + 0.5f);
            doorGameObject.transform.localRotation = ToRotation(tile.Facing);
          }

          tileGameObject.transform.parent = roomObject.transform;
          tileGameObjectMap.Add(tile, tileGameObject);
        }
      }
    }
    GameWorld.MapTileChanged += OnMapTileChanged;
  }

  private GameObject GetWallPrefab()
  {
    var rng = UnityEngine.Random.Range(0, 100);
    foreach (var p in wallPrefabs)
    {
      if (rng < p.Percentage)
        return p.Prefab;
      rng -= Mathf.CeilToInt(p.Percentage);
    }
    // As a fallback use last item;
    return wallPrefabs[wallPrefabs.Count - 1].Prefab;
  }

  private Mesh GenerateTerrain(int xSize, int ySize)
  {
    Mesh mesh = new Mesh();
    mesh.name = "Procedural Grid";
    var vertices = new Vector3[(xSize + 1) * (ySize + 1)];
    for (int i = 0, y = 0; y <= ySize; y++)
    {
      for (int x = 0; x <= xSize; x++, i++)
      {
        vertices[i] = new Vector3(x, y);
      }
    }
    mesh.vertices = vertices;

    int[] triangles = new int[xSize * ySize * 6];
    for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
    {
      for (int x = 0; x < xSize; x++, ti += 6, vi++)
      {
        triangles[ti] = vi;
        triangles[ti + 3] = triangles[ti + 2] = vi + 1;
        triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
        triangles[ti + 5] = vi + xSize + 2;
      }
    }
    mesh.triangles = triangles;
    return mesh;
  }

  private static Quaternion[] rotations = {
    Quaternion.identity,
    Quaternion.identity,
    Quaternion.Euler(0f, 90f, 0f),
    Quaternion.Euler(0f, 90f, 0f),
    Quaternion.Euler(0f, 180f, 0f),
    Quaternion.Euler(0f, 180f, 0f),
    Quaternion.Euler(0f, 270f, 0f),
    Quaternion.Euler(0f, 270f, 0f)
  };

  public static Quaternion ToRotation(TileFacing facing)
  {
    return rotations[(int)facing];
  }
}
