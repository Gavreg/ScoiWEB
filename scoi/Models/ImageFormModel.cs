using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;


namespace scoi.Models
{
    public class ImageFormModel
    {
        public IFormFile file { set; get; }
        public string matr_str { set; get; }
    }

    public class BinaryModel
    {
        public IFormFile file { set; get; }
        public int wndSize { set; get; }
        public float sens { set; get; }
        public int sav_wndSize { set; get; }
        public float sav_sens { set; get; }
        public int bred_wndSize { set; get; }
        public float bred_sens { set; get; }
    }

    public class FurierModel
    {
        public IFormFile file { set; get; }
        public string filter { set; get; }
        public double inFilter { set; get; }
        public double outFilter { set; get; }
    }
}
