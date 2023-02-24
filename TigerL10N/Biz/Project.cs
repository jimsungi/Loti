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

        private string? _l10NFileName = "L10N";
        public string L10NFileName
        {
            get => _l10NFileName ??= "";
            set => SetProperty(ref _l10NFileName, value);
        }


        public void OneFileProc(string ChildSourceFile, string ChildTargetFile, ProcessOption option)
        {
            string ChildSourceFileLower = ChildSourceFile.ToLower();
            string? ext = System.IO.Path.GetExtension(ChildSourceFile)?.ToLower();
            if (ext != null && !option.IgnoreFiles.Contains(ext.ToLower()))
            {

                if (ChildSourceFileLower.EndsWith(".csproj"))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(ChildSourceFile);

                    if (option.IsPrepare)
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

                        XmlElement? root = doc.DocumentElement;
                        string FetchXmlText = "";
                        if (root != null)
                        {
                            FetchXmlText = PrintXml(root);

                            List<XmlNode> itemGroupCompier = FindElement(root, 0, "ItemGroup", "Compile");
                            List<XmlNode> itemGroupRsc = FindElement(root, 0, "ItemGroup", "EmbeddedResource");

                            if (itemGroupCompier != null)
                            {
                                foreach (XmlNode eachItem in itemGroupCompier)
                                {
                                    XmlNode? updateNode = eachItem.SelectSingleNode("Update");
                                    if (updateNode != null && updateNode is XmlAttribute)
                                    {
                                        string ExpectUpdatePath = "L10N\\" + L10NFileName + ".Designer.cs";
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
                                        string ExpectUpdatePath = "L10N\\" + L10NFileName + ".res";
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

                        XmlElement? root = doc.DocumentElement;
                        string FetchXmlText = "";
                        if (root != null)
                        {
                            FetchXmlText = PrintXml(root);

                            if (!option.HasL10NDesigner)
                            {
                                XmlNode? itemGroup = null;
                                List<XmlNode> item_groups = FindElement(root, 0, "ItemGroup", "Compile");
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
                                    compileTag.SetAttribute("Update", "L10N\\L10N.Designer.cs");
                                    {
                                        XmlElement designTime = doc.CreateElement("DesignTime");
                                        designTime.InnerText = "True";
                                        compileTag.AppendChild(designTime);

                                        XmlElement autoGen = doc.CreateElement("AutoGen");
                                        autoGen.InnerText = "True";
                                        compileTag.AppendChild(autoGen);

                                        XmlElement dpUpon = doc.CreateElement("DependentUpon");
                                        dpUpon.InnerText = "L10N.resx";
                                        compileTag.AppendChild(dpUpon);

                                    }
                                    itemGroup.AppendChild(compileTag);
                                    root.AppendChild(itemGroup);
                                }
                                string? NewDesignerFile = Path.GetDirectoryName(ChildTargetFile);
                                if (NewDesignerFile != null)
                                {
                                    NewDesignerFile = Path.Combine(NewDesignerFile, "L10N");
                                    Directory.CreateDirectory(NewDesignerFile);
                                    if (Directory.Exists(NewDesignerFile))
                                    {
                                        NewDesignerFile = Path.Combine(NewDesignerFile, "L10N.Designer.cs");
                                        CreateNewDesignerFile(NewDesignerFile,"OKMM","L10N");
                                    }
                                }

                            }

                            if (!option.HasL10NResource)
                            {
                                XmlNode? itemGroup = null;
                                List<XmlNode> item_groups = FindElement(root, 0, "ItemGroup", "EmbeddedResource");
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
                                    embeddedResrc.SetAttribute("Update", "L10N\\L10N.resx");
                                    {
                                        XmlElement generator = doc.CreateElement("Generator");
                                        generator.InnerText = "ResXFileCodeGenerator";
                                        embeddedResrc.AppendChild(generator);

                                        XmlElement lastGenOutput = doc.CreateElement("LastGenOutput");
                                        lastGenOutput.InnerText = "L10N.Designer.cs ";
                                        embeddedResrc.AppendChild(lastGenOutput);
                                    }
                                    itemGroup.AppendChild(embeddedResrc);
                                    root.AppendChild(itemGroup);
                                }
                                string? NewDesignerFile = Path.GetDirectoryName(ChildTargetFile);
                                if (NewDesignerFile != null)
                                {
                                    NewDesignerFile = Path.Combine(NewDesignerFile, "L10N");
                                    Directory.CreateDirectory(NewDesignerFile);
                                    if (Directory.Exists(NewDesignerFile))
                                    {
                                        NewDesignerFile = Path.Combine(NewDesignerFile, "L10N.resx");
                                        CreateNewResxFile(NewDesignerFile);
                                    }
                                }
                            }

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
                    if(option.IsPrepare==true)
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
        private void CreateNewDesignerFile(string filenmae,string Pjmainclass, string Pjnamespace)
        {
            string content = @"
//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.42000
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------

namespace " + Pjmainclass + @" {
    using System;
    
    
    /// <summary>
    ///   지역화된 문자열 등을 찾기 위한 강력한 형식의 리소스 클래스입니다.
    /// </summary>
    // 이 클래스는 ResGen 또는 Visual Studio와 같은 도구를 통해 StronglyTypedResourceBuilder
    // 클래스에서 자동으로 생성되었습니다.
    // 멤버를 추가하거나 제거하려면 .ResX 파일을 편집한 다음 /str 옵션을 사용하여 ResGen을
    // 다시 실행하거나 VS 프로젝트를 다시 빌드하십시오.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute(""System.Resources.Tools.StronglyTypedResourceBuilder"", ""17.0.0.0"")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class " + Pjnamespace + @" {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute(""Microsoft.Performance"", ""CA1811:AvoidUncalledPrivateCode"")]
        internal " +Pjnamespace + @"() {
        }
        
        /// <summary>
        ///   이 클래스에서 사용하는 캐시된 ResourceManager 인스턴스를 반환합니다.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager(""" + Pjmainclass + @"." + Pjnamespace + @""", typeof(" + Pjnamespace + @").Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   이 강력한 형식의 리소스 클래스를 사용하여 모든 리소스 조회에 대해 현재 스레드의 CurrentUICulture 속성을
        ///   재정의합니다.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
    }
}";
            using (StreamWriter writer = new StreamWriter(filenmae))
            {
                writer.Write(content);
            }
        }

        private void CreateNewResxFile(string  filename)
        {
            string content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
	<!-- 
		Microsoft ResX Schema

		Version 1.3

		The primary goals of this format is to allow a simple XML format 
		that is mostly human readable. The generation and parsing of the 
		various data types are done through the TypeConverter classes 
		associated with the data types.

		Example:

		... ado.net/XML headers & schema ...
		<resheader name=""resmimetype"">text/microsoft-resx</resheader>
		<resheader name=""version"">1.3</resheader>
		<resheader name=""reader"">System.Resources.ResXResourceReader, System.Windows.Forms, ...</resheader>
		<resheader name=""writer"">System.Resources.ResXResourceWriter, System.Windows.Forms, ...</resheader>
		<data name=""Name1"">this is my long string</data>
		<data name=""Color1"" type=""System.Drawing.Color, System.Drawing"">Blue</data>
		<data name=""Bitmap1"" mimetype=""application/x-microsoft.net.object.binary.base64"">
			[base64 mime encoded serialized .NET Framework object]
		</data>
		<data name=""Icon1"" type=""System.Drawing.Icon, System.Drawing"" mimetype=""application/x-microsoft.net.object.bytearray.base64"">
			[base64 mime encoded string representing a byte array form of the .NET Framework object]
		</data>

		There are any number of ""resheader"" rows that contain simple 
		name/value pairs.

		Each data row contains a name, and value. The row also contains a 
		type or mimetype. Type corresponds to a .NET class that support 
		text/value conversion through the TypeConverter architecture. 
		Classes that don't support this are serialized and stored with the 
		mimetype set.

		The mimetype is used for serialized objects, and tells the 
		ResXResourceReader how to depersist the object. This is currently not 
		extensible. For a given mimetype the value must be set accordingly:

		Note - application/x-microsoft.net.object.binary.base64 is the format 
		that the ResXResourceWriter will generate, however the reader can 
		read any of the formats listed below.

		mimetype: application/x-microsoft.net.object.binary.base64
		value   : The object must be serialized with 
			: System.Serialization.Formatters.Binary.BinaryFormatter
			: and then encoded with base64 encoding.

		mimetype: application/x-microsoft.net.object.soap.base64
		value   : The object must be serialized with 
			: System.Runtime.Serialization.Formatters.Soap.SoapFormatter
			: and then encoded with base64 encoding.

		mimetype: application/x-microsoft.net.object.bytearray.base64
		value   : The object must be serialized into a byte array 
			: using a System.ComponentModel.TypeConverter
			: and then encoded with base64 encoding.
	-->
	
	<xsd:schema id=""root"" xmlns="""" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
		<xsd:element name=""root"" msdata:IsDataSet=""true"">
			<xsd:complexType>
				<xsd:choice maxOccurs=""unbounded"">
					<xsd:element name=""data"">
						<xsd:complexType>
							<xsd:sequence>
								<xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />
								<xsd:element name=""comment"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""2"" />
							</xsd:sequence>
							<xsd:attribute name=""name"" type=""xsd:string"" msdata:Ordinal=""1"" />
							<xsd:attribute name=""type"" type=""xsd:string"" msdata:Ordinal=""3"" />
							<xsd:attribute name=""mimetype"" type=""xsd:string"" msdata:Ordinal=""4"" />
						</xsd:complexType>
					</xsd:element>
					<xsd:element name=""resheader"">
						<xsd:complexType>
							<xsd:sequence>
								<xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />
							</xsd:sequence>
							<xsd:attribute name=""name"" type=""xsd:string"" use=""required"" />
						</xsd:complexType>
					</xsd:element>
				</xsd:choice>
			</xsd:complexType>
		</xsd:element>
	</xsd:schema>
	<resheader name=""resmimetype"">
		<value>text/microsoft-resx</value>
	</resheader>
	<resheader name=""version"">
		<value>1.3</value>
	</resheader>
	<resheader name=""reader"">
		<value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=2.0.3500.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
	</resheader>
	<resheader name=""writer"">
		<value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=2.0.3500.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
	</resheader>
</root>
";
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.Write(content);
            }
        }
        public List<XmlNode> GetNodeList(XmlNodeList list)
        {
            List<XmlNode> nodes = new List<XmlNode>();
            foreach(XmlNode node in list)
            {
                nodes.Add(node);
            }
            return nodes;
        }

        public List<XmlNode> FindElement(XmlNode node, int depth, params string[] pathNodeNames)
        {
            List<XmlNode> result = new List<XmlNode>();
            if (pathNodeNames.Length > depth)
            {
                string node_name = pathNodeNames[depth];
                List<XmlNode> list = GetNodeList(node.ChildNodes).Where(n => n.Name == pathNodeNames[depth]).ToList();
                bool meetTargetLevel = false;
                if (depth == pathNodeNames.Length - 1)
                {
                    meetTargetLevel = true;
                }

                if (list != null)
                {
                    int nd = depth + 1;
                    foreach (XmlNode CC in list)
                    {
                        if (meetTargetLevel)
                        {
                            result.Add(CC);
                        }
                        else
                        {
                            
                            List<XmlNode> cRes = FindElement(CC, nd, pathNodeNames);
                            foreach (XmlNode n in cRes)
                            {
                                result.Add(n);
                            }
                        }
                    }
                }
            }
            return result;
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
