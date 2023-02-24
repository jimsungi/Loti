using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.MonoCSharp;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace TigerL10N.Biz
{
    public class StringParserOrg
    {
        public StringParserOrg()
        {
            _namedRsc.Add("", "");
            _groupRsc.Add("", "R");
        }

        public StringParserOrg ParseFile(string filename)
        {
            return this;
        }

        public StringParserOrg BuildResourceDictionary(string rsc_filename, Dictionary<string, string> rsc, Dictionary<string, string> grp)
        {
            string[] lines = File.ReadAllLines(rsc_filename);
            if (lines.Length > 0)
            {
                string groupName = "R";
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (line.Contains("public static class "))
                    {
                        string oldGroup = groupName;
                        string clean = line.Replace("public static class ", "").Trim();
                        string[] unnote = clean.Split('/');
                        if (unnote.Length > 0)
                        {
                            groupName = unnote[0].Trim();
                        }
                        else
                        {
                            groupName = clean;
                        }
                        if (string.IsNullOrWhiteSpace(groupName))
                            groupName = oldGroup;
                    }
                    else if (line.Contains("public const string "))
                    {
                        string clean = line.Replace("public const string", "").Trim();
                        string[] kv = clean.Split('=');
                        if (kv.Length == 2)
                        {
                            string key = kv[0].Trim();
                            string value = kv[1].Trim();
                            value = value.Substring(1, value.Length - 3);
                            value = value.Replace("\\\"", "\"");
                            if (!rsc.Keys.Contains(value))
                            {
                                rsc[value] = key;
                                grp[key] = groupName;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Check public const string");
                        }
                    }
                }
            }

            return this;
        }

        public StringParserOrg CreateResourceFile(string resource_filename, string backup_suffix, Dictionary<string, string> name_rsc, Dictionary<string, string> group_rsc)
        {
            List<string> Group = new List<string>();
            foreach (string vv in group_rsc.Values)
            {
                if (!Group.Contains(vv))
                    Group.Add(vv);
            }
            string tmp_filename = resource_filename + ".tmp";
            if (!string.IsNullOrWhiteSpace(backup_suffix))
            {
                File.Copy(resource_filename, resource_filename + backup_suffix, true);
            }
            File.Delete(tmp_filename);

            StreamWriter writer = File.CreateText(tmp_filename);

            writer.Write(@"
///
///   Auto Generated string Resource File

namespace GSCM.FP.Common.UIL
{
");
            foreach (string eachgroup in Group)
            {

                writer.Write("    public static class " + eachgroup + @"
    {
");
                ;
                //          public const string Y = "Y";
                foreach (KeyValuePair<string, string> kvp in name_rsc)
                {
                    string value = kvp.Key;
                    string key = kvp.Value;
                    if (value == "")
                    {

                    }
                    else if (group_rsc[key] == eachgroup)
                    {
                        writer.WriteLine(string.Format("        public const string {0} = \"{1}\";", key, value));
                    }
                }
                writer.Write(
                    @"   }
");
            }
            writer.Write(@"
}
");
            writer.Close();
            File.Copy(tmp_filename, resource_filename, true);
            File.Delete(tmp_filename);
            return this;
        }

        private bool _folderConversion = false;
        public StringParserOrg SetFolderConversion(bool folderConversion)
        {
            this._folderConversion = folderConversion;
            return this;
        }

        private bool _createNewResource = false;
        public StringParserOrg SetCreateNewResource(bool createNewResource)
        {
            _createNewResource = createNewResource;
            return this;
        }

        private string _rscFilename="";
        public StringParserOrg SetResourceFile(string resourceFile)
        {
            _rscFilename = resourceFile;
            return this;
        }

        private string src_filename="";
        public StringParserOrg SetSourceFile(string SourceFile)
        {
            src_filename = SourceFile;
            return this;
        }

        private string src_foldername = "";
        public StringParserOrg SetFolder(string Folder)
        {
            src_foldername = Folder;
            return this;
        }

        private bool _replaceSrcFile = false;

        public StringParserOrg SetReplaceSource(bool ReplaceSource)
        {
            _replaceSrcFile = ReplaceSource;
            return this;
        }

        private bool _doNotReadResourceFile = false;
        public StringParserOrg SetReadResourceFile(bool DoNotReadResourceFile)
        {
            _doNotReadResourceFile = !DoNotReadResourceFile;
            return this;
        }

        string _backupSuffix = "." + DateTime.Now.ToString("yyyyMMddHHmmm") + ".bak";
        private readonly Dictionary<string, string> _namedRsc = new Dictionary<string, string>(); // resource : resource_key
        private readonly Dictionary<string, string> _groupRsc = new Dictionary<string, string>(); //

        public bool RunParser()
        {
            if (_createNewResource)
            {
                BuildResourceDictionary(_rscFilename, _namedRsc, _groupRsc);

            }


            if (_createNewResource)
            {
                VistaOpenFileDialog newFile = new VistaOpenFileDialog();
                if (newFile.ShowDialog() == true)
                {
                    _rscFilename = newFile.FileName;
                }
                else
                {
                    _rscFilename = "c:\\test.cs";
                    _backupSuffix = "";
                }
            }
            CreateResourceFile(_rscFilename, _backupSuffix, _namedRsc, _groupRsc);
            if (!_folderConversion)
            {
                ProcessSrcFile(src_filename, _backupSuffix, _namedRsc, _groupRsc);
            }
            else
            {
                ProcessFolder(src_foldername, _backupSuffix, _namedRsc, _groupRsc);
            }

            BuildResourceDictionary(_rscFilename, _namedRsc, _groupRsc);
            CreateResourceFile(_rscFilename, _backupSuffix, _namedRsc, _groupRsc);
            return true;
        }

        void ProcessSrcFile(string src_filename, string backup_suffix, Dictionary<string, string> dictRsc, Dictionary<string, string> grpRsc)
        {
            string tmp_filename = src_filename + ".tmp";

            if (!string.IsNullOrWhiteSpace(backup_suffix))
            {
                File.Copy(src_filename, src_filename + backup_suffix, true);
            }
            File.Delete(tmp_filename);

            StreamWriter writer;
            StreamReader reader;
            reader = new StreamReader(src_filename);
            string? line = "";

            writer = File.CreateText(tmp_filename);
            _status = "";
            while ((line = reader.ReadLine()) != null)
            {
                ProcessLine(line, reader, writer, dictRsc, grpRsc);
            }
            _status = "";
            writer.Close();
            reader.Close();
            if (_replaceSrcFile)
            {
                File.Copy(tmp_filename, src_filename, true);
            }
            File.Delete(tmp_filename);

        }
        string _status = "";
        void ProcessLine(string line, StreamReader reader, StreamWriter writer, Dictionary<string, string> dictRsc, Dictionary<string, string> grpRsc)
        {
            if (line != "")
            {
                switch (_status)
                {
                    case "openlongnote":
                        {
                            //int quote = line.IndexOf("\"");
                            //int shortnote = line.IndexOf("//");
                            int closelongnote = line.IndexOf("*/");
                            if (closelongnote >= 0)
                            {
                                _status = "";
                            }
                            writer.WriteLine(line);
                        }
                        break;
                    case "":
                        {
                            int quote = line.IndexOf("\"");
                            int shortnote = line.IndexOf("//");
                            int longnote = line.IndexOf("/*");
                            if (quote < 0 && shortnote < 0 && longnote < 0)
                            {
                                writer.WriteLine(line);
                            }
                            else if (longnote >= 0 && (longnote < quote || quote == -1) && (longnote < shortnote || shortnote == -1))
                            {
                                writer.WriteLine(line);
                                _status = "openlongnote";
                            }
                            else if (shortnote >= 0 && (shortnote < quote || quote == -1) && (shortnote < longnote || longnote == -1))
                            {
                                writer.WriteLine(line);
                            }
                            else if (quote >= 0 && (quote < shortnote || shortnote == -1) && (quote < longnote || longnote == -1))
                            {
                                string quotestr = "";
                                string key_str = "";
                                int quoteclose = line.IndexOf("\"", quote + 1);
                                if (quoteclose > 0)
                                {
                                    quotestr = line.Substring(quote, quoteclose - quote + 1);
                                    quotestr = quotestr.Replace("\"", "");
                                    key_str = quotestr.ToUpper();
                                    key_str = key_str.Replace("/", "_");
                                    key_str = key_str.Replace(".", "_");
                                    key_str = key_str.Replace(" ", "_");
                                    key_str = key_str.Replace("=", "_");
                                    key_str = key_str.Replace(",", "_");
                                    key_str = key_str.Replace("[", "_");
                                    key_str = key_str.Replace("]", "_");
                                    key_str = key_str.Replace("<", "_");
                                    key_str = key_str.Replace(">", "_");
                                    key_str = key_str.Replace("!", "_");
                                    key_str = key_str.Replace("?", "_");
                                    key_str = key_str.Replace("+", "_");
                                    key_str = key_str.Replace("-", "_");
                                    key_str = key_str.Replace("(", "_");
                                    key_str = key_str.Replace(")", "_");
                                    key_str = key_str.Replace("'", "_");
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
                                    if (quotestr == "")
                                    {
                                        line = line.Replace("\"" + quotestr + "\"", "string.Empty");
                                        writer.WriteLine(line);
                                    }
                                    else if (dictRsc.ContainsKey(quotestr))
                                    {
                                        string group_name = grpRsc[dictRsc[quotestr]];
                                        //string old_var = dictRsc[quotestr];
                                        line = line.Replace("\"" + quotestr + "\"", group_name + "." + key_str);
                                        writer.WriteLine(line);
                                    }
                                    else
                                    {
                                        if (_doNotReadResourceFile == false)
                                        {
                                            dictRsc[quotestr] = key_str;
                                            grpRsc[key_str] = "R";
                                            line = line.Replace("\"" + quotestr + "\"", "R." + key_str);
                                            ProcessLine(line, reader, writer, dictRsc, grpRsc);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                writer.WriteLine(line);
                            }
                        }
                        break;
                }
            }
            else
            {
                writer.WriteLine(line);
            }
        }

        void ProcessFolder(string folderPath, string backup_suffix, Dictionary<string, string> rsc, Dictionary<string, string> grp)
        {

        }

        string ReadLine()
        {
            return "";
        }
    }

    public class StringParser
    {
        public void AddID(string txt, string skey, string fileName, TextLocation? start, TextLocation? end)
        {
            RawIDResultsOfAll.Add(new L
                {
                Org= txt,
                F = fileName,
                    S = start,
                    E = end,
                    Key = skey
                }); 
            if (!_idDic.ContainsKey(txt))
                _idDic.Add(txt, skey);
            if (!_idRevDic.ContainsKey(skey))
                _idRevDic.Add(skey, txt);
        }

        public void AddStr(string txt, string skey, string fileName, TextLocation? start, TextLocation? end)
        {
            L k = new L
            {
                Org = txt,
                F = fileName,
                S = start,
                E = end,
                Key = skey
            };
            string n = GetText(lines, k);
            k.Org = n; 

            RawStringResultsOfAll.Add(k);
            if (!_strDic.ContainsKey(n))
                _strDic.Add(n, skey);
            if(!_strRevDic.ContainsKey(skey))
                _strRevDic.Add(skey, n);
        }

        public StringParser()
        {
            AddID("", "", "", null, null);
            AddStr("", "G","",null, null);
        }

        string _rscText="";
        public StringParser SetResourceText(string rscText)
        {
            _rscText = rscText;
            return this;
        }

        string _srcFileName = "";
        public StringParser SetResourceFile(string filename)
        {
            _srcFileName = filename;
            _rscText = File.ReadAllText(filename);
            return this;
        }

        public StringParser ParseSource()
        {
            // 줄바꿈 문자를 \r\n으로 통일함
            // 위치 정보를 정확히 가져오기 위함
            _rscText = _rscText.Replace("\r\n", "\n");
            _rscText = _rscText.Replace("\r", "\n");
            _rscText = _rscText.Replace("\n", "\r\n");

            lines = _rscText.Split(new[] { "\r\n" }, StringSplitOptions.None);

            var document = new StringBuilderDocument(_rscText);
            var syntaxTree = SyntaxTree.Parse(document, document.FileName);

            foreach(var node in syntaxTree.Members )
            {
                RecRec(node);
            }

            return this;
        }
        string _refNamespace = "";
        string ProjectNamespace = "";
        public void RecRec(AstNode node)
        {
            string nodeRole = node.Role.ToString()??"";
            string nodeString = node.ToString();
            if(node.IsNull)
            {

            }
            string first = "";
            if(node.FirstChild!=null)
                first = node.FirstChild.ToString();
            // Meet using - skip using
            if (node.FirstChild != null && node.FirstChild.NodeType == NodeType.Token && first == "using")
                return;
            if(node is  PrimitiveExpression )
            {
                PrimitiveExpression nodej = (PrimitiveExpression)node;
                string nodeValue = nodej.Value.ToString()??"";
                if (nodej.Value is string)
                {
                    AddStr(nodeValue, nodeValue, _srcFileName, node.StartLocation, node.EndLocation);
                }
            }
            if (node.NodeType == NodeType.Expression)
            {
                if(nodeRole == "Expression" && node.HasChildren == false)
                {
                    //AddStr(nodeString, nodeString);
                }
                else if( nodeRole =="Expression")
                {
                    //
                }
                else
                {

                }    
            }
            if(node.NodeType == NodeType.Unknown)
            {
                if (nodeRole == "Member")
                {
                    
                }
                else
                {

                }
            }
            if (node.NodeType == NodeType.Whitespace)
                return;
            if (node.NodeType == NodeType.Token)
            {
                if (node.Role == Roles.Identifier)
                {
                    if (!string.IsNullOrWhiteSpace(_refNamespace) && nodeString =="App")
                    {
                        ProjectNamespace = _refNamespace;
                        _refNamespace = "";
                    }
                }
                else if(nodeRole== "Member")
                {
                    
                }
                else if(nodeRole == "{")
                {

                }
                else if(nodeRole == "Modifier")
                {

                }
            }
            if(nodeString == "namespace" && node.NodeType == NodeType.Token)
            {
                _refNamespace = nodeString;
                return;
            }
            // Meet namespace 


            foreach (var childNode in node.Children)
            {
                RecRec(childNode);
            }
        }


        public StringParser CreateResourceFile(string resource_filename, string backup_suffix, Dictionary<string, string> name_rsc, Dictionary<string, string> group_rsc)
        {
            List<string> Group = new List<string>();
            foreach (string vv in group_rsc.Values)
            {
                if (!Group.Contains(vv))
                    Group.Add(vv);
            }
            string tmp_filename = resource_filename + ".tmp";
            if (!string.IsNullOrWhiteSpace(backup_suffix))
            {
                File.Copy(resource_filename, resource_filename + backup_suffix, true);
            }
            File.Delete(tmp_filename);

            StreamWriter writer = File.CreateText(tmp_filename);

            writer.Write(@"
///
///   Auto Generated string Resource File

namespace GSCM.FP.Common.UIL
{
");
            foreach (string eachgroup in Group)
            {

                writer.Write("    public static class " + eachgroup + @"
    {
");
                ;
                //          public const string Y = "Y";
                foreach (KeyValuePair<string, string> kvp in name_rsc)
                {
                    string value = kvp.Key;
                    string key = kvp.Value;
                    if (value == "")
                    {

                    }
                    else if (group_rsc[key] == eachgroup)
                    {
                        writer.WriteLine(string.Format("        public const string {0} = \"{1}\";", key, value));
                    }
                }
                writer.Write(
                    @"   }
");
            }
            writer.Write(@"
}
");
            writer.Close();
            File.Copy(tmp_filename, resource_filename, true);
            File.Delete(tmp_filename);
            return this;
        }

        private bool _folderConversion = false;
        public StringParser SetFolderConversion(bool folderConversion)
        {
            this._folderConversion = folderConversion;
            return this;
        }

        private bool _createNewResource = false;
        public StringParser SetCreateNewResource(bool createNewResource)
        {
            _createNewResource = createNewResource;
            return this;
        }

        private string src_filename = "";
        public StringParser SetSourceFile(string SourceFile)
        {
            src_filename = SourceFile;
            return this;
        }

        private string src_foldername = "";
        public StringParser SetFolder(string Folder)
        {
            src_foldername = Folder;
            return this;
        }

        private bool _replaceSrcFile = false;

        public StringParser SetReplaceSource(bool ReplaceSource)
        {
            _replaceSrcFile = ReplaceSource;
            return this;
        }

        private bool _doNotReadResourceFile = false;
        public StringParser SetReadResourceFile(bool DoNotReadResourceFile)
        {
            _doNotReadResourceFile = !DoNotReadResourceFile;
            return this;
        }

        string _backupSuffix = "." + DateTime.Now.ToString("yyyyMMddHHmmm") + ".bak";
        
        public class L
        {
            public string Org = "";
            public string AutoKey = "";
            public string Key = "";
            public string A = "";
            public TextLocation? S;
            public TextLocation? E;
            public string? F;
            public int lineDelta = 0;
        }

        //private readonly Dictionary<string, L> _namedRsc = new Dictionary<string, L>(); // resource : resource_key
        //private readonly Dictionary<string, L> _groupRsc = new Dictionary<string, L>(); //

        public List<L> RawIDResultsOfAll = new List<L>();
        public List<L> RawStringResultsOfAll = new List<L>();

        private readonly Dictionary<string, string> _idDic = new Dictionary<string, string>(); // resource : resource_key
        private readonly Dictionary<string, string> _idRevDic = new Dictionary<string, string>(); // resource : resource_key
        private readonly Dictionary<string, string> _strDic = new Dictionary<string, string>(); //
        private readonly Dictionary<string, string> _strRevDic = new Dictionary<string, string>(); //

        /// <summary>
        /// string aaa = "fasfasfasdf" ==> 
        /// </summary>
        /// <returns></returns>

        public StringParser RunParser()
        {
            ParseSource();

            if (_createNewResource)
            {
            }

            if (_createNewResource)
            {

            }
            //CreateResourceFile(_rscFilename, _backupSuffix, _namedRsc, _groupRsc);
            //if (!_folderConversion)
            //{
            //    ProcessSrcFile(src_filename, _backupSuffix, _namedRsc, _groupRsc);
            //}
            //else
            //{
            //    ProcessFolder(src_foldername, _backupSuffix, _namedRsc, _groupRsc);
            //}

            //BuildResourceDictionary(_rscFilename, _namedRsc, _groupRsc);
            //CreateResourceFile(_rscFilename, _backupSuffix, _namedRsc, _groupRsc);
            return this;
        }

        string[] lines;
        public StringParser SaveTempFile(string targetFileName)
        {
            string tmpFileName = targetFileName + ".tmp";
            string src_txt = _rscText;


            //src_txt = src_txt.Replace("@\"\"", "string.Empty");
            //src_txt = src_txt.Replace("\"\"", "string.Empty");
            int i = 0;
            foreach (L eachRaw in RawStringResultsOfAll)
            {
                eachRaw.AutoKey = string.Format("G.auto_{0}", i++);
                src_txt = ReplaceFirstOccurence(src_txt, eachRaw.Org, eachRaw.AutoKey, eachRaw);
            }
            File.WriteAllText(tmpFileName, src_txt);
            
            return this;
        }

        string GetText(string[] srcLines, L rawInfo)
        {
            if (srcLines == null || rawInfo == null || rawInfo.S ==null || rawInfo.E==null)
                return "";
            TextLocation? s = rawInfo.S;
            TextLocation? e = rawInfo.E;

            int start_line = s.Value.Line;
            int start_pos = s.Value.Column;

            int end_line = e.Value.Line;
            int end_pos = e.Value.Column;

            string buf = "";
            string in_the_line = "";
            for(int line = start_line; line <= end_line; line++)
            {
                string line_text = srcLines[line-1];
                in_the_line = "";
                if(line== start_line && line == end_line)
                {
                    if (start_pos== end_pos)
                    {
                        return "";
                    }
                    buf = line_text.Substring(start_pos-1, end_pos - start_pos);
                    return buf;
                }
                else if(line == start_line && line != end_line)
                {
                    in_the_line = line_text.Substring(start_pos-1);
                    buf += in_the_line;
                }
                else if(line == end_line && line != start_line)
                {
                    if (end_pos > 1)
                    {
                        in_the_line = "\r\n" + line_text.Substring(0, end_pos - 1);
                        buf += in_the_line;
                    }
                    else
                    {
                        buf += "\r\n";
                    }
                }
                else
                {
                    in_the_line = line_text;
                    buf += "\r\n" + in_the_line;
                }
            }
            return buf;
        }
        public string ReplaceFirstOccurence(string inputString, string targetString, string replaceString, L rawRef)
        {
            try
            {
                string raw = GetText(lines, rawRef);
                string target = targetString;
                if (string.IsNullOrEmpty(targetString))
                    replaceString = string.Empty;
                if (string.IsNullOrWhiteSpace(target))
                    return inputString;
                int index = inputString.IndexOf(target);
                if (index >= 0)
                {
                    if (index > 0)
                    {
                        string at_check = inputString.Substring(index - 1, 1);
                        if (at_check == "@")
                        {
                            target = "@" + target;
                            index--;
                        }
                    }
                    return inputString.Remove(index, target.Length).Insert(index, replaceString);
                }
            }
            catch(Exception ee)
            {
                int x = 0;
            }
            return inputString;
            //throw new Exception("string not found");
        }

        void ProcessSrcFile(string src_filename, string backup_suffix, Dictionary<string, string> dictRsc, Dictionary<string, string> grpRsc)
        {
            string tmp_filename = src_filename + ".tmp";

            if (!string.IsNullOrWhiteSpace(backup_suffix))
            {
                File.Copy(src_filename, src_filename + backup_suffix, true);
            }
            File.Delete(tmp_filename);

            StreamWriter writer;
            StreamReader reader;
            reader = new StreamReader(src_filename);
            string? line = "";

            writer = File.CreateText(tmp_filename);
            _status = "";
            while ((line = reader.ReadLine()) != null)
            {
                ProcessLine(line, reader, writer, dictRsc, grpRsc);
            }
            _status = "";
            writer.Close();
            reader.Close();
            if (_replaceSrcFile)
            {
                File.Copy(tmp_filename, src_filename, true);
            }
            File.Delete(tmp_filename);

        }
        string _status = "";
        void ProcessLine(string line, StreamReader reader, StreamWriter writer, Dictionary<string, string> dictRsc, Dictionary<string, string> grpRsc)
        {
            if (line != "")
            {
                switch (_status)
                {
                    case "openlongnote":
                        {
                            //int quote = line.IndexOf("\"");
                            //int shortnote = line.IndexOf("//");
                            int closelongnote = line.IndexOf("*/");
                            if (closelongnote >= 0)
                            {
                                _status = "";
                            }
                            writer.WriteLine(line);
                        }
                        break;
                    case "":
                        {
                            int quote = line.IndexOf("\"");
                            int shortnote = line.IndexOf("//");
                            int longnote = line.IndexOf("/*");
                            if (quote < 0 && shortnote < 0 && longnote < 0)
                            {
                                writer.WriteLine(line);
                            }
                            else if (longnote >= 0 && (longnote < quote || quote == -1) && (longnote < shortnote || shortnote == -1))
                            {
                                writer.WriteLine(line);
                                _status = "openlongnote";
                            }
                            else if (shortnote >= 0 && (shortnote < quote || quote == -1) && (shortnote < longnote || longnote == -1))
                            {
                                writer.WriteLine(line);
                            }
                            else if (quote >= 0 && (quote < shortnote || shortnote == -1) && (quote < longnote || longnote == -1))
                            {
                                string quotestr = "";
                                string key_str = "";
                                int quoteclose = line.IndexOf("\"", quote + 1);
                                if (quoteclose > 0)
                                {
                                    quotestr = line.Substring(quote, quoteclose - quote + 1);
                                    quotestr = quotestr.Replace("\"", "");
                                    key_str = quotestr.ToUpper();
                                    key_str = key_str.Replace("/", "_");
                                    key_str = key_str.Replace(".", "_");
                                    key_str = key_str.Replace(" ", "_");
                                    key_str = key_str.Replace("=", "_");
                                    key_str = key_str.Replace(",", "_");
                                    key_str = key_str.Replace("[", "_");
                                    key_str = key_str.Replace("]", "_");
                                    key_str = key_str.Replace("<", "_");
                                    key_str = key_str.Replace(">", "_");
                                    key_str = key_str.Replace("!", "_");
                                    key_str = key_str.Replace("?", "_");
                                    key_str = key_str.Replace("+", "_");
                                    key_str = key_str.Replace("-", "_");
                                    key_str = key_str.Replace("(", "_");
                                    key_str = key_str.Replace(")", "_");
                                    key_str = key_str.Replace("'", "_");
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
                                    if (quotestr == "")
                                    {
                                        line = line.Replace("\"" + quotestr + "\"", "string.Empty");
                                        writer.WriteLine(line);
                                    }
                                    else if (dictRsc.ContainsKey(quotestr))
                                    {
                                        string group_name = grpRsc[dictRsc[quotestr]];
                                        //string old_var = dictRsc[quotestr];
                                        line = line.Replace("\"" + quotestr + "\"", group_name + "." + key_str);
                                        writer.WriteLine(line);
                                    }
                                    else
                                    {
                                        if (_doNotReadResourceFile == false)
                                        {
                                            dictRsc[quotestr] = key_str;
                                            grpRsc[key_str] = "R";
                                            line = line.Replace("\"" + quotestr + "\"", "R." + key_str);
                                            ProcessLine(line, reader, writer, dictRsc, grpRsc);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                writer.WriteLine(line);
                            }
                        }
                        break;
                }
            }
            else
            {
                writer.WriteLine(line);
            }
        }

        void ProcessFolder(string folderPath, string backup_suffix, Dictionary<string, string> rsc, Dictionary<string, string> grp)
        {

        }

        string ReadLine()
        {
            return "";
        }
    }
}
