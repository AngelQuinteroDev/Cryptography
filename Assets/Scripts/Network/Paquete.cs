[System.Serializable]
public class Paquete
{
    public string mensaje;
    public string hash;
    public string firma;
    public string llavePublica;
    public bool usarLibrerias;
    public string claveCustom;   // solo relevante cuando usarLibrerias == false
}
