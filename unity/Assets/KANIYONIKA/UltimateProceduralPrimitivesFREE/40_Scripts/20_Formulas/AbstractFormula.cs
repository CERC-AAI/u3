using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UltimateProceduralPrimitivesFREE
{
  public class AbstractFormula
  {
    public List<Vector3> myVtx = new List<Vector3>();
    public List<Vector2> myUVs = new List<Vector2>();
    public List<int> myIdx = new List<int>();

    public List<List<Vector3>> myGlobeVtx = new List<List<Vector3>>();
    public List<List<Vector2>> myGlobeUVs = new List<List<Vector2>>();
    public List<List<Vector3>> myGlobeVtx_ForDoubleSided = new List<List<Vector3>>();
    public List<List<Vector2>> myGlobeUVs_ForDoubleSided = new List<List<Vector2>>();

    public List<Vector3> myGlobeVtx_For2D = new List<Vector3>();
    public List<Vector2> myGlobeUVs_For2D = new List<Vector2>();
    public List<Vector3> myGlobeVtx_For2D_ForDoubleSided = new List<Vector3>();
    public List<Vector2> myGlobeUVs_For2D_ForDoubleSided = new List<Vector2>();

    // Set Vertexes as a Triangle, and apply UVs to Vertexes
    public void CreateMyVtxUVsIdx_FlexPlane_Supershape_SuperEllipsoid_TearDrop(List<Vector3> ref_myVtx, List<Vector2> ref_myUVs, List<List<Vector3>> ref_myGlobeVtx, List<List<Vector2>> ref_myGlobeUVs, List<int> ref_myIdx)
    {
      // Create MyVtx
      foreach (var list in ref_myGlobeVtx)
      {
        foreach (var vec3 in list)
        {
          ref_myVtx.Add(vec3);
        }
      }

      // Create MyUVs
      foreach (var list in ref_myGlobeUVs)
      {
        foreach (var vec3 in list)
        {
          ref_myUVs.Add(vec3);
        }
      }

      for (int i = 0; i < ref_myGlobeVtx.Count - 1; i++)
      {
        for (int j = 0; j < ref_myGlobeVtx[0].Count - 1; j++)
        {
          var groveCount = ref_myGlobeVtx[0].Count;

          ref_myIdx.Add((i * groveCount + 1 * groveCount) + (j));
          ref_myIdx.Add((i * groveCount + 1 * groveCount) + (j + 1));
          ref_myIdx.Add((i * groveCount) + (j));

          ref_myIdx.Add((i * groveCount + 1 * groveCount) + (j + 1));
          ref_myIdx.Add((i * groveCount) + (j + 1));
          ref_myIdx.Add((i * groveCount) + (j));

        }
      }
    }

    // When flipNormals, do reverse.
    // List is Reference Passing
    public void FlipNormals(bool flipNormals, List<Vector3> ref_myVtx, List<Vector2> ref_myUvs, List<int> ref_myIdx)
    {
      if (flipNormals)
      {
        ref_myIdx.Reverse();
      }
    }

    // DirectionCollection
    // List is Reference Passing
    public void DirectionCollection(Direction direction, List<Vector3> ref_myVtx)
    {
      var newVertexes = new List<Vector3>();

      // memo: no need to change uvs
      switch (direction)
      {
        case Direction.Y_Axis:
          foreach (Vector3 vec3 in ref_myVtx) { newVertexes.Add(new Vector3(vec3.x, vec3.y, vec3.z)); } // keep default
          break;

        case Direction.X_Axis:
          foreach (Vector3 vec3 in ref_myVtx) { newVertexes.Add(new Vector3(vec3.y, vec3.x * -1, vec3.z)); }
          break;

        case Direction.Z_Axis:
          foreach (Vector3 vec3 in ref_myVtx) { newVertexes.Add(new Vector3(vec3.x, vec3.z * -1, vec3.y)); }
          break;

        default:
          break;
      }
      ref_myVtx.Clear();
      foreach (Vector3 newVec3 in newVertexes) { ref_myVtx.Add(new Vector3(newVec3.x, newVec3.y, newVec3.z)); }
    }

    // SetPivot
    // List is Reference Passing
    public void SetPivotPosition(PivotPosition pivotPosition, List<Vector3> ref_myVtx)
    {
      if (pivotPosition == PivotPosition.Center) { return; }

      float offsetY = ref_myVtx[0].y;
      switch (pivotPosition)
      {
        case PivotPosition.Top:
          foreach (var item in ref_myVtx) { if (item.y > offsetY) { offsetY = item.y; } }
          break;

        case PivotPosition.Bottom:
          foreach (var item in ref_myVtx) { if (item.y < offsetY) { offsetY = item.y; } }
          break;

        default:
          break;
      }

      var newVertexes = new List<Vector3>();
      for (int i = 0; i < ref_myVtx.Count; i++)
      {
        newVertexes.Add(new Vector3(ref_myVtx[i].x, ref_myVtx[i].y - offsetY, ref_myVtx[i].z));
      }

      ref_myVtx.Clear();
      foreach (var item in newVertexes)
      {
        ref_myVtx.Add(item);
      }

    }

    // Create Struct and return
    // List is Reference Passing
    // Recommend to look CreateFlatSurfaceMyMeshInfoStruct in the AbstractGenerator
    public MyMeshInfo CreateMyMeshInfoStruct(List<Vector3> ref_myVtx, List<Vector2> ref_myUVs, List<int> ref_myIdx)
    {
      var _myVtx = new List<Vector3>();
      var _myUVs = new List<Vector2>();
      var _myIdx = new List<int>();

      foreach (var item in ref_myVtx)
        _myVtx.Add(item);
      foreach (var item in ref_myUVs)
        _myUVs.Add(item);
      foreach (var item in ref_myIdx)
        _myIdx.Add(item);

      var myMeshInfo = new MyMeshInfo()
      {
        myVtx = _myVtx,
        myUVs = _myUVs,
        myIdx = _myIdx,
      };
      return myMeshInfo;
    }
  }
}