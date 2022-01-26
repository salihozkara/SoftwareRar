using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;

namespace SoftwareRar
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args==null)
                args = new[] { @"C:\Users\salih\Desktop\deeme\Yeni klasör" };
            NewMethod();
            var mainPath = Path.GetFullPath(args[0]);
            var zipFile = Directory.GetParent(mainPath)+"\\"+
                          Path.GetFileName(mainPath)+@".zip"
                          ;
            Console.WriteLine(mainPath+Environment.NewLine+zipFile);
            Console.WriteLine(zipFile);
            var gitignores=Directory.GetFiles(mainPath, ".gitignore", SearchOption.AllDirectories);
            Console.WriteLine(JsonSerializer.Serialize(gitignores));
            var directories = Directory.GetDirectories(mainPath, "", SearchOption.AllDirectories);
            var files = Directory.GetFiles(mainPath, "",SearchOption.AllDirectories);
            //Console.WriteLine(JsonSerializer.Serialize(directories));
            List<string> ignores = new();
            
            foreach (var gitignore in gitignores)
            {
                foreach (var line in File.ReadAllLines(gitignore))
                {
                    if (!line.Trim().StartsWith('#')&&!string.IsNullOrEmpty(line.Trim()))
                    {
                        ignores.Add(line.Trim());
                    }
                }
            }
            bas(ignores);
            using var archive = ZipFile.Open(zipFile, ZipArchiveMode.Create);
          
            foreach (var file in files)
            {
                if (control(file,mainPath,ignores))
                {
                    archive.CreateEntryFromFile( file, file.Replace(mainPath+"\\",""));
                }
                
            }
            // Zip(zipFile, mainPath, "");
        }

        private static void NewMethod()
        {
            const string name = "PATH";
            const EnvironmentVariableTarget scope = EnvironmentVariableTarget.User; // or User
            var oldValue = Environment.GetEnvironmentVariable(name, scope);
            var path = Directory.GetCurrentDirectory();
            if (oldValue != null && !oldValue.Contains(path))
            {
                var newValue = oldValue + @";" + path + ";";
                Environment.SetEnvironmentVariable(name, newValue, scope);
            }
        }

        private static bool control(string file,string mainPath, IEnumerable<string> ignores)
        {
            file = file.Replace(mainPath , "").Replace("/","\\");
            return !ignores.Any(ignore =>file.StartsWith(ignore.Replace("/","\\")) );
        }
        private static void bas(object obj)
        {
            Console.WriteLine(JsonSerializer.Serialize(obj));
        }
        private static void Zip(string zipFile, string mainPath, string basePath)
        {
            using var archive = ZipFile.Open(zipFile, ZipArchiveMode.Create);
            foreach (var fPath in Directory.GetFiles(mainPath))
            {
                try
                {
                    archive.CreateEntryFromFile( fPath, basePath +Path.GetFileName(fPath));
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}