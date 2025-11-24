using UnityEngine;
using System.Collections; // Necesario para las Corrutinas

public class EnemyController : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform player;

    [Header("Configuración de Visión")]
    public float detectionRange = 5f;
    public float visionAngle = 30f;

    [Header("Configuración de Ataque")]
    public float attackRange = 1.5f;
    public float attackCooldown = 2.0f;

    [Header("Configuración de Vida y Daño")]
    public int vida = 3;
    public float fuerzaRebote = 5f;   // Qué tan fuerte lo empujan
    public float tiempoAturdido = 0.5f; // Tiempo que se queda quieto tras el golpe

    [Header("Movimiento")]
    public float speed = 2.0f;
    public float rotationSpeed = 12f;

    // Componentes
    private Rigidbody rb;
    private Animator animator;

    // Estados
    private bool enMovimiento;
    private bool muerto = false;
    private bool recibiendoDaño = false;
    private bool isAttacking = false;

    // Control de tiempo
    private float lastAttackTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // ------------------ BLOQUEOS DE ESTADO ------------------
        
        // 1. Si está muerto, no hace nada.
        if (muerto) return;

        // 2. Si le acaban de pegar, el script de movimiento se pausa para dejar que la física actúe.
        if (recibiendoDaño) return;

        // 3. Si el jugador no está asignado o desapareció.
        if (player == null) return;

        // ------------------ LÓGICA DE INTELIGENCIA ARTIFICIAL ------------------

        // Calculamos distancia y dirección
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f; // Ignoramos altura
        float distanceToPlayer = toPlayer.magnitude;
        float angleToPlayer = Vector3.Angle(transform.forward, toPlayer);

        // Si está en medio de una animación de ataque, esperamos a que termine
        if (isAttacking) return;

        // --- TOMAS DE DECISIÓN ---

        // CASO A: Está lo suficientemente cerca para atacar
        if (distanceToPlayer <= attackRange)
        {
            DetenerMovimiento();
            
            // Revisamos el cooldown (tiempo de espera entre golpes)
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Atacar();
            }
        }
        // CASO B: Está lejos, pero lo ve y está en rango -> PERSEGUIR
        else if (distanceToPlayer <= detectionRange && angleToPlayer <= visionAngle)
        {
            MoverHaciaJugador(toPlayer.normalized);
        }
        // CASO C: No lo ve o está muy lejos -> QUIETO (IDLE)
        else
        {
            DetenerMovimiento();
        }

        // Actualizamos la animación de caminar/correr
        if (animator != null) animator.SetBool("enMovimiento", enMovimiento);
    }

    // ================= MÉTODOS DE MOVIMIENTO =================

    void MoverHaciaJugador(Vector3 direction)
    {
        enMovimiento = true;

        // Girar suavemente hacia el jugador
        Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

        // Moverse hacia adelante
        Vector3 moveStep = direction * speed * Time.deltaTime;
        
        if (rb != null) rb.MovePosition(rb.position + moveStep);
        else transform.position += moveStep;
    }

    void DetenerMovimiento()
    {
        enMovimiento = false;
    }

    // ================= MÉTODOS DE ATAQUE =================

    void Atacar()
    {
        // Pequeño giro final para asegurar que mire al jugador al atacar
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);

        lastAttackTime = Time.time;
        isAttacking = true;

        // Disparar animación
        if (animator != null) animator.SetTrigger("atacar");

        // Reiniciar estado después de 1 segundo (ajusta según dure tu animación)
        Invoke("ResetAttackState", 1.0f);
    }

    void ResetAttackState()
    {
        isAttacking = false;
    }

    // ================= MÉTODOS PÚBLICOS (RECIBIR DAÑO) =================

    // ESTA ES LA FUNCIÓN QUE LLAMA TU JUGADOR O ARMA
    public void RecibirDaño(Vector3 posicionAtacante, int cantidad)
    {
        if (muerto) return; // Si ya está muerto, ignoramos golpes extra

        vida -= cantidad;

        // --- MUERTE ---
        if (vida <= 0)
        {
            muerto = true;
            enMovimiento = false;
            recibiendoDaño = false;

            if (animator != null)
            {
                animator.SetBool("enMovimiento", false);
                animator.SetTrigger("morir");
            }

            // Desactivamos la física para que no estorbe
            if (rb != null) rb.isKinematic = true;
            GetComponent<Collider>().enabled = false;

            // Destruimos el objeto en 3 segundos
            Destroy(gameObject, 3.0f);
            return;
        }

        // --- GOLPE (Si sigue vivo) ---
        if (!recibiendoDaño)
        {
            recibiendoDaño = true; // Bloquea el movimiento normal en Update
            enMovimiento = false;
            
            // Animación de dolor
            if (animator != null) animator.SetTrigger("hit");

            // Lógica de Rebote (Knockback 3D)
            Vector3 direccionEmpuje = (transform.position - posicionAtacante).normalized;
            direccionEmpuje.y = 0.2f; // Un poquito hacia arriba para evitar fricción con el suelo

            if (rb != null)
            {
                // Use linearVelocity instead of the deprecated velocity property
                rb.linearVelocity = Vector3.zero; // Frenar en seco antes del empuje
                rb.AddForce(direccionEmpuje * fuerzaRebote, ForceMode.Impulse);
            }

            // Iniciar cuenta regresiva para recuperarse
            StartCoroutine(DesactivarDano());
        }
    }

    IEnumerator DesactivarDano()
    {
        yield return new WaitForSeconds(tiempoAturdido);
        recibiendoDaño = false; 
        // Al ponerse en false, el Update() vuelve a tomar el control
    }

    // ================= AYUDAS VISUALES (GIZMOS) =================
    void OnDrawGizmosSelected()
    {
        // Rango de visión
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}