﻿using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.Helpers.NPC
{
    public static class NpcHelper
    {
        private static Dictionary<uint, string>? EventObjectNames = null;
        private static Dictionary<uint, (string Name, string Plural, string Title)> _ENpcResident = new();

        private static class Offsets
        {
            [Offset("Search E8 ? ? ? ? 48 85 C0 74 ? 48 8D 48 08 E8 ? ? ? ? EB ? 83 C8 ? TraceCall")]
            internal static IntPtr GetENpcResident;
        }

        public static Npc? GetClosestNpc(IEnumerable<Npc> npcs)
        {
            if (npcs.Any(i => i.IsInCurrentZone))
            {
                return npcs.Where(i => i.CanGetTo).OrderByDescending(i => i.IsInCurrentZone).ThenByDescending(i => i.IsInCurrentArea).ThenBy(i => i.TeleportCost).ThenBy(i => i.Location.Coordinates.Distance2DSqr(Core.Me.Location)).FirstOrDefault();
            }

            return npcs.Where(i => i.CanGetTo).OrderByDescending(i => i.IsInCurrentZone).ThenByDescending(i => i.IsInCurrentArea).ThenBy(i => i.TeleportCost).ThenBy(i => i.Location.Coordinates.Distance2DSqr(i.Location.ClosestAetheryteResult.Position)).FirstOrDefault();
        }

        public static List<Npc> OrderByDistance(IEnumerable<Npc> npcs)
        {
            if (npcs.Any(i => i.IsInCurrentZone))
            {
                return npcs.Where(i => i.CanGetTo).OrderByDescending(i => i.IsInCurrentZone).ThenByDescending(i => i.IsInCurrentArea).ThenBy(i => i.TeleportCost).ThenBy(i => i.Location.Coordinates.Distance2DSqr(Core.Me.Location)).ToList();
            }

            return npcs.Where(i => i.CanGetTo).OrderByDescending(i => i.IsInCurrentZone).ThenByDescending(i => i.IsInCurrentArea).ThenBy(i => i.TeleportCost).ThenBy(i => i.Location.Coordinates.Distance2DSqr(i.Location.ClosestAetheryteResult.Position)).ToList();
        }

        public static string GetNpcName(uint npcId, bool includeTitle = true)
        {
            if (npcId > 2_000_000)
            {
                return GetEventObjectName(npcId);
            }

            if (npcId <= 1_000_000)
            {
                return "";
            }

            (var name, _, var title) = CallGetENpcResident(npcId);
            return (includeTitle && title != "") ? $"{name} ({title})" : name;
        }

        public static string GetEventObjectName(uint npcId)
        {
            if (EventObjectNames == null)
            {
                using var Database = new Database("db.s3db");
                var results = Database.AllAsDictionary<EventObjectResult>();
                EventObjectNames = new Dictionary<uint, string>(results.Count);
                foreach (var objectResult in results)
                {
                    if (objectResult.Value.CurrentLocaleName != "")
                    {
                        EventObjectNames.Add(objectResult.Key, objectResult.Value.CurrentLocaleName);
                    }
                }

                results.Clear();
            }

            if (npcId > 2_000_000)
            {
                npcId -= 2_000_000;
            }

            EventObjectNames.TryGetValue(npcId, out var value);
            return value ?? "";
        }

        public static (string Name, string Plural, string Title) CallGetENpcResident(uint npcId)
        {
            if (_ENpcResident.TryGetValue(npcId, out var value))
            {
                return value;
            }

            var ptr = Core.Memory.CallInjected64<IntPtr>(Offsets.GetENpcResident, npcId);

            if (ptr == IntPtr.Zero)
            {
                return ("", "", "");
            }

            var name = Core.Memory.ReadStringUTF8(ptr + 0x18);
            var plural = Core.Memory.ReadStringUTF8(ptr + 0x18 + name.Length + 1);
            var title = Core.Memory.ReadStringUTF8(ptr + 0x18 + name.Length + 1 + 1 + plural.Length);
            _ENpcResident.Add(npcId, (name, plural, title));
            return (name, plural, title);
        }
    }
}