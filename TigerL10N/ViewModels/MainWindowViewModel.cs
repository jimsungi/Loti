using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using TigerL10N.Control;
using TigerL10N.Service;
using TigerL10N.Views;
using TigerL10N.Biz;
using Unity;
using System.Windows.Controls;
using System.Windows;
using TigerL10N.Utils;

namespace TigerL10N.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region property
        private string? _baGo="";
        public string BaGo
        {
            get => _baGo ??= "";
            set => SetProperty(ref _baGo, value);
        }

        



        private List<GoItemNode>? _sourceTreeData;
        public List<GoItemNode>? SourceTreeData
        {
            get => _sourceTreeData;
            set => SetProperty(ref _sourceTreeData, value);
        }


        // Per Stage Localization Source Words
        private List<WordItem>? _localizationSource;
        public List<WordItem>? LocalizationSource
        {
            get => _localizationSource;
            set => SetProperty(ref _localizationSource, value);
        }


        /// <summary>
        /// Selected Index of selected word to translate
        /// </summary>

        private int? _localizedWord=0;
        public int? LocalizedWord
        {
            get => _localizedWord;
            set
            {
                SetProperty(ref _localizedWord, value);
                ShowSelected();             
            }
        }


        private WordItem? _selectedWord;
        public WordItem? SelectedWord
        {
            get => _selectedWord;
            set => SetProperty(ref _selectedWord, value);
        }


        void ShowSelected()
        {
            if(Solution !=null && Solution.CurrentProject!=null && Solution.CurrentProject.Words!=null && LocalizedWord >= 0)
            {
                if(Solution.CurrentProject.Words.Count >LocalizedWord)
                    SelectedWord = Solution.CurrentProject.Words[LocalizedWord.Value];
            }
            else
            {
                SelectedWord = null;
            }
        }

        private List<GoItemNode>? _targetTreeData;
        public List<GoItemNode>? TargetTreeData
        {
            get => _targetTreeData;
            set => SetProperty(ref _targetTreeData, value);
        }


        private string? _projectMenuName = "Project";

        public string ProjectMenuName
        {
            get => _projectMenuName ??= "";
            set => SetProperty(ref _projectMenuName, value);
        }
        #endregion
        #region Delegate Command
        private DelegateCommand? _newProjectCommandCmd = null;

        public DelegateCommand NewProjectCmd =>
            _newProjectCommandCmd ??= new DelegateCommand(NewProjectCommandFunc);



        private DelegateCommand? _saveTranslationAllCmd = null;
        public DelegateCommand SaveTranslationAllCmd =>
            _saveTranslationAllCmd ??= new DelegateCommand(SaveTranslationAllFunc);
        void SaveTranslationAllFunc()
        {
            // throw new NotImplementException();
        }

        private DelegateCommand? _saveTranslationCmd = null;
        public DelegateCommand SaveTranslationCmd =>
            _saveTranslationCmd ??= new DelegateCommand(SaveTranslationFunc);
        void SaveTranslationFunc()
        {
            // throw new NotImplementException();
            /// 1. Backup
            /// Copy From Source Project
            /// 
            if (this.Solution != null && this.Solution.CurrentProject !=null)
            {
                LProject project = Solution.CurrentProject;
                //List<WordItem> saveWords = LocalizationSource.OrderBy(w => w.FileName).ToList();
                string tmpFileName = "";
                string targetFileName = "";
                string contents = "";

                // Read All text from file
                List<WordItem>? saveWords = Solution.CurrentProject.Words;
                if (saveWords != null)
                {
                    Dictionary<string, string> fileNameContentBuf = new Dictionary<string, string>();
                    foreach (WordItem eachWord in saveWords)
                    {
                        tmpFileName = eachWord.TmpFile;
                        if(!fileNameContentBuf.Keys.Contains(tmpFileName))
                        {
                            contents = File.ReadAllText(tmpFileName);
                            fileNameContentBuf.Add(tmpFileName, contents);
                        }
                    }

                    foreach (WordItem eachWord in saveWords)
                    {
                        tmpFileName = eachWord.TmpFile;
                        contents = fileNameContentBuf[tmpFileName];


                        if (eachWord.Ignore == true)
                        {
                            fileNameContentBuf[tmpFileName] = contents.Replace(eachWord.TargetId, eachWord.SourceString);
                        }
                        else if (eachWord.AsId)
                        {
                            fileNameContentBuf[tmpFileName] = contents.Replace(eachWord.TargetId, eachWord.FinalId);
                        }
                        else
                        {
                            fileNameContentBuf[tmpFileName] = contents.Replace(eachWord.TargetId, eachWord.FinalId);
                        }
                        // eachWord.TargetId => TargetString으로 바꾼다.
                    }

                    foreach(KeyValuePair<string,string> fileAndContens in fileNameContentBuf)
                    {
                        string fileaneme = fileAndContens.Key;
                        string fContent = fileAndContens.Value;
                        targetFileName = "";
                        if (!string.IsNullOrWhiteSpace(tmpFileName))
                        {
                            if (tmpFileName.EndsWith(".ltmp"))
                            {
                                targetFileName = fileaneme.Substring(0, fileaneme.Length - 5);
                                if(!string.IsNullOrWhiteSpace(targetFileName))
                                {
                                    File.WriteAllText(targetFileName, fContent);
                                }
                            }
                        }
                    }
                }
                LocService.CreateNewDesignerFile(project);
                LocService.CreateNewResxFile(project);
                LocService.CreateNewIdFile(project);
            }
        }


        private string message = "......";
        void NewProjectCommandFunc()
        {
            //BaGo = "dddddxx";
            _dialogService.ShowDialog("NewProject", new DialogParameters($"message={message}"), result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    SourceTreeData = ProjectManageService.ListSolutionTree(ProjectManageService.Solution, ProjectManageService.Solution.FolderPath, false);
                    Solution = ProjectManageService.Solution;
                    if (Solution.Projects?.Count > 0)
                    {
                        Solution.CurrentProject = Solution.Projects.First();
                        foreach (LProject project in Solution.Projects)
                        {
                            project.BuildFileLang();
                            project.BuildWords();


                        }

                        //List<WordItem> sortedByName = Solution.CurrentProject.Words.OrderBy(w => w.SourceString).ToList();
                        //this.LocalizationSource = sortedByName;
                        //foreach (WordItem eachWord in sortedByName)
                        //{
                        //    eachWord.init = true;
                        //    eachWord.RefAll = sortedByName;
                        //}
                    }

                    //SrcFiles = CheckFileTreeModel.gettest();
                    // load 
                    //SrcFiles = CheckFileTreeModel.SetTree("ddd");
                    //TransFiles = CheckFileTreeModel.SetTree("ddd");
                }
            });
        }
        #endregion




        #region Service
        private IContainerProvider _ic;
    //    private IContainerRegistry _icRegistry;
        private IDialogService _dialogService;
        #endregion

        public MainWindowViewModel(IContainerProvider ic, IDialogService dialogService)
        {
            _ic = ic;
//            _icRegistry = ir;
            _dialogService = dialogService;
            OpenLastProjectFunc();
  //          _icRegistry.RegisterDialog<NewProjectDialog, NewProjectDialogViewModel>("NewProject");
        }

        #region Delegate Command
        private DelegateCommand? _saveProjectCmd = null;
        public DelegateCommand SaveProjectCmd =>
            _saveProjectCmd ??= new DelegateCommand(SaveProjectFunc);
        void SaveProjectFunc()
        {
            // throw new NotImplementException();
        }


        private DelegateCommand? _closeProjectCmd = null;
        public DelegateCommand CloseProjectCmd =>
            _closeProjectCmd ??= new DelegateCommand(CloseProjectFunc);
        void CloseProjectFunc()
        {
            // throw new NotImplementException();
        }

        /// <summary>
        /// Current Solution
        /// you can get by ProjectManageService.Solution
        /// </summary>
        private LSolution? _solution;
        public LSolution? Solution
        {
            get => _solution;
            set => SetProperty(ref _solution, value);
        }


        private DelegateCommand? _openProjectCmd = null;
        public DelegateCommand OpenProjectCmd =>
            _openProjectCmd ??= new DelegateCommand(OpenProjectFunc);
        void OpenProjectFunc()
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string project_file_name = dlg.FileName;
                LProject? p = ProjectManageService.LoadFromXml(project_file_name);
                if (p != null)
                {
                    TargetTreeData = ProjectManageService.ListFileTree(p, p.RawPath, IsTargetChecked);
                    SourceTreeData = ProjectManageService.ListFileTree(null, p.ProjectPath, false);
                    LProject.Current = p;
                }
            }
            // throw new NotImplementException();
        }

        void OpenLastProjectFunc()
        {
            string last_l18N_file = AppConfigService.Settings.LastL18NFile;
            if (File.Exists(last_l18N_file))
            {

                LProject? p = ProjectManageService.LoadFromXml(last_l18N_file);
                if (p != null)
                {
                    p.Open();
                    TargetTreeData = ProjectManageService.ListFileTree(p, p.RawPath, IsTargetChecked);
                    ProjectName = p.ProjectName;
                    ProjectPath = p.ProjectPath;
                    RawPath = p.RawPath;
                    LProject.Current = p;

                    SourceTreeData = ProjectManageService.ListFileTree(null, p.ProjectPath, false);
                }
            }
        }



        private DelegateCommand? _moveNextCmd = null;
        public DelegateCommand MoveNextCmd =>
            _moveNextCmd ??= new DelegateCommand(MoveNextCmdFunc);
        void MoveNextCmdFunc()
        {
            if (SelectedWord != null)
            {
                SelectedWord.TargetString = SelectedWord.TargetString;
            }
            LocalizedWord += 1;
            //SelectedWord = SelectedWord + 1;
            // throw new NotImplementException();
        }


        private DelegateCommand? _movePrevCmd = null;
        public DelegateCommand MovePrevCmd =>
            _movePrevCmd ??= new DelegateCommand(MovePrevFunc);
        void MovePrevFunc()
        {
            if(LocalizedWord > 0)
                LocalizedWord -= 1;
            // throw new NotImplementException();
        }


        private DelegateCommand? _asIdCmd = null;
        public DelegateCommand AsIdCmd =>
            _asIdCmd ??= new DelegateCommand(AsIdFunc);
        void AsIdFunc()
        {
            // throw new NotImplementException();
        }



        private DelegateCommand? _ignoreCmd = null;
        public DelegateCommand IgnoreCmd =>
            _ignoreCmd ??= new DelegateCommand(IgnoreFunc);
        void IgnoreFunc()
        {

            // throw new NotImplementException();
        }


        private DelegateCommand? _applyAllCmd = null;
        public DelegateCommand ApplyAllCmd =>
            _applyAllCmd ??= new DelegateCommand(ApplyAllFunc);
        void ApplyAllFunc()
        {
            if (SelectedWord != null)
            {
                WordItem sItem = SelectedWord;
                if (LocalizationSource != null)
                {
                    foreach (WordItem eachWord in LocalizationSource)
                    {
                        //if (eachWord != sItem)
                        {
                            if(sItem.SourceString == eachWord.SourceString)
                            {
                                eachWord.Dirty = true;
                                eachWord.TargetId = sItem.TargetId;
                                eachWord.TargetString = sItem.TargetString;
                                eachWord.FinalId = sItem.FinalId;
                                eachWord.DupIdCount = sItem.DupIdCount;
                                eachWord.UseAuto = sItem.UseAuto;
                                eachWord.Ignore = sItem.Ignore;
                                eachWord.AsId = sItem.AsId;
                            }
                        }
                    }
                }
            }
           
            // throw new NotImplementException();
        }


        public void MyUserControl_My(object sender, RoutedEventArgs e)
        {
            MyEventArgs? args = e as MyEventArgs;
            if (args != null)
            {
                //messageTextBlock.Text = args.Message;
            }
        }

        private DelegateCommand? _closeProjectCommandCmd = null;

        public DelegateCommand CloseProjectCommandCmd =>
            _closeProjectCommandCmd ??= new DelegateCommand(CloseProjectCommandFunc);

        void CloseProjectCommandFunc()
        {
            // throw new NotImplementException();
        }


        private DelegateCommand? _clearTargetSetsCmd = null;
        public DelegateCommand ClearTargetSetsCmd =>
            _clearTargetSetsCmd ??= new DelegateCommand(ClearTargetSetsFunc);
        void ClearTargetSetsFunc()
        {
            // throw new NotImplementException();
        }


        private DelegateCommand? _saveTargetSetsCmd = null;
        public DelegateCommand SaveTargetSetsCmd =>
            _saveTargetSetsCmd ??= new DelegateCommand(SaveTargetSetsFunc);
        void SaveTargetSetsFunc()
        {
            // throw new NotImplementException();
        }

        #endregion

        #region property
        private bool? _isTargetChecked=false;
        public bool IsTargetChecked
        {
            get => _isTargetChecked ??= false;
            set {
                LProject? c = ProjectManageService.GetCurrentProject();
                if (c != null)
                {
                    TargetTreeData = ProjectManageService.ListFileTree(c, c.RawPath, value);
                }
                SetProperty(ref _isTargetChecked, value); 
            }
        }




        private string? _projectName;
        public string ProjectName
        {
            get => _projectName ??= "";
            set => SetProperty(ref _projectName, value);
        }



        private string? _rawPath;
        public string RawPath
        {
            get => _rawPath ??= "";
            set => SetProperty(ref _rawPath, value);
        }



        private string? _projectPath;
        public string ProjectPath
        {
            get => _projectPath ??= "";
            set => SetProperty(ref _projectPath, value);
        }
        #endregion

        #region Translation Commands

        /// <summary>
        /// Create All files to ready to translate
        /// 
        /// Source Files -> OneLanguage File -> Multi Language File -> All Translations files
        /// 
        /// </summary>
        private DelegateCommand? _createAutoPrepareFilesCmd = null;
        public DelegateCommand CreateAutoPrepareFilesCmd =>
            _createAutoPrepareFilesCmd ??= new DelegateCommand(CreateAutoPrepareFilesFunc);
        void CreateAutoPrepareFilesFunc()
        {
            LProject? cu = ProjectManageService.GetCurrentProject();
            if(cu == null)
            {
                if(System.Windows.Forms.MessageBox.Show("You need project. Do you want to create one?", "Warning", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) 
                {
                    NewProjectCommandFunc();
                }
                return;
            }
            //CreateFolders();
            CreateFiles(cu);
            LocalizedWord = 0;
            //LoadFiles();
        }

        void CreateFiles(LProject project)
        {
            /// 1. Backup
            /// Copy From Source Project
            /// 
            string RawPath = project.RawPath;
            LProject.Gindex = 0;
            if(Directory.Exists(RawPath))
            {
                if (project.BackUp)
                {
                    string BackPath = project.BackupPath + Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyyMMdd_HHmmss").ToString();
                    Directory.CreateDirectory(BackPath);
                    IgnoreOption Option = new IgnoreOption();
                    Etc.CopyDirectory(RawPath, BackPath, Option);
                }
                LProject.ProcessOption PO = new LProject.ProcessOption();
                PO.IsPrepare = true;
                project.DirOneFileProc(RawPath, project.OneLangPath, PO);
                PO.IsPrepare = false;
                project.DirOneFileProc(RawPath, project.OneLangPath, PO);

                project.Words = project.BuildWords();
                List<WordItem> sortedByName = project.Words.OrderBy(w => w.SourceString).ToList();
                this.LocalizationSource = sortedByName;
                foreach(WordItem eachWord in sortedByName)
                {
                    eachWord.init = true;
                    eachWord.RefAll = sortedByName;
                }
                LocService.IdKey.Clear();
                LocService.StringKey.Clear();

            }
            //Directory.CreateDirectory(RawPath);



        }


        /// <summary>
        /// Create All files to ready to translate
        /// 
        /// Source Files -> One Languate File -> Multi Language File -> All Translations Files
        /// 
        /// 
        /// </summary>

        private DelegateCommand? _createEffectPrepareFilesCmd = null;
        public DelegateCommand CreateEffectPrepareFilesCmd =>
            _createEffectPrepareFilesCmd ??= new DelegateCommand(CreateEffectPrepareFilesFunc);
        void CreateEffectPrepareFilesFunc()
        {
            // throw new NotImplementException();
        }


        private DelegateCommand? _applyTranslateFilesCmd = null;
        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand ApplyTranslateFilesCmd =>
            _applyTranslateFilesCmd ??= new DelegateCommand(ApplyTranslateFilesFunc);
        void ApplyTranslateFilesFunc()
        {
            // throw new NotImplementException();
        }


        private DelegateCommand? _makeMultiLanguageVersionCmd = null;
        /// <summary>
        /// MultiLanguage version is created from OneLanguage Version
        /// If you create multi language version - means contains all multilanguage resource, and all multilanguage function -
        /// Loosely, you can say
        /// MultiLanguage version = OneLanguage version + resource function.cs + resource file
        /// 
        /// This functions means
        /// From the translated resources ==> Apply all translation ==> Create Target Multilangugae version
        /// 
        /// Then you can build MultiLangugge Version then Deploy
        /// 
        /// </summary>
        public DelegateCommand MakeMultiLanguageVersionCmd =>
            _makeMultiLanguageVersionCmd ??= new DelegateCommand(MakeMultiLanguageVersionFunc);
        void MakeMultiLanguageVersionFunc()
        {
            //Step 0
            // Create Folders


            // Find project file
            // 

            // Insert resource relating files
            // {projectname}Res.Design.cs
            // {projectname}L10N.res

            // Create Target resource file
            // {projectname}L10N.en.res
            // {projectname}L10N.ko.res

            // throw new NotImplementException();
        }

        private DelegateCommand? _makeOneLanguageVersionCmd = null;
        /// <summary>
        /// In Normal, Initial version is the OneLanguage Version.
        /// One Language Version contains contains only one language version
        /// and has no multi language script. 
        /// 
        /// This function means
        /// From the MultiLangugae Version, Create One Language Version
        /// 
        /// If need there would be shortage script G("") for compatablility with multi-language vs one-language
        /// L(""), S(""),D(""),G(""),M("")
        /// 
        /// </summary>
        public DelegateCommand MakeOneLanguageVersionCmd =>
            _makeOneLanguageVersionCmd ??= new DelegateCommand(MakeOneLanguageVersionFunc);
        void MakeOneLanguageVersionFunc()
        {
            LProject? cu = ProjectManageService.GetCurrentProject();
            if (cu == null)
            {
                if (System.Windows.Forms.MessageBox.Show("You need project. Do you want to create one?", "Warning", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    NewProjectCommandFunc();
                }
                return;
            }
            //CreateFolders();
            SaveFiles(cu);
            LocalizedWord = 0;
            // throw new NotImplementException();
        }

        void SaveFiles(LProject project)
        {
            /// 1. Backup
            /// Copy From Source Project
            /// 
            if (this.LocalizationSource != null)
            {
                List<WordItem> saveWords = LocalizationSource.OrderBy(w => w.FileName).ToList();
                string tmpFileName = "";
                string targetFileName = "";
                string contents = "";
                foreach (WordItem eachWord in saveWords)
                {
                    if(string.IsNullOrWhiteSpace(tmpFileName) || tmpFileName != eachWord.TmpFile)
                    {
                        if(!string.IsNullOrWhiteSpace(tmpFileName))
                        {                            
                            File.WriteAllText(targetFileName, contents);
                        }
                        tmpFileName = eachWord.TmpFile;
                        if (tmpFileName.Contains(".ltmp"))
                        {
                            targetFileName = tmpFileName.Substring(0, tmpFileName.Length - 5);
                            contents = File.ReadAllText(tmpFileName);
                        }
                    }

                    if (eachWord.Ignore == true)
                    {
                        contents = contents.Replace(eachWord.TargetId, eachWord.SourceString);

                    }
                    else if (eachWord.AsId)
                    {
                        contents = contents.Replace(eachWord.TargetId, eachWord.FinalId);
                    }
                    else if (!eachWord.AsId)
                    {
                        contents = contents.Replace(eachWord.TargetId, eachWord.FinalId);
                    }
                    // eachWord.TargetId => TargetString으로 바꾼다.
                }
                if (!string.IsNullOrWhiteSpace(tmpFileName))
                {
                    File.WriteAllText(targetFileName, contents);
                }

            }
            //Directory.CreateDirectory(RawPath);
            if(this.LocalizationSource!=null)
                project.Words = this.LocalizationSource;
            LocService.CreateNewDesignerFile(project);
            LocService.CreateNewResxFile(project);
            LocService.CreateNewIdFile(project);
        }
        #endregion Translation Commands
    }


}
