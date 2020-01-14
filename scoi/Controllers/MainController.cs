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

        public ViewResult Index()
        {
            return View();
        }

        public ViewResult Fur()
        {
            return View("fur");
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
               
                var newimg = ImageOperations.MatrixFilter(jt, img, data.matr_str);
                newimg.Save(_hostingEnvironment.WebRootPath + outputName);

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

            var newFn = Path.GetRandomFileName() + Path.GetRandomFileName();
            var outputName = "\\Files\\" + newFn + ".jpg"; //+extension;
            jt.result_file = outputName;

            jt.action = () =>
            {

                var (img1, img2) = ImageOperations.Furier(jt,img);
                img1.Save(_hostingEnvironment.WebRootPath + outputName);

                jt.progress = 100;
            };

            id = dictionary.setTask(jt);
            return id.ToString() + ':' + outputName;
        }

        [HttpPost]
        public int getOpStatus(uint id)
        {
            return dictionary.tasks[id].progress;
        }

        [HttpPost]
        public double getOpTime(uint id)
        {
            var jt = dictionary.tasks[id];
            jt.contextTask.Wait();
            var ts = jt.endTime - jt.startTime;
            return ts.TotalMilliseconds;
        }

    }
}
