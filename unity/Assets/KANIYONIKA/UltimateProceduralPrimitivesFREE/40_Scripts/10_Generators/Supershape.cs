using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UltimateProceduralPrimitivesFREE
{
  [System.Serializable]
  public class Supershape : AbstractGenerator
  {
    public SurfaceType surfaceType = SurfaceType.Smooth;
    public Direction direction = Direction.Y_Axis;
    public PivotPosition pivotPosition = PivotPosition.Center;

    public int segments = 100;
    public float radius = 1.0f;

    [Space(10)]
    public float lonShape_N1 = 0.249778f;
    public float lonShape_N2 = 47.8498f;
    public float lonShape_N3 = -0.8625f;
    public float lonShape_M = 6.0f;
    public float lonShape_A = 1.0f;
    public float lonShape_B = 1.0f;

    [Space(10)]
    public float latShape_N1 = -76.8867f;
    public float latShape_N2 = 0.521395f;
    public float latShape_N3 = -56.75f;
    public float latShape_M = 7.0f;
    public float latShape_A = 1.0f;
    public float latShape_B = 1.0f;

    [Space(10)]
    public bool flipNormals = false;


    public Supershape() { }

    public override void Generate(Mesh mesh)
    {
      var parameter = new SupershapeParameters
      {
        SurfaceType = this.surfaceType,
        Direction = this.direction,
        PivotPosition = this.pivotPosition,

        Radius = this.radius,
        Segments = this.segments,

        LonShape_N1 = this.lonShape_N1,
        LonShape_N2 = this.lonShape_N2,
        LonShape_N3 = this.lonShape_N3,
        LonShape_M = this.lonShape_M,
        LonShape_A = this.lonShape_A,
        LonShape_B = this.lonShape_B,

        LatShape_N1 = this.latShape_N1,
        LatShape_N2 = this.latShape_N2,
        LatShape_N3 = this.latShape_N3,
        LatShape_M = this.latShape_M,
        LatShape_A = this.latShape_A,
        LatShape_B = this.latShape_B,

        FlipNormals = this.flipNormals,
      };

      var myMeshInfo = new FormulaSupershape().CalculateVertexesAndUVs(parameter);
      Finishing(mesh, myMeshInfo, surfaceType);
    }
  }
}