using UnityEngine;
using IPA;
using UnityEngine.SceneManagement;
using CustomFloorPlugin.Util;
using CustomUI.Utilities;
using Harmony;

namespace CustomFloorPlugin
{
    public class Plugin : IBeatSaberPlugin
    {
        public static BS_Utils.Utilities.Config config;
        public static IPA.Logging.Logger logger;
        private bool init = false;

        public void Init(object thisWillBeNull, IPA.Logging.Logger logger)
        {
            Plugin.logger = logger;
        }

        public void OnApplicationStart()
        {
            //Instance = this;
            BSEvents.OnLoad();
            BSEvents.menuSceneLoadedFresh += OnMenuSceneLoadedFresh;

            HarmonyInstance hi = HarmonyInstance.Create("com.rolopogo.customplatforms");
            hi.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(LightWithId))]
        [HarmonyPatch("Start")]
        public class LightsWithId_Patch
        {
            static bool HasBeenRun = false;
            public static LightWithIdManager GameLightManager = null;
            static public void Postfix(LightWithIdManager ____lighManager)
            {
                if (HasBeenRun) return;
                //HasBeenRun = true;
                GameLightManager = ____lighManager;
            }
        }

        private void OnMenuSceneLoadedFresh()
        {
            if(!init){ 
                init = true;
                config = new BS_Utils.Utilities.Config("Custom Platforms");
                PlatformManager.OnLoad();
            }
        }

        public void OnApplicationQuit()
        {
            BSEvents.menuSceneLoadedFresh -= OnMenuSceneLoadedFresh;
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode) {
            Scene[] scenes = SceneManager.GetAllScenes();
            LightWithId[] lights = GameObject.FindObjectsOfType<LightWithId>();
            LightWithId[] lights2 = Resources.FindObjectsOfTypeAll<LightWithId>();

            if (LightsWithId_Patch.GameLightManager != null)
                Traverse.Create(LightsWithId_Patch.GameLightManager).SetPrivateField("_lights", new System.Collections.Generic.List<LightWithId>[21]);

            foreach (LightWithId light in lights)
            {
                LightsWithId_Patch.GameLightManager.RegisterLight(light);
            }

            foreach (LightWithId light in lights2)
            {
                LightsWithId_Patch.GameLightManager.RegisterLight(light);
            }
        }

        public void OnSceneUnloaded(Scene scene) { }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene) { }

        public void OnUpdate() { }

        public void OnFixedUpdate() { }
    }
}