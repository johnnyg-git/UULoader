using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UULoader
{
    [Serializable]
    public class Mod
    {
        /// <summary>
        /// Name of the mod
        /// </summary>
        public string name;
        /// <summary>
        /// Assemblies to try find and load
        /// </summary>
        public List<Assembly> assemblies = new List<Assembly>();
        /// <summary>
        /// AssetBundles and their prefabs to try find and load
        /// </summary>
        public List<Bundle> bundles = new List<Bundle>();

        /// <summary>
        /// Path to the mod
        /// </summary>
        public string modPath { get; internal set; }

        [Serializable]
        public class Assembly
        {
            /// <summary>
            /// Name of the assembly to load
            /// </summary>
            public string assembly;

            /// <summary>
            /// The loaded assembly
            /// </summary>
            [NonSerialized] public System.Reflection.Assembly loadedAssembly;
        }

        [Serializable]
        public class Bundle
        {
            /// <summary>
            /// Name of the bundle
            /// </summary>
            public string bundle;

            /// <summary>
            /// Name of assets inside of the bundle
            /// </summary>
            public string[] assets;

            [NonSerialized] public AssetBundle loadedBundle;
            [NonSerialized] public List<UnityEngine.Object> loadedAssets;

            public static explicit operator AssetBundle(Bundle b) => b.loadedBundle;

            /// <summary>
            /// Finds an object inside of the loaded asset bundle, will throw exception if not found
            /// </summary>
            /// <param name="objectName">Name of object to find</param>
            /// <returns>Object found</returns>
            /// <exception cref="NullReferenceException"></exception>>
            public UnityEngine.Object GetObject(string objectName)
            {
                if (loadedAssets == null) throw new NullReferenceException("No assets loaded for bundle yet GetObject ran");
                return loadedAssets.SingleOrDefault(a => a.name == objectName);
            }

            public bool Contains(string objectName)
            {
                return loadedAssets.Any(loadedAsset => loadedAsset.name == objectName);
            }
        }

        /// <summary>
        /// Finds an Assembly with a specific name, will throw exception if not found
        /// </summary>
        /// <param name="name">Name of Assembly to find</param>
        /// <returns>Assembly found</returns>
        /// <exception cref="NullReferenceException"></exception>>
        public Assembly GetAssemblyByName(string name)
        {
            if (assemblies == null) throw new NullReferenceException("No assemblies loaded yet GetAssemblyByName ran");
            return assemblies.SingleOrDefault(s => s.assembly == name);
        }

        /// <summary>
        /// Finds a bundle with a specific name, will throw exception if not found
        /// </summary>
        /// <param name="bundleName">Name of the bundle to find</param>
        /// <returns>Bundle found</returns>
        /// <exception cref="NullReferenceException"></exception>>
        public Bundle GetBundleByName(string bundleName)
        {
            if (bundles == null) throw new NullReferenceException("No bundles loaded yet GetBundleByName ran");
            return bundles.SingleOrDefault(s => s.bundle == bundleName);
        }

        /// <summary>
        /// Finds an AssetBundle with a specific name, will throw exception if not found
        /// </summary>
        /// <param name="bundleName">Name of the AssetBundle to find</param>
        /// <returns>AssetBundle found</returns>
        /// <exception cref="NullReferenceException"></exception>>
        public AssetBundle GetAssetBundleByName(string bundleName)
        {
            if (bundles == null) throw new NullReferenceException("No bundles loaded yet GetAssetBundleByName ran");
            return (AssetBundle)GetBundleByName(bundleName);
        }

        /// <summary>
        /// Gets all asset bundles that have been loaded, will throw exception if none have been loaded
        /// </summary>
        /// <returns>All asset bundles</returns>
        /// <exception cref="NullReferenceException"></exception>>
        public List<AssetBundle> GetAssetBundles()
        {
            if (bundles == null) throw new NullReferenceException("No bundles have been loaded yet GetAssetBundles ran");
            return (from bundle in bundles where bundle.loadedBundle != null select bundle.loadedBundle).ToList();
        }

        /// <summary>
        /// Finds an object with a specific name from a specific bundle, will throw exception if not found
        /// </summary>
        /// <param name="objectName">Name of object to find</param>
        /// <param name="bundleName">Name of the bundle to find this object in</param>
        /// <returns>Object found</returns>
        /// <exception cref="NullReferenceException"></exception>>
        public UnityEngine.Object GetObjectFromBundle(string objectName, string bundleName)
        {
            if (bundles == null) throw new NullReferenceException("No bundles have been loaded yet GetObjectFromBundle ran");
            return GetBundleByName(bundleName).GetObject(objectName);
        }

        /// <summary>
        /// Finds an object with a specific name regardless of what bundle it is in, will throw exception if not found
        /// </summary>
        /// <param name="objectName">Name of object to find</param>
        /// <returns>Object</returns>
        /// <exception cref="NullReferenceException"></exception>>
        public UnityEngine.Object GetObject(string objectName)
        {
            if (bundles == null) throw new NullReferenceException("No bundles have been loaded yet GetObject ran");
            return (from assetBundle in bundles where assetBundle.loadedBundle != null from asset in assetBundle.loadedAssets select asset).FirstOrDefault(asset => asset.name == objectName);
        }
    }
}