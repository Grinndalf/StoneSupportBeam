using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using JsonObject = Vintagestory.API.Datastructures.JsonObject;
using System.Text;

namespace StoneSupportBeam
{
    public class StoneSupportBeamConfig
    {
        public bool includeAllStoneTypes = true;
        public string description = "Set this to false if you want to exclude implausible types (by default, every variant is available.)";
        public string ImplausibleTypes = "halite, chalk, claystone, shale, bauxite, kimberlite, obsidian, chalk-ashlar, claystone-ashlar, shale-ashlar, bauxite-ashlar (list of implausible types, changing this does NOTHING.)";
    }
    public class StoneSupportBeamModSystem : ModSystem
    {
        StoneSupportBeamConfig allStones;
        string[] implausibleStates = ["halite", "chalk", "claystone", "shale", "bauxite", "kimberlite", "obsidian", "chalk-ashlar", "claystone-ashlar", "shale-ashlar", "bauxite-ashlar"];
        public override void Start(ICoreAPI api)
        {
            allStones = api.LoadModConfig<StoneSupportBeamConfig>("stonesupportbeamconfig.json");
            if (allStones == null)
            {
                allStones = new StoneSupportBeamConfig();
                api.StoreModConfig(allStones, "stonesupportbeamconfig.json");
            }
            // Mod.Logger.Notification($"Current setting is {allStones.includeAllStoneTypes}");
        }

        public override void AssetsLoaded(ICoreAPI api)
        {   
            if (allStones.includeAllStoneTypes == false)
            {
                PatchJson(api, "blocktypes/stonesupportbeam.json", "states");
                PatchJson(api, "recipes/grid/stonesupportbeam.json", "allowedVariants");
                PatchJson(api, "recipes/grid/crushedstone.json", "allowedVariants");
                PatchJson(api, "recipes/grid/crushedbrick.json", "allowedVariants");
                PatchJson(api, "recipes/barrel/stonebeam-cementmix.json", "allowedVariants");
                PatchJson(api, "recipes/barrel/stonebeam-concretemix.json", "allowedVariants");
                PatchJson(api, "recipes/barrel/stonebeam-concretemix-ashlar.json", "allowedVariants");
                PatchJson(api, "recipes/barrel/stonebeam-concretemix-other.json", "allowedVariants");
                PatchJson(api, "itemtypes/resource/crushedbrick.json", "states");
                PatchJson(api, "itemtypes/resource/crushedstone.json", "states");
                PatchJson(api, "itemtypes/resource/stonebeam-concretemix.json", "states");
                PatchJson(api, "itemtypes/liquid/cementmix.json", "states", "portion");
            }
            
        }
        void RemoveStates(JArray array, string[] toRemove)
        {
            List<JToken> toRemoveList = new List<JToken>();
            foreach (var state in array)
            {
                if (toRemove.Contains(state.ToString()))
                {
                    toRemoveList.Add(state);
                }
            }
            foreach (var removeState in toRemoveList)
            {
                array.Remove(removeState);
            }
        }
        void PatchJson(ICoreAPI api, string path, string listType, string suffix = "")
        {
            var jsonAsset = api.Assets.TryGet(new AssetLocation("stonesupportbeam", path));
            if (jsonAsset == null)
            {
                Mod.Logger.Notification("No assets are loaded!");
                return;
            }
            Mod.Logger.Notification("Assets loaded!");

            List<string> statesToRemove = new List<string>();
            foreach (var state in implausibleStates)
            {
                statesToRemove.Add(state + suffix);
            }
            JToken wholeJson = JToken.Parse(jsonAsset.ToText());
            var allowedLists = wholeJson.SelectTokens("$.." + listType).ToList();
            foreach (var allowedList in allowedLists)
            {
                JArray list = (JArray)allowedList;
                RemoveStates(list, statesToRemove.ToArray());
                jsonAsset.Data = Encoding.UTF8.GetBytes(wholeJson.ToString());
            }

        }
    }
}