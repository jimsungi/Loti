using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TigerL10N.Service;

namespace TigerL10N.Biz
{
    public class L10NProject : BindableBase
    {
        public static L10NProject? Current = null;

        public Dictionary<string, ProjectFile> ProjectFiles = new Dictionary<string, ProjectFile>();

        private string? _name;
        public string ProjectName
        {
            get => _name ??= "";
            set => SetProperty(ref _name, value);
        }

        private string? _projectPath;
        public string ProjectPath
        {
            get => _projectPath ??= "";
            set => SetProperty(ref _projectPath, value);
        }


        private string? _targetPath;
        public string RawPath
        {
            get => _targetPath ??= "";
            set => SetProperty(ref _targetPath, value);
        }

        public L10NProject(string project, string filepath)
        {
            ProjectName = project;
            ProjectPath = filepath;
        }

        public L10NProject SetCurrent()
        {
            if (Current != null)
            {
                Current.Save();
                Current.Close();
            }
            Current = this;
            return this;
        }



        private bool? _backUp;
        public bool BackUp
        {
            get => _backUp ??= false;
            set => SetProperty(ref _backUp, value);
        }

        public void Save()
        {
            string projectFileName = ProjectPath + System.IO.Path.DirectorySeparatorChar + ProjectName
                                              + ProjectManageService.ProjectExt;

            CreateDefaultFolder();
            Save(projectFileName);
        }

        public void Open()
        {
            CreateDefaultFolder();
        }

        private void Save(string projectFileName)
        {
            AppConfigService.Settings.LastL18NFile = projectFileName;
            if (BackUp)
            {
                if (File.Exists(projectFileName))
                {
                    string backupFileName = projectFileName + DateTime.Now.ToString("yyyyMMdd") + ".bak";
                    File.Copy(projectFileName, backupFileName);
                    File.Delete(projectFileName);
                }
            }
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            System.Xml.XmlNode root = doc.CreateElement("Localization");
            {
                XmlNode header = doc.CreateElement("Info");
                {
                    XmlNode project_name = doc.CreateElement("Name");
                    project_name.InnerText = ProjectName;
                    XmlNode target_path = doc.CreateElement("Source");
                    target_path.InnerText = RawPath;
                    header.AppendChild(project_name);
                    header.AppendChild(target_path);
                }
                root.AppendChild(header);
            }
            doc.AppendChild(root);
            doc.Save(projectFileName);
        }



        public void Close()
        {

        }

        System.Globalization.CultureInfo _cEnUS = new System.Globalization.CultureInfo("en-US");
        System.Globalization.CultureInfo _cKoKR = new System.Globalization.CultureInfo("ko-KR");
        string _resFolder = "translate";
        string _backFolder = "backup";
        string _baseFolder = "base";
        string _workFolder = "work";
        public bool Create()
        {
            if (!Directory.Exists(ProjectPath))
                Directory.CreateDirectory(ProjectPath);

            CreateDefaultFolder();

            Save();
            //string projectData = "";

            // Xml Node Create


            return true;
        }

        private void CreateDefaultFolder()
        {
            // Create Language Folder
            // Default Base(eng) - Target(eng,kor)
            // L10N - base
            // L10N - eng
            // L10N - kor
            string BasePath = ProjectPath + Path.DirectorySeparatorChar + _resFolder + Path.DirectorySeparatorChar + _baseFolder;
            string EngPath = ProjectPath + Path.DirectorySeparatorChar + _resFolder + Path.DirectorySeparatorChar + Path.DirectorySeparatorChar + _cEnUS.Name;
            string KorPath = ProjectPath + Path.DirectorySeparatorChar + _resFolder + Path.DirectorySeparatorChar + _cKoKR.Name;


            Directory.CreateDirectory(BasePath);
            Directory.CreateDirectory(EngPath);
            Directory.CreateDirectory(KorPath);

            string BackPath = ProjectPath + Path.DirectorySeparatorChar + _backFolder;
            Directory.CreateDirectory(BackPath);

            string WorkPath = ProjectPath + Path.DirectorySeparatorChar + _workFolder;
            Directory.CreateDirectory(WorkPath);
        }
    }
}
