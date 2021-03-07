using System;
using HarmonyLib;

namespace InventorySorter.patches {
    [HarmonyPatch]
    public class InventoryAccessor {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Inventory), "Changed")]
        public static void Changed(object instance) {
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Inventory), "TopFirst")]
        public static bool TopFirst(object instance, ItemDrop.ItemData item) {
            throw new Exception();
        }
    }
}