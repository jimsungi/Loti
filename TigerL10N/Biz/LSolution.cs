using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.MonoCSharp;
using ICSharpCode.NRefactory.MonoCSharp.Linq;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using Prism.Mvvm;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using TigerL10N.Service;
using TigerL10N.Utils;
using static System.Net.Mime.MediaTypeNames;
using static TigerL10N.Biz.LProject;
using Path = System.IO.Path;

namespace TigerL10N.Biz
{
    [Serializable]
    public class LSolution : BindableBase
    {
        public LSolution()
        {
            SelectedLang = null;
            OptionLanguageList = 0;
        }
        [XmlIgnore]
        public LogDelegate? LogFunc;
        public LSolution(string vsSolutionFile, LogDelegate _logFunc)
        {
            LogFunc = _logFunc;
        }

        public void Save()
        {
            string vsSolutionFile = VsSolutionPath;
            string lsolutionPath = vsSolutionFile;
            lsolutionPath = vsSolutionFile.ToLower();
            if (lsolutionPath.ToLower().EndsWith(".sln"))
            {
                // .l10n
                FilePath = vsSolutionFile.Substring(0, vsSolutionFile.Length - 4) + ".ln";
                FileTitle = Path.GetFileName(vsSolutionFile).Replace(".sln", "");
                string? dir = Path.GetDirectoryName(vsSolutionFile);
                if (dir != null)
                {
                    Folder = Path.Combine(dir, ".ln");
                }
            }
            if (!File.Exists(vsSolutionFile))
            {
                string msg = "Solution File Not Found";
                System.Windows.MessageBox.Show(msg);
                throw new Exception(msg);
            }
            else
            {
                if (File.Exists(FilePath))
                {
                    ReadSolutionFile();
                    AppConfigService.Settings.LastI18NFilePath = FilePath;
                }
                else
                {
                    CreateBackupFiles();
                    CreateProjectFiles();
                    SaveSolutionFile();
                }
            }
        }

        public void InitBuild()
        {

            if (Projects?.Count > 0)
            {
                CurrentProject = Projects.First();
                foreach (LProject project in Projects)
                {
                    project.BuildFileLang();
                    project.BuildWords();
                }
            }
            SelectedLang = SelectedLang;
        }

        private bool? _optionBackupSource=true;
        /// <summary>
        /// Backup or not Source Files
        /// </summary>
        public bool OptionBackupSource
        {
            get => _optionBackupSource ??= false;
            set => SetProperty(ref _optionBackupSource, value);
        }


        private string? _backPath;
        public string BackPath
        {
            get => _backPath ??= "";
            set => SetProperty(ref _backPath, value);
        }



        void CreateBackupFiles()
        {
            if (OptionBackupSource == true)
            {             
                //if (Directory.Exists(Folder))
                {
                    //// if directory A contains A.sln, backup start to A/.ln/backup/backup20110101_123001 foler
                    ///  then you need to know 1) .ln in A folder would not be copied
                    ///  
                    {
                        string BackPath = Path.Combine(Folder,"backup", DateTime.Now.ToString("yyyyMMdd_HHmmss").ToString());
                        Directory.CreateDirectory(BackPath);
                        IgnoreOption Option = new IgnoreOption();
                        string? sourcePath = Path.GetDirectoryName(VsSolutionPath);
                        if (sourcePath != null)
                        {
                            Etc.CopyDirectory(sourcePath, BackPath, Option);
                            Backup = BackPath;
                        }
                    }
                }
            }
        }

        private string? _backup;
        /// <summary>
        /// Backup Path
        /// </summary>
        public string Backup
        {
            get => _backup ??= "";
            set => SetProperty(ref _backup, value);
        }



        public List<string> RoopFolderForCsProject(string sDir, IgnoreOption option)
        {
            List<string> csfile = new List<string>();
            if (Directory.Exists(sDir))
            {
                //try
                //{
                    // File First
                    string[] files = Directory.GetFiles(sDir);
                    // Process CsProj then others
                    foreach (string ChildSourceFile in files)
                    {
                        string lowername = ChildSourceFile.ToLower();
                        string fName = Path.GetFileName(ChildSourceFile);
                        if (lowername.EndsWith(".csproj"))
                        {
                            csfile.Add(ChildSourceFile);
                            //OneFileProc(ChildSourceFile, ChildTargetFile, option);
                        }
                    }

                    // Directory Follows
                    foreach (string ChildSourceDir in Directory.GetDirectories(sDir))
                    {
                        string? dirName = Path.GetDirectoryName(ChildSourceDir);
                        dirName = Path.GetFileName(ChildSourceDir);
                        if (dirName != null && !option.IgnoreFolder.Contains(dirName.ToLower()))
                        {
                            List<string> c =  RoopFolderForCsProject(ChildSourceDir,option);
                        if(c!=null && c.Count >0)
                        {
                            foreach(string cName in c)
                            {
                                csfile.Add(cName);
                            }
                        }
                        }
                    }
                //}
                //catch (System.Exception excpt)
                //{
                //    throw excpt;// new Exception("");
                //    //Console.WriteLine(excpt.Message);
                //}
            }
            return csfile;
        }


