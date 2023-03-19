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
        public static string ProjectFolder = ".L10N";
        public static string ProjectExt = ".l10n";
        public static LSolution Solution = new LSolution();

        public static LSolution CreateSolution(string vsProjectName)
        {
            return new LSolution(vsProjectName);
        }

        //public static LProject CreateLproject(string name, string filePath, string targetPath)
        //{
        //    LProject pr = new(name, filePath)
        //    {
        //        RawPath = targetPath
        //    };
        //    return pr;
        //}

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
                    case ".ln":
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
                        foreach (string f in Directory.GetFiles(sDir))
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
                            if (init != file_node.IsChecked)
                            {
                                state_checker = true;
                            }

                            if (file_node.IsChecked == false && hideUnchecked)
                            {
                                // pass to add
                            }
                            else
                            {
                                node.Children.Add(file_node);
                            }
                        }
                        foreach (string d in Directory.GetDirectories(sDir))
                        {
                            GoItemNode dir_node = new GoItemNode(d);

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

        public static List<GoItemNode> ListFileTree(LProject? project, string filepath, bool hideUnchecked)
        {
            List<GoItemNode> treeView = new List<GoItemNode>();

            GoItemNode tv = new GoItemNode(filepath);
            DirSearch(tv, filepath, hideUnchecked, project?.ProjectFiles);

            treeView.Add(tv);
            return treeView;
        }

        public static List<GoItemNode> ListSolutionTree(LSolution? solution, string filepath, bool hideUnchecked)
        {
            List<GoItemNode> treeView = new List<GoItemNode>();

            GoItemNode root = new GoItemNode(filepath);
            
            RecurseSolutionTree(solution,root, filepath, hideUnchecked);

            treeView.Add(root);
            return treeView;
        }

        static void RecurseSolutionTree(LSolution? solution,GoItemNode baseNode, string baseNodePath, bool hideUnchecked)
        {
            if (Directory.Exists(baseNodePath))
            {
                bool? init = false;
                bool state_checker = false;
                baseNode.Path = baseNodePath;
                baseNode.IsFile = false;

                baseNode.ItemType = "Directory";


                //node.IsChecked = true;
                switch (baseNode.Name.ToLower())
                {
                    case "bin":
                    case "lib":
                    case "obj":
                    case ".vs":
                    case ".ln":
                    case "properties":
                    case "img":
                    case "packages":
                    case "resources":
                        baseNode.IconKind = "FolderHidden";
                        baseNode.Expand = false;
                        return;
                        //node.IsChecked = false;
                        //break;
                    default:
                        baseNode.IconKind = "Folder";
                        baseNode.Expand = true;
                        //node.IsChecked = true;
                        break;
                }
                init = baseNode.IsChecked;
                try
                {
                    if (baseNode.IsChecked == false && hideUnchecked)
                    {
                        // pass to add
                    }
                    else
                    {
                        foreach (string childFile in Directory.GetFiles(baseNodePath))
                        {
                            GoItemNode childFileNode = new GoItemNode(childFile);
                            childFileNode.IsFile = true;
                            //file_node.IsChecked = false;
                            childFileNode.ItemType = "File";
                            string? ext = System.IO.Path.GetExtension(childFile)?.ToLower();
                            switch (ext)
                            {
                                case ".ln":
                                    childFileNode.IconKind = "CheckBold";
                                    childFileNode.IsChecked = true;
                                    
                                    break;
                                case ".sln":
                                    childFileNode.IconKind = "CheckAll";
                                    childFileNode.IsChecked = true;
                                    baseNode.IconKind = "AlphaSBoxOutline";
                                    break;
                                case ".csproj":
                                    childFileNode.IconKind = "Check";
                                    childFileNode.IsChecked = true;
                                    baseNode.IconKind = "AlphaPBoxOutline";
                                    if (solution != null && solution.Projects !=null)
                                    {
                                        LProject project = new LProject(childFile);
                                        project.Solution = Solution;
                                        baseNode.SetProject(project);
                                        childFileNode.SetProject(project);
                                        solution.Projects.Add(project);
                                    }
                                    break;
                                case ".cs":
                                    childFileNode.IconKind = "LanguageCsharp";
                                    childFileNode.IsChecked = true;
                                    break;
                                case ".xaml":
                                    childFileNode.IconKind = "LanguageXaml";
                                    childFileNode.IsChecked = true;
                                    break;
                                default:
                                    childFileNode.IconKind = "File";
                                    childFileNode.IsChecked = false;
                                    break;
                            }
                            if (init != childFileNode.IsChecked)
                            {
                                state_checker = true;
                            }

                            if (childFileNode.IsChecked == false && hideUnchecked)
                            {
                                // pass to add
                            }
                            else
                            {
                                baseNode.Children.Add(childFileNode);
                            }
                        }
                        foreach (string childDirectoryNodePath in Directory.GetDirectories(baseNodePath))
                        {
                            GoItemNode childDirectoryNode = new GoItemNode(childDirectoryNodePath);

                            RecurseSolutionTree(solution,childDirectoryNode, childDirectoryNodePath, hideUnchecked);
                            baseNode.Children.Add(childDirectoryNode);
                            if (init != childDirectoryNode.IsChecked)
                                state_checker = true;
                        }
                    }
                    if (state_checker == true)
                    {
                        baseNode.IsChecked = null;
                    }
                }
                catch (System.Exception excpt)
                {
                    Console.WriteLine(excpt.Message);
                }
            }
        }

        public static LProject? GetCurrentProject()
            {
            return LProject.Current;
            }

        public static LProject? LoadFromXml(string xmlFileName)
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
                    LProject project = new LProject(targetFilesFolder);
                    //project.RawPath = targetFilesFolder;

                    return project;
                }
                   
                

            }

            return null;
        }
    }
    [Serializable]
    public class ProjectFile
    {
        //string FileName="";
        //bool Include=true;
        public ProjectFile()
        {

        }

    }
}
