using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;

public class NetworkReceiver : MonoBehaviour
{
    [Header("Red")]
    public int puerto = 9000;

    [Header("UI - Resultados")]
    public TextMeshProUGUI labelMensaje;
    public TextMeshProUGUI labelIntegridad;
    public TextMeshProUGUI labelAutenticidad;
    public TextMeshProUGUI labelModo;

    private TcpListener _listener;
    private Thread _hilo;

    // Cola thread-safe para pasar resultados al hilo principal
    private string _mensajeRecibido;
    private string _resultadoHash;
    private string _resultadoFirma;
    private string _resultadoIntegridad;
    private string _resultadoAutenticidad;
    private string _resultadoModo;
    private bool _hayResultadoPendiente;

    void Start()
    {
        _listener = new TcpListener(IPAddress.Any, puerto);
        _listener.Start();
        _hilo = new Thread(Escuchar) { IsBackground = true };
        _hilo.Start();
        Debug.Log($"[NetworkReceiver] Escuchando en puerto {puerto}");
    }

    void Update()
    {
        if (!_hayResultadoPendiente) return;

        // Actualizar UI en el hilo principal
        if (labelMensaje     != null) labelMensaje.text     = _mensajeRecibido;
        if (labelIntegridad  != null) labelIntegridad.text  = _resultadoIntegridad;
        if (labelAutenticidad != null) labelAutenticidad.text = _resultadoAutenticidad;
        if (labelModo        != null) labelModo.text        = _resultadoModo;

        Debug.Log($"[RECEPTOR]\n{_mensajeRecibido}\n{_resultadoHash}\n{_resultadoFirma}\n{_resultadoIntegridad}\n{_resultadoAutenticidad}\n{_resultadoModo}");
        _hayResultadoPendiente = false;
    }

    private void Escuchar()
    {
        while (true)
        {
            try
            {
                using var client = _listener.AcceptTcpClient();
                using var stream = client.GetStream();
                var buffer = new byte[65536];
                int leidos = stream.Read(buffer, 0, buffer.Length);
                string json = Encoding.UTF8.GetString(buffer, 0, leidos);

                var paquete = JsonUtility.FromJson<Paquete>(json);
                ProcesarPaquete(paquete);
            }
            catch (SocketException)
            {
                break; // Listener cerrado
            }
        }
    }

    private void ProcesarPaquete(Paquete p)
    {
        // Instanciar el adaptador correcto según lo que indica el paquete
        ICryptoAdapter crypto = p.usarLibrerias
            ? (ICryptoAdapter)new LibraryCryptoAdapter()
            : new CustomCryptoAdapter(p.claveCustom);

        // Recalcular hash del mensaje recibido
        string hashRecalculado = crypto.Hash(p.mensaje);

        // Verificar firma con la llave pública recibida
        bool firmaOk      = crypto.Verify(p.hash, p.firma, p.llavePublica);
        bool integridadOk = hashRecalculado == p.hash;

        string modo = p.usarLibrerias ? "Librerías (SHA-256 + RSA)" : "Custom";

        // Preparar resultados para el hilo principal
        _mensajeRecibido      = $"Mensaje: {p.mensaje}";
        _resultadoHash        = $"Hash: {p.hash}";
        _resultadoFirma       = $"Firma: {p.firma}";
        _resultadoIntegridad  = $"Integridad: {(integridadOk ? "OK" : "FALLO")}";
        _resultadoAutenticidad = $"Autenticidad: {(firmaOk ? "OK" : "FALLO")}";
        _resultadoModo        = $"Modo: {modo}";
        _hayResultadoPendiente = true;
    }

    void OnDestroy()
    {
        _listener?.Stop();
    }
}