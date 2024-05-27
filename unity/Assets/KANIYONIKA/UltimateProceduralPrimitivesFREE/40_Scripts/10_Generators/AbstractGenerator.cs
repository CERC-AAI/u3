using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateProceduralPrimitivesFREE
{
  public class AbstractGenerator
  {
    public virtual void Generate(Mesh mesh) { }

    protected void Finishing(Mesh mesh, MyMeshInfo myMeshInfo, SurfaceType surfaceType)
    {
      mesh.Clear();

      if (surfaceType == SurfaceType.Smooth)
      {
        if (myMeshInfo.myVtx.Count > 65535) mesh.indexFormat = IndexFormat.UInt32;
        mesh.SetVertices(myMeshInfo.myVtx);
        mesh.SetUVs(0, myMeshInfo.myUVs);
        mesh.SetIndices(myMeshInfo.myIdx, MeshTopology.Triangles, 0);
        // mesh.RecalculateNormals();
        NormalSolver.RecalculateNormals(mesh, 60);
      }
      if (surfaceType == SurfaceType.Flat)
      {
        var flatSurfaceMyMeshInfo = CreateFlatSurfaceMyMeshInfoStruct(myMeshInfo);
        if (flatSurfaceMyMeshInfo.myVtx.Count > 65535) mesh.indexFormat = IndexFormat.UInt32;
        mesh.SetVertices(flatSurfaceMyMeshInfo.myVtx);
        mesh.SetUVs(0, flatSurfaceMyMeshInfo.myUVs);
        mesh.SetIndices(flatSurfaceMyMeshInfo.myIdx, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
      }
      Debug.Log($"UPP RESULTs : mesh.vertices.Length = {mesh.vertices.Length},  mesh.GetIndexCount(0) = {mesh.GetIndexCount(0)}");
    }


    public MyMeshInfo CreateFlatSurfaceMyMeshInfoStruct(MyMeshInfo myMeshInfo)
    {
      var _myVtx = new List<Vector3>();
      var _myUVs = new List<Vector2>();
      var _myIdx = new List<int>();

      var i = 0;
      foreach (var idx in myMeshInfo.myIdx)
      {
        _myVtx.Add(myMeshInfo.myVtx[idx]);
        _myUVs.Add(myMeshInfo.myUVs[idx]);
        _myIdx.Add(i);
        i++;
      }

      var flatSurfaceMyMeshInfo = new MyMeshInfo()
      {
        myVtx = _myVtx,
        myUVs = _myUVs,
        myIdx = _myIdx,
      };
      return flatSurfaceMyMeshInfo;
    }
  }
}
