using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : RayCastController
{
    public struct CollisionInfo
    {
        public bool Above, Below;
        public bool Left, Right;

        public bool ClimbingSlope;
        public bool DescendingSlope;
        public float SlopeAngle, SlopeAngleOld;

        public int FaceDirection;

        public Vector3 VelocityOld;

        public void Reset()
        {
            Above = Below = false;
            Left = Right = false;
            ClimbingSlope = false;
            DescendingSlope = false;
            

            SlopeAngleOld = SlopeAngle;
            SlopeAngle = 0;
        }
    }

    private float MaxClimbAngle = 50;
    private float MaxDescendAngle = 60;

    public CollisionInfo Collisions;

    public override void Start()
    {
        base.Start();
        Collisions.FaceDirection = 1;
    }

    public void Move(Vector3 Velocity, bool StandingOnPlatform = false)
    {
        UpdateRayCastOrigins();
        Collisions.Reset();
        Collisions.VelocityOld = Velocity;

        if (Velocity.x != 0)
        {
            Collisions.FaceDirection = (int)Mathf.Sign(Velocity.x);
        }

        if (Velocity.y < 0)
        {
            DescendSlope(ref Velocity);
        }

       
        HorizontalCollision(ref Velocity);
        

        if (Velocity.y != 0)
        {
            VerticalCollision(ref Velocity);
        }

        if (StandingOnPlatform)
        {
            Collisions.Below = true;
        }

        transform.Translate(Velocity);
    }

    private void HorizontalCollision(ref Vector3 Velocity)
    {
        float DirectionX = Mathf.Sign(Velocity.x);
        float RayLength = Mathf.Abs(Velocity.x) + SkinWidth;

        if (Mathf.Abs(Velocity.x) < SkinWidth)
        {
            RayLength = SkinWidth * 2;
        }

        for (int i = 0; i < HorizontalRayCount; i++)
        {
            Vector2 RayOrigin = (DirectionX == -1) ? RayCastOringsCorners.BottomLeft : RayCastOringsCorners.BottomRight;
            RayOrigin += Vector2.up * (HorizontalRaySpacing * i);
            RaycastHit2D RayCastHit = Physics2D.Raycast(RayOrigin, Vector2.right * DirectionX, RayLength, CollisionMask);

            Debug.DrawRay(RayOrigin, Vector2.right * DirectionX * RayLength, Color.red); // Testing (Drawing the rays)

            if (RayCastHit)
            {
                if (RayCastHit.distance == 0)
                {
                    continue;
                }

                float SlopeAngle = Vector2.Angle(RayCastHit.normal, Vector2.up);

                if (i == 0 && SlopeAngle <= MaxClimbAngle)
                {
                    if (Collisions.DescendingSlope)
                    {
                        Collisions.DescendingSlope = false;
                        Velocity = Collisions.VelocityOld;
                    }

                    float DistanceToSlopeStart = 0;

                    if (SlopeAngle != Collisions.SlopeAngleOld)
                    {
                        DistanceToSlopeStart = RayCastHit.distance - SkinWidth;
                        Velocity.x -= DistanceToSlopeStart * DirectionX;
                    }

                    ClimbSlope(ref Velocity, SlopeAngle);
                    Velocity.x += DistanceToSlopeStart * DirectionX;
                }

                if (!Collisions.ClimbingSlope || SlopeAngle > MaxClimbAngle)
                {
                    Velocity.x = (RayCastHit.distance - SkinWidth) * DirectionX;
                    RayLength = RayCastHit.distance;

                    if (Collisions.ClimbingSlope)
                    {
                        Velocity.y = Mathf.Tan(Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(Velocity.x);
                    }

                    Collisions.Left = DirectionX == -1;
                    Collisions.Right = DirectionX == 1;
                }
            }
        }
    }

    private void VerticalCollision(ref Vector3 Velocity)
    {
        float DirectionY = Mathf.Sign(Velocity.y);
        float RayLength = Mathf.Abs(Velocity.y) + SkinWidth;

        for (int i = 0; i < VerticalRayCount; i++)
        {
            Vector2 RayOrigin = (DirectionY == -1) ? RayCastOringsCorners.BottomLeft : RayCastOringsCorners.TopLeft;
            RayOrigin += Vector2.right * (VerticalRaySpacing * i + Velocity.x);
            RaycastHit2D RayCastHit = Physics2D.Raycast(RayOrigin, Vector2.up * DirectionY, RayLength, CollisionMask);

            Debug.DrawRay(RayOrigin, Vector2.up * DirectionY * RayLength, Color.red); // Testing (Drawing the rays)

            if (RayCastHit)
            {
                Velocity.y = (RayCastHit.distance - SkinWidth) * DirectionY;
                RayLength = RayCastHit.distance;

                if (Collisions.ClimbingSlope)
                {
                    Velocity.x = Velocity.y / Mathf.Tan(Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Sign(Velocity.x); 
                }

                Collisions.Below = DirectionY == -1;
                Collisions.Above = DirectionY == 1;
            }
        }

        if (Collisions.ClimbingSlope)
        {
            float DirectionX = Mathf.Sign(Velocity.x);

            RayLength = Mathf.Abs(Velocity.x) + SkinWidth;
            Vector2 RayOrigin = ((DirectionX == -1) ? RayCastOringsCorners.BottomLeft : RayCastOringsCorners.BottomRight) + Vector2.up * Velocity.y;
            RaycastHit2D RayCastHit = Physics2D.Raycast(RayOrigin, Vector2.right * DirectionX, RayLength, CollisionMask);

            if (RayCastHit)
            {
                float SlopeAngle = Vector2.Angle(RayCastHit.normal, Vector2.up);

                if (SlopeAngle != Collisions.SlopeAngle)
                {
                    Velocity.x = (RayCastHit.distance - SkinWidth) * DirectionX;
                    Collisions.SlopeAngle = SlopeAngle;
                }
            }
        }
    }

    private void DescendSlope(ref Vector3 Velocity)
    {
        float DirectionX = Mathf.Sign(Velocity.x);

        Vector2 RayOrigin = (DirectionX == 1) ? RayCastOringsCorners.BottomRight : RayCastOringsCorners.BottomLeft;
        RaycastHit2D RayCastHit = Physics2D.Raycast(RayOrigin, -Vector2.up, Mathf.Infinity, CollisionMask);

        if (RayCastHit)
        {
            float SlopeAngle = Vector2.Angle(RayCastHit.normal, Vector2.up);

            if (SlopeAngle != 0 && SlopeAngle <= MaxDescendAngle)
            {
                if (Mathf.Sign(RayCastHit.normal.x) == DirectionX)
                {
                    if (RayCastHit.distance - SkinWidth <= Mathf.Tan(SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(Velocity.x))
                    {
                        float MoveDistance = Mathf.Abs(Velocity.x);
                        float DescendVelocityY = Mathf.Sin(SlopeAngle * Mathf.Deg2Rad) * MoveDistance;

                        Velocity.x = Mathf.Cos(SlopeAngle * Mathf.Deg2Rad) * MoveDistance;
                        Velocity.y -= DescendVelocityY;

                        Collisions.SlopeAngle = SlopeAngle;
                        Collisions.DescendingSlope = true;
                        Collisions.Below = true;

                    }
                }
            }
        }
    }

    private void ClimbSlope(ref Vector3 Velocity, float SlopeAngle)
    {
        float MoveDistance = Mathf.Abs(Velocity.x);
        float ClimbVelocityY = Mathf.Sin(SlopeAngle * Mathf.Deg2Rad) * MoveDistance;

        if (Velocity.y <= ClimbVelocityY)
        {
            Velocity.y = ClimbVelocityY;
            Velocity.x = Mathf.Cos(SlopeAngle * Mathf.Deg2Rad) * MoveDistance * Mathf.Sign(Velocity.x);
            Collisions.Below = true;
            Collisions.ClimbingSlope = true;
            Collisions.SlopeAngle = SlopeAngle;
        }
    }

}
