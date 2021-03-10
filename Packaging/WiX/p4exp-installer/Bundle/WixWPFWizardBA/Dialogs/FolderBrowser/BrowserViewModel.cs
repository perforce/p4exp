using System;
//using System.Collections.Generic;
using System.Linq;
//using System.Text;
using System.Collections.Generic; // List
using System.Collections.ObjectModel;
//using Prism.Commands; // DelegateCommand // From nuget package Prism.wpf.
using System.IO;

namespace WixWPFWizardBA.Dialogs.FolderBrowser
{
    public class BrowserViewModel : ViewModelBase
    {
        private string _selectedFolder;
        private string _dialogTitle;
        private string _description;
        private bool _expanding = false;

        public string DialogTitle
        {
            get
            {
                return _dialogTitle;
            }
            set
            {
                _dialogTitle = value;
                OnPropertyChanged("DialogTitle"); // This results in it being displayed.
            }
        }


        // TODO Is the description needed?
        public string Description
        {
            get
            {
                return _description;
            }
            set {
                _description = value;
                OnPropertyChanged("Description"); // This results in it being displayed.
            }
        }
        public string SelectedFolder
        {
            get
            {
                return _selectedFolder;
            }
            set
            {
                _selectedFolder = value;
                OnPropertyChanged("SelectedFolder");
                OnSelectedFolderChanged();
            }
        }

        public ObservableCollection<FolderViewModel> Folders
        {
            get;
            set;
        }
#if false
        public DelegateCommand<object> FolderSelectedCommand
        {
            get
            {
                return new DelegateCommand<object>(it => SelectedFolder = Environment.GetFolderPath((Environment.SpecialFolder)it));
            }
        }
#endif
        
        public BrowserViewModel()
        {
            Folders = new ObservableCollection<FolderViewModel>();

            // Exclude drive types we don't want.
            List<System.IO.DriveInfo> driveInfos = System.IO.DriveInfo.GetDrives().ToList();
            driveInfos.RemoveAll(driveInfo => driveInfo.DriveType == DriveType.Network ||
                                              driveInfo.DriveType == DriveType.Removable ||
                                              driveInfo.DriveType == DriveType.CDRom);

            List<string> drives = new List<string>();
            foreach (System.IO.DriveInfo driveInfo in driveInfos)
            {
                drives.Add(driveInfo.Name);
            }

            drives.ForEach(it => Folders.Add(new FolderViewModel
                { Root = this, FolderPath = it.TrimEnd('\\'), FolderName = it.TrimEnd('\\'), FolderIcon = "Resources\\HardDisk.ico" }));
        }

        private void OnSelectedFolderChanged()
        {
            if (!_expanding)
            {
                try
                {
                    FolderViewModel child = Expand(Folders, SelectedFolder);
                    if (null != child)
                    { 
                        child.IsSelected = true;
                        _expanding = true;
                    }
                }
                finally
                {
                    _expanding = false;
                }
            }
        }

        private FolderViewModel Expand(ObservableCollection<FolderViewModel> childFolders, string path)
        {
            if (String.IsNullOrEmpty(path) || childFolders.Count == 0)
            {
                return null;
            }

            string folderName = path;
            if (path.Contains('/') || path.Contains('\\'))
            {
                int idx = path.IndexOfAny(new char[] { '/', '\\' });
                folderName = path.Substring(0, idx);
                path = path.Substring(idx + 1);
            }
            else
            {
                path = null;
            }

            var results = childFolders.Where<FolderViewModel>(folder => folder.FolderName == folderName);
            if (results != null && results.Count() > 0)
            {
                FolderViewModel fvm = results.First();
                fvm.IsExpanded = true;
                
                var retVal = Expand(fvm.Folders, path);
                if (retVal != null)
                {
                    return retVal;
                }
                else
                {
                    return fvm;
                }
            }

            return null;
        }
    }
}
