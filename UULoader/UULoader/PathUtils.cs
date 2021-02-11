using System;
using System.IO;

namespace UULoader
{
    public static class PathUtils
    {
        public static string ProcessName { get; internal set; }
        public static string ExePath { get; internal set; }
        public static string DLLPath { get; internal set; }
        public static string ManagedPath { get; internal set; }
        public static string ModsPath { get; internal set; }

        internal static void FindPaths()
        {
            DLLPath = Environment.GetEnvironmentVariable("DOORSTOP_INVOKE_DLL_PATH");
            ManagedPath = Environment.GetEnvironmentVariable("DOORSTOP_MANAGED_FOLDER_DIR");
            ExePath = Environment.GetEnvironmentVariable("DOORSTOP_PROCESS_PATH");
            ProcessName = Path.GetFileNameWithoutExtension(ExePath);
            ModsPath = Path.GetFullPath(Path.Combine(ManagedPath, @"..\..\UULoader\Mods"));
            Directory.CreateDirectory(ModsPath);
            Console.WriteLine($"Process Name: {ProcessName}");
            Console.WriteLine($"Exe Path: {ExePath}");
            Console.WriteLine($"DLL Path: {DLLPath}");
            Console.WriteLine($"Managed Path: {ManagedPath}");
            Console.WriteLine($"Mods Path: {ModsPath}");
        }
    }
}