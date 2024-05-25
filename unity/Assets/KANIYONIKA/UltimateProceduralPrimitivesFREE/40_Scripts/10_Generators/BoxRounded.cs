using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UltimateProceduralPrimitivesFREE
{
  [System.Serializable]
  public class BoxRounded : AbstractGenerator
  {
    public SurfaceType surfaceType = SurfaceType.Smooth;
    public Direction direction = Direction.Y_Axis;
    public PivotPosition pivotPosition = PivotPosition.Center;

    public float width = 3.0f;
    public float height = 3.0f;
    public float depth = 3.0f;
    public float radius = 0.2f;
    public int segments = 5;

    public bool flipNormals = false;


    public BoxRounded() { }

    public override void Generate(Mesh mesh)
    {
      var parameter = new BoxRoundedParameters()
      {
        SurfaceType = this.surfaceType,
        Direction = this.direction,
        PivotPosition = this.pivotPosition,

        Width = this.width,
        Height = this.height,
        Depth = this.depth,
        Radius = this.radius,
        Segments = this.segments,

        FlipNormals = this.flipNormals,
      };

      var myMeshInfo = new FormulaBoxRounded().CalculateVertexesAndUVs(parameter);
      Finishing(mesh, myMeshInfo, surfaceType);
    }
  }
}