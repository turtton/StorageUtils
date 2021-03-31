using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace StorageUtils {
    [BepInPlugin("net.github.turtton.plugins.StorageUtils", "StorageUtils", "1.0.0")]
    public class StorageUtils : BaseUnityPlugin {
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
            } else {
                inventoryGrid = (InventoryGrid) _playerGridField?.GetValue(instance);
            }

            if (!(inventoryGrid is null)) InventoryUtils.SortInventory(inventoryGrid.GetInventory());
        }
    }
}