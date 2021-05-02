using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.Tilemaps;
using Newtonsoft.Json;


namespace ZetaGames.RPG {
    public class MapManager : MonoBehaviour {

        public static MapManager Instance;
        public List<Tilemap> mapList;
        [SerializeField] private List<GlobalTileData> globalTileDataList;
        [SerializeField] private List<BaseObject> mapFeaturesDataList;
        [SerializeField] private int mapWidth = 50;
        [SerializeField] private int mapHeight = 50;
        [SerializeField] private float cellSize = 1;
        [SerializeField] private int chunkSize = 20;
        [SerializeField] private Vector3 originPosition = new Vector3(0, 0);
        private Dictionary<TileBase, GlobalTileData> globalTileDataDictionary;
        private ZetaGrid<WorldTile> worldTileGrid;
        private ZetaGrid<List<WorldTile>> chunkGrid;
        public float treeClusterChance = 0.1f; // in 1/100 percent
        public float treeAdjacencyChance = 20f; // 35 default
        public bool saveMapToFile;
        public bool loadMapFromFile;
        [HideInInspector] public bool initialized;
        public string fileName = "mapData";
        private string filePath;
        private string spriteAtlasAddress = "Assets/ZetaGamesRPG/Development/TeamSharedAssets/Sprites_Minifantasy/Biome - Forgotten Plains/Tileset/Minifactory_Grassland.spriteatlas";

        private void Awake() {
            // Create instance
            Instance = this;

            // Initialize
            filePath = @"d:\GameDevProjects\Team\ZetaRPG\Assets\ZetaGamesRPG\Development\Zach\Json\" + fileName + ".json";
            chunkGrid = new ZetaGrid<List<WorldTile>>(mapWidth / chunkSize, mapHeight / chunkSize, chunkSize, originPosition, (int x, int y) => new List<WorldTile>());
            worldTileGrid = new ZetaGrid<WorldTile>(mapWidth, mapHeight, cellSize, originPosition, (int x, int y) => new WorldTile(x, y));
            globalTileDataDictionary = new Dictionary<TileBase, GlobalTileData>();

            if (loadMapFromFile) {
                LoadJson();
            } else {
                // Create dictionary of all BaseTiles and their global data
                CreateGlobalDataDict();

                // Fill world tile grid with data. Will hold individual data on every tile in the game.
                FillWorldTileGrid();
            }

            if (saveMapToFile) {
                SaveJson();
            } else {
                // Randomize world features using world tile dictionary
                CreateRandomTrees();
            }

            initialized = true;
        }

        private void Start() {
            // Update Astar pathfinding graph to include world tile info ((MUST BE IN START))
            UpdatePathfindingGrid();
        }

        public ZetaGrid<WorldTile> GetWorldTileGrid() {
            return worldTileGrid;
        }

        public ZetaGrid<List<WorldTile>> GetChunkGrid() {
            return chunkGrid;
        }

        private void Update() {
            /*
            if (Input.GetMouseButtonDown(0)) {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                worldTileGrid.GetXY(mouseWorldPos, out int x, out int y);
                Vector3Int mouseGridPos = new Vector3Int(x, y, 0);

                WorldTile clickedTile = worldTileGrid.GetGridObject(mouseWorldPos);

                Debug.Log("Type: " + clickedTile.type + " || Walkable: " + clickedTile.walkable + " || Move cost: " + clickedTile.pathPenalty);

                mapList[1].SetTile(mouseGridPos, null);
            }
            */
        }

        /*
        public float GetTileMovementCost(Vector2 worldPosition) {
            Vector3Int gridPosition = map.WorldToCell(worldPosition);
            TileBase tile = map.GetTile(gridPosition);

            if (tile == null) {
                return 20;
            }

            return dataFromTiles[tile].movementCost;
        }
        */

        /*
        private void InitChunkDict() {
            for (int chunkX = 0; chunkX < mapWidth / chunkSize; chunkX++) {
                for (int chunkY = 0; chunkY < mapHeight / chunkSize; chunkY++) {
                    //mapChunkDictionary.Add(new Vector3Int(chunkX, chunkY, 0), new Dictionary<Vector3Int, WorldTile>());
                }
            }
        }
        */

