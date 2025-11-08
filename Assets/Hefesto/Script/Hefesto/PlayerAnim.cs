using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    private PlayerController player;

    private Animator playerAnim;

    void Awake()
    {
        player = FindFirstObjectByType<PlayerController>();
        playerAnim = GetComponent<Animator>();
    }

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        RunAnim();
        JumpAnim();
    }

    void Update()
    {
        AttackAnim();
    }

    void RunAnim()
    {
        if (player.playerMove)
        {
            playerAnim.SetBool("Run", true);
        }
        else
        {
            playerAnim.SetBool("Run", false);
        }
    }

    void JumpAnim()
    {
        if (player.checkGround == false)
        {
            playerAnim.SetBool("Jump", true);
        }
        else
        {
            playerAnim.SetBool("Jump", false);
        }
    }

    void AttackAnim()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            playerAnim.SetTrigger("Atk");
            player.PlayerAttack();
        }
    }
}
