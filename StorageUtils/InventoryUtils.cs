using System;
using System.Collections.Generic;
using System.Linq;
using InventorySorter.patches;

namespace StorageUtils {
    public static class InventoryUtils {
        // private static readonly MethodInfo InventoryChangedMethod = typeof(Inventory).GetMethod("Changed", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic);

        public static void SortInventory(Inventory inventory) {
            var player = Player.m_localPlayer;
            var isPlayerInventory = player.GetInventory() == inventory;
            var items = new List<ItemDrop.ItemData>(inventory.GetAllItems());
            // items.Sort((firstData, secondData) => string.Compare(firstData.m_shared.m_name, secondData.m_shared.m_name, StringComparison.Ordinal));
            var compared = new List<ItemDrop.ItemData>();
            var sorted = new List<string>();
            foreach (var itemData in items) {
                var shared = itemData.m_shared;
                if (sorted.Contains(shared.m_name)) continue;

                sorted.Add(shared.m_name);

                var targetItems = items
                    .FindAll(data => data.m_shared.m_name == itemData.m_shared.m_name)
                    .FindAll(data => !data.m_shared.m_questItem && !player.IsItemEquiped(data) && !(isPlayerInventory && data.m_gridPos.y == 0));

                foreach (var item in targetItems) {
                    inventory.RemoveItem(item);
                }

                if (shared.m_maxStackSize > 1) {
                    var amount = targetItems.ConvertAll(data => data.m_stack).Sum();
                    var stacks = amount / shared.m_maxStackSize;
                    amount -= stacks * shared.m_maxStackSize;

                    if (stacks > 0) {
                        for (var i = 0; i < stacks; i++) {
                            var maxStack = itemData.Clone();
                            maxStack.m_stack = shared.m_maxStackSize;
                            compared.Add(maxStack);
                        }
                    }

                    if (amount > 0) {
                        var surplus = itemData.Clone();
                        surplus.m_stack = amount;
                        compared.Add(surplus);
                    }
                } else {
                    compared.AddRange(targetItems);
                }
            }

            compared.Sort((firstData, secondData) => string.Compare(firstData.m_shared.m_name, secondData.m_shared.m_name, StringComparison.Ordinal));

            var result = new List<ItemDrop.ItemData>();
            foreach (var unStackableItem in compared.FindAll(data => InventoryAccessor.TopFirst(inventory, data))) {
                compared.Remove(unStackableItem);
                result.Add(unStackableItem);
            }

            result.AddRange(compared);

            var x = 0;
            var y = isPlayerInventory ? 1 : 0;
            foreach (var item in result) {
                while (true) {
                    var current = inventory.GetItemAt(x, y);
                    if (current is null) {
                        item.m_gridPos = new Vector2i(x, y);
                        inventory.GetAllItems().Add(item);
                        break;
                    }

                    if (++x <= inventory.GetWidth() - 1) continue;

                    if (++y >= inventory.GetHeight()) {
                        throw new Exception("out of slot height!!");
                    }

                    x = 0;
                }
            }

            InventoryAccessor.Changed(inventory);

            // inventory.GetAllItems().ForEach(data => InventorySorter.LOGGER.LogInfo(data.m_shared.m_name + ":" + data.m_gridPos));
        }

        public static void TransportItems(Inventory from, Inventory target, bool stackOnly) {
            var targetItems = target.GetAllItems();
            var targetItemNames = targetItems.ConvertAll(input => input.m_shared.m_name);
            var fromInvItems = from.GetAllItems()
                .FindAll(data => !stackOnly || !InventoryAccessor.TopFirst(from, data) && targetItemNames.Contains(data.m_shared.m_name));

            var player = Player.m_localPlayer;
            if (player.GetInventory() == from) {
                fromInvItems = fromInvItems.FindAll(data => !player.IsItemEquiped(data) && !player.IsItemQueued(data))
                    .FindAll(data => stackOnly || data.m_gridPos.y != 0);
            }

            foreach (var name in fromInvItems.ConvertAll(input => input.m_shared.m_name).Distinct()) {
                var items = fromInvItems.FindAll(data => data.m_shared.m_name == name);

                var item = items.First().Clone();
                var maxStackSize = item.m_shared.m_maxStackSize;

                if (maxStackSize <= 1) {
                    foreach (var itemData in items) {
                        if (target.AddItem(itemData)) {
                            from.RemoveItem(itemData);
                        } else {
                            break;
                        }
                    }
                } else {
                    var currentAmount = items.ConvertAll(input => input.m_stack).Sum();
                    var surplus = targetItems.FindAll(data => data.m_shared.m_name == name).ConvertAll(input => input.m_stack).Sum() % maxStackSize;

                    var emptyAmount = surplus == 0 ? 0 : maxStackSize - surplus;
                    emptyAmount += target.GetEmptySlots() * maxStackSize;

                    var movableAmount = emptyAmount >= currentAmount ? currentAmount : emptyAmount;

                    if (movableAmount <= 0) continue;

                    var stackAmount = movableAmount / maxStackSize;
                    var amount = movableAmount - stackAmount * maxStackSize;

                    if (stackAmount > 0) {
                        for (var i = 0; i < stackAmount; i++) {
                            var clone = item.Clone();
                            clone.m_stack = maxStackSize;
                            target.AddItem(clone);
                        }
                    }

                    if (amount > 0) {
                        var clone = item.Clone();
                        clone.m_stack = amount;
                        target.AddItem(clone);
                    }

                    from.RemoveItem(name, movableAmount);
                }
            }

            InventoryAccessor.Changed(target);
            InventoryAccessor.Changed(from);
            // InventoryChangedMethod.Invoke(target, null);
            // InventoryChangedMethod.Invoke(from, null);
        }
    }
}