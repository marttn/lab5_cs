using System;
using System.Diagnostics;

namespace lab5.Model
{
    class ThreadItem
    {
        private readonly ProcessThread thread;
        public int Id => thread.Id;
        public ThreadState State => thread.ThreadState;
        public DateTime LaunchDateTime
        {
            get
            {
                try
                {
                    return thread.StartTime;
                }
                catch(Exception)
                {
                    return DateTime.Now;
                }
            }
        }

         
        public ThreadItem(ProcessThread thread)
        {
            this.thread = thread;
        }

        
    }
}
