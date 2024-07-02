﻿using System;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentItemAppraisal : AgentInterface<AgentItemAppraisal>, IAgent
    {
        public IntPtr RegisteredVtable => Offsets.VTable;

        private static class Offsets
        {
            [Offset("Search 48 8D 05 ? ? ? ? 33 D2 48 89 03 48 8D 4B ? 33 C0 Add 3 TraceRelative")]
            [OffsetDawntrail("Search 48 8D 05 ? ? ? ? 48 89 03 33 C0 48 89 43 ? 48 89 43 ? 48 89 83 ? ? ? ? Add 3 TraceRelative")]
            internal static IntPtr VTable;
            //6.4
            [Offset("Search 66 C7 87 ? ? ? ? ? ? 48 8D 45 ? Add 3 Read8")]
            [OffsetDawntrail("Search 66 C7 87 ? ? ? ? ? ? 48 8D 4D ? Add 3 Read8")]
            internal static int ItemAppraisalReady;
        }

        protected AgentItemAppraisal(IntPtr pointer) : base(pointer)
        {
        }
    }
}