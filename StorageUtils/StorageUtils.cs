using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace StorageUtils {
    [BepInPlugin("net.github.turtton.plugins.StorageUtils", "StorageUtils", "1.0.0")]
    [BepInDependency("randyknapp.mods.equipmentandquickslots", BepInDependency.DependencyFlags.SoftDependency)]
    public class StorageUtils : BaseUnityPlugin {
        public static ManualLogSource LOGGER;
        public static bool isEAQSLoaded;

        private Harmony _harmony;

        private void Awake() {
            LOGGER = Logger;

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(),
                "net.github.turtton.plugins.patch");

            try {
                isEAQSLoaded = CheckDependFiles.IsEAQSLoaded();
            }
            catch (FileNotFoundException) {
            }
        }

        private void OnDestroy() {
            _harmony.UnpatchAll();
        }

        private readonly FieldInfo _containerGridField = typeof(InventoryGui)
            .GetField("m_containerGrid", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);

        private readonly FieldInfo _playerGridField = typeof(InventoryGui)
            .GetField("m_playerGrid", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);

        private readonly FieldInfo _animatorField = typeof(InventoryGui)
            .GetField("m_animator", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);

        private static readonly int Visible = Animator.StringToHash("visible");

        private void Update() {
            var instance = InventoryGui.instance;
            if (instance is null) return;

            var animator = (Animator) _animatorField?.GetValue(instance);

            // if (!(animator is null) && ((!ZInput.IsMouseActive() || !Input.GetMouseButtonDown(2)) && (!ZInput.IsGamepadActive() || !ZInput.GetButtonDown("joyrstick")) || !animator.GetBool(Visible))) return;

            //Is Opening Inventory
            if (!(animator is null) && !animator.GetBool(Visible)) {
                return;
            }

            //Is pushed short cut button
            if ((!ZInput.IsMouseActive() || !Input.GetMouseButtonDown(2)) && (!ZInput.IsGamepadActive() || !ZInput.GetButtonDown("JoyRStick"))) {
                return;
            }


            InventoryGrid inventoryGrid;
            if (instance?.IsContainerOpen() == true) {
                inventoryGrid = (InventoryGrid) _containerGridField?.GetValue(instance);
                if (inventoryGrid is null) {
                    return;
                }

                InventoryUtils.SortInventory(inventoryGrid.GetInventory(), false);
            } else {
                inventoryGrid = (InventoryGrid) _playerGridField?.GetValue(instance);
                if (inventoryGrid is null) {
                    return;
                }

                if (isEAQSLoaded) {
                    EAQSWrapper.SortPlayerInventory(inventoryGrid.GetInventory());
                } else {
                    InventoryUtils.SortInventory(inventoryGrid.GetInventory(), true);
                }
            }
        }
    }
}