using EFT;
using System.Reflection;
using EFT.InventoryLogic;
using UnityEngine;
using Comfort.Common;

namespace ahhmyears
{
    public class DeafController : MonoBehaviour
    {
        private bool playerIsShooting;

        void Update()
        {
            if (Ready())
            {
                FC.OnShot += () => { DoEarOuchie(); };
            }
        }

        bool Ready()
        {                                                                                               // hideout player has no AHC
            if (gameWorld == null || gameWorld.AllPlayers == null || gameWorld.AllPlayers.Count <= 0 || player is HideoutPlayer || FC == null)
            {
                return false;
            }
            return true;
        }

        bool GoodToDeafen() => player.Profile.Inventory.Equipment.GetSlot(EquipmentSlot.Earpiece).ContainedItem != null || FC.IsSilenced ? false : true;

        void DoEarOuchie()
        {
            if (GoodToDeafen())
            {
                if (FC.Item.AmmoCaliber == "86x70")
                {
                    player.ActiveHealthController.DoContusion(4, 100);
                }
                player.ActiveHealthController.DoContusion(0, 100);
            }
        }

        GameWorld gameWorld
        {
            get
            {
                return Singleton<GameWorld>.Instance;
            }
        }

        Player.FirearmController FC
        {
            get
            {
                return player.HandsController as Player.FirearmController;
            }
        }

        Player player
        {
            get
            {
                return gameWorld.AllPlayers[0];
            }
        }
    }
}