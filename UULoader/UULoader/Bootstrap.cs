using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace UULoader
{
    public static class Bootstrap
    {
        private static string ManagedPath;
        private static string GamePath;
        internal static Config config;

        public static void Main(string[] args)
        {
            try
            {
                ManagedPath = Environment.GetEnvironmentVariable("DOORSTOP_MANAGED_FOLDER_DIR");
                GamePath = Path.GetFullPath(ManagedPath + @"\..\..\");

                string ConfigPath = Path.GetFullPath(Path.Combine(ManagedPath, @"..\..\UULoader\config.json"));
                if (File.Exists(ConfigPath))
                {
                    string lines = File.ReadAllText(ConfigPath);
                    config = JsonConvert.DeserializeObject<Config>(lines);
                }
                else config = new Config();


                using (AssemblyDefinition mainDef = AssemblyDefinition.ReadAssembly(GamePath + "UULoader\\UULoader.dll"))
                {
                    if (config.useDefault)
                    {
                        string dll = "UnityEngine.CoreModule.dll";
                        if (!File.Exists(Path.Combine(ManagedPath, "UnityEngine.CoreModule.dll")))
                            dll = "UnityEngine.dll";

                        using (AssemblyDefinition unityCoreDef =
                            AssemblyDefinition.ReadAssembly(Path.Combine(ManagedPath, dll)))
                        {

                            TypeDefinition applicationDef =
                                unityCoreDef.MainModule.Types.FirstOrDefault((TypeDefinition x) =>
                                    x.Name == "Application");

                            MethodDefinition startInfo = mainDef.MainModule.Types
                                .First((TypeDefinition x) => x.Name == "UULoaderMain")
                                .Methods.First((MethodDefinition x) => x.Name == "Start");
                            MethodReference startMethod = unityCoreDef.MainModule.ImportReference(startInfo);

                            MethodDefinition constructor =
                                applicationDef.Methods.FirstOrDefault((MethodDefinition m) =>
                                    m.IsConstructor && m.IsStatic);
                            if (constructor == null)
                            {
                                constructor = new MethodDefinition(".cctor",
                                    MethodAttributes.Private | MethodAttributes.HideBySig |
                                    MethodAttributes.SpecialName |
                                    MethodAttributes.RTSpecialName | MethodAttributes.Static,
                                    unityCoreDef.MainModule.ImportReference(typeof(void)));
                                applicationDef.Methods.Add(constructor);
                                ILProcessor ilprocessor = constructor.Body.GetILProcessor();
                                ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
                            }

                            ILProcessor ilprocessor2 = constructor.Body.GetILProcessor();
                            Instruction instruction = ilprocessor2.Body.Instructions.First<Instruction>();
                            ilprocessor2.InsertBefore(instruction, ilprocessor2.Create(OpCodes.Call, startMethod));

                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                unityCoreDef.Write(memoryStream);
                                Assembly.Load(memoryStream.ToArray());
                            }
                        }
                    }
                    else
                    {
                        using (AssemblyDefinition unityCoreDef =
                            AssemblyDefinition.ReadAssembly(Path.Combine(ManagedPath, config.assembly)))
                        {

                            TypeDefinition applicationDef =
                                unityCoreDef.MainModule.Types.FirstOrDefault((TypeDefinition x) =>
                                    x.Name == config.type);

                            MethodDefinition startInfo = mainDef.MainModule.Types
                                .First((TypeDefinition x) => x.Name == "UULoaderMain")
                                .Methods.First((MethodDefinition x) => x.Name == "Start");
                            MethodReference startMethod = unityCoreDef.MainModule.ImportReference(startInfo);

                            MethodDefinition constructor =
                                applicationDef.Methods.FirstOrDefault((MethodDefinition m) =>
                                    m.Name==config.method);

                            ILProcessor ilprocessor2 = constructor.Body.GetILProcessor();
                            Instruction instruction = ilprocessor2.Body.Instructions.First<Instruction>();
                            ilprocessor2.InsertBefore(instruction, ilprocessor2.Create(OpCodes.Call, startMethod));

                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                unityCoreDef.Write(memoryStream);
                                Assembly.Load(memoryStream.ToArray());
                            }
                        }
                    }

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        mainDef.Write(memoryStream);
                        Assembly.Load(memoryStream.ToArray());
                    }
                }
            }
            catch (Exception e)
            {
                using (TextWriter textWriter = File.CreateText("ERROR.txt"))
                {
                    textWriter.WriteLine(e);
                    textWriter.Flush();
                }
            }
        }

        internal static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            Console.WriteLine($"Looking for {args.Name} | {new AssemblyName(args.Name).Name}.dll");
            var path = $@"{PathUtils.ManagedPath}\..\..\UULoader\{new AssemblyName(args.Name).Name}.dll";

            if (!File.Exists(path))
            {
                path = $@"{PathUtils.ManagedPath}\{new AssemblyName(args.Name).Name}.dll";
                if (!File.Exists(path))
                    return null;
            }

                try
            {
                Console.WriteLine($"Loading at {path}");
                return Assembly.LoadFile(path);
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal class Config
        {
            public bool useDefault = true;
            public string assembly;
            public string type;
            public string method;
        }
    }
}
