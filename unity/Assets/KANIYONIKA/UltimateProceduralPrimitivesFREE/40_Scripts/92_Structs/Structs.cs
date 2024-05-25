using System.Collections.Generic;
using UnityEngine;

namespace UltimateProceduralPrimitivesFREE
{
  public struct MyMeshInfo
  {
    public List<Vector3> myVtx;
    public List<Vector2> myUVs;
    public List<int> myIdx;
  }

  public struct SupershapeParameters
  {
    public SurfaceType SurfaceType; public Direction Direction; public PivotPosition PivotPosition;
    public int Segments;
    public float Radius;
    public float LonShape_N1; public float LonShape_N2; public float LonShape_N3; public float LonShape_M; public float LonShape_A; public float LonShape_B;
    public float LatShape_N1; public float LatShape_N2; public float LatShape_N3; public float LatShape_M; public float LatShape_A; public float LatShape_B;
    public bool FlipNormals;
  }

  public struct BoxSuperEllipsoidParameters
  {
    public SurfaceType SurfaceType; public Direction Direction; public PivotPosition PivotPosition;
    public float Width; public float Height; public float Depth; public float N1; public float N2;
    public int Segments;
    public bool FlipNormals;
  }

  public struct CylinderParameters
  {
    public SurfaceType SurfaceType; public Direction Direction; public PivotPosition PivotPosition;
    public float TopRadius; public float BottomRadius; public float Height; public int Columns; public int Rows; public bool Caps;
    public bool FlipNormals;
  }

  public struct SphereParameters
  {
    public SurfaceType SurfaceType; public Direction Direction; public PivotPosition PivotPosition;
    public float Radius;
    public int Columns;
    public int Rows;
    public bool FlipNormals;
  }

  public struct SphereIcoParameters
  {
    public SurfaceType SurfaceType; public Direction Direction; public PivotPosition PivotPosition;
    public float Radius;
    public int Subdivision;
    public UVPattern UVPattern;
    public bool FlipNormals;
  }

  public struct SphereFibonacciParameters
  {
    public SurfaceType SurfaceType; public Direction Direction; public PivotPosition PivotPosition;
    public float Radius;
    public int Vertices;
    public UVPattern UVPattern;
    public bool FlipNormals;
  }

  public struct TearDropParameters
  {
    public SurfaceType SurfaceType; public Direction Direction; public PivotPosition PivotPosition;
    public float Width; public float Height; public float Depth;
    public int Segments;
    public bool FlipNormals;
  }
  public struct ConeParameters
  {
    public SurfaceType SurfaceType; public Direction Direction; public PivotPosition PivotPosition;
    public float TopRadius; public float BottomRadius; public float Height; public int Columns; public int Rows; public bool Caps;
    public bool FlipNormals;
  }

  public struct PlaneBasicParameters
  {
    public SurfaceType SurfaceType;
    public Orientation Orientation;
    public Direction Direction;
    public PivotPosition PivotPosition;
    public bool DoubleSided;

    public Vector2 Segments;

    public float Width;
    public float Height;

    public bool FlipNormals;
  }

  public struct PlaneFlexParameters
  {
    public SurfaceType SurfaceType;
    public Orientation Orientation;
    public Direction Direction;
    public PivotPosition PivotPosition;
    public bool DoubleSided;

    public Vector2 Segments;

    public float Width;
    public float Height;

    public Vector3 OffsetLeftForwardVtxForOrientationUp;
    public Vector3 OffsetRightForwardVtxForOrientationUp;
    public Vector3 OffsetLeftBackwardVtxForOrientationUp;
    public Vector3 OffsetRightBackwardVtxForOrientationUp;

    public Vector3 OffsetLeftForwardVtxForOrientationDown;
    public Vector3 OffsetRightForwardVtxForOrientationDown;
    public Vector3 OffsetLeftBackwardVtxForOrientationDown;
    public Vector3 OffsetRightBackwardVtxForOrientationDown;

    public Vector3 OffsetUpForwardVtxForOrientationLeft;
    public Vector3 OffsetUpBackwardVtxForOrientationLeft;
    public Vector3 OffsetDownForwardVtxForOrientationLeft;
    public Vector3 OffsetDownBackwardVtxForOrientationLeft;

