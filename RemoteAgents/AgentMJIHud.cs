﻿using System;
using ff14bot;
using ff14bot.Managers;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.RemoteAgents
{
    public class AgentMJIHud : AgentInterface<AgentMJIHud>, IAgent
    {
        public IntPtr RegisteredVtable => AgentMJIHudOffsets.VTable;

        

        protected AgentMJIHud(IntPtr pointer) : base(pointer)
        {
        }

        public IntPtr InfoPtr => Core.Memory.Read<IntPtr>(Pointer + AgentMJIHudOffsets.InfoPtr);
        public uint CurrentExp => Core.Memory.Read<uint>(InfoPtr + AgentMJIHudOffsets.CurrentExp);
    }
}