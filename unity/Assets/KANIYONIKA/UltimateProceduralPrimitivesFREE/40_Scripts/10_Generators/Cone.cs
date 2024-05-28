using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UltimateProceduralPrimitivesFREE
{
  [System.Serializable]
  public class Cone : AbstractGenerator
  {
    public SurfaceType surfaceType = SurfaceType.Smooth;
    public Direction direction = Direction.Y_Axis;
    public PivotPosition pivotPosition = PivotPosition.Center;

    public float topRadius = 0.0f;
    public float bottomRadius = 1.0f;
    public float Height = 3.0f;
    public int columns = 24; // 3
    public int rows = 12; // 1
    public bool caps = true;

    public bool flipNormals = false;


    public Cone() { }

    public override void Generate(Mesh mesh)
    {
      var parameter = new ConeParameters()
      {
        SurfaceType = this.surfaceType,
        Direction = this.direction,
        PivotPosition = this.pivotPosition,

        TopRadius = this.topRadius,
        BottomRadius = this.bottomRadius,
        Height = this.Height,
        Columns = this.columns,
        Rows = this.rows,
        Caps = this.caps,

        FlipNormals = this.flipNormals,
      };

      var myMeshInfo = new FormulaCone().CalculateVertexesAndUVs(parameter);
      Finishing(mesh, myMeshInfo, surfaceType);
    }
  }
}