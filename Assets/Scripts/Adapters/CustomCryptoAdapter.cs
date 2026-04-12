using System;
using System.Collections.Generic;
using System.Text;

public class CustomCryptoAdapter : ICryptoAdapter
{

    private const int    RONDAS  = 4;          
    private const double H_EULER = 0.25;        
    private const uint   MAGIC   = 0x9E3779B9u; 

    private static readonly Dictionary<char, string> _dic = new()
    {
        // Minúsculas
        { 'a', "9oiL7y" }, { 'b', "up34fe" }, { 'c', "f5tgvG" },
        { 'd', "Hoif98" }, { 'e', "Wud7g"  }, { 'f', "hf7e8O" },
        { 'g', "keI8dR" }, { 'h', "8DTEyw" }, { 'i', "p2Hf5d" },
        { 'j', "Gityjr" }, { 'k', "uf3RFk" }, { 'l', "ork4DL" },
        { 'm', "DE65pd" }, { 'n', "I8dhO2" }, { 'ñ', "S49igk" },
        { 'o', "8rjr9w" }, { 'p', "0fPDis" }, { 'q', "q8RJdd" },
        { 'r', "Tdu7jd" }, { 's', "jdjd5F" }, { 't', "84hdoQ" },
        { 'u', "hs7QSa" }, { 'v', "7hsYWj" }, { 'w', "569Asj" },
        { 'x', "Uhdy75" }, { 'y', "21Sha8" }, { 'z', "1LAuhd" },

        { 'A', "Xp92mN" }, { 'B', "rL4TwZ" }, { 'C', "Vn8KqP" },
        { 'D', "bJ6YsE" }, { 'E', "Oc1FhU" }, { 'F', "Zt5RdM" },
        { 'G', "Wy0GjI" }, { 'H', "Lk3NvA" }, { 'I', "Qe7BxS" },
        { 'J', "Hm2DcT" }, { 'K', "Fp6WuO" }, { 'L', "Cr9ZiV" },
        { 'M', "Ds4LpY" }, { 'N', "Gw1MtB" }, { 'O', "Ib8HnX" },
        { 'P', "Nj5EoR" }, { 'Q', "Ak7CwF" }, { 'R', "Pm3GyQ" },
        { 'S', "Tu6JkL" }, { 'T', "We4PoZ" }, { 'U', "Yc0FrN" },
        { 'V', "Sx9IbK" }, { 'W', "Rv2NuH" }, { 'X', "Qd8MaG" },
        { 'Y', "Pt1LwE" }, { 'Z', "Ob6KjD" },

        { '0', "n4Vx2Q" }, { '1', "h8Tz6W" }, { '2', "b3Rp0Y" },
        { '3', "f7Jm4S" }, { '4', "d1Hn8L" }, { '5', "j5Bq2K" },
        { '6', "l9Fw6O" }, { '7', "p0Cs4M" }, { '8', "r6Dk8N" },
        { '9', "t2Gv0P" },
    };


    private static readonly int[] _perm =
        { 7, 12, 3, 14, 1, 10, 5, 0, 15, 6, 11, 2, 9, 4, 13, 8 };


    private readonly string _clavePrivada;
    private readonly string _clavePublica;

    public CustomCryptoAdapter(string clavePrivada)
    {
        _clavePrivada = clavePrivada;
        _clavePublica = DerivarPublica(clavePrivada);
    }

    public string Hash(string mensaje)
    {
        string sustituido = Sustituir(mensaje);
        byte[] datos      = Encoding.UTF8.GetBytes(sustituido);

        byte[] permutado = Permutar(datos);

        uint[] estado = EulerHashConRondas(permutado);

        return FormatoIPv6(estado);
    }

    private static string Sustituir(string texto)
    {
        var sb = new StringBuilder();
        foreach (char c in texto)
            sb.Append(_dic.TryGetValue(c, out string cod) ? cod : c.ToString());
        return sb.ToString();
    }

