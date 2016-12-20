using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;



namespace HelloCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Brak parametru ze sciezka.");
                return;
            }
            
            string directoryPath = args[0].ToLower();
          
            //Remove white-space characters from the start and end of path.
            directoryPath = directoryPath.Trim();
            
            bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (IsPathSyntaxCorrect(directoryPath, isWindows) == false)
            {
                if (isWindows == true)
                {
                    Console.WriteLine("Nieprawidlowy format sciezki w systemie Windows.");
                    Console.WriteLine("Poprawny format: d:\\kat");
                }
                else
                {
                    Console.WriteLine("Nieprawidlowy format sciezki w systemie typu Unix.");
                    Console.WriteLine("Poprawny format: /kat/kat2");
                }

                return;
            }
           
            directoryPath = RemoveSpacesFromFilePath(directoryPath, isWindows);

            if (Directory.Exists(directoryPath) == false)
            {
                Console.WriteLine("\nKatalog nie istnieje.");
                return;
            }

            try
            {
                Console.WriteLine("\nKatalog istnieje.\n");
                string [] filePaths = Directory.GetFiles(directoryPath);

                foreach(string filePath in filePaths)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(filePath);
                        FileAttributes fileAttributes = File.GetAttributes(filePath);
                        bool isHidden = false;

                        if ((fileAttributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                            isHidden = true;

                        DisplayPath(filePath, isWindows);
                        
                        Console.WriteLine("{0,-26} {1}", "Utworzenie:", fileInfo.CreationTime);
                        Console.WriteLine("{0,-26} {1}", "Ostatni dostep:", fileInfo.LastAccessTime);
                        Console.WriteLine("{0,-26} {1}", "Ostatni zapis:", fileInfo.LastWriteTime);
                        Console.WriteLine("{0, -26} {1}", "Plik jest ukryty:", isHidden);
                        Console.WriteLine("{0,-26} {1}\n", "Tylko do odczytu:", fileInfo.IsReadOnly);
                    }
                    catch (UnauthorizedAccessException uaE)
                    {
                        Console.WriteLine(uaE.Message + "\n" + uaE.StackTrace);
                    }
                    catch (PathTooLongException ptlEx)
                    {
                        Console.WriteLine(ptlEx.Message + "\n" + ptlEx.StackTrace);
                        Console.WriteLine("PathTooLongException jest czasami spowodowana zbyt długa nazwa katalogu w sciezce, a nie dlugoscia samej sciezki");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                    }

                }
                
            }
            catch (UnauthorizedAccessException uaE)
            {
                Console.WriteLine(uaE.Message + "\n" + uaE.StackTrace);
            }
            catch(PathTooLongException ptlEx)
            {
                Console.WriteLine(ptlEx.Message + "\n" + ptlEx.StackTrace);
                Console.WriteLine("PathTooLongException jest czasami spowodowana zbyt długa nazwa katalogu w sciezce, a nie dlugoscia samej sciezki");
                //Too long name of directory or filename in filepath. (article)
                //https://blogs.msdn.microsoft.com/jeremykuhne/2016/07/30/net-4-6-2-and-long-paths-on-windows-10/
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }
            
        }

        /// <summary>
        /// Checks with regular expression whether path has correct syntax in defined OS. 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isWindows"></param>
        /// <returns></returns>
        public static bool IsPathSyntaxCorrect(string path, bool isWindows)
        {
            if (isWindows)
            {
                if (Regex.IsMatch(path, @"(^[a-zA-Z]\:\\$|^[a-zA-Z]\:\\[^/:*?<>""|]*[^\\/:*?<>""|]$)") == false)
                   return false;
            }
            //Unix type OS
            else
            {
                if (Regex.IsMatch(path, @"^\/[^\0]*[^\0/]$|^\/$") == false)
                   return false;
            }

            return true;
        }

        /// <summary>
        /// //Removes white-space characters from the start and end of path segments.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="isWindows"></param>
        /// <returns></returns>
        public static string RemoveSpacesFromFilePath(string filePath, bool isWindows)
        {
            string trimmedPath = "";
            string[] pathSegments;

            if (isWindows)
            {
                pathSegments = filePath.Split(new char[] { '\\', });

                for (int i = 1; i < pathSegments.Length; i++)
                    pathSegments[i] = pathSegments[i].Trim();

                foreach (string segment in pathSegments)
                    trimmedPath += segment + "\\";

                return trimmedPath.TrimEnd(new char[] { '\\' });
            }
            else
            {
                pathSegments = filePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < pathSegments.Length; i++)
                    pathSegments[i] = pathSegments[i].Trim();

                foreach (string segment in pathSegments)
                    trimmedPath += "/" + segment;

                return trimmedPath;

            }
        }

        /// <summary>
        /// Displays multiline path based on length of path segments.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isWindows"></param>
        public static void DisplayPath(string path, bool isWindows)
        {
            //Max characters count in single line.
            int lineLimit = Console.WindowWidth;

            //Path is within a single line.
            if (path.Length < lineLimit)
            {
                Console.WriteLine("\n{0}\n", path);
                return;
            }

            char separator;
            string line = "";

            if (isWindows == false)
            {
                separator = '/';
                //Path in Unix-type Os starts with: /
                line += separator;
            }
            else
                separator = '\\';

            string [] pathParts = path.Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries);

            int position = 0;
            Console.WriteLine();
                        
            for (int i = 0; i < pathParts.Length; i++)
            {
                position += pathParts[i].Length + 1;

                //Is the first segment in line or fits within current line. 
                if (line == "" || line == "/" || position < lineLimit)
                    line += pathParts[i] + separator;
                else
                {
                    Console.WriteLine(line);
                    line = pathParts[i] + separator;
                    position = line.Length;
                }

                //The last segment in path.
                if (i == pathParts.Length - 1)
                {
                    line = line.TrimEnd(new char[] { separator });
                    Console.WriteLine(line);
                }
                   
            }

            Console.WriteLine();
        }
    }
}
