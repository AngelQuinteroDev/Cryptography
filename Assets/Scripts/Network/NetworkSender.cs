using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetworkSender : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int puerto = 9000;
    public CryptoManager cryptoManager;

    public void Enviar()
    {
        var (hash, firma, llave) = cryptoManager.ObtenerPaquete();

        if (string.IsNullOrEmpty(hash))
        {
            Debug.LogWarning("[NetworkSender] Paquete vacío, no se envía.");
            return;
        }

        var paquete = new Paquete
        {
            mensaje      = cryptoManager.campoMensaje.text,
            hash         = hash,
            firma        = firma,
            llavePublica = llave,
            usarLibrerias = cryptoManager.usarLibrerias,
            claveCustom   = cryptoManager.usarLibrerias ? "" : cryptoManager.claveCustom
        };

        string json = JsonUtility.ToJson(paquete);
        byte[] datos = Encoding.UTF8.GetBytes(json);

        try
        {
            using var client = new TcpClient(ip, puerto);
            using var stream = client.GetStream();
            stream.Write(datos, 0, datos.Length);
            Debug.Log($"[NetworkSender] Enviado {datos.Length} bytes a {ip}:{puerto}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[NetworkSender] Error al enviar: {ex.Message}");
        }
    }
}