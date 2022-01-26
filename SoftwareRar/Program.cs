using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.Win32;

namespace SoftwareRar
{
    public class Program
    {


        static void Main(string[] args)
        {
            NewMethod();
            if (args == null || args.Length == 0) return;
            if (args[0]=="-del")
            {
                Remove();
                return;
            }
            var mainPath = Path.GetFullPath(args[0]);
            var zipFile = Directory.GetParent(mainPath) + "\\" +
                          Path.GetFileName(mainPath) + @".zip"
                          ;
            var gitIgnores = Directory.GetFiles(mainPath, ".gitignore", SearchOption.AllDirectories);
            var files = Directory.GetFiles(mainPath, "", SearchOption.AllDirectories);
            var ignores = (from gitignore in gitIgnores from line in File.ReadAllLines(gitignore) where !line.Trim().StartsWith('#') && !string.IsNullOrEmpty(line.Trim()) select line.Trim());

            using var archive = ZipFile.Open(zipFile, ZipArchiveMode.Create);

            foreach (var file in files.Where(f => control(f, mainPath, ignores)))
            {
                archive.CreateEntryFromFile(file, Path.GetFileName(mainPath)+"\\"+file.Replace(mainPath + "\\", ""));
            }
            
        }

        public static void Remove()
        {
            RemoveContext();
            RemoveEnv();
        }

        #region MyRegion
        const string name = "PATH";
        const EnvironmentVariableTarget scope = EnvironmentVariableTarget.User; // or User
        static string oldValue = Environment.GetEnvironmentVariable(name, scope);
        static string path = AppDomain.CurrentDomain.BaseDirectory;

        static string appName = AppDomain.CurrentDomain.FriendlyName + ".exe";
        static string regName = AppDomain.CurrentDomain.FriendlyName;
        static string menuName = "Folder\\shell\\" + regName;
        static string command = $"Folder\\shell\\{regName}\\command";
        const string contextName = "Zip ignore";
        static string applicationName = (path + appName + " \"%1\"");


        #endregion
        private static void NewMethod()
        {
             //this is the pah to your program
            var reg = Registry.ClassesRoot.OpenSubKey(command);
            if (reg == null)
                AddContext();
            if (oldValue == null || oldValue.Contains(path)) return;
            var newValue = oldValue + @";" + path + ";";
            Environment.SetEnvironmentVariable(name, newValue, scope);

            
        }

        static void RemoveEnv()
        {
            if (oldValue == null || oldValue.Contains(path)) return;
            var newValue = oldValue.Replace(@";" + path + ";", "");
            Environment.SetEnvironmentVariable(name, newValue, scope);
        }
        static void AddContext()
        {
            RegistryKey regMenu = null;
            RegistryKey regCmd = null;
            try
            {
                regMenu = Registry.ClassesRoot.CreateSubKey(menuName);
                regMenu?.SetValue("", contextName);
                regCmd = Registry.ClassesRoot.CreateSubKey(command);
                regCmd?.SetValue("", applicationName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                regMenu?.Close();
                regCmd?.Close();
            }
        }

        static void RemoveContext()
        {
            try
            {
                var reg = Registry.ClassesRoot.OpenSubKey(command);
                if (reg != null)
                {
                    reg.Close();
                    Registry.ClassesRoot.DeleteSubKey(command);
                }
                reg = Registry.ClassesRoot.OpenSubKey(menuName);
                if (reg == null) return;
                reg.Close();
                Registry.ClassesRoot.DeleteSubKey(menuName);
            }
            catch (Exception ex)
            {
                Console.WriteLine
                    (ex.Message);
            }
        }
        private static bool control(string file, string mainPath, IEnumerable<string> ignores)
        {
            file = file.Replace(mainPath, "").Replace("/", "\\");
            return !ignores.Any(ignore => file.StartsWith(ignore.Replace("/", "\\")));
        }

    }
}