public interface ICryptoAdapter
{
    // Devuelve el hash del mensaje como string hex
    string Hash(string mensaje);

    // Firma el hash con la llave privada; devuelve la firma en Base64
    string Sign(string hashHex);

    // Verifica la firma usando la llave pública
    bool Verify(string hashHex, string firmaBase64, string llavePublicaXml);

    // Exporta la llave pública en XML para enviarla por red
    string ExportarLlavePublica();
}