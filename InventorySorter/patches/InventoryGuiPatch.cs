﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySorter.patches {
    [HarmonyPatch(typeof(InventoryGui), "Show")]
    public static class InventoryGuiPatch {
        private static readonly MethodInfo InventoryChangedMethod = typeof(Inventory).GetMethod("Changed", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic);

        private static RectTransform _sortButton;
        private static RectTransform _stackButton;

        private static void Postfix(InventoryGrid ___m_playerGrid, InventoryGrid ___m_containerGrid) {
            var instance = InventoryGui.instance;
            // ResizeTakeAllButton(instance.m_takeAllButton.transform.parent);
            if (!instance.IsContainerOpen()) return;

            _sortButton = PrepareButton(instance, "sort", "←");
            RelocateButtons(_sortButton, 0.3f);
            _sortButton.GetComponent<Button>().onClick.AddListener(() => {
                if (Player.m_localPlayer.IsTeleporting() || !(bool) ___m_containerGrid) return;

                var inventory = ___m_containerGrid.GetInventory();
                var items = new List<ItemDrop.ItemData>(inventory.GetAllItems());
                // items.Sort((firstData, secondData) => string.Compare(firstData.m_shared.m_name, secondData.m_shared.m_name, StringComparison.Ordinal));
                var result = new List<ItemDrop.ItemData>();
                var sorted = new List<string>();
                foreach (var itemData in items) {
                    var shared = itemData.m_shared;
                    if (sorted.Contains(shared.m_name)) continue;

                    sorted.Add(shared.m_name);

                    var targetItems = items.FindAll(data => data.m_shared.m_name == itemData.m_shared.m_name);
                    InventorySorter.LOGGER.LogInfo("find" + targetItems.ConvertAll(input => input.m_shared.m_name).Join());
                    if (shared.m_maxStackSize > 1) {
                        var amount = targetItems.ConvertAll(data => data.m_stack).Sum();
                        var stacks = amount / shared.m_maxStackSize;
                        amount -= stacks * shared.m_maxStackSize;

                        InventorySorter.LOGGER.LogInfo("amount:" + amount + ",stacks:" + stacks);
                        if (stacks > 0) {
                            for (var i = 0; i < shared.m_maxStackSize; i++) {
                                var maxStack = itemData.Clone();
                                maxStack.m_stack = shared.m_maxStackSize;
                                result.Add(maxStack);
                            }
                        }

                        var surplus = itemData.Clone();
                        surplus.m_stack = amount;
                        result.Add(surplus);
                    }
                    else {
                        result.AddRange(targetItems);
                    }
                }

                inventory.RemoveAll();
                foreach (var item in result) {
                    inventory.AddItem(item);
                }

                InventoryChangedMethod?.Invoke(inventory, null);

                // inventory.GetAllItems().ForEach(data => InventorySorter.LOGGER.LogInfo(data.m_shared.m_name + ":" + data.m_gridPos));
            });

            _stackButton = PrepareButton(instance, "stack", "↓");
            RelocateButtons(_stackButton, 1.5f);
            _stackButton.GetComponent<Button>().onClick.AddListener(() => {
                if (Player.m_localPlayer.IsTeleporting() || !(bool) ___m_containerGrid) return;

                TransportItems(___m_playerGrid.GetInventory(), ___m_containerGrid.GetInventory(), !Input.GetKey(KeyCode.LeftShift));
            });
        }

        private static RectTransform PrepareButton(InventoryGui instance, string name, string text) {
            var targetTransform = (RectTransform) instance.transform.parent.Find(name);
            if (targetTransform != null) return targetTransform;

            var buttonTransform = instance.m_takeAllButton.transform;
            var additionalTransform = UnityEngine.Object.Instantiate(buttonTransform, buttonTransform.transform.parent);
            additionalTransform.name = name;
            var resultTransform = additionalTransform.transform;
            // resultTransform.SetAsFirstSibling();

            targetTransform = (RectTransform) resultTransform.transform;
            targetTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 45f);
            targetTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 30f);

            var textTransform = (RectTransform) targetTransform.transform.Find("Text");
            textTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 45f);
            textTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 30f);

            var component = textTransform.GetComponent<Text>();
            component.text = text;
            component.resizeTextForBestFit = true;

            // var uiTransform = (RectTransform) targetTransform.transform.Find("UIToolTip");
            // uiTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 45f);
            // uiTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 30f);
            //
            // uiTransform.GetComponent<UITooltip>().m_text = "sort";

            return targetTransform;
        }

        private static void RelocateButtons(RectTransform transform, float vertical) {
            if (!(bool) transform) return;

            transform.pivot = new Vector2(-10f, vertical);
        }

        // private static void ResizeTakeAllButton(Transform transform) {
        //     transform.localScale = new Vector3(0.8f, 1f, 1f);
        //     var rectTransform = (RectTransform) transform;
        //     if (!(bool) rectTransform) return;
        //
        //     rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
        //     rectTransform.pivot = new Vector2(0.0f, 0.5f);
        // }

        private static void TransportItems(Inventory from, Inventory target, bool stackOnly) {
            var fromInvItems = new List<ItemDrop.ItemData>(from.GetAllItems());
            var targetItems = target.GetAllItems();

            foreach (var playerItem in fromInvItems) {
                var searchedItems = targetItems.FindAll(data => data.m_shared.m_name == playerItem.m_shared.m_name);

                if (stackOnly && searchedItems.Count <= 0) continue;

                var containAmount = searchedItems.ConvertAll(data => data.m_stack).Sum();
                var spaceSize = playerItem.m_shared.m_maxStackSize - containAmount % playerItem.m_shared.m_maxStackSize;
                var amount = spaceSize >= playerItem.m_stack ? playerItem.m_stack : spaceSize;

                if (spaceSize <= 0) continue;

                from.RemoveItem(playerItem, amount);
                var copy = playerItem.Clone();
                copy.m_stack = amount;
                target.AddItem(copy);
            }

            InventoryChangedMethod.Invoke(target, null);
            InventoryChangedMethod.Invoke(from, null);
        }
    }
}