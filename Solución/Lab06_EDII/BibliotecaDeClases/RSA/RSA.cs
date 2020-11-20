using System;
using System.IO;
using System.Numerics;

namespace BibliotecaDeClases.RSA
{
    public class RSA
    {
        const int length = 1000;
        public static int e;

        public int MCD(int a, int b)
        {
            int restante;
            do
            {
                restante = b;
                b = a % b;
                a = restante;
            }
            while (b != 0);
            return restante;
        }

        public void GenerarLlaves(int ValorP, int ValorQ){
            var p = ValorP;
            var q = ValorQ;
            var n = p * q;
            var QN = (p - 1) * (q - 1);

            int CountA = 0; 
            int CountB = 0;

            for (var x = 2; x < QN; x++)
            {
                CountA = MCD(x, n);
                CountB = MCD(x, QN);
                if ((CountA == 1) && (CountB == 1))
                {
                    e = x;
                    break;
                }
            }

            var Temp = 0;
            int d = 2; // Encontrar el valor de D
            do
            {
                d++;
                Temp = (d * e) % QN;
            } 
            while (Temp != 1);
            var RutaOrigen = Environment.CurrentDirectory + "\\temp";
            
            using (var Ws = new FileStream(RutaOrigen + "/" + "private.Key", FileMode.OpenOrCreate))//Escribiendo llave privada
            {
                using (var Writer = new StreamWriter(Ws))
                {
                    Writer.Write(n.ToString() + "," + d.ToString());
                }
                Ws.Close();
            }

            using (var Ws2 = new FileStream(RutaOrigen + "/" + "public.Key", FileMode.OpenOrCreate))//Escribiendo llave privada
            {
                using (var Writer2 = new StreamWriter(Ws2))
                {
                    Writer2.Write(n.ToString() + "," + e.ToString());
                }
                Ws2.Close();
            }
        }

        public void RSACifrado(string RutaArchivo, string RutaLlave, string NuevoNombre){
            StreamReader Lector = new StreamReader(RutaLlave);
            var e = 0;
            var n = 0;
            while (!Lector.EndOfStream)
            {
                var Linea = Lector.ReadLine();
                var Valores = Linea.Split(Convert.ToChar(","));
                n = Convert.ToInt32(Valores[0]);
                e = Convert.ToInt32(Valores[1]);
            }
            Lector.Close();
            var RutaOrigen = Environment.CurrentDirectory + "\\temp"; 
            int size = Convert.ToInt32(Math.Ceiling(Math.Log(n, 256)));
            var RutaArchCifrado = Path.Combine(RutaOrigen, NuevoNombre + ".rsa");
           
            using (var Fstream = new FileStream(RutaArchivo, FileMode.Open))
            {
                using (var Reader = new BinaryReader(Fstream))
                {
                    using (var Wstream = new FileStream(RutaArchCifrado, FileMode.OpenOrCreate))
                    {
                        using (var Writer = new BinaryWriter(Wstream))
                        {
                            var bytes = new byte[length];
                            while (Reader.BaseStream.Position != Reader.BaseStream.Length)
                            {
                                bytes = Reader.ReadBytes(length);
                                foreach (var item in bytes)
                                {
                                    BigInteger DataCifrada = BigInteger.ModPow(item, Convert.ToInt32(e), n);
                                    string BinarioCifrado = Convert.ToString((int)(DataCifrada), 2);
                                    string textoCifrado = BinarioCifrado.PadLeft(size * 8, '0');
                                    while (textoCifrado.Length != 0)
                                    {
                                        Writer.Write(Convert.ToByte(textoCifrado.Substring(0, 8), 2));
                                        textoCifrado = textoCifrado.Remove(0, 8);
                                    }
                                }
                            }
                            Writer.Close();
                        }
                        Wstream.Close();
                    }
                    Reader.Close();
                }
                Fstream.Close();
            }
        }
        
        public void RSADecifrado(string RutaArchivo, string RutaLlave, string NuevoNombre){
            StreamReader Lector = new StreamReader(RutaLlave);
            var d = 0;
            var n = 0;
            while (!Lector.EndOfStream)
            {
                var Linea = Lector.ReadLine();
                var Valores = Linea.Split(Convert.ToChar(","));
                n = Convert.ToInt32(Valores[0]);
                d = Convert.ToInt32(Valores[1]);
            }
            Lector.Close();
            var RutaOrigen = Environment.CurrentDirectory + "\\temp";
            int size = Convert.ToInt32(Math.Ceiling(Math.Log(n, 256)));
            var RutaArchCifrado = Path.Combine(RutaOrigen, NuevoNombre + ".rsa");
            
            using (var Fstream = new FileStream(RutaArchivo, FileMode.Open))
            {
                using (var Reader = new BinaryReader(Fstream))
                {
                    using (var Wstream = new FileStream(RutaArchCifrado, FileMode.OpenOrCreate))
                    {
                        using (var Writer = new BinaryWriter(Wstream))
                        {
                            var bytes = new byte[length];
                            while (Reader.BaseStream.Position != Reader.BaseStream.Length)
                            {
                                bytes = Reader.ReadBytes(length * size);
                                int Contador = 1;
                                string TextoDecifrado = "";
                                foreach (var item in bytes)
                                {
                                    TextoDecifrado += Convert.ToString((int)(item), 2).PadLeft(8, '0');
                                    if (Contador % size == 0)
                                    {
                                        int BinarioCifrado = Convert.ToInt32(TextoDecifrado, 2);
                                        int DataDecifrada = ValorCifrado(BinarioCifrado, d, n);
                                        Writer.Write(Convert.ToByte(DataDecifrada));
                                        TextoDecifrado = "";
                                    }
                                    Contador++;
                                }
                            }Writer.Close();
                        }
                        Wstream.Close();
                    }
                    Reader.Close();
                }
                Fstream.Close();
            }
        }
        public int ValorCifrado(int ValorCifrado, int d, int n){
            var Valor = ValorCifrado % n;
            var Multiplicador = 1;
            for (var x = 0; x < d; x++)
            {
                Multiplicador = (Multiplicador * Valor) % n;
            }
            var ValorOriginal = Convert.ToInt32(Multiplicador);

            return ValorOriginal;
        }
    }
}