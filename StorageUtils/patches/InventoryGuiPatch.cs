using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySorter.patches {
    [HarmonyPatch(typeof(InventoryGui), "Show")]
    public static class InventoryGuiShowMethodPatch {
        private static RectTransform _sortButton;
        private static RectTransform _stackButton;

        private static void Postfix(InventoryGui __instance, InventoryGrid ___m_playerGrid, InventoryGrid ___m_containerGrid) {
            // ResizeTakeAllButton(instance.m_takeAllButton.transform.parent);
            if (!__instance.IsContainerOpen()) return;

            _sortButton = PrepareButton(__instance, "sort", "←");
            RelocateButtons(_sortButton, 0.3f);
            _sortButton.GetComponent<Button>().onClick.AddListener(() => {
                if (Player.m_localPlayer.IsTeleporting() || !(bool) ___m_containerGrid) return;

                InventoryUtils.SortInventory(___m_containerGrid.GetInventory());
            });

            _stackButton = PrepareButton(__instance, "stack", "↓");
            RelocateButtons(_stackButton, 1.5f);
            _stackButton.GetComponent<Button>().onClick.AddListener(() => {
                if (Player.m_localPlayer.IsTeleporting() || !(bool) ___m_containerGrid) return;

                InventoryUtils.TransportItems(___m_playerGrid.GetInventory(), ___m_containerGrid.GetInventory(), !Input.GetKey(KeyCode.LeftShift));
            });
        }

        // private static void ResizeTakeAllButton(Transform transform) {
        //     transform.localScale = new Vector3(0.8f, 1f, 1f);
        //     var rectTransform = (RectTransform) transform;
        //     if (!(bool) rectTransform) return;
        //
        //     rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
        //     rectTransform.pivot = new Vector2(0.0f, 0.5f);
        // }

        private static RectTransform PrepareButton(InventoryGui instance, string name, string text) {
            var targetTransform = (RectTransform) instance.transform.parent.Find(name);
            if (targetTransform != null) return targetTransform;

            var buttonTransform = instance.m_takeAllButton.transform;
            var additionalTransform = Object.Instantiate(buttonTransform, buttonTransform.transform.parent);
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
    }
}