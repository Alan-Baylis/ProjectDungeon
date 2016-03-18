using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities.Pathfinding
{
  public class TileGraph<T> where T : IPathObject
  {
    public Dictionary<T, Node<T>> Nodes;
    public TileGraph(IEnumerable<T> objects, Func<T, List<T>> neighbourFunction, Func<T, float> costFunction)
    {
      Nodes = new Dictionary<T, Node<T>>();
      foreach (var obj in objects)
      {
        Nodes.Add(obj, new Node<T> { Data = obj });
      }

      foreach (var r in Nodes.Keys)
      {
        var pn = Nodes[r];
        pn.Edges = new List<Edge<T>>();

        foreach (var e in from t in neighbourFunction(r)
                          let cost = costFunction(t)
                          where cost > 0
                          select new Edge<T>
                          {
                            Cost = cost,
                            Node = Nodes[t]
                          })
        {
          pn.Edges.Add(e);
        }
      }
    }
  }
}
