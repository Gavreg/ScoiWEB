using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using scoi.Models;
using System.Threading.Tasks;

namespace FFT_test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            int w = (int)Math.Pow(2, 4);
            int h = (int)Math.Pow(2, 4);
            var mas = Enumerable.Range(1, w*h).Select(x => new Complex(x, 0)).ToArray();

            //DateTime time1 = DateTime.Now;
            
            //var ft_result = FFT.ditfft2d(mas, w, h);
            
            ////ft_result = ft_result.Select(x => new Complex(x.Real / ft_result.Length, x.Imaginary / ft_result.Length) ).ToArray();

            //ft_result = FFT.ditifft2d(ft_result, w, h);

            //DateTime time2 = DateTime.Now;

            //Console.WriteLine( (time2-time1).TotalMilliseconds);
            //foreach (var VARIABLE in ft_result)
            //{
            //    Console.WriteLine(VARIABLE);
            //}
            Console.WriteLine();


            using Bitmap b = new Bitmap("..\\..\\..\\512 1.png");

            //var b_arr = (new Bitmap[372]).Select(x => b.Clone() as Bitmap).ToArray();


            int r1 = 0;
            int r2 = 0;
            int i = 0;

            using var f = new Font("Arial", 20);
            var (b1, b2) = ImageOperations.Furier(null, b, String.Format("256;256;200;260"));
            b1.Save("..\\..\\..\\tb1.jpg");
            b2.Save("..\\..\\..\\tb2.jpg");
            b1.Dispose();
            b2.Dispose();

            //for (r2 = 0; r2 <= 200; ++r2)
            //{ 

            //    var (b1, b2) = ImageOperations.Furier(null, b, String.Format("{0};{1};{2};{3}",256,256,r1,r2));
            //    using Bitmap newb = new Bitmap(2 * b.Width, b.Height);
            //    newb.SetResolution(b.HorizontalResolution, b.VerticalResolution);
            //    using Graphics g = Graphics.FromImage(newb);
            //    g.DrawImageUnscaled(b1, 0, 0);
            //    g.DrawImageUnscaled(b2, b.Width, 0);
            //    g.DrawString(String.Format("r1={0}   r2={1}",r1,r2),f,Brushes.GreenYellow, 530,10 );
            //    newb.Save("..\\..\\..\\Out\\" + Convert.ToString(i) + ".png");
            //    b1.Dispose();
            //    b2.Dispose();
            //    Console.WriteLine(i);
            //    ++i;
            //}

            //for (r1 = 1; r1 < 200; ++r1)
            //{

            //    var (b1, b2) = ImageOperations.Furier(null, b, String.Format("{0};{1};{2};{3}", 256, 256, r1, r2));
            //    using Bitmap newb = new Bitmap(2 * b.Width, b.Height);
            //    newb.SetResolution(b.HorizontalResolution, b.VerticalResolution);
            //    using Graphics g = Graphics.FromImage(newb);
            //    g.DrawImageUnscaled(b1, 0, 0);
            //    g.DrawImageUnscaled(b2, b.Width, 0);
            //    g.DrawString(String.Format("r1={0}   r2={1}", r1, r2), f, Brushes.GreenYellow, 530, 10);
            //    newb.Save("..\\..\\..\\Out\\" + Convert.ToString(i) + ".png");
            //    b1.Dispose();
            //    b2.Dispose();
            //    Console.WriteLine(i);
            //    ++i;
            //}



            /*
            int _h=0;
            for (int dif = 5; dif <= 50; dif += _h)
            {
                for (r1 = 0; r1 <= 100; ++r1)
                {

                    r2 = r1 + dif;
                    var (b1, b2) = ImageOperations.Furier(null, b, String.Format("{0};{1};{2};{3}", 256, 256, r1, r2), 5, 0, 5);
                    using Bitmap newb = new Bitmap(2 * b.Width, b.Height);
                    newb.SetResolution(b.HorizontalResolution, b.VerticalResolution);
                    using Graphics g = Graphics.FromImage(newb);
                    g.DrawImageUnscaled(b1, 0, 0);
                    g.DrawImageUnscaled(b2, b.Width, 0);
                    g.DrawString(String.Format("r1={0}   r2={1}", r1, r2), f, Brushes.GreenYellow, 530, 10);
                    newb.Save("..\\..\\..\\Out\\" + Convert.ToString(i) + ".png");
                    b1.Dispose();
                    b2.Dispose();
                    Console.WriteLine(i);
                    ++i;

                }

                _h += 5;
            }
            */

            //r1 =  0;
            //r2 = 20;
            //int k = 2;
            //int R = 20;

            //for (k=2; k<=8; k+=2)
            //for (R = 40; R < 160; R += 20)
            //{
            //    for (double fi = 0; fi < Math.PI * 2; fi += 5.0/180*Math.PI)
            //    {
            //        string filter = String.Empty;
            //        for (int _k = 0; _k < k; _k++)
            //        {
            //            double fase = 2 * Math.PI / k * _k;
            //            double x = 256 + R * Math.Cos(fi + fase);
            //            double y = 256.0 + R * Math.Sin(fi + fase);
            //            filter += String.Format(new System.Globalization.CultureInfo("en-US"),"{0:f3};{1:f3};{2};{3}\n", x , y , r1, r2);

            //        }
            //        var (b1, b2) = ImageOperations.Furier(null, b, filter, 5, 0, 5);
            //        using Bitmap newb = new Bitmap(2 * b.Width, b.Height);
            //        newb.SetResolution(b.HorizontalResolution, b.VerticalResolution);
            //        using Graphics g = Graphics.FromImage(newb);
            //        g.DrawImageUnscaled(b1, 0, 0);
            //        g.DrawImageUnscaled(b2, b.Width, 0);
            //        g.DrawString(filter, f, Brushes.GreenYellow, 530, 10);
            //        newb.Save("..\\..\\..\\Out\\" + Convert.ToString(i) + ".png");
            //        b1.Dispose();
            //        b2.Dispose();
            //        Console.WriteLine(i);
            //        ++i;
            //    }
            //}




        }
    }
}
