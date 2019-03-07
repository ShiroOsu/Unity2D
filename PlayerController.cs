using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float RunSpeed = 10f;
    [SerializeField] private Transform FeetPosistion;
    [SerializeField] private float CheckRadius;
    [SerializeField] private LayerMask Ground;


    private Rigidbody2D RigidBody;
    private float HorizontalDirection;

    private bool IsOnGround;
    private float JumpForce = 10f;
    private float JumpTimer;
    private float JumpTime = 0.35f;
    private bool IsJumping;

    private Vector3 LookingRight = new Vector3(0, 0, 0);
    private Vector3 LookingLeft = new Vector3(0, 180, 0);


    // Start is called before the first frame update
    private void Start()
    {
        RigidBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        PlayerJump();
    }


    private void FixedUpdate()
    {
        PlayerMove();
    }

    private void PlayerMove()
    {
        HorizontalDirection = Input.GetAxisRaw("Horizontal");
        RigidBody.velocity = new Vector2(HorizontalDirection * RunSpeed, RigidBody.velocity.y);

        if (HorizontalDirection > 0)
        {
            transform.eulerAngles = LookingRight;
        }
        else if (HorizontalDirection < 0)
        {
            transform.eulerAngles = LookingLeft;
        }
    }

    private void PlayerJump()
    {
        IsOnGround = Physics2D.OverlapCircle(FeetPosistion.position, CheckRadius, Ground);

        if (Input.GetKeyDown(KeyCode.Space) && IsOnGround)
        {
            IsJumping = true;
            JumpTimer = JumpTime;
            RigidBody.velocity = Vector2.up * JumpForce;
        }

        if (Input.GetKey(KeyCode.Space) && IsJumping)
        {
            if (JumpTimer > 0)
            {
                RigidBody.velocity = Vector2.up * JumpForce;
                JumpTimer -= Time.deltaTime;
            }
            else
            {
                IsJumping = false;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            IsJumping = false;
        }
    }
}
