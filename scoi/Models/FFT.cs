using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.Configuration;

namespace scoi.Models
{

    public static class FFT
    {
        //Быстрое преобразование Фурье (FFT).
        public static Complex[] ditfft2(Complex[] arr, int x0, int N , int s)
        {
            Complex[] X  = new Complex[N];
            if (N == 1)
            {
                X[0] = arr[x0];
            }
            else
            {
                ditfft2(arr,x0, N / 2, 2 * s).CopyTo(X,0);
                ditfft2(arr,x0+s, N / 2, 2 * s).CopyTo(X, N/2);

                for (int k = 0; k < N / 2; k++)
                {
                    var t = X[k];
                    double u = -2.0 * Math.PI * k / N;
                    X[k] = t + new Complex(Math.Cos(u), Math.Sin(u)) * X[k + N / 2];
                    X[k+N/2] = t - new Complex(Math.Cos(u), Math.Sin(u)) * X[k + N / 2];
                }
            }

            return X;
        }

        //Преобразование Фурье
        public static Complex[] ditft(Complex[] arr)
        {
            Complex []X = new Complex[arr.Length];
            for (int i = 0; i < arr.Length; ++i)
            {
                for (int j = 0; j < arr.Length; ++j)
                {
                    double u = -2.0 * Math.PI * i * j / arr.Length;
                    X[i] += (new Complex(Math.Cos(u), Math.Sin(u)) * arr[j]);
                }
                
            }

            return X;
        }

        public static Complex[] ditfft2d(Complex[] arr, int width, int height, bool use_FFT = true)
        {
            Complex[] X = new Complex[arr.Length];

            ParallelOptions opt = new ParallelOptions();
            if (Environment.ProcessorCount > 2)
                opt.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;
            else opt.MaxDegreeOfParallelism = 1;
            //for (int i = 0; i < height; ++i)
            Parallel.For(0, height,opt, i =>
                {
                    Complex[] tmp = new Complex[width];
                    Array.Copy(arr, i * width, tmp, 0, width);

                    tmp = use_FFT ? ditfft2(tmp, 0, width, 1) : ditft(tmp);

                    for (int k = 0; k < width; ++k)
                        X[i * width + k] = tmp[k]/width ;
                }
            );
            //for (int j = 0; j < width; ++j)
            Parallel.For(0, width, opt, j =>
                {
                    Complex[] tmp = new Complex[height];
                    for (int k = 0; k < height; ++k)
                        tmp[k] = X[j + k * width];

                    tmp = use_FFT ? ditfft2(tmp, 0, tmp.Length, 1) : ditft(tmp);

                    for (int k = 0; k < height; ++k)
                        X[j + k * width] = tmp[k]/height ;
                }
            );
            return X;
        }

        public static Complex[] ditifft2d(Complex[] arr, int width, int height, bool use_FFT = true)
        {
            Complex[] X = new Complex[arr.Length];

            ParallelOptions opt = new ParallelOptions();
            if (Environment.ProcessorCount > 2)
                opt.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;
            else opt.MaxDegreeOfParallelism = 1;
            //for (int i = 0; i < height; ++i)
            Parallel.For(0, height, opt,i =>
                {
                    Complex[] tmp = new Complex[width];
                    Array.Copy(arr, i * width, tmp, 0, width);
                    for (int k = 0; k < width; ++k)
                        tmp[k] = new Complex(arr[i * width + k].Real, -arr[i * width + k].Imaginary);
                    
                    tmp = use_FFT ? ditfft2(tmp, 0, width, 1) : ditft(tmp);
                    
                    for (int k = 0; k < width; ++k)
                        X[i * width + k] = (new Complex(tmp[k].Real, -tmp[k].Imaginary));

                }
            );

            //for (int j = 0; j < width; ++j)
            Parallel.For(0, width, opt, j =>
                {
                    Complex[] tmp = new Complex[height];
                    for (int k = 0; k < height; ++k)
                        tmp[k] = new Complex(X[j + k * width].Real, -X[j + k * width].Imaginary);
                    
                    tmp = use_FFT ? ditfft2(tmp, 0, tmp.Length, 1) : ditft(tmp);
                
                    for (int k = 0; k < height; ++k)
                        X[j + k * width] = (new Complex(tmp[k].Real,-tmp[k].Imaginary));
                }
            );
            return X;
        }
    }
}
