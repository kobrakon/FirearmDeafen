using System;
using Comfort.Common;
using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using System.Threading.Tasks;
using Aki.Reflection.Patching;
using UnityEngine;
using System.Linq;

namespace ahhmyears
{
    internal struct PlayerInfo
    {
        internal static GameWorld gameWorld
        { get => Singleton<GameWorld>.Instance; }

        internal static Player.FirearmController FC
        { get => player.HandsController as Player.FirearmController; }

        internal static Player player
        { get => gameWorld.AllPlayers[0]; }

        internal static bool PlayerHasEarPro()
        {
            LootItemClass helm;

            if (player.Profile.Inventory.Equipment.GetSlot(EquipmentSlot.Earpiece).ContainedItem != null)
                return true;

            if ((helm = player.Profile.Inventory.Equipment.GetSlot(EquipmentSlot.Headwear).ContainedItem as LootItemClass) != null)
            {
                SlotBlockerComponent blocker = helm.GetItemComponent<SlotBlockerComponent>();
                if (blocker != null && blocker.ConflictingSlotNames.Contains("Earpiece"))
                    return true;

                return helm.Slots.Any(slot => slot.ContainedItem != null && slot.ContainedItem.GetItemComponent<SlotBlockerComponent>() != null && slot.ContainedItem.GetItemComponent<SlotBlockerComponent>().ConflictingSlotNames.Contains("Earpiece"));
            }

            return false;
        }
    }

    public class OuchiePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(Player.FirearmController).GetMethod("RegisterShot", BindingFlags.Instance | BindingFlags.NonPublic);

        [PatchPostfix]
        static void PostFix(Player.FirearmController __instance, GClass2401 shot)
        {
            if (PlayerInfo.player is HideoutPlayer) return; // hideout player has no health controller
            if (PlayerInfo.FC == __instance && GoodToDeafen(shot)) DoEarOuchie(false); else if (TargetGoodToDeafen(__instance, shot)) DoEarOuchie(true);
        }

        static bool TargetGoodToDeafen(Player.FirearmController target, GClass2401 shot) => Vector3.Distance(target.gameObject.transform.position, PlayerInfo.player.Transform.position) <= 45 && !PlayerInfo.PlayerHasEarPro() && !target.IsSilenced && shot.InitialSpeed > 343f;

        static bool GoodToDeafen(GClass2401 shot) => !PlayerInfo.PlayerHasEarPro() && !PlayerInfo.FC.IsSilenced && (shot.InitialSpeed > 343f || PlayerInfo.player.Environment == EnvironmentType.Indoor); // <343m/s subsonic

        static void DoEarOuchie(bool invokedByBot)
        {
            if (!invokedByBot && PlayerInfo.FC.Item.AmmoCaliber == "86x70") // THIRTY HURTY ATE
            {
                try
                {
                    PlayerInfo.player.ActiveHealthController.DoStun(1, 0);
                    PlayerInfo.player.ActiveHealthController.DoContusion(4, 50);
                } catch (Exception e)
                {
                    Plugin.logger.LogError("Attempting to access ActiveHealthController resulted in an exception, falling back to PlayerHealthControlller" + e);
                    PlayerInfo.player.PlayerHealthController.DoStun(1, 0);
                    PlayerInfo.player.PlayerHealthController.DoContusion(4, 100);
                }
            }
            try
            {
                PlayerInfo.player.ActiveHealthController.DoStun(1, 0);
                PlayerInfo.player.ActiveHealthController.DoContusion(0, 100);
            } catch (Exception e)
            {
                Plugin.logger.LogError("Attempting to access ActiveHealthController resulted in an exception, falling back to PlayerHealthControlller" + e);
                PlayerInfo.player.PlayerHealthController.DoStun(1, 0);
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
            if (!PlayerInfo.PlayerHasEarPro() && dist <= 30)
            {
                PlayerInfo.player.ActiveHealthController.DoStun(1, 0);
                PlayerInfo.player.ActiveHealthController.DoContusion(30 / (dist / 2), 100 / dist);
            }
        }
    }
}