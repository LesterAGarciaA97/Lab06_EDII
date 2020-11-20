using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public async Task<ActionResult> Get(int p, int q) {
            CreateDirectory();
            var ValorP = data.ValidacionPrimo(p, 2);
            var ValorQ = data.ValidacionPrimo(q, 2);
            if (ValorP == true && ValorQ == true)
            {
                rSA.GenerarLlaves(p,q);
            }
            else if ((ValorP != true && ValorQ != true)||(ValorP != true && ValorQ == true)||(ValorP == true && ValorQ != true))
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return await DownloadZip();
           
        }
        /// <summary>
        /// Crea el directorio temporal
        /// </summary>
        private void CreateDirectory() {
            Directory.CreateDirectory($"temp");
        }
        /// <summary>
        /// Borra el directorio temporal
        /// </summary>
        private void DeleteDirectory() {
            Directory.Delete($"temp", true);
        }
        private void DeleteFile() {
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
    }
}
