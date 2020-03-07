using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using scoi.Models;
using System.IO;

namespace scoi.Controllers
{
    public class MainController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private static readonly TaskDictionary dictionary = new TaskDictionary();
        public MainController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public ViewResult Matrix()
        {
            return View();
        }

        public ViewResult Fur()
        {
            return View();
        }

        public ViewResult Index()
        {
            return View();
        }

        public ViewResult Binary()
        {
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

            var newFn = Path.GetRandomFileName() + Path.GetRandomFileName();
            var outputName = "\\Files\\" + newFn + ".jpg"; //+extension;
            jt.result_file = outputName;

            jt.action = () =>
            {
               
                using var newimg = ImageOperations.MatrixFilter(jt, img, data.matr_str);
                newimg.Save(_hostingEnvironment.WebRootPath + outputName);

                img.Dispose();
                jt.progress = 100;
            };
            
            id = dictionary.setTask(jt);
            return id.ToString() + ':' + outputName;
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

            var outputName1 = "\\Files\\" + Path.GetRandomFileName() + ".jpg"; //+extension;
            var outputName2 = "\\Files\\" + Path.GetRandomFileName() + ".jpg"; //+extension;

            //jt.result_file = outputName;

            jt.action = () =>
            {
                Bitmap img1=null, img2=null;
                try
                {
                    (img1, img2) = ImageOperations.Furier(jt, img, data.filter, data.inFilter, data.outFilter);
                    img1.Save(_hostingEnvironment.WebRootPath + outputName1);
                    img2.Save(_hostingEnvironment.WebRootPath + outputName2);
                }
                finally
                {
                    img1?.Dispose();
                    img2?.Dispose();
                    img.Dispose();
                }
                
                jt.progress = 100;
            };

            id = dictionary.setTask(jt);
            return id.ToString() + ':' + outputName1 + ':' + outputName2;
        }

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

            
            var outputName = "\\Files\\" + Path.GetRandomFileName() + ".jpg"; //+extension;
            var outputName1 = "\\Files\\" + Path.GetRandomFileName() + ".jpg"; //+extension;
            var outputName2 = "\\Files\\" + Path.GetRandomFileName() + ".jpg"; //+extension;

            jt.result_file = outputName;
            jt.action = () =>
            {
                using var new_img = ImageOperations.BinaryzationAvg(img);
                new_img.Save(_hostingEnvironment.WebRootPath + outputName);
                jt.progress = 33;

                using var new_img2 = ImageOperations.BinaryzationOtsu(img);
                new_img2.Save(_hostingEnvironment.WebRootPath + outputName1);
                jt.progress = 66;

                using var new_img3 = ImageOperations.BinarizationNiblack(img,data.wndSize,data.sens);
                new_img3.Save(_hostingEnvironment.WebRootPath + outputName2);

                img.Dispose();

                jt.progress = 100;
            };
            id = dictionary.setTask(jt);


            return id.ToString() + ':' + outputName + ':' + outputName1+ ":" + outputName2;
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
