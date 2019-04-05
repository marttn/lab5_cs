using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Windows;
using Microsoft.VisualBasic.Devices;

namespace lab5.Model
{
    class ProcessListItem
    {

        #region Fields
        private ObservableCollection<ThreadItem> threads;
        private ObservableCollection<ModuleItem> modules;
        #endregion

        #region Properties 
        Process Process { get; }
        public PerformanceCounter RamCounter { get; }
        public PerformanceCounter CpuCounter { get; }
        public int ProcessID { get; }
        public string ProcessName { get; }
        public double Cpu { get; set; }
        public double RamSize { get; set; }
        public double RamPercents { get; set; }
        public bool IsAlive { get; }
        public string FileName { get; }
        public string ProcessOwner { get; }
        public DateTime LaunchDateTime { get; }
        public int ThreadsAmount { get; set; }


        public ObservableCollection<ThreadItem> Threads
        {
            get
            {
                if (threads == null)
                {
                    GetThreads();
                }
                return threads;
            }
        }

        public ObservableCollection<ModuleItem> Modules
        {
            get
            {
                if (modules == null)
                {
                    GetModules();
                }
                return modules;
            }
        }
        #endregion

        #region Constructor
        public ProcessListItem(Process process)
        {
            Process = process;
            RamCounter = new PerformanceCounter("Process", "Working Set", process.ProcessName);
            CpuCounter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
            ProcessID = process.Id;
            ProcessName = process.ProcessName;
            IsAlive = process.Responding;
            Cpu = (int)CpuCounter.NextValue();
            RamSize = (int)(RamCounter.NextValue() / 1024 / 1024);
            RamPercents = GetRamPercentage(RamSize);
            ProcessOwner = GetProcessOwner(ProcessID);
            ThreadsAmount = Process.Threads.Count;
            try
            {
                FileName = GetMainModuleFilepath(ProcessID);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            try
            {
                LaunchDateTime = Process.StartTime;
            }
            catch (Exception)
            {
                LaunchDateTime = DateTime.Now;
            }
        }
        #endregion
        
        private void GetThreads()
        {
            try
            {
                threads = new ObservableCollection<ThreadItem>();
                foreach (ProcessThread thread in Process.Threads)
                {
                    threads.Add(new ThreadItem(thread));
                }
            }
            catch (Exception)
            {

            }
        }

        private void GetModules()
        {
            try
            {
                modules = new ObservableCollection<ModuleItem>();
                foreach (ProcessModule module in Process.Modules)
                {
                    modules.Add(new ModuleItem(module));
                }
            }
            catch(Exception)
            {

            }
        }

        public  string GetProcessOwner(int processId)
        {
            string query = "Select * From Win32_Process Where ProcessID = " + processId;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            foreach (ManagementObject obj in processList)
            {
                string[] argList = new string[] { string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    return  argList[0];
                }
            }
            return "";
        }

        private string GetMainModuleFilepath(int processId)
        {
            string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            {
                using (var results = searcher.Get())
                {
                    ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                    if (mo != null)
                    {
                        return (string)mo["ExecutablePath"];
                    }
                }
            }
            return null;
        }
       
        public double GetRamPercentage(double ram)
        {
            ComputerInfo CI = new ComputerInfo();
            ulong total = CI.TotalPhysicalMemory / (1024 * 1024);
            return (ram/total) * 100;
        }
    }


}
