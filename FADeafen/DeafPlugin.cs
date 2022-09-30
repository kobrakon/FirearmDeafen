using BepInEx;
using UnityEngine;
using BepInEx.Logging;

namespace ahhmyears
{
    [BepInPlugin("com.kobrakon.gunshotdeaf", "GunShotDeafen", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource logger;

        void Awake()
        {
            logger = Logger;
            new OuchiePatch().Enable();
            new OuchieGrenadePatch().Enable();
        }
    }
}