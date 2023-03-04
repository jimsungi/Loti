using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
    }
}
