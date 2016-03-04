using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.Pathfinding
{
  /// <summary>
  /// Contains the coordinate information needed to perform path finding,
  /// Any object implementing this interface can be used with the pathfinding framework
  /// </summary>
  public interface IPathObject
  {
    /// <summary>
    /// The X co-ordinate of this object
    /// </summary>
    int X { get; }
    /// <summary>
    /// The Y co-ordinate of this object
    /// </summary>
    int Y { get; }

    /// <summary>
    /// The Width of the object
    /// </summary>
    int Width { get; }

    /// <summary>
    /// The Height of the object
    /// </summary>
    int Height { get; }
  }
}
