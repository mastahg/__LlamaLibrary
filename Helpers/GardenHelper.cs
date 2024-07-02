﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Service_Navigation;
using ff14bot.RemoteAgents;
using ff14bot.RemoteWindows;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.RemoteWindows;

namespace LlamaLibrary.Helpers
{
    public static class GardenHelper
    {
        private static readonly LLogger Log = new("TheGardener", Colors.LawnGreen);

        private static class Offsets
        {
            [Offset("Search 48 89 5C 24 ? 56 48 83 EC ? 48 8B F1 41 0F B7 D8 48 8D 0D ? ? ? ? E8 ? ? ? ? 48 85 C0 0F 84 ? ? ? ?")]
            [OffsetDawntrail("Search E8 ? ? ? ? 48 8B CB C7 43 ? ? ? ? ? E8 ? ? ? ? 48 8B 5C 24 ? B0 01 TraceCall")]
            internal static IntPtr PlantFunction;
            [Offset("Search 41 8B 4E ? 8D 93 ? ? ? ? Add 3 Read8")]
            internal static int StructOffset;
        }

        public static HousingPlantSelectedItemStruct SoilStruct => Core.Memory.Read<HousingPlantSelectedItemStruct>(AgentHousingPlant.Instance.Pointer + Offsets.StructOffset);
        public static HousingPlantSelectedItemStruct SeedStruct => Core.Memory.Read<HousingPlantSelectedItemStruct>(AgentHousingPlant.Instance.Pointer + Offsets.StructOffset + GreyMagic.MarshalCache<HousingPlantSelectedItemStruct>.Size);

        /*
        public static async Task GoGarden(uint AE, Vector3 gardenLoc)
        {
            Navigator.PlayerMover = new SlideMover();
            Navigator.NavigationProvider = new ServiceNavigationProvider();
            var house = WorldManager.AvailableLocations.FirstOrDefault(i => i.AetheryteId == AE);

            Log.Information($"Teleporting to housing: (ZID: {DataManager.ZoneNameResults[house.ZoneId]}, AID: {house.AetheryteId}) {house.Name}");
            await CommonTasks.Teleport(house.AetheryteId);

            Log.Debug("Waiting for zone to change.");
            await Coroutine.Wait(20000, () => WorldManager.ZoneId == house.ZoneId);

            Log.Information("Moving to selected garden plot.");

            if (gardenLoc != null)
            {
                await Navigation.FlightorMove(gardenLoc);
                await GardenHelper.Main(gardenLoc);
            }
        }
                */

        // TODO: arg `plantPlan` unused here.  Unfinished feature?  Safe to remove despite public method?
        public static async Task GoGarden(uint AE, Vector3 gardenLoc, List<Tuple<uint, uint>> plantPlan)
        {
            if (gardenLoc != default)
            {
                if (Navigator.NavigationProvider == null)
                {
                    Navigator.PlayerMover = new SlideMover();
                    Navigator.NavigationProvider = new ServiceNavigationProvider();
                }

                var house = WorldManager.AvailableLocations.FirstOrDefault(i => i.AetheryteId == AE);

                Log.Information($"Teleporting to housing: {house.Name} (Zone: {DataManager.ZoneNameResults[house.ZoneId]}, Aetheryte: {house.AetheryteId})");
                await GeneralFunctions.StopBusy(dismount: false);
                await CommonTasks.Teleport(house.AetheryteId);

                Log.Information("Waiting for zone to change.");
                await Coroutine.Wait(20000, () => WorldManager.ZoneId == house.ZoneId);

                if (WorldManager.ZoneId != house.ZoneId)
                {
                    Log.Information("Teleport failed for some reason, trying again.");
                    await CommonTasks.Teleport(house.AetheryteId);
                }

                Log.Information("Moving to selected garden plot.");
                await Navigation.FlightorMove(gardenLoc);
                await Main(gardenLoc);
            }
            else
            {
                Log.Information("No Garden Location set. Exiting Task.");
            }
        }

        public static bool AlwaysWater { get; set; }

