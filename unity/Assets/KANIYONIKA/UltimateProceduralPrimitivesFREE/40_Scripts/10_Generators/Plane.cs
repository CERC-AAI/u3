using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UltimateProceduralPrimitivesFREE
{
  [System.Serializable]
  public class Plane : AbstractGenerator
  {
    public SurfaceType surfaceType = SurfaceType.Flat;
    public Orientation orientation = Orientation.Up;
    public Direction direction = Direction.Y_Axis;
    public PivotPosition pivotPosition = PivotPosition.Center;
    public bool doubleSided = true;

    public Vector2 faceSegments = new Vector2(3, 3);

    public float width = 3.0f;
    public float height = 3.0f;

    public bool flipNormals = false;


    public Plane() { }


    public override void Generate(Mesh mesh)
    {
      var parameter = new PlaneBasicParameters()
      {
        SurfaceType = this.surfaceType,
        Orientation = this.orientation,
        Direction = this.direction,
        PivotPosition = this.pivotPosition,
        DoubleSided = this.doubleSided,

        Segments = this.faceSegments,

        Width = this.width,
        Height = this.height,

        FlipNormals = this.flipNormals,
      };

      var myMeshInfo = new FormulaPlane().CalculateVertexesAndUVs(parameter);
      Finishing(mesh, myMeshInfo, surfaceType);
    }
  }
}