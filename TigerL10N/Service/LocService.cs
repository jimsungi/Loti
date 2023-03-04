using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Shapes;
using TigerL10N.Biz;

namespace TigerL10N.Service
{
    public class LocService
    {
        public static CultureInfo[] GetAllCultures()
        {
            CultureInfo[] cinfo = CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures);
            return cinfo;
        }

        /// <summary>
        /// Generate auto key from the text
        /// </summary>
        /// <param name="recommandID"></param>
        /// <returns></returns>

        public static List<string> IdKey = new List<string>();
        public static List<string> StringKey = new List<string>();

        public static string GetRecommandID(string recommandID, bool id, bool notToMakeduplicate = true)
        {
            List<string>? source_list = null;

            if (id == true)
                source_list = IdKey;
            else
                source_list = StringKey;

            string res = string.Empty;
            string key_str = recommandID;
            key_str = key_str.Replace("\r\n", "_");
            key_str = key_str.Replace("\"", "_");
            
            key_str = key_str.Replace("!", "_");
            key_str = key_str.Replace("@", "at");
            key_str = key_str.Replace("#", "_");
            key_str = key_str.Replace("$", "_");
            key_str = key_str.Replace("%", "_");
            key_str = key_str.Replace("^", "_");
            key_str = key_str.Replace("&", "_");
            key_str = key_str.Replace("*", "_");
            key_str = key_str.Replace("(", "_");
            key_str = key_str.Replace(")", "_");
            key_str = key_str.Replace("_", "_");
            key_str = key_str.Replace("+", "_");

            key_str = key_str.Replace("=", "_");
            
            key_str = key_str.Replace("/", "_");
            key_str = key_str.Replace(".", "_");
            key_str = key_str.Replace(" ", "_");
            key_str = key_str.Replace(":", "_");
            key_str = key_str.Replace(";", "_");
            key_str = key_str.Replace(",", "_");
            key_str = key_str.Replace("[", "_");
            key_str = key_str.Replace("]", "_");
            key_str = key_str.Replace("<", "_");
            key_str = key_str.Replace(">", "_");
            key_str = key_str.Replace("!", "_");
            key_str = key_str.Replace("?", "_");
            key_str = key_str.Replace("+", "_");
            key_str = key_str.Replace("-", "_");
            key_str = key_str.Replace("'", "_");

            key_str = key_str.Replace("\\", "");
            if (key_str.StartsWith("0"))
                key_str = "_" + key_str;
            if (key_str.StartsWith("1"))
                key_str = "_" + key_str;
            if (key_str.StartsWith("2"))
                key_str = "_" + key_str;
            if (key_str.StartsWith("3"))
                key_str = "_" + key_str;
            if (key_str.StartsWith("4"))
                key_str = "_" + key_str;
            if (key_str.StartsWith("5"))
                key_str = "_" + key_str;
            if (key_str.StartsWith("6"))
                key_str = "_" + key_str;
            if (key_str.StartsWith("7"))
                key_str = "_" + key_str;
            if (key_str.StartsWith("8"))
                key_str = "_" + key_str;
            if (key_str.StartsWith("9"))
                key_str = "_" + key_str;
            if (key_str.Length == 0)
                key_str = "_";

            string base_str = key_str;
            if (notToMakeduplicate)
            {
                int i = 0;
                while (source_list.Contains(base_str))
                {
                    base_str = string.Format("{0}_{1}", key_str, i++);
                }
            }
            while(base_str.Contains("__"))
            {
                base_str = base_str.Replace("__", "_");
            }
            source_list.Add(base_str);
            if (base_str == "_")
                return "G.special";
            return "L." +base_str;
        }

        public static string OneOfCode(string codeCandidate)
        {
            switch (codeCandidate)
            {
                case "": return "string.Empty";
                case "<": return "G.Lo";
                case ">": return "G.Le";
                case " ": return "G.space";
                case "\r\n": return "G.linecode";
                case "\\r\\n": return "G.line";
                case "\t": return "G.tabcode";
                case "\\t": return "G.tab";
                case "(": return "G.Qo";
                case ")": return "G.Qe";
            }
            return string.Empty;
        }
        //public static int CountOfDupId(List<WordItem> string id, bool isString)
        //{

