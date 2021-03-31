using System.Collections.Generic;
using System.Reflection;
using EquipmentAndQuickSlots;

namespace StorageUtils {
    public static class EAQSWrapper {
        private static readonly FieldInfo _inventoriesField = typeof(ExtendedInventory)
            .GetField("_inventories", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);

        public static void SortPlayerInventory(Inventory inventory) {
            var inventories = (List<Inventory>) _inventoriesField?.GetValue(inventory);
            var inv = inventories[0];
            InventoryUtils.SortInventory(inv, true);
        }
    }
}