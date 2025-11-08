using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    public float playerSpeed;

    public float playerRotate;

    public float jumpSpeed;

    public bool playerMove = false;

    public bool checkGround = true;

    public Transform chkGround;

    public Transform atkPoint;

    public float atkRange;

    public LayerMask enemyLayer;

    private Rigidbody rb;

    private Vector3 displacement;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        float mh = Input.GetAxis("Horizontal");
        PlayerMove(mh);
        PlayerJumper();
    }

    void Update()
    {
        
    }

    void PlayerMove(float mh)
    {
        displacement.Set(0f, 0f, mh);
        displacement = displacement.normalized * playerSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + displacement);

        if(mh != 0f)
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
        Vector3 dwn = transform.TransformDirection(Vector3.down);
        RaycastHit hit;

        if (Input.GetButton("Jump") && checkGround)
        {
            rb.linearVelocity = new Vector3(0f, jumpSpeed, 0f);
            checkGround = false;
        }

        if (Physics.Raycast(chkGround.position, dwn, out hit, 0.2f) && hit.collider.CompareTag("Ground"))
        {
            checkGround = true;
        }
        else
        {
            checkGround = false;
        }
    }

    public void PlayerAttack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(atkPoint.position, atkRange, enemyLayer);
        foreach (Collider hitenemy in hitColliders)
        {
            print("Atacando" + hitenemy.name);
        }
    }
}
