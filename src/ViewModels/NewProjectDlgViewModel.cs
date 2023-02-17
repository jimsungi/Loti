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


        private DelegateCommand? _selectFolderCmd = null;
        public DelegateCommand SelectFolderCmd =>
            _selectFolderCmd ??= new DelegateCommand(SelectFolderCmdFunc);
        void SelectFolderCmdFunc()
        {
            VistaFolderBrowserDialog od = new();
            var res = od.ShowDialog();
            if (res == true)
            {
                ProjectPath = od.SelectedPath.ToString();
            }
        }


        private DelegateCommand? _selectTargetFolderCmd = null;
        public DelegateCommand SelectTargetFolderCmd =>
            _selectTargetFolderCmd ??= new DelegateCommand(SelectTargetFolderCmdFunc);
        void SelectTargetFolderCmdFunc()
        {
            VistaFolderBrowserDialog od = new();
            var res = od.ShowDialog();
            if (res == true)
            {
                TargetPath= od.SelectedPath.ToString();
            }
        }

        private DelegateCommand? _createProjectCmd = null;

        public DelegateCommand CreateProjectCmd =>
            _createProjectCmd ??= new DelegateCommand(CreateProjectFunc);

        string IDialogAware.Title => throw new NotImplementedException();

        void CreateProjectFunc()
        {
            if (string.IsNullOrWhiteSpace(ProjectName))
            {
                MessageBox.Show("ProjectName is required");
                return;
            }
            if (string.IsNullOrWhiteSpace(ProjectPath))
            {
                MessageBox.Show("ProjectPath is required");
                return;
            }

            ProjectManageService.CreateProject(ProjectName, ProjectPath, TargetPath)
                .SetCurrent().Save();
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
    }
}
