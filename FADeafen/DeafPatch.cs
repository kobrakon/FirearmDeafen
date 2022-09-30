using System;
using Comfort.Common;
using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using System.Threading.Tasks;
using Aki.Reflection.Patching;
using UnityEngine;

namespace ahhmyears
{
    internal struct PlayerInfo
    {
        internal static GameWorld gameWorld
        {
            get
            {
                return Singleton<GameWorld>.Instance;
            }
        }

        internal static Player.FirearmController FC
        {
            get
            {
                return player.HandsController as Player.FirearmController;
            }
        }

        internal static Player player
        {
            get
            {
                return gameWorld.AllPlayers[0];
            }
        }

        internal static bool PlayerHasEarPro() => player.Profile.Inventory.Equipment.GetSlot(EquipmentSlot.Earpiece).ContainedItem != null;
    }

    public class OuchiePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player.FirearmController).GetMethod("RegisterShot", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [PatchPostfix]
        static void PostFix(Player.FirearmController __instance)
        {
            if (PlayerInfo.FC == __instance && GoodToDeafen()) { DoEarOuchie(false); } else if (TargetGoodToDeafen(__instance)) DoEarOuchie(true);
        }

        static bool TargetGoodToDeafen(Player.FirearmController target) => Vector3.Distance(target.gameObject.transform.position, PlayerInfo.player.Transform.position) <= 45 && !PlayerInfo.PlayerHasEarPro() && !target.IsSilenced;

        static bool GoodToDeafen() => !PlayerInfo.PlayerHasEarPro() && !PlayerInfo.FC.IsSilenced;

        static void DoEarOuchie(bool invokedByBot)
        {
            if (!invokedByBot && PlayerInfo.FC.Item.AmmoCaliber == "86x70")
            {
                try
                {
                    PlayerInfo.player.ActiveHealthController.DoContusion(4, 50);
                } catch (Exception e)
                {
                    Plugin.logger.LogError("Attempting to access ActiveHealthController resulted in an exception, falling back to PlayerHealthControlller\n" + e);
                    PlayerInfo.player.PlayerHealthController.DoContusion(4, 100);
                }
            }
            try
            {
                PlayerInfo.player.ActiveHealthController.DoContusion(0, 100);
            } catch (Exception e)
            {
                Plugin.logger.LogError("Attempting to access ActiveHealthController resulted in an exception, falling back to PlayerHealthControlller\n" + e);
                PlayerInfo.player.PlayerHealthController.DoContusion(0, 100);
            }
        }
    }

    public class OuchieGrenadePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Grenade).GetMethod("OnExplosion", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        static void PreFix(Grenade __instance)
        {
            float dist = Vector3.Distance(__instance.transform.position, PlayerInfo.player.Transform.position);
            if (!PlayerInfo.PlayerHasEarPro() && dist <= 50) PlayerInfo.player.ActiveHealthController.DoContusion(100 / (dist / 2), dist * 0.50f);
        }
    }
}