using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player2D : MonoBehaviour
{
    public Controller2D Controller;
    public float JumpHeight = 4f;
    public float TimeToJumpApex = 0.4f;
    private float MoveSpeed = 6f;

    private Vector3 Velocity;
    private float VelocityXSmoothing;
    private float AccelerationTimeAirborne = 0.2f;
    private float AccelerationTimeGrounded = 0.1f;

    public float MaxWallSlideSpeed = 3;
    public float WallStickTime = 0.25f;
    float TimeToWallUnstick;

    public Vector2 WallJumpClimb;
    public Vector2 WallJumpOff;
    public Vector2 WallLeap;

    private float Gravity;
    private float JumpVelocity;

   void Start()
    {
        Controller = GetComponent<Controller2D>();

        Gravity = -((2 * JumpHeight) / Mathf.Pow(TimeToJumpApex, 2));
        JumpVelocity = Mathf.Abs(Gravity) * TimeToJumpApex;
    }

    void Update()
    {
        Vector2 HorizontalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        int WallDirectionX = (Controller.Collisions.Left) ? -1 : 1;

        float TargetVelocityX = HorizontalInput.x * MoveSpeed;
        Velocity.x = Mathf.SmoothDamp(Velocity.x, TargetVelocityX, ref VelocityXSmoothing, (Controller.Collisions.Below) ? AccelerationTimeGrounded : AccelerationTimeAirborne);

        bool WallSliding = false;

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

                if (HorizontalInput.x != WallDirectionX && HorizontalInput.x != 0)
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

        if (Controller.Collisions.Above || Controller.Collisions.Below)
        {
            Velocity.y = 0;
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (WallSliding)
            {
                if (WallDirectionX == HorizontalInput.x)
                {
                    Velocity.x = -WallDirectionX * WallJumpClimb.x;
                    Velocity.y = WallJumpClimb.y;
                }
                else if (HorizontalInput.x == 0)
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
                Velocity.y = JumpVelocity;
            }
        }
        Velocity.y += Gravity * Time.deltaTime;
        Controller.Move(Velocity * Time.deltaTime);
    }

}