    public Vector3 OffsetUpForwardVtxForOrientationRight;
    public Vector3 OffsetUpBackwardVtxForOrientationRight;
    public Vector3 OffsetDownForwardVtxForOrientationRight;
    public Vector3 OffsetDownBackwardVtxForOrientationRight;

    public Vector3 OffsetUpLeftVtxForOrientationForward;
    public Vector3 OffsetUpRightVtxForOrientationForward;
    public Vector3 OffsetDownLeftVtxForOrientationForward;
    public Vector3 OffsetDownRightVtxForOrientationForward;

    public Vector3 OffsetUpLeftVtxForOrientationBackward;
    public Vector3 OffsetUpRightVtxForOrientationBackward;
    public Vector3 OffsetDownLeftVtxForOrientationBackward;
    public Vector3 OffsetDownRightVtxForOrientationBackward;

    public bool FlipNormals;
  }

  public struct PlaneSuperEllipseParameters
  {
    public SurfaceType SurfaceType;
    public Orientation Orientation;
    public Direction Direction;
    public PivotPosition PivotPosition;
    public bool DoubleSided;

    public float Width; public float Height;
    public float N1; public float N2; public float N3; public float N4;
    public int Segments;
    public bool FlipNormals;
  }

  public struct BoxBasicParameters
  {
    public SurfaceType SurfaceType; public Direction Direction; public PivotPosition PivotPosition;
    public float Width; public float Height; public float Depth;
    public Vector2 Segments;
    public bool FlipNormals;
  }

  public struct BoxFlexParameters
  {
    public SurfaceType SurfaceType; public Direction Direction; public PivotPosition PivotPosition;

    public float Width;
    public float Height;
    public float Depth;

    public Vector2 SegmentsUp;
    public Vector2 SegmentsDown;
    public Vector2 SegmentsLeft;
    public Vector2 SegmentsRight;
    public Vector2 SegmentsForward;
    public Vector2 SegmentsBackward;

    public Vector3 OffsetUpLeftForwardVtx;
    public Vector3 OffsetUpRightForwardVtx;
    public Vector3 OffsetUpLeftBackwardVtx;
    public Vector3 OffsetUpRightBackwardVtx;

    public Vector3 OffsetDownLeftForwardVtx;
    public Vector3 OffsetDownRightForwardVtx;
    public Vector3 OffsetDownLeftBackwardVtx;
    public Vector3 OffsetDownRightBackwardVtx;

    public bool FlipNormals;
  }

  public struct BoxRoundedParameters
  {
    public SurfaceType SurfaceType; public Direction Direction; public PivotPosition PivotPosition;
    public float Width; public float Height; public float Depth; public float Radius; public int Segments;
    public bool FlipNormals;
  }

  public struct PyramidBasicParameters
  {
    public SurfaceType SurfaceType; public Direction Direction; public PivotPosition PivotPosition;
    public float Width; public float Height; public float Depth;
    public Vector2 SegmentsSides;
    public Vector2 SegmentsBottom;
    public bool FlipNormals;
  }

  public struct PyramidFlexParameters
  {
    public SurfaceType SurfaceType; public Direction Direction; public PivotPosition PivotPosition;
    public float Width; public float Height; public float Depth;
    public Vector2 SegmentsSides;
    public Vector2 SegmentsBottom;

    public Vector3 OffsetUpVtx;
    public Vector3 OffsetDownLeftForwardVtx;
    public Vector3 OffsetDownRightForwardVtx;
    public Vector3 OffsetDownLeftBackwardVtx;
    public Vector3 OffsetDownRightBackwardVtx;

    public bool FlipNormals;
  }

  public struct PyramidPerfectTriangularFlexParameters
  {
    public SurfaceType SurfaceType; public Direction Direction; public PivotPosition PivotPosition;
    public float Length;

    public Vector2 SegmentsSides;
    public Vector2 SegmentsBottom;

    public Vector3 OffsetUpVtx;
    public Vector3 OffsetDownForwardVtx;
    public Vector3 OffsetDownLeftBackwardVtx;
    public Vector3 OffsetDownRightBackwardVtx;

    public bool FlipNormals;
  }
}
