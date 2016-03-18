using System;
using System.Collections.Generic;
using System.Linq;
using Priority_Queue;

namespace Utilities.Pathfinding
{
  public class PathFinderAStar<T> where T : IPathObject
  {
    private readonly TileGraph<T> _tileGraph;
    public PathFinderAStar(TileGraph<T> tileGraph)
    {
      _tileGraph = tileGraph;
    }

    public Queue<T> PathTo(T start, T goal)
    {
      var nodes = _tileGraph.Nodes;
      var startNode = nodes[start];
      var goalNode = nodes[goal];

      var closedSet = new List<Node<T>>();
      var openSet = new SimplePriorityQueue<Node<T>>();
      openSet.Enqueue(startNode, 0);

      var cameFrom = new Dictionary<Node<T>, Node<T>>();

      var gScore = nodes.Values.ToDictionary(nr => nr, nr => float.MaxValue);
      gScore[startNode] = 0;

      var fScore = nodes.Values.ToDictionary(nr => nr, nr => float.MaxValue);
      fScore[startNode] = HeuristicCostEstimate(startNode, goalNode);

      while (openSet.Count > 0)
      {
        var current = openSet.Dequeue();
        if (current == goalNode)
        {
          return ReconstructPath(cameFrom, current);
        }
        closedSet.Add(current);

        foreach (var edgeNeighbour in current.Edges)
        {
          var neighbour = edgeNeighbour.Node;
          if (closedSet.Contains(neighbour))
            continue;

          var tentativeGScore = gScore[current] + DistBetween(current, neighbour);

          if (openSet.Contains(neighbour) && tentativeGScore >= gScore[neighbour])
            continue;

          cameFrom[neighbour] = current;
          gScore[neighbour] = tentativeGScore;
          fScore[neighbour] = gScore[neighbour] + HeuristicCostEstimate(neighbour, goalNode);

          if (openSet.Contains(neighbour) == false)
            openSet.Enqueue(neighbour, fScore[neighbour]);
        }
      }
      // no match...
      return null;
    }

    public Queue<T> PathBetween(List<T> path)
    {
      // Can't make a path with less than 2 points wtf.
      if (path.Count < 2)
        return null;

      // Only 2 rooms, no point using path between, just work out the single path to and return that.
      if (path.Count == 2)
        return PathTo(path[0], path[1]);
      var i = 0;
      var finalPath = new Queue<T>();
      finalPath.Enqueue(path[i]);
      while (true)
      {
        var currentPath = PathTo(path[i], path[i + 1]);
        // Check if this section of the path worked, we don't want to return an incomplete path so just return null instead.
        if (currentPath == null)
          return null;

        finalPath = JoinQueues(finalPath, currentPath, path[i]);
        i++;
        if (i >= path.Count - 1)
          return finalPath;
      }
    }

    public Queue<T> JoinQueues(Queue<T> q1, Queue<T> q2, T joiningObject)
    {
      while (q1.Count > 0)
      {
        var temp = q1.Dequeue();

        if (!EqualityComparer<T>.Default.Equals(temp, joiningObject))
          q2.Enqueue(temp);
      }
      return q2;
    }

    protected float HeuristicCostEstimate(Node<T> a, Node<T> b)
    {
      return (float)Math.Sqrt(Math.Pow(a.Data.X - b.Data.X, 2) + Math.Pow(a.Data.Y - b.Data.Y, 2));
    }

    protected float DistBetween(Node<T> a, Node<T> b)
    {
      var aCenterX = a.Data.X + ((float)a.Data.Width);
      var aCenterY = a.Data.Y + ((float)a.Data.Height);

      var bCenterX = b.Data.X + ((float)b.Data.Width);
      var bCenterY = b.Data.Y + ((float)b.Data.Height);

      return (float)Math.Sqrt(Math.Pow(aCenterX - bCenterX, 2) + Math.Pow(aCenterY - bCenterY, 2));
    }

    protected Queue<T> ReconstructPath(Dictionary<Node<T>, Node<T>> cameFrom, Node<T> current)
    {
      var finalPath = new Queue<T>();
      finalPath.Enqueue(current.Data);
      while (cameFrom.ContainsKey(current))
      {
        current = cameFrom[current];
        finalPath.Enqueue(current.Data);
      }
      return finalPath;
    }
  }
}
