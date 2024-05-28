using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UltimateProceduralPrimitivesFREE
{
  [System.Serializable]
  public class Pyramid : AbstractGenerator
  {
    public SurfaceType surfaceType = SurfaceType.Flat;
    public Direction direction = Direction.Y_Axis;
    public PivotPosition pivotPosition = PivotPosition.Center;

    public float width = 3.0f;
    public float height = 2.5f;
    public float depth = 3.0f;

    public Vector2 segmentsSides = new Vector2(10.0f, 10.0f);
    public Vector2 segmentsBottom = new Vector2(5.0f, 5.0f);

    public bool flipNormals = false;


    public Pyramid() { }


    public override void Generate(Mesh mesh)
    {
      var parameter = new PyramidBasicParameters()
      {
        SurfaceType = this.surfaceType,
        Direction = this.direction,
        PivotPosition = this.pivotPosition,

        Width = this.width,
        Height = this.height,
        Depth = this.depth,

        SegmentsSides = this.segmentsSides,
        SegmentsBottom = this.segmentsSides,

        FlipNormals = this.flipNormals,
      };

      var myMeshInfo = new FormulaPyramid().CalculateVertexesAndUVs(parameter);
      Finishing(mesh, myMeshInfo, surfaceType);
    }
  }
}