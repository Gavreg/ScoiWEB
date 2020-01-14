using System;
using System.Linq;
using System.Numerics;
using scoi.Models;

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

            DateTime time1 = DateTime.Now;
            
            var ft_result = FFT.ditfft2d(mas, w, h);
            
            //ft_result = ft_result.Select(x => new Complex(x.Real / ft_result.Length, x.Imaginary / ft_result.Length) ).ToArray();

            ft_result = FFT.ditifft2d(ft_result, w, h);

            DateTime time2 = DateTime.Now;

            Console.WriteLine( (time2-time1).TotalMilliseconds);
            //foreach (var VARIABLE in ft_result)
            //{
            //    Console.WriteLine(VARIABLE);
            //}
            Console.WriteLine();





        }
    }
}
