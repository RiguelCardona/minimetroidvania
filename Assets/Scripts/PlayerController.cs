using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerStateList pState;
    [SerializeField] Animator anim;
    private Rigidbody2D rb;

    [Header("Movilidad")]
    [SerializeField] float runSpeed = 25f;

    [Space(2)]

    [SerializeField] float jumpSpeed = 45f;
    [SerializeField] float fallSpeed = 45f;
    [SerializeField] int jumpSteps = 20;
    [SerializeField] int jumpThreshold = 7;
    [Space(5)]

    [Header("Chequeo Suelo")]
    [SerializeField] Transform groundTransform;
    [SerializeField] float groundCheckX = 1f;
    [SerializeField] float groundCheckY = 0.3f;
    [SerializeField] LayerMask groundLayer;

    [Header("Chequeo Techo")]
    [SerializeField] Transform roofTransform;
    [SerializeField] float roofCheckX = 1f;
    [SerializeField] float roofCheckY = 0.3f; 
    [Space(5)]

    float grabity;
    int stepsJumped = 0;
    float xAxis;
    float yAxis;
    //public bool isGrounded;

    void Awake()
    {
		if(pState == null)
        {
            pState = GetComponent<PlayerStateList>();
        }

        rb = GetComponent<Rigidbody2D>();

        grabity = rb.gravityScale;        

        pState.facingRight = true;
    }


    void Update()
    {
        GetInputs();

        if (xAxis > 0 && !pState.facingRight)
        {
            Flip();
        } 
        else if (xAxis < 0 && pState.facingRight)
        {
            Flip();
        }
    }


    void FixedUpdate()
    {
        Grounded();
        /*
        isGrounded = Physics2D.OverlapCircle(groundTransform.position, 0.3f, groundLayer);
        if(isGrounded)
        {
            pState.onGround = true;
        }
        */
        Move(xAxis, pState.jumping, pState.dashing);
    }




    //FUNCIONES
    void Flip()
    {
        if (xAxis > 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
            pState.facingRight = true;
        }
        else if (xAxis < 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
            pState.facingRight = false;
        }
    }

    void Jump()
    {
        if (pState.jumping)
        {

            if (stepsJumped < jumpSteps && !Roofed())
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
                stepsJumped++;
            }
            else
            {
                StopJumpSlow();
            }
        }

        //Limite de velocidad al caer
        if (rb.velocity.y < -Mathf.Abs(fallSpeed))
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -Mathf.Abs(fallSpeed), Mathf.Infinity));
        }
    }

    void StopJumpQuick()
    {
        //Stops The player jump immediately, causing them to start falling as soon as the button is released.
        stepsJumped = 0;
        pState.jumping = false;
        rb.velocity = new Vector2(rb.velocity.x, 0);
    }

    void StopJumpSlow()
    {
        //stops the jump but lets the player hang in the air for awhile.
        stepsJumped = 0;
        pState.jumping = false;
    }

    public bool Grounded()
    {
        //this does three small raycasts at the specified positions to see if the player is grounded.
        if (Physics2D.Raycast(groundTransform.position, Vector2.down, groundCheckY, groundLayer) || Physics2D.Raycast(groundTransform.position + new Vector3(-groundCheckX, 0), Vector2.down, groundCheckY, groundLayer) || Physics2D.Raycast(groundTransform.position + new Vector3(groundCheckX, 0), Vector2.down, groundCheckY, groundLayer))
        {
            if(!pState.onGround)
            {
                anim.SetTrigger("landing");
            }
            pState.onGround = true;
            return true;
        }
        else
        {
            pState.onGround = false;
            return false;
        }
    }

    public bool Roofed()
    {
        //This does the same thing as grounded but checks if the players head is hitting the roof instead.
        //Used for canceling the jump.
        if (Physics2D.Raycast(roofTransform.position, Vector2.up, roofCheckY, groundLayer) || Physics2D.Raycast(roofTransform.position + new Vector3(roofCheckX, 0), Vector2.up, roofCheckY, groundLayer) || Physics2D.Raycast(roofTransform.position + new Vector3(roofCheckX, 0), Vector2.up, roofCheckY, groundLayer))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void GetInputs()
    {
        yAxis = Input.GetAxis("Vertical");
        xAxis = Input.GetAxis("Horizontal");

        //Treshold para sensibilidad y falsos positivos, revisar inputmanagger
        if (yAxis > 0.25)
        {
            yAxis = 1;
        }
        else if (yAxis < -0.25)
        {
            yAxis = -1;
        }
        else
        {
            yAxis = 0;
        }

        if (xAxis > 0.25)
        {
            xAxis = 1;
        }
        else if (xAxis < -0.25)
        {
            xAxis = -1;
        }
        else
        {
            xAxis = 0;
        }

        anim.SetBool("Grounded", Grounded());
        anim.SetFloat("YVelocity", rb.velocity.y);

        //Jumping
        if (Input.GetButtonDown("Jump") && Grounded())
        {
            pState.jumping = true;
            anim.SetTrigger("takeOf");
        }

        if (!Input.GetButton("Jump") && stepsJumped < jumpSteps && stepsJumped > jumpThreshold && pState.jumping)
        {
            StopJumpQuick();
        }
        else if (!Input.GetButton("Jump") && stepsJumped < jumpThreshold && pState.jumping)
        {
            StopJumpSlow();
        }
    }


    private void Move(float xAxis, bool jump, bool dash)
    {
        //rb.velocity = new Vector2(xAxis * runSpeed * Time.fixedDeltaTime), rb.velocity.y);
        rb.velocity = new Vector2(xAxis * runSpeed, rb.velocity.y);

        if (Mathf.Abs(rb.velocity.x) > 0)
        {
            pState.walking = true;
        }
        else
        {
            pState.walking = false;
        }        

        anim.SetBool("isRunning", pState.walking);

        if (pState.jumping)
        {
            Jump();
        }
        /*
        if (pState.dashing)
        {
            Dash();
        }
        */
    }    

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundTransform.position, new Vector2(groundCheckX, groundCheckY));

        Gizmos.DrawLine(groundTransform.position, groundTransform.position + new Vector3(0, -groundCheckY));
        Gizmos.DrawLine(groundTransform.position + new Vector3(-groundCheckX, 0), groundTransform.position + new Vector3(-groundCheckX, -groundCheckY));
        Gizmos.DrawLine(groundTransform.position + new Vector3(groundCheckX, 0), groundTransform.position + new Vector3(groundCheckX, -groundCheckY));

        Gizmos.DrawLine(roofTransform.position, roofTransform.position + new Vector3(0, roofCheckY));
        Gizmos.DrawLine(roofTransform.position + new Vector3(-roofCheckX, 0), roofTransform.position + new Vector3(-roofCheckX, roofCheckY));
        Gizmos.DrawLine(roofTransform.position + new Vector3(roofCheckX, 0), roofTransform.position + new Vector3(roofCheckX, roofCheckY));
    }
}
