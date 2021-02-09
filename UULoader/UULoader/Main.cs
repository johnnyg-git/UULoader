using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UULoader
{
    public static class UULoaderMain
    {
        public static List<Mod> mods { get; internal set; } = new List<Mod>();

        public static void Start()
        {
            ConsoleManager.CreateConsole(true);
            PathUtils.FindPaths();
            LoadMods();
        }

        public static Mod GetModByName(string name)
        {
            if (mods == null) throw new NullReferenceException("No mods loaded yet GetModByName ran");
            return mods.Single(s => s.name == name);
        }

        public static Mod.Bundle GetBundle(string bundleName)
        {
            if (mods == null) throw new NullReferenceException("No mods loaded yet GetBundle ran");
            return (from VARIABLE in mods where VARIABLE.bundles != null from var2 in VARIABLE.bundles where var2.bundle == bundleName select var2).FirstOrDefault();
        }

        internal static void LoadMods()
        {
            Console.WriteLine("Loading mods...");
            try
            {
                foreach (var file in Directory.GetFiles(PathUtils.ModsPath, "mod.json", SearchOption.AllDirectories))
                {
                    try
                    {
                        string lines = File.ReadAllText(file);
                        var info = JsonConvert.DeserializeObject<Mod>(lines);
                        info.modPath = file.Replace("mod.json", "");
                        mods.Add(info);
                        Debug.Log($"Loaded Mod:\nName: {info.name}\nPath: {info.modPath}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed to load mod json {file}\nException: {e}");
                    }
                }

                Console.WriteLine("Loading assemblies");
                LoadAssemblies();
                Console.WriteLine("Loaded assemblies\nLoading bundles");
                LoadBundles();
                Console.WriteLine("Bundles loaded\nMods loaded");

                foreach (Type type in mods.SelectMany(mod =>
                    from assembly in mod.assemblies
                    from type in assembly.loadedAssembly.GetTypes()
                    where type.IsSubclassOf(typeof(ModModule))
                    select type))
                {
                    GameObject managerObject = new GameObject();
                    GameObject.DontDestroyOnLoad(managerObject);
                    managerObject.AddComponent(type);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading mods " + e);
            }
        }

        internal static void LoadAssemblies()
        {
            foreach (var mod in mods)
            {
                foreach (var assembly in mod.assemblies)
                {
                    foreach (var file in Directory.GetFiles(mod.modPath, assembly.assembly))
                    {
                        assembly.loadedAssembly = Assembly.LoadFile(file);
                        Debug.Log($"{assembly.loadedAssembly.FullName} has been loaded for {mod.name}");
                        goto Continue;
                    }

                    Debug.Log($"Could not find assembly {assembly} for {mod.name}");
                    Continue: continue;
                }
            }
        }

        internal static void LoadBundles()
        {
            foreach (var mod in mods)
            {
                foreach (var bundle in mod.bundles)
                {
                    bundle.loadedAssets = new List<Object>();
                    foreach (var file in Directory.GetFiles(mod.modPath, bundle.bundle))
                    {
                        bundle.loadedBundle = AssetBundle.LoadFromFile(file);
                        Debug.Log($"{bundle.bundle} has been loaded for {mod.name}");
                        foreach (var asset in bundle.assets)
                        {
                            if (bundle.loadedBundle.Contains(asset))
                            {
                                Object obj = bundle.loadedBundle.LoadAsset(asset);
                                bundle.loadedAssets.Add(obj);
                                Console.WriteLine($"{obj.name} has been loaded in {bundle.bundle}");
                            }
                            else
                            {
                                Console.WriteLine($"{bundle.bundle} does not contain {asset}");
                            }
                        }
                        goto Continue;
                    }
                    Debug.Log($"Could not find bundle {bundle.bundle} for {mod.name}");
                    Continue: continue;
                }
            }
        }
    }
}
