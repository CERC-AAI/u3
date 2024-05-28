using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UltimateProceduralPrimitivesFREE
{
  public class FormulaPlaneFlex : AbstractFormula
  {
    public MyMeshInfo CalculateVertexesAndUVs(PlaneFlexParameters parameter)
    {

      switch (parameter.Orientation)
      {
        case Orientation.Up:
          CreateVertexesAndUVs(parameter.Width, 0, parameter.Height, (int)parameter.Segments.x, (int)parameter.Segments.y, parameter.OffsetLeftForwardVtxForOrientationUp, parameter.OffsetRightForwardVtxForOrientationUp, parameter.OffsetLeftBackwardVtxForOrientationUp, parameter.OffsetRightBackwardVtxForOrientationUp, Orientation.Up, myGlobeVtx, myGlobeUVs);
          break;
        case Orientation.Down:
          CreateVertexesAndUVs(parameter.Width, 0, parameter.Height, (int)parameter.Segments.x, (int)parameter.Segments.y, parameter.OffsetRightForwardVtxForOrientationDown, parameter.OffsetLeftForwardVtxForOrientationDown, parameter.OffsetRightBackwardVtxForOrientationDown, parameter.OffsetLeftBackwardVtxForOrientationDown, Orientation.Down, myGlobeVtx, myGlobeUVs);
          break;
        case Orientation.Left:
          CreateVertexesAndUVs(0, parameter.Height, parameter.Width, (int)parameter.Segments.x, (int)parameter.Segments.y, parameter.OffsetUpForwardVtxForOrientationLeft, parameter.OffsetUpBackwardVtxForOrientationLeft, parameter.OffsetDownForwardVtxForOrientationLeft, parameter.OffsetDownBackwardVtxForOrientationLeft, Orientation.Left, myGlobeVtx, myGlobeUVs);
          break;
        case Orientation.Right:
          CreateVertexesAndUVs(0, parameter.Height, parameter.Width, (int)parameter.Segments.x, (int)parameter.Segments.y, parameter.OffsetUpBackwardVtxForOrientationRight, parameter.OffsetUpForwardVtxForOrientationRight, parameter.OffsetDownBackwardVtxForOrientationRight, parameter.OffsetDownForwardVtxForOrientationRight, Orientation.Right, myGlobeVtx, myGlobeUVs);
          break;
        case Orientation.Forward:
          CreateVertexesAndUVs(parameter.Width, parameter.Height, 0, (int)parameter.Segments.x, (int)parameter.Segments.y, parameter.OffsetUpRightVtxForOrientationForward, parameter.OffsetUpLeftVtxForOrientationForward, parameter.OffsetDownRightVtxForOrientationForward, parameter.OffsetDownLeftVtxForOrientationForward, Orientation.Forward, myGlobeVtx, myGlobeUVs);
          break;
        case Orientation.Backward:
          CreateVertexesAndUVs(parameter.Width, parameter.Height, 0, (int)parameter.Segments.x, (int)parameter.Segments.y, parameter.OffsetUpLeftVtxForOrientationBackward, parameter.OffsetUpRightVtxForOrientationBackward, parameter.OffsetDownLeftVtxForOrientationBackward, parameter.OffsetDownRightVtxForOrientationBackward, Orientation.Backward, myGlobeVtx, myGlobeUVs);
          break;
        default:
          break;
      }

      if (parameter.DoubleSided)
      {
        switch (parameter.Orientation)
        {
          case Orientation.Up:
            CreateVertexesAndUVs(parameter.Width, 0, parameter.Height, (int)parameter.Segments.x, (int)parameter.Segments.y, parameter.OffsetRightForwardVtxForOrientationUp, parameter.OffsetLeftForwardVtxForOrientationUp, parameter.OffsetRightBackwardVtxForOrientationUp, parameter.OffsetLeftBackwardVtxForOrientationUp, Orientation.Down, myGlobeVtx_ForDoubleSided, myGlobeUVs_ForDoubleSided);
            break;
          case Orientation.Down:
            CreateVertexesAndUVs(parameter.Width, 0, parameter.Height, (int)parameter.Segments.x, (int)parameter.Segments.y, parameter.OffsetLeftForwardVtxForOrientationDown, parameter.OffsetRightForwardVtxForOrientationDown, parameter.OffsetLeftBackwardVtxForOrientationDown, parameter.OffsetRightBackwardVtxForOrientationDown, Orientation.Up, myGlobeVtx_ForDoubleSided, myGlobeUVs_ForDoubleSided);
            break;
          case Orientation.Left:
            CreateVertexesAndUVs(0, parameter.Height, parameter.Width, (int)parameter.Segments.x, (int)parameter.Segments.y, parameter.OffsetUpBackwardVtxForOrientationLeft, parameter.OffsetUpForwardVtxForOrientationLeft, parameter.OffsetDownBackwardVtxForOrientationLeft, parameter.OffsetDownForwardVtxForOrientationLeft, Orientation.Right, myGlobeVtx_ForDoubleSided, myGlobeUVs_ForDoubleSided);
            break;
          case Orientation.Right:
            CreateVertexesAndUVs(0, parameter.Height, parameter.Width, (int)parameter.Segments.x, (int)parameter.Segments.y, parameter.OffsetUpForwardVtxForOrientationRight, parameter.OffsetUpBackwardVtxForOrientationRight, parameter.OffsetDownForwardVtxForOrientationRight, parameter.OffsetDownBackwardVtxForOrientationRight, Orientation.Left, myGlobeVtx_ForDoubleSided, myGlobeUVs_ForDoubleSided);
            break;
          case Orientation.Forward:
            CreateVertexesAndUVs(parameter.Width, parameter.Height, 0, (int)parameter.Segments.x, (int)parameter.Segments.y, parameter.OffsetUpLeftVtxForOrientationForward, parameter.OffsetUpRightVtxForOrientationForward, parameter.OffsetDownLeftVtxForOrientationForward, parameter.OffsetDownRightVtxForOrientationForward, Orientation.Backward, myGlobeVtx_ForDoubleSided, myGlobeUVs_ForDoubleSided);
            break;
          case Orientation.Backward:
            CreateVertexesAndUVs(parameter.Width, parameter.Height, 0, (int)parameter.Segments.x, (int)parameter.Segments.y, parameter.OffsetUpRightVtxForOrientationBackward, parameter.OffsetUpLeftVtxForOrientationBackward, parameter.OffsetDownRightVtxForOrientationBackward, parameter.OffsetDownLeftVtxForOrientationBackward, Orientation.Forward, myGlobeVtx_ForDoubleSided, myGlobeUVs_ForDoubleSided);
            break;
          default:
            break;
        }

      }

      CreateMyVtxUVsIdx_FlexPlane_Supershape_SuperEllipsoid_TearDrop(myVtx, myUVs, myGlobeVtx, myGlobeUVs, myIdx);

      FlipNormals(parameter.FlipNormals, myVtx, myUVs, myIdx);
      DirectionCollection(parameter.Direction, myVtx);
      SetPivotPosition(parameter.PivotPosition, myVtx);
      var myMeshInfo = CreateMyMeshInfoStruct(myVtx, myUVs, myIdx);

      if (parameter.DoubleSided)
      {
        myVtx.Clear();
        myUVs.Clear();
        myIdx.Clear();

        CreateMyVtxUVsIdx_FlexPlane_Supershape_SuperEllipsoid_TearDrop(myVtx, myUVs, myGlobeVtx_ForDoubleSided, myGlobeUVs_ForDoubleSided, myIdx);
        FlipNormals(parameter.FlipNormals, myVtx, myUVs, myIdx);
        DirectionCollection(parameter.Direction, myVtx);
        SetPivotPosition(parameter.PivotPosition, myVtx);
        var myMeshInfo_ForDoubleSided = CreateMyMeshInfoStruct(myVtx, myUVs, myIdx);

        var newMyVtx = new List<Vector3>();
        foreach (var item in myMeshInfo.myVtx)
          newMyVtx.Add(item);
        foreach (var item in myMeshInfo_ForDoubleSided.myVtx)
          newMyVtx.Add(item);

        var newMyUVs = new List<Vector2>();
        foreach (var item in myMeshInfo.myUVs)
          newMyUVs.Add(item);
        foreach (var item in myMeshInfo_ForDoubleSided.myUVs)
          newMyUVs.Add(item);

        var newMyIdx = new List<int>();
        foreach (var item in myMeshInfo.myIdx)
          newMyIdx.Add(item);
        foreach (var item in myMeshInfo_ForDoubleSided.myIdx)
          newMyIdx.Add(item + myMeshInfo.myVtx.Count);

        return CreateMyMeshInfoStruct(newMyVtx, newMyUVs, newMyIdx);
      }
      else
      {
        return myMeshInfo;
      }

    }

    // List is Reference Passing
    void CreateVertexesAndUVs(float width, float height, float depth, int segmentsX, int segmentsY, Vector3 offsetFL, Vector3 offsetFR, Vector3 offsetBL, Vector3 offsetBR, Orientation orientation, List<List<Vector3>> ref_myGlobeVtx, List<List<Vector2>> ref_myGlobeUVs)
    {
      // Think it as an Up-Orientation Plane !!
      var planeGlobeOffsets_FromFL = new List<List<Vector3>>(); // Forward Left
      var planeGlobeOffsets_FromFR = new List<List<Vector3>>(); // Forward Right
      var planeGlobeOffsets_FromBL = new List<List<Vector3>>(); // Backward Left
      var planeGlobeOffsets_FromBR = new List<List<Vector3>>(); // Backward Right

      //Calculate offsets for each vertexes
      for (int i = 0; i < segmentsY + 1; i++)
      {
        float iWeight_FromFL = 0;
        float iWeight_FromFR = 0;
        float iWeight_FromBL = 0;
        float iWeight_FromBR = 0;

        iWeight_FromFL = (float)i / segmentsY;
        iWeight_FromFR = (float)i / segmentsY;
        iWeight_FromBL = 1.0f - (float)i / segmentsY;
        iWeight_FromBR = 1.0f - (float)i / segmentsY;

        planeGlobeOffsets_FromFL.Add(new List<Vector3>());
        planeGlobeOffsets_FromFR.Add(new List<Vector3>());
        planeGlobeOffsets_FromBL.Add(new List<Vector3>());
        planeGlobeOffsets_FromBR.Add(new List<Vector3>());

        // Debug.Log($"START J LOOP   i:{i} ----------------------------");
        for (int j = 0; j < segmentsX + 1; j++)
        {
          // Debug.Log($"i:{i}  j: {j}");
          var jWeight_FL = 1.0f - (float)j / segmentsX;
          var jWeight_FR = (float)j / segmentsX;
          var jWeight_BL = 1.0f - (float)j / segmentsX;
          var jWeight_BR = (float)j / segmentsX;

          var weightX_FL = Mathf.Lerp(0, offsetFL.x, iWeight_FromFL * jWeight_FL);
          var weightY_FL = Mathf.Lerp(0, offsetFL.y, iWeight_FromFL * jWeight_FL);
          var weightZ_FL = Mathf.Lerp(0, offsetFL.z, iWeight_FromFL * jWeight_FL);
          var weightX_FR = Mathf.Lerp(0, offsetFR.x, iWeight_FromFR * jWeight_FR);
          var weightY_FR = Mathf.Lerp(0, offsetFR.y, iWeight_FromFR * jWeight_FR);
          var weightZ_FR = Mathf.Lerp(0, offsetFR.z, iWeight_FromFR * jWeight_FR);
          var weightX_BL = Mathf.Lerp(0, offsetBL.x, iWeight_FromBL * jWeight_BL);
          var weightY_BL = Mathf.Lerp(0, offsetBL.y, iWeight_FromBL * jWeight_BL);
          var weightZ_BL = Mathf.Lerp(0, offsetBL.z, iWeight_FromBL * jWeight_BL);
          var weightX_BR = Mathf.Lerp(0, offsetBR.x, iWeight_FromBR * jWeight_BR);
          var weightY_BR = Mathf.Lerp(0, offsetBR.y, iWeight_FromBR * jWeight_BR);
          var weightZ_BR = Mathf.Lerp(0, offsetBR.z, iWeight_FromBR * jWeight_BR);

          planeGlobeOffsets_FromFL[i].Add(new Vector3(weightX_FL, weightY_FL, weightZ_FL));
          planeGlobeOffsets_FromFR[i].Add(new Vector3(weightX_FR, weightY_FR, weightZ_FR));
          planeGlobeOffsets_FromBL[i].Add(new Vector3(weightX_BL, weightY_BL, weightZ_BL));
          planeGlobeOffsets_FromBR[i].Add(new Vector3(weightX_BR, weightY_BR, weightZ_BR));
        }
      }

      // create globeVertexes and globeUvs
      for (int i = 0; i < segmentsY + 1; i++)
      {
        var iVertexX = Mathf.Lerp(-width / 2.0f, width / 2.0f, (float)i / (float)segmentsY);
        var iVertexY = Mathf.Lerp(-height / 2.0f, height / 2.0f, (float)i / (float)segmentsY);
        var iVertexZ = Mathf.Lerp(-depth / 2.0f, depth / 2.0f, (float)i / (float)segmentsY);
        var uvY = Mathf.Lerp(0.0f, 1.0f, (float)i / (float)segmentsY);
        ref_myGlobeVtx.Add(new List<Vector3>());
        ref_myGlobeUVs.Add(new List<Vector2>());
        for (int j = 0; j < segmentsX + 1; j++)
        {
          var jVertexX = Mathf.Lerp(-width / 2.0f, width / 2.0f, (float)j / (float)segmentsX);
          var jVertexY = Mathf.Lerp(-height / 2.0f, height / 2.0f, (float)j / (float)segmentsX);
          var jVertexZ = Mathf.Lerp(-depth / 2.0f, depth / 2.0f, (float)j / (float)segmentsX);
          var uvX = Mathf.Lerp(0.0f, 1.0f, (float)j / (float)segmentsX);
          var offsetSummary = planeGlobeOffsets_FromFL[i][j] + planeGlobeOffsets_FromFR[i][j] + planeGlobeOffsets_FromBL[i][j] + planeGlobeOffsets_FromBR[i][j];
          switch (orientation)
          {
            case Orientation.Up:
              ref_myGlobeVtx[i].Add(new Vector3(jVertexX, height / 2.0f, iVertexZ) + offsetSummary);
              ref_myGlobeUVs[i].Add(new Vector2(uvX, uvY));
              break;
            case Orientation.Down:
              ref_myGlobeVtx[i].Add(new Vector3(-jVertexX, -height / 2.0f, iVertexZ) + offsetSummary);
              ref_myGlobeUVs[i].Add(new Vector2(uvX, uvY));
              break;
            case Orientation.Left:
              ref_myGlobeVtx[i].Add(new Vector3(-width / 2.0f, iVertexY, -jVertexZ) + offsetSummary);
              ref_myGlobeUVs[i].Add(new Vector2(uvX, uvY));
              break;
            case Orientation.Right:
              ref_myGlobeVtx[i].Add(new Vector3(width / 2.0f, iVertexY, jVertexZ) + offsetSummary);
              ref_myGlobeUVs[i].Add(new Vector2(uvX, uvY));
              break;
            case Orientation.Forward:
              ref_myGlobeVtx[i].Add(new Vector3(-jVertexX, iVertexY, depth / 2.0f) + offsetSummary);
              ref_myGlobeUVs[i].Add(new Vector2(uvX, uvY));
              break;
            case Orientation.Backward:
              ref_myGlobeVtx[i].Add(new Vector3(jVertexX, iVertexY, -depth / 2.0f) + offsetSummary);
              ref_myGlobeUVs[i].Add(new Vector2(uvX, uvY));
              break;

            default:
              break;
          }
        }
      }
    }
  }
}