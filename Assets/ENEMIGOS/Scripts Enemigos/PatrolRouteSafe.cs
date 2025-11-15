using UnityEngine;

public class PatrolRouteSafe : MonoBehaviour
{
    [Header("Puntos de patrulla")]
    public Transform pointA;
    public Transform pointB;
    public float autoPointOffset = 2f;

    [Header("Movimiento")]
    public float moveSpeed = 2f;
    public float stopDistance = 0.1f;
    public float rotationSpeed = 10f; // suaviza el giro

    [Header("Animator")]
    public AnimatorBridgeSafe animatorBridge;

    Transform currentTarget;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (animatorBridge == null) animatorBridge = GetComponent<AnimatorBridgeSafe>();

        if (pointA == null) pointA = transform.Find("PointA");
        if (pointB == null) pointB = transform.Find("PointB");

        if (pointA == null)
        {
            GameObject a = new GameObject("PointA");
            a.transform.parent = transform;
            a.transform.localPosition = new Vector3(-autoPointOffset, 0f, 0f);
            pointA = a.transform;
        }
        if (pointB == null)
        {
            GameObject b = new GameObject("PointB");
            b.transform.parent = transform;
            b.transform.localPosition = new Vector3(autoPointOffset, 0f, 0f);
            pointB = b.transform;
        }

        currentTarget = pointB;
    }

    void Update()
    {
        if (currentTarget == null) return;

        // Dirección en plano XZ hacia el target
        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0f;

        // Avance con velocidad constante
        Vector3 desiredVel = (dir.sqrMagnitude > stopDistance * stopDistance)
            ? dir.normalized * moveSpeed
            : Vector3.zero;

        // Movimiento con Rigidbody (fallback si no hay)
        if (rb != null)
            rb.MovePosition(rb.position + desiredVel * Time.deltaTime);
        else
            transform.position += desiredVel * Time.deltaTime;

        // Orientación segura (evita vector cero)
        if (desiredVel.sqrMagnitude > 1e-6f)
        {
            Quaternion targetRot = Quaternion.LookRotation(desiredVel.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Animator: velocidad real del Rigidbody (o de la intención)
        if (animatorBridge != null)
        {
            float currentSpeed = (rb != null) ? rb.linearVelocity.magnitude : desiredVel.magnitude;
            animatorBridge.SetSpeed(currentSpeed);
        }

        // Cambio de punto cuando se alcanza
        if (dir.magnitude <= stopDistance)
            currentTarget = (currentTarget == pointA) ? pointB : pointA;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (pointA != null) Gizmos.DrawSphere(pointA.position, 0.15f);
        if (pointB != null) Gizmos.DrawSphere(pointB.position, 0.15f);
        if (pointA != null && pointB != null) Gizmos.DrawLine(pointA.position, pointB.position);
    }
}