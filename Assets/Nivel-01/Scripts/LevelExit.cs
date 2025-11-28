using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [Header("Configuración de Escenas")]
    public string targetSceneName = "MenuPrincipal";

    private const string PlayerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(PlayerTag))
        {
            Debug.Log("Jugador alcanzó la salida. Cargando Menú Principal.");
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
