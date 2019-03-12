using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RayCastController : MonoBehaviour
{
    public struct RayCastOrigins
    {
        public Vector2 TopLeft, TopRight;
        public Vector2 BottomLeft, BottomRight;
    }

    public const float SkinWidth = 0.015f;
    public int HorizontalRayCount = 4;
    public int VerticalRayCount = 4;

    public LayerMask CollisionMask;

    [HideInInspector]
    public float HorizontalRaySpacing;
    [HideInInspector]
    public float VerticalRaySpacing;

    [HideInInspector]
    public BoxCollider2D BoxCollider;
    public RayCastOrigins RayCastOringsCorners;

    public virtual void Start()
    {
        BoxCollider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    public void UpdateRayCastOrigins()
    {
        Bounds BoxBounds = BoxCollider.bounds;
        BoxBounds.Expand(SkinWidth * -2);

        RayCastOringsCorners.BottomLeft = new Vector2(BoxBounds.min.x, BoxBounds.min.y);
        RayCastOringsCorners.BottomRight = new Vector2(BoxBounds.max.x, BoxBounds.min.y);
        RayCastOringsCorners.TopLeft = new Vector2(BoxBounds.min.x, BoxBounds.max.y);
        RayCastOringsCorners.TopRight = new Vector2(BoxBounds.max.x, BoxBounds.max.y);
    }

    public void CalculateRaySpacing()
    {
        Bounds BoxBounds = BoxCollider.bounds;
        BoxBounds.Expand(SkinWidth * -2);

        HorizontalRayCount = Mathf.Clamp(HorizontalRayCount, 2, int.MaxValue);
        VerticalRayCount = Mathf.Clamp(VerticalRayCount, 2, int.MaxValue);

        HorizontalRaySpacing = BoxBounds.size.y / (HorizontalRayCount - 1);
        VerticalRaySpacing = BoxBounds.size.x / (VerticalRayCount - 1);

    }
}
