using UnityEngine;
using System.Collections;
using System;

namespace Models
{
  /// <summary>
  /// Room tile set consisting of a floor material and a wall material
  /// </summary>
  [Serializable]
  public class TileSet
  {
    /// <summary>
    ///  The Floor Material
    /// </summary>
    public Material FloorMaterial;

    /// <summary>
    /// The Wall Material
    /// </summary>
    public Material WallMaterial;
  }

  [Serializable]
  public class WeightedPrefab
  {
    public int Weight;
    public GameObject Prefab;

    public float Percentage { get; set; }
  }
}
