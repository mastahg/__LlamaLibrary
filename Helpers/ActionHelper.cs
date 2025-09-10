﻿using System;
using System.Linq;
using System.Windows.Media;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaLibrary.Enums;
using LlamaLibrary.Extensions;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Utilities;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Helpers
{
    /// <summary>
    /// RB's ActionManager won't do the map decipher action on an item so this re-implements DoAction just for that reason.
    /// </summary>
    public static class ActionHelper
    {
        private static readonly LLogger Log = new(nameof(ActionHelper), Colors.Gold);

        

        public static bool UseAction(ff14bot.Enums.ActionType actionType, uint actionID, long targetID = 0xE000_0000, uint a4 = 0, uint a5 = 0, uint a6 = 0, uint a7 = 0)
        {
            Core.Memory.ClearCallCache();
            var result = Core.Memory.CallInjectedWraper<byte>(ActionHelperOffsets.DoAction,
            ActionHelperOffsets.ActionManagerParam,
            (uint)actionType,
            actionID,
            targetID,
            a4,
            a5,
            a6,
            a7) == 1;

            Core.Memory.ClearCallCache();

            return result;
        }

        public static bool DoActionDecipher(BagSlot slot)
        {
            if ((slot.Item.MyItemRole() != MyItemRole.Map) || HasMap())
            {
                return false;
            }

            Core.Memory.ClearCallCache();
            var result = Core.Memory.CallInjectedWraper<byte>(ActionHelperOffsets.DoAction,
            ActionHelperOffsets.ActionManagerParam,
            (uint)ff14bot.Enums.ActionType.Spell,
            (uint)ActionHelperOffsets.DecipherSpell,
            (long)Core.Player.ObjectId,
            slot.Slot | ((int)slot.BagId << 16),
            0,
            0,
            0) == 1;

            Core.Memory.ClearCallCache();

            return result;
        }

        public static bool HasMap()
        {
            var questMaps = new uint[] { 2001351, 2001705, 2001772, 200974 };
            return InventoryManager.GetBagByInventoryBagId(InventoryBagId.KeyItems).FilledSlots.Any(i => i.EnglishName.EndsWith("map", StringComparison.InvariantCultureIgnoreCase) && !questMaps.Contains(i.RawItemId));
        }

        public static void DiscardCurrentMap()
        {
            var map = CurrentMap();

            if (map != default(BagSlot))
            {
                map.Discard();
            }
        }

        public static BagSlot? CurrentMap()
        {
            var questMaps = new uint[] { 2001351, 2001705, 2001772, 200974 };
            var map = InventoryManager.GetBagByInventoryBagId(InventoryBagId.KeyItems).FilledSlots.Where(i => i.EnglishName.EndsWith("map", StringComparison.InvariantCultureIgnoreCase) && !questMaps.Contains(i.RawItemId)).ToList();
            return map.Count != 0 ? map.First() : default;
        }
    }
}