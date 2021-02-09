using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace UULoader
{
    public static class Bootstrap
    {
        private static string ManagedPath;
        private static string GamePath;

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
            try
            {
                ManagedPath = Environment.GetEnvironmentVariable("DOORSTOP_MANAGED_FOLDER_DIR");
                GamePath = Path.GetFullPath(ManagedPath + @"\..\..\");
                using (AssemblyDefinition mainDef = AssemblyDefinition.ReadAssembly(GamePath + "UULoader\\UULoader.dll"))
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

        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var path = Path.Combine(GamePath, $@"UULoader{new AssemblyName(args.Name).Name}.dll");

            if (!File.Exists(path))
            {
                return null;
            }

                try
            {
                return Assembly.LoadFile(path);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
