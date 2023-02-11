using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TigerL10N.Biz
{
    public class StringParser
    {
        public StringParser()
        {
            named_rsc.Add("", "");
            group_rsc.Add("", "R");
        }

        public StringParser ParseFile(string filename)
        {
            return this;
        }

        public StringParser BuildResourceDictionary(string rsc_filename, Dictionary<string, string> rsc, Dictionary<string, string> grp)
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

        private string _rscFilename;
        public StringParser SetResourceFile(string resourceFile)
        {
            _rscFilename = resourceFile;
            return this;
        }

        private string src_filename;
        public StringParser SetSourceFile(string SourceFile)
        {
            src_filename = SourceFile;
            return this;
        }

        private string src_foldername;
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

        string backup_suffix = "." + DateTime.Now.ToString("yyyyMMddHHmmm") + ".bak";
        Dictionary<string, string> named_rsc = new Dictionary<string, string>(); // resource : resource_key
        Dictionary<string, string> group_rsc = new Dictionary<string, string>(); //

        public bool RunParser()
        {
            if (_createNewResource)
            {
                BuildResourceDictionary(_rscFilename, named_rsc, group_rsc);

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
                    backup_suffix = "";
                }
            }
            CreateResourceFile(_rscFilename, backup_suffix, named_rsc, group_rsc);
            if (!_folderConversion)
            {
                ProcessSrcFile(src_filename, backup_suffix, named_rsc, group_rsc);
            }
            else
            {
                ProcessFolder(src_foldername, backup_suffix, named_rsc, group_rsc);
            }

            BuildResourceDictionary(_rscFilename, named_rsc, group_rsc);
            CreateResourceFile(_rscFilename, backup_suffix, named_rsc, group_rsc);
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
}
