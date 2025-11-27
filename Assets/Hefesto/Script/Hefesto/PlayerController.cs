using System.Net;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    public float playerSpeed = 5f;

    public float playerRotate;

    [Header("Movimiento")]
    public float jumpSpeed = 7f;

    [Header("Chequeo de Suelo")]
    public Transform chkGround; 
    public float groundCheckRadius = 0.2f;
    public LayerMask whatIsGround;

    private Rigidbody rb;
    private bool isGrounded = false;

    public bool playerMove = false;

    public bool checkGround = true;

    public Transform atkPoint;

    [Header("Dash de Fuego")]
    public float dashForce = 18f; 
    public float dashDuration = 0.2f;
    public float dashCooldown = 1.2f;
    private bool isDashing = false;
    private bool canDash = true;

    [Header("Combate")]
    public float atkDamage = 25f;
    public float atkRange;
    
    public LayerMask enemyLayer;

    private Animator PlayerAnim;

    private Vector3 displacement;

    private float horizontalInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        PlayerAnim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(horizontalInput * playerSpeed, rb.linearVelocity.y, rb.linearVelocity.z);

        if (Mathf.Abs(horizontalInput) < 0.01f)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, rb.linearVelocity.z);
        }

        if (horizontalInput < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (horizontalInput > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        PlayerMove(horizontalInput);
    }

    void Update()
    {
        HandleAttackInput();
        horizontalInput = Input.GetAxisRaw("Horizontal");

        PlayerJumper();

        UpdateAnimation();

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Dash();
        }
    }

    void PlayerMove(float mh)
    {
        displacement.Set(0f, 0f, mh);
        displacement = displacement.normalized * playerSpeed * Time.deltaTime;

        rb.MovePosition(transform.position + displacement);

        if (Mathf.Abs(mh) < 0.01f)
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (mh != 0f)
        {
            PlayerRotate(mh);
        }

        bool playerRun = mh != 0f;

        if (playerRun)
        {
            playerMove = true;
        }
        else
        {
            playerMove = false;
        }
    }

    void PlayerRotate(float mh)
    {
        float interpolation = playerRotate * mh;
        Vector3 targetDireccion = new Vector3(0f, 0f, mh);
        Quaternion targetRotation = Quaternion.LookRotation(targetDireccion, Vector3.up);
        Quaternion newRotation = Quaternion.Slerp(rb.rotation, targetRotation, interpolation);
        rb.MoveRotation(newRotation);
    }

    void PlayerJumper()
    {
        CheckIfGrounded();
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpSpeed, rb.linearVelocity.z);
            isGrounded = false;
        }
    }

    void CheckIfGrounded()
    {
        isGrounded = Physics.CheckSphere(chkGround.position, groundCheckRadius, whatIsGround);
    }

    void UpdateAnimation()
    {
        PlayerAnim.SetFloat("Run", Mathf.Abs(rb.linearVelocity.x));
    }

    void HandleAttackInput()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            PlayerAnim.SetTrigger("atk");
            PlayerAttack();
        }
    }

    public void PlayerAttack()
    {
        float atkDamage = 25f;
        Collider[] hitColliders = Physics.OverlapSphere(atkPoint.position, atkRange);

        foreach (Collider hitenemy in hitColliders)
        {
            print("Atacando" + hitenemy.name);
            Destructible destructible = hitenemy.GetComponent<Destructible>();
            if (destructible != null)
            {
                destructible.TakeDamage(atkDamage);
                continue;
            }
        }
    }

    void Dash()
    {
        if (canDash && !isDashing)
        {
            canDash = false;
            isDashing = true;
            StartCoroutine(DashSequence());
            Invoke("ResetDashCooldown", dashCooldown);
        }
    }

    void ResetDashCooldown()
    {
        canDash = true;
    }

    IEnumerator DashSequence()
    {
        float dashDirection = transform.localScale.x;
        rb.useGravity = false;
        rb.linearVelocity = new Vector3(0, 0, dashDirection * dashForce);
        yield return new WaitForSeconds(dashDuration);
        rb.useGravity = true;
        isDashing = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0f);
    }
}