using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace scoi.Models
{
    public class JobTask
    {
       
        public Action action { set; get; }
        public int progress { set; get; }
        public string result_file { set; get; }

        public DateTime startTime { get; private set; }
        public DateTime endTime { get; private set; }



        public Task contextTask { get; private set; }

        public void Start()
        {
            contextTask = Task.Run(() =>
            {
                startTime = DateTime.Now;
                System.Diagnostics.Debug.WriteLine(startTime);

                //try
                {

                    action();

                }
               // catch (Exception e)
                {
                   // Console.WriteLine(e);
                  //  throw e;
                  //  progress = -1;
                }
                //finally
                {
                    endTime = DateTime.Now;
                    System.Diagnostics.Debug.WriteLine(endTime);
                }

            });
        }
    }
}
