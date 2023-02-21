using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml;
using Microsoft.VisualBasic.CompilerServices;
using Prism.Mvvm;
using TigerL10N.Control;
using TigerL10N.Biz;

namespace TigerL10N.Service
{


    public class ProjectManageService
    {
        public static string ProjectExt = ".l10n.project";
        public static L10NProject CreateProject(string name, string filePath, string targetPath)
        {
            L10NProject pr = new(name, filePath)
            {
                RawPath = targetPath
            };




            return pr;
        }

        static void DirSearch(GoItemNode node, string sDir, bool hideUnchecked, Dictionary<string, ProjectFile>? fileSettings)
        {
            if (Directory.Exists(sDir))
            {
                bool? init = false;
                bool state_checker = false;
                node.Path = sDir;
                node.IsFile = false;
                node.IconKind = "Folder";
                node.ItemType = "Directory";
                node.Expand = true;
                //node.IsChecked = true;
                switch (node.Name.ToLower())
                {
                    case "bin":
                    case "lib":
                    case "obj":
                    case ".vs":
                    case "properties":
                    case "img":
                    case "packages":
                    case "resources":
                        node.Expand = false;
                        //node.IsChecked = false;
                        break;
                    default:
                        //node.IsChecked = true;
                        break;
                }
                init = node.IsChecked;
                try
                {
                    if (node.IsChecked == false && hideUnchecked)
                    {
                        // pass to add
                    }
                    else 
                    {
                        foreach (string d in Directory.GetDirectories(sDir))
                        {
                            GoItemNode dir_node = new GoItemNode(d);
                            foreach (string f in Directory.GetFiles(d))
                            {
                                GoItemNode file_node = new GoItemNode(f);
                                file_node.IsFile = true;
                                //file_node.IsChecked = false;
                                file_node.ItemType = "File";
                                file_node.IconKind = "FileOutline";
                                string? ext = System.IO.Path.GetExtension(f)?.ToLower();
                                switch (ext)
                                {
                                    case ".cs":
                                    case ".xaml":
                                        file_node.IsChecked = true;
                                        break;
                                    default:
                                        file_node.IsChecked = false;
                                        break;
                                }
                                if(init != file_node.IsChecked)
                                {
                                    state_checker = true;
                                }

                                if (file_node.IsChecked == false && hideUnchecked)
                                {
                                    // pass to add
                                }
                                else
                                {
                                    dir_node.Children.Add(file_node);
                                }
                            }
                            DirSearch(dir_node, d, hideUnchecked, fileSettings);
                            node.Children.Add(dir_node);
                            if (init != dir_node.IsChecked)
                                state_checker = true;
                        }
                    }
                    if(state_checker== true)
                    {
                        node.IsChecked = null;
                    }
                }
                catch (System.Exception excpt)
                {
                    Console.WriteLine(excpt.Message);
                }
            }
        }

        public static List<GoItemNode> ListFileTree(L10NProject? project, string filepath, bool hideUnchecked)
        {
            List<GoItemNode> treeView = new List<GoItemNode>();

            GoItemNode tv = new GoItemNode(filepath);
            DirSearch(tv, filepath, hideUnchecked, project?.ProjectFiles);

            treeView.Add(tv);
            return treeView;
        }

        public static L10NProject? GetCurrentProject()
            {
            return L10NProject.Current;
            }

        public static L10NProject? LoadFromXml(string xmlFileName)
        {
            var xml = File.ReadAllText(xmlFileName);
            System.Xml.XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNode? root = doc.DocumentElement as XmlNode;

            string projectName = "";
            string? projectFolder = "";
            string targetFilesFolder = "";
            projectFolder = Path.GetDirectoryName(xmlFileName);
            if (root != null)
            {
                XmlNodeList? headers = doc.GetElementsByTagName("Info");
                XmlNode? info = headers?[0];
                if (info != null)
                {
                    foreach(XmlNode infonode in info.ChildNodes)
                    {
                        if(infonode.NodeType == XmlNodeType.Element)
                        {
                            if(infonode.Name == "Name")
                            {
                                projectName = infonode.InnerText;
                            }
                            else if(infonode.Name == "Source")
                            {
                                targetFilesFolder= infonode.InnerText;
                            }
                        }
                        
                    }
                }
                if(!string.IsNullOrWhiteSpace(projectFolder)
                    && !string.IsNullOrWhiteSpace(projectName))
                {
                    L10NProject project = new L10NProject(projectName, projectFolder);
                    project.RawPath = targetFilesFolder;

                    return project;
                }
                   
                

            }

            return null;
        }
    }

    public class ProjectFile
    {
        string FileName="";
        bool Include=true;

    }
}
