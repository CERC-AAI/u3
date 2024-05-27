using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UltimateProceduralPrimitivesFREE
{
  [System.Serializable]
  public class Sphere : AbstractGenerator
  {
    public SurfaceType surfaceType = SurfaceType.Smooth;
    public Direction direction = Direction.Y_Axis;
    public PivotPosition pivotPosition = PivotPosition.Center;

    public float radius = 1.5f;
    public int columns = 24;
    public int rows = 12;

    public bool flipNormals = false;


    public Sphere() { }

    public override void Generate(Mesh mesh)
    {
      var parameter = new SphereParameters()
      {
        SurfaceType = this.surfaceType,
        Direction = this.direction,
        PivotPosition = this.pivotPosition,

        Radius = this.radius,
        Columns = this.columns,
        Rows = this.rows,

        FlipNormals = this.flipNormals,
      };

      var myMeshInfo = new FormulaSphere().CalculateVertexesAndUVs(parameter);
      Finishing(mesh, myMeshInfo, surfaceType);
    }


  }
}