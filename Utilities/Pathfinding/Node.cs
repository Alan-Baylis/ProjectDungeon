using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.Pathfinding
{
  public class Node<T> where T : IPathObject
  {
    public T Data;

    public List<Edge<T>> Edges;
  }
}
