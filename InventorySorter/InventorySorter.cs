using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace InventorySorter {
    [BepInPlugin("net.github.turtton.plugins.inventorysorter", "Inventory Sorter", "1.0.0")]
    public class InventorySorter : BaseUnityPlugin {
        public static ManualLogSource LOGGER;
        private Harmony _harmony;

        private void Awake() {
            LOGGER = Logger;

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(),
                "net.github.turtton.plugins.patch");
        }

        private void OnDestroy() {
            _harmony.UnpatchAll();
        }
    }
}