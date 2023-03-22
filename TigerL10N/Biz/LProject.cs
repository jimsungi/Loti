using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.MonoCSharp;
using Microsoft.VisualBasic;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using TigerL10N.Service;
using TigerL10N.Utils;
using static System.Net.Mime.MediaTypeNames;
using static TigerL10N.Utils.Etc;
using Path = System.IO.Path;

namespace TigerL10N.Biz
{
    [Serializable]
    public class LProject : BindableBase
    {
        public static LProject? Current = null;

        public XmlSerializableDictionary<string, ProjectFile> ProjectFiles = new XmlSerializableDictionary<string, ProjectFile>();

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


        private string? _rawPath;
        public string RawPath
        {
            get => _rawPath ??= "";
            set => SetProperty(ref _rawPath, value);
        }

        public LProject()
        {

        }
        //public LProject(string project, string filepath)
        //{
        //    ProjectName = project;
        //    ProjectPath = filepath;
        //}
        string ProjectFileName = string.Empty;
        public LProject(string rawPath, LogDelegate logFunc)
        {
            LogFunc = logFunc;
            RawPath = rawPath;
            ProjectFileName = Path.GetFileName(RawPath);
            string ext = Path.GetExtension(ProjectFileName);
            if (!string.IsNullOrWhiteSpace(ext))
            {
                ProjectName = ProjectFileName.Replace(ext, string.Empty).Trim();
            }
        }

        [XmlIgnore]
        public LogDelegate? LogFunc=null;

        public LProject SetCurrent()
        {
            if (Current != null)
            {
                Current.Save();
                Current.Close();
            }
            Current = this;
            return this;
        }


        private string? _backupPath;
        public string BackupPath
        {
            get => _backupPath ??= "";
            set => SetProperty(ref _backupPath, value);
        }


        private string? _workPath;
        public string WorkPath
        {
            get => _workPath ??= "";
            set => SetProperty(ref _workPath, value);
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
            //AppConfigService.Settings.LastL18NFile = projectFileName;
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

            BackupPath = ProjectPath + Path.DirectorySeparatorChar + _backFolder;
            Directory.CreateDirectory(BackupPath);

            WorkPath = ProjectPath + Path.DirectorySeparatorChar + _workFolder;
            Directory.CreateDirectory(WorkPath);

            OneLangPath = WorkPath + Path.DirectorySeparatorChar + "one";
            MultiLangPath = WorkPath + Path.DirectorySeparatorChar + "multi";
        }

        public class ProcessOption
        {
            public string NameSpaceDefinition = "";
            public string L10NName = "L10N";
            public bool HasL10NDesigner = false;
            public bool HasL10NResource = false;
            public bool IsPrepare = true;
            public List<string> IgnoreFiles = new List<string>();
            public List<string> IgnoreFolder = new List<string>();
            public ProcessOption()
            {
                //gnoreFiles.Add(".dll");
                IgnoreFiles.Add(".obj");

                IgnoreFolder.Add("bin");
                IgnoreFolder.Add("obj");
                IgnoreFolder.Add(".vs");
            }
        }

        public string L10NFileName = "L";
        public string L10NFolderName = "L10N";
        public string ClrNamespace = "Local";

        public string L10NGlobalFileName = "G";
        public string L10NDesignerPath = string.Empty;
        public string L10NResourcePath = string.Empty;
        public string L10NGlobalPath = string.Empty;

        public void OneFileProc(string ChildSourceFile, string ChildTargetFile, ProcessOption option)
        {
            string _ChildSourceFileLower = ChildSourceFile.ToLower();
            string? ext = System.IO.Path.GetExtension(ChildSourceFile)?.ToLower();
            if (ext != null && !option.IgnoreFiles.Contains(ext.ToLower()))
            {

                if (_ChildSourceFileLower.EndsWith(".csproj"))
                {
                    ProjectProc(ChildSourceFile, ChildTargetFile, option);

                }
                else if (_ChildSourceFileLower.EndsWith(".xaml"))
                {
                    XamlProc(ChildSourceFile, ChildTargetFile, option);
                }

                else if (_ChildSourceFileLower.EndsWith(".cs"))
                {
                    CsProc(ChildSourceFile, ChildTargetFile, option);
                }
                else
                {
                    if (!option.IsPrepare)
                    {
                        File.Copy(ChildSourceFile, ChildTargetFile, true);
                    }
                }
            }
        }

