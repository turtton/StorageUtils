using HarmonyLib;
using UnityEngine;

namespace InventorySorter.patches {
    [HarmonyPatch]
    public class InventoryAccessor {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Inventory), "Changed")]
        public static void Changed(object instance) {}
    }
}