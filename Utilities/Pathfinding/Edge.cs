using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.Pathfinding
{
  public class Edge<T> where T : IPathObject
  {
    public float Cost;
    public Node<T> Node;
  }
}
