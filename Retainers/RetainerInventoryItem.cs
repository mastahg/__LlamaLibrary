﻿namespace LlamaLibrary.Retainers
{
    //TODO I'm baffled on how this is actually used right now
    public class RetainerInventoryItem
    {
        public uint TrueItemID;
        public uint RawItemID;
        public uint Count;
        public int Slot;
        public bool HQ => TrueItemID != RawItemID;

        public RetainerInventoryItem(uint trueItemId, uint rawItemId, uint count, int slot)
        {
            TrueItemID = trueItemId;
            RawItemID = rawItemId;
            Count = count;
            Slot = slot;
        }

        protected bool Equals(RetainerInventoryItem other)
        {
            return TrueItemID == other.TrueItemID && Count == other.Count && Slot == other.Slot;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((RetainerInventoryItem)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)TrueItemID;
                hashCode = (hashCode * 397) ^ (int)Count;
                hashCode = (hashCode * 397) ^ Slot;
                return hashCode;
            }
        }
    }
}