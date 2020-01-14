using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scoi.Models
{
    static class Mathematics
    {
        public static int NOD(int a, int b)
        {
            if (a == 0 || b == 0)
                return a + b;

            if (a<b)
            {
                a = a + b;
                b = a - b;
                a = a - b;
            }

            while (true)
            {
                a = a - (a / b) * b;
                if (a == 0) return b;
                b = b - (b / a) * a;
                if (b == 0) return a;
            }
            
        }

        public static int NOK(int a, int b)
        {
            return Math.Abs(a * b) / NOD(a, b);
        }

        public static int NumSize(int i)
        {

            int razm = 0;
            
            while (true)
            {
                if (i / Math.Pow(10, razm) >= 1 && i / Math.Pow(10, razm + 1) < 1)
                    return razm + 1;
                razm++;
            }
        }
    }
}
