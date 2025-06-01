﻿using System;
using System.Text;
using ff14bot;
using ff14bot.Enums;
using LlamaLibrary.Helpers;

namespace LlamaLibrary.RemoteWindows
{
    //TODO Move element numbers to dictionary
    public class HWDSupply : RemoteWindow<HWDSupply>
    {
        public HWDSupply() : base("HWDSupply")
        {
        }

        public int CurrentClassSelected()
        {
            if (Translator.Language == Language.Chn)
            {
                return Elements[29].TrimmedData;
            }

            return Elements[62].TrimmedData;
        }

        public int GetAccumulatedScore()
        {
            return Elements[17 + CurrentClassSelected()].TrimmedData;
        }

        public int GetKupoVoucherCount()
        {
            if (Translator.Language == Language.Chn)
            {
                return 0;
            }

            var data = Core.Memory.ReadString((IntPtr)Elements[3].Data, Encoding.UTF8).Split('/');
            return data.Length < 2 ? 0 : int.Parse(data[0].Trim());
        }

        public int ClassSelected
        {
            get => CurrentClassSelected();
            set
            {
                if (WindowByName != null && CurrentClassSelected() != value)
                {
                    SendAction(2, 0, 1, 1, (ulong)value);
                }
            }
        }

        public void ClickItem(int index)
        {
            SendAction(2, 3, 1, 3, (ulong)index);
        }

        public override void Close()
        {
            SendAction(1, 3, ulong.MaxValue);
        }
    }
}