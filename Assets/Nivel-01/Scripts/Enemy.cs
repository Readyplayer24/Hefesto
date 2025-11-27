using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Configuración del Enemigo")]
    public int maxHealth = 3; 
    private int currentHealth; 

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Enemigo golpeado. Vida restante: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("¡Enemigo destruido!");
        Destroy(gameObject);
    }
}
