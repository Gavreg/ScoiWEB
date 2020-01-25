using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Routing.Template;
using System.Globalization;
//using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;


namespace scoi.Models
{
    
    public static class ImageOperations
    {

        static byte[] getImgBytes(Bitmap img)
        {
            byte[] bytes = new byte[img.Width*img.Height*3];
            var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly,
                img.PixelFormat);
            Marshal.Copy(data.Scan0,bytes,0,bytes.Length);
            img.UnlockBits(data);
            return bytes;
        }

        static void writeImageBytes(Bitmap img, byte[] bytes)
        {
            var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly,
                img.PixelFormat);
            Marshal.Copy(bytes,0,data.Scan0,bytes.Length);

            img.UnlockBits(data);
        }
        
        static double[,]  getCoreFromStr(string matrix)
        {
            char[] splitter = { '\n' };
            matrix = matrix.Replace('\r', ' ');
            var str_list = matrix.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
            double[,] mat = new double[0, 0];


            for (int i = 0; i < str_list.Count(); ++i)
            {
                str_list[i] = str_list[i].Replace('\r', ' ');
                var chars = str_list[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (i == 0)
                {
                    mat = new double[str_list.Length, chars.Length];
                }

                for (int j = 0; j < chars.Length; ++j)
                {
                    var frac = Fraction.fromString(chars[j]);
                    mat[i, j] = frac.toDouble();
                }
            }

            return mat;

        }
        static byte clmp(double d)
        {
            if (d > 255)
                return 255;
            if (d < 0)
                return 0;
            return (byte)d;
        }
        public static Bitmap MatrixFilter(JobTask job, Bitmap input, string matrix)
        {
            job.progress = 0;

            int width = input.Width;
            int height = input.Height;


            Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution,input.VerticalResolution);
            using (Graphics g = Graphics.FromImage(_tmp))
            {
                g.DrawImageUnscaled(input, 0, 0);
                //g.DrawImage(input,0,0,new RectangleF(0,0,input.Width,input.Height),GraphicsUnit.Pixel);
            }
           

            byte[] old_bytes = new byte[width * height * 3];
            byte[] new_bytes = new byte[width * height * 3];

            var _tmp_data = _tmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, _tmp.PixelFormat);
            Marshal.Copy(_tmp_data.Scan0, old_bytes, 0, old_bytes.Length);
           
            _tmp.UnlockBits(_tmp_data);

            var core = getCoreFromStr(matrix);
            int M = core.GetLength(0);
            int N = core.GetLength(1);


            for (int _i = 0; _i < height; ++_i)
            {

                for (int _j = 0; _j < width; ++_j)
                {
                    double sum1 = 0;
                    double sum2 = 0;
                    double sum3 = 0;

                    for (int ii = 0; ii < M; ++ii)   // h - (i - h)     h - i + h = 2h-i
                    {
                        int i = _i + ii - M / 2;
                        if (i < 0)
                            i *= -1;
                        if (i >= height)
                            i = 2 * height - i - 1;

                        for (int jj = 0; jj < N; ++jj)
                        {
                            int j = _j + jj - N / 2;

                            if (j < 0)
                                j *= -1;

                            if (j >= width)
                                j = 2 * height - j - 1;

                            sum1 += old_bytes[width * i * 3 + j * 3 + 0] * core[ii, jj];
                            sum2 += old_bytes[width * i * 3 + j * 3 + 1] * core[ii, jj];
                            sum3 += old_bytes[width * i * 3 + j * 3 + 2] * core[ii, jj];
                        }
                    }
                    new_bytes[width * _i * 3 + _j * 3 + 0] = clmp(sum1);
                    new_bytes[width * _i * 3 + _j * 3 + 1] = clmp(sum2);
                    new_bytes[width * _i * 3 + _j * 3 + 2] = clmp(sum3);
                }
                job.progress = (int)Math.Floor(1.0 * _i / width * 100);
            }
            Bitmap new_bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var new_bitmap_data = new_bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, new_bitmap.PixelFormat);
            Marshal.Copy(new_bytes, 0, new_bitmap_data.Scan0, new_bytes.Length);
            new_bitmap.UnlockBits(new_bitmap_data);
            //writeImageBytes(new_bitmap,new_bytes);
            
            return new_bitmap;
        }

        public static (Bitmap, Bitmap) Furier(JobTask job,Bitmap input, string filter, double in_filter_zone=1.0, double out_filter_zone=0.0 )
        {
            int width = input.Width;
            int height = input.Height;
            
            int new_width = width;
            int new_height = height;
            
            var p = Math.Log2(width);
            if (p != Math.Floor(p))
                new_width = (int)Math.Pow(2, Math.Ceiling(p));
            p = Math.Log2(height);
            if (p != Math.Floor(p))
                new_height = (int)Math.Pow(2, Math.Ceiling(p));

            using Bitmap _tmp = new Bitmap(new_width, new_height, PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);

            byte[] new_bytes = new byte[new_width * new_height * 3];
            byte[] furier_ma_bytes = new byte[new_width * new_height * 3];
            byte[] furier_im_bytes = new byte[new_width * new_height * 3];

            double[] spec_im = new double[new_width * new_height];
            double[] spec_re = new double[new_width * new_height];
            double[] spec_ma = new double[new_width * new_height];



            using Graphics g = Graphics.FromImage(_tmp);
            g.DrawImageUnscaled(input, 0, 0);



            byte[] old_bytes = getImgBytes(_tmp);



            var ss = StringSplitOptions.RemoveEmptyEntries;
            var filter_params_strings = filter.Split("\n", ss);
            filter_params_strings = (from s in filter_params_strings where (s.Trim() != string.Empty) select s).ToArray();
            var cult = new CultureInfo("en-US");
            
            var filter_params_double = filter_params_strings.Select(a=> a.Split(";", ss)
                .Select(b=>Convert.ToDouble(b.Trim(),cult)).ToArray() ).ToArray();

            
            job.progress = 0;

            Complex[] complex_bytes = new Complex[new_width * new_height];
            for (int color = 0; color <= 2; color++)
            {
                
                for (int i = 0; i < new_width * new_height; ++i)
                {
                    int y = i / new_width;
                    int x = i - y * new_width;
                    complex_bytes[i] = Math.Pow(-1,x+y)*old_bytes[i * 3 + color];
                }


                complex_bytes = FFT.ditfft2d(complex_bytes, new_width, new_height);
                job.progress += 16;


                var max_ma = complex_bytes.Max(x => Math.Log(x.Magnitude+1.0,100));
                //var max_im = complex_bytes.Max(x => Math.Log10(x.Imaginary+1.0));

               /* var max_ma = Math.Log10(complex_bytes[1].Real + 1.0);
                for (int i = 2; i < new_width * height; ++i)
                {
                    if (max_ma <  Math.Log10(complex_bytes[i].Real + 1.0))
                        max_ma = Math.Log10(complex_bytes[i].Real + 1.0);
                }*/

                for (int i = 0; i < new_width * new_height; ++i)
                {
                    spec_re[i] += complex_bytes[i].Real * 1.0 / 3.0;
                    spec_im[i] += complex_bytes[i].Imaginary * 1.0 / 3.0;
                    spec_ma[i] += complex_bytes[i].Magnitude * 1.0 / 3.0;
                }

                var complex_bytes_filtered = complex_bytes.Select((a, i) =>
                {
                    int y = i / new_width;
                    int x = i - y * new_width;
                    foreach (var v in filter_params_double)
                    {
                        //if ((x - v[0] * new_width) * (x - v[0] * new_width) + (y - v[1] * new_height) * (y - v[1] * new_height) > v[2] * v[2] * new_height * new_height &&
                        //    (x - v[0] * new_width) * (x - v[0] * new_width) + (y - v[1] * new_height) * (y - v[1] * new_height) <= v[3] * v[3] * new_height * new_height)
                        if ((x - v[0] ) * (x - v[0] ) + (y - v[1] ) * (y - v[1] ) > v[2] * v[2]   &&
                            (x - v[0] ) * (x - v[0] ) + (y - v[1] ) * (y - v[1] ) <= v[3] * v[3])
                            return a*in_filter_zone;
                    }
                    return a * out_filter_zone;
                }).ToArray();

                var complex_bytes_result = FFT.ditifft2d(complex_bytes_filtered, new_width, new_height);

                for (int i = 0; i < new_width * new_height; ++i)
                {
                    int y = i / new_width;
                    int x = i - y * new_width;
                    new_bytes[i * 3 + color] = clmp(Math.Round( (Math.Pow(-1,x+y) * complex_bytes_result[i]).Real));
                    furier_ma_bytes[i * 3 + color] = clmp(Math.Log(complex_bytes[i].Magnitude + 1.0, 100) * 255.0 / max_ma);
                }
                job.progress += 16;
            }

            //Вывод коэф. преобразования в *.csv
            /*using StreamWriter sw = new StreamWriter("fur.csv");
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < new_width; ++j)
                {
                    sw.Write("\""+ spec_re[i*new_width+j].ToString() + "\",");
                }

                sw.Write("\n");
            }
            sw.Close();
           */

            using Bitmap new_bitmap = new Bitmap(new_width, new_height, PixelFormat.Format24bppRgb);

            writeImageBytes(new_bitmap,new_bytes);

            Bitmap new_bitmap_re = new Bitmap(new_width, new_height, PixelFormat.Format24bppRgb);
            writeImageBytes(new_bitmap_re, furier_ma_bytes);


            Bitmap new_bitamp_ret = new Bitmap(width,height, PixelFormat.Format24bppRgb);
            new_bitamp_ret.SetResolution(new_bitmap.HorizontalResolution, new_bitmap.VerticalResolution);
            using (Graphics g1 = Graphics.FromImage(new_bitamp_ret))
            {
                g1.DrawImageUnscaled(new_bitmap,0,0);
            }
            
            return (new_bitamp_ret, new_bitmap_re);

        }


        //https://ru.wikipedia.org/wiki/%D0%9C%D0%B5%D1%82%D0%BE%D0%B4_%D0%9E%D1%86%D1%83
        public static Bitmap Binaryzation(Bitmap input)
        {
            int width = input.Width;
            int height = input.Height;
            using Bitmap _tmp = new Bitmap(width,height,PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using var g = Graphics.FromImage(_tmp);
            g.DrawImageUnscaled(input,0,0);

            byte[] input_bytes = getImgBytes(_tmp);
            byte[] out_bytes = new byte[input_bytes.Length];
            
            for (int color = 0; color <= 2; ++color)
            {
                double[] N = new double[256];
                double [] sum_N = new double[256]; 
                double [] sum_u = new double[256];
                var Nt = 0.0;
                var max = 0;
                for (int i = 0; i < width * height; ++i)
                {
                    N[input_bytes[i * 3+color]] += 1.0 / width / height;
                    Nt += 1.0 / width / height;
                    if (input_bytes[i * 3 + color] > max)
                        max = input_bytes[i * 3 + color];
                }

                var sum = 0.0;
                var _sum_u=0.0;
                
                for (int i = 0; i <= max; ++i)
                {
                    sum += N[i];
                    _sum_u += i * N[i];
                    sum_N[i] = sum;
                    sum_u[i] = _sum_u;
                }

                double w1 = 0.0, w2 = 0.0, u1 = 0.0, u2 = 0.0;

                int final_t = 0;
                double sig_max = 0.0;
                for (int t = 1; t <= max; ++t)
                {
                    w1 = sum_N[t - 1];
                    w2 = 1.0 - w1;
                    u1 = sum_u[t - 1] / w1;
                    u2 = (sum_u[max] - u1 * w1) / w2;
                    var sig = w1 * w2 * (u1 - u2) * (u1 - u2);
                    if (sig > sig_max)
                    {
                        sig_max = sig;
                        final_t = t;
                    }
                }
                for (int i = 0; i < width * height; ++i)
                {

                    if (input_bytes[i * 3 + color] > final_t)
                        out_bytes[i * 3 + color] = 255;
                    else
                        out_bytes[i * 3 + color] = 0;
                }

            }

            Bitmap img_ret = new Bitmap(width,height,PixelFormat.Format24bppRgb);
            writeImageBytes(img_ret,out_bytes);
            return img_ret;

        }
    }


}
