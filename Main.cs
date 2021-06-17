using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;
using NetworkHelper;
using System;
using System.IO;
using System.Reflection;


namespace ShareEm
{
    [BepInPlugin(Id, "ShareEm", Version)]
    public class Main : BaseUnityPlugin
    {
        #region[Declarations]

        public const string
            MODNAME = "$safeprojectname$",
            AUTHOR = "",
            GUID = AUTHOR + "_" + MODNAME,
            VERSION = "1.0.0.0";

        internal readonly ManualLogSource log;
        internal readonly Harmony harmony;
        internal readonly Assembly assembly;
        public readonly string modFolder;

        #endregion


        public const string Id = "mod.iiveil.ShareEm";
        public const string Version = "1.0.0";
        public const string Name = "ShareEm";
        public static Session session = new Session(Id);
        public Main()
        {
            
            log = Logger;
            harmony = new Harmony(Id);
            assembly = Assembly.GetExecutingAssembly();
            modFolder = Path.GetDirectoryName(assembly.Location);
        }

        public void Start()
        {
            harmony.PatchAll(assembly);
            
        }
    }
}
