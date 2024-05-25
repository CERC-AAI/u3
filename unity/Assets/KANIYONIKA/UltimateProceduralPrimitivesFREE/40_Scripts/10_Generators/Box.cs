using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UltimateProceduralPrimitivesFREE
{
  [System.Serializable]
  public class Box : AbstractGenerator
  {
    public SurfaceType surfaceType = SurfaceType.Flat;
    public Direction direction = Direction.Y_Axis;
    public PivotPosition pivotPosition = PivotPosition.Center;

    public float width = 3.0f;
    public float height = 3.0f;
    public float depth = 3.0f;

    public Vector2 segments = new Vector2(1, 1);

    public bool flipNormals = false;


    public Box() { }


    public override void Generate(Mesh mesh)
    {
      var parameter = new BoxBasicParameters()
      {
        SurfaceType = this.surfaceType,
        Direction = this.direction,
        PivotPosition = this.pivotPosition,

        Width = this.width,
        Height = this.height,
        Depth = this.depth,

        Segments = this.segments,

        FlipNormals = this.flipNormals,
      };

      var myMeshInfo = new FormulaBox().CalculateVertexesAndUVs(parameter);
      Finishing(mesh, myMeshInfo, surfaceType);
    }
  }
}