using lab5.Model;
using lab5.Tools;
using lab5.Tools.Managers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace lab5.ViewModel
{
    class ProcessListViewModel : INotifyPropertyChanged
    {
        #region Fields
        private string selectedProperty;
        private ProcessListItem selectedProcess;

        private readonly Thread _updateThread;
        private ObservableCollection<ProcessListItem> processesList;

        #region Commands
        private ICommand killProcessCommand;
        private ICommand folderCommand;
        private ICommand sortCommand;
        #endregion
        #endregion

        #region Properties

        public ObservableCollection<ProcessListItem> ProcessesList
        {
            get { return processesList; }
            private set
            {
                processesList = value;
                OnPropertyChanged();
            }
        }

        public ProcessListItem SelectedProcess
        {
            get { return selectedProcess; }
            set
            {
                selectedProcess = value;
                OnPropertyChanged();
                OnPropertyChanged("IsProcessSelected");
            }
        }

        public bool IsProcessSelected => SelectedProcess != null;


        public string SelectedProperty
        {
            get
            {
                return selectedProperty;
            }
            set
            {
                selectedProperty = value;
                OnPropertyChanged();
            }
        }


        public ICommand KillProcessCommand
        {
            get
            {
                return killProcessCommand ?? (killProcessCommand = new RelayCommand<object>(KillProcess, CanExecute));
            }
        }
        

        public ICommand FolderCommand
        {
            get
            {
                return folderCommand ?? (folderCommand = new RelayCommand<object>(OpenFolder, CanExecute));
            }
        }


        public ICommand SortCommand
        {
            get
            {
                return sortCommand ?? (sortCommand = new RelayCommand<object>(SortImplementation, CanSort));
            }
        }
        #endregion

        #region Constructor
        public ProcessListViewModel()
        {
            _updateThread = new Thread(RefreshGrid);
            Thread startThread = new Thread(StartProcesses);
            startThread.Start();
        }
        #endregion


        private async void RefreshGrid()
        {
            while (true)
            {
                await Task.Run(() =>
                {
                   Thread.Sleep(5000);
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        try
                        {
                            lock(ProcessesList)
                            {
                                List<ProcessListItem> itemsToRemove = ProcessesList.Where(proc => !ProcessesManager.Processes.ContainsKey(proc.ProcessID)).ToList();
                                foreach (ProcessListItem p in itemsToRemove)
                                {
                                    ProcessesList.Remove(p);
                                }

                                List<ProcessListItem> itemsToAdd = ProcessesManager.Processes.Values.Where(proc => !ProcessesList.Contains(proc)).ToList();
                                foreach (ProcessListItem p in itemsToAdd)
                                {
                                    ProcessesList.Add(p);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message); 
                        }
                    });
                });
            }
        }

        private async void StartProcesses()
        {
            await Task.Run(() =>
            {
                ProcessesList = new ObservableCollection<ProcessListItem>(ProcessesManager.Processes.Values);
            });
            _updateThread.Start();
        }
        

        private void SortImplementation(object obj)
        {
            if (SelectedProperty.Contains("Name"))
                ProcessesList = new ObservableCollection<ProcessListItem>(from i in ProcessesList orderby i.ProcessName ascending select i);
            else if (SelectedProperty.Contains("ID"))
                ProcessesList = new ObservableCollection<ProcessListItem>(from i in ProcessesList orderby i.ProcessID ascending select i);
            else if (SelectedProperty.Contains("Active"))
                ProcessesList = new ObservableCollection<ProcessListItem>(from i in ProcessesList orderby i.IsAlive ascending select i);
            else if (SelectedProperty.Contains("CPU"))
                ProcessesList = new ObservableCollection<ProcessListItem>(from i in ProcessesList orderby i.Cpu ascending select i);
            else if (SelectedProperty.Contains("RAM, MB"))
                ProcessesList = new ObservableCollection<ProcessListItem>(from i in ProcessesList orderby i.RamSize ascending select i);
            else if (SelectedProperty.Contains("RAM, %"))
                ProcessesList = new ObservableCollection<ProcessListItem>(from i in ProcessesList orderby i.RamPercents ascending select i);
            else if (SelectedProperty.Contains("Threads"))
                ProcessesList = new ObservableCollection<ProcessListItem>(from i in ProcessesList orderby i.ThreadsAmount ascending select i);
            else if (SelectedProperty.Contains("User"))
                ProcessesList = new ObservableCollection<ProcessListItem>(from i in ProcessesList orderby i.ProcessOwner ascending select i);
            else if (SelectedProperty.Contains("Path"))
                ProcessesList = new ObservableCollection<ProcessListItem>(from i in ProcessesList orderby i.FileName ascending select i);
            else if (SelectedProperty.Contains("LaunchTime"))
                ProcessesList = new ObservableCollection<ProcessListItem>(from i in ProcessesList orderby i.LaunchDateTime ascending select i);
        }

        

        private void KillProcess(object obj)
        {
                try
                {
                    foreach (Process proc in Process.GetProcessesByName(SelectedProcess.ProcessName))
                    {
                        proc.Kill();
                    }
                    foreach (var itemToRemove in ProcessesList.Where(x => x.ProcessName == SelectedProcess.ProcessName).ToList())
                    {
                        ProcessesList.Remove(itemToRemove);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
        }

        private void OpenFolder(object obj)
        {
            try
            {
                Regex regex = new Regex(@"\\(\w*\.*_*\d*){1,}\.exe");
                string path = regex.Replace(SelectedProcess.FileName, "");
                Process.Start(path);
            }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
        }


        private bool CanSort(object obj)
        {
            return SelectedProperty != null;
        }

        private bool CanExecute(object obj)
        {
            return IsProcessSelected;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
