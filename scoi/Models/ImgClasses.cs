using System;
using System.Collections;
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
using System.Threading;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static System.Environment;

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
            

            int  width = input.Width;
            int height = input.Height;


            Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution,input.VerticalResolution);
            using (Graphics g = Graphics.FromImage(_tmp))
            {
                g.DrawImageUnscaled(input, 0, 0);
                //g.DrawImage(input,0,0,new RectangleF(0,0,input.Width,input.Height),GraphicsUnit.Pixel);
            }
           

            byte[] old_bytes = getImgBytes(_tmp);
            byte[] new_bytes = new byte[width * height * 3];
        

            var core = getCoreFromStr(matrix);
            int M = core.GetLength(0);
            int N = core.GetLength(1);

            ParallelOptions opt = new ParallelOptions();
            if (ProcessorCount > 2)
                opt.MaxDegreeOfParallelism = ProcessorCount - 2;
            else opt.MaxDegreeOfParallelism = 1;
            Parallel.For(0, width*height, opt,arr_i =>
            {
                int _i = arr_i / width;
                int _j = arr_i - _i * width;

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
                            j = 2 * width - j - 1;

                        sum1 += old_bytes[width * i * 3 + j * 3 + 0] * core[ii, jj];
                        sum2 += old_bytes[width * i * 3 + j * 3 + 1] * core[ii, jj];
                        sum3 += old_bytes[width * i * 3 + j * 3 + 2] * core[ii, jj];
                    }
                }
                new_bytes[arr_i * 3 + 0] = clmp(sum1);
                new_bytes[arr_i * 3 + 1] = clmp(sum2);
                new_bytes[arr_i * 3 + 2] = clmp(sum3);

                job.incrementProgress();

            });

            Bitmap new_bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            new_bitmap.SetResolution(input.HorizontalResolution,input.VerticalResolution);
            writeImageBytes(new_bitmap,new_bytes);
     
            return new_bitmap;
        }

        //быстрая функция для нахождения медианы
        private static (byte,int) quickselect((byte, int)[] arr, int k)
        {
            if (arr.Length == 1)
                return arr[0];

            Random r = new Random();
            int pivot = r.Next(arr.Length);
            //int pivot = 0;

            var lows = arr.Where(x => x.Item1  < arr[pivot].Item1).ToArray();
            var high = arr.Where(x => x.Item1  > arr[pivot].Item1).ToArray();
            var eqv  = arr.Where(x => x.Item1 == arr[pivot].Item1).ToArray();

            if (k < lows.Length)
                return quickselect(lows, k);
            else if (k  < lows.Length + eqv.Length)
                return eqv[k - lows.Length];
            else
                return quickselect(high, k - lows.Length - eqv.Length);
            
        }

        private static int quickselect2(int[] arr, int left, int right, int k)
        {
            if (right - left == 1)
                return arr[left];

            int left_count = 0;
            int eqv_count = 0;
            int tmp = 0;

            for (int i = left; i < right - 1; ++i)
            {
                if (arr[i] < arr[right - 1])
                {
                    tmp = arr[i];
                    arr[i] = arr[left + left_count];
                    arr[left + left_count] = tmp;
                    left_count++;
                }
            }
            for (int i = left + left_count; i < right - 1; ++i)
            {
                if (arr[i] == arr[right - 1])
                {
                    tmp = arr[i];
                    arr[i] = arr[left + left_count + eqv_count];
                    arr[left + left_count + eqv_count] = tmp;
                    eqv_count++;
                }
            }
            tmp = arr[right - 1];
            arr[right - 1] = arr[left + left_count + eqv_count];
            arr[left + left_count + eqv_count] = tmp;


            if (k < left_count)
                return quickselect2(arr, left, left + left_count, k);
            else if (k < left_count + eqv_count)
                return arr[left + left_count];
            else
                return quickselect2(arr, left + left_count + eqv_count, right, k - left_count - eqv_count);

        }
        public static Bitmap Median(JobTask job, Bitmap input, int wnd_size)
        {

            job.progress = 0;


            int width = input.Width;
            int height = input.Height;


            Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using (Graphics g = Graphics.FromImage(_tmp))
            {
                g.DrawImageUnscaled(input, 0, 0);
                //g.DrawImage(input,0,0,new RectangleF(0,0,input.Width,input.Height),GraphicsUnit.Pixel);
            }

            byte[] old_bytes = getImgBytes(_tmp);
            byte[] new_bytes = new byte[width * height * 3];

            //массивчик для медианы

            Mutex mutex = new Mutex();
            int iter_count = old_bytes.Length / 3;

            job.operations_count = height;

            //for (int _i = 0; _i < height; ++_i)
            ParallelOptions opt = new ParallelOptions();
            if (ProcessorCount > 2)
                opt.MaxDegreeOfParallelism = ProcessorCount - 2;
            else opt.MaxDegreeOfParallelism = 1;

            //opt.MaxDegreeOfParallelism = 1;

            Parallel.For(0, height, opt, _i =>
            //for (int _i = 0; _i < height; ++_i)
            {

                var curPriority = Thread.CurrentThread.Priority;
                Thread.CurrentThread.Priority = ThreadPriority.Lowest;

                int[] wndR = new int[wnd_size * wnd_size];
                int[] wndG = new int[wnd_size * wnd_size];
                int[] wndB = new int[wnd_size * wnd_size];

                for (int _j = 0; _j < width; ++_j)
                {

                    for (int ii = 0; ii < wnd_size; ++ii) // h - (i - h)     h - i + h = 2h-i
                    {
                        for (int jj = 0; jj < wnd_size; ++jj)
                        {
                            int i = _i + ii - wnd_size / 2;

                            if (i < 0)
                                i *= -1;
                            if (i >= height)
                                i = 2 * height - i - 1;

                            int j = _j + jj - wnd_size / 2;
                            if (j < 0)
                                j *= -1;
                            if (j >= width)
                                j = 2 * width - j - 1;

                            wndR[ii * wnd_size + jj] = old_bytes[i * width * 3 + j * 3 + 0];
                            wndG[ii * wnd_size + jj] = old_bytes[i * width * 3 + j * 3 + 1];
                            wndB[ii * wnd_size + jj] = old_bytes[i * width * 3 + j * 3 + 2];
                        }
                    }

                    //new_bytes[_i * width * 3 + _j * 3 + 0] = (byte)QuickSelect.kthSmallest(wndR, 0, wndR.Length - 1, wnd_size * wnd_size / 2 - 1); 
                    //new_bytes[_i * width * 3 + _j * 3 + 1] = (byte)QuickSelect.kthSmallest(wndG, 0, wndG.Length - 1, wnd_size * wnd_size / 2 - 1); ;
                    //new_bytes[_i * width * 3 + _j * 3 + 2] = (byte)QuickSelect.kthSmallest(wndB, 0, wndB.Length - 1, wnd_size * wnd_size / 2 - 1); ;

                    //new_bytes[_i * width * 3 + _j * 3 + 0] = (byte)quickselect(wndR, wnd_size * wnd_size / 2);
                    //new_bytes[_i * width * 3 + _j * 3 + 1] = (byte)quickselect(wndG, wnd_size * wnd_size / 2);
                    //new_bytes[_i * width * 3 + _j * 3 + 2] = (byte)quickselect(wndB, wnd_size * wnd_size / 2);

                    new_bytes[_i * width * 3 + _j * 3 + 0] = (byte)quickselect2(wndR, 0, wnd_size * wnd_size, wnd_size * wnd_size / 2);
                    new_bytes[_i * width * 3 + _j * 3 + 1] = (byte)quickselect2(wndG, 0, wnd_size * wnd_size, wnd_size * wnd_size / 2);
                    new_bytes[_i * width * 3 + _j * 3 + 2] = (byte)quickselect2(wndB, 0, wnd_size * wnd_size, wnd_size * wnd_size / 2);


                }

                job.incrementProgress();
                Thread.CurrentThread.Priority = curPriority;

            });


            Bitmap new_bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            new_bitmap.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(new_bitmap, new_bytes);

            return new_bitmap;

        }

        public static double F(double x)
        {
            return Math.Log(x+1);
        }

        public static double Butter(double x, double y, double wx,  double n, double dx=0, double dy=0, double G=1.0, double h=0)
        {
            double D = Math.Sqrt((x - dx) * (x - dx) + (y - dy) * (y - dy)) - h;
            return G /( 1 +  Math.Pow( D / wx, 2*n)  );
        }

        public static double Gauss(double x, double y, double wx, double dx = 0, double dy = 0, double G = 1.0, double h = 0)
        {
            double D = Math.Sqrt((x - dx) * (x - dx) + (y - dy) * (y - dy)) - h;
            return G * Math.Exp( -(D*D / (2.0 * wx * wx) ) );
        }


        public static (Bitmap, Bitmap, Bitmap) Furier(
            JobTask job,
            Bitmap input, 
            int filter_type,
            string filter, 
            double in_filter_zone=1.0, 
            double out_filter_zone=0.0, 
            double furier_multiplyer = 1.0
            )
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
            byte[] filter_bytes = new byte[new_width * new_height * 3];

            using Graphics g = Graphics.FromImage(_tmp);
            g.DrawImageUnscaled(input, 0, 0);

            byte[] old_bytes = getImgBytes(_tmp);


            var ss = StringSplitOptions.RemoveEmptyEntries;
            var filter_params_strings = filter.Split("\n", ss);
            filter_params_strings = (from s in filter_params_strings where (s.Trim() != string.Empty) select s).ToArray();
            var cult = new CultureInfo("en-US");
            var filter_params_double = filter_params_strings.Select(a=> a.Split(";", ss)
                .Select(b=>Convert.ToDouble(b.Trim(),cult)).ToArray() ).ToArray();


            if (job != null) job.operations_count = 6;

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
                if (job != null) job.incrementProgress();


                var max_ma = complex_bytes.Max(x => F( x.Imaginary ) );

                Complex[] complex_bytes_filtered = null;


                if (filter_type == 0) //идеальный
                {
                    complex_bytes_filtered = complex_bytes.Select((a, i) =>
                    {
                        int y = i / new_width;
                        int x = i - y * new_width;
                        y -= new_height / 2;
                        x -= new_width / 2;


                        foreach (var v in filter_params_double)
                        {
                            if ((x - v[0]) * (x - v[0]) + (y - v[1]) * (y - v[1]) >= v[2] * v[2] &&
                                (x - v[0]) * (x - v[0]) + (y - v[1]) * (y - v[1]) <= v[3] * v[3])
                            {
                                filter_bytes[i * 3 + color] = clmp(255 * in_filter_zone);
                                return a * in_filter_zone;
                            }
                                
                        }
                        filter_bytes[i * 3 + color] = clmp(255 * out_filter_zone);
                        return a * out_filter_zone;

                    }).ToArray();
                }
                else 
                if (filter_type == 1) //Баттерворта ФНЧ
                {
                    complex_bytes_filtered = complex_bytes.Select((a, i) =>
                    {
                        var val = filter_params_double.Select(v =>
                        {
                            int y = i / new_width;
                            int x = i - y * new_width;
                            y -= new_height / 2;
                            x -= new_width / 2;

                            double wc = 0.5 * v[3] - 0.5 * v[2];
                            double h = v[3] - wc;
                            double b = Butter(x, y, wc, (int)out_filter_zone, v[0], v[1], in_filter_zone,h);
                            return b;
                        }).Max();
                        filter_bytes[i*3+color] = clmp(255 * val);
                        return a * val;
                    }).ToArray();
                }
                else if (filter_type == 2)          //Баттерворта ФВЧ
                {
                    complex_bytes_filtered = complex_bytes.Select((a, i) =>
                    {
                        var val = filter_params_double.Select(v =>
                        {
                            int y = i / new_width;
                            int x = i - y * new_width;
                            y -= new_height / 2;
                            x -= new_width / 2;

                            double wc = 0.5 * v[3] - 0.5 * v[2];
                            double h = v[3] - wc;
                            double b = in_filter_zone - Butter(x, y, wc, (int)out_filter_zone, v[0], v[1], in_filter_zone,h);
                            return b;
                        }).Min();
                        filter_bytes[i * 3 + color] = clmp(255 * val);
                        return a * val;
                    }).ToArray();
                }
                else if (filter_type == 3)  //Гаусса ФНЧ
                {
                    complex_bytes_filtered = complex_bytes.Select((a, i) =>
                    {
                        var val = filter_params_double.Select(v =>
                        {
                            int y = i / new_width;
                            int x = i - y * new_width;
                            y -= new_height / 2;
                            x -= new_width / 2;
                            double wc = 0.5 * v[3] - 0.5 * v[2];
                            double h = v[3] - wc;
                            double b = Gauss(x, y, wc,  v[0], v[1], in_filter_zone, h);
                            return b;
                        }).Max();
                        filter_bytes[i * 3 + color] = clmp(255 * val);
                        return a * val;
                    }).ToArray();
                }
                else if (filter_type == 4) //Гаусса ФВЧ
                {
                    complex_bytes_filtered = complex_bytes.Select((a, i) =>
                    {

                        var val = filter_params_double.Select(v =>
                        {
                            int y = i / new_width;
                            int x = i - y * new_width;
                            y -= new_height / 2;
                            x -= new_width / 2;
                            double wc = 0.5 * v[3] - 0.5 * v[2];
                            double h = v[3] - wc;
                            double b = in_filter_zone - Gauss(x, y, wc, v[0], v[1], in_filter_zone, h);
                            return b;
                        }).Min();
                        filter_bytes[i * 3 + color] = clmp(255 * val);
                        return a * val;
                    }).ToArray();
                }


                var complex_bytes_result = FFT.ditifft2d(complex_bytes_filtered, new_width, new_height);

                for (int i = 0; i < new_width * new_height; ++i)
                {
                    int y = i / new_width;
                    int x = i - y * new_width;
                    y -= new_height / 2;
                    x -= new_width / 2;
                    new_bytes[i * 3 + color] = clmp(Math.Round( (Math.Pow(-1,x+y) * complex_bytes_result[i]).Real));
                    furier_ma_bytes[i * 3 + color] = clmp(furier_multiplyer * F(complex_bytes[i].Magnitude)*255/max_ma );
                }
                if (job != null) job.incrementProgress();
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
           
            //формируем восстановленное изображение
            using Bitmap new_bitmap = new Bitmap(new_width, new_height, PixelFormat.Format24bppRgb);
            new_bitmap.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(new_bitmap,new_bytes);

            //рисуем восстановленное изображение на новом, размер которого совпадает с исходным
            //так как размер восстановленного может отличатся (степени двойки)
            Bitmap new_bitamp_ret = new Bitmap(width,height, PixelFormat.Format24bppRgb);
            new_bitamp_ret.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using (Graphics g1 = Graphics.FromImage(new_bitamp_ret))
            {
                g1.DrawImageUnscaled(new_bitmap,0,0);
            }

            //рисуем Фурье-образ и рисуем на нем оверлеи.
            Bitmap new_bitmap_re = new Bitmap(new_width, new_height, PixelFormat.Format24bppRgb);
            new_bitmap_re.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(new_bitmap_re, furier_ma_bytes);
            using var g_fur = Graphics.FromImage(new_bitmap_re);
            foreach (var v in filter_params_double)
            {
                g_fur.DrawEllipse(Pens.GreenYellow, (int)v[0] - (int)v[2] + new_width / 2, (int)v[1] - (int)v[2] + new_height /2, (int)v[2] * 2, (int)v[2] * 2);
                g_fur.DrawEllipse(Pens.GreenYellow, (int)v[0] - (int)v[3] + new_width / 2, (int)v[1] - (int)v[3] + new_height / 2, (int)v[3] * 2, (int)v[3] * 2);
            }

            //рисуем маску фильтра
            Bitmap new_bitmap_mask = new Bitmap(new_width, new_height, PixelFormat.Format24bppRgb);
            new_bitmap_mask.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(new_bitmap_mask,filter_bytes);

            return (new_bitamp_ret, new_bitmap_re, new_bitmap_mask);

        }

        //https://ru.wikipedia.org/wiki/%D0%9C%D0%B5%D1%82%D0%BE%D0%B4_%D0%9E%D1%86%D1%83
        public static Bitmap BinaryzationOtsu(Bitmap input)
        {
            int width = input.Width;
            int height = input.Height;
            using Bitmap _tmp = new Bitmap(width,height,PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using var g = Graphics.FromImage(_tmp);
            g.DrawImageUnscaled(input,0,0);

            byte[] input_bytes = getImgBytes(_tmp);
            byte[] out_bytes = new byte[input_bytes.Length];
            

            //записываем в красный канал среднее по каналам пикселя.
            //работаем только с ним
            for (int i = 0; i < input_bytes.Length; i += 3)
            {
                input_bytes[i]= clmp(0.2125 * input_bytes[i+2] + 0.7154 * input_bytes[i+1] + 0.0721 * input_bytes[i]);
                //input_bytes[i + 2] = input_bytes[i + 1] = input_bytes[i];
            }
            

            
            double[] N = new double[256];
            double [] sum_N = new double[256]; 
            double [] sum_u = new double[256];
            var Nt = 0.0;
            var max = 0;
            for (int i = 0; i < width * height; ++i)
            {
                N[input_bytes[i * 3]] += 1.0 / width / height;
                Nt += 1.0 / width / height;
                if (input_bytes[i * 3] > max)
                    max = input_bytes[i * 3];
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

                if (input_bytes[i * 3] > final_t)
                {
                    out_bytes[i * 3 + 0] = 255;
                    out_bytes[i * 3 + 1] = 255;
                    out_bytes[i * 3 + 2] = 255;
                }
                        
                else
                {
                    out_bytes[i * 3 + 0] = 0;
                    out_bytes[i * 3 + 1] = 0;
                    out_bytes[i * 3 + 2] = 0;
                }
                        
            }

            

            Bitmap img_ret = new Bitmap(width,height,PixelFormat.Format24bppRgb);
            writeImageBytes(img_ret,out_bytes);
            return img_ret;

        }
        
        public static Bitmap BinarizationNiblack(Bitmap input, int a = 21, double sens = -0.2, int h=0)
        {
            int width = input.Width;
            int height = input.Height;
            using Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using var g = Graphics.FromImage(_tmp);
            g.DrawImageUnscaled(input, 0, 0);

            byte[] input_bytes = getImgBytes(_tmp);



            //чб изображние кладем в красный канал

            Parallel.For(0, width * height, arr_i =>
            {
                int i = arr_i * 3;
                input_bytes[i] = (byte)(0.2125 * input_bytes[i + 2] + 0.7154 * input_bytes[i + 1] +
                                        0.0721 * input_bytes[i + 0]);
            });


            //интегральные матрицы для простого вычисления сумм.
            //https://habr.com/ru/post/278435/

            var int_mat = new long[height, width];
            var int_sqr_mat = new long[height, width];

            Parallel.For(0, width * height, arr_i =>
            {
                int i = arr_i / width;
                int j = arr_i - i * width;
                int_mat[i, j] = input_bytes[i * width * 3 + j * 3] +
                                (j >= 1 ? int_mat[i, j - 1] : 0) +
                                (i >= 1 ? int_mat[i - 1, j] : 0) -
                                (i >= 1 && j >= 1 ? int_mat[i - 1, j - 1] : 0);

                int_sqr_mat[i, j] = input_bytes[i * width * 3 + j * 3] * input_bytes[i * width * 3 + j * 3] +
                                    (j >= 1 ? int_sqr_mat[i, j - 1] : 0) +
                                    (i >= 1 ? int_sqr_mat[i - 1, j] : 0) -
                                    (i >= 1 && j >= 1 ? int_sqr_mat[i - 1, j - 1] : 0);
            });


            Parallel.For(0, width * height, arr_i =>
            {
                if (input_bytes[arr_i*3] < h)
                {
                    input_bytes[arr_i*3 + 1] = 0;
                    return;
                }
                else if (input_bytes[arr_i*3] > 255 - h)
                {
                    input_bytes[arr_i*3 + 1] = 255;
                    return;
                }

                int _i = arr_i / width;
                int _j = arr_i - _i * width;

                int y_min = _i - (int)Math.Ceiling(1.0 * a / 2) + 1;
                y_min = (y_min < 0) ? 0 : y_min;
                int y_max = _i + (int)Math.Floor(1.0 * a / 2);
                y_max = (y_max >= height) ? height - 1 : y_max;

                int index = arr_i*3;

                long sum = 0;
                long sqr_sum = 0;

                int x_min = _j - (int)Math.Ceiling(1.0 * a / 2) + 1;
                x_min = (x_min < 0) ? 0 : x_min;
                int x_max = _j + (int)Math.Floor(1.0 * a / 2);
                x_max = (x_max >= width) ? width - 1 : x_max;



                sum = ((x_min >= 1 && y_min >= 1) ? int_mat[y_min - 1, x_min - 1] : 0) + //A
                      int_mat[y_max, x_max] -    //D
                      ((y_min >= 1) ? int_mat[y_min - 1, x_max] : 0) -   //B
                      ((x_min >= 1) ? int_mat[y_max, x_min - 1] : 0);  //C

                sqr_sum = ((x_min >= 1 && y_min >= 1) ? int_sqr_mat[y_min - 1, x_min - 1] : 0) + //A
                          int_sqr_mat[y_max, x_max] -    //D
                          ((y_min >= 1) ? int_sqr_mat[y_min - 1, x_max] : 0) -   //B
                          ((x_min >= 1) ? int_sqr_mat[y_max, x_min - 1] : 0);  //C

                sqr_sum /= (x_max - x_min + 1) * (y_max - y_min + 1);
                sum /= (x_max - x_min + 1) * (y_max - y_min + 1);

                double D = Math.Sqrt(sqr_sum - sum * sum);
                double t = sum + sens * D;


                //результат обработки кладем в синий канал
                input_bytes[index + 1] = (input_bytes[index] <= t) ? (byte)0 : (byte)255;

            });


            Parallel.For(0, width * height, arr_i =>
            {
                input_bytes[arr_i * 3 + 0] = input_bytes[arr_i * 3 + 1];
                input_bytes[arr_i * 3 + 2] = input_bytes[arr_i * 3 + 1];
            });



            Bitmap img_ret = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            img_ret.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(img_ret, input_bytes);
            return img_ret;
        }

        public static Bitmap BinarizationSauval(Bitmap input, int a = 21, double k = 0.5, int h = 0)
        {


            int width = input.Width;
            int height = input.Height;
            using Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using var g = Graphics.FromImage(_tmp);
            g.DrawImageUnscaled(input, 0, 0);

            byte[] input_bytes = getImgBytes(_tmp);



            //чб изображние кладем в красный канал

            for (int i = 0; i < input_bytes.Length; i += 3)
            {
                input_bytes[i] = (byte)(0.2125 * input_bytes[i + 2] + 0.7154 * input_bytes[i + 1] +
                                        0.0721 * input_bytes[i + 0]);
            }


            //интегральные матрицы для простого вычисления сумм.
            //https://habr.com/ru/post/278435/

            var int_mat = new long[height, width];
            var int_sqr_mat = new long[height, width];

            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    int_mat[i, j] = input_bytes[i * width * 3 + j * 3] +
                                    (j >= 1 ? int_mat[i, j - 1] : 0) +
                                    (i >= 1 ? int_mat[i - 1, j] : 0) -
                                    (i >= 1 && j >= 1 ? int_mat[i - 1, j - 1] : 0);

                    int_sqr_mat[i, j] = input_bytes[i * width * 3 + j * 3] * input_bytes[i * width * 3 + j * 3] +
                                    (j >= 1 ? int_sqr_mat[i, j - 1] : 0) +
                                    (i >= 1 ? int_sqr_mat[i - 1, j] : 0) -
                                    (i >= 1 && j >= 1 ? int_sqr_mat[i - 1, j - 1] : 0);
                }
            }

            for (int _i = 0; _i < height; ++_i)
            {
                int y_min = _i - (int)Math.Ceiling(1.0 * a / 2) + 1;
                y_min = (y_min < 0) ? 0 : y_min;
                int y_max = _i + (int)Math.Floor(1.0 * a / 2);
                y_max = (y_max >= height) ? height - 1 : y_max;

                for (int _j = 0; _j < width; ++_j)
                {

                    int index = _i * width * 3 + _j * 3;

                    if (input_bytes[index] < h)
                    {
                        input_bytes[index + 1] = 0;
                        continue;
                    }
                    else if (input_bytes[index] > 255 - h)
                    {
                        input_bytes[index + 1] = 255;
                        continue;
                    }

                    long sum = 0;
                    long sqr_sum = 0;

                    int x_min = _j - (int)Math.Ceiling(1.0 * a / 2) + 1;
                    x_min = (x_min < 0) ? 0 : x_min;
                    int x_max = _j + (int)Math.Floor(1.0 * a / 2);
                    x_max = (x_max >= width) ? width - 1 : x_max;



                    sum = ((x_min >= 1 && y_min >= 1) ? int_mat[y_min - 1, x_min - 1] : 0) + //A
                        int_mat[y_max, x_max] -    //D
                        ((y_min >= 1) ? int_mat[y_min - 1, x_max] : 0) -   //B
                        ((x_min >= 1) ? int_mat[y_max, x_min - 1] : 0);  //C

                    sqr_sum = ((x_min >= 1 && y_min >= 1) ? int_sqr_mat[y_min - 1, x_min - 1] : 0) + //A
                              int_sqr_mat[y_max, x_max] -    //D
                          ((y_min >= 1) ? int_sqr_mat[y_min - 1, x_max] : 0) -   //B
                          ((x_min >= 1) ? int_sqr_mat[y_max, x_min - 1] : 0);  //C

                    sqr_sum /= (x_max - x_min + 1) * (y_max - y_min + 1);
                    sum /= (x_max - x_min + 1) * (y_max - y_min + 1);

                    double D = Math.Sqrt(sqr_sum - sum * sum);
                    double t = sum * (1 + k * (D / 128 - 1));




                    //результат обработки кладем в зеленый канал
                    input_bytes[index + 1] = (input_bytes[index] <= t) ? (byte)0 : (byte)255;

                }
            }

            for (int i = 0; i < input_bytes.Length; i += 3)
            {
                input_bytes[i + 0] = input_bytes[i + 1];
                input_bytes[i + 2] = input_bytes[i + 1];
            }


            Bitmap img_ret = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            img_ret.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(img_ret, input_bytes);
            return img_ret;
        }

        public static Bitmap BinarizationWolf(Bitmap input, int a = 21, double gain = 0.5, int h = 0)
        {


            int width = input.Width;
            int height = input.Height;
            using Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using var g = Graphics.FromImage(_tmp);
            g.DrawImageUnscaled(input, 0, 0);

            byte[] input_bytes = getImgBytes(_tmp);



            //чб изображние кладем в красный канал

            byte M = 255;
            for (int i = 0; i < input_bytes.Length; i += 3)
            {
                input_bytes[i] = (byte)(0.2125 * input_bytes[i + 2] + 0.7154 * input_bytes[i + 1] +
                                        0.0721 * input_bytes[i + 0]);
                if (input_bytes[i] < M)
                    M = input_bytes[i];
            }


            //интегральные матрицы для простого вычисления сумм.
            //https://habr.com/ru/post/278435/

            var int_mat = new long[height, width];
            var int_sqr_mat = new long[height, width];

            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    int_mat[i, j] = input_bytes[i * width * 3 + j * 3] +
                                    (j >= 1 ? int_mat[i, j - 1] : 0) +
                                    (i >= 1 ? int_mat[i - 1, j] : 0) -
                                    (i >= 1 && j >= 1 ? int_mat[i - 1, j - 1] : 0);

                    int_sqr_mat[i, j] = input_bytes[i * width * 3 + j * 3] * input_bytes[i * width * 3 + j * 3] +
                                    (j >= 1 ? int_sqr_mat[i, j - 1] : 0) +
                                    (i >= 1 ? int_sqr_mat[i - 1, j] : 0) -
                                    (i >= 1 && j >= 1 ? int_sqr_mat[i - 1, j - 1] : 0);
                }
            }

            double R = 0;
            double[] s_arr = new double[width*height];
            double[] m_arr = new double[width*height];

            for (int _i = 0; _i < height; ++_i)
            {
                int y_min = _i - (int)Math.Ceiling(1.0 * a / 2) + 1;
                y_min = (y_min < 0) ? 0 : y_min;
                int y_max = _i + (int)Math.Floor(1.0 * a / 2);
                y_max = (y_max >= height) ? height - 1 : y_max;

                for (int _j = 0; _j < width; ++_j)
                {

                    int index = _i * width * 3 + _j * 3;

                    if (input_bytes[index] < h)
                    {
                        input_bytes[index + 1] = 0;
                        continue;
                    }
                    else if (input_bytes[index] > 255 - h)
                    {
                        input_bytes[index + 1] = 255;
                        continue;
                    }

                    long sum = 0;
                    long sqr_sum = 0;

                    int x_min = _j - (int)Math.Ceiling(1.0 * a / 2) + 1;
                    x_min = (x_min < 0) ? 0 : x_min;
                    int x_max = _j + (int)Math.Floor(1.0 * a / 2);
                    x_max = (x_max >= width) ? width - 1 : x_max;



                    sum = ((x_min >= 1 && y_min >= 1) ? int_mat[y_min - 1, x_min - 1] : 0) + //A
                          int_mat[y_max, x_max] -    //D
                          ((y_min >= 1) ? int_mat[y_min - 1, x_max] : 0) -   //B
                          ((x_min >= 1) ? int_mat[y_max, x_min - 1] : 0);  //C

                    sqr_sum = ((x_min >= 1 && y_min >= 1) ? int_sqr_mat[y_min - 1, x_min - 1] : 0) + //A
                              int_sqr_mat[y_max, x_max] -    //D
                              ((y_min >= 1) ? int_sqr_mat[y_min - 1, x_max] : 0) -   //B
                              ((x_min >= 1) ? int_sqr_mat[y_max, x_min - 1] : 0);  //C

                    sqr_sum /= (x_max - x_min + 1) * (y_max - y_min + 1);
                    sum /= (x_max - x_min + 1) * (y_max - y_min + 1);
                    double D = Math.Sqrt(sqr_sum - sum * sum);

                    m_arr[_i * width + _j] = sum;
                    s_arr[_i * width + _j] = D;

                    if (D > R) R = D;

                }
            }


            for (int i = 0; i < width * height; ++i)
            {
                double t = (1.0 - gain) * m_arr[i] + gain * M + gain * s_arr[i] / R * (m_arr[i] - M);

                input_bytes[i*3 + 1] = (input_bytes[i*3] <= t) ? (byte)0 : (byte)255;
                input_bytes[i*3 + 0] = input_bytes[i * 3 + 1];
                input_bytes[i * 3 + 2] = input_bytes[i * 3 + 1];
            }
            
   
            Bitmap img_ret = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            img_ret.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(img_ret, input_bytes);
            return img_ret;
        }

        public static Bitmap BinarizationBredly(Bitmap input, int a = 21, double t = 0.15, int h = 0)
        {
            int width = input.Width;
            int height = input.Height;
            using Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using var g = Graphics.FromImage(_tmp);
            g.DrawImageUnscaled(input, 0, 0);

            byte[] input_bytes = getImgBytes(_tmp);



            //чб изображние кладем в красный канал

            for (int i = 0; i < input_bytes.Length; i += 3)
            {
                input_bytes[i] = (byte)(0.2125 * input_bytes[i + 2] + 0.7154 * input_bytes[i + 1] +
                                        0.0721 * input_bytes[i + 0]);
            }


            //интегральные матрицы для простого вычисления сумм.
            //https://habr.com/ru/post/278435/

            var int_mat = new long[height, width];
            
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    int_mat[i, j] = input_bytes[i * width * 3 + j * 3] +
                                    (j >= 1 ? int_mat[i, j - 1] : 0) +
                                    (i >= 1 ? int_mat[i - 1, j] : 0) -
                                    (i >= 1 && j >= 1 ? int_mat[i - 1, j - 1] : 0);
                }
            }

            for (int _i = 0; _i < height; ++_i)
            {
                int y_min = _i - (int)Math.Ceiling(1.0 * a / 2) + 1;
                y_min = (y_min < 0) ? 0 : y_min;
                int y_max = _i + (int)Math.Floor(1.0 * a / 2);
                y_max = (y_max >= height) ? height - 1 : y_max;

                for (int _j = 0; _j < width; ++_j)
                {

                    int index = _i * width * 3 + _j * 3;

                    if (input_bytes[index] < h)
                    {
                        input_bytes[index + 1] = 0;
                        continue;
                    }
                    else if (input_bytes[index] > 255 - h)
                    {
                        input_bytes[index + 1] = 255;
                        continue;
                    }

                    long sum = 0;
                    int x_min = _j - (int)Math.Ceiling(1.0 * a / 2) + 1;
                    x_min = (x_min < 0) ? 0 : x_min;
                    int x_max = _j + (int)Math.Floor(1.0 * a / 2);
                    x_max = (x_max >= width) ? width - 1 : x_max;



                    sum = ((x_min >= 1 && y_min >= 1) ? int_mat[y_min - 1, x_min - 1] : 0) + //A
                        int_mat[y_max, x_max] -    //D
                        ((y_min >= 1) ? int_mat[y_min - 1, x_max] : 0) -   //B
                        ((x_min >= 1) ? int_mat[y_max, x_min - 1] : 0);  //C

                    int count = (x_max - x_min + 1) * (y_max - y_min + 1);


                    //результат обработки кладем в синий канал
                    input_bytes[index + 1] = ( (Int64)(input_bytes[index]*count) < (Int64)(sum*(1.0-t))  ) ? (byte)0 : (byte)255;

                }
            }

            for (int i = 0; i < input_bytes.Length; i += 3)
            {
                input_bytes[i + 0] = input_bytes[i + 1];
                input_bytes[i + 2] = input_bytes[i + 1];
            }


            Bitmap img_ret = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            img_ret.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(img_ret, input_bytes);
            return img_ret;
        }

        public static Bitmap BinaryzationAvg(Bitmap input)
        {
            int width = input.Width;
            int height = input.Height;
            using Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using var g = Graphics.FromImage(_tmp);
            g.DrawImageUnscaled(input, 0, 0);

            byte[] input_bytes = getImgBytes(_tmp);
            byte[] out_bytes = new byte[input_bytes.Length];

            for (int i = 0; i < input_bytes.Length; i += 3)
            {
                input_bytes[i] = clmp(0.2125 * input_bytes[i+2] + 0.7154 * input_bytes[i + 1] + 0.0721 * input_bytes[0]);
                input_bytes[i + 2] = input_bytes[i + 1] = input_bytes[i];
            }

            var t = input_bytes.Average(x=>x);

            var bytes = input_bytes.Select(x => (x<t)?(byte)0: (byte)255 ).ToArray();
            int a = 10;
            var ty =  a << 2;

            Bitmap img_ret = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            img_ret.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(img_ret, bytes);

            return img_ret;

        }


        public static Bitmap SomeFunction(Bitmap input)
        {
            int width = input.Width;
            int height = input.Height;
            //создаем временное изображние с нужным нам форматом хранения.
            //так как обработка побайтовая, там важно расположение байтов в картинке.
            //а оно опеределено форматом хранения

            byte[] input_bytes = new byte[0]; //пустой массивчик байт

            using (Bitmap _tmp = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                //устанавливаем DPI такой же как у исходного
                _tmp.SetResolution(input.HorizontalResolution, input.VerticalResolution);

                //рисуем исходное изображение на временном.
                using (var g = Graphics.FromImage(_tmp))
                {
                    g.DrawImageUnscaled(input, 0, 0);
                }
                input_bytes = getImgBytes(input); //получаем байты изображения, см. описание ф-ции ниже
            }

            /*
                Вот на этом моменте у нам в массиве input_bytes лежит побайтовая копия исходной картинки.
                в формате BGR-BGR-BGR-BGR-BGR-BGR (обратите внимание, цвета хранятся наоборот)
                P.S. Там еще что то перевурнуто, толи строки, толи столбцы, подзабыл

                Обработка картинки таким образом В РАЗЫ быстрее, чем через Bitmap
                

             */
            
            //Допустим, мы обработаки картинку и сложили результат сюда:
            byte[] bytes = new byte[width*height*3];

            Bitmap img_ret = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            img_ret.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            writeImageBytes(img_ret, bytes);
            return img_ret;

        }
        public static byte[] getImgBytes(Bitmap img, int bytes_per_pixel = 3)
        {
            byte[] bytes = new byte[img.Width * img.Height * bytes_per_pixel];  //выделяем память под массив байтов
            
            var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),  //блокируем участок памати, занимаемый изображением
                ImageLockMode.ReadOnly,
                img.PixelFormat);
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);  //копируем байты изображения в массив
            img.UnlockBits(data);   //разблокируем изображение
            return bytes; //возвращаем байты
        }

        static void writeImageBytes(Bitmap img, byte[] bytes)
        {
            var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),  //блокируем участок памати, занимаемый изображением
                ImageLockMode.WriteOnly,
                img.PixelFormat);
            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length); //копируем байты массива в изображение

            img.UnlockBits(data);  //разблокируем изображение
        }
        public static Bitmap to1Bit(Bitmap input)
        {
            Bitmap b = new Bitmap(input.Width, input.Height, PixelFormat.Format1bppIndexed);
            b.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            using var g = Graphics.FromImage(b);
            g.DrawImageUnscaled(input, 0, 0);
            return input;
        }

        
    }


}
