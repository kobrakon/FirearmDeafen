using BepInEx;
using UnityEngine;

namespace ahhmyears
{
    [BepInPlugin("com.kobrakon.gunshotdeaf", "GunShotDeafen", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private static GameObject Hook;

        void Awake()
        {
            Hook = new GameObject("GunShotDeafener");
            Hook.AddComponent<DeafController>();
            Logger.LogInfo("remember to wear your ear pro kids");
            DontDestroyOnLoad(Hook);
        }
    }
}