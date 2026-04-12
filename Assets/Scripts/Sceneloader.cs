using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Nombres exactos de las escenas (sin .unity)")]
    public string escenaSender   = "Sender";
    public string escenaReceiver = "Receiver";

    void Start()
    {
        StartCoroutine(CargarEscenas());
    }

    private IEnumerator CargarEscenas()
    {
        // 1. Carga el Receiver y espera a que su Start() se ejecute
        yield return SceneManager.LoadSceneAsync(escenaReceiver, LoadSceneMode.Additive);
        yield return null;  // frame extra para que TcpListener quede activo

        // 2. Ahora carga el Sender
        yield return SceneManager.LoadSceneAsync(escenaSender, LoadSceneMode.Additive);

        Debug.Log("[SceneLoader] Ambas escenas listas.");
    }
}