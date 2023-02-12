using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Prism.Commands;
using Prism.Mvvm;

namespace TigerL10N.ViewModel
{
    public  class MainWindowViewModel : BindableBase
    {
        private DelegateCommand? _newProjectCommandCmd = null;
        public DelegateCommand NewProjectCommandCmd =>
            _newProjectCommandCmd ??= new DelegateCommand(NewProjectCommandFunc);
        void NewProjectCommandFunc()
        {
            // throw new NotImplementException();
        }

        private DelegateCommand? _saveProjectCommandCmd = null;
        public DelegateCommand SaveProjectCommandCmd =>
            _saveProjectCommandCmd ??= new DelegateCommand(SaveProjectCommandFunc);
        void SaveProjectCommandFunc()
        {
            // throw new NotImplementException();
        }


        private DelegateCommand? _closeProjectCommandCmd = null;
        public DelegateCommand CloseProjectCommandCmd =>
            _closeProjectCommandCmd ??= new DelegateCommand(CloseProjectCommandFunc);
        void CloseProjectCommandFunc()
        {
            // throw new NotImplementException();
        }
    }
}
