using System;
//using System.Collections.Generic;
using System.Linq;
//using System.Text;
using System.Collections.ObjectModel;
using System.IO;
//using System.Windows.Controls;

namespace WixWPFWizardBA.Dialogs.FolderBrowser
{
    public class FolderViewModel : ViewModelBase
    {
        private bool _isSelected;
        private bool _isExpanded;
        private string _folderIcon;

        public BrowserViewModel Root
        {
            get;
            set;
        }

        public string FolderIcon
        {
            get
            {
                return _folderIcon;
            }
            set
            {
                _folderIcon = value;
                OnPropertyChanged("FolderIcon");
            }
        }

        public string FolderName
        {
            get;
            set;
        }

        public string FolderPath
        {
            get;
            set;
        }

        public ObservableCollection<FolderViewModel> Folders
        {
            get;
            set;
        }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;

                    OnPropertyChanged("IsSelected");

                    if (value)
                    {
                        // With this condition, we prevent new folders (in Root.SelectedFolder) at the end of an
                        // existing folder path (in FolderPath) from being removed from Root.SelectedFolder.
                        // Otherwise when we open the FolderBrowserDialog with a path that has new folders
                        // the new folders are removed from the path.
                        if (!Root.SelectedFolder.Contains(FolderPath))
                        {
                            Root.SelectedFolder = FolderPath; // comment-out to preserve path with one or more new folders.
                        }
                        IsExpanded = true; //Default windows behaviour of expanding the selected folder
                    }
                }
            }
        }

        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;

                    OnPropertyChanged("IsExpanded");

                    if (!FolderName.Contains(':'))//Folder icon change not applicable for drive(s)
                    {
                        if (_isExpanded)
                            FolderIcon = "Resources\\FolderOpen.png";
                        else
                            FolderIcon = "Resources\\FolderClosed.png";
                    }

                    LoadFolders();
                }

            }
        }

        private void LoadFolders()
        {
            try
            {
                if (Folders.Count > 0)
                    return;

                string[] dirs = null;

                string fullPath = Path.Combine(FolderPath, FolderName);

                if (FolderName.Contains(':'))//This is a drive
                    fullPath = string.Concat(FolderName, "\\");
                else
                    fullPath = FolderPath;

                dirs = Directory.GetDirectories(fullPath);

                Folders.Clear();

                foreach (string dir in dirs)
                {
                    try
                    {
                        DirectoryInfo di = new DirectoryInfo(dir);
                        // create the sub-structure only if this is not a hidden directory
                        if ((di.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                        {
                            Folders.Add(new FolderViewModel { Root = this.Root, FolderName = Path.GetFileName(dir), FolderPath = Path.GetFullPath(dir), FolderIcon = "Resources\\FolderClosed.png" });
                        }
                    }
                    catch (UnauthorizedAccessException ae)
                    {
                        Console.WriteLine(ae.Message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                if (FolderName.Contains(":"))
                {
                    FolderIcon = "Resources\\HardDisk.ico";
                }

                // Root.SelectedFolder = FolderPath; My addition?
            }
            catch (UnauthorizedAccessException ae)
            {
                Console.WriteLine(ae.Message); // TODO Make an error dialog.
            }
            catch (IOException ie)
            {
                Console.WriteLine(ie.Message); // TODO Make an error dialog.
            }
        }

        public FolderViewModel()
        {
            Folders = new ObservableCollection<FolderViewModel>();
        }
    }
}
