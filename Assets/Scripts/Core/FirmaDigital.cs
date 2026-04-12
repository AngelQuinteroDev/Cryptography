using UnityEngine;

public class FirmaDigital : MonoBehaviour
{
    [Header("Modo")]
    public bool usarLibrerias = true;
    public string claveCustom = "gato"; 

    void Start()
    {
        ICryptoAdapter crypto = CryptoManager.CrearAdapter(usarLibrerias, claveCustom);

        string mensaje          = "Angely";
        string mensajeModificado = "Angeli";

        string hashOriginal = crypto.Hash(mensaje);
        Debug.Log($"Mensaje original:   {mensaje}");
        Debug.Log($"Hash original:      {hashOriginal}");
        string hashModificado = crypto.Hash(mensajeModificado);
        Debug.Log($"Mensaje modificado: {mensajeModificado}");
        Debug.Log($"Hash modificado:    {hashModificado}");
        Debug.Log($"¿Hashes iguales?   {hashOriginal == hashModificado}");   // siempre false

        string firma = crypto.Sign(hashOriginal);
        Debug.Log($"Firma generada:     {firma[..40]}…");

        string llavePublica = crypto.ExportarLlavePublica();
        bool validoOriginal = crypto.Verify(hashOriginal, firma, llavePublica);
        Debug.Log($"¿Firma válida (original)?   {validoOriginal}");

        bool validoModificado = crypto.Verify(hashModificado, firma, llavePublica);
        Debug.Log($"¿Firma válida (modificado)? {validoModificado}");
    }
}