        public void ProjectProc(string ChildSourceFile, string ChildTargetFile, ProcessOption option)
        {
            string ChildSourceFileLower = ChildSourceFile.ToLower();
            if (option.IsPrepare)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(ChildSourceFile);
                // Read .csproj
                //< Project Sdk = "Microsoft.NET.Sdk" >

                //< PropertyGroup >
                //< OutputType > WinExe </ OutputType >
                //< TargetFramework > net6.0 - windows </ TargetFramework >
                //< Nullable > enable </ Nullable >
                //< UseWPF > true </ UseWPF >
                // < AssemblyName > Fafa </ AssemblyName >
                //< RootNamespace > GoGo </ RootNamespace >
                //</ PropertyGroup >

                //< ItemGroup >
                //< Compile Update = "L10N\L10N.Designer.cs" >
                //< DesignTime > True </ DesignTime >
                //< AutoGen > True </ AutoGen >
                //< DependentUpon > L10N.resx </ DependentUpon >
                //</ Compile >
                //</ ItemGroup >

                //< ItemGroup >
                //< EmbeddedResource Update = "L10N\L10N.resx" >
                //< Generator > ResXFileCodeGenerator </ Generator >
                //< LastGenOutput > L10N.Designer.cs </ LastGenOutput >
                //</ EmbeddedResource >
                //</ ItemGroup >

                //</ Project >

                XmlElement? root = doc.DocumentElement;
                string FetchXmlText = "";
                if (root != null)
                {
                    FetchXmlText = PrintXml(root);
                    ClrNamespace = string.Empty;
                    List<XmlNode> nameSpaceNode = Etc.FindElement(root, 0, "PropertyGroup", "RootNamespace");
                    List<XmlNode> itemGroupCompier = Etc.FindElement(root, 0, "ItemGroup", "Compile");
                    List<XmlNode> itemGroupRsc = Etc.FindElement(root, 0, "ItemGroup", "EmbeddedResource");
                    if (nameSpaceNode != null)
                    {
                        foreach (XmlNode eachItem in nameSpaceNode)
                        {
                            ClrNamespace = eachItem.InnerText;
                        }
                    }
                    if (ClrNamespace == string.Empty)
                    {
                        ClrNamespace = Path.GetFileName(ChildSourceFile).Replace(".csproj", "");
                    }

                    if (itemGroupCompier != null)
                    {
                        foreach (XmlNode eachItem in itemGroupCompier)
                        {
                            XmlNode? updateNode = eachItem.SelectSingleNode("Update");
                            if (updateNode != null && updateNode is XmlAttribute)
                            {
                                string ExpectUpdatePath = Path.Combine(L10NFolderName, L10NFileName) + ".Designer.cs";
                                if (updateNode.Value == ExpectUpdatePath)
                                {
                                    option.HasL10NDesigner = true;
                                }
                            }
                        }
                    }

                    if (itemGroupRsc != null)
                    {
                        foreach (XmlNode eachItem in itemGroupRsc)
                        {
                            XmlNode? updateNode = eachItem.SelectSingleNode("Update");
                            if (updateNode != null && updateNode is XmlAttribute)
                            {
                                string ExpectUpdatePath = Path.Combine(L10NFolderName, L10NFileName) + ".res";
                                if (updateNode.Value == ExpectUpdatePath)
                                {
                                    option.HasL10NResource = true;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Read .csproj
                //< Project Sdk = "Microsoft.NET.Sdk" >

                //< PropertyGroup >
                //< OutputType > WinExe </ OutputType >
                //< TargetFramework > net6.0 - windows </ TargetFramework >
                //< Nullable > enable </ Nullable >
                //< UseWPF > true </ UseWPF >
                //</ PropertyGroup >

                //< ItemGroup >
                //< Compile Update = "L10N\L10N.Designer.cs" >
                //< DesignTime > True </ DesignTime >
                //< AutoGen > True </ AutoGen >
                //< DependentUpon > L10N.resx </ DependentUpon >
                //</ Compile >
                //</ ItemGroup >

                //< ItemGroup >
                //< EmbeddedResource Update = "L10N\L10N.resx" >
                //< Generator > ResXFileCodeGenerator </ Generator >
                //< LastGenOutput > L10N.Designer.cs </ LastGenOutput >
                //</ EmbeddedResource >
                //</ ItemGroup >

                //</ Project >
                XmlDocument doc = new XmlDocument();
                doc.Load(ChildSourceFile);
                XmlElement? root = doc.DocumentElement;
                string FetchXmlText = "";
                if (root != null)
                {
                    FetchXmlText = PrintXml(root);
                    ClrNamespace = string.Empty;
                    List<XmlNode> nameSpaceNode = Etc.FindElement(root, 0, "PropertyGroup", "RootNamespace");
                    if (nameSpaceNode != null)
                    {
                        foreach (XmlNode eachItem in nameSpaceNode)
                        {
                            ClrNamespace = eachItem.InnerText;
                        }
                    }
                    if (ClrNamespace == string.Empty)
                    {
                        ClrNamespace = Path.GetFileName(ChildSourceFile).Replace(".csproj", "");
                    }
                    if (!option.HasL10NDesigner)
                    {


                        // Adding Designer File information to project file
                        XmlNode? itemGroup = null;
                        List<XmlNode> item_groups = Etc.FindElement(root, 0, "ItemGroup", "Compile");
                        if (item_groups?.Count > 0)
                        {
                            itemGroup = item_groups[0].ParentNode;
                        }
                        else
                        {
                            itemGroup = doc.CreateElement("ItemGroup");
                        }
                        if (itemGroup != null)
                        {
                            XmlElement compileTag = doc.CreateElement("Compile");
                            compileTag.SetAttribute("Update", Path.Combine(L10NFolderName, L10NFileName) + ".Designer.cs");
                            {
                                XmlElement designTime = doc.CreateElement("DesignTime");
                                designTime.InnerText = "True";
                                compileTag.AppendChild(designTime);

                                XmlElement autoGen = doc.CreateElement("AutoGen");
                                autoGen.InnerText = "True";
                                compileTag.AppendChild(autoGen);

                                XmlElement dpUpon = doc.CreateElement("DependentUpon");
                                dpUpon.InnerText = L10NFileName + ".resx";
                                compileTag.AppendChild(dpUpon);

                            }
                            itemGroup.AppendChild(compileTag);
                            root.AppendChild(itemGroup);
                        }
                        // Create Designer File
                        string? NewDesignerFile = Path.GetDirectoryName(ChildTargetFile);
                        if (NewDesignerFile != null)
                        {
                            NewDesignerFile = Path.Combine(NewDesignerFile, L10NFolderName);
                            Directory.CreateDirectory(NewDesignerFile);
                            if (Directory.Exists(NewDesignerFile))
                            {
                                NewDesignerFile = Path.Combine(NewDesignerFile, L10NFileName + ".Designer.cs");
                                L10NDesignerPath = NewDesignerFile;
                            }
                        }

                    }

                    if (!option.HasL10NResource)
                    {
                        // Adding Resource File information to project file
                        XmlNode? itemGroup = null;
                        List<XmlNode> item_groups = Etc.FindElement(root, 0, "ItemGroup", "EmbeddedResource");
                        if (item_groups?.Count > 0)
                        {
                            itemGroup = item_groups[0].ParentNode;
                        }
                        else
                        {
                            itemGroup = doc.CreateElement("ItemGroup");
                        }
                        if (itemGroup != null)
                        {
                            XmlElement embeddedResrc = doc.CreateElement("EmbeddedResource");
                            embeddedResrc.SetAttribute("Update", Path.Combine(L10NFolderName, L10NFileName) + ".resx");
                            {
                                XmlElement generator = doc.CreateElement("Generator");
                                generator.InnerText = "ResXFileCodeGenerator";
                                embeddedResrc.AppendChild(generator);

                                XmlElement lastGenOutput = doc.CreateElement("LastGenOutput");
                                lastGenOutput.InnerText = L10NFileName + ".Designer.cs ";
                                embeddedResrc.AppendChild(lastGenOutput);
                            }
                            itemGroup.AppendChild(embeddedResrc);
                            root.AppendChild(itemGroup);
                        }
                        // Create Resource File
                        string? NewDesignerFile = Path.GetDirectoryName(ChildTargetFile);
                        if (NewDesignerFile != null)
                        {
                            NewDesignerFile = Path.Combine(NewDesignerFile, L10NFolderName);
                            Directory.CreateDirectory(NewDesignerFile);
                            if (Directory.Exists(NewDesignerFile))
                            {
                                string tar_file_name = string.Empty;
                                tar_file_name = Path.Combine(NewDesignerFile, L10NFileName + ".resx");
                                L10NResourcePath = tar_file_name;
                                tar_file_name = Path.Combine(NewDesignerFile, L10NGlobalFileName + ".cs");
                                L10NGlobalPath = tar_file_name;
                            }
                        }
                    }

                    // Save Project File
                    FetchXmlText = PrintXml(root);
                    using (StreamWriter writer = new StreamWriter(ChildTargetFile))
                    {
                        writer.Write(FetchXmlText);
                    }
                }
            }
            // Fetch with items

            // Write new xml
        }


        private bool? _hasL10NDesigner;
        public bool HasL10NDesigner
        {
            get => _hasL10NDesigner ??= false;
            set => SetProperty(ref _hasL10NDesigner, value);
        }


        private bool? _hasL10NResource;
        public bool HasL10NResource
        {
            get => _hasL10NResource ??= false;
            set => SetProperty(ref _hasL10NResource, value);
        }


        public void DoCsprojFile(string ChildSourceFile, string ChildTargetFile, Step step)
        {
            string ChildSourceFileLower = ChildSourceFile.ToLower();
            if (step == Step.Project)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(ChildSourceFile);
                // Read .csproj
                //< Project Sdk = "Microsoft.NET.Sdk" >

                //< PropertyGroup >
                //< OutputType > WinExe </ OutputType >
                //< TargetFramework > net6.0 - windows </ TargetFramework >
                //< Nullable > enable </ Nullable >
                //< UseWPF > true </ UseWPF >
                // < AssemblyName > Fafa </ AssemblyName >
                //< RootNamespace > GoGo </ RootNamespace >
                //</ PropertyGroup >

                //< ItemGroup >
                //< Compile Update = "L10N\L10N.Designer.cs" >
                //< DesignTime > True </ DesignTime >
                //< AutoGen > True </ AutoGen >
                //< DependentUpon > L10N.resx </ DependentUpon >
                //</ Compile >
                //</ ItemGroup >

                //< ItemGroup >
                //< EmbeddedResource Update = "L10N\L10N.resx" >
                //< Generator > ResXFileCodeGenerator </ Generator >
                //< LastGenOutput > L10N.Designer.cs </ LastGenOutput >
                //</ EmbeddedResource >
                //</ ItemGroup >

                //</ Project >

                XmlElement? root = doc.DocumentElement;
                string FetchXmlText = "";
                if (root != null)
                {
                    FetchXmlText = PrintXml(root);
                    ClrNamespace = string.Empty;
                    List<XmlNode> nameSpaceNode = Etc.FindElement(root, 0, "PropertyGroup", "RootNamespace");
                    List<XmlNode> itemGroupCompier = Etc.FindElement(root, 0, "ItemGroup", "Compile");
                    List<XmlNode> itemGroupRsc = Etc.FindElement(root, 0, "ItemGroup", "EmbeddedResource");
                    if (nameSpaceNode != null)
                    {
                        foreach (XmlNode eachItem in nameSpaceNode)
                        {
                            ClrNamespace = eachItem.InnerText;
                        }
                    }
                    if (ClrNamespace == string.Empty)
                    {
                        ClrNamespace = Path.GetFileName(ChildSourceFile).Replace(".csproj", "");
                    }

                    if (itemGroupCompier != null)
                    {
                        foreach (XmlNode eachItem in itemGroupCompier)
                        {
                            XmlNode? updateNode = eachItem.SelectSingleNode("Update");
                            if (updateNode != null && updateNode is XmlAttribute)
                            {
                                string ExpectUpdatePath = Path.Combine(L10NFolderName, L10NFileName) + ".Designer.cs";
                                if (updateNode.Value == ExpectUpdatePath)
                                {
                                    HasL10NDesigner = true;
                                }
                            }
                        }
                    }

                    if (itemGroupRsc != null)
                    {
                        foreach (XmlNode eachItem in itemGroupRsc)
                        {
                            XmlNode? updateNode = eachItem.SelectSingleNode("Update");
                            if (updateNode != null && updateNode is XmlAttribute)
                            {
                                string ExpectUpdatePath = Path.Combine(L10NFolderName, L10NFileName) + ".res";
                                if (updateNode.Value == ExpectUpdatePath)
                                {
                                    HasL10NResource = true;
                                }
                            }
                        }
                    }
                }
            }
            else if (step == Step.Done)
            {
                // Read .csproj
                //< Project Sdk = "Microsoft.NET.Sdk" >

                //< PropertyGroup >
                //< OutputType > WinExe </ OutputType >
                //< TargetFramework > net6.0 - windows </ TargetFramework >
                //< Nullable > enable </ Nullable >
                //< UseWPF > true </ UseWPF >
                //</ PropertyGroup >

                //< ItemGroup >
                //< Compile Update = "L10N\L10N.Designer.cs" >
                //< DesignTime > True </ DesignTime >
                //< AutoGen > True </ AutoGen >
                //< DependentUpon > L10N.resx </ DependentUpon >
                //</ Compile >
                //</ ItemGroup >

                //< ItemGroup >
                //< EmbeddedResource Update = "L10N\L10N.resx" >
                //< Generator > ResXFileCodeGenerator </ Generator >
                //< LastGenOutput > L10N.Designer.cs </ LastGenOutput >
                //</ EmbeddedResource >
                //</ ItemGroup >

                //</ Project >
                XmlDocument doc = new XmlDocument();
                doc.Load(ChildSourceFile);
                XmlElement? root = doc.DocumentElement;
                string FetchXmlText = "";
                if (root != null)
                {
                    FetchXmlText = PrintXml(root);
                    ClrNamespace = string.Empty;
                    List<XmlNode> nameSpaceNode = Etc.FindElement(root, 0, "PropertyGroup", "RootNamespace");
                    if (nameSpaceNode != null)
                    {
                        foreach (XmlNode eachItem in nameSpaceNode)
                        {
                            ClrNamespace = eachItem.InnerText;
                        }
                    }
                    if (ClrNamespace == string.Empty)
                    {
                        ClrNamespace = Path.GetFileName(ChildSourceFile).Replace(".csproj", "");
                    }
                    if (!HasL10NDesigner)
                    {
                        // Adding Designer File information to project file
                        XmlNode? itemGroup = null;
                        List<XmlNode> item_groups = Etc.FindElement(root, 0, "ItemGroup", "Compile");
                        if (item_groups?.Count > 0)
                        {
                            itemGroup = item_groups[0].ParentNode;
                        }
                        else
                        {
                            itemGroup = doc.CreateElement("ItemGroup");
                        }
                        if (itemGroup != null)
                        {
                            XmlElement compileTag = doc.CreateElement("Compile");
                            compileTag.SetAttribute("Update", Path.Combine(L10NFolderName, L10NFileName) + ".Designer.cs");
                            {
                                XmlElement designTime = doc.CreateElement("DesignTime");
                                designTime.InnerText = "True";
                                compileTag.AppendChild(designTime);

                                XmlElement autoGen = doc.CreateElement("AutoGen");
                                autoGen.InnerText = "True";
                                compileTag.AppendChild(autoGen);

                                XmlElement dpUpon = doc.CreateElement("DependentUpon");
                                dpUpon.InnerText = L10NFileName + ".resx";
                                compileTag.AppendChild(dpUpon);

                            }
                            itemGroup.AppendChild(compileTag);
                            root.AppendChild(itemGroup);
                        }
                        // Create Designer File
                        string? NewDesignerFile = Path.GetDirectoryName(ChildTargetFile);
                        if (NewDesignerFile != null)
                        {
                            NewDesignerFile = Path.Combine(NewDesignerFile, L10NFolderName);
                            Directory.CreateDirectory(NewDesignerFile);
                            if (Directory.Exists(NewDesignerFile))
                            {
                                NewDesignerFile = Path.Combine(NewDesignerFile, L10NFileName + ".Designer.cs");
                                L10NDesignerPath = NewDesignerFile;
                            }
                        }

                    }

                    if (!HasL10NResource)
                    {
                        // Adding Resource File information to project file
                        XmlNode? itemGroup = null;
                        List<XmlNode> item_groups = Etc.FindElement(root, 0, "ItemGroup", "EmbeddedResource");
                        if (item_groups?.Count > 0)
                        {
                            itemGroup = item_groups[0].ParentNode;
                        }
                        else
                        {
                            itemGroup = doc.CreateElement("ItemGroup");
                        }
                        if (itemGroup != null)
                        {
                            XmlElement embeddedResrc = doc.CreateElement("EmbeddedResource");
                            embeddedResrc.SetAttribute("Update", Path.Combine(L10NFolderName, L10NFileName) + ".resx");
                            {
                                XmlElement generator = doc.CreateElement("Generator");
                                generator.InnerText = "ResXFileCodeGenerator";
                                embeddedResrc.AppendChild(generator);

                                XmlElement lastGenOutput = doc.CreateElement("LastGenOutput");
                                lastGenOutput.InnerText = L10NFileName + ".Designer.cs ";
                                embeddedResrc.AppendChild(lastGenOutput);
                            }
                            itemGroup.AppendChild(embeddedResrc);
                            root.AppendChild(itemGroup);
                        }
                        // Create Resource File
                        string? NewDesignerFile = Path.GetDirectoryName(ChildTargetFile);
                        if (NewDesignerFile != null)
                        {
                            NewDesignerFile = Path.Combine(NewDesignerFile, L10NFolderName);
                            Directory.CreateDirectory(NewDesignerFile);
                            if (Directory.Exists(NewDesignerFile))
                            {
                                string tar_file_name = string.Empty;
                                tar_file_name = Path.Combine(NewDesignerFile, L10NFileName + ".resx");
                                L10NResourcePath = tar_file_name;
                                tar_file_name = Path.Combine(NewDesignerFile, L10NGlobalFileName + ".cs");
                                L10NGlobalPath = tar_file_name;
                            }
                        }
                    }

                    // Save Project File
                    FetchXmlText = PrintXml(root);
                    using (StreamWriter writer = new StreamWriter(ChildTargetFile))
                    {
                        writer.Write(FetchXmlText);
                    }
                }
            }
            // Fetch with items

            // Write new xml
        }
        public void DoCsFile(string ChildSourceFile, string ChildTargetFile, Step step)
        {
            string ChildSourceFileLower = ChildSourceFile.ToLower();
            bool pass = false;
            string contents = File.ReadAllText(ChildSourceFileLower);
            if (ChildSourceFileLower.EndsWith(".designer.cs")
                && File.Exists(ChildSourceFileLower.Replace(".designer.cs", ".resx"))
                && !contents.Contains("private void InitializeComponent()"))
            {
                // Form.Designer.cs file might be process
                // Resource.cs file might not be process
                pass = true;
            }
            if (pass)
            {
            }
            else if (step == Step.None)
            {
                string? tmpDir = "";
                tmpDir = Path.GetDirectoryName(ChildTargetFile);
                if (tmpDir != null)
                    Directory.CreateDirectory(tmpDir);

                StringParser p = StringParseService.CreateParser2()
 .SetProject(this)
.SetResourceFile(ChildSourceFileLower)
.RunParser()
.SaveTempFile(ChildTargetFile);

                Parsers.Add(ChildSourceFileLower, p);
                Replaces.Add(ChildSourceFileLower, ChildTargetFile);
            }
            else if (step == Step.Done)
            {
                //                        StringParser p = StringParseService.CreateParser2()
                //.SetResourceFile(ChildSourceFile)
                //.SetReferenceResult(parsers, replaces)
                //.SaveTmpSource();
            }
        }
        public void DoXamlFile(string ChildSourceFile, string ChildTargetFile, Step step)
        {
            string ChildSourceFileLower = ChildSourceFile.ToLower();
        }

        public void CsProc(string ChildSourceFile, string ChildTargetFile, ProcessOption option)
        {
            string ChildSourceFileLower = ChildSourceFile.ToLower();
            bool pass = false;
            string contents = File.ReadAllText(ChildSourceFileLower);
            if (ChildSourceFileLower.EndsWith(".designer.cs")
                && File.Exists(ChildSourceFileLower.Replace(".designer.cs", ".resx"))
                && !contents.Contains("private void InitializeComponent()"))
            {
                // Form.Designer.cs file might be process
                // Resource.cs file might not be process
                pass = true;
            }
            if (pass)
            {
            }
            else if (option.IsPrepare == true)
            {
                StringParser p = StringParseService.CreateParser2()
.SetResourceFile(ChildSourceFileLower)
.RunParser()
.SaveTempFile(ChildTargetFile);

                Parsers.Add(ChildSourceFileLower, p);
                Replaces.Add(ChildSourceFileLower, ChildTargetFile);
            }
            else
            {
                //                        StringParser p = StringParseService.CreateParser2()
                //.SetResourceFile(ChildSourceFile)
                //.SetReferenceResult(parsers, replaces)
                //.SaveTmpSource();
            }
        }

        public void XamlProc(string ChildSourceFile, string ChildTargetFile, ProcessOption option)
        {
            string ChildSourceFileLower = ChildSourceFile.ToLower();
        }


        public XmlSerializableDictionary<string, StringParser> Parsers = new XmlSerializableDictionary<string, StringParser>();
        public XmlSerializableDictionary<string, string> Replaces = new XmlSerializableDictionary<string, string>();


        private string? _langCode;
        public string LangCode
        {
            get => _langCode ??= "";
            set
            {
                SetProperty(ref _langCode, value);
                if (Words != null)
                {
                    foreach (WordItem ew in Words)
                    {
                        ew.LangCode = value;
                    }
                }
            }
        }


        private List<WordItem>? _words;
        public List<WordItem>? Words
        {
            get => _words;
            set => SetProperty(ref _words, value);
        }


        public List<WordItem> BuildWords()
        {
            List<WordItem> words = new List<WordItem>();
            foreach (KeyValuePair<string, StringParser> p in Parsers)
            {
                string f = p.Key;
                StringParser sp = p.Value;


                foreach (StringParser.L eachFileLn in sp.RawStringResultsOfAll)
                {
                    int lines = Etc.CountLines(eachFileLn.Org);

                    bool useAuto = true;
                    bool ignore = false;
                    bool asId = false;
                    string targetString = "";
                    string finalId = "";
                    string raw_string = eachFileLn.Org;
                    int org_len = raw_string.Length;
                    string org_string = string.Empty;
                    if (raw_string.StartsWith("@"))
                        org_string = raw_string.Substring(2, org_len - 3);
                    else
                        org_string = raw_string.Substring(1, org_len - 2);
                    string code = LocService.OneOfCode(org_string);
                    string prev_ref = "";
                    string next_ref = "";
                    string current_ref = "";

                    if (eachFileLn != null)
                    {
                        int s = eachFileLn != null ? (eachFileLn.S != null ? eachFileLn.S.Value.Line : 0) : 0;
                        int e = eachFileLn != null ? (eachFileLn.E != null ? eachFileLn.E.Value.Line : 0) : 0;
                        if (s != 0 && e != 0)
                        {
                            int ss = s - 8;
                            int ee = e + 8;
                            if (ss < 1)
                                ss = 1;
                            string[] _lines = File.ReadAllLines(f);
                            if (ee > _lines.Length)
                                ee = _lines.Length;
                            for (int i = ss; i < s; i++)
                            {
                                prev_ref += "\r\n" + _lines[i - 1];
                            }
                            for (int k = s; k <= e; k++)
                            {
                                if (k != s)
                                    current_ref += "\r\n";
                                current_ref += _lines[k - 1];

                            }
                            for (int j = e + 1; j < ee; j++)
                            {
                                next_ref += _lines[j - 1] + "\r\n";
                            }

                        }

                        if (lines > 1)
                        {
                            ignore = true;
                            targetString = org_string;
                        }
                        else if (code != "")
                        {
                            targetString = "";
                            finalId = code;
                            asId = true;
                        }
                        else
                        {
                            targetString = org_string;
                            finalId = LocService.GetRecommandID(org_string, true, false);
                            if (finalId.StartsWith("G."))
                            {
                                asId = true;
                                targetString = "";
                            }
                        }
                        if (eachFileLn != null)
                        {
                            WordItem item = new WordItem()
                            {
                                FileName = f,
                                TmpFile = eachFileLn.TmpFile,
                                SourceString = eachFileLn.Org,
                                FinalId = finalId,
                                TargetString = targetString,
                                TargetId = eachFileLn.AutoKey,
                                UseAuto = useAuto,
                                Ignore = ignore,
                                AsId = asId,
                                PrevRef = prev_ref,
                                NextRef = next_ref,
                                CurrentRef = current_ref
                            };

                            words.Add(item);
                        }
                    }

                }


            }
            foreach (WordItem eachWord in words)
            {
                eachWord.init = true;
                eachWord.RefAll = words;
            }
            Words = words;
            return Words;
        }

        //public List<XmlNode> GetPathNode(XmlNode rode, string A, string B, string C, params string[] pathNodeNames)
        //{
        //    List<XmlNode> result=new List<XmlNode>();
        //    int depth = 0;
        //    if(pathNodeNames.Length > depth)
        //    {
        //        XmlNodeList? list = rode.SelectNodes(pathNodeNames[depth]);
        //        depth++;
        //        if(list!=null)
        //        {
        //            foreach(XmlNode AA in list)
        //            {
        //                if (pathNodeNames.Length > depth)
        //                {
        //                    XmlNodeList? list2 = AA.SelectNodes(pathNodeNames[depth]);
        //                    depth++;
        //                    if (list2 != null)
        //                    {
        //                        foreach (XmlNode BB in list2)
        //                        {
        //                            if (pathNodeNames.Length > depth)
        //                            {
        //                                XmlNodeList? list3 = BB.SelectNodes(pathNodeNames[depth]);


        //                                if(depth == pathNodeNames.Length-1)
        //                                {
        //                                    foreach(XmlNode CC in list3)
        //                                    {
        //                                        result.Add(CC);
        //                                    }
        //                                }
        //                                else
        //                                {

        //                                }
        //                                depth++;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        ////}

        public List<string> GetBList(string xmlFilePath, string aTagName, string bTagName)
        {
            List<string> bList = new List<string>();

            // XML 파일을 로드합니다.
            XmlDocument xml = new XmlDocument();
            xml.Load(xmlFilePath);

            // A 태그를 선택합니다.
            XmlNodeList aNodeList = xml.GetElementsByTagName(aTagName);

            // 각 A 태그의 B 태그 목록을 가져옵니다.
            foreach (XmlNode aNode in aNodeList)
            {
                if (aNode != null)
                {
                    XmlNodeList? bNodeList = aNode.SelectNodes(bTagName);

                    if (bNodeList != null)
                    {
                        // 각 B 태그의 텍스트를 bList에 추가합니다.
                        foreach (XmlNode bNode in bNodeList)
                        {
                            bList.Add(bNode.InnerText);
                        }
                    }
                }
            }

            return bList;
        }


        //static void Main(string[] args)
        //{
        //    // Person 클래스 인스턴스 생성
        //    Person person = new Person { Name = "John", Age = 30 };

        //    // XmlSerializer 인스턴스 생성
        //    XmlSerializer serializer = new XmlSerializer(typeof(Person));

        //    // XmlSerializerNamespaces 인스턴스 생성
        //    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        //    ns.Add("", "");

        //    // Person 클래스 인스턴스를 XmlElement로 변환
        //    XmlDocument xmlDoc = new XmlDocument();
        //    using (XmlWriter writer = xmlDoc.CreateNavigator().AppendChild())
        //    {
        //        serializer.Serialize(writer, person, ns);
        //    }
        //    XmlElement personElement = xmlDoc.DocumentElement;

        //    // 변환된 XmlElement 출력
        //    Console.WriteLine(personElement.OuterXml);
        //}



        string Line = "\r\n";
        string PrintXml(XmlNode? node, int level = 0)
        {
            if (level == 0)
            {
                Line = "";
            }
            else
            {
                Line = "\r\n";
            }
            string PrevTab = "";
            for (int i = 0; i < level; i++)
            {
                PrevTab += "\t";
            }
            if (node == null)
                return "";

            string Ret = Line + PrevTab + "<";
            // 현재 노드 출력
            Ret += node.Name;
            if (node.NodeType == XmlNodeType.Element)
            {
                XmlElement element = (XmlElement)node;
                if (element.HasAttributes)
                {
                    // 노드의 속성 출력
                    foreach (XmlAttribute attribute in element.Attributes)
                    {
                        Ret += (" " + attribute.Name + "=\"" + attribute.Value + "\"");
                    }
                }
            }
            Ret += ">";

            // 자식 노드 출력
            if (node.HasChildNodes)
            {
                level++;
                XmlNodeList children = node.ChildNodes;
                if (children.Count == 1 && children[0] is XmlText)
                {
                    Ret += node.InnerText + "</" + node.Name + ">";
                    return Ret;
                }
                else
                {
                    foreach (XmlNode child in children)
                    {
                        if (child is XmlText)
                        {
                            Ret += node.InnerText;
                        }
                        else
                        {
                            string NodeStr = PrintXml(child, level);
                            Ret += NodeStr;
                        }
                    }
                }
            }
            return Ret + Line + PrevTab + "</" + node.Name + ">";
        }
        public static int Gindex = 0;

        private string? _oneLangPath;
        public string OneLangPath
        {
            get => _oneLangPath ??= "";
            set => SetProperty(ref _oneLangPath, value);
        }

        private string? _multiLangPath;
        public string MultiLangPath
        {
            get => _multiLangPath ??= "";
            set => SetProperty(ref _multiLangPath, value);
        }

        //public void BuildAllFile(List<Lang>? langs)
        //{
        //    //if (langs != null)
        //    //{
        //    //    foreach (Lang eLang in langs)
        //    //    {
        //    //        if(eLang.IsSelected==true)
        //                BuildFileLang(langs);
        //    //    }
        //    //}
        //}

        void PrepareProjectDir(string filename, Step step)
        {

        }
        [XmlIgnore]
        public LSolution? Solution = null;

        void PrepareProjectFile(string findFileFullPath, Step step)
        {
            string lowerPath = findFileFullPath.ToLower();
            string? ext = System.IO.Path.GetExtension(findFileFullPath)?.ToLower();
            string targetBase = "";
            if (Solution != null && Solution.FolderPath != null)
                targetBase = Path.Combine(Solution.FolderPath, ".ln", "translation");
            else
                return;
            string targetPath = findFileFullPath.Replace(Solution.FolderPath, targetBase);
            string? targetDir = Path.GetDirectoryName(targetPath);
            if (targetDir != null)
                Directory.CreateDirectory(targetDir);

            if (ext != null && !processOption.IgnoreFiles.Contains(ext.ToLower()))
            {
                if (step == Step.Project)
                {
                    if (lowerPath.EndsWith(".csproj") && step == Step.Project)
                    {
                        DoCsprojFile(findFileFullPath, targetPath, step);

                    }
                }
                else
                {
                    if (lowerPath.EndsWith(".csproj"))
                    {
                        DoCsprojFile(findFileFullPath, targetPath, step);

                    }
                    else if (lowerPath.EndsWith(".xaml"))
                    {
                        DoXamlFile(findFileFullPath, targetPath, step);
                    }

                    else if (lowerPath.EndsWith(".cs"))
                    {
                        DoCsFile(findFileFullPath, targetPath, step);
                    }
                    else
                    {
                        if (step == Step.Done)
                        {
                            File.Copy(findFileFullPath, targetPath, true);
                        }
                    }
                }
            }
        }
        IgnoreOption processOption = new IgnoreOption();

        public void BuildFileLang()
        {
            LProject project = this;
            /// 1. Backup
            /// Copy From Source Project
            /// 
            string? rawPath = Path.GetDirectoryName(RawPath);
            if (rawPath == null)
                return;
            LProject.Gindex = 0;
            if (Directory.Exists(rawPath))
            {
                Action<string, Step> DirFucc = PrepareProjectDir;
                Action<string, Step> FileFunc = PrepareProjectFile;

                LocService.IdKey.Clear();
                LocService.StringKey.Clear();

                Etc.RecurActionDirectory(rawPath, DirFucc, FileFunc, Step.Project, processOption);
                Etc.RecurActionDirectory(rawPath, DirFucc, FileFunc, Step.None, processOption);
                Etc.RecurActionDirectory(rawPath, DirFucc, FileFunc, Step.Done, processOption);

                BuildWords();
                LocService.IdKey.Clear();
                LocService.StringKey.Clear();
            }
        }
        public void AddLog(string title, string log)
        {
            if (LogFunc != null)
                LogFunc(LogType.Dev, DateTime.Now, title, log, null);
        }
        public void SaveTranslation()
        {
            string tmpFileName = "";
            string targetFileName = "";
            string contents = "";
            if (Words != null)
            {
                // Read All text from file
                List<WordItem>? saveWords = Words;
                if (saveWords != null)
                {
                    Dictionary<string, string> fileNameContentBuf = new Dictionary<string, string>();
                    foreach (WordItem eachWord in saveWords)
                    {
                        contents = "";
                        tmpFileName = eachWord.TmpFile;
                        if (!fileNameContentBuf.Keys.Contains(tmpFileName))
                        {
                            contents = File.ReadAllText(tmpFileName);
                            fileNameContentBuf.Add(tmpFileName, contents);
                        }
                    }

                    foreach (WordItem eachWord in saveWords)
                    {
                        AddLog("", "Process +" + eachWord.TargetId + " " + eachWord.TargetString);
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

                    foreach (KeyValuePair<string, string> fileAndContens in fileNameContentBuf)
                    {
                        string fileaneme = fileAndContens.Key;
                        string fContent = fileAndContens.Value;
                        targetFileName = "";
                        if (!string.IsNullOrWhiteSpace(tmpFileName))
                        {
                            if (tmpFileName.EndsWith(".ltmp"))
                            {
                                targetFileName = fileaneme.Substring(0, fileaneme.Length - 5);
                                if (!string.IsNullOrWhiteSpace(targetFileName))
                                {
                                    File.WriteAllText(targetFileName, fContent);
                                }
                                AddLog("", "Process +" + tmpFileName);
                            }
                        }
                    }
                }
            }
        }


        void DeployDir(string filename, Step step)
        {

        }

        void DeployFile(string findFileFullPath, Step step)
        {
            string? baseFolder = Path.GetDirectoryName(findFileFullPath);
            if (Solution != null)
            {
                string gogoPath = findFileFullPath.Replace(Solution.FolderPath, "");
                string targetPath = Path.Combine(Solution.FolderPath, ".ln", "translation");
                gogoPath = findFileFullPath.Replace(targetPath, "");
                string orgPath = Solution.FolderPath + gogoPath;
                if (File.Exists(findFileFullPath))
                {
                    string ext = Path.GetExtension(findFileFullPath.ToLower());
                    switch(ext)
                    {
                        case ".cs":
                        case ".resx":
                            // do copy
                            break;
                        case ".ltmp":
                        case ".user":
                        case ".xml":
                        case ".sln":
                        case ".ln":
                            // ignore
                            return;
                        default:
                            // ignore
                            return;
                    }
                    string? dirname = Path.GetDirectoryName(orgPath);
                    if (dirname != null)
                        Directory.CreateDirectory(dirname);
                    File.Copy(findFileFullPath, orgPath,true);
                }
            }
            
        }
        public void Deploy()
        {
            IgnoreOption deployOption = new IgnoreOption();
            string? rawPath = Path.GetDirectoryName(RawPath);
            if (rawPath == null)
                return;
            if (Solution != null)
            {
                
                string folderBase = Path.Combine(Solution.FolderPath, ".ln", "translation");// + gogoPath;
                if (Directory.Exists(rawPath))
                {
                    Action<string, Step> DirFucc = DeployDir;
                    Action<string, Step> FileFunc = DeployFile;

                    Etc.RecurActionDirectory(folderBase, DirFucc, FileFunc, Step.Project, deployOption);

                }
            }
        }
    }
}
