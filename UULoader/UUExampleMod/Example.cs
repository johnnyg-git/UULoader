using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UULoader;

namespace UUExampleMod
{
    public class Example : ModModule
    {
        public Mod modInstance;

        void Awake()
        {
            modInstance = UULoaderMain.GetModByName("Example Mod");
            print($"Awake ran - {modInstance.name}");
            foreach (Mod.Bundle bundle in modInstance.bundles)
            {
                foreach (UnityEngine.Object obj in bundle.loadedAssets)
                {
                    print($"{obj.name} in bundle {bundle.bundle}");
                }
            }
        }

        void Update()
        {
            print("Update");
        }
    }
}
