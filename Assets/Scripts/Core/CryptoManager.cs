using TMPro;
using UnityEngine;

public class CryptoManager : MonoBehaviour
{
    [Header("UI - Entrada")]
    public TMP_InputField campoMensaje;
    public TMP_InputField campoClaveCustom;

    [Header("UI - Salida (debug)")]
    public TextMeshProUGUI labelHash;
    public TextMeshProUGUI labelFirma;
    public TextMeshProUGUI labelLlave;
    public TextMeshProUGUI labelModo;

    [Header("Modo")]
    public bool usarLibrerias = true;

    public string claveCustom =>
        campoClaveCustom != null && !string.IsNullOrEmpty(campoClaveCustom.text)
            ? campoClaveCustom.text
            : "gato";

    private ICryptoAdapter _adapter;

    void Awake() => InicializarAdapter();

    public void InicializarAdapter()
    {
        _adapter = usarLibrerias
            ? (ICryptoAdapter)new LibraryCryptoAdapter()
            : new CustomCryptoAdapter(claveCustom);

        Debug.Log($"[CryptoManager] Adaptador activo: {_adapter.GetType().Name}");

        if (labelModo != null)
            labelModo.text = usarLibrerias ? "Modo: Librerías" : "Modo: Custom";
    }

    public void CambiarModo(bool libreria)
    {
        usarLibrerias = libreria;
        InicializarAdapter();
    }

    public (string hash, string firma, string llavePublica) ObtenerPaquete()
    {
        string mensaje = campoMensaje != null ? campoMensaje.text : "";

        if (string.IsNullOrEmpty(mensaje))
        {
            Debug.LogWarning("[CryptoManager] Mensaje vacío.");
            return ("", "", "");
        }

        if (!usarLibrerias) InicializarAdapter();

        string hash  = _adapter.Hash(mensaje);
        string firma = _adapter.Sign(hash);
        string llave = _adapter.ExportarLlavePublica();

        if (labelHash  != null) labelHash.text  = "Hash:\n"  + Cortar(hash,  56);
        if (labelFirma != null) labelFirma.text = "Firma:\n" + Cortar(firma, 56);
        if (labelLlave != null) labelLlave.text = "Llave:\n" + Cortar(llave, 56);

        return (hash, firma, llave);
    }


    public static ICryptoAdapter CrearAdapter(bool usarLibs, string clave = "gato")
    {
        return usarLibs
            ? (ICryptoAdapter)new LibraryCryptoAdapter()
            : new CustomCryptoAdapter(clave);
    }

    private static string Cortar(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}
