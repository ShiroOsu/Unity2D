using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RayCastController
{
    struct PassengerMovement
    {
        public Transform transform; // Feels bad man
        public Vector3 Velocity;
        public bool StandingOnPlatform;
        public bool MoveBeforePlatform;

        public PassengerMovement(Transform _transform, Vector3 _Velocity, bool _StandingOnPlatform, bool _MoveBeforePlatform)
        {
            transform = _transform;
            Velocity = _Velocity;
            StandingOnPlatform = _StandingOnPlatform;
            MoveBeforePlatform = _MoveBeforePlatform;
        }
    }

    public Vector3[] LocalWaypoints;
    Vector3[] GlobalWaypoints;

    int FromWaypointIndex;
    float PercentBetweenWaypoints;
    float NextMoveTime;

    public float Speed;
    public bool Cyclic;
    public float WaitTime;
    [Range(0, 2)]
    public float EaseAmount;

    public LayerMask PassengerMask;

    List<PassengerMovement> PassengersMovementList;
    Dictionary<Transform, Controller2D> PassengerDictionary = new Dictionary<Transform, Controller2D>();

    public override void Start()
    {
        base.Start();

        GlobalWaypoints = new Vector3[LocalWaypoints.Length];

        for (int i = 0; i < LocalWaypoints.Length; i++)
        {
            GlobalWaypoints[i] = LocalWaypoints[i] + transform.position;
        }
    }

    void Update()
    {
        UpdateRayCastOrigins();

        Vector3 Velocity = CalculatePlatformMovement();

        CalculatePassengerMovement(Velocity);

        MovePassengers(true);
        transform.Translate(Velocity);
        MovePassengers(false);

    }

    float Ease(float x)
    {
        float a = EaseAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    Vector3 CalculatePlatformMovement()
    {

        if (Time.time < NextMoveTime)
        {
            return Vector3.zero;
        }

        FromWaypointIndex %= GlobalWaypoints.Length;

        int ToWayPointIndex = (FromWaypointIndex + 1) % GlobalWaypoints.Length;
        float DistanceBetweenWayPoints = Vector3.Distance(GlobalWaypoints[FromWaypointIndex], GlobalWaypoints[ToWayPointIndex]);

        PercentBetweenWaypoints += Time.deltaTime * Speed / DistanceBetweenWayPoints;
        PercentBetweenWaypoints = Mathf.Clamp01(PercentBetweenWaypoints);

        float EasedPercentBetweenWaypoints = Ease(PercentBetweenWaypoints);

        Vector3 NewPos = Vector3.Lerp(GlobalWaypoints[FromWaypointIndex], GlobalWaypoints[ToWayPointIndex], EasedPercentBetweenWaypoints);

        if (PercentBetweenWaypoints >= 1)
        {
            PercentBetweenWaypoints = 0;
            FromWaypointIndex++;

            if (!Cyclic)
            {
                if (FromWaypointIndex >= GlobalWaypoints.Length - 1)
                {
                    FromWaypointIndex = 0;
                    System.Array.Reverse(GlobalWaypoints);
                }

            }
            NextMoveTime = Time.time + WaitTime;
        }
        return NewPos - transform.position;
    }

    void MovePassengers(bool BeforeMovePlatform)
    {
        foreach (PassengerMovement Passenger in PassengersMovementList)
        {
            if (!PassengerDictionary.ContainsKey(Passenger.transform))
            {
                PassengerDictionary.Add(Passenger.transform, Passenger.transform.GetComponent<Controller2D>());
            }

            if (Passenger.MoveBeforePlatform == BeforeMovePlatform)
            {
                PassengerDictionary[Passenger.transform].Move(Passenger.Velocity, Passenger.StandingOnPlatform);
            }
        }
    }

    void CalculatePassengerMovement(Vector3 Velocity)
    {
        HashSet<Transform> MovedPassengers = new HashSet<Transform>();
        PassengersMovementList = new List<PassengerMovement>();

        float DirectionX = Mathf.Sign(Velocity.x);
        float DirectionY = Mathf.Sign(Velocity.y);

        // Vertically moving Platform
        if (Velocity.y != 0)
        {
            float RayLength = Mathf.Abs(Velocity.y) + SkinWidth;

            for (int i = 0; i < VerticalRayCount; i++)
            {
                Vector2 RayOrigin = (DirectionY == -1) ? RayCastOringsCorners.BottomLeft : RayCastOringsCorners.TopLeft;
                RayOrigin += Vector2.right * (VerticalRaySpacing * i);
                RaycastHit2D RayCastHit = Physics2D.Raycast(RayOrigin, Vector2.up * DirectionY, RayLength, PassengerMask);

                if (RayCastHit)
                {
                    if (!MovedPassengers.Contains(RayCastHit.transform))
                    {
                        MovedPassengers.Add(RayCastHit.transform);

                        float PushX = (DirectionY == 1) ? Velocity.x : 0;
                        float PushY = Velocity.y - (RayCastHit.distance - SkinWidth) * DirectionY;

                        PassengersMovementList.Add(new PassengerMovement(RayCastHit.transform, new Vector3(PushX, PushY), DirectionY == 1, true));
                    }
                }
            }
        }

        // Horizontally moving platform
        if (Velocity.x != 0)
        {
            float RayLength = Mathf.Abs(Velocity.x) * SkinWidth;

            for (int i = 0; i < HorizontalRayCount; i++)
            {
                Vector2 RayOrigin = (DirectionX == -1) ? RayCastOringsCorners.BottomLeft : RayCastOringsCorners.BottomRight;
                RayOrigin += Vector2.up * (HorizontalRaySpacing * i);
                RaycastHit2D RayCastHit = Physics2D.Raycast(RayOrigin, Vector2.right * DirectionX, RayLength, PassengerMask);

                if (RayCastHit)
                {
                    if (!MovedPassengers.Contains(RayCastHit.transform))
                    {
                        MovedPassengers.Add(RayCastHit.transform);
                        float PushX = Velocity.x - (RayCastHit.distance - SkinWidth) * DirectionX;
                        float PushY = -SkinWidth;

                        PassengersMovementList.Add(new PassengerMovement(RayCastHit.transform, new Vector3(PushX, PushY), false, true));
                    }
                }
            }
        }

        // Passenger on top of a horizontally or downward moving platform
        if (DirectionY == -1 || Velocity.y == 0 && Velocity.x != 0)
        {
            float RayLength = SkinWidth * 2;

            for (int i = 0; i < VerticalRayCount; i++)
            {
                Vector2 RayOrigin = RayCastOringsCorners.TopLeft + Vector2.right * (VerticalRaySpacing * i);
                RaycastHit2D RayCastHit = Physics2D.Raycast(RayOrigin, Vector2.up, RayLength, PassengerMask);

                if (RayCastHit)
                {
                    if (!MovedPassengers.Contains(RayCastHit.transform))
                    {
                        MovedPassengers.Add(RayCastHit.transform);

                        float PushX = Velocity.x;
                        float PushY = Velocity.y;

                        PassengersMovementList.Add(new PassengerMovement(RayCastHit.transform, new Vector3(PushX, PushY), true, false));
                    }
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (LocalWaypoints != null)
        {
            Gizmos.color = Color.red;
            float Size = 0.3f;

            for (int i = 0; i < LocalWaypoints.Length; i++)
            {
                Vector3 GlobalWayPointsPos = (Application.isPlaying) ? GlobalWaypoints[i] : LocalWaypoints[i] + transform.position;

                // Drawing Waypoints
                Gizmos.DrawLine(GlobalWayPointsPos - Vector3.up * Size, GlobalWayPointsPos + Vector3.up * Size);
                Gizmos.DrawLine(GlobalWayPointsPos - Vector3.left * Size, GlobalWayPointsPos + Vector3.left * Size);
            }
        }
    }
}
