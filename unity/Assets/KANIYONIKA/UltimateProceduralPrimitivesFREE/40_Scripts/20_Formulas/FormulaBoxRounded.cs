/*
 * The following code was studied and constructed based on https://github.com/keijiro/Metamesh
 */

using System.Collections.Generic;
using UnityEngine;


namespace UltimateProceduralPrimitivesFREE
{
  public class FormulaBoxRounded : AbstractFormula
  {
    private BoxRoundedParameters parameter;


    public MyMeshInfo CalculateVertexesAndUVs(BoxRoundedParameters _parameter)
    {
      this.parameter = _parameter;

      Generate(null);

      FlipNormals(parameter.FlipNormals, myVtx, myUVs, myIdx);
      DirectionCollection(parameter.Direction, myVtx);
      SetPivotPosition(parameter.PivotPosition, myVtx);
      return CreateMyMeshInfoStruct(myVtx, myUVs, myIdx);
    }


    float GetEdgePoint(int i, float length)
      => i <= parameter.Segments ?
            parameter.Radius / length * (i) / parameter.Segments :
        1 + parameter.Radius / length * (i - parameter.Segments * 2 - 1) / parameter.Segments;

    (Vector3, Vector3) RoundPoint(Vector3 v)
    {
      var extent = new Vector3(parameter.Width, parameter.Height, parameter.Depth) / 2;
      var anchor = Utils.MultiplyVec3(Utils.Sign(v), (new Vector3(extent.x - parameter.Radius, extent.y - parameter.Radius, extent.z - parameter.Radius)));
      var normal = Vector3.Normalize(v - anchor);
      return (anchor + normal * parameter.Radius, normal);
    }

    List<(Vector3, Vector3, Vector2)> MakePlane(Vector4 ax, Vector4 ay, Vector3 offs)
    {
      var vc_edge = 2 + parameter.Segments * 2;
      var vtx = new List<(Vector3, Vector3, Vector2)>();
      for (var iy = 0; iy < vc_edge; iy++)
      {
        var v = GetEdgePoint(iy, ay.w);
        var y = new Vector3(ay.x, ay.y, ay.z) * (v - 0.5f) * ay.w;
        for (var ix = 0; ix < vc_edge; ix++)
        {
          var u = GetEdgePoint(ix, ax.w);
          var x = new Vector3(ax.x, ax.y, ax.z) * (u - 0.5f) * ax.w;
          var (p, n) = RoundPoint(x + y + offs);
          vtx.Add((p, n, new Vector2(u, v)));
        }
      }
      return vtx;
    }

    public void Generate(Mesh mesh)
    {
      var vc_edge = 2 + parameter.Segments * 2;

      var vtx = new List<(Vector3 p, Vector3 n, Vector2 uv)>();
      vtx.AddRange(MakePlane(new Vector4(1, 0, 0, parameter.Width), new Vector4(0, 1, 0, parameter.Height), new Vector3(0, 0, -0.5f * parameter.Depth)));
      vtx.AddRange(MakePlane(new Vector4(-1, 0, 0, parameter.Width), new Vector4(0, 1, 0, parameter.Height), new Vector3(0, 0, 0.5f * parameter.Depth)));
      vtx.AddRange(MakePlane(new Vector4(0, 0, 1, parameter.Depth), new Vector4(0, -1, 0, parameter.Height), new Vector3(-0.5f * parameter.Width, 0, 0)));
      vtx.AddRange(MakePlane(new Vector4(0, 0, -1, parameter.Depth), new Vector4(0, -1, 0, parameter.Height), new Vector3(0.5f * parameter.Width, 0, 0)));
      vtx.AddRange(MakePlane(new Vector4(1, 0, 0, parameter.Width), new Vector4(0, 0, -1, parameter.Depth), new Vector3(0, -0.5f * parameter.Height, 0)));
      vtx.AddRange(MakePlane(new Vector4(-1, 0, 0, parameter.Width), new Vector4(0, 0, -1, parameter.Depth), new Vector3(0, 0.5f * parameter.Height, 0)));

      var idx = new List<int>();
      var i = 0;
      for (var ip = 0; ip < 6; ip++)
      {
        for (var iy = 0; iy < vc_edge - 1; iy++, i++)
        {
          for (var ix = 0; ix < vc_edge - 1; ix++, i++)
          {
            // Lower triangle
            idx.Add(i);
            idx.Add(i + vc_edge);
            idx.Add(i + 1);
            // Upper triangle
            idx.Add(i + 1);
            idx.Add(i + vc_edge);
            idx.Add(i + vc_edge + 1);
          }
        }
        i += vc_edge;
      }

      foreach (var item in vtx) { myVtx.Add(item.p); }
      foreach (var item in vtx) { myUVs.Add(item.uv); }
      foreach (var item in idx) { myIdx.Add(item); }

      // Debug.Log($"vertexes.Count: {vertexes.Count}");
      // Debug.Log($"uvs.Count: {uvs.Count}");
      // Debug.Log($"idx.Count: {idx.Count}");
    }


  }
}