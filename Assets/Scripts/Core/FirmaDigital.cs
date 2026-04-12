using UnityEngine;

// Script de prueba rápida — corre en Start() sin necesidad de UI ni red.
// Usa el factory de CryptoManager para evitar duplicar lógica de instanciación.
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

        // --- Hash del mensaje original ---
        string hashOriginal = crypto.Hash(mensaje);
        Debug.Log($"Mensaje original:   {mensaje}");
        Debug.Log($"Hash original:      {hashOriginal}");

        // --- Hash del mensaje modificado (debe ser completamente distinto) ---
        string hashModificado = crypto.Hash(mensajeModificado);
        Debug.Log($"Mensaje modificado: {mensajeModificado}");
        Debug.Log($"Hash modificado:    {hashModificado}");
        Debug.Log($"¿Hashes iguales?   {hashOriginal == hashModificado}");   // siempre false

        // --- Firma del hash original ---
        string firma = crypto.Sign(hashOriginal);
        Debug.Log($"Firma generada:     {firma[..40]}…");

        // --- Verificación con el hash CORRECTO → debe ser true ---
        string llavePublica = crypto.ExportarLlavePublica();
        bool validoOriginal = crypto.Verify(hashOriginal, firma, llavePublica);
        Debug.Log($"¿Firma válida (original)?   {validoOriginal}");

        // --- Verificación con el hash MODIFICADO → debe ser false ---
        bool validoModificado = crypto.Verify(hashModificado, firma, llavePublica);
        Debug.Log($"¿Firma válida (modificado)? {validoModificado}");
    }
}
