using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ParadoxModInstaller
{
    internal class Program
    {
        static string currentDir = Environment.CurrentDirectory;
        const string DESC_FILE = "descriptor.mod";
        static void Main(string[] args)
        {
            List<string> modList = FindNotInstalledMods();
            try
            {
                InstallMods(modList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }

            Console.WriteLine("Задача выполнена. Бог император защитит.");
            Console.ReadLine();
        }

        static List<string> FindNotInstalledMods()
        {
            List<string> folders = Directory.GetDirectories(currentDir).ToList();
            List<string> mods = new List<string>();
            for (int i = 0; i < folders.Count(); i++)
            {
                string folderName = Path.GetFileName(folders[i].TrimEnd(Path.DirectorySeparatorChar));
                string modDesc = Path.Combine(currentDir, folderName + ".mod");
                string workshopDesc = Path.Combine(folders[i], DESC_FILE);
                if (!File.Exists(modDesc) && File.Exists(workshopDesc)) mods.Add(folders[i]);
            }
            return mods;
        }

        static void CheckZip(string mod)
        {
            Console.WriteLine("Проверка наличия архивов");
            string archive = File.ReadAllLines(Path.Combine(mod, DESC_FILE)).Where(x => x.Contains("archive=")).FirstOrDefault();
            if (archive != null)
            {
                Console.WriteLine("Обнаружен архив");
                if (Directory.EnumerateDirectories(mod).Count() == 0)
                {
                    Console.WriteLine("Не обнаружены папки мода, распаковка архива...");
                    archive = archive.Split('=')[1].Trim('"');
                    Unpack(mod, archive);
                }
            }
            else return;
        }

        static void Unpack(string mod, string archiveName)
        {
            Console.WriteLine("Распаковка архива " + archiveName);
            System.IO.Compression.ZipFile.ExtractToDirectory(Path.Combine(mod, archiveName), mod);
            
        }

        static void InstallMods(List<string> mods)
        {
            for (int i = 0; i < mods.Count; i++)
            {
                Console.WriteLine("Обнаружен мод для установки - " + mods[i]);
                CheckZip(mods[i]);
                Console.WriteLine("Корректировка имени.");
                mods[i] = CorrectName(mods[i]);
                Console.WriteLine("Копирование файла описания.");
                CreateModDesc(mods[i]);
                Console.WriteLine("Модификация файла описания.");
                ModifyModDesc(mods[i]);
                Console.WriteLine("Мод установлен.");
            }
        }

        static string CorrectName(string mod)
        {
            string folderName = Path.GetFileName(mod.TrimEnd(Path.DirectorySeparatorChar));
            string modDesc = Path.Combine(mod, DESC_FILE);
            List<string> file = File.ReadAllLines(modDesc).ToList();
            string name = file.Where(x => x.StartsWith("name=")).FirstOrDefault();
            name = name.Remove(0, 5);
            name = string.Concat(name.Split(Path.GetInvalidFileNameChars()));

            if (Regex.IsMatch(name, @"[^\u0000-\u007F]+"))
            {
                name = Regex.Replace(name, @"[^\u0000-\u007F]+", string.Empty);
                name += "Rus";
            }
            string newDir = Path.Combine(currentDir, name);
            if (folderName != name)
            {
                try { Directory.Move(mod, newDir); }
                catch(Exception ex) 
                {
                    Console.WriteLine(mod);
                    Console.WriteLine("В папке мода есть файлы открытые другим приложением, закройте приложения использующие файлы из папки мода и повторите попытку");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
                mod = newDir;
            }
            while (!Directory.Exists(newDir)) { continue; };
            return newDir;
        }

        static string CreateModDesc(string mod)
        {
            string folderName = Path.GetFileName(mod.TrimEnd(Path.DirectorySeparatorChar));
            string modDesc = Path.Combine(currentDir, folderName + ".mod");
            if (File.Exists(Path.Combine(modDesc))) File.Delete(modDesc);
            Console.WriteLine("Обнаружен существующий файл мода, удаление...");
            File.Copy(Path.Combine(mod, DESC_FILE), modDesc);
            return modDesc;
        }

        static void ModifyModDesc(string mod)
        {
            string folderName = Path.GetFileName(mod.TrimEnd(Path.DirectorySeparatorChar));
            string modDesc = Path.Combine(currentDir, folderName + ".mod");
            List<string> file = File.ReadAllLines(modDesc).ToList();
            bool stringFound = false;
            string rightPath = "\"" + mod.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + "\"";
            for (int i = 0; i < file.Count(); i++)
            {
                if (file[i].StartsWith("path")) { file[i] = "path=" + rightPath; stringFound = true; }
            }
            if (!stringFound) file.Add("path=" + rightPath);
            File.WriteAllLines(modDesc, file);
        }
    }
}
