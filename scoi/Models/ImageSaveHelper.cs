using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Razor.Language;
using System.IO;

namespace scoi.Models
{
    public static class ImageSaveHelper
    {
         
        public static string getImageMD5(Bitmap bitmap)
        {
            var bytes = ImageOperations.getImgBytes(bitmap);
            MD5 md5Hash = MD5.Create();
            var hash = md5Hash.ComputeHash(bytes);
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sBuilder.Append(hash[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        
        public static string getOrCreateImageDir(string baseDir, Bitmap image)
        {
            using Bitmap new_img = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
            new_img.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using Graphics g = Graphics.FromImage(new_img);
            g.DrawImageUnscaled(image,0,0);
            string hash = getImageMD5(new_img);
            
            var dir_info = new DirectoryInfo($"{baseDir}\\{hash}");
            if (!dir_info.Exists)
            {
                Directory.CreateDirectory(dir_info.FullName);
                new_img.Save( $"{dir_info.FullName}\\original.png");
            }
            return dir_info.Name;

        }
    }
}
