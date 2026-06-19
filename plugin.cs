using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using UnityEngine;
// Game classes (Box, DisplaySlot, TrashBin, etc.) live in the global namespace,
// so no extra 'using' is needed for them.

namespace AutoRestock
{
    [BepInPlugin("AutoRestock", "Auto Restock", "1.1.0")]
    public class Plugin : BasePlugin
    {
        internal static new ManualLogSource Log;
        internal static ConfigEntry<KeyCode> RestockKey;
        internal static ConfigEntry<bool> TrashEmptyBox;

        public override void Load()
        {
            Log = base.Log;

            RestockKey = Config.Bind(
                "General",
                "RestockKey",
                KeyCode.Y,
                "Hold an open box and press this key to instantly stock it onto matching labeled shelves.");

            TrashEmptyBox = Config.Bind(
                "General",
                "TrashEmptyBox",
                true,
                "If on: pressing the key while holding an ALREADY-EMPTY box throws it into the nearest trash bin. Turn off if it conflicts with another mod (e.g. New Box Spawner).");

            Log.LogInfo($"Auto Restock loaded. Press {RestockKey.Value} to restock; press again on an empty box to trash it (TrashEmptyBox={TrashEmptyBox.Value}).");
            AddComponent<RestockListener>();
        }
    }

    public class RestockListener : MonoBehaviour
    {
        public RestockListener(IntPtr ptr) : base(ptr) { }

        void Update()
        {
            if (Input.GetKeyDown(Plugin.RestockKey.Value))
                Restocker.Run();
        }
    }

    public static class Restocker
    {
        public static void Run()
        {
            var log = Plugin.Log;
            try
            {
                var boxInteraction = UnityEngine.Object.FindObjectOfType<BoxInteraction>();
                if (boxInteraction == null) { log.LogInfo("No BoxInteraction found (not in-game?)."); return; }

                Box box = boxInteraction.Box;
                if (box == null) { log.LogInfo("No box in hand."); return; }
                if (!box.IsOpen) { log.LogInfo("Box is not open."); return; }

                // Empty box in hand -> this is the "second press": trash it (if enabled).
                if (!box.HasProducts)
                {
                    if (!Plugin.TrashEmptyBox.Value)
                    {
                        log.LogInfo("Box is empty (TrashEmptyBox is off).");
                        return;
                    }
                    TrashBox(boxInteraction, box);
                    return;
                }

                // Box has products -> restock onto labeled shelves.
                int productID = box.Data.ProductID;

                var displayManager = UnityEngine.Object.FindObjectOfType<DisplayManager>();
                if (displayManager == null) { log.LogInfo("No DisplayManager found."); return; }

                var slots = new Il2CppSystem.Collections.Generic.List<DisplaySlot>();
                displayManager.GetLabeledEmptyDisplaySlots(productID, slots);

                if (slots.Count == 0)
                {
                    log.LogInfo($"No labeled empty shelves for product {productID}.");
                    return;
                }

                int placed = 0;
                int safety = 0;

                for (int i = 0; i < slots.Count; i++)
                {
                    DisplaySlot slot = slots[i];
                    if (slot == null) continue;
                    if (!slot.CanRestockWith(productID)) continue;

                    while (box.HasProducts && !slot.Full)
                    {
                        boxInteraction.PlaceProductToDisplay(box, slot, slot.InteractionPosition);
                        placed++;

                        if (++safety > 5000)
                        {
                            log.LogWarning("Safety limit reached; stopping.");
                            log.LogInfo($"Placed {placed} products.");
                            return;
                        }
                    }

                    if (!box.HasProducts) break;
                }

                log.LogInfo($"Placed {placed} products onto shelves.");
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
            }
        }

        // Find the closest trash bin and use the game's own throw-into-bin logic.
        private static void TrashBox(BoxInteraction boxInteraction, Box box)
        {
            var log = Plugin.Log;

            var bins = UnityEngine.Object.FindObjectsOfType<TrashBin>();
            if (bins == null || bins.Length == 0)
            {
                log.LogInfo("Box is empty, but no trash bin was found to put it in.");
                return;
            }

            Vector3 boxPos = box.transform.position;
            TrashBin nearest = null;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < bins.Length; i++)
            {
                TrashBin bin = bins[i];
                if (bin == null) continue;

                float sqr = (bin.transform.position - boxPos).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = bin;
                }
            }

            if (nearest == null)
            {
                log.LogInfo("Box is empty, but no usable trash bin was found.");
                return;
            }

            boxInteraction.SetCurrentTrashBin(nearest);
            boxInteraction.ThrowIntoTrashBin();
            log.LogInfo("Threw empty box into the trash.");
        }
    }
}
