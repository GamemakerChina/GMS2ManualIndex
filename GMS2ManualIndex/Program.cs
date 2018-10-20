using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMS2ManualIndex
{
    class Program
    {
        private static bool argLevel = true;
        private static string argFileName = "";
        private static string rootDir;

        static void Main(string[] args)
        {
            // 参数
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-level" || args[i] == "-l")
                {
                    if (args.Length > i + 1)
                    {
                        if (args[i + 1].ToLower() == "y")
                        {
                            argLevel = true;
                        }
                        else if (args[i + 1].ToLower() == "n")
                        {
                            argLevel = false;
                        }
                        else
                        {
                            Console.WriteLine("-level参数不正确！y为生成层级格式，n为生成搜索格式，不设置即默认为生成层级格式");
                            Environment.Exit(-1);
                        }
                    }
                    else
                    {
                        Console.WriteLine("-level参数不正确！y为生成层级格式，n为生成搜索格式，不设置即默认为层级模式");
                        Environment.Exit(-1);
                    }
                    break;
                }
            }
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-name" || args[i] == "-n")
                {
                    if (args.Length > i + 1)
                    {
                        if (!args[i + 1].Contains(".html"))
                        {
                            argFileName = args[i + 1] + ".html";
                        }
                        else
                        {
                            argFileName = args[i + 1];
                        }
                        Console.WriteLine("输出文件名：" + argFileName);
                    }
                    else
                    {
                        Console.WriteLine("-name参数不正确！");
                        Environment.Exit(-1);
                    }
                    break;
                }
            }
            Console.WriteLine(argLevel ? "层级格式" : "搜索格式");
            if (argFileName == "")
            {
                argFileName = argLevel ? "level.html" : "search.html";
                Console.WriteLine("输出文件名：" + argFileName);
            }

            rootDir = Environment.CurrentDirectory;
            if (File.Exists(rootDir + "\\" + argFileName))
            {
                File.Delete(rootDir + "\\" + argFileName);
            }
            if (argLevel)
            {
                WriteLevelFile(rootDir, 1);
            }
            else
            {
                WriteSearchFile(rootDir, 1);
            }
            FileWriter("</ul>");
            Console.WriteLine("输出完毕！");
        }

        /// <summary>
        /// 写入层级文件
        /// </summary>
        /// <param name="sourcePath">路径</param>
        /// <param name="depth">深度</param>
        private static void WriteLevelFile(string sourcePath, int depth)
        {
            DirectoryInfo Dir = new DirectoryInfo(sourcePath);
            DirectoryInfo[] DirSub = Dir.GetDirectories();
            bool existIndex = false;
            List<string> fileNameList = new List<string>();
            var fileInfos = Dir.GetFiles("*.html", SearchOption.TopDirectoryOnly);
            FileStream fileStream = new FileStream(rootDir + "\\" + argFileName, FileMode.Append);
            StreamWriter streamWriter = new StreamWriter(fileStream);
            if (depth == 1)
            {
                streamWriter.WriteLine("<ul>");
                streamWriter.Flush();
            }
            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.ToString() == "index.html" && depth != 1)
                {
                    existIndex = true;
                    streamWriter.WriteLine(getSpace((int)Math.Pow(2, depth - 2) - 1) + "<li>");
                    streamWriter.WriteLine(getSpace((int)Math.Pow(2, depth - 2))
                                           + "<a href=\""
                                           + (Dir + @"\" + fileInfo).Replace(rootDir + "\\", "")
                                           + "\">"
                                           + getTitle(Dir + @"\" + fileInfo)
                                           + "</a>");
                    streamWriter.Flush();
                }
                else
                {
                    fileNameList.Add(fileInfo.ToString());
                }
            }
            if (depth != 1 && fileInfos.Length != 0)
            {
                streamWriter.WriteLine(getSpace((int)Math.Pow(2, depth - 2)) + "<ul>");
                streamWriter.Flush();
            }
            foreach (var fileName in fileNameList) //查找文件
            {
                string title = getTitle(Dir + @"\" + fileName);
                if (title != "$获取失败$")
                {
                    streamWriter.WriteLine(getSpace(depth * 2 - 3) +
                                           "<li><a href=\"" +
                                           (Dir + @"\" + fileName).Replace(rootDir + "\\", "")
                                           + "\">"
                                           + title
                                           + "</a></li>");
                    streamWriter.Flush();
                }
                else
                {
                    streamWriter.WriteLine(getSpace(depth * 2 - 3) +
                                           "<!--<li><a href=\"" +
                                           (Dir + @"\" + fileName).Replace(rootDir + "\\", "")
                                           + "\">"
                                           + title
                                           + "</a></li>-->");
                    streamWriter.Flush();
                }
            }

            streamWriter.Close();
            fileStream.Close();

            foreach (DirectoryInfo directoryInfo in DirSub)//查找子目录 
            {
                if (directoryInfo.Name != "images")
                    WriteLevelFile(Dir + @"\" + directoryInfo, depth + 1);
            }

            FileStream fileStream2 = new FileStream(rootDir + "\\" + argFileName, FileMode.Append);
            StreamWriter streamWriter2 = new StreamWriter(fileStream2);
            if (depth != 1 && fileInfos.Length != 0)
            {
                streamWriter2.WriteLine(getSpace((int)Math.Pow(2, depth - 2)) + "</ul>");
                streamWriter2.Flush();
            }
            if (existIndex)
            {
                streamWriter2.WriteLine(getSpace((int)Math.Pow(2, depth - 2) - 1) + "</li>");
                streamWriter2.Flush();
            }
            streamWriter2.Close();
            fileStream2.Close();

        }

        /// <summary>
        /// 写入搜索文件
        /// </summary>
        /// <param name="sourcePath">路径</param>
        /// <param name="depth">深度</param>
        private static void WriteSearchFile(string sourcePath, int depth)
        {
            FileStream fileStream = new FileStream(rootDir + "\\" + argFileName, FileMode.Append);
            StreamWriter streamWriter = new StreamWriter(fileStream);
            DirectoryInfo Dir = new DirectoryInfo(sourcePath);
            DirectoryInfo[] DirSub = Dir.GetDirectories();
            List<string> fileNameList = new List<string>();
            if (depth == 1)
            {
                streamWriter.WriteLine("<ul>");
            }
            foreach (var fileName in Dir.GetFiles("*.html", SearchOption.TopDirectoryOnly)) //查找文件
            {
                if (depth == 1 && fileName.ToString() != "index.html")
                    continue;
                string title = getTitle(Dir + @"\" + fileName);
                if (title != "$获取失败$")
                {
                    streamWriter.WriteLine(getSpace(1) +
                                           "<li><a href=\"" +
                                           (Dir + @"\" + fileName).Replace(rootDir + "\\", "")
                                           + "\">"
                                           + title
                                           + "</a></li>");
                    streamWriter.Flush();
                }
                else
                {
                    streamWriter.WriteLine(getSpace(1) +
                                           "<!--<li><a href=\"" +
                                           (Dir + @"\" + fileName).Replace(rootDir + "\\", "")
                                           + "\">"
                                           + title
                                           + "</a></li>-->");
                    streamWriter.Flush();
                }
            }

            streamWriter.Close();
            fileStream.Close();

            foreach (DirectoryInfo directoryInfo in DirSub)//查找子目录 
            {
                if (directoryInfo.Name != "images")
                    WriteSearchFile(Dir + @"\" + directoryInfo, depth + 1);
            }
        }

        /// <summary>
        /// 缩进
        /// </summary>
        /// <param name="tabNum"></param>
        /// <returns></returns>
        private static string getSpace(int tabNum)
        {
            var space = "";
            for (var i = 0; i < tabNum * 4; i++)
            {
                space += " ";
            }
            return space;
        }

        /// <summary>
        /// 获取HTML title标签内容
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>title</returns>
        private static string getTitle(string path)
        {
            StreamReader sr = new StreamReader(path, Encoding.UTF8);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains("<title>") && line.Contains("</title>"))
                {
                    line = line.Replace("<title>", "").Replace("</title>", "");
                    return line;
                }
            }
            return "$获取失败$";
        }

        /// <summary>
        /// 单行文件追加
        /// </summary>
        /// <param name="writeLineString">追加字符串</param>
        private static void FileWriter(string writeLineString)
        {
            FileStream fileStream = new FileStream(rootDir + "\\" + argFileName, FileMode.Append);
            StreamWriter streamWriter = new StreamWriter(fileStream);
            streamWriter.WriteLine(writeLineString);
            streamWriter.Flush();
            streamWriter.Close();
            fileStream.Close();
        }
    }
}
