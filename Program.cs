using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
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
            InstallMods(modList);

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

        static void InstallMods(List<string> mods) 
        {
            for (int i = 0; i < mods.Count; i++)
            {
                Console.WriteLine("Обнаружен мод для установки - " + mods[i]);
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
                Directory.Move(mod, newDir);
                mod = newDir;
            }
            while (!Directory.Exists(newDir)) { continue; };
            return newDir;
        }

        static string CreateModDesc(string mod) 
        { 
            string folderName = Path.GetFileName(mod.TrimEnd(Path.DirectorySeparatorChar));
            string modDesc = Path.Combine(currentDir, folderName + ".mod");
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
