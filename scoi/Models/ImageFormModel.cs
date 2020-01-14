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

    public class FurierModel
    {
        public IFormFile file { set; get; }
    }
}