        private void SaveJson() {
            List<WorldTile> mapTileList = new List<WorldTile>();
            Vector3Int mapGridPos = new Vector3Int();

            for (int mapX = 0; mapX < worldTileGrid.GetWidth(); mapX++) {
                for (int mapY = 0; mapY < worldTileGrid.GetHeight(); mapY++) {
                    mapGridPos.x = mapX;
                    mapGridPos.y = mapY;
                    mapGridPos.z = 0;
                    mapTileList.Add(worldTileGrid.GetGridObject(mapGridPos));
                }
            }

            string jsonSaveString = JsonConvert.SerializeObject(mapTileList, Formatting.None);
            File.WriteAllText(filePath, jsonSaveString);
        }

        private void LoadJson() {
            string jsonLoadData = File.ReadAllText(filePath);
            List<WorldTile> mapTileList = JsonConvert.DeserializeObject<List<WorldTile>>(jsonLoadData);

            foreach (WorldTile tile in mapTileList) {
                // world tile grid holds only the tile info for the highest priority tile (higher tilemap index)
                worldTileGrid.SetGridObject(tile.x, tile.y, tile);
                // chunk grid will hold all tiles so that it can render everything
                chunkGrid.GetGridObject(tile.chunkX, tile.chunkY).Add(tile);
            }
        }

        /*
        private void CreateMapFromJson() {
            // Populate world tile dictionary from json file
            LoadJson();
            Debug.Log("Json loaded");

            // Populate tilemap asynchronously with info from each world tile
            foreach (var tileList in mapChunkDictionary.Values) {
                foreach (WorldTile worldTile in tileList.Values) {
                    Debug.Log("Iterating through world tiles in dictionary");
                    foreach (Tilemap tilemap in mapList) {
                        Debug.Log("Iterating through tile maps in mapList");
                        if (tilemap.name.Equals(worldTile.tilemap)) {
                            Debug.Log("Tilemap name matched, loading sprite async next");
                            //   Tile tile = ScriptableObject.CreateInstance<Tile>();
                            //   string atlasedSpriteAddress = spriteAtlasAddress + '[' + worldTile.spriteName + ']';
                            //   var asyncOperationHandle = Addressables.LoadAssetAsync<Sprite>(atlasedSpriteAddress);
                            //   tile.sprite = asyncOperationHandle.Result;
                            //   tilemap.SetTile(new Vector3Int(worldTile.x, worldTile.y, 0), tile);
                            StartCoroutine(LoadSpriteAsync(worldTile, tilemap));

                        } else {
                            Debug.Log("Did not find a matching tilemap for this world tile");
                        }
                    }
                }
            }
        }
        */

        // Loads a single tile onto the tilemap asynchronously.
        // TODO: Come up with a way to load a chunk all together as one asynchronously
        public IEnumerator LoadSpriteAsync(WorldTile worldTile, Tilemap tilemap) {
            //Debug.Log("LoadSpriteAsync called");
            string atlasedSpriteAddress = spriteAtlasAddress + '[' + worldTile.spriteName + ']';
            var asyncOperationHandle = Addressables.LoadAssetAsync<Sprite>(atlasedSpriteAddress);

            yield return asyncOperationHandle;
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = asyncOperationHandle.Result;

            Vector3 worldPos = worldTileGrid.GetWorldPosition(worldTile.x, worldTile.y);
            Vector3Int tilemapPos = new Vector3Int((int)worldPos.x, (int)worldPos.y, 0);
            tilemap.SetTile(tilemapPos, tile);

            // Release at some point
            yield return new WaitForSeconds(3);
            Addressables.Release(asyncOperationHandle);
        }

        private void UpdatePathfindingGrid() {
            AstarPath.active.AddWorkItem(() => {
                var gg = AstarPath.active.data.gridGraph;
                Vector3Int mapTilePos = new Vector3Int();

                gg.GetNodes(node => {
                    // Find world tile based on current node position
                    worldTileGrid.GetXY((Vector3)node.position, out int mapX, out int mapY);

                    if (worldTileGrid.IsWithinGridBounds(mapX, mapY)) {
                        mapTilePos.x = mapX;
                        mapTilePos.y = mapY;
                        mapTilePos.z = 0;

                        WorldTile worldTile = worldTileGrid.GetGridObject(mapTilePos);

                        node.Penalty = (uint)(1000 * worldTile.pathPenalty);

                        if (!worldTile.walkable) {
                            node.Walkable = false;
                        }
                    }
                });
            });
        }

        private void CreateGlobalDataDict() {
            foreach (GlobalTileData tileData in globalTileDataList) {
                foreach (TileBase tile in tileData.tiles) {
                    if (!globalTileDataDictionary.ContainsKey(tile)) {
                        globalTileDataDictionary.Add(tile, tileData);
                    }
                }
            }
        }

        private void FillWorldTileGrid() {
            for (int chunkX = 0; chunkX < mapWidth / chunkSize; chunkX++) {
                for (int chunkY = 0; chunkY < mapHeight / chunkSize; chunkY++) {
                    for (int mapX = 0 + (chunkX * chunkSize); mapX < chunkSize + (chunkX * chunkSize); mapX++) {
                        for (int mapY = 0 + (chunkY * chunkSize); mapY < chunkSize + (chunkY * chunkSize); mapY++) {
                            List<WorldTile> chunkTileList = chunkGrid.GetGridObject(chunkX, chunkY);
                            WorldTile worldTile = worldTileGrid.GetGridObject(mapX, mapY);
                            Vector3Int mapGridPos = new Vector3Int(mapX, mapY, 0);

                            foreach (Tilemap tilemap in mapList) {
                                TileBase tile = tilemap.GetTile(mapGridPos);
                                bool addedToChunk = false;

                                if (tile != null) {
                                    if (globalTileDataDictionary.TryGetValue(tile, out GlobalTileData globalTileData)) {
                                        // set the global data per tile
                                        worldTile.pathPenalty = globalTileData.pathPenalty;
                                        worldTile.walkable = globalTileData.walkable;
                                        worldTile.type = globalTileData.type;
                                        worldTile.speedPercent = globalTileData.speedPercent;
                                        //worldTile.animated = globalTileData.animated;

                                        // set tilemap data
                                        worldTile.tilemap = mapList.IndexOf(tilemap);
                                        worldTile.spriteName = tilemap.GetSprite(mapGridPos).name;
                                        worldTile.chunkX = chunkX;
                                        worldTile.chunkY = chunkY;

                                        // if tile is animated
                                        if (worldTile.animated) {

                                        }
                                        
                                        // add map tile to chunk grid
                                        foreach (WorldTile chunkTile in chunkTileList) {
                                            if (chunkTile.x == worldTile.x && chunkTile.y == worldTile.y) {
                                                chunkTileList[chunkTileList.IndexOf(chunkTile)] = worldTile;
                                                addedToChunk = true;
                                                break;
                                            }
                                        }

                                        if (!addedToChunk) {
                                            chunkTileList.Add(worldTile);
                                        }
                                        
                                    } else {
                                        Debug.Log("TileBase not found in global tile dictionary: " + tile.name);
                                    }
                                } 
                            }
                        }
                    }
                }
            }
            /*
            List<WorldTile> worldTileList = new List<WorldTile>();

            for (int mapX = 0; mapX < mapWidth; mapX++) {
                for (int mapY = 0; mapY < mapHeight; mapY++) {
                    worldTileList.Add(worldTileGrid.GetGridObject(mapX, mapY));
                }
            }

            
            Debug.Log("Total World Tiles: " + worldTileList.Count);

            for (int chunkX = 0; chunkX < mapWidth / chunkSize; chunkX++) {
                for (int chunkY = 0; chunkY < mapHeight / chunkSize; chunkY++) {
                    Debug.Log("Chunk: (" + chunkX + ", " + chunkY + ") || List Size: " + chunkGrid.GetGridObject(chunkX, chunkY).Count);
                }
            }
            */
        }