        //}

        public static void CreateNewDesignerFile(L10NProject cProject, List<string>? language=null)
        {
            L10NProject p = cProject;
            string filenmae = cProject.L10NDesignerPath;
            string Pjmainclass = cProject.L10NFileName;
            string Pjnamespace = cProject.ClrNamespace;

            string codesList = CreateResourceKeyList(p);
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
        internal " + Pjnamespace + @"() {
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
" 
+ codesList
+ @"    }
}";

            using (StreamWriter writer = new StreamWriter(filenmae))
            {
                writer.Write(content);
            }
        }
        static string CreateResourceKeyList(L10NProject p)
        {
            string ret = string.Empty;
            List<WordItem> items = p.Words;

            string eachWord = string.Empty;
            string eachKey = string.Empty;
            List<string> registered = new List<string>();

            foreach (WordItem w in items)
            {
                eachKey = w.FinalId;
                if (eachKey.StartsWith("L.") && !registered.Contains(eachKey))
                {
                    registered.Add(eachKey);
                    eachKey = eachKey.Substring(2, eachKey.Length - 2);

                    eachWord = @"        /// <summary>
        /// " + eachKey + @"  과(와) 유사한 지역화된 문자열을 찾습니다.
        /// </summary>
        internal static string " + eachKey + @"
        {
            get
            {
                return ResourceManager.GetString(""" + eachKey + @""", resourceCulture);
            }
        }
";
 ret += eachWord;
                }
               
            }
            return ret;
        }

        static string CreateResourceKeyValueList(L10NProject p)
        {
            string ret = string.Empty;
            List<WordItem> items = p.Words;

            string eachWord = string.Empty;
            string eachKey = string.Empty;
            string eachValue = string.Empty;
            List<string> registered = new List<string>();

            foreach (WordItem w in items)
            {
                eachKey = w.FinalId;
                eachValue = w.TargetString;
                if (eachKey.StartsWith("L.") && !registered.Contains(eachKey))
                {
                    registered.Add(eachKey);
                    eachKey = eachKey.Substring(2, eachKey.Length - 2);
                    
                    eachWord = @"  <data name=""" + eachKey + @""" xml:space=""preserve"">
    <value>" + eachValue + @"</value>
  </data>0
";
                    ret += eachWord;
                }                
            }
            return ret;
        }
        public static void CreateNewResxFile(L10NProject p)
        {
            string filename = p.L10NResourcePath;
            string resources = CreateResourceKeyValueList(p);
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
	</resheader>" + resources +
@"
</root>
";
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.Write(content);
            }
        }
   
    
        public static void CreateNewIdFile(L10NProject cProject)
        {
            L10NProject p = cProject;
            string filenmae = cProject.L10NGlobalPath;
            string Pjmainclass = cProject.L10NGlobalFileName;
            string Pjnamespace = cProject.ClrNamespace;

            string codesList = CreateGlobalKeyValueList(p);
            string content = @"
//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------

namespace " + Pjnamespace + @" {
    using System;
    
    internal class " + Pjmainclass + @" {
        
        internal " + Pjnamespace + @"() {
        }
"
+ codesList
+ @"    }
}";

            using (StreamWriter writer = new StreamWriter(filenmae))
            {
                writer.Write(content);
            }
        }

        static string CreateGlobalKeyValueList(L10NProject p)
        {
            string ret = string.Empty;
            List<WordItem> items = p.Words;

            string eachWord = string.Empty;
            string eachKey = string.Empty;
            string eachValue = string.Empty;
            List<string> registered = new List<string>();

            foreach (WordItem w in items)
            {
                eachKey = w.FinalId;
                eachValue = w.TargetString;
                if (eachKey.StartsWith("G.") && !registered.Contains(eachKey))
                {
                    registered.Add(eachKey);
                    eachKey = eachKey.Substring(2, eachKey.Length - 2);

                    eachWord = @"  public static string " + eachKey + @" = """ + eachValue + @""";
";
                    ret += eachWord;
                }
            }
            return ret;
        }
    }
}
