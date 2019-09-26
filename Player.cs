using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    [Header("Controller2D")]
    public Controller2D Controller;

    [Header("Jump settings")]
    public float MaxJumpHeight = 4f;
    public float MinJumpHeight = 1f;
    public float TimeToJumpApex = 0.4f;

    [Header("Move speed")]
    public float MoveSpeed = 6f;

    private Vector2 Velocity;

    [Header("Acceleration smoothing (Airborne & Ground)")]
    public float AccelerationTimeAirborne = 0.2f;
    public float AccelerationTimeGrounded = 0.1f;
    private float VelocityXSmoothing;

    [HideInInspector]
    [Header("Wall slide settings")]
    public float MaxWallSlideSpeed = 3;
    [HideInInspector]
    public float WallStickTime = 0.25f;
    float TimeToWallUnstick;

    [HideInInspector]
    [Header("Wall jump settings")]
    public Vector2 WallJumpClimb;
    [HideInInspector]
    public Vector2 WallJumpOff;
    [HideInInspector]
    public Vector2 WallLeap;

    private float MaxJumpVelocity;
    private float MinJumpVelocity;
    private float Gravity;


    private Vector2 DirectionalInput;
    private bool WallSliding;
    private int WallDirectionX;
    private Animator animator;

    private float MaxGravity = -20f;

    private void Start()
    {
        Controller = GetComponent<Controller2D>();
        animator = GetComponent<Animator>();

        Gravity = -((2 * MaxJumpHeight) / Mathf.Pow(TimeToJumpApex, 2));
        MaxJumpVelocity = Mathf.Abs(Gravity) * TimeToJumpApex;
        MinJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Gravity) * MinJumpHeight);
    }

    private void Update()
    {
        WallDirectionX = (Controller.Collisions.Left) ? -1 : 1;

        CalculateVelocity();
        HandleWallSliding(); 

        // Commented out and put Move function in FixedUpdate where it 'should' be
        // by having the Move function in Update instead of FixedUpdate
        // there was some strange vibrating on the player in the Y-axis 

        Controller.Move(Velocity * Time.deltaTime, DirectionalInput);

        if (Controller.Collisions.Above || Controller.Collisions.Below)
        {
            if (Controller.Collisions.SlidingDownMaxSlope)
            {
                Velocity.y += Controller.Collisions.SlopeNormal.y * -Gravity * Time.deltaTime;
            }
            else
            {
                Velocity.y = 0;
            }
        }
    }

   private void FixedUpdate()
    {
        Controller.Move(Velocity * Time.deltaTime, DirectionalInput);
        if (DirectionalInput.x > 0.1 || DirectionalInput.x < -0.1)
            animator.SetBool("IsRunning?", true);
        else
        {
            animator.SetBool("IsRunning?", false);
        }
    }

    public void SetDirectionalInput(Vector2 _DirectionalInput)
    {
        DirectionalInput = _DirectionalInput;
    }

    public void OnJumpInputDown()
    {
        animator.SetBool("IsJumpStart?", true);     //ALEX

        if (WallSliding)
        {
            if (WallDirectionX == DirectionalInput.x)
            {
                Velocity.x = -WallDirectionX * WallJumpClimb.x;
                Velocity.y = WallJumpClimb.y;
            }
            else if (DirectionalInput.x == 0)
            {
                Velocity.x = -WallDirectionX * WallJumpOff.x;
                Velocity.y = WallJumpOff.y;
            }
            else
            {
                Velocity.x = -WallDirectionX * WallLeap.x;
                Velocity.y = WallLeap.y;
            }
        }

        if (Controller.Collisions.Below)
        {
            //animator.SetBool("IsJumpStart?", false);
            // animator.SetBool("IsJumpFall?", true);

            if (Controller.Collisions.SlidingDownMaxSlope)
            {
                if (DirectionalInput.x != -Mathf.Sign(Controller.Collisions.SlopeNormal.x)) // Not jumping against max slope
                {
                    Velocity.y = MaxJumpVelocity * Controller.Collisions.SlopeNormal.y;
                    Velocity.x = MaxJumpVelocity * Controller.Collisions.SlopeNormal.x;
                }
            }
            else
            {
                Velocity.y = MaxJumpVelocity;

            }
        }
    }

    public void OnJumpInputUp()
    {
        animator.SetBool("IsJumpStart?", false);
        if (Velocity.y > MinJumpVelocity)
        {
            Velocity.y = MinJumpVelocity;
        }
    }

    private void HandleWallSliding()
    {
        WallSliding = false;

        if ((Controller.Collisions.Left || Controller.Collisions.Right) && !Controller.Collisions.Below && Velocity.y < 0)
        {
            WallSliding = true;

            if (Velocity.y < -MaxWallSlideSpeed)
            {
                Velocity.y = -MaxWallSlideSpeed;
            }

            if (TimeToWallUnstick > 0)
            {
                Velocity.x = 0;
                VelocityXSmoothing = 0;

                if (DirectionalInput.x != WallDirectionX && DirectionalInput.x != 0)
                {
                    TimeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    TimeToWallUnstick = WallStickTime;
                }
            }
            else
            {
                TimeToWallUnstick = WallStickTime;
            }
        }
    }

    private void CalculateVelocity()
    {
        float TargetVelocityX = DirectionalInput.x * MoveSpeed;
        Velocity.x = Mathf.SmoothDamp(Velocity.x, TargetVelocityX, ref VelocityXSmoothing, (Controller.Collisions.Below) ? AccelerationTimeGrounded : AccelerationTimeAirborne);

        // If we are falling increase velocity
        if (!Controller.Collisions.Below)
        {
            Velocity.y += Gravity * Time.deltaTime;
            if (Velocity.y < MaxGravity)
            {
                Velocity.y = MaxGravity;

            }
        }
    }
}
