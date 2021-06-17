using HarmonyLib;
using BepInEx;
using UnityEngine;
using TMPro;
using System;
using PacketHelper;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Steamworks;
using Steamworks.Data;

namespace ShareEm.Patches
{
    [HarmonyPatch]
    class ShareEm
    {


        /*
         * Create new packets.
         */
        public static Session session = new Session(Main.Id);
        private static BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
        [HarmonyPatch(typeof(Server), "InitializeServerPackets")]
        [HarmonyPostfix]
        static void CreateNewServerPackets()
        {
            session.CreateNewServerPacket("ServerHandleShare", ServerHandleShare);
        }

        [HarmonyPatch(typeof(LocalClient), "InitializeClientData")]
        [HarmonyPostfix]
        static void CreateNewClientPackets()
        {
            session.CreateNewClientPacket("ClientHandleShare", ClientHandleShare);
        }




        /*
         * Send share packet when a player picks up an item
         */
        [HarmonyPatch(typeof(ClientSend), "PickupItem")]
        [HarmonyPrefix]
        static void ClientSendSharePacket(ClientSend __instance, int itemID)
        {
            Item item = ItemManager.Instance.list[itemID].GetComponent<Item>();
            if (item.powerup)
            {
                Data data = new Data();
                data.ints = new int[] { item.powerup.id };
                MonoBehaviour.print($"Sending ClientSharePacket from {LocalClient.instance.myId}");
                session.SendPacketToServer("ServerHandleShare", data);
            }
        }


        // Share to the client
        static void ClientHandleShare(Packet packet)
        {
            Debug.Log("Recieved share packet");
            int fromClient = packet.ReadInt();
            int itemID = packet.ReadInt();
            string fromUsername = packet.ReadString();
            Debug.Log($"{fromClient}, {itemID}");

            if (fromClient == LocalClient.instance.myId)
            {
                ChatBox.Instance.AppendMessage(-1, $"<color=red>You <color=white>just shared <color={getPowerupByID(itemID).GetColorName()}>{getPowerupByID(itemID).name} <color=white>with everyone!", "");
            } else
            {
                ChatBox.Instance.AppendMessage(-1, $"<color=red>{fromUsername} <color=white>just shared <color={getPowerupByID(itemID).GetColorName()}>{getPowerupByID(itemID).name} <color=white>with you!", "");
                addPowerup(getPowerupByID(itemID));
            }
        }

        static void ServerHandleShare(int fromClient, Packet packet)
        {
            int itemID = packet.ReadInt();
            MonoBehaviour.print($"Received share packet from {fromClient}");
            Data data = new Data();
            data.ints = new int[] { fromClient, itemID };
            data.strings = new string[] { Server.clients[fromClient].player.username };
            MonoBehaviour.print($"Echoing share to all players.");
            session.SendPacketToAllClients("ClientHandleShare", data);
        }


        // Add a powerup to the current client.
        static void addPowerup(Powerup powerup)
        {
            UiEvents.Instance.AddPowerup(powerup);
            PowerupUI.Instance.AddPowerup(powerup.id);
            int[] currentPowerups = typeof(PowerupInventory).GetField("powerups", flags).GetValue(PowerupInventory.Instance) as int[];
            currentPowerups[powerup.id]++;
            PlayerStatus.Instance.UpdateStats();
        }

        static Powerup getPowerupByID(int id)
        {
            return ItemManager.Instance.allPowerups[id];
        }



    }
}
