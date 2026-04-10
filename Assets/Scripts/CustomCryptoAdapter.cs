using System;
using System.Text;

// Adaptador SIN librerías criptográficas externas — implementación propia (punto 2 del lab).
// Por ahora lanza NotImplementedException para que el proyecto compile mientras terminas la lógica.
// Reemplaza cada método con tu algoritmo de hash y cifrado manual.
public class CustomCryptoAdapter : ICryptoAdapter
{
    private readonly string _claveSecreta;

    public CustomCryptoAdapter(string claveSecreta)
    {
        _claveSecreta = claveSecreta;
    }

    public string Hash(string mensaje)
    {
        // TODO: implementar tu propio algoritmo de hash
        throw new NotImplementedException("Hash propio pendiente");
    }

    public string Sign(string hashHex)
    {
        // TODO: implementar tu propio cifrado con clave privada
        throw new NotImplementedException("Firma propia pendiente");
    }

    public bool Verify(string hashHex, string firmaBase64, string llavePublicaXml)
    {
        // TODO: implementar tu propia verificación
        throw new NotImplementedException("Verificación propia pendiente");
    }

    public string ExportarLlavePublica()
    {
        // TODO: devolver tu representación de llave pública
        throw new NotImplementedException("Exportar llave propia pendiente");
    }
}
