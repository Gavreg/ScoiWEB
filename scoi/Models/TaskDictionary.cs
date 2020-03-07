using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;


namespace scoi.Models
{
    public class TaskDictionary
    {
        uint max_code = 0;
        public Dictionary<uint, JobTask> tasks = new Dictionary<uint, JobTask>();

        public TaskDictionary()
        {
            Task.Run(() =>
            {
                
                while (true)
                {
                    Thread.Sleep(30000);
                    var curDate = DateTime.Now;

                    var keysToRemove = (from v in tasks where v.Value.contextTask.IsCompleted && (curDate - v.Value.endTime).TotalMinutes >= 10 select v.Key).ToList();

                    foreach (var k in keysToRemove)
                    {
                        tasks.Remove(k);
                    }
                }

            });
        }

        public uint setTask (JobTask t)
        {
            tasks.Add(max_code++, t);

            if (max_code == uint.MaxValue)
                max_code = 1;
            
            t.Start();

            return max_code-1;
        }
        
    }
}
