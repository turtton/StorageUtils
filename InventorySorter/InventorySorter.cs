using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace InventorySorter {
    [BepInPlugin("net.github.turtton.plugins.inventorysorter", "Inventory Sorter", "1.0.0")]
    public class InventorySorter : BaseUnityPlugin {
        public static ManualLogSource LOGGER;

        private void Awake() {
            LOGGER = Logger;
        }

        private void Update() {
            var instance = InventoryGui.instance;
            if (instance?.IsContainerOpen() != true && Input.GetMouseButtonDown(2)) return;

            var m_containerGrid = (InventoryGrid) typeof(InventoryGui).GetField(
                    "m_containerGrid", BindingFlags.Instance |
                                       BindingFlags.GetField | BindingFlags.NonPublic)
                ?.GetValue(instance);
            var inventory = m_containerGrid.GetInventory();

            if (inventory == null) return;

            var itemDatas =
                new List<ItemDrop.ItemData>(inventory.GetAllItems());
            itemDatas.Sort((data, itemData) =>
                string.Compare(data.m_shared.m_name, itemData.m_shared.m_name,
                    StringComparison.Ordinal));
            inventory.RemoveAll();
            foreach (var itemData in itemDatas) {
                inventory.AddItem(itemData);
            }

            try {
                typeof(Inventory)
                    .GetMethod("Changed",
                        BindingFlags.Instance | BindingFlags.InvokeMethod |
                        BindingFlags.NonPublic)
                    ?.Invoke(inventory, null);
            }
            catch (NullReferenceException e) {
                System.Console.WriteLine(e);
                throw;
            }

            LOGGER.LogDebug("sorted inventory!!");
        }
    }
}