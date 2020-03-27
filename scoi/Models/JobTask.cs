using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;

namespace scoi.Models
{
    public class JobTask
    {
       
        Mutex prog_mutex = new Mutex();
        int _prog = 0;
       
        public Action action { set; get; }
        private bool finalied = false;
        public int progress
        {
            set
            {
                //prog_mutex.WaitOne();
                _prog = value;
                Interlocked.Exchange(ref _prog, value);
               // prog_mutex.ReleaseMutex();
            }

            get
            {
                if (finalied) return 100;
                //prog_mutex.WaitOne();
                int p = (int)(99.0 * _prog / operations_count);
                //prog_mutex.ReleaseMutex();
                return p;
            }
        }

        public void incrementProgress()
        {
            Interlocked.Increment(ref _prog);
        }
        public void finalize()
        {
            finalied = true;

        }

        public int operations_count { set; get; } = 1;
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