    private static byte[] Permutar(byte[] datos)
    {
        int    bloques = (datos.Length + 15) / 16;
        byte[] salida  = new byte[bloques * 16];

        for (int b = 0; b < bloques; b++)
        {
            byte[] bloque = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                int idx  = b * 16 + i;
                bloque[i] = idx < datos.Length ? datos[idx] : (byte)0xFF;
            }

            byte[] permBloque = new byte[16];
            for (int i = 0; i < 16; i++)
                permBloque[_perm[i]] = bloque[i];

            Array.Copy(permBloque, 0, salida, b * 16, 16);
        }
        return salida;
    }


    private static uint[] EulerHashConRondas(byte[] datos)
    {
        uint[] s = {
            0x6A09E667u, 0xBB67AE85u, 0x3C6EF372u, 0xA54FF53Au,
            0x510E527Fu, 0x9B05688Cu, 0x1F83D9ABu, 0x5BE0CD19u
        };

        for (int ronda = 0; ronda < RONDAS; ronda++)
        {
            uint constRonda = RotarIzq(MAGIC * (uint)(ronda + 1), ronda * 8);

            for (int i = 0; i < datos.Length; i++)
            {
                byte b   = datos[i];
                int  rot = (i % 31) + 1;

                for (int j = 0; j < 8; j++)
                {
                    uint f = (s[j] * MAGIC)
                           ^ ((uint)b << (rot % 24))
                           ^ constRonda
                           ^ (uint)(j * 0x1B);

                    uint delta = (uint)(H_EULER * f);

                    s[j] = RotarIzq(s[j] + delta, rot % 32) ^ f;

                    s[(j + 1) % 8] ^= RotarDer(s[j], 7);
                }
            }

            for (int j = 0; j < 4; j++)
            {
                uint temp  = s[j];
                s[j]       = s[j + 4] ^ RotarIzq(s[j],     13);
                s[j + 4]   = temp     ^ RotarDer(s[j + 4], 17);
            }
        }

        return s;
    }

    private static string FormatoIPv6(uint[] estado)
    {
        ushort[] grupos = new ushort[8];
        for (int i = 0; i < 8; i++)
            grupos[i] = (ushort)(estado[i] ^ (estado[i] >> 16));

        return string.Join(":",
            Array.ConvertAll(grupos, g => g.ToString("x4")));
    }

    public string Sign(string hashHex)
    {
        byte[] hashBytes = Encoding.UTF8.GetBytes(hashHex);
        byte[] clave     = ExpandirClave(_clavePrivada, hashBytes.Length);
        byte[] firma     = new byte[hashBytes.Length];

        for (int i = 0; i < hashBytes.Length; i++)
        {
            byte mezclado = (byte)(hashBytes[i] ^ clave[i] ^ (byte)(i * 13));
            firma[i]      = RotarByte(mezclado, (i % 7) + 1);
        }
        return Convert.ToBase64String(firma);
    }

    public bool Verify(string hashHex, string firmaBase64, string llavePublicaRecibida)
    {
        if (llavePublicaRecibida != _clavePublica) return false;

        byte[] firmaBytes = Convert.FromBase64String(firmaBase64);
        byte[] clave      = ExpandirClave(_clavePrivada, firmaBytes.Length);
        byte[] hashBytes  = Encoding.UTF8.GetBytes(hashHex);

        if (firmaBytes.Length != hashBytes.Length) return false;

        byte[] recuperado = new byte[firmaBytes.Length];
        for (int i = 0; i < firmaBytes.Length; i++)
        {
            byte desrotado = RotarByteDer(firmaBytes[i], (i % 7) + 1);
            recuperado[i]  = (byte)(desrotado ^ clave[i] ^ (byte)(i * 13));
        }
        return Encoding.UTF8.GetString(recuperado) == hashHex;
    }

    public string ExportarLlavePublica() => _clavePublica;

    private static uint RotarIzq(uint v, int bits) =>
        bits == 0 ? v : (v << bits) | (v >> (32 - bits));

    private static uint RotarDer(uint v, int bits) =>
        bits == 0 ? v : (v >> bits) | (v << (32 - bits));

    private static byte RotarByte(byte v, int bits) =>
        (byte)((v << bits) | (v >> (8 - bits)));

    private static byte RotarByteDer(byte v, int bits) =>
        (byte)((v >> bits) | (v << (8 - bits)));

    private static byte[] ExpandirClave(string clave, int length)
    {
        byte[] cb  = Encoding.UTF8.GetBytes(clave);
        byte[] res = new byte[length];
        for (int i = 0; i < length; i++)
            res[i] = cb[i % cb.Length];
        return res;
    }

    private static string DerivarPublica(string privada)
    {
        byte[] b = Encoding.UTF8.GetBytes(privada);
        for (int i = 0; i < b.Length; i++) b[i] = (byte)~b[i];
        return Convert.ToBase64String(b);
    }
}