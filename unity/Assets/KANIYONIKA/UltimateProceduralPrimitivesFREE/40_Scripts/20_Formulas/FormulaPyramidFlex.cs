using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UltimateProceduralPrimitivesFREE
{
  public class FormulaPyramidFlex : AbstractFormula
  {
    public MyMeshInfo CalculateVertexesAndUVs(PyramidFlexParameters parameter)
    {
      var offsetY_ForDown = new Vector3(0, -parameter.Height / 2.0f, 0);
      var downPlaneParameter = new PlaneFlexParameters()
      {
        SurfaceType = parameter.SurfaceType,
        Orientation = Orientation.Down,
        Direction = Direction.Y_Axis,  // Collect Direction Later
        PivotPosition = PivotPosition.Center, // Collect Pivot Later
        DoubleSided = false,
        Segments = parameter.SegmentsBottom,
        Width = parameter.Width,
        Height = parameter.Depth,

        OffsetLeftForwardVtxForOrientationDown = offsetY_ForDown + parameter.OffsetDownLeftForwardVtx,
        OffsetRightForwardVtxForOrientationDown = offsetY_ForDown + parameter.OffsetDownRightForwardVtx,
        OffsetLeftBackwardVtxForOrientationDown = offsetY_ForDown + parameter.OffsetDownLeftBackwardVtx,
        OffsetRightBackwardVtxForOrientationDown = offsetY_ForDown + parameter.OffsetDownRightBackwardVtx,
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
        Segments = parameter.SegmentsSides,
        Width = parameter.Depth,
        Height = parameter.Height,

        OffsetUpForwardVtxForOrientationLeft = offsetX_ForLeft + new Vector3(parameter.Width / 2.0f, 0, -parameter.Depth / 2.0f) + parameter.OffsetUpVtx,
        OffsetUpBackwardVtxForOrientationLeft = offsetX_ForLeft + new Vector3(parameter.Width / 2.0f, 0, parameter.Depth / 2.0f) + parameter.OffsetUpVtx,
        OffsetDownForwardVtxForOrientationLeft = offsetX_ForLeft + parameter.OffsetDownLeftForwardVtx,
        OffsetDownBackwardVtxForOrientationLeft = offsetX_ForLeft + parameter.OffsetDownLeftBackwardVtx,
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
        Segments = parameter.SegmentsSides,
        Width = parameter.Depth,
        Height = parameter.Height,

        OffsetUpForwardVtxForOrientationRight = offsetX_ForRight + new Vector3(-parameter.Width / 2.0f, 0, -parameter.Depth / 2.0f) + parameter.OffsetUpVtx,
        OffsetUpBackwardVtxForOrientationRight = offsetX_ForRight + new Vector3(-parameter.Width / 2.0f, 0, parameter.Depth / 2.0f) + parameter.OffsetUpVtx,
        OffsetDownForwardVtxForOrientationRight = offsetX_ForRight + parameter.OffsetDownRightForwardVtx,
        OffsetDownBackwardVtxForOrientationRight = offsetX_ForRight + parameter.OffsetDownRightBackwardVtx,
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
        Segments = parameter.SegmentsSides,
        Width = parameter.Width,
        Height = parameter.Height,

        OffsetUpLeftVtxForOrientationForward = offsetZ_ForForward + new Vector3(parameter.Width / 2.0f, 0, -parameter.Depth / 2.0f) + parameter.OffsetUpVtx,
        OffsetUpRightVtxForOrientationForward = offsetZ_ForForward + new Vector3(-parameter.Width / 2.0f, 0, -parameter.Depth / 2.0f) + parameter.OffsetUpVtx,
        OffsetDownLeftVtxForOrientationForward = offsetZ_ForForward + parameter.OffsetDownLeftForwardVtx,
        OffsetDownRightVtxForOrientationForward = offsetZ_ForForward + parameter.OffsetDownRightForwardVtx,
        FlipNormals = false, // Collect FlipNormals Later
      };

      var offsetZ_ForBackward = new Vector3(0, 0, -parameter.Depth / 2.0f);
      var backwardPlaneParameters = new PlaneFlexParameters()
      {
        SurfaceType = parameter.SurfaceType,
        Orientation = Orientation.Backward,
        Direction = Direction.Y_Axis,  // Collect Direction Later
        PivotPosition = PivotPosition.Center, // Collect Pivot Later
        DoubleSided = false,
        Segments = parameter.SegmentsSides,
        Width = parameter.Width,
        Height = parameter.Height,

        OffsetUpLeftVtxForOrientationBackward = offsetZ_ForBackward + new Vector3(parameter.Width / 2.0f, 0, parameter.Depth / 2.0f) + parameter.OffsetUpVtx,
        OffsetUpRightVtxForOrientationBackward = offsetZ_ForBackward + new Vector3(-parameter.Width / 2.0f, 0, parameter.Depth / 2.0f) + parameter.OffsetUpVtx,
        OffsetDownLeftVtxForOrientationBackward = offsetZ_ForBackward + parameter.OffsetDownLeftBackwardVtx,
        OffsetDownRightVtxForOrientationBackward = offsetZ_ForBackward + parameter.OffsetDownRightBackwardVtx,
        FlipNormals = false, // Collect FlipNormals Later
      };

      var myMeshInfo_downPlane = new FormulaPlaneFlex().CalculateVertexesAndUVs(downPlaneParameter);
      var myMeshInfo_leftPlane = new FormulaPlaneFlex().CalculateVertexesAndUVs(leftPlaneParameter);
      var myMeshInfo_rightPlane = new FormulaPlaneFlex().CalculateVertexesAndUVs(rightPlaneParameter);
      var myMeshInfo_forwardPlane = new FormulaPlaneFlex().CalculateVertexesAndUVs(forwardPlaneParameter);
      var myMeshInfo_backwardPlane = new FormulaPlaneFlex().CalculateVertexesAndUVs(backwardPlaneParameters);


      foreach (var item in myMeshInfo_downPlane.myVtx) { myVtx.Add(item); }
      foreach (var item in myMeshInfo_leftPlane.myVtx) { myVtx.Add(item); }
      foreach (var item in myMeshInfo_rightPlane.myVtx) { myVtx.Add(item); }
      foreach (var item in myMeshInfo_forwardPlane.myVtx) { myVtx.Add(item); }
      foreach (var item in myMeshInfo_backwardPlane.myVtx) { myVtx.Add(item); }

      foreach (var item in myMeshInfo_downPlane.myUVs) { myUVs.Add(item); }
      foreach (var item in myMeshInfo_leftPlane.myUVs) { myUVs.Add(item); }
      foreach (var item in myMeshInfo_rightPlane.myUVs) { myUVs.Add(item); }
      foreach (var item in myMeshInfo_forwardPlane.myUVs) { myUVs.Add(item); }
      foreach (var item in myMeshInfo_backwardPlane.myUVs) { myUVs.Add(item); }


      int d = myMeshInfo_downPlane.myVtx.Count;
      int l = myMeshInfo_leftPlane.myVtx.Count;
      int r = myMeshInfo_rightPlane.myVtx.Count;
      int f = myMeshInfo_forwardPlane.myVtx.Count;
      int b = myMeshInfo_backwardPlane.myVtx.Count;

      for (int i = 0; i < myMeshInfo_downPlane.myIdx.Count; i++)
        myIdx.Add(myMeshInfo_downPlane.myIdx[i] + 0);

      for (int i = 0; i < myMeshInfo_leftPlane.myIdx.Count; i++)
        myIdx.Add(myMeshInfo_leftPlane.myIdx[i] + d);

      for (int i = 0; i < myMeshInfo_rightPlane.myIdx.Count; i++)
        myIdx.Add(myMeshInfo_rightPlane.myIdx[i] + d + l);

      for (int i = 0; i < myMeshInfo_forwardPlane.myIdx.Count; i++)
        myIdx.Add(myMeshInfo_forwardPlane.myIdx[i] + d + l + r);

      for (int i = 0; i < myMeshInfo_backwardPlane.myIdx.Count; i++)
        myIdx.Add(myMeshInfo_backwardPlane.myIdx[i] + d + l + r + f);


      FlipNormals(parameter.FlipNormals, myVtx, myUVs, myIdx);      // Collect FlipNormals here
      DirectionCollection(parameter.Direction, myVtx);  // Collect Direction here
      SetPivotPosition(parameter.PivotPosition, myVtx);  // Collect Pivot here
      return CreateMyMeshInfoStruct(myVtx, myUVs, myIdx);
    }
  }
}