using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    /// <summary>
    /// Carga las escenas Tcp_Sender y Tcp_Receiver en simultáneo.
    /// Conectar al botón "Librerías" en el Inspector.
    /// </summary>
    public void AbrirLibrerias()
    {
        SceneManager.LoadScene("Tcp_Sender");
        SceneManager.LoadScene("Tcp_Receiver", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Carga las escenas Custom_Sender y Custom_Receiver en simultáneo.
    /// Conectar al botón "Custom" en el Inspector.
    /// </summary>
    public void AbrirCustom()
    {
        SceneManager.LoadScene("Custom_Sender");
        SceneManager.LoadScene("Custom_Receiver", LoadSceneMode.Additive);
    }
}
