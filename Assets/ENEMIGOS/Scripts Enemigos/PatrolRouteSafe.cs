using UnityEngine;
using System.Collections;

public class PatrolRouteSafe : MonoBehaviour
{
    [Header("IA Objetivos")]
    public Transform player;
    public float detectionRange = 6f;
    public float attackRange = 1.5f;

    [Header("Estado de Combate")]
    public int vida = 3;
    public float attackCooldown = 2.0f;
    public float fuerzaRebote = 5f;
    public float tiempoAturdido = 0.5f;

    [Header("Puntos de patrulla")]
    public Transform pointA;
    public Transform pointB;
    public float autoPointOffset = 2f;

    [Header("Movimiento")]
    public float moveSpeed = 2f;
    [Tooltip("Aumentado a 0.5 para evitar que se quede girando")]
    public float stopDistance = 0.5f; // <--- IMPORTANTE: Aumentado para evitar el error
    public float rotationSpeed = 5f;  // <--- REDUCIDO: Giro más suave

    [Header("Animator")]
    public AnimatorBridgeSafe animatorBridge;

    // Variables internas
    private Transform currentTarget;
    private Rigidbody rb;
    private Animator animator;
    
    // Estados
    private bool isAttacking = false;
    private bool recibiendoDaño = false;
    private bool muerto = false;
    private float lastAttackTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        
        if (animatorBridge == null) animatorBridge = GetComponent<AnimatorBridgeSafe>();

        // Crear puntos si no existen
        if (pointA == null) pointA = transform.Find("PointA");
        if (pointB == null) pointB = transform.Find("PointB");

        if (pointA == null)
        {
            GameObject a = new GameObject("PointA");
            a.transform.parent = transform; 
            // IMPORTANTE: Ponemos los puntos a la misma altura del enemigo para evitar problemas
            a.transform.position = transform.position + new Vector3(-autoPointOffset, 0f, 0f);
            pointA = a.transform;
        }
        if (pointB == null)
        {
            GameObject b = new GameObject("PointB");
            b.transform.parent = transform;
            b.transform.position = transform.position + new Vector3(autoPointOffset, 0f, 0f);
            pointB = b.transform;
        }

        currentTarget = pointB;
    }

    void Update()
    {
        if (muerto || recibiendoDaño) return;

        // --- DETECCIÓN DE JUGADOR ---
        bool playerDetected = false;
        float distToPlayer = 999f;

        if (player != null)
        {
            // Calculamos distancia ignorando altura para evitar errores si uno está más alto
            distToPlayer = Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(player.position.x, 0, player.position.z)
            );

            if (distToPlayer <= detectionRange) playerDetected = true;
        }

        // --- LÓGICA ---
        if (playerDetected)
        {
            if (isAttacking) return;

            if (distToPlayer <= attackRange)
            {
                DetenerMovimiento();
                if (Time.time - lastAttackTime >= attackCooldown) Atacar();
            }
            else
            {
                MoverHacia(player.position);
            }
        }
        else // MODO PATRULLA
        {
            if (currentTarget == null) return;

            // CORRECCIÓN DEL ERROR DE GIRO:
            // Calculamos la distancia PLANA (solo X y Z), ignorando si el punto está más alto o bajo.
            float distToPoint = Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(currentTarget.position.x, 0, currentTarget.position.z)
            );

            // Si está cerca, cambiamos de punto
            if (distToPoint <= stopDistance)
            {
                currentTarget = (currentTarget == pointA) ? pointB : pointA;
            }

            MoverHacia(currentTarget.position);
        }
    }

    void MoverHacia(Vector3 destino)
    {
        Vector3 dir = destino - transform.position;
        dir.y = 0f; // Importante: Forzar plano horizontal

        // CORRECCIÓN DE GIRO LOCO:
        // Solo intentamos movernos o girar si la distancia es significativa.
        if (dir.magnitude < 0.1f) 
        {
             // Si está casi encima del punto, no te muevas ni gires.
             if (animatorBridge != null) animatorBridge.SetSpeed(0f);
             return; 
        }

        Vector3 desiredVel = dir.normalized * moveSpeed;

        // Movimiento físico
        if (rb != null)
            rb.MovePosition(rb.position + desiredVel * Time.deltaTime);
        else
            transform.position += desiredVel * Time.deltaTime;

        // Rotación Suavizada
        if (desiredVel.sqrMagnitude > 0.1f) // Solo girar si se mueve rápido
        {
            Quaternion targetRot = Quaternion.LookRotation(desiredVel.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        if (animatorBridge != null)
        {
            float currentSpeed = (rb != null) ? rb.linearVelocity.magnitude : desiredVel.magnitude;
            animatorBridge.SetSpeed(currentSpeed);
        }
    }

    void DetenerMovimiento()
    {
        if (animatorBridge != null) animatorBridge.SetSpeed(0f);
    }

    void Atacar()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        if(dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);

        lastAttackTime = Time.time;
        isAttacking = true;

        if (animator != null) animator.SetTrigger("atacar");
        Invoke("ResetAttackState", 1.0f);
    }

    void ResetAttackState()
    {
        isAttacking = false;
    }

    public void RecibirDaño(Vector3 posicionAtacante, int cantidad)
    {
        if (muerto) return;

        vida -= cantidad;

        if (vida <= 0)
        {
            muerto = true;
            recibiendoDaño = false;
            if (animatorBridge != null) animatorBridge.SetSpeed(0);
            if (animator != null) animator.SetTrigger("morir");
            if (rb != null) rb.isKinematic = true;
            GetComponent<Collider>().enabled = false;
            Destroy(gameObject, 3.0f);
        }
        else if (!recibiendoDaño)
        {
            recibiendoDaño = true;
            if (animatorBridge != null) animatorBridge.SetSpeed(0);
            if (animator != null) animator.SetTrigger("hit");

            Vector3 dirEmpuje = (transform.position - posicionAtacante).normalized;
            dirEmpuje.y = 0.2f;

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.AddForce(dirEmpuje * fuerzaRebote, ForceMode.Impulse);
            }
            StartCoroutine(Recuperarse());
        }
    }

    IEnumerator Recuperarse()
    {
        yield return new WaitForSeconds(tiempoAturdido);
        recibiendoDaño = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (pointA != null) Gizmos.DrawSphere(pointA.position, 0.2f);
        if (pointB != null) Gizmos.DrawSphere(pointB.position, 0.2f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}