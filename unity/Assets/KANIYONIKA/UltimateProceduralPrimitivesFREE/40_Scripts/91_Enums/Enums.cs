

using UnityEngine;

namespace UltimateProceduralPrimitivesFREE
{
  public enum Shape
  {
    [InspectorName("Plane")] Plane = 10,
    // [InspectorName("Plane - Flex")] PlaneFlex = 11,
    // [InspectorName("Plane - Super Ellipse")] PlaneSuperEllipse = 12,
    [InspectorName("Box")] Box = 20,
    // [InspectorName("Box - Flex")] BoxFlex = 21,
    [InspectorName("Box - Rounded")] BoxRounded = 22,
    // [InspectorName("Box - Super Ellipsoid")] BoxSuperEllipsoid = 23,
    [InspectorName("Pyramid")] Pyramid = 30,
    // [InspectorName("Pyramid - Flex")] PyramidFlex = 31,
    // [InspectorName("Pyramid - Perfect Triangular Flex")] PyramidPerfectTriangularFlex = 32,
    [InspectorName("Sphere")] Sphere = 40,
    // [InspectorName("Sphere - Icosphere")] SphereIco = 41,
    // [InspectorName("Sphere - Fibonacci Sphere")] SphereFibonacci = 42,
    // [InspectorName("Tear Drop")] TearDrop = 50,
    // [InspectorName("Cylinder")] Cylinder = 60,
    [InspectorName("Cone")] Cone = 70,
    [InspectorName("Supershape")] Supershape = 99,

    [InspectorName("OtherShapes")] OtherShapes = 999,
  }

  public enum SurfaceType
  {
    Smooth,
    Flat,
  }

  public enum PivotPosition
  {
    Top,
    Center,
    Bottom,
  }

  public enum Direction
  {
    Y_Axis,
    X_Axis,
    Z_Axis,
  }
  public enum Orientation
  {
    Up,
    Down,
    Left,
    Right,
    Forward,
    Backward,
  }
  public enum UVPattern
  {
    BothSidesButOtherSideFlip,
    WrappingButOtherSideJaggies,
    OneSideButOtherSideJaggies,
  }
}