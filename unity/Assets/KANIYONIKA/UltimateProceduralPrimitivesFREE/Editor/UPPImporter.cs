using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

namespace UltimateProceduralPrimitivesFREE
{
  [ScriptedImporter(1, "upp")]
  public sealed class UPPImporter : ScriptedImporter
  {
    [SerializeField] Shape _shape = Shape.BoxRounded;
    [SerializeField] Plane _plane = new Plane();
    [SerializeField] NotAvailableInFreeVersion _planeFlex = new NotAvailableInFreeVersion();
    [SerializeField] NotAvailableInFreeVersion _planeSuperEllipse = new NotAvailableInFreeVersion();
    [SerializeField] Box _box = new Box();
    [SerializeField] NotAvailableInFreeVersion _boxFlex = new NotAvailableInFreeVersion();
    [SerializeField] BoxRounded _boxRounded = new BoxRounded();
    [SerializeField] NotAvailableInFreeVersion _boxSuperEllipsoid = new NotAvailableInFreeVersion();
    [SerializeField] Pyramid _pyramid = new Pyramid();
    [SerializeField] NotAvailableInFreeVersion _pyramidFlex = new NotAvailableInFreeVersion();
    [SerializeField] NotAvailableInFreeVersion _pyramidPerfectTriangularFlex = new NotAvailableInFreeVersion();
    [SerializeField] Sphere _sphere = new Sphere();
    [SerializeField] NotAvailableInFreeVersion _sphereIco = new NotAvailableInFreeVersion();
    [SerializeField] NotAvailableInFreeVersion _sphereFibonacci = new NotAvailableInFreeVersion();
    [SerializeField] NotAvailableInFreeVersion _tearDrop = new NotAvailableInFreeVersion();
    [SerializeField] NotAvailableInFreeVersion _cylinder = new NotAvailableInFreeVersion();
    [SerializeField] Cone _cone = new Cone();
    [SerializeField] Supershape _supershape = new Supershape();

    [SerializeField] NotAvailableInFreeVersion _otherShapes = new NotAvailableInFreeVersion();

    public override void OnImportAsset(AssetImportContext context)
    {
      var gameObject = new GameObject();
      var mesh = ImportAsMesh(context.assetPath);

      var meshFilter = gameObject.AddComponent<MeshFilter>();
      meshFilter.sharedMesh = mesh;

      var pipelineAsset = GraphicsSettings.currentRenderPipeline;
      var baseMaterial = pipelineAsset ? pipelineAsset.defaultMaterial : AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");

      var meshRenderer = gameObject.AddComponent<MeshRenderer>();
      meshRenderer.sharedMaterial = baseMaterial;

      context.AddObjectToAsset("prefab", gameObject);
      if (mesh != null) context.AddObjectToAsset("mesh", mesh);

      context.SetMainObject(gameObject);
    }

    Mesh ImportAsMesh(string path)
    {
      var mesh = new Mesh();
      mesh.name = $"UPP_{_shape.ToString()}";

      switch (_shape)
      {
        case Shape.Plane: _plane.Generate(mesh); break;
        // case Shape.PlaneFlex: _planeFlex.Generate(mesh); break;
        // case Shape.PlaneSuperEllipse: _planeSuperEllipse.Generate(mesh); break;
        case Shape.Box: _box.Generate(mesh); break;
        // case Shape.BoxFlex: _boxFlex.Generate(mesh); break;
        case Shape.BoxRounded: _boxRounded.Generate(mesh); break;
        // case Shape.BoxSuperEllipsoid: _boxSuperEllipsoid.Generate(mesh); break;
        case Shape.Pyramid: _pyramid.Generate(mesh); break;
        // case Shape.PyramidFlex: _pyramidFlex.Generate(mesh); break;
        // case Shape.PyramidPerfectTriangularFlex: _pyramidPerfectTriangularFlex.Generate(mesh); break;
        case Shape.Sphere: _sphere.Generate(mesh); break;
        // case Shape.SphereIco: _sphereIco.Generate(mesh); break;
        // case Shape.SphereFibonacci: _sphereFibonacci.Generate(mesh); break;
        // case Shape.TearDrop: _tearDrop.Generate(mesh); break;
        // case Shape.Cylinder: _cylinder.Generate(mesh); break;
        case Shape.Cone: _cone.Generate(mesh); break;
        case Shape.Supershape: _supershape.Generate(mesh); break;
      }

      mesh.RecalculateBounds();

      return mesh;
    }
  }

}