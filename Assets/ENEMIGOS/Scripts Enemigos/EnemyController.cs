using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 5f;
    public float visionAngle = 30f;   // ángulo de visión frontal
    public float speed = 2.0f;
    public float rotationSpeed = 12f;

    private Rigidbody rb;
    private bool enMovimiento;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null)
        {
            enMovimiento = false;
            if (animator != null) animator.SetBool("enMovimiento", enMovimiento);
            return;
        }

        // Dirección hacia el jugador en plano XZ
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float distanceToPlayer = toPlayer.magnitude;

        // Ángulo entre forward y dirección al jugador
        float angleToPlayer = Vector3.Angle(transform.forward, toPlayer);

        Vector3 desiredVel = Vector3.zero;

        // Solo detecta si está dentro del rango Y dentro del ángulo de visión
        if (distanceToPlayer <= detectionRange && angleToPlayer <= visionAngle)
        {
            Vector3 direction = toPlayer.normalized;
            desiredVel = direction * speed;
            enMovimiento = true;

            // Orientación hacia el jugador
            Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
        else
        {
            enMovimiento = false;
        }

        // Movimiento con Rigidbody
        if (rb != null)
            rb.MovePosition(rb.position + desiredVel * Time.deltaTime);
        else
            transform.position += desiredVel * Time.deltaTime;

        // Animator
        if (animator != null) animator.SetBool("enMovimiento", enMovimiento);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Aquí puedes poner lógica de daño
        }
    }

    void OnDrawGizmosSelected()
    {
        // Dibuja el rango y el cono de visión para depurar
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Cono de visión
        Vector3 leftDir = Quaternion.Euler(0, -visionAngle, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, visionAngle, 0) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, leftDir * detectionRange);
        Gizmos.DrawRay(transform.position, rightDir * detectionRange);
    }
}