using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace scoi.Models
{
    public class ImageFormModel
    {
        [JsonIgnore]
        public IFormFile file { set; get; }
        
        public string matr_str { set; get; }
    }

    public class MedianFilterModel
    {
        [JsonIgnore]
        public IFormFile file { set; get; }
        public int wnd_size { set; get; }
    }

    public class BinaryModel
    {
        [JsonIgnore]
        public IFormFile file { set; get; }
        public int wndSize { set; get; }
        public float sens { set; get; }
        public int sav_wndSize { set; get; }
        public float sav_sens { set; get; }
        public int bred_wndSize { set; get; }
        public float bred_sens { set; get; }
        public int wolf_wndSize { set; get; }
        public float wolf_sens { set; get; }
    }

    public class FurierModel
    {
        [JsonIgnore]
        public IFormFile file { set; get; }
        public int filter_type {set;get;}
        public string filter { set; get; }
        public double inFilter { set; get; }
        public double outFilter { set; get; }
        public double fur_mult { set; get; }


    }
}
