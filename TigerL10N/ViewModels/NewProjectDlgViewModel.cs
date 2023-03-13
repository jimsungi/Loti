using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ookii.Dialogs.Wpf;
using Prism.Mvvm;
using TigerL10N.Service;
using System.IO;
using TigerL10N.Biz;
using System.Globalization;
using TigerL10N.Utils;
using MahApps.Metro.IconPacks;

namespace TigerL10N.ViewModels
{
    public class NewProjectDlgViewModel :BindableBase, IDialogAware
    {

        public NewProjectDlgViewModel()
        {
            

      
        }

        public event Action<IDialogResult>? RequestClose;
        protected virtual void CloseDialog(string parameter)
        {
            ButtonResult result = ButtonResult.None;

            if (parameter?.ToLower() == "true")
                result = ButtonResult.OK;
            else if (parameter?.ToLower() == "false")
                result = ButtonResult.Cancel;

            RaiseRequestClose(new Prism.Services.Dialogs.DialogResult(result));
        }

        private string? _projectName;

        public string ProjectName
        {
            get => _projectName ??= "";
            set => SetProperty(ref _projectName, value);
        }

        private string? _projectPath;

        public string ProjectPath
        {
            get => _projectPath ??= "";
            set => SetProperty(ref _projectPath, value);
        }

        private string? _targetPath;

        public string TargetPath
        {
            get => _targetPath ??= "";
            set => SetProperty(ref _targetPath, value);
        }

        private DelegateCommand? _createFolderCmd = null;

        public DelegateCommand CreateFolderCmd =>
            _createFolderCmd ??= new DelegateCommand(CreateFolderFunc);

        void CreateFolderFunc()
        {
            // throw new NotImplementException();
        }


        //private DelegateCommand? _selectFolderCmd = null;
        //public DelegateCommand SelectFolderCmd =>
        //    _selectFolderCmd ??= new DelegateCommand(SelectFolderCmdFunc);
        //void SelectFolderCmdFunc()
        //{
        //    VistaFolderBrowserDialog od = new();
        //    var res = od.ShowDialog();
        //    if (res == true)
        //    {
        //        ProjectPath = od.SelectedPath.ToString();
        //    }
        //}


        private DelegateCommand? _selectSolutionCmd = null;
        public DelegateCommand SelectSolutionCmd =>
            _selectSolutionCmd ??= new DelegateCommand(SelectSolutionFunc);
        void SelectSolutionFunc()
        {
            //// throw new NotImplementException();
            //VistaFolderBrowserDialog od = new();
            //var res = od.ShowDialog();
            //if (res == true)
            //{
            //    ProjectPath = od.SelectedPath.ToString();
            //}
            VistaOpenFileDialog of = new();
            var res = of.ShowDialog();
            if (res == true)
            {
                if(Solution !=null)
                {

                    string ProjectPath = of.FileName.ToString();
                    string filename = Path.GetFileName(of.FileName);
                    string ProjectName = filename.Substring(0, filename.Length - 4);
                    string L10NProjectPath = ProjectPath.Substring(0, ProjectPath.Length - 4);
                    L10NProjectPath = L10NProjectPath + ".ln";
                    Solution.VsSolutionPath= ProjectPath;
                    Solution.FilePath = L10NProjectPath;
                    Solution.FileTitle = filename.Substring(0, filename.Length - 4);
                }
            }
        }


        private string? _l10NProjectPath;
        public string L10NProjectPath
        {
            get => _l10NProjectPath ??= "";
            set => SetProperty(ref _l10NProjectPath, value);
        }



        //private DelegateCommand? _selectTargetFolderCmd = null;
        //public DelegateCommand SelectTargetFolderCmd =>
        //    _selectTargetFolderCmd ??= new DelegateCommand(SelectTargetFolderCmdFunc);
        //void SelectTargetFolderCmdFunc()
        //{
        //    VistaFolderBrowserDialog od = new();
        //    var res = od.ShowDialog();
        //    if (res == true)
        //    {
        //        TargetPath= od.SelectedPath.ToString();
        //    }
        //}

        private DelegateCommand? _createProjectCmd = null;

        public DelegateCommand CreateProjectCmd =>
            _createProjectCmd ??= new DelegateCommand(CreateProjectFunc);

        string IDialogAware.Title
        {
            get => "GoGo";
        }


        private String? _title;
        public String Title
        {
            get => _title ??= "New Project";
            set => SetProperty(ref _title, value);
        }


        void CreateProjectFunc()
        {
            if (string.IsNullOrWhiteSpace(ProjectPath) && File.Exists(ProjectPath))
            {
                MessageBox.Show("Select Solution file");
                return;
            }

            if (Solution != null)
            {
                Solution.Save();
                ProjectManageService.Solution = Solution;
            }
            //ProjectManageService.CreateSolution(ProjectPath);

            //ProjectManageService.CreateLproject(ProjectName, ProjectPath, TargetPath)
            //    .SetCurrent().Save();
            ButtonResult result = ButtonResult.OK;
            RaiseRequestClose(new Prism.Services.Dialogs.DialogResult(result));
        }

        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        bool IDialogAware.CanCloseDialog()
        {
            return true;
        }

        void IDialogAware.OnDialogClosed()
        {
        }

        public string? Message { set; get; }
        void IDialogAware.OnDialogOpened(IDialogParameters parameters)
        {
            Message = parameters.GetValue<string>("message");
        }


        private bool? _optionOverwrite;
        public bool OptionOverwrite
        {
            get => _optionOverwrite ??= false;
            set => SetProperty(ref _optionOverwrite, value);
        }


        private bool? _optionForceNamespace;
        public bool OptionForceNamespace
        {
            get => _optionForceNamespace ??= false;
            set => SetProperty(ref _optionForceNamespace, value);
        }


        private string? _defaultNamespace;
        public string DefaultNamespace
        {
            get => _defaultNamespace ??= "";
            set => SetProperty(ref _defaultNamespace, value);
        }


        private int? _optionResourceType=0;
        public int? OptionResourceType
        {
            get => _optionResourceType;
            set => SetProperty(ref _optionResourceType, value);
        }


        private LSolution? _solution = new LSolution();
        public LSolution? Solution
        {
            get => _solution;
            set => SetProperty(ref _solution, value);
        }

    }
}