        private LProject? _currentProject;
        public LProject? CurrentProject
        {
            get => _currentProject;
            set =>SetProperty(ref _currentProject, value);
        }

        private List<LProject>? _projects = new List<LProject>();
        public List<LProject>? Projects 
        {
            get => _projects;
            set => SetProperty(ref _projects, value);
        }
        private int? _currentIndex = 0;
        /// <summary>
        /// Index of CurrentProject
        /// </summary>
        public int? CurrentIndex
        {
            get => _currentIndex;
            set => SetProperty(ref _currentIndex, value);
        }

        private bool? _optionBackup;
        public bool OptionBackup
        {
            get => _optionBackup ??= false;
            set => SetProperty(ref _optionBackup, value);
        }
        public void AddLog(string title, string log)
        {
            if (LogFunc != null)
                LogFunc(LogType.Dev, DateTime.Now, title, log, null);
        }

        void CreateProjectFiles()
        {
            // List .csproj for Each LProject
            List<string> csfiles = RoopFolderForCsProject(Folder, new IgnoreOption());
            List<LProject> Projects = new List<LProject>();
            foreach (string csfile in csfiles)
            {
                LProject p = new LProject(csfile, LogFunc);
                Projects.Add(p);
            }
            CurrentProject = null;


            //LProject.ProcessOption PO = new LProject.ProcessOption();
            //PO.IsPrepare = true;
            //project.DirOneFileProc(RawPath, project.OneLangPath, PO);
            //PO.IsPrepare = false;
            //project.DirOneFileProc(RawPath, project.OneLangPath, PO);

            //project.Words = project.BuildWords();
            //List<WordItem> sortedByName = project.Words.OrderBy(w => w.SourceString).ToList();
            //this.LocalizationSource = sortedByName;
            //foreach (WordItem eachWord in sortedByName)
            //{
            //    eachWord.init = true;
            //    eachWord.RefAll = sortedByName;
            //}
            //LocService.IdKey.Clear();
            //LocService.StringKey.Clear();
        }

        private string? _vsSolutionPath;
        public string VsSolutionPath
        {
            get => _vsSolutionPath ??= "";
            set => SetProperty(ref _vsSolutionPath, value);
        }


        private List<Lang>? _languages;
        // All List Of language > you can select one of them
        public List<Lang>? Languages
        {
            get => _languages;
            set => SetProperty(ref _languages, value);
        }


        private Lang? _selectedLang;
        /// <summary>
        /// Current Lang
        /// </summary>
        public Lang? SelectedLang
        {
            get => _selectedLang;
            set
            {
                SetProperty(ref _selectedLang, value);

                if (Languages != null && value != null)
                {
                    string code = value.Code;
                    if (Projects != null)
                    {
                        foreach (LProject ep in Projects)
                        {
                            ep.LangCode = code;
                        }
                    }
                }
            } 
        }


        private int? _optionLanguageList = 0;
        /// <summary>
        /// Show 
        /// </summary>
        public int? OptionLanguageList
        {
            get => _optionLanguageList;
            set
            {
                SetProperty(ref _optionLanguageList, value);
                switch(_optionLanguageList)
                {
                    case 0: 
                        System.Tuple<List<Lang>, Lang> r= GetShortList();
                        Languages = r.Item1;
                        SelectedLang = r.Item2;
                        break;
                    case 1:
                        Languages = GetAllLanguage();
                        SelectedLang = null;
                        break;
                    default:
                        break;
                }
            }
        }

        System.Tuple<List<Lang>,Lang> GetShortList()
        {

            //{
            
                List<Lang> langList = new List<Lang>();

        var ko_kr = new Lang("ko-KR");
        ko_kr.IsSelected = true;
                langList.Add(ko_kr);

                var en_US = new Lang("en-US");
        en_US.IsSelected = true;
                langList.Add(en_US);

                langList.Add(new Lang("zh-CN"));
                langList.Add(new Lang("ja-JP"));

                var fr_FR = new Lang("fr-FR");
        fr_FR.IsSelected = true;
                langList.Add(fr_FR);
                langList.Add(new Lang("de-DE"));
                langList.Add(new Lang("ru-RU"));
                langList.Add(new Lang("zh-TW"));
                langList.Add(new Lang("pl-PL"));
                langList.Add(new Lang("hi-IN"));
                langList.Add(new Lang("en-GB"));
                langList.Add(new Lang("es-ES"));
                langList.Add(new Lang("pt-BR"));
                langList.Add(new Lang("en-CA"));
                langList.Add(new Lang("fr-CA"));
                langList.Add(new Lang("ko-KL"));

            return  new System.Tuple<List<Lang>, Lang>(langList,ko_kr); ;

        }

        List<Lang> GetAllLanguage()
        {
            {
                List<Lang> tmp =
                CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Select(culture => new Lang(culture.EnglishName))
            .ToList();
                Etc.ToFirst(tmp, x => x.EngName == "ko-KR");

                return tmp;
            }
        }



