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
using System.Xml.Serialization;
using System.Diagnostics;
using System.Security.Policy;
using System.Windows.Threading;

namespace TigerL10N.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Constructor
        public MainWindowViewModel(IContainerProvider ic, IDialogService dialogService)
        {
            //_ic = ic;
            _dialogService = dialogService;
            LoadLastest();
        }
        #endregion

        #region View Visible Props


        private bool? _isTranVisible;
        public bool IsTranVisible
        {
            get => _isTranVisible ??= false;
            set => SetProperty(ref _isTranVisible, value);
        }


        private bool? _isResourceVisible;
        public bool IsResourceVisible
        {
            get => _isResourceVisible ??= false;
            set => SetProperty(ref _isResourceVisible, value);
        }


        private bool? _isSolutionVisible;
        public bool IsSolutionVisible
        {
            get => _isSolutionVisible ??= false;
            set => SetProperty(ref _isSolutionVisible, value);
        }


        private bool? _isProjectVisible;
        public bool IsProjectVisible
        {
            get => _isProjectVisible ??= false;
            set => SetProperty(ref _isProjectVisible, value);
        }




        private bool? _isInfoVisible;
        public bool IsInfoVisible
        {
            get => _isInfoVisible ??= false;
            set => SetProperty(ref _isInfoVisible, value);
        }




        private bool? _isLangVisible;
        public bool IsLangVisible
        {
            get => _isLangVisible ??= false;
            set => SetProperty(ref _isLangVisible, value);
        }




        private bool? _isLogVisible;
        public bool IsLogVisible
        {
            get => _isLogVisible ??= false;
            set => SetProperty(ref _isLogVisible, value);
        }


        #endregion

        #region property

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

        private int? _localizedWord = 0;
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
            if (Solution != null && Solution.CurrentProject != null && Solution.CurrentProject.Words != null && LocalizedWord >= 0)
            {
                if (Solution.CurrentProject.Words.Count > LocalizedWord)
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
            AddLog("", "not implemented SaveTranslationAllFunc");
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
            if (this.Solution != null && this.Solution.CurrentProject != null)
            {
                LProject project = Solution.CurrentProject;
                if (project.Solution == null)
                    project.Solution = Solution;
                //List<WordItem> saveWords = LocalizationSource.OrderBy(w => w.FileName).ToList();
       
                AddLog("", "Process +" + project.ProjectName);
                project.SaveTranslation();
                
                LocService.CreateNewDesignerFile(project);
                AddLog("", "LocService.CreateNewDesignerFile");
                LocService.CreateNewResxFile(project);
                AddLog("", "LocService.CreateNewResxFile");
                LocService.CreateNewIdFile(project);
                AddLog("", "LocService.CreateNewIdFile");
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
                    LSolution solution = ProjectManageService.Solution;
                    solution.LogFunc = LogFunc;
                    SourceTreeData = ProjectManageService.ListSolutionTree(solution, false, true);
                    AddLog("", "Show source tree : ");

                    ProjectManageService.Solution.InitBuild();
                    AddLog("", "Init Build : ");
                    Solution = ProjectManageService.Solution;
                    AddLog("", "Set Solution : ");
                    Solution.SaveStream();
                    AddLog("", "Save Solution Stream : ");
                }
            });
        }
        #endregion


        private void LoadLastest()
        {
            string translation_filename = AppConfigService.Settings.LastI18NFilePath;
            if (File.Exists(translation_filename))
            {
                if (translation_filename != null)
                {
                    string? translation_folder = Path.GetDirectoryName(translation_filename);
                    AddLog("", "Found Latest translation : " + translation_filename);
                    if (translation_folder != null)
                    {
                        string xml_file = Path.Combine(translation_folder, ".ln", "translation", "solution.xml");
                        if (File.Exists(xml_file))
                        {
                            try
                            {
                                LSolution? s = LSolution.LoadStream(xml_file, LogFunc);
                                if (s != null)
                                {
                                    if (s.Projects != null)
                                    {
                                        foreach (LProject p in s.Projects)
                                        {
                                            p.Solution = s;
                                        }
                                    }
                                    SourceTreeData = ProjectManageService.ListSolutionTree(s, false, false);
                                    s.SelectedLang = s.SelectedLang;
                                    s.CurrentProject = s.Projects[0];
                                    Solution = s;
                                }
                                AddLog("", "Load Latest translation from " + xml_file);
                            }
                            catch (Exception e)
                            {
                                AddLog("", "Load Latest translation : " + e.Message);
                            }
                        }
                    }
                }
            }
        }


        private DelegateCommand? _saveAsProjectCmd = null;
        public DelegateCommand SaveAsProjectCmd =>
            _saveAsProjectCmd ??= new DelegateCommand(SaveAsProjectFunc);
        void SaveAsProjectFunc()
        {
            AddLog("", "not implemented SaveAsProjectFunc");
        }



        #region Service
        //private IContainerProvider _ic;
        //    private IContainerRegistry _icRegistry;
        private IDialogService _dialogService;
        #endregion



        #region Delegate Command
        private DelegateCommand? _saveProjectCmd = null;
        public DelegateCommand SaveProjectCmd =>
            _saveProjectCmd ??= new DelegateCommand(SaveProjectFunc);
        void SaveProjectFunc()
        {
            if (Solution != null)
                Solution.SaveStream();
            AddLog("", "Save Solution stream");
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
            dlg.Filter = "Translation Project Files (*.ln)|*.ln|All Files (*.*)|*.*";
            dlg.Multiselect = false;
            dlg.FilterIndex = 0;
            dlg.RestoreDirectory = true;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string translation_filename = dlg.FileName;
                if (translation_filename != null)
                {
                    string? translation_folder = Path.GetDirectoryName(translation_filename);
                    if (translation_folder != null)
                    {
                        string xml_file = Path.Combine(translation_folder, ".ln", "translation", "solution.xml");
                        if (File.Exists(xml_file))
                        {
                            LSolution.LoadStream(xml_file, LogFunc);
                            AddLog("", "Open Solution: " + translation_filename);
                        }
                        else
                        {
                            AddLog("", "Solution Collpased not found: " + translation_filename);
                        }
                    }
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
            AddLog("", "To Next Word");
        }


        private DelegateCommand? _movePrevCmd = null;
        public DelegateCommand MovePrevCmd =>
            _movePrevCmd ??= new DelegateCommand(MovePrevFunc);
        void MovePrevFunc()
        {
            if (SelectedWord != null)
            {
                SelectedWord.TargetString = SelectedWord.TargetString;
            }
            if (LocalizedWord > 0)
                LocalizedWord -= 1;
            AddLog("", "To Prev Word");
        }


        private DelegateCommand? _asIdCmd = null;
        public DelegateCommand AsIdCmd =>
            _asIdCmd ??= new DelegateCommand(AsIdFunc);
        void AsIdFunc()
        {
            AddLog("", "not implenmented AsIdFunc");
            // throw new NotImplementException();
        }



        private DelegateCommand? _ignoreCmd = null;
        public DelegateCommand IgnoreCmd =>
            _ignoreCmd ??= new DelegateCommand(IgnoreFunc);
        void IgnoreFunc()
        {
            AddLog("", "not implenmented ignoreFunc");
        }


        private DelegateCommand? _applyAllCmd = null;
        public DelegateCommand ApplyAllCmd =>
            _applyAllCmd ??= new DelegateCommand(ApplyAllFunc);
        void ApplyAllFunc()
        {
            if (SelectedWord != null)
            {
                AddLog("", "ApplyAllFunc");
                WordItem sItem = SelectedWord;
                if (LocalizationSource != null)
                {
                    foreach (WordItem eachWord in LocalizationSource)
                    {
                        //if (eachWord != sItem)
                        {
                            if (sItem.SourceString == eachWord.SourceString)
                            {
                                AddLog("", string.Format("{0} -> {1}", eachWord.TargetId, sItem.TargetId));
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


        private DelegateCommand? _clearTargetSetsCmd = null;
        public DelegateCommand ClearTargetSetsCmd =>
            _clearTargetSetsCmd ??= new DelegateCommand(ClearTargetSetsFunc);
        void ClearTargetSetsFunc()
        {
            AddLog("", "not implemented ClearTargetSetsFunc");
            // throw new NotImplementException();
        }


        private DelegateCommand? _saveTargetSetsCmd = null;
        public DelegateCommand SaveTargetSetsCmd =>
            _saveTargetSetsCmd ??= new DelegateCommand(SaveTargetSetsFunc);
        void SaveTargetSetsFunc()
        {
            AddLog("", "not implemented SaveTargetSetsFunc");

            // throw new NotImplementException();
        }

        #endregion

        #region property
        private bool? _isTargetChecked = false;
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
        /// Source Files -> One Languate File -> Multi Language File -> All Translations Files
        /// 
        /// 
        /// </summary>

        private DelegateCommand? _createEffectPrepareFilesCmd = null;
        public DelegateCommand CreateEffectPrepareFilesCmd =>
            _createEffectPrepareFilesCmd ??= new DelegateCommand(CreateEffectPrepareFilesFunc);
        void CreateEffectPrepareFilesFunc()
        {
            AddLog("", "not implemented CreateEffectPrepareFilesFunc");
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
            AddLog("", "not implemented ApplyTranslateFilesFunc");
        }

        private DelegateCommand? _deployCmd = null;
        public DelegateCommand DeployCmd =>
            _deployCmd ??= new DelegateCommand(DeployFunc);
        void DeployFunc()
        {
            if(Solution !=null && Solution.Projects!=null)
            {
                foreach(LProject prj in Solution.Projects)
                {
                    prj.Deploy();
                }
            }
        }


        private DelegateCommand? _openVSFileCmd = null;
        public DelegateCommand OpenVSFileCmd =>
            _openVSFileCmd ??= new DelegateCommand(OpenVSFileFunc);
        void OpenVSFileFunc()
        {
            if (Solution != null)
            {
                string solution_file = Solution.VsSolutionPath;
                if (File.Exists(solution_file))
                {
                    try
                    {
                        VSSelector vs = new VSSelector();
                        vs.ShowDialog();
                        string sel = vs.SelectedPath;
                        string vs_path = Path.Combine(sel, "Common7", "IDE", "devenv.exe");
                        Process.Start(vs_path, "\"" + solution_file + "\"");
                        AddLog("", "Run Visual Studio :" + vs_path + " " + solution_file);
                    }
                    catch (Exception e)
                    {
                        AddLog("", "Fail Run Visual Studio :" + e.Message);
                    }
                }
            }
        }

        private DelegateCommand? _testVSFileCmd = null;
        public DelegateCommand TestVSFileCmd =>
            _testVSFileCmd ??= new DelegateCommand(TestVSFileFunc);
        void TestVSFileFunc()
        {
            // throw new NotImplementException();
            if (Solution != null)
            {
                string solution_file = Path.Combine(Solution.FolderPath, ".ln", "translation", Solution.FileTitle + ".sln");
                if (!File.Exists(solution_file) && File.Exists(Solution.VsSolutionPath))
                {
                    File.Copy(Solution.VsSolutionPath, solution_file);
                    AddLog("", "TestVSFileFunc . not found  solution_file. Copy :" + solution_file);
                }
                if (File.Exists(solution_file))
                {
                    try
                    {
                        VSSelector vs = new VSSelector();
                        vs.ShowDialog();
                        string sel = vs.SelectedPath;
                        string vs_path = Path.Combine(sel, "Common7", "IDE", "devenv.exe");
                        Process.Start(vs_path, "\"" + solution_file + "\"");
                        AddLog("", "Run Visual Studio :" + vs_path + " " + solution_file);
                    }
                    catch (Exception e)
                    {
                        AddLog("", "Fail Run Visual Studio :" + e.Message);
                    }
                }
            }
        }

        private DelegateCommand? _buildTranslationCmd = null;
        public DelegateCommand BuildTranslationCmd =>
            _buildTranslationCmd ??= new DelegateCommand(BuildTranslationFunc);
        void BuildTranslationFunc()
        {
            AddLog("", "BuildTranslationFunc : Apply translation setting to target files");
            if (this.Solution != null && this.Solution.Projects != null)
            {
                foreach (LProject project in Solution.Projects)
                {
                    project.SaveTranslation();
                   
                    LocService.CreateNewDesignerFile(project);
                    AddLog("", "LocService.CreateNewDesignerFile");
                    LocService.CreateNewResxFile(project);
                    AddLog("", "LocService.CreateNewDesignerFile");
                    LocService.CreateNewIdFile(project);
                    AddLog("", "LocService.CreateNewDesignerFile");
                }
            }
        }


        private DelegateCommand? _exitCmd = null;
        public DelegateCommand ExitCmd =>
            _exitCmd ??= new DelegateCommand(ExitFunc);
        void ExitFunc()
        {
            System.Windows.Application.Current.Shutdown();
        }


        private ObservableCollection<LogEntry>? _logGo = new ObservableCollection<LogEntry>();
        public ObservableCollection<LogEntry>? LogGo
        {
            get => _logGo;
            set => SetProperty(ref _logGo, value);
        }

        public void ClearLog()
        {
            if (LogGo != null)
                LogGo.Clear();
        }

        public void AddLog(string title, string log)
        {
            if (LogGo != null)
            {
                LogGo.Add(new LogEntry(log));
            }
        }

        public int LogFunc(LogType logType, DateTime logTime, string title, string message, object? option)
        { 
            if(LogGo !=null)
            {
                LogGo.Add(new LogEntry(message));
            }
            return 0;
        }

         
        #endregion Translation Commands
    }


}
