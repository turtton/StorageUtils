using HarmonyLib;

namespace InventorySorter.patches {
    [HarmonyPatch]
    public class InventoryAccessor {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Inventory), "Changed")]
        public static void Changed(object instance) {
        }
    }
}