        private string? _filePath;
        /// <summary>
        /// Solution File Path
        /// </summary>
        public string FilePath
        {
            get => _filePath ??= "";
            set
            {
                SetProperty(ref _filePath, value);
                string? t = Path.GetDirectoryName(_filePath);
                if (t != null)
                    FolderPath = t;
            }
        }


        private string? _folderPath;
        public string FolderPath
        {
            get => _folderPath ??= "";
            set => SetProperty(ref _folderPath, value);
        }


        private string? _fileName;
        /// <summary>
        /// Solution File Name
        /// </summary>
        public string FileTitle
        {
            get => _fileName ??= "";
            set => SetProperty(ref _fileName, value);
        }


        private string? _folder;
        /// <summary>
        /// Folder to save solution related files
        /// </summary>
        public string Folder
        {
            get => _folder ??= "";
            set => SetProperty(ref _folder, value);
        }


        private int? _optionResource = 0;
        /// <summary>
        /// Which Resource to use @ref OutputOptions
        /// </summary>
        public int? OptionResource 
        {
            get => _optionResource??=0;
            set => SetProperty(ref _optionResource, value);
        }


        private bool? _optionOverwrite = false;
        /// <summary>
        /// Overwite or not when you create project
        /// </summary>
        public bool OptionOverwrite
        {
            get => _optionOverwrite ??= false;
            set => SetProperty(ref _optionOverwrite, value);
        }


        private bool? _optionForceNamespace=false;
        /// <summary>
        /// Overwrite or not namespace
        /// </summary>
        public bool OptionForceNamespace
        {
            get => _optionForceNamespace ??= false;
            set => SetProperty(ref _optionForceNamespace, value);
        }


        private string? _forceNamespace="";
        /// <summary>
        /// Default Namespace which override project namespace (not recommended)
        /// </summary>
        public string ForceNamespace
        {
            get => _forceNamespace ??= "";
            set => SetProperty(ref _forceNamespace, value);
        }

        /// <summary>
        /// Read solution file
        /// </summary>
        public void ReadSolutionFile()
        {

        }

        public void SaveSolutionFile()
        {
            // .in 폴더는 숨김폴더임
            DirectoryInfo directoryInfo = Directory.CreateDirectory(Folder);
            directoryInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            XmlDocument doc = new XmlDocument();
            {
                XmlElement tSolution = doc.CreateElement("Solution");
                {
                    // Attribute
                    int rtype = 0;
                    if(OptionResource!=null)
                        rtype = OptionResource.Value;
                    tSolution.SetAttribute("ResourceType", OutputOptions(rtype));
                    tSolution.SetAttribute("Overwrite", OptionOverwrite == true ? "Y" : "N");
                    if (OptionForceNamespace == true)
                    {
                        tSolution.SetAttribute("Namespace", ForceNamespace);
                    }

                    // Children
                    // Languages
                    XmlElement tLanguauges = doc.CreateElement("Languages");
                    {
                        // Attribute
                        if (SelectedLang != null && SelectedLang.IsSelected)
                            tLanguauges.SetAttribute("Default", SelectedLang.Code);
                        if (Languages != null)
                        {
                            foreach (Lang eachLang in Languages)
                            {
                                if (eachLang.IsSelected)
                                {
                                    XmlElement tLanguage = doc.CreateElement("Language");
                                    {
                                        tLanguage.SetAttribute("Code", eachLang.Code);                                        
                                    }
                                    tLanguauges.AppendChild(tLanguage);
                                }
                            }
                        }
                        tSolution.AppendChild(tLanguauges);
                    }
                }
                doc.AppendChild(tSolution);
            }

            doc.Save(FilePath);
            AppConfigService.Settings.LastI18NFilePath = FilePath;
            SaveStream();
        }

        string OutputOptions(int idex)
        {
            switch(idex)
            {
                case 1: return "resource+id";
                case 2: return "resource+id+message";
                default: return "resource";
            }
        }

        public void SaveStream()
        {
            XmlSerializer se = new XmlSerializer(typeof(LSolution));
            string filename = Path.Combine(FolderPath, ".ln", "translation");
            Directory.CreateDirectory(filename);
            filename = Path.Combine(filename, "solution.xml");
            StreamWriter writer = new StreamWriter(filename);
            se.Serialize(writer, this);
            writer.Close();
            AppConfigService.Settings.LastStreamFilePath = filename;
        }
        public static LSolution? LoadStream(string xml_file, LogDelegate _logFunc)
        {
            if (File.Exists(xml_file))
            {
                string? translation = File.ReadAllText(xml_file);
                if (!string.IsNullOrWhiteSpace(translation))
                {
                    XmlSerializer se = new XmlSerializer(typeof(LSolution));

                    using (StringReader reader = new StringReader(translation))
                    {
                        var s = se.Deserialize(reader);
                        if (s != null)
                        {
                            LSolution ss = (LSolution)s;
                            ss.LogFunc = _logFunc;
                            return ss;
                        }
                    }
                }
            }
            return null;
        }
    }
}
