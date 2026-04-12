using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void AbrirLibrerias()
    {
        SceneManager.LoadScene("Tcp_Sender");
        SceneManager.LoadScene("Tcp_Receiver", LoadSceneMode.Additive);
    }

    public void AbrirCustom()
    {
        SceneManager.LoadScene("Custom_Sender");
        SceneManager.LoadScene("Custom_Receiver", LoadSceneMode.Additive);
    }
}