        public static async Task<bool> Main(Vector3 gardenLoc)
        {
            var watering = GardenManager.Plants.Where(r => !Blacklist.Contains(r) && r.Distance2D(gardenLoc) < 10).ToArray();
            foreach (var plant in watering)
            {
                //Water it if it needs it or if we have fertilized it 5 or more times.
                if (AlwaysWater || GardenManager.NeedsWatering(plant))
                {
                    var result = GardenManager.GetCrop(plant);
                    if (result != null)
                    {
                        Log.Information($"Watering {result} {plant.ObjectId:X}");
                        await Navigation.FlightorMove(plant.Location);
                        plant.Interact();
                        if (!await Coroutine.Wait(5000, () => Talk.DialogOpen))
                        {
                            continue;
                        }

                        Talk.Next();
                        if (!await Coroutine.Wait(5000, () => SelectString.IsOpen))
                        {
                            continue;
                        }

                        if (!await Coroutine.Wait(5000, () => SelectString.LineCount > 0))
                        {
                            continue;
                        }

                        if (SelectString.LineCount == 4)
                        {
                            SelectString.ClickSlot(1);
                            await Coroutine.Sleep(2300);
                        }
                        else
                        {
                            Log.Information("Plant is ready to be harvested");
                            SelectString.ClickSlot(1);
                            await Coroutine.Sleep(1000);
                        }
                    }
                    else
                    {
                        Log.Error($"GardenManager.GetCrop returned null {plant.ObjectId:X}");
                    }
                }
            }

            var slots = GeneralFunctions.MainBagsFilledSlots().Where(x => x.RawItemId == 7767);
            Log.Information($"Found {slots.Count()} slots filled with fish meal.");
            if (slots.Count() < 1)
            {
                Log.Information($"No fertilizer in bag, skipping fertilize.");
                return true;
            }
            var plants = GardenManager.Plants.Where(r => r.Distance2D(gardenLoc) < 10).ToArray();
            foreach (var plant in plants)
            {
                var result = GardenManager.GetCrop(plant);
                if (result == null)
                {
                    continue;
                }

                Log.Information($"Fertilizing {GardenManager.GetCrop(plant)} {plant.ObjectId:X}");
                await Navigation.FlightorMove(plant.Location);
                plant.Interact();
                if (!await Coroutine.Wait(5000, () => Talk.DialogOpen))
                {
                    continue;
                }

                Talk.Next();
                if (!await Coroutine.Wait(5000, () => SelectString.IsOpen))
                {
                    continue;
                }

                if (!await Coroutine.Wait(5000, () => SelectString.LineCount > 0))
                {
                    continue;
                }

                if (SelectString.LineCount == 4)
                {
                    SelectString.ClickSlot(0);
                    if (await Coroutine.Wait(2000, () => GardenManager.ReadyToFertilize))
                    {
                        if (GardenManager.Fertilize() != FertilizeResult.Success)
                        {
                            continue;
                        }

                        Log.Information($"Plant with objectId {plant.ObjectId:X} was fertilized");
                        await Coroutine.Sleep(2300);
                    }
                    else
                    {
                        Log.Information($"Plant with objectId {plant.ObjectId:X} not able to be fertilized, trying again later");
                    }
                }
                else
                {
                    Log.Information("Plant is ready to be harvested");
                    SelectString.ClickSlot(1);
                    await Coroutine.Sleep(1000);
                }
            }

            return true;
        }

        public static async Task Plant(BagSlot seeds, BagSlot soil)
        {
            var result = Core.Memory.CallInjected64<IntPtr>(Offsets.PlantFunction, new object[3]
            {
                AgentHousingPlant.Instance.Pointer,
                (uint)soil.BagId,
                soil.Slot
            });
            result = Core.Memory.CallInjected64<IntPtr>(Offsets.PlantFunction, new object[3]
            {
                AgentHousingPlant.Instance.Pointer,
                (uint)seeds.BagId,
                seeds.Slot
            });

            await Coroutine.Wait(5000, () => SeedStruct.ItemId == seeds.RawItemId && SoilStruct.ItemId == soil.RawItemId);
            HousingGardening.Confirm();
            await Coroutine.Wait(5000, () => SelectYesno.IsOpen);
            if (SelectYesno.IsOpen)
            {
                SelectYesno.Yes();
            }

            await Coroutine.Wait(5000, () => !SelectYesno.IsOpen);
        }

        public static async Task Plant(int GardenIndex, int PlantIndex, BagSlot seeds, BagSlot soil)
        {
            var plants = GardenManager.Plants.Where(i => i.Distance(Core.Me.Location) < 10);
            EventObject? plant = null;
            foreach (var tmpPlant in plants)
            {
                var _GardenIndex = Lua.GetReturnVal<int>($"return _G['{tmpPlant.LuaString}']:GetHousingGardeningIndex();");
                if (_GardenIndex != GardenIndex)
                {
                    continue;
                }

                var _PlantIndex = Lua.GetReturnVal<int>($"return _G['{tmpPlant.LuaString}']:GetHousingGardeningPlantIndex();");
                if (_PlantIndex != PlantIndex)
                {
                    continue;
                }

                var _Plant = DataManager.GetItem(Lua.GetReturnVal<uint>($"return _G['{tmpPlant.LuaString}']:GetHousingGardeningPlantCrop();"));
                if (_Plant != null)
                {
                    plant = tmpPlant;
                    break;
                }
            }

            if (plant != null)
            {
                await Plant(plant, seeds, soil);
            }
        }

        public static async Task Plant(EventObject plant, BagSlot seeds, BagSlot soil)
        {
            if (plant != null)
            {
                if (!plant.IsWithinInteractRange)
                {
                    await Navigation.FlightorMove(plant.Location);
                }

                if (plant.IsWithinInteractRange)
                {
                    plant.Interact();
                    await Coroutine.Wait(5000, () => Talk.DialogOpen);
                    if (Talk.DialogOpen)
                    {
                        Talk.Next();
                    }

                    await Coroutine.Wait(5000, () => Conversation.IsOpen);
                    if (Conversation.IsOpen)
                    {
                        Conversation.SelectLine(0);
                    }

                    await Coroutine.Wait(5000, () => HousingGardening.IsOpen);
                    if (HousingGardening.IsOpen)
                    {
                        await Plant(seeds, soil);
                    }
                }
            }
        }
    }
}