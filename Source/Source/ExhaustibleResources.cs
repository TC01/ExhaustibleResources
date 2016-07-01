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

        // For now, hardcode a default probability for resources to deplete.
        // If we could add XML tags (which we *should* be able to were it not for this mod entry point interface,
        // since we have the source code...), I would make this dependent on which faction you were playing as.
        int exhaustThreshold = 10;

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
                    foreach (InfoResource resource in server.infos().resources())
                    {
                        ResourceType type = resource.meType;
                        InfoBuilding buildingInfo = server.infos().building(building.getType());
                        if (buildingInfo.maiResourceMining[(int)resource.miType] > 0)
                        {
                            bool check = CheckExhaustion();
                            if (check)
                            {
                                ExhaustTileResource(server, tile, type, resource);
                            }                    
                        }
                    }
                }
            }
        }

        private bool CheckExhaustion()
        {
            Random random = new Random();
            
            int result = random.Next(100);
            UnityEngine.Debug.Log("[Exhaustible Resources] Rolling random number. It is " + result.ToString());

            if (result <= this.exhaustThreshold)
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
            int nextID = levelInfo.miType -= 1;

            // Do not go below "traces" as our minimum threshold
            if (nextID <= 1)
            {
                nextID = 1;
            }

            // Now, deplete the resources!
            ResourceLevelType nextLevel = (ResourceLevelType)nextID;
            tile.setResourceLevel(type, nextLevel);
            
            // Fire some notifications and updates.
            server.doUpdate();
            UnityEngine.Debug.Log("[Exhaustible Resources] Reduced resource " + resource.mzType + " in tile at x = " + tile.getX().ToString() + ", y = " + tile.getY().ToString() + " to level " + levelInfo.mzType);
            
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

        // Use the events system to fire off a message, depending on the type of resource.
        // This requires a hardcoded resource -> event mapping, sadly, since we cannot seem to add XML fields.
        private string GetExhaustionEvent(GameServer server, ResourceType resource)
        {
            string type = server.infos().resource(resource).mzType;
            switch (type)
            {
                case "RESOURCE_IRON":
                    return "EVENTGAME_EXHAUSTED_IRON";
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
