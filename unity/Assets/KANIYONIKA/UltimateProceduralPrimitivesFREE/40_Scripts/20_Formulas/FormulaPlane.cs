using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UltimateProceduralPrimitivesFREE
{
  public class FormulaPlane : AbstractFormula
  {
    public MyMeshInfo CalculateVertexesAndUVs(PlaneBasicParameters parameter)
    {
      var planeParameter = new PlaneFlexParameters()
      {
        SurfaceType = parameter.SurfaceType,
        Orientation = parameter.Orientation,
        Direction = parameter.Direction,
        PivotPosition = parameter.PivotPosition,
        DoubleSided = parameter.DoubleSided,
        Segments = parameter.Segments,
        Width = parameter.Width,
        Height = parameter.Height,
        FlipNormals = parameter.FlipNormals,
      };

      var myMeshInfo = new FormulaPlaneFlex().CalculateVertexesAndUVs(planeParameter);

      return myMeshInfo;
    }
  }
}