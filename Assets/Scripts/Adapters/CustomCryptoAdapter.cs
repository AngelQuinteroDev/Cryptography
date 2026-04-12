using System;
using System.Collections.Generic;
using System.Text;

public class CustomCryptoAdapter : ICryptoAdapter
{
    // ==========================================================
    // CONSTANTES
    // ==========================================================

    private const int    RONDAS  = 4;           // número de rondas
    private const double H_EULER = 0.25;         // paso de integración
    private const uint   MAGIC   = 0x9E3779B9u; // constante áurea

    // ==========================================================
    // DICCIONARIO DE SUSTITUCIÓN
    // Minúsculas + Mayúsculas + Dígitos
    // ==========================================================
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

        // Mayúsculas
        { 'A', "Xp92mN" }, { 'B', "rL4TwZ" }, { 'C', "Vn8KqP" },
        { 'D', "bJ6YsE" }, { 'E', "Oc1FhU" }, { 'F', "Zt5RdM" },
        { 'G', "Wy0GjI" }, { 'H', "Lk3NvA" }, { 'I', "Qe7BxS" },
        { 'J', "Hm2DcT" }, { 'K', "Fp6WuO" }, { 'L', "Cr9ZiV" },
        { 'M', "Ds4LpY" }, { 'N', "Gw1MtB" }, { 'O', "Ib8HnX" },
        { 'P', "Nj5EoR" }, { 'Q', "Ak7CwF" }, { 'R', "Pm3GyQ" },
        { 'S', "Tu6JkL" }, { 'T', "We4PoZ" }, { 'U', "Yc0FrN" },
        { 'V', "Sx9IbK" }, { 'W', "Rv2NuH" }, { 'X', "Qd8MaG" },
        { 'Y', "Pt1LwE" }, { 'Z', "Ob6KjD" },

