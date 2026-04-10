using TMPro;
using UnityEngine;

public class CryptoManager : MonoBehaviour
{
    [Header("UI - Entrada")]
    public TMP_InputField campoMensaje;

    [Header("UI - Salida (opcional, para debug)")]
    public TextMeshProUGUI labelHash;
    public TextMeshProUGUI labelFirma;
    public TextMeshProUGUI labelLlave;

    [Header("Modo")]
    public bool usarLibrerias = true;   // Cambia con el Toggle de la UI
    public string claveCustom = "gato"; // Solo para CustomCryptoAdapter

    private ICryptoAdapter _adapter;

    void Awake() => InicializarAdapter();

    // Instancia el adaptador correcto según el modo
    public void InicializarAdapter()
    {
        _adapter = usarLibrerias
            ? (ICryptoAdapter)new LibraryCryptoAdapter()
            : new CustomCryptoAdapter(claveCustom);

        Debug.Log($"[CryptoManager] Adaptador activo: {_adapter.GetType().Name}");
    }

    // Llamado por el Toggle de la UI (arrastra este método a OnValueChanged)
    public void CambiarModo(bool libreria)
    {
        usarLibrerias = libreria;
        InicializarAdapter();
    }

    // Llamado por NetworkSender.Enviar() para obtener hash, firma y llave pública
    public (string hash, string firma, string llavePublica) ObtenerPaquete()
    {
        string mensaje = campoMensaje != null ? campoMensaje.text : "";

        if (string.IsNullOrEmpty(mensaje))
        {
            Debug.LogWarning("[CryptoManager] Mensaje vacío.");
            return ("", "", "");
        }

        string hash  = _adapter.Hash(mensaje);
        string firma = _adapter.Sign(hash);
        string llave = _adapter.ExportarLlavePublica();

        // Muestra valores en pantalla (truncados para que quepan)
        if (labelHash  != null) labelHash.text  = "Hash:\n"  + Cortar(hash,  56);
        if (labelFirma != null) labelFirma.text = "Firma:\n" + Cortar(firma, 56);
        if (labelLlave != null) labelLlave.text = "Llave:\n" + Cortar(llave, 56);

        return (hash, firma, llave);
    }

    private static string Cortar(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}
