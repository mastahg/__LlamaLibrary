﻿using System.Runtime.InteropServices;

namespace LlamaLibrary.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    public struct BeastTribeStat
    {
        [FieldOffset(0x08)]
        public byte _Rank;

        [FieldOffset(0x0A)]
        public ushort Reputation;

        public ushort Rank => (ushort)(_Rank & 0x7F);
        public bool Unlocked => Rank != 0;

        public override string ToString()
        {
            return $"Rank: {Rank} Reputation: {Reputation}";
        }
    }
}