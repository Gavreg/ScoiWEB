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
            using (Graphics g = Graphics.FromImage(_tmp))
            {
                g.DrawImageUnscaled(input, 0, 0);
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

            return new_bitmap;
        }

        public static (Bitmap, Bitmap) Furier(JobTask job,Bitmap input, string filter, double in_filter_zone=1.0, double out_filter_zone=0.0 )
        {
            int width = input.Width;
            int height = input.Height;
            using Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            byte[] old_bytes = new byte[width * height * 3];
            byte[] new_bytes = new byte[width * height * 3];
            byte[] furier_ma_bytes = new byte[width * height * 3];
            byte[] furier_im_bytes = new byte[width * height * 3];

            double[] spec_im = new double[width * height];
            double[] spec_re = new double[width * height];
            double[] spec_ma = new double[width * height];



            using Graphics g = Graphics.FromImage(_tmp);
            g.DrawImageUnscaled(input, 0, 0);
            

            var _tmp_data = _tmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, _tmp.PixelFormat);
            Marshal.Copy(_tmp_data.Scan0, old_bytes, 0, old_bytes.Length);
            _tmp.UnlockBits(_tmp_data);

            var ss = StringSplitOptions.RemoveEmptyEntries;
            var filter_params_strings = filter.Split("\n", ss);
            filter_params_strings = (from s in filter_params_strings where (s.Trim() != string.Empty) select s).ToArray();
            var cult = new CultureInfo("en-US");
            
            var filter_params_double = filter_params_strings.Select(a=> a.Split(";", ss)
                .Select(b=>Convert.ToDouble(b.Trim(),cult)).ToArray() ).ToArray();

            
            job.progress = 0;

            Complex[] complex_bytes = new Complex[width * height];
            for (int color = 0; color <= 2; color++)
            {
                
                for (int i = 0; i < width * height; ++i)
                {
                    int y = i / width;
                    int x = i - y * width;
                    complex_bytes[i] = Math.Pow(-1,x+y)*old_bytes[i * 3 + color];
                }


                complex_bytes = FFT.ditfft2d(complex_bytes, width, height);
                job.progress += 16;


                var max_ma = complex_bytes.Max(x => Math.Log(x.Magnitude+1.0,100));
                //var max_im = complex_bytes.Max(x => Math.Log10(x.Imaginary+1.0));

               /* var max_ma = Math.Log10(complex_bytes[1].Real + 1.0);
                for (int i = 2; i < width * height; ++i)
                {
                    if (max_ma <  Math.Log10(complex_bytes[i].Real + 1.0))
                        max_ma = Math.Log10(complex_bytes[i].Real + 1.0);
                }*/

                for (int i = 0; i < width * height; ++i)
                {
                    spec_re[i] += complex_bytes[i].Real * 1.0 / 3.0;
                    spec_im[i] += complex_bytes[i].Imaginary * 1.0 / 3.0;
                    spec_ma[i] += complex_bytes[i].Magnitude * 1.0 / 3.0;
                }

                var complex_bytes_filtered = complex_bytes.Select((a, i) =>
                {
                    int y = i / width;
                    int x = i - y * width;
                    foreach (var v in filter_params_double)
                    {
                        if ((x - v[0] * width) * (x - v[0] * width) + (y - v[1] * height) * (y - v[1] * height) > v[2] * v[2] * height * height &&
                            (x - v[0] * width) * (x - v[0] * width) + (y - v[1] * height) * (y - v[1] * height) <= v[3] * v[3] * height * height)
                            return a*in_filter_zone;
                    }
                    return a * out_filter_zone;
                }).ToArray();

                var complex_bytes_result = FFT.ditifft2d(complex_bytes_filtered, width, height);

                for (int i = 0; i < width * height; ++i)
                {
                    int y = i / width;
                    int x = i - y * width;
                    new_bytes[i * 3 + color] = clmp(Math.Round( (Math.Pow(-1,x+y) * complex_bytes_result[i]).Real));
                    furier_ma_bytes[i * 3 + color] = clmp(Math.Log(complex_bytes[i].Magnitude + 1.0, 100) * 255.0 / max_ma);
                    
                }
                job.progress += 16;
            }

            //Вывод коэф. преобразования в *.csv
            /*using StreamWriter sw = new StreamWriter("fur.csv");
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    sw.Write("\""+ spec_re[i*width+j].ToString() + "\",");
                }

                sw.Write("\n");
            }
            sw.Close();
           */

            Bitmap new_bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var new_bitmap_data = new_bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, new_bitmap.PixelFormat);
            Marshal.Copy(new_bytes, 0, new_bitmap_data.Scan0, new_bytes.Length);
            new_bitmap.UnlockBits(new_bitmap_data);

            Bitmap new_bitmap_re = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var new_bitmap_re_data = new_bitmap_re.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, new_bitmap.PixelFormat);
            Marshal.Copy(furier_ma_bytes, 0, new_bitmap_re_data.Scan0, furier_ma_bytes.Length);
            new_bitmap_re.UnlockBits(new_bitmap_re_data);

            return (new_bitmap, new_bitmap_re);

        }
    }


}