        private void CreateRandomTrees() {
            for (int mapX = 0; mapX < worldTileGrid.GetWidth(); mapX++) {
                for (int mapY = 0; mapY < worldTileGrid.GetHeight(); mapY++) {
                    List<WorldTile> combinedNeighbors = new List<WorldTile>();
                    List<WorldTile> oneTileNeighbors = new List<WorldTile>();
                    List<WorldTile> twoTileNeighbors = new List<WorldTile>();
                    List<WorldTile> threeTileNeighbors = new List<WorldTile>();

                    int step;
                    int adjGridSize;
                    bool neighborsOccupied = false;

                    // get current tile
                    WorldTile currentTile = worldTileGrid.GetGridObject(mapX, mapY);

                    // if current tile is grass and unoccupied, then plant trees!
                    if (currentTile.type == ZetaUtilities.TERRAIN_GRASS && !currentTile.occupied) {
                        // NW - N - NE
                        // W  - X - E
                        // SW - S - SE

                        // Cache neighbor tiles one tile away (3x3 grid with one step from origin)
                        adjGridSize = 3;
                        step = 1;

                        for (int x = 0; x < adjGridSize; x++) {
                            for (int y = 0; y < adjGridSize; y++) {
                                if (Mathf.Abs(x - step) < step && Mathf.Abs(y - step) < step) {
                                    // skip inner tiles
                                    continue;
                                }
                                // check grid bounds
                                if (worldTileGrid.IsWithinGridBounds(mapX + (x - step), mapY + (y - step))) {
                                    WorldTile neighborTile = worldTileGrid.GetGridObject(mapX + (x - step), mapY + (y - step));
                                    oneTileNeighbors.Add(neighborTile);
                                }
                            }
                        }

                        // Cache neighbor tiles two tiles away (5x5 grid with two steps from origin)
                        adjGridSize = 5;
                        step = 2;

                        for (int x = 0; x < adjGridSize; x++) {
                            for (int y = 0; y < adjGridSize; y++) {
                                if (Mathf.Abs(x - step) < step && Mathf.Abs(y - step) < step) {
                                    // skip inner tiles
                                    continue;
                                }
                                // check grid bounds
                                if (worldTileGrid.IsWithinGridBounds(mapX + (x - step), mapY + (y - step))) {
                                    WorldTile neighborTile = worldTileGrid.GetGridObject(mapX + (x - step), mapY + (y - step));
                                    twoTileNeighbors.Add(neighborTile);
                                }
                            }
                        }

                        // Cache neighbor tiles three tiles away (7x7 grid with three steps from origin)
                        adjGridSize = 7;
                        step = 3;

                        for (int x = 0; x < adjGridSize; x++) {
                            for (int y = 0; y < adjGridSize; y++) {
                                if (Mathf.Abs(x - step) < step && Mathf.Abs(y - step) < step) {
                                    // skip inner tiles
                                    continue;
                                }
                                // check grid bounds
                                if (worldTileGrid.IsWithinGridBounds(mapX + (x - step), mapY + (y - step))) {
                                    WorldTile neighborTile = worldTileGrid.GetGridObject(mapX + (x - step), mapY + (y - step));
                                    threeTileNeighbors.Add(neighborTile);
                                }
                            }
                        }
                        // If all those neighbors are not occupied by a tree, then allow random chance of spawning first tree in a cluster
                        combinedNeighbors.AddRange(oneTileNeighbors);
                        combinedNeighbors.AddRange(twoTileNeighbors);
                        combinedNeighbors.AddRange(threeTileNeighbors);
                        foreach (WorldTile tile in combinedNeighbors) {
                            if (tile.occupied) {
                                if (tile.occupiedStatus.Contains(ResourceType.Wood.ToString() + ZetaUtilities.OCCUPIED_NODE)) {
                                    neighborsOccupied = true;
                                    break;
                                }
                            }
                        }

                        if (!neighborsOccupied) {
                            if (Random.Range(0, 100f) <= treeClusterChance) {
                                if (!currentTile.HasTileObjectPool()) {
                                    currentTile.occupied = true;
                                    currentTile.walkable = false;
                                    currentTile.occupiedStatus = ResourceType.Wood.ToString() + ZetaUtilities.OCCUPIED_NODE_FULL;
                                    currentTile.SetTileObjectPool(TileObjectPool.SharedInstance);

                                    // Specific to oak tree. Will change later
                                    currentTile.occupiedType = NodeType.Oak.ToString();

                                    // TODO: Possibly do a switch case to determine which tree is planted...base it on a biome type??
                                    // set additional tiles unwalkable depending on size of tree
                                    foreach (var featureData in mapFeaturesDataList) {
                                        if (typeof(ResourceNodeData).IsInstanceOfType(featureData)) {
                                            ResourceNodeData treeData = (ResourceNodeData)featureData;
                                            if (treeData.type.Equals(NodeType.Oak)) {
                                                foreach (Vector3Int modPos in treeData.adjacentGridOccupation) {
                                                    if (worldTileGrid.IsWithinGridBounds(mapX + modPos.x, mapY + modPos.y)) {
                                                        WorldTile otherTile = worldTileGrid.GetGridObject(mapX + modPos.x, mapY + modPos.y);
                                                        otherTile.occupied = true;
                                                        otherTile.walkable = false;
                                                        otherTile.occupiedStatus = ResourceType.Wood.ToString() + ZetaUtilities.OCCUPIED_NODE_ADJACENT;
                                                        currentTile.occupiedType = NodeType.Oak.ToString();
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    Debug.Log("Tree cluster seeded");
                                }
                            }
                        }

                        // If one and two tile neighbors are not occupied, but a tree is found in a three tile neighbor, then allow a good chance to spawn an additional tree (helps create clusters)
                        neighborsOccupied = false;
                        combinedNeighbors.Clear();
                        combinedNeighbors.AddRange(oneTileNeighbors);
                        combinedNeighbors.AddRange(twoTileNeighbors);

                        foreach (WorldTile tile in combinedNeighbors) {
                            if (tile.occupied) {
                                if (tile.occupiedStatus.Contains(ResourceType.Wood.ToString() + ZetaUtilities.OCCUPIED_NODE)) {
                                    neighborsOccupied = true;
                                    break;
                                }
                            }
                        }

                        if (!neighborsOccupied) {
                            // check for a tree occupying 3 tile neighbors
                            foreach (WorldTile tile in threeTileNeighbors) {
                                if (tile.occupied) {
                                    if (tile.occupiedStatus.Contains(ResourceType.Wood.ToString() + ZetaUtilities.OCCUPIED_NODE)) {
                                        neighborsOccupied = true;
                                        break;
                                    }
                                }
                            }

                            // if tree occupies a 3 tile neighbor, then roll to plant a tree
                            if (neighborsOccupied) {
                                if (Random.Range(0, 100f) <= treeAdjacencyChance) {
                                    if (!currentTile.HasTileObjectPool()) {
                                        currentTile.occupied = true;
                                        currentTile.walkable = false;
                                        currentTile.occupiedStatus = ResourceType.Wood.ToString() + ZetaUtilities.OCCUPIED_NODE_FULL;
                                        currentTile.SetTileObjectPool(TileObjectPool.SharedInstance);
                                        
                                        // Specific to oak tree. Will change later
                                        currentTile.occupiedType = NodeType.Oak.ToString();
                                        

                                        // set additional tiles unwalkable depending on size of tree
                                        foreach (var feature in mapFeaturesDataList) {
                                            if (typeof(ResourceNodeData).IsInstanceOfType(feature)) {
                                                ResourceNodeData tree = (ResourceNodeData)feature;
                                                if (tree.type.Equals(NodeType.Oak)) {
                                                    foreach (Vector3Int modPos in tree.adjacentGridOccupation) {
                                                        WorldTile otherTile = worldTileGrid.GetGridObject(mapX + modPos.x, mapY + modPos.y);
                                                        otherTile.occupied = true;
                                                        otherTile.walkable = false;
                                                        otherTile.occupiedStatus = ResourceType.Wood.ToString() + ZetaUtilities.OCCUPIED_NODE_ADJACENT;
                                                        currentTile.occupiedType = NodeType.Oak.ToString();
                                                    }
                                                }
                                            }
                                        }

                                        Debug.Log("Tree neighbor created");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
