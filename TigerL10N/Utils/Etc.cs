using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TigerL10N.Biz;

namespace TigerL10N.Utils
{
    public class Etc
    {
        public static int CountLines(string inputString)
        {
            return inputString.Split('\n').Length;
        }

        public static string ReadNthLine(string filePath, int n)
        {
            string[] lines = File.ReadAllLines(filePath);
            if (n < 1 || n > lines.Length)
            {
                throw new ArgumentException("n is out of range");
            }
            return lines[n - 1];
        }

        public static List<XmlNode> FindElement(XmlNode node, int depth, params string[] pathNodeNames)
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

        public  static List<XmlNode> GetNodeList(XmlNodeList list)
        {
            List<XmlNode> nodes = new List<XmlNode>();
            foreach (XmlNode node in list)
            {
                nodes.Add(node);
            }
            return nodes;
        }

        public static void ToFirst<T>(List<T> list, Func<T, bool> predicate) where T : class
        {
            T? item = list.SingleOrDefault(predicate);

            if (item != null)
            {
                list.Remove(item);
                list.Insert(0, item);
            }
        }

        public static void CopyDirectory(string sDir, string tDir, IgnoreOption option)
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
                            CopyDirectory(ChildSourceDir, ChildTargetDir, option);
                        }
                    }

                    foreach (string ChildSourceFile in Directory.GetFiles(sDir))
                    {
                        string fName = Path.GetFileName(ChildSourceFile);
                        string ChildTargetFile = tDir + Path.DirectorySeparatorChar + fName;

                        string? ext = System.IO.Path.GetExtension(ChildSourceFile)?.ToLower();
                        if (ext != null && !option.IgnoreFiles.Contains(ext.ToLower()))
                        {
                            File.Copy(ChildSourceFile, ChildTargetFile, true);
                        }
                    }


                }
                catch (System.Exception excpt)
                {
                    Console.WriteLine(excpt.Message);
                }
            }
        }

        public enum Step
            {
            None = 0,
            Project = 2,
            Done=1
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="funcForDir"></param>
        /// <param name="funcForFile"></param>
        public static void RecurActionDirectory(string directoryPath, Action<string,Step> funcForDir, Action<string,Step> funcForFile, Step step, IgnoreOption? option=null)
        {
            // 현재 디렉터리의 파일 및 디렉터리를 가져옵니다.
            string[] files = Directory.GetFiles(directoryPath);
            string[] directories = Directory.GetDirectories(directoryPath);

            // 현재 디렉터리의 모든 파일에 대해 함수를 실행합니다.
            foreach (string file in files)
            {
                string? ext = System.IO.Path.GetExtension(file)?.ToLower();
                if (option==null || (ext != null && !option.IgnoreFiles.Contains(ext.ToLower()) ))
                {
                    funcForFile(file, step);
                }
            }

            funcForDir(directoryPath,step);
            // 하위 디렉터리에 대해 재귀적으로 함수를 실행합니다.
            foreach (string directory in directories)
            {
                string? dirName = Path.GetDirectoryName(directory);
                dirName = Path.GetFileName(directory);
                if (option == null || (dirName != null && !option.IgnoreFolder.Contains(dirName.ToLower())))
                {
                    RecurActionDirectory(directory, funcForDir, funcForFile, step, option);
                }
            }
        }
    }
}
