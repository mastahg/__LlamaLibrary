﻿using System.Runtime.InteropServices;
using ff14bot.Managers;

namespace LlamaLibrary.Structs;

[StructLayout(LayoutKind.Explicit, Size = 0x98)]
public struct TripleTriadCoinExchangeCard
{
    [FieldOffset(0x0)]
    public uint ItemId;

    [FieldOffset(0x04)]
    public uint Price;

    [FieldOffset(0x08)]
    public uint CardId;

    [FieldOffset(0x88)]
    public uint Count;

    [FieldOffset(0x8C)]
    public uint Count1;

    [FieldOffset(0x90)]
    public byte Index;

    [FieldOffset(0x90)]
    public uint SendAction;

    //This is fucked - ignore
    /*[FieldOffset(0x40)]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
    public byte[] name_bytes;*/

    public string Name => DataManager.GetItem(ItemId).CurrentLocaleName;

}