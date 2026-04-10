using System;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;

public class FirmaDigital : MonoBehaviour
{
    void Start()
    {
        string mensaje = "Angely"; //Eleccion mensajr
        string mensajeModificado = "Angeli"; // Cambio una letra

        byte[] hashOriginal; //Creo hash
        using (SHA256 sha256 = SHA256.Create()) //El algortimo
        {
            hashOriginal = sha256.ComputeHash(Encoding.UTF8.GetBytes(mensaje));
        }

        Debug.Log("Mensaje original: " + mensaje);
        Debug.Log("Hash original: " + BitConverter.ToString(hashOriginal));

        byte[] hashModificado;
        using (SHA256 sha256 = SHA256.Create())
        {
            hashModificado = sha256.ComputeHash(Encoding.UTF8.GetBytes(mensajeModificado));
        }

        Debug.Log("Mensaje modificado: " + mensajeModificado);
        Debug.Log("Hash modificado: " + BitConverter.ToString(hashModificado));

        using (RSA rsa = RSA.Create()) //Firmar el hash con publica
        {
     
            byte[] firma = rsa.SignHash(hashOriginal, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            Debug.Log("Firma generada: " + BitConverter.ToString(firma));

            bool esValido = rsa.VerifyHash(hashOriginal, firma, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1); //Confirmar

            Debug.Log("¿Firma válida? " + esValido);
        }
    }
}