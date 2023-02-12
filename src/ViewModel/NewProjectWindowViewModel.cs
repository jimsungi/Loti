using System;
using System.Windows.Controls;
using Prism.Commands;
using Prism.Mvvm;

namespace TigerL10N.ViewModel;

public class NewProjectWindowViewModel : BindableBase
{

    private string? _projectFolder;
    public string ProjectFolder
    {
        get => _projectFolder ??= "";
        set => SetProperty(ref _projectFolder, value);
    }



    private string? _projectPath;
    public string ProjectPath
    {
        get => _projectPath ??= "";
        set => SetProperty(ref _projectPath, value);
    }

    private DelegateCommand? _createFolderCmd = null;
    public DelegateCommand CreateFolderCmd =>
        _createFolderCmd ??= new DelegateCommand(CreateFolderFunc);
    void CreateFolderFunc()
    {
        // throw new NotImplementException();
    }


    private DelegateCommand? _createProjectCmd = null;
    public DelegateCommand CreateProjectCmd =>
        _createProjectCmd ??= new DelegateCommand(CreateProjectFunc);
    void CreateProjectFunc()
    {
        // throw new NotImplementException();
    }

}