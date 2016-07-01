using System;
using System.Collections.Generic;

using Offworld.AppCore;
using Offworld.GameCore;

namespace ExhaustibleResources
{
    public class ExhaustibleResources : ModEntryPointAdapter
    {
        // I can't figure out a good way run code when the day changes.
        // Other than storing the day, setting it initially, then checking for it when update's called.
        // This is horrendously inefficient. But it should work.
        int currentDay = -1;

        public override void Initialize()
        {
            UnityEngine.Debug.Log("[Exhaustible Resources] Loading Exhaustible Resources.");

            base.Initialize();
        }

        public override void Update()
        {
            GameClient client = AppGlobals.GameGlobals.GameClient;
            GameServer server = AppGlobals.GameGlobals.GameServer;
            if (client != null)
            {
                if (currentDay == -1)
                {
                    currentDay = client.getDays();
                }
                // The day has changed. Run the exhaustion code.
                else if (currentDay != client.getDays())
                {
                    UnityEngine.Debug.Log("[Exhaustible Resources] The day has changed, attempting to exhaust resources randomly.");
                    MapServer map = server.mapServer();
                    this.ExhaustResources(server, map);
                    currentDay = client.getDays();
                }
            }

            // This may be redundant.
            base.Update();
        }

        // Loop over the map, pick resources to deplete.
        public void ExhaustResources(GameServer server, MapServer map)
        {
            List<TileClient> tiles = map.getTileArray();
            foreach (TileClient tileClient in tiles)
            {
                TileServer tile = (TileServer)tileClient;
                //UnityEngine.Debug.Log("[Exhaustible Resources] Looping over a tile. tile x = " + tile.getX().ToString() + ", " + tile.getY().ToString());
                if (tile.hasResources() && tile.isBuilding())
                {
                    //UnityEngine.Debug.Log("[Exhaustible Resources] Tile has building and resource.");

                    BuildingServer building = server.buildingServer(tile.getBuildingID());

                    // Loop through all resources. If the building on the tile wants them, we have a chance to exhaust.
                    // (And if, you know, the resource is actually on the tile).
                    foreach (InfoResource resource in server.infos().resources())
                    {
                        ResourceType type = resource.meType;
                        InfoBuilding buildingInfo = server.infos().building(building.getType());
                        if (buildingInfo.maiResourceMining[(int)resource.miType] > 0 && tile.getResourceLevel(type, false) != ResourceLevelType.NONE)
                        {
                            bool check = CheckExhaustion(server);
                            if (check)
                            {
                                ExhaustTileResource(server, tile, type, resource);
                            }                    
                        }
                    }
                }
            }
        }

        // Look up the global int threshold.
        // Ideally, this would be a per-hq or per-building knob.
        private int GetExhaustThreshold(GameServer server)
        {
            int threshold = server.infos().getGlobalInt("EXHAUSTIBLE_RESOURCES_DEFAULT_THRESHOLD");
            return threshold;
        }

        private bool CheckExhaustion(GameServer server)
        {
            int result = server.random().Next(100);
            UnityEngine.Debug.Log("[Exhaustible Resources] Rolling random number. It is " + result.ToString());

            if (result <= this.GetExhaustThreshold(server))
            {
                return true;
            }
            return false;
        }

        // Exhaust a tile resource by setting its LevelType to the LevelType that's smaller by one.
        private void ExhaustTileResource(GameServer server, TileServer tile, ResourceType type, InfoResource resource)
        {
            ResourceLevelType level = tile.getResourceLevel(type, false);
            InfoResourceLevel levelInfo = server.infos().resourceLevel(level);

            // Get the level to deplete to.
            InfoResourceLevel nextInfo = GetNextResourceLevelInfo(server, levelInfo);
            ResourceLevelType nextLevel = nextInfo.meType;

            // Now, deplete the resources!
            tile.setResourceLevel(type, nextLevel);
            
            // Fire some notifications.
            //server.doUpdate();
            string x = tile.getX().ToString();
            string y = tile.getY().ToString();
            UnityEngine.Debug.Log("[Exhaustible Resources] Reduced resource " + resource.mzType + " in tile at x = " + x + ", y = " + y + " from " + levelInfo.mzType + " to " + nextInfo.mzType);
            
            // Get the hardcoded event to fire, get its ID, then fire it.
            string eventName = GetExhaustionEvent(server, type);
            if (eventName != "EVENTGAME_NONE")
            {
                foreach (InfoEventGame infoEvent in server.infos().eventGames())
                {
                    if (infoEvent.mzType == eventName)
                    {
                        server.gameEventsServer().AddEventGame(infoEvent.meType);
                        break;
                    }
                }
            }

        }

        private InfoResourceLevel GetNextResourceLevelInfo(GameServer server, InfoResourceLevel levelInfo)
        {
            // There has got to be a better way to do this. Where's our old (civ 4) friend getInfoTypeFromString?
            string nextLevelType = GetNextResourceLevelString(server, levelInfo);
            foreach (InfoResourceLevel testLevelInfo in server.infos().resourceLevels())
            {
                if (testLevelInfo.mzType == nextLevelType)
                {
                    return testLevelInfo;
                }
            }
            return server.infos().resourceLevel((ResourceLevelType)0);
        }

        // Hardcode the next resource level ID; I'm not sure what order they get loaded in.
        private string GetNextResourceLevelString(GameServer server, InfoResourceLevel levelInfo)
        {
            string type = levelInfo.mzType;
            switch (type)
            {
                case "RESOURCELEVEL_NONE":
                    return "RESOURCELEVEL_NONE";
                case "RESOURCELEVEL_TRACE":
                    return "RESOURCELEVEL_TRACE";
                case "RESOURCELEVEL_LOW":
                    return "RESOURCELEVEL_TRACE";
                case "RESOURCELEVEL_MEDIUM":
                    return "RESOURCELEVEL_LOW";
                case "RESOURCELEVEL_HIGH":
                    return "RESOURCELEVEL_HIGH";
                default:
                    return "RESOURCELEVEL_NONE";
            }
        }

        // Use the events system to fire off a message, depending on the type of resource.
        // This requires a hardcoded resource -> event mapping, sadly, since we cannot seem to add XML fields.
        private string GetExhaustionEvent(GameServer server, ResourceType resource)
        {
            string type = server.infos().resource(resource).mzType;
            switch (type)
            {
                case "RESOURCE_IRON":
                    return "EVENTGAME_EXHAUSTED_IRON";
                // This case doesn't seem to work properly. I am not sure why.
                case "RESOURCE_ALUMINUM":
                    return "EVENTGAME_EXHAUSTED_ALUMINUM";
                case "RESOURCE_SILICON":
                    return "EVENTGAME_EXHAUSTED_SILICON";
                case "RESOURCE_CARBON":
                    return "EVENTGAME_EXHAUSTED_CARBON";
                case "RESOURCE_WATER":
                    return "EVENTGAME_EXHAUSTED_IRON";
                default:
                    return "EVENTGAME_NONE";
            }
        }
    }
}
