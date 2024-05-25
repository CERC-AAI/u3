using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UltimateProceduralPrimitivesFREE
{
  public class FormulaBoxFlex : AbstractFormula
  {
    public MyMeshInfo CalculateVertexesAndUVs(BoxFlexParameters parameter)
    {
      var offsetY_ForUp = new Vector3(0, parameter.Height / 2.0f, 0);
      var upPlaneParameter = new PlaneFlexParameters()
      {
        SurfaceType = parameter.SurfaceType,
        Orientation = Orientation.Up,
        Direction = Direction.Y_Axis,  // Collect Direction Later
        PivotPosition = PivotPosition.Center, // Collect Pivot Later
        DoubleSided = false,
        Segments = parameter.SegmentsUp,
        Width = parameter.Width,
        Height = parameter.Depth,

        OffsetLeftForwardVtxForOrientationUp = parameter.OffsetUpLeftForwardVtx + offsetY_ForUp,
        OffsetRightForwardVtxForOrientationUp = parameter.OffsetUpRightForwardVtx + offsetY_ForUp,
        OffsetLeftBackwardVtxForOrientationUp = parameter.OffsetUpLeftBackwardVtx + offsetY_ForUp,
        OffsetRightBackwardVtxForOrientationUp = parameter.OffsetUpRightBackwardVtx + offsetY_ForUp,
        FlipNormals = false, // Collect FlipNormals Later
      };

      var offsetY_ForDown = new Vector3(0, -parameter.Height / 2.0f, 0);
      var downPlaneParameter = new PlaneFlexParameters()
      {
        SurfaceType = parameter.SurfaceType,
        Orientation = Orientation.Down,
        Direction = Direction.Y_Axis,  // Collect Direction Later
        PivotPosition = PivotPosition.Center, // Collect Pivot Later
        DoubleSided = false,
        Segments = parameter.SegmentsDown,
        Width = parameter.Width,
        Height = parameter.Depth,

        OffsetLeftForwardVtxForOrientationDown = parameter.OffsetDownLeftForwardVtx + offsetY_ForDown,
        OffsetRightForwardVtxForOrientationDown = parameter.OffsetDownRightForwardVtx + offsetY_ForDown,
        OffsetLeftBackwardVtxForOrientationDown = parameter.OffsetDownLeftBackwardVtx + offsetY_ForDown,
        OffsetRightBackwardVtxForOrientationDown = parameter.OffsetDownRightBackwardVtx + offsetY_ForDown,
        FlipNormals = false, // Collect FlipNormals Later
      };

      var offsetX_ForLeft = new Vector3(-parameter.Width / 2.0f, 0, 0);
      var leftPlaneParameter = new PlaneFlexParameters()
      {
        SurfaceType = parameter.SurfaceType,
        Orientation = Orientation.Left,
        Direction = Direction.Y_Axis,  // Collect Direction Later
        PivotPosition = PivotPosition.Center, // Collect Pivot Later
        DoubleSided = false,
        Segments = parameter.SegmentsLeft,
        Width = parameter.Depth,
        Height = parameter.Height,

        OffsetUpForwardVtxForOrientationLeft = parameter.OffsetUpLeftForwardVtx + offsetX_ForLeft,
        OffsetUpBackwardVtxForOrientationLeft = parameter.OffsetUpLeftBackwardVtx + offsetX_ForLeft,
        OffsetDownForwardVtxForOrientationLeft = parameter.OffsetDownLeftForwardVtx + offsetX_ForLeft,
        OffsetDownBackwardVtxForOrientationLeft = parameter.OffsetDownLeftBackwardVtx + offsetX_ForLeft,
        FlipNormals = false, // Collect FlipNormals Later
      };

      var offsetX_ForRight = new Vector3(parameter.Width / 2.0f, 0, 0);
      var rightPlaneParameter = new PlaneFlexParameters()
      {
        SurfaceType = parameter.SurfaceType,
        Orientation = Orientation.Right,
        Direction = Direction.Y_Axis,  // Collect Direction Later
        PivotPosition = PivotPosition.Center, // Collect Pivot Later
        DoubleSided = false,
        Segments = parameter.SegmentsRight,
        Width = parameter.Depth,
        Height = parameter.Height,

        OffsetUpForwardVtxForOrientationRight = parameter.OffsetUpRightForwardVtx + offsetX_ForRight,
        OffsetUpBackwardVtxForOrientationRight = parameter.OffsetUpRightBackwardVtx + offsetX_ForRight,
        OffsetDownForwardVtxForOrientationRight = parameter.OffsetDownRightForwardVtx + offsetX_ForRight,
        OffsetDownBackwardVtxForOrientationRight = parameter.OffsetDownRightBackwardVtx + offsetX_ForRight,
        FlipNormals = false, // Collect FlipNormals Later
      };

      var offsetZ_ForForward = new Vector3(0, 0, parameter.Depth / 2.0f);
      var forwardPlaneParameter = new PlaneFlexParameters()
      {
        SurfaceType = parameter.SurfaceType,
        Orientation = Orientation.Forward,
        Direction = Direction.Y_Axis,  // Collect Direction Later
        PivotPosition = PivotPosition.Center, // Collect Pivot Later
        DoubleSided = false,
        Segments = parameter.SegmentsForward,
        Width = parameter.Width,
        Height = parameter.Height,

        OffsetUpLeftVtxForOrientationForward = parameter.OffsetUpLeftForwardVtx + offsetZ_ForForward,
        OffsetUpRightVtxForOrientationForward = parameter.OffsetUpRightForwardVtx + offsetZ_ForForward,
        OffsetDownLeftVtxForOrientationForward = parameter.OffsetDownLeftForwardVtx + offsetZ_ForForward,
        OffsetDownRightVtxForOrientationForward = parameter.OffsetDownRightForwardVtx + offsetZ_ForForward,
        FlipNormals = false, // Collect FlipNormals Later
      };

      var offsetZ_ForBackward = new Vector3(0, 0, -parameter.Depth / 2.0f);
      var backwardPlaneParameter = new PlaneFlexParameters()
      {
        SurfaceType = parameter.SurfaceType,
        Orientation = Orientation.Backward,
        Direction = Direction.Y_Axis,  // Collect Direction Later
        PivotPosition = PivotPosition.Center, // Collect Pivot Later
        DoubleSided = false,
        Segments = parameter.SegmentsBackward,
        Width = parameter.Width,
        Height = parameter.Height,

        OffsetUpLeftVtxForOrientationBackward = parameter.OffsetUpLeftBackwardVtx + offsetZ_ForBackward,
        OffsetUpRightVtxForOrientationBackward = parameter.OffsetUpRightBackwardVtx + offsetZ_ForBackward,
        OffsetDownLeftVtxForOrientationBackward = parameter.OffsetDownLeftBackwardVtx + offsetZ_ForBackward,
        OffsetDownRightVtxForOrientationBackward = parameter.OffsetDownRightBackwardVtx + offsetZ_ForBackward,
        FlipNormals = false, // Collect FlipNormals Later
      };

      var myMeshInfo_upPlane = new FormulaPlaneFlex().CalculateVertexesAndUVs(upPlaneParameter);
      var myMeshInfo_downPlane = new FormulaPlaneFlex().CalculateVertexesAndUVs(downPlaneParameter);
      var myMeshInfo_leftPlane = new FormulaPlaneFlex().CalculateVertexesAndUVs(leftPlaneParameter);
      var myMeshInfo_rightPlane = new FormulaPlaneFlex().CalculateVertexesAndUVs(rightPlaneParameter);
      var myMeshInfo_forwardPlane = new FormulaPlaneFlex().CalculateVertexesAndUVs(forwardPlaneParameter);
      var myMeshInfo_backwardPlane = new FormulaPlaneFlex().CalculateVertexesAndUVs(backwardPlaneParameter);


      foreach (var item in myMeshInfo_upPlane.myVtx) { myVtx.Add(item); }
      foreach (var item in myMeshInfo_downPlane.myVtx) { myVtx.Add(item); }
      foreach (var item in myMeshInfo_leftPlane.myVtx) { myVtx.Add(item); }
      foreach (var item in myMeshInfo_rightPlane.myVtx) { myVtx.Add(item); }
      foreach (var item in myMeshInfo_forwardPlane.myVtx) { myVtx.Add(item); }
      foreach (var item in myMeshInfo_backwardPlane.myVtx) { myVtx.Add(item); }

      foreach (var item in myMeshInfo_upPlane.myUVs) { myUVs.Add(item); }
      foreach (var item in myMeshInfo_downPlane.myUVs) { myUVs.Add(item); }
      foreach (var item in myMeshInfo_leftPlane.myUVs) { myUVs.Add(item); }
      foreach (var item in myMeshInfo_rightPlane.myUVs) { myUVs.Add(item); }
      foreach (var item in myMeshInfo_forwardPlane.myUVs) { myUVs.Add(item); }
      foreach (var item in myMeshInfo_backwardPlane.myUVs) { myUVs.Add(item); }

      int u = myMeshInfo_upPlane.myVtx.Count;
      int d = myMeshInfo_downPlane.myVtx.Count;
      int l = myMeshInfo_leftPlane.myVtx.Count;
      int r = myMeshInfo_rightPlane.myVtx.Count;
      int f = myMeshInfo_forwardPlane.myVtx.Count;
      int b = myMeshInfo_backwardPlane.myVtx.Count;

      for (int i = 0; i < myMeshInfo_upPlane.myIdx.Count; i++)
        myIdx.Add(myMeshInfo_upPlane.myIdx[i] + 0);

      for (int i = 0; i < myMeshInfo_downPlane.myIdx.Count; i++)
        myIdx.Add(myMeshInfo_downPlane.myIdx[i] + u);

      for (int i = 0; i < myMeshInfo_leftPlane.myIdx.Count; i++)
        myIdx.Add(myMeshInfo_leftPlane.myIdx[i] + u + d);

      for (int i = 0; i < myMeshInfo_rightPlane.myIdx.Count; i++)
        myIdx.Add(myMeshInfo_rightPlane.myIdx[i] + u + d + l);

      for (int i = 0; i < myMeshInfo_forwardPlane.myIdx.Count; i++)
        myIdx.Add(myMeshInfo_forwardPlane.myIdx[i] + u + d + l + r);

      for (int i = 0; i < myMeshInfo_backwardPlane.myIdx.Count; i++)
        myIdx.Add(myMeshInfo_backwardPlane.myIdx[i] + u + d + l + r + f);


      FlipNormals(parameter.FlipNormals, myVtx, myUVs, myIdx);  // Collect FlipNormals here
      DirectionCollection(parameter.Direction, myVtx);  // Collect Direction here
      SetPivotPosition(parameter.PivotPosition, myVtx);  // Collect Pivot here
      return CreateMyMeshInfoStruct(myVtx, myUVs, myIdx);
    }
  }
}