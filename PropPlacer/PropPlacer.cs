using HarmonyLib;
using NewHorizons.Utility.DebugTools;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;

namespace PropPlacer
{
    public class PropPlacer : ModBehaviour
    {
        public static PropPlacer Instance;

        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            new Harmony("xen.PropPlacer").PatchAll(Assembly.GetExecutingAssembly());

            OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen);
            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
        }

        public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
        {
            if (newScene != OWScene.SolarSystem && newScene != OWScene.EyeOfTheUniverse) return;

            ModHelper.Events.Unity.FireOnNextUpdate(() =>
            {
                Locator.GetPlayerBody().gameObject.AddComponent<PropPlacerComponent>();
                Locator.GetPlayerBody().gameObject.AddComponent<PropPlacerMenu>();
            });
        }

        public override void SetupPauseMenu(IPauseMenuManager pauseMenu)
        {
            base.SetupPauseMenu(pauseMenu);
            PropPlacerMenu.InitializePauseMenu(pauseMenu);
        }
    }
}
