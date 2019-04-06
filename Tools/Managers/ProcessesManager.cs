using lab5.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace lab5.Tools.Managers
{
    internal static class ProcessesManager
    {
        private static readonly Thread UpdateListThread;
        private static readonly Thread UpdateMetadataThread;

        internal static Dictionary<int, ProcessListItem> Processes { get; private set; }

        static ProcessesManager()
        {
            Processes = new Dictionary<int, ProcessListItem>();
            UpdateMetadataThread = new Thread(UpdateMetadata);
            UpdateListThread = new Thread(UpdateList);
            UpdateListThread.Start();
            UpdateMetadataThread.Start();
        }
        

        private static async void UpdateList()
        {
            while (true)
            {
                await Task.Run(() =>
                {
                    lock (Processes)
                    {
                        List<Process> processes = Process.GetProcesses().ToList();
                        IEnumerable<int> keys = Processes.Keys.ToList()
                            .Where(id => processes.All(process => process.Id != id));
                        foreach (int key in keys)
                        {
                            Processes.Remove(key);
                        }

                        foreach (Process process in processes)
                        {
                            if (!Processes.ContainsKey(process.Id))
                            {
                                try
                                {
                                    Processes[process.Id] = new ProcessListItem(process);
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                });
                Thread.Sleep(5000);
            }
        }

        private static async void UpdateMetadata()
        {
            while (true)
            {
                await Task.Run(() =>
                {
                    lock (Processes)
                    {
                        foreach (int id in Processes.Keys.ToList())
                        {
                            Process p;
                            try
                            {
                                p = Process.GetProcessById(id);
                            }
                            catch (ArgumentException)
                            {
                                Processes.Remove(id);
                                continue;
                            }
                            try
                            {
                                Processes[id].Cpu = (int)Processes[id].CpuCounter.NextValue();
                                Processes[id].RamSize = (int)(Processes[id].RamCounter.NextValue() / 1024 / 1024);
                                Processes[id].ThreadsAmount = p.Threads.Count;
                            }
                            catch(InvalidOperationException)
                            {

                            }
                        }
                    }
                });
                Thread.Sleep(2000);
            }
        }
    }
}
