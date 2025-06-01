﻿using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.RemoteAgents;
using ff14bot.RemoteWindows;
using LlamaLibrary.Logging;

namespace LlamaLibrary.RemoteWindows
{
    public class ShopExchangeItem : RemoteWindow<ShopExchangeItem>
    {
        private static readonly LLogger Log = new(nameof(ShopExchangeItem), Colors.Fuchsia);

        public ShopExchangeItem() : base("ShopExchangeItem")
        {
        }

        public async Task<uint> Purchase(uint itemId, uint itemCount = 1)
        {
            if (!IsOpen)
            {
                return 0u;
            }

            var items = SpecialShopManager.Items;

            var specialShopItem = items?.Cast<SpecialShopItem?>().FirstOrDefault(i => i.HasValue && i.Value.ItemIds.Contains(itemId) && CanAfford(i.Value));

            if (!specialShopItem.HasValue)
            {
                return 0u;
            }

            Log.Information($"Buying {specialShopItem.Value.Item0.CurrentLocaleName}");

            if (itemCount > specialShopItem.Value.Item0.StackSize)
            {
                itemCount = specialShopItem.Value.Item0.StackSize;
            }

            if (!CanAfford(specialShopItem.Value))
            {
                return 0;
            }

            Log.Information($"Can afford {CanAfford(specialShopItem.Value)}");
            var index = items?.IndexOf(specialShopItem.Value);

            if (index == null)
            {
                return 0;
            }

            var obj = new ulong[8]
            {
                3uL,
                0uL,
                3uL,
                0uL,
                3uL,
                0uL,
                0uL,
                0uL
            };
            obj[3] = (uint)index;
            obj[5] = itemCount;

            SendAction(4, obj);
            Log.Verbose("Sent Action for purchase");

            await Coroutine.Wait(5000, () => RaptureAtkUnitManager.GetWindowByName("ShopExchangeItemDialog") != null);

            if (RaptureAtkUnitManager.GetWindowByName("ShopExchangeItemDialog") != null)
            {
                Log.Verbose("ShopExchangeItemDialog Open");
                RaptureAtkUnitManager.GetWindowByName("ShopExchangeItemDialog").SendAction(1, 3, 0);
                await Coroutine.Wait(5000, () => RaptureAtkUnitManager.GetWindowByName("ShopExchangeItemDialog") == null);

                await Coroutine.Wait(2000, () => SelectYesno.IsOpen || Request.IsOpen);

                if (SelectYesno.IsOpen)
                {
                    Log.Verbose("SelectYesno open");
                    SelectYesno.Yes();
                    await Coroutine.Wait(2000, () => !SelectYesno.IsOpen);

                    await Coroutine.Wait(3000, () => Request.IsOpen);
                }

                if (Request.IsOpen)
                {
                    Log.Verbose("Purchase request");

                    await CommonTasks.HandOverRequestedItems();

                    /*
                    for (var i = 0; i < 3; i++)
                    {
                        BagSlot item;
                        if (specialShopItem.Value.HqCurrencies[i])
                        {
                            item = InventoryManager.FilledInventoryAndArmory.FirstOrDefault(j => j.RawItemId == specialShopItem.Value.CurrencyTypes[i] && j.Count >= specialShopItem.Value.CurrencyCosts[i] && j.IsHighQuality == specialShopItem.Value.HqCurrencies[i]);
                        }
                        else
                        {
                            item = InventoryManager.FilledInventoryAndArmory.FirstOrDefault(j => j.RawItemId == specialShopItem.Value.CurrencyTypes[i] && j.Count >= specialShopItem.Value.CurrencyCosts[i]);
                        }

                        Log.Information($"[Purchase] Request item {item}");
                        if (item != null)
                        {
                            item.Handover();
                            await Coroutine.Sleep(500);
                        }
                    }

                    if (Request.HandOverButtonClickable)
                    {
                        Request.HandOver();
                    }

                    await Coroutine.Sleep(1000);
                    */
                }
                else
                {
                    Log.Debug("[Purchase] Request Not open");
                }

                if (QuestLogManager.InCutscene && AgentInterface<AgentCutScene>.Instance.CanSkip && !SelectString.IsOpen)
                {
                    AgentInterface<AgentCutScene>.Instance.PromptSkip();
                    await Coroutine.Wait(6000, () => SelectString.IsOpen);
                    SelectString.ClickSlot(0);
                    await CommonTasks.HandleLoading();
                    await Coroutine.Wait(6000, () => !QuestLogManager.InCutscene);
                    await CommonTasks.HandleLoading();
                    await Coroutine.Sleep(500);
                }
            }

            return itemCount;
        }

        private static bool CanAfford(SpecialShopItem item)
        {
            for (var i = 0; i < 3; i++)
            {
                if (item.CurrencyCosts[i] == 0)
                {
                    continue;
                }

                if (!InventoryManager.FilledInventoryAndArmory.Any(j => j.RawItemId == item.CurrencyTypes[i] && j.Count >= item.CurrencyCosts[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}