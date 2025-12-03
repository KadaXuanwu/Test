using UnityEngine;

/// <summary>
/// Ground detection and slope information.
/// </summary>
public struct GroundInfo {
    public bool OnGround;
    public bool OnSlope;
    public float SlopeAngle;
    public Vector3 SlopeNormal;
    public Vector3 SlideDirection;
    public Collider GroundCollider;
    public Vector3 GroundVelocity;

    public static GroundInfo None => new GroundInfo {
        SlopeNormal = Vector3.up,
        SlideDirection = Vector3.zero
    };
}
