public interface ICryptoAdapter
{
    string Hash(string mensaje);
    string Sign(string hashHex);
    bool Verify(string hashHex, string firmaBase64, string llavePublicaXml);
    string ExportarLlavePublica();
}