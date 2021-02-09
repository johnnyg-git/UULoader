# UULoader - Universal Unity Loader
UULoader is a Unity Mod Loader that I made for my own personal usage.
It allows users to quickly load .Net assemblies and AssetBundles into just about any unity game and use UULoader as a base for their own addon mod loaders.

## Usage
UULoader can be installed into a game by extracting the zip, from the Releases section, into your games base directory. It uses Unity Doorstop which utilises a DLL Proxy to automatically load the mod loader whenever the game is launched
*More advanced info on using this loader to make your own mods can be found at the [wiki](https://github.com/johnnyg-git/UULoader/wiki)*

## Uninstalling
UULoader can be installed by just deleting all of the files that were extracted into the game directory. No real changes are made to the game at any time

## Used libraries
* [NeighTools/UnityDoorstop](https://github.com/NeighTools/UnityDoorstop)
* [pardeike/Harmony](https://github.com/pardeike/Harmony)
* [jbevain/cecil](https://github.com/jbevain/cecil)
* [MonoMod/MonoMod](https://github.com/MonoMod/MonoMod)
* [BIGDummyHead/DummyReflection](https://github.com/BIGDummyHead/DummyReflection)
