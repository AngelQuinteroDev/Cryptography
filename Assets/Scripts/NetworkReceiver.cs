using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NetworkReceiver : MonoBehaviour
{
    public int puerto = 9000;
    public bool usarLibrerias = true;

    private TcpListener _listener;
    private Thread _hilo;
    private string _logPendiente;

    void Start()
    {
        _listener = new TcpListener(IPAddress.Any, puerto);
        _listener.Start();
        _hilo = new Thread(Escuchar) { IsBackground = true };
        _hilo.Start();
        Debug.Log($"Escuchando en puerto {puerto}");
    }

    void Update()
    {
        if (_logPendiente != null)
        {
            Debug.Log(_logPendiente);
            _logPendiente = null;
        }
    }

    private void Escuchar()
    {
        while (true)
        {
            using var client = _listener.AcceptTcpClient();
            using var stream = client.GetStream();
            var buffer = new byte[65536];
            int leidos = stream.Read(buffer, 0, buffer.Length);
            string json = Encoding.UTF8.GetString(buffer, 0, leidos);

            var paquete = JsonUtility.FromJson<NetworkSender.Paquete>(json);
            ProcesarPaquete(paquete);
        }
    }

    private void ProcesarPaquete(NetworkSender.Paquete p)
    {
        ICryptoAdapter crypto = usarLibrerias
            ? (ICryptoAdapter)new LibraryCryptoAdapter()
            : new CustomCryptoAdapter("gato");

        // Recalcula el hash del mensaje recibido
        string hashRecalculado = crypto.Hash(p.mensaje);

        // Verifica la firma con la llave pública recibida
        bool firmaOk = crypto.Verify(p.hash, p.firma, p.llavePublica);

        // Verifica integridad (hash que viene == hash recalculado)
        bool integridadOk = hashRecalculado == p.hash;

        _logPendiente = $"[RECEPTOR]\n" +
                        $"Mensaje:    {p.mensaje}\n" +
                        $"Hash recib: {p.hash}\n" +
                        $"Hash recalc:{hashRecalculado}\n" +
                        $"Integridad: {integridadOk}\n" +
                        $"Autenticidad: {firmaOk}";
    }

    void OnDestroy() => _listener?.Stop();
}