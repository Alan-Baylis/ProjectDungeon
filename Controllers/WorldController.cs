using UnityEngine;
using Models.Maps;
using System;
using System.Collections.Generic;

public interface IWorldController
{
  bool MapUpdateRequired { get; }
  EventHandler MapUpdated { get; set; }
  Map GameWorld { get; }
}

public abstract class BaseWorldController : MonoBehaviour, IWorldController
{
  public static IWorldController Instance { get; protected set; }

  public int ProgressPercent;
  public string ProgressText;
  public bool MapUpdateRequired { get; protected set; }
  public EventHandler MapUpdated { get; set; }
  public Map GameWorld { get; protected set; }
}

public class WorldController : BaseWorldController
{
  void Awake()
  {
    if (Instance != null)
      Debug.Log("SHIT, two world controllers exist.");
    Instance = this;

    GameWorld = new Map(new MapSettings(32, 32, 3)
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

    if (GameWorld.Generate())
      MapUpdateRequired = true;
  }

  void Update()
  {
    if (MapUpdateRequired && MapUpdated != null)
    {
      MapUpdated(this, EventArgs.Empty);
      MapUpdateRequired = false;
    }
  }
}
