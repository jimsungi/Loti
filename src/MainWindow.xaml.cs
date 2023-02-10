using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           Ookii.Dialogs.Wpf.VistaOpenFileDialog od = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            //Ookii.Dialogs.Wpf.VistaFolderBrowserDialog od = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            bool? res = od.ShowDialog();
            if(res!=null && res== true)
            {
                txtSrcFile.Text = od.FileName;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaOpenFileDialog od = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            //Ookii.Dialogs.Wpf.VistaFolderBrowserDialog od = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            bool? res = od.ShowDialog();
            if (res != null && res == true)
            {
                txtTarFile.Text = od.FileName;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog od = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            bool? res = od.ShowDialog();
            if (res != null && res == true)
            {
                txtSrcFile.Text = od.SelectedPath.ToString();
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            bool bCreateNewResource = false;
            bool bFolderConversion = false;
            string rsc_filename = txtTarFile.Text;
            string src_filename = txtSrcFile.Text;
            string src_foldername = txtSrcFolder.Text;
            string backup_suffix = "." + DateTime.Now.ToString("yyyyMMddHHmmm") + ".bak";
            if (!File.Exists(src_filename))
            {
                bFolderConversion = true;
            }

            if (bFolderConversion && !File.Exists(src_foldername))
            {
                MessageBox.Show("바꿀 대상이 없습니다.");
                return;
            }

            if (!File.Exists(rsc_filename))
            {
                MessageBox.Show("새 파일에 결과를 저장합니다");
                bCreateNewResource = true;
            }


            Dictionary<string, string> named_rsc = new Dictionary<string, string>(); // resource : resource_key
            Dictionary<string, string> group_rsc = new Dictionary<string, string>(); //
            named_rsc.Add("", "");
            group_rsc.Add("", "R");
            if (bCreateNewResource)
            {

            }
            else
            {
                BuildResourceDictionary(rsc_filename, named_rsc, group_rsc);
            }

            if (!bFolderConversion)
            {
                ProcessSrcFile(src_filename, backup_suffix, named_rsc, group_rsc);
            }
            else
            {
                ProcessFolder(src_foldername, backup_suffix, named_rsc, group_rsc);
            }
            if (bCreateNewResource)
            {
                VistaOpenFileDialog newFile = new VistaOpenFileDialog();
                if (newFile.ShowDialog() == true)
                {
                    rsc_filename = newFile.FileName;
                }
                else
                {
                    rsc_filename = "c:\\test.cs";
                    backup_suffix = "";
                }
            }
            CreateResourceFile(rsc_filename, backup_suffix, named_rsc, group_rsc);
        }

        void CreateResourceFile(string resource_filename, string backup_suffix, Dictionary<string, string> name_rsc, Dictionary<string, string> group_rsc)
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

            StreamWriter writer;
            writer = File.CreateText(tmp_filename);

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
            status = "";
            while ((line = reader.ReadLine()) != null)
            {
                ProcessLine(line, reader, writer, dictRsc, grpRsc);
            }
            status = "";
            writer.Close();
            reader.Close();
            if (chkReplace.IsChecked == true)
            {
                File.Copy(tmp_filename, src_filename, true);
            }
            File.Delete(tmp_filename);

        }
        string status = "";
        void ProcessLine(string line, StreamReader reader, StreamWriter writer, Dictionary<string, string> dictRsc, Dictionary<string, string> grpRsc)
        {
            if (line != "")
            {
                switch (status)
                {
                    case "openlongnote":
                        {
                            //int quote = line.IndexOf("\"");
                            //int shortnote = line.IndexOf("//");
                            int closelongnote = line.IndexOf("*/");
                            if (closelongnote >= 0)
                            {
                                status = "";
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
                                status = "openlongnote";
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
                                        if (chkNoread.IsChecked==false)
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
        void BuildResourceDictionary(string rsc_filename, Dictionary<string, string> rsc, Dictionary<string, string> grp)
        {
            string[] lines = File.ReadAllLines(rsc_filename);
            if (lines != null && lines.Length > 0)
            {
                string GroupName = "R";
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (line.Contains("public static class "))
                    {
                        string old_group = GroupName;
                        string clean = line.Replace("public static class ", "").Trim();
                        string[] unnote = clean.Split('/');
                        if (unnote.Length > 0)
                        {
                            GroupName = unnote[0].Trim();
                        }
                        else
                        {
                            GroupName = clean;
                        }
                        if (string.IsNullOrWhiteSpace(GroupName))
                            GroupName = old_group;
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
                                grp[key] = GroupName;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Check public const string");
                        }
                    }
                }
            }
        }
        string ReadLine()
        {
            return "";
        }
    }
}
