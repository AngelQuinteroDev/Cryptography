using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetworkSender : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int puerto = 9000;
    public CryptoManager cryptoManager;

    [System.Serializable]
    public class Paquete
    {
        public string mensaje;
        public string hash;
        public string firma;
        public string llavePublica;
    }

    public void Enviar()
    {
        var (hash, firma, llave) = cryptoManager.ObtenerPaquete();

        var paquete = new Paquete
        {
            mensaje = cryptoManager.campoMensaje.text,
            hash = hash,
            firma = firma,
            llavePublica = llave
        };

        string json = JsonUtility.ToJson(paquete);
        byte[] datos = Encoding.UTF8.GetBytes(json);

        using var client = new TcpClient(ip, puerto);
        using var stream = client.GetStream();
        stream.Write(datos, 0, datos.Length);

        Debug.Log($"Enviado {datos.Length} bytes a {ip}:{puerto}");
    }
}