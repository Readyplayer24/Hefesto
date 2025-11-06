using JetBrains.Annotations;
using UnityEngine;

public class HefestoMovimientos : MonoBehaviour
{
    [Header("Configuracion de movimiento")]
    public float velocidadMovimiento = 5f;
    public float fuerzaDeSalto = 7f;
    public bool enElSuelo;

    private Rigidbody rb;
    private float ejeZInicial;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        ejeZInicial = transform.position.z;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump") && enElSuelo)
        {
            rb.AddForce(Vector3.up * fuerzaDeSalto, ForceMode.Impulse);
            enElSuelo = false;
        }
    }

    private void FixedUpdate()
    {
        float movimientoX = Input.GetAxis("Horizontal");
        Vector3 movimiento = new Vector3(movimientoX, 0f, 0f);

        rb.MovePosition(transform.position + movimiento * velocidadMovimiento * Time.fixedDeltaTime);

        transform.position = new Vector3(transform.position.x, transform.position.y, ejeZInicial);

        if (movimiento != Vector3.zero)
        {
            transform.rotation = Quaternion.Euler(0f, movimientoX > 0 ? 90f : -90f, 0f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            enElSuelo = true;
        }
    }

}