        // Dígitos
        { '0', "n4Vx2Q" }, { '1', "h8Tz6W" }, { '2', "b3Rp0Y" },
        { '3', "f7Jm4S" }, { '4', "d1Hn8L" }, { '5', "j5Bq2K" },
        { '6', "l9Fw6O" }, { '7', "p0Cs4M" }, { '8', "r6Dk8N" },
        { '9', "t2Gv0P" },
    };

    // ==========================================================
    // TABLA DE PERMUTACIÓN FIJA (16 posiciones)
    // Lee como: el byte en posición i se mueve a posición _perm[i]
    // ==========================================================
    private static readonly int[] _perm =
        { 7, 12, 3, 14, 1, 10, 5, 0, 15, 6, 11, 2, 9, 4, 13, 8 };

    // ==========================================================
    // LLAVES
    // ==========================================================
    private readonly string _clavePrivada;
    private readonly string _clavePublica;

    public CustomCryptoAdapter(string clavePrivada)
    {
        _clavePrivada = clavePrivada;
        _clavePublica = DerivarPublica(clavePrivada);
    }

    // ==========================================================
    // HASH — pipeline completo
    // ==========================================================
    public string Hash(string mensaje)
    {
        // ── Paso 1: Sustitución con el diccionario ──────────────
        // Cada carácter del mensaje se reemplaza por su código.
        // "Ho" → "Lk3NvA" + "8rjr9w"
        string sustituido = Sustituir(mensaje);
        byte[] datos      = Encoding.UTF8.GetBytes(sustituido);

        // ── Paso 2: Permutación de bytes ────────────────────────
        // Los bytes se reordenan en bloques de 16 según _perm.
        // Rompe patrones posicionales antes de la mezcla.
        byte[] permutado = Permutar(datos);

        // ── Paso 3: Euler Hash con 4 rondas ─────────────────────
        // Los 8 acumuladores se mezclan R veces con los datos.
        // Cada ronda usa una constante distinta para diferenciarse.
        uint[] estado = EulerHashConRondas(permutado);

        // ── Paso 4: Formato IPv6 ─────────────────────────────────
        // Los 8 uint32 se comprimen a 8 grupos de 16 bits hex.
        // Salida: "a3f2:8b1c:0e47:d629:3af1:7c84:b2e0:591d"
        return FormatoIPv6(estado);
    }

    // ----------------------------------------------------------
    // PASO 1 — Sustitución
    // Cada carácter reconocido se reemplaza por su código de 6 chars.
    // Los caracteres no mapeados (espacios, puntuación) pasan tal cual.
    // ----------------------------------------------------------
    private static string Sustituir(string texto)
    {
        var sb = new StringBuilder();
        foreach (char c in texto)
            sb.Append(_dic.TryGetValue(c, out string cod) ? cod : c.ToString());
        return sb.ToString();
    }

    // ----------------------------------------------------------
    // PASO 2 — Permutación de bloques de 16 bytes
    //
    // Para cada bloque de 16 bytes:
    //   byte en posición i  →  va a posición _perm[i]
    //
    // Si el último bloque no tiene 16 bytes se rellena con 0xFF.
    // El 0xFF como padding es intencional — evita que dos mensajes
    // con distinto largo pero mismo contenido colisionen.
    // ----------------------------------------------------------
    private static byte[] Permutar(byte[] datos)
    {
        int    bloques = (datos.Length + 15) / 16;
        byte[] salida  = new byte[bloques * 16];

        for (int b = 0; b < bloques; b++)
        {
            // Extrae bloque de 16 bytes (con padding si es necesario)
            byte[] bloque = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                int idx  = b * 16 + i;
                bloque[i] = idx < datos.Length ? datos[idx] : (byte)0xFF;
            }

            // Aplica la permutación: mueve cada byte a su nueva posición
            byte[] permBloque = new byte[16];
            for (int i = 0; i < 16; i++)
                permBloque[_perm[i]] = bloque[i];

            Array.Copy(permBloque, 0, salida, b * 16, 16);
        }
        return salida;
    }

    // ----------------------------------------------------------
    // PASO 3 — Euler Hash con 4 rondas
    //
    // Estado inicial: 8 semillas fijas (mismas que SHA-256)
    // Por cada ronda:
    //   a) Calcula constante única de ronda
    //   b) Recorre todos los bytes aplicando el paso de Euler
    //   c) Permuta los acumuladores entre sí (difusión cruzada)
    //
    // Analogía con métodos numéricos:
    //   EDO:   y' = f(y,t)  →  y[n+1] = y[n] + h·f(y[n], t[n])
    //   Hash:  s[j] = s[j] + h·f(s[j], byte[i], constRonda)
    //   Cada ronda es un paso de integración que refina el estado.
    // ----------------------------------------------------------
    private static uint[] EulerHashConRondas(byte[] datos)
    {
        // Estado inicial — 8 semillas (fraccionarios de raíces cuadradas de primos)
        uint[] s = {
            0x6A09E667u, 0xBB67AE85u, 0x3C6EF372u, 0xA54FF53Au,
            0x510E527Fu, 0x9B05688Cu, 0x1F83D9ABu, 0x5BE0CD19u
        };

        // ── BUCLE DE RONDAS ──────────────────────────────────────
        for (int ronda = 0; ronda < RONDAS; ronda++)
        {
            // a) Constante única por ronda
            //    Ronda 0: MAGIC rotado  0 bits
            //    Ronda 1: MAGIC rotado  8 bits  (diferente)
            //    Ronda 2: MAGIC rotado 16 bits  (diferente)
            //    Ronda 3: MAGIC rotado 24 bits  (diferente)
            //    Sin esto dos rondas idénticas cancelarían su efecto.
            uint constRonda = RotarIzq(MAGIC * (uint)(ronda + 1), ronda * 8);

            // b) Recorre todos los bytes — paso de Euler por cada uno
            for (int i = 0; i < datos.Length; i++)
            {
                byte b   = datos[i];
                int  rot = (i % 31) + 1;   // rotación variable 1..31 bits

                for (int j = 0; j < 8; j++)
                {
                    // Función de mezcla f(s[j], b):
                    //   multiplica por MAGIC (dispersión),
                    //   XOR con el byte desplazado (introduce el dato),
                    //   XOR con constRonda (diferencia la ronda),
                    //   XOR con j*0x1B (diferencia cada acumulador)
                    uint f = (s[j] * MAGIC)
                           ^ ((uint)b << (rot % 24))
                           ^ constRonda
                           ^ (uint)(j * 0x1B);

                    // Paso de Euler:  delta = h · f
                    uint delta = (uint)(H_EULER * f);

                    // Actualiza acumulador j:
                    //   suma el delta (integración),
                    //   rota (permutación de bits),
                    //   XOR con f (irreversibilidad)
                    s[j] = RotarIzq(s[j] + delta, rot % 32) ^ f;

                    // Difusión cruzada al acumulador siguiente:
                    // el acumulador j afecta al j+1 → efecto avalancha global
                    s[(j + 1) % 8] ^= RotarDer(s[j], 7);
                }
            }

            // c) Permutación de acumuladores entre rondas
            //    Cruza los acumuladores 0↔4, 1↔5, 2↔6, 3↔7
            //    con rotaciones asimétricas para evitar cancelación.
            //    Esto hace que en la siguiente ronda cada acumulador
            //    tenga información de todos los bytes anteriores.
            for (int j = 0; j < 4; j++)
            {
                uint temp  = s[j];
                s[j]       = s[j + 4] ^ RotarIzq(s[j],     13);
                s[j + 4]   = temp     ^ RotarDer(s[j + 4], 17);
            }
        }

        return s;
    }

    // ----------------------------------------------------------
    // PASO 4 — Formato IPv6
    //
    // Cada uint32 se "dobla" sobre sí mismo:
    //   grupos[i] = parte alta XOR parte baja del acumulador i
    //   = los 16 bits altos mezclados con los 16 bits bajos
    //
    // Resultado: "xxxx:xxxx:xxxx:xxxx:xxxx:xxxx:xxxx:xxxx"
    // ----------------------------------------------------------
    private static string FormatoIPv6(uint[] estado)
    {
        ushort[] grupos = new ushort[8];
        for (int i = 0; i < 8; i++)
            grupos[i] = (ushort)(estado[i] ^ (estado[i] >> 16));

        return string.Join(":",
            Array.ConvertAll(grupos, g => g.ToString("x4")));
    }

    // ==========================================================
    // FIRMA — XOR + rotación de bits con clave privada
    // ==========================================================
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

    // ==========================================================
    // VERIFICACIÓN — invierte la firma y compara con el hash
    // ==========================================================
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

    // ==========================================================
    // HELPERS
    // ==========================================================
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