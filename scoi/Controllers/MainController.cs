using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using scoi.Models;
using System.IO;
using System.Text.Json;
using System.Text;

namespace scoi.Controllers
{
    public class MainController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private static readonly TaskDictionary dictionary = new TaskDictionary();
        private static string js_comon_version = "1";
        public MainController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-Us");
        }

        public ViewResult Matrix()
        {
            ViewBag.jsversion = js_comon_version;
            return View();
        }

        public ViewResult Fur()
        {
            ViewBag.jsversion = js_comon_version;
            return View();
        }

        public ViewResult Index()
        {
            ViewBag.jsversion = js_comon_version;
            return View();
        }

        public ViewResult Binary()
        {
            ViewBag.jsversion = js_comon_version;
            return View();
        }
        public ViewResult Median()
        {
            ViewBag.jsversion = js_comon_version;
            return View();
        }
        [HttpPost]
        public async Task<string> LoadImage(ImageFormModel data)
        {
            var size = data.file.Length;

            var jt = new JobTask();
            ulong id = 0;
            Bitmap img;

            using (var stream = new MemoryStream())
            {
                await data.file.CopyToAsync(stream);
                img = new Bitmap(stream);
            }

            var dir = $"\\Files\\Results\\Matrix\\{ImageSaveHelper.getOrCreateImageDir(_hostingEnvironment.WebRootPath + "\\Files\\Results\\Matrix", img)}";

            var outputName = Path.GetRandomFileName();
            
            jt.action = () =>
            {
                jt.operations_count = img.Height*img.Width;
                using var newimg = ImageOperations.MatrixFilter(jt, img, data.matr_str);
                
                newimg.Save($"{_hostingEnvironment.WebRootPath}\\{dir}\\{outputName}_result.jpg");
                
                string json = JsonSerializer.Serialize(data);
                FileStream fs = new FileStream($"{_hostingEnvironment.WebRootPath}\\{dir}\\{outputName}.txt", FileMode.OpenOrCreate);
                fs.Write(Encoding.ASCII.GetBytes(json));
                fs.Close();

                img.Dispose();
                jt.finalize();
            };
            id = dictionary.setTask(jt);

            return $"{id}:{dir}\\{outputName}_result.jpg";
            //return $"{id}:https://cdn.shopify.com/s/files/1/1061/1924/products/Smiling_Devil_Emoji_1024x1024.png?v=1571606036";
        }

        [HttpPost]
        public async Task<string> LoadImageFur(FurierModel data)
        {
            var size = data.file.Length;

            var jt = new JobTask();
            ulong id = 0;
            Bitmap img;

            using (var stream = new MemoryStream())
            {
                await data.file.CopyToAsync(stream);
                img = new Bitmap(stream);
            }



            var dir = $"\\Files\\Results\\Furier\\{ImageSaveHelper.getOrCreateImageDir(_hostingEnvironment.WebRootPath + "\\Files\\Results\\Furier", img)}";
            var random_name = Path.GetRandomFileName();

            var outputName1 = $"{dir}\\{random_name}_result.jpg"; //+extension;
            var outputName2 = $"{dir}\\{random_name}_furier.jpg"; ; //+extension;
            var outputName3 = $"{dir}\\{random_name}_mask.jpg"; ; //+extension;
            var paramName  =  $"{dir}\\{random_name}.txt"; ; //+extension;

            //jt.result_file = outputName;

            jt.action = () =>
            {
                Bitmap img1=null, img2=null, img3 = null;
                try
                {
                    (img1, img2, img3) = ImageOperations.Furier(jt, img, data.filter_type, data.filter, data.inFilter, data.outFilter,data.fur_mult);
                    img1.Save($"{_hostingEnvironment.WebRootPath}\\{outputName1}");
                    img2.Save($"{_hostingEnvironment.WebRootPath}\\{outputName2}");
                    img3.Save($"{_hostingEnvironment.WebRootPath}\\{outputName3}");

                    string json = JsonSerializer.Serialize(data);
                    FileStream fs = new FileStream($"{_hostingEnvironment.WebRootPath}\\{paramName}", FileMode.OpenOrCreate);
                    fs.Write(Encoding.ASCII.GetBytes(json));
                    fs.Close();

                }
                finally
                {
                    img1?.Dispose();
                    img2?.Dispose();
                    img.Dispose();
                }
                
                jt.finalize();
            };

            id = dictionary.setTask(jt);
            return id.ToString() + ':' + outputName1 + ':' + outputName2 + ':' + outputName3;
        }

        [HttpPost]
        public async Task<string> LoadImageBinary(BinaryModel data)
        {
            var size = data.file.Length;

            var jt = new JobTask();
            ulong id = 0;
            Bitmap img;

            using (var stream = new MemoryStream())
            {
                await data.file.CopyToAsync(stream);
                img = new Bitmap(stream);
            }

            var dir = $"\\Files\\Results\\Binarization\\{ImageSaveHelper.getOrCreateImageDir(_hostingEnvironment.WebRootPath + "\\Files\\Results\\Binarization", img)}";

            var random_name = Path.GetRandomFileName();

            var outputName =  "\\Files\\" + Path.GetRandomFileName() + ".tiff"; //+extension;
            var outputName1 = "\\Files\\" + Path.GetRandomFileName() + ".tiff"; //+extension;
            var outputName2 = "\\Files\\" + Path.GetRandomFileName() + ".tiff"; //+extension;
            var outputName3 = "\\Files\\" + Path.GetRandomFileName() + ".tiff"; //+extension;
            var outputName4 = "\\Files\\" + Path.GetRandomFileName() + ".tiff"; //+extension;
            var outputName5 = "\\Files\\" + Path.GetRandomFileName() + ".tiff"; //+extension;

            jt.result_file = outputName;
            jt.action = () =>
            {

                jt.operations_count = 6;

                using var new_img = ImageOperations.BinaryzationAvg(img);
                new_img.Save($"{_hostingEnvironment.WebRootPath}\\{dir}\\{random_name}_avg.tiff");


                jt.incrementProgress();

                using var new_img2 =  ImageOperations.BinaryzationOtsu(img);
                new_img2.Save($"{_hostingEnvironment.WebRootPath}\\{dir}\\{random_name}_otsu.tiff");

                jt.incrementProgress();

                using var new_img3 = ImageOperations.BinarizationNiblack(img, data.wndSize, data.sens);
                new_img3.Save($"{_hostingEnvironment.WebRootPath}\\{dir}\\{random_name}_niblack.tiff");
                jt.incrementProgress();

                using var new_img4 = ImageOperations.BinarizationSauval(img, data.sav_wndSize, data.sav_sens);
                new_img4.Save($"{_hostingEnvironment.WebRootPath}\\{dir}\\{random_name}_sauval.tiff");
                jt.incrementProgress();

                using var new_img5 = ImageOperations.BinarizationBredly(img, data.bred_wndSize, data.bred_sens);
                new_img5.Save($"{_hostingEnvironment.WebRootPath}\\{dir}\\{random_name}_bredly.tiff");
                jt.incrementProgress();

                using var new_img6 = ImageOperations.BinarizationWolf(img, data.wolf_wndSize, data.wolf_sens);
                new_img6.Save($"{_hostingEnvironment.WebRootPath}\\{dir}\\{random_name}_woolf.tiff");

                string json = JsonSerializer.Serialize(data);
                FileStream fs = new FileStream($"{_hostingEnvironment.WebRootPath}\\{dir}\\{random_name}.txt", FileMode.OpenOrCreate);
                fs.Write(Encoding.ASCII.GetBytes(json));
                fs.Close();

                jt.finalize();



                img.Dispose();

                
            };
            id = dictionary.setTask(jt);

            return id.ToString() + ':' +
                   $"{dir}\\{random_name}_avg.tiff" + ':' +
                   $"{dir}\\{random_name}_otsu.tiff" + ":" +
                   $"{dir}\\{random_name}_niblack.tiff" + ":" +
                   $"{dir}\\{random_name}_sauval.tiff" + ":" +
                   $"{dir}\\{random_name}_bredly.tiff" + ":" +
                   $"{dir}\\{random_name}_woolf.tiff";
        }

        [HttpPost]
        public async Task<string> LoadImageMedian(MedianFilterModel data)
        {
            var size = data.file.Length;

            var jt = new JobTask();
            ulong id = 0;
           
            using var stream = new MemoryStream();
            
            await data.file.CopyToAsync(stream);
            Bitmap img = new Bitmap(stream);

            var dir = $"\\Files\\Results\\Median\\{ImageSaveHelper.getOrCreateImageDir(_hostingEnvironment.WebRootPath + "\\Files\\Results\\Median", img)}";
            var random_name = Path.GetRandomFileName();

            var outputName = $"{dir}\\{random_name}_result.jpg"; //+extension;
            var paramName = $"{dir}\\{random_name}.txt"; ; //+extension;

            jt.result_file = outputName;
            jt.action = () =>
            {
                
                using var new_img = ImageOperations.Median(jt,img,data.wnd_size);
                new_img.Save($"{_hostingEnvironment.WebRootPath}\\{outputName}");
                img.Dispose();

                string json = JsonSerializer.Serialize(data);
                FileStream fs = new FileStream($"{_hostingEnvironment.WebRootPath}\\{paramName}", FileMode.OpenOrCreate);
                fs.Write(Encoding.ASCII.GetBytes(json));
                fs.Close();

                jt.finalize();

            };

            id = dictionary.setTask(jt);

            return id.ToString() + ':' + outputName;
        }

        [HttpPost]
        public int getOpStatus(uint id)
        {
            if (dictionary.tasks.ContainsKey(id))
                return dictionary.tasks[id].progress;
            return -1;
        }

        [HttpPost]
        public double getOpTime(uint id)
        {
            var jt = dictionary.tasks[id];
            jt.contextTask.Wait();
            var ts = jt.endTime - jt.startTime;
            return ts.TotalMilliseconds;
        }

        public string getTasks()
        {
            string ret = string.Empty;
            foreach (var jt  in dictionary.tasks)
            {
                ret += jt.Key + " " + jt.Value.startTime + jt.Value.endTime + jt.Value.progress + "\r\n";
            }

            return ret;
        }

    }
}
