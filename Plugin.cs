using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RubiksPrisons;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
public class Plugin : BaseUnityPlugin {
    // Logger
    internal static ManualLogSource? Log;

    public static string workingPath = Assembly.GetExecutingAssembly().Location;
    public static string workingDir = Path.GetDirectoryName(workingPath);
    public const string PluginGUID = "com.github.end-4.rubiksPrisons";
    public const string PluginName = "RubiksPrisons";
    public const string PluginVersion = "1.0.0";

    public static Texture2D LoadTexture(string FilePath) {
        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        if (File.Exists(FilePath)) {
            byte[] fileData;
            Texture2D tex;
            fileData = File.ReadAllBytes(FilePath);
            tex = new Texture2D(2, 2); // Create new "empty" texture
            if (tex.LoadImage(fileData)) // Load the imagedata into the texture (size is set automatically)
                return tex; // If data = readable -> return texture
        }

        return null; // Return null if load failed
    }

    internal static GameObject FindNestedObject(GameObject baseObject, string path) {
        Transform t = baseObject.transform;
        string[] pathItems = path.Split("/");
        for (int i = 0; i < pathItems.Length; i++) {
            string itemStr = pathItems[i];
            t = t.transform.Find(itemStr);
            if (t == null) {
                Log.LogWarning(itemStr + " not found for object path " + baseObject.name + "/" + path);
                return null;
            }
        }

        return t.gameObject;
    }

    private void Awake() {
        Log = Logger;

        // Add scene load replacement
        SceneManager.sceneLoaded += (_, _) => {
            Material[] allMats = Resources.FindObjectsOfTypeAll<Material>();
            Texture2D minosPrisonTex = LoadTexture(Path.Combine(workingDir, "assets/T_FleshPrison.png"));
            Texture2D sisyPrisonTex = LoadTexture(Path.Combine(workingDir, "assets/T_FleshPrison2.png"));
            foreach (var mat in allMats) {
                if (mat.name == "FleshPrison" || mat.name == "FleshPrisonUnlit") {
                    mat.mainTexture = minosPrisonTex;
                } else if (mat.name == "FleshPanopticon" || mat.name == "FleshPanopticon Unlit") {
                    mat.mainTexture = sisyPrisonTex;
                }
            }
        };

        Harmony Harmony = new Harmony(PluginName);
        Harmony.PatchAll();

        // Done
        Log.LogInfo($"{PluginName} loaded!");
    }
}

[HarmonyPatch(typeof(FleshPrison), "ChangeTexture")]
public class NoTextureChangePatch {
    static bool Prefix(FleshPrison __instance, Texture tex) {
        // Disallow texture change cuz idk how to replace the "attack" texture or whatever it is
        return false;
    }
}
