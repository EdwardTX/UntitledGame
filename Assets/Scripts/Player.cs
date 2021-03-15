using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public float gravityScaleFalling = 1.0f;
    public float movementSpeed = 20.0f;
    public float jumpForce = 1.0f;
    public int maxJumpCount = 2;
    public float wallSlideSpeed = 1.0f;
    public float wallJumpForceX = 0.5f;
    public float wallJumpForceY = 0.5f;
    public float wallJumpTime = 1.0f;
    public float dashMultiplier = 2.0f;
    public float dashCooldown = 1.5f;
    public Transform wallCheck;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public LayerMask obstaclesLayer;

    private Rigidbody2D rb;

    private float gravityScale;
    private float movementDirection;
    private float wallJumpDirection;
    private bool inWallJumpWindow;
    private bool isFacingRight;
    private bool isOnGround;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isWallJumping;
    private bool jump;
    private int jumpCount;
    private bool dash;
    private bool isDashing;
    private bool isDashAvailable;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
        isFacingRight = true;
        isDashAvailable = true;
    }

    void Update()
    {
        bool wasWallSliding = isWallSliding;

        isOnGround = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, 0.1f, groundLayer);
        movementDirection = Input.GetAxisRaw("Horizontal");
        isWallSliding = isTouchingWall && !isOnGround && movementDirection != 0;

        if (isWallSliding && wasWallSliding && !isWallJumping)
        {
            inWallJumpWindow = true;
            Invoke(nameof(ResetWallJumpWindow), 0.25f);
        }

        if ((isFacingRight && movementDirection < 0) || (!isFacingRight && movementDirection > 0))
        {
            Flip();
        }

        if (Input.GetButtonDown("Jump") && !isWallJumping && !isDashing)
        {
            jump = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isWallJumping && !isDashing && isDashAvailable)
        {
            dash = true;
        }
    }

    void FixedUpdate()
    {
        if (!isWallJumping)
        {
            ApplyMovement();
        }

        if (!isDashing)
        {
            if (isWallSliding)
            {
                ApplyWallSlide();
            }

            if (isWallJumping)
            {
                WallJump();
            }
            else if (jump)
            {
                ApplyJump();
            }

            ApplyGravity();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (1 << collision.gameObject.layer == obstaclesLayer)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void ApplyGravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = gravityScaleFalling;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }
    }

    private void ApplyMovement()
    {
        if (dash)
        {
            dash = false;
            isDashing = true;
            isDashAvailable = false;
            Invoke(nameof(ResetDash), 0.25f);
            Invoke(nameof(ResetDashAvailability), dashCooldown);
        }

        if (isDashing)
        {
            rb.velocity = new Vector2((isFacingRight ? 1 : -1) * movementSpeed * dashMultiplier, 0);
        }
        else
        {
            rb.velocity = new Vector2(movementDirection * movementSpeed, rb.velocity.y);
        }
    }

    private void ApplyWallSlide()
    {
        rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlideSpeed, float.MaxValue));
    }

    private void ApplyJump()
    {
        if (isOnGround || isWallSliding || inWallJumpWindow)
        {
            jumpCount = 0;
        }

        if (jumpCount < maxJumpCount)
        {
            if (isWallSliding)
            {
                isWallJumping = true;
                wallJumpDirection = movementDirection * -1;

                WallJump();
                Invoke(nameof(ResetWallJump), wallJumpTime);
            }
            else
            {
                Jump();
            }

            jumpCount++;
        }

        jump = false;
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    private void WallJump()
    {
        rb.velocity = new Vector2(wallJumpForceX * wallJumpDirection, wallJumpForceY);
    }

    private void ResetWallJump()
    {
        isWallJumping = false;
    }

    private void ResetDash()
    {
        isDashing = false;
    }

    private void ResetDashAvailability()
    {
        isDashAvailable = true;
    }

    private void ResetWallJumpWindow()
    {
        inWallJumpWindow = false;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        rb.transform.Rotate(0, 180, 0);
    }
}
