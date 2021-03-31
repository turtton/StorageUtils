using System;

namespace StorageUtils {
    public static class CheckDependFiles {
        //Checking EquipmentAndQuickSlots is Loaded
        public static bool IsEAQSLoaded() {
            var result = false;
            try {
                var t = EquipmentAndQuickSlots.EquipmentAndQuickSlots.EquipSlotTypes;
                result = true;
            }
            catch (Exception) {
                // ignored
            }

            return result;
        }
    }
}