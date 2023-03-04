using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Editor;
using Microsoft.VisualBasic;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using TigerL10N.Service;
using TigerL10N.Utils;
using static System.Net.Mime.MediaTypeNames;
using Path = System.IO.Path;

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
            string ChildSourceFileLower = ChildSourceFile.ToLower();
            string? ext = System.IO.Path.GetExtension(ChildSourceFile)?.ToLower();
            if (ext != null && !option.IgnoreFiles.Contains(ext.ToLower()))
            {

                if (ChildSourceFileLower.EndsWith(".csproj"))
                {


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

                            List<XmlNode> itemGroupCompier = Etc.FindElement(root, 0, "ItemGroup", "Compile");
                            List<XmlNode> itemGroupRsc = Etc.FindElement(root, 0, "ItemGroup", "EmbeddedResource");

                            if (itemGroupCompier != null)
                            {
                                foreach (XmlNode eachItem in itemGroupCompier)
                                {
                                    XmlNode? updateNode = eachItem.SelectSingleNode("Update");
                                    if (updateNode != null && updateNode is XmlAttribute)
                                    {
                                        string ExpectUpdatePath = Path.Combine(L10NFolderName, L10NFileName)  + ".Designer.cs";
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
                                        string ExpectUpdatePath = Path.Combine(L10NFolderName, L10NFileName)  + ".res";
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
                                if(itemGroup!=null)
                                {
                                    XmlElement compileTag = doc.CreateElement("Compile");
                                    compileTag.SetAttribute("Update",Path.Combine(L10NFolderName, L10NFileName) +  ".Designer.cs");
                                    {
                                        XmlElement designTime = doc.CreateElement("DesignTime");
                                        designTime.InnerText = "True";
                                        compileTag.AppendChild(designTime);

                                        XmlElement autoGen = doc.CreateElement("AutoGen");
                                        autoGen.InnerText = "True";
                                        compileTag.AppendChild(autoGen);

                                        XmlElement dpUpon = doc.CreateElement("DependentUpon");
                                        dpUpon.InnerText = L10NFileName +".resx";
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
                                if(itemGroup !=null)
                                {
                                    XmlElement embeddedResrc = doc.CreateElement("EmbeddedResource");
                                    embeddedResrc.SetAttribute("Update", Path.Combine(L10NFolderName, L10NFileName) + ".resx");
                                    {
                                        XmlElement generator = doc.CreateElement("Generator");
                                        generator.InnerText = "ResXFileCodeGenerator";
                                        embeddedResrc.AppendChild(generator);

                                        XmlElement lastGenOutput = doc.CreateElement("LastGenOutput");
                                        lastGenOutput.InnerText = L10NFileName +  ".Designer.cs ";
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
                else if (ChildSourceFileLower.EndsWith(".xaml"))
                {

                }
                else if (ChildSourceFileLower.EndsWith(".xaml"))
                {

                }
                else if (ChildSourceFileLower.EndsWith(".cs"))
                {
                    bool pass = false;
                    string contents = File.ReadAllText(ChildSourceFileLower);
                    if(ChildSourceFileLower.EndsWith(".designer.cs") 
                        && File.Exists(ChildSourceFileLower.Replace(".designer.cs",".resx"))
                        && !contents.Contains("private void InitializeComponent()"))
                    {
                        // Form.Designer.cs file might be process
                        // Resource.cs file might not be process
                        pass = true;
                    }
                    if(pass) 
                    {
                    }
                    else if(option.IsPrepare==true)
                    {
                        StringParser p = StringParseService.CreateParser2()
    .SetResourceFile(ChildSourceFile)
    .RunParser()
    .SaveTempFile(ChildTargetFile);

                        Parsers.Add(ChildSourceFile, p);
                        Replaces.Add(ChildSourceFile, ChildTargetFile);
                    }
                    else
                    {
//                        StringParser p = StringParseService.CreateParser2()
//.SetResourceFile(ChildSourceFile)
//.SetReferenceResult(parsers, replaces)
//.SaveTmpSource();
                    }
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

        public Dictionary<string, StringParser> Parsers = new Dictionary<string, StringParser>();
        public Dictionary<string, string> Replaces = new Dictionary<string, string>();




        public List<WordItem> Words = new List<WordItem>();

        public List<WordItem> BuildWords()
        {
            List<WordItem> items = new List<WordItem>();
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


                    int s = eachFileLn != null ? eachFileLn.S.Value.Line : 0;
                    int e = eachFileLn != null ? eachFileLn.E.Value.Line : 0;
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
                    items.Add(item);
                }


            }
            return items;
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

        public void DirOneFileProc(string sDir, string tDir, ProcessOption option)
        {
            if (Directory.Exists(sDir))
            {
                Directory.CreateDirectory(tDir);
                try
                {
                    foreach (string ChildSourceDir in Directory.GetDirectories(sDir))
                    {
                        string? dirName = Path.GetDirectoryName(ChildSourceDir);
                        dirName = Path.GetFileName(ChildSourceDir);
                        if (dirName != null && !option.IgnoreFolder.Contains(dirName.ToLower()))
                        {
                            string ChildTargetDir = tDir + Path.DirectorySeparatorChar + dirName;
                            DirOneFileProc(ChildSourceDir, ChildTargetDir, option);
                        }
                    }

                    foreach (string ChildSourceFile in Directory.GetFiles(sDir))
                    {
                        string fName = Path.GetFileName(ChildSourceFile);
                        string ChildTargetFile = tDir + Path.DirectorySeparatorChar + fName;

                        
                        OneFileProc(ChildSourceFile, ChildTargetFile, option);
                    }

                }
                catch (System.Exception excpt)
                {
                    Console.WriteLine(excpt.Message);
                }
            }
        }

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

    }
}
