using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace scoi.Controllers
{
    public class FurController : Controller
    {
        static List<double> X = new List<double>();


        private string tok = string.Empty;

        [HttpPost]
        public string addX(double x)
        {
            return "";
        }

        public ViewResult Matrix()
        {
            ViewBag.hash = this.GetHashCode();
            return View();
        }
    }
}
