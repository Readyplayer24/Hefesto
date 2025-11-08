using UnityEngine;

public class EnemyBackstab : MonoBehaviour
{
    [Header("Configuración")]
    public float backAngleThreshold = 50f; // Angulo para considerar 'por la espalda'
    public bool isDead = false;

    [Header("Animación")]
    public Animator animator;
    public string deathAnimation = "Death"; // Nombre del clip en el Animator

    // Llamado por el jugador cuando ataca
    public void TakeDamage(Transform attacker)
    {
        if (isDead) return;

        // Vector desde el enemigo hacia el atacante
        Vector3 dirToAttacker = (attacker.position - transform.position).normalized;

        // Compara con la dirección hacia ATRÁS del enemigo
        float angle = Vector3.Angle(-transform.forward, dirToAttacker);

        if (angle < backAngleThreshold)
        {
            // Ataque por la espalda
            Die();
        }
        else
        {
            // Ataque normal (opcional: daño normal)
            Debug.Log("Ataque frontal o lateral, no muere instantáneamente");
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Backstab exitoso — enemigo muerto");

        if (animator != null && !string.IsNullOrEmpty(deathAnimation))
        {
            animator.Play(deathAnimation);
        }

        // Si quieres destruir el objeto luego:
        Destroy(gameObject, 2f);
    }
}

