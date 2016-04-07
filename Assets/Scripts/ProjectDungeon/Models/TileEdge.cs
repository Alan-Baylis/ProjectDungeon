namespace Models
{
  public class TileEdge
  {
    public TileEdge(Facing facing, TileEdgeType edgeType = TileEdgeType.WALL)
    {
      Facing = facing;
      EdgeType = edgeType;
    }

    public Facing Facing { get; set; }

    public TileEdgeType EdgeType { get; set; }
  }
}
