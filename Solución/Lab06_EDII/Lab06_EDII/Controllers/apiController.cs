using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BibliotecaDeClases.RSA;
using Lab06_EDII.Models;
using System.IO.Compression;
using System.Net.Mime;

namespace Lab06_EDII.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class apiController : ControllerBase
    {
        RSA rSA = new RSA();
        Data data = new Data();

        /// <summary>
        /// Metodo para generar llaves
        /// </summary>
        /// <param name="p">Numero primo, se validara si este es un valor primo</param>
        /// <param name="q">Numero primo, se validara si este es un valor primo</param>
        /// <returns>Retornara un archivo "Keys.zip", donde contendra las llaves generadas (public.key, private.key)</returns>
        [HttpGet("rsa/{p}/{q}")] //generar las llaves
        public async Task<ActionResult> Get(int p, int q)
        {
            ExistFiles();
            CreateDirectory();
            var ValorP = data.ValidacionPrimo(p, 2);
            var ValorQ = data.ValidacionPrimo(q, 2);
            if (ValorP == true && ValorQ == true)
            {
                rSA.GenerarLlaves(p, q);
            }
            else if ((ValorP != true && ValorQ != true) || (ValorP != true && ValorQ == true) || (ValorP == true && ValorQ != true))
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return await DownloadZip();
        }
        /// <summary>
        /// Metodo para Cifrar o Decifrar, Dependiendo de la llave que se le ingrese
        /// </summary>
        /// <param name="nombre">Nuevo nombre del archivo a cifrar</param>
        /// <param name="file">Archivo a cifrar</param>
        /// <param name="key">llave generada por el sistema (public.key | private.key)</param>
        /// <returns>Archivo Cifrado o Decifrado con extension (.rsa)</returns>
        [HttpPost("rsa/{nombre}")]
        public async Task<ActionResult> Post(string nombre, IFormFile file, IFormFile key) //Si el metodo devuelve letras chinas existe un problema
        {
            ExistFiles();
            CreateDirectory();
            ArchivoARuta(file);
            ArchivoARuta(key);
            var TipoKey = Path.GetFileNameWithoutExtension(key.FileName);
            var NombreArchivo = Path.GetFileNameWithoutExtension(file.FileName);
            var RutaFile = Environment.CurrentDirectory + "\\temp\\" + file.FileName;
            var RutaKey = Environment.CurrentDirectory + "\\temp\\" + key.FileName;
            if (NombreArchivo != nombre) //Validacion no permite tener archivo de entrada con mismo nombre que el de salida.
            {
                if (TipoKey == "public")
                {
                    rSA.RSACifrado(RutaFile, RutaKey, nombre);
                }
                else if (TipoKey == "private")
                {
                    rSA.RSADecifrado(RutaFile, RutaKey, nombre);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                } 
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var path = Environment.CurrentDirectory + "\\temp\\" + nombre + ".rsa";
            var Memory = new MemoryStream();
            using (var Stream = new FileStream(path, FileMode.Open))
            {
                await Stream.CopyToAsync(Memory);
            }
            Memory.Position = 0;
            var extensionFile = Path.GetExtension(path).ToLowerInvariant();
            DeleteDirectory();
            return File(Memory, GetMimeTypes()[extensionFile], Path.GetFileName(path));
        }

        /// <summary>
        /// Crea el directorio temporal
        /// </summary>
        private void CreateDirectory()
        {
            Directory.CreateDirectory($"temp");
        }
        /// <summary>
        /// Borra el directorio temporal
        /// </summary>
        private void DeleteDirectory()
        {
            Directory.Delete($"temp", true);
        }
        private void DeleteFile()
        {
            System.IO.File.Delete(Environment.CurrentDirectory + "\\Keys.zip");
        }/// <summary>
         /// Metodo para retornar el archivo zip, introducir dentro del zip los archivos dentro de temp.
         /// </summary>
         /// <returns></returns>
        async Task<FileStreamResult> DownloadZip()
        {
            var RutaZip = Path.Combine(Environment.CurrentDirectory, "Keys.zip");
            var RutaTemp = Path.Combine(Environment.CurrentDirectory, "temp");

            if (System.IO.File.Exists(RutaZip))
            {
                System.IO.File.Delete(RutaZip);
            }

            ZipFile.CreateFromDirectory(RutaTemp, RutaZip);

            return await Download(RutaZip);
        }
        /// <summary>
        /// Metodo para descargar
        /// </summary>
        /// <param name="path">Ruta del archivo .Zip</param>
        /// <returns>Archivo zip</returns>
        async Task<FileStreamResult> Download(string path)
        {
            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            DeleteDirectory();
            DeleteFile();
            return File(memory, MediaTypeNames.Application.Octet, Path.GetFileName(path));
        }
        public static void ArchivoARuta(IFormFile Archivo)
        {
            using (var reader = new BinaryReader(Archivo.OpenReadStream()))
            {
                using (var st = new FileStream($"temp\\{Archivo.FileName}", FileMode.OpenOrCreate))
                {
                    using (var w = new BinaryWriter(st))
                    {
                        var bl = 10000;
                        var bf = new byte[bl];
                        bf = reader.ReadBytes(bl);
                        foreach (var car in bf)
                        {
                            w.Write(car);
                        }
                    }
                    reader.Close();
                }
            }
        }
        /// <summary>
        /// Metodo donde crea el diccionario con las diferentes extension, y con tipo de contenido de la extension
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string> {
                {".txt","text/plain"},
                {".rsa","text/plain"},
            };
        }

        private void ExistFiles() {
            if (System.IO.File.Exists(Environment.CurrentDirectory + "\\Keys.zip"))
            {
                System.IO.File.Delete(Environment.CurrentDirectory + "\\Keys.zip");
            }

            if (Directory.Exists($"temp"))
            {
                Directory.Delete($"temp",true);
            }
            
        }
    }
}