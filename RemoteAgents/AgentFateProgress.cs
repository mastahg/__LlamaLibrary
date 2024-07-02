﻿using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Structs;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentFateProgress : AgentInterface<AgentFateProgress>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.VTable;
        private static class Offsets
        {
            [Offset("Search 48 8D 05 ? ? ? ? 48 89 07 48 8D 4F 50 Add 3 TraceRelative")]
            [OffsetDawntrail("Search 48 8D 05 ? ? ? ? 66 41 C7 46 ? ? ? Add 3 TraceRelative")]
            internal static IntPtr VTable;

            //[Offset("Search 66 89 47 ? 48 8B 5C 24 ? 48 8B 74 24 ? Add 3 Read8")]
            //internal static int LoadedZones;
            //[Offset("Search 48 8B 47 ? 48 8B CF 48 89 47 ? 33 C0 Add 3 Read8")]
            //internal static int ZoneStructs;
        }

        public int NumberOfLoadedZones => 0; //Core.Memory.NoCacheRead<byte>(Pointer + Offsets.LoadedZones);

        public SharedFateProgress[] ProgressArray => new SharedFateProgress[0];

        //Core.Memory.ReadArray<SharedFateProgress>(Core.Memory.Read<IntPtr>(Pointer + Offsets.ZoneStructs), NumberOfLoadedZones);

        protected AgentFateProgress(IntPtr pointer) : base(pointer)
        {
        }
    }
}