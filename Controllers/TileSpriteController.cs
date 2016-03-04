using Models;
using Models.Maps;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
  public abstract class SpriteController : MonoBehaviour
  {

  }

  public class TileSpriteController : SpriteController
  {
    public Sprite floorSprite;
    public Sprite wallSprite;
    public Sprite debugSprite;

    private Dictionary<Tile, GameObject> tileGameObjectMap;
    private Map GameWorld { get { return WorldController.Instance.GameWorld; } }

    private void Start()
    {
      WorldController.Instance.MapUpdated += OnMapGenerated;
    }

    private void Update() { }

    private void OnMapTileChanged(object sender, EventArgs e)
    {
      var obj = sender as Tile;
      if (obj == null)
        return;

      if (!tileGameObjectMap.ContainsKey(obj))
        return;

      var gameObj = tileGameObjectMap[obj];
      gameObj.GetComponent<SpriteRenderer>().sprite = GetSpriteForTile(obj);
    }

    private void OnMapGenerated(object sender, EventArgs e)
    {
      BuildMap();
    }

    private void BuildMap()
    {
      if (tileGameObjectMap != null && tileGameObjectMap.Count > 0)
      {
        foreach (var k in tileGameObjectMap.Keys)
        {
          k.TileChanged -= OnMapTileChanged;
          Destroy(tileGameObjectMap[k]);
        }
        tileGameObjectMap.Clear();
      }
      tileGameObjectMap = new Dictionary<Tile, GameObject>();
      for (int x = 0; x < GameWorld.ActualWidth; ++x)
      {
        for (int y = 0; y < GameWorld.ActualHeight; ++y)
        {
          Tile tile = GameWorld.GetTileAt(x, y);
          if (tile == null)
            continue;

          GameObject tileGameObject = new GameObject();
          tileGameObject.name = string.Format("Tile_{0}_{1}", x, y);
          tileGameObject.transform.SetParent(transform);
          tileGameObject.transform.Translate(new Vector3(x, y, 1));

          tileGameObject.AddComponent<SpriteRenderer>().sprite = GetSpriteForTile(tile);
          tileGameObjectMap.Add(tile, tileGameObject);
        }
      }
      GameWorld.MapTileChanged += OnMapTileChanged;
    }

    private Sprite GetSpriteForTile(Tile t)
    {
      switch (t.Type)
      {
        case TileType.FLOOR:
          return floorSprite;
        case TileType.WALL:
          return wallSprite;
        case TileType.DEBUG:
          return debugSprite;
        default:
          return null;
      }
    }
  }
}
