using UnityEngine;
using System.Collections;

public class SwingingPlatform : MonoBehaviour
{
    [Header("Configuración del Movimiento")]
    public float moveDistance = 3f;
    public float moveSpeed = 1f;
    public float delayTime = 3f;

    private Vector3 startPosition;
    private int direction = 1;

    void Start()
    {
        startPosition = transform.position;
        InvokeRepeating("ChangeDirection", 0f, delayTime);
    }

    void FixedUpdate()
    {
        Vector3 targetPosition = startPosition + (Vector3.forward * direction * moveDistance);

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    void ChangeDirection()
    {
        direction = -direction;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null);
        }
    }
}
