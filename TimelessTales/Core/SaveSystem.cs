using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TimelessTales.Blocks;
using TimelessTales.Entities;
using TimelessTales.World;

namespace TimelessTales.Core
{
    /// <summary>
    /// Data class holding all saved player state
    /// </summary>
    public class PlayerSaveData
    {
        public float PositionX, PositionY, PositionZ;
        public float RotationX, RotationY;
        public float Health, Hunger, Thirst;
        public Dictionary<BlockType, int> InventoryItems = new();
        public Dictionary<MaterialType, float> PouchMaterials = new();
        public BlockType SelectedBlock;
    }

    /// <summary>
    /// Data class for chunk save data
    /// </summary>
    public class ChunkSaveData
    {
        public int ChunkX, ChunkZ;
        public BlockType[,,] Blocks = new BlockType[Chunk.CHUNK_SIZE, Chunk.CHUNK_HEIGHT, Chunk.CHUNK_SIZE];
    }

    /// <summary>
    /// Complete save data for the game
    /// </summary>
    public class WorldSaveData
    {
        public int Seed;
        public float SpawnX, SpawnY, SpawnZ;
        public PlayerSaveData Player = new();
        public List<ChunkSaveData> Chunks = new();
        public int DayCount;
        public float TimeOfDay;
    }

    public static class SaveSystem
    {
        private const string SAVE_DIRECTORY = "Saves";
        private const string SAVE_FILE = "world.sav";
        private const int SAVE_VERSION = 1;

        /// <summary>
        /// Get the full path to the save file
        /// </summary>
        public static string GetSavePath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDir, SAVE_DIRECTORY, SAVE_FILE);
        }

        /// <summary>
        /// Check if a save file exists
        /// </summary>
        public static bool SaveExists(string? savePath = null)
        {
            return File.Exists(savePath ?? GetSavePath());
        }

        /// <summary>
        /// Save the complete game state to disk using binary format
        /// </summary>
        public static bool SaveGame(WorldSaveData data, string? savePath = null)
        {
            string path = savePath ?? GetSavePath();
            try
            {
                string? dir = Path.GetDirectoryName(path);
                if (dir != null)
                    Directory.CreateDirectory(dir);

                using var stream = File.Create(path);
                using var writer = new BinaryWriter(stream);

                // Header
                writer.Write(SAVE_VERSION);

                // World
                writer.Write(data.Seed);
                writer.Write(data.SpawnX);
                writer.Write(data.SpawnY);
                writer.Write(data.SpawnZ);
                writer.Write(data.DayCount);
                writer.Write(data.TimeOfDay);

                // Player
                var p = data.Player;
                writer.Write(p.PositionX);
                writer.Write(p.PositionY);
                writer.Write(p.PositionZ);
                writer.Write(p.RotationX);
                writer.Write(p.RotationY);
                writer.Write(p.Health);
                writer.Write(p.Hunger);
                writer.Write(p.Thirst);
                writer.Write((int)p.SelectedBlock);

                // Inventory
                writer.Write(p.InventoryItems.Count);
                foreach (var kvp in p.InventoryItems)
                {
                    writer.Write((int)kvp.Key);
                    writer.Write(kvp.Value);
                }

                // Pouch
                writer.Write(p.PouchMaterials.Count);
                foreach (var kvp in p.PouchMaterials)
                {
                    writer.Write((int)kvp.Key);
                    writer.Write(kvp.Value);
                }

                // Chunks
                writer.Write(data.Chunks.Count);
                foreach (var chunk in data.Chunks)
                {
                    writer.Write(chunk.ChunkX);
                    writer.Write(chunk.ChunkZ);
                    WriteChunkBlocksRLE(writer, chunk.Blocks);
                }

                Logger.Info($"Game saved successfully to {path}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to save game", ex);
                return false;
            }
        }

        /// <summary>
        /// Load the complete game state from disk
        /// </summary>
        public static WorldSaveData? LoadGame(string? savePath = null)
        {
            string path = savePath ?? GetSavePath();
            try
            {
                if (!File.Exists(path))
                {
                    Logger.Error("Save file not found: " + path);
                    return null;
                }

                using var stream = File.OpenRead(path);
                using var reader = new BinaryReader(stream);

                int version = reader.ReadInt32();
                if (version != SAVE_VERSION)
                {
                    Logger.Error($"Unsupported save version: {version}");
                    return null;
                }

                var data = new WorldSaveData();
                data.Seed = reader.ReadInt32();
                data.SpawnX = reader.ReadSingle();
                data.SpawnY = reader.ReadSingle();
                data.SpawnZ = reader.ReadSingle();
                data.DayCount = reader.ReadInt32();
                data.TimeOfDay = reader.ReadSingle();

                // Player
                var p = new PlayerSaveData();
                p.PositionX = reader.ReadSingle();
                p.PositionY = reader.ReadSingle();
                p.PositionZ = reader.ReadSingle();
                p.RotationX = reader.ReadSingle();
                p.RotationY = reader.ReadSingle();
                p.Health = reader.ReadSingle();
                p.Hunger = reader.ReadSingle();
                p.Thirst = reader.ReadSingle();
                p.SelectedBlock = (BlockType)reader.ReadInt32();

                int invCount = reader.ReadInt32();
                for (int i = 0; i < invCount; i++)
                {
                    var type = (BlockType)reader.ReadInt32();
                    int amount = reader.ReadInt32();
                    p.InventoryItems[type] = amount;
                }

                int pouchCount = reader.ReadInt32();
                for (int i = 0; i < pouchCount; i++)
                {
                    var type = (MaterialType)reader.ReadInt32();
                    float amount = reader.ReadSingle();
                    p.PouchMaterials[type] = amount;
                }

                data.Player = p;

                // Chunks
                int chunkCount = reader.ReadInt32();
                for (int i = 0; i < chunkCount; i++)
                {
                    var chunk = new ChunkSaveData();
                    chunk.ChunkX = reader.ReadInt32();
                    chunk.ChunkZ = reader.ReadInt32();
                    chunk.Blocks = ReadChunkBlocksRLE(reader);
                    data.Chunks.Add(chunk);
                }

                Logger.Info($"Game loaded successfully from {path}");
                return data;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load game", ex);
                return null;
            }
        }

        /// <summary>
        /// Write chunk blocks using RLE compression.
        /// Iterates z (inner), then y (middle), then x (outer) for consecutive block grouping.
        /// </summary>
        private static void WriteChunkBlocksRLE(BinaryWriter writer, BlockType[,,] blocks)
        {
            var runs = new List<(byte blockType, ushort count)>();
            byte currentType = (byte)blocks[0, 0, 0];
            ushort currentCount = 1;

            for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
            {
                for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
                {
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                    {
                        if (x == 0 && y == 0 && z == 0)
                            continue;

                        byte bt = (byte)blocks[x, y, z];
                        if (bt == currentType && currentCount < ushort.MaxValue)
                        {
                            currentCount++;
                        }
                        else
                        {
                            runs.Add((currentType, currentCount));
                            currentType = bt;
                            currentCount = 1;
                        }
                    }
                }
            }
            runs.Add((currentType, currentCount));

            writer.Write(runs.Count);
            foreach (var (blockType, count) in runs)
            {
                writer.Write(blockType);
                writer.Write(count);
            }
        }

        /// <summary>
        /// Read RLE-encoded chunk blocks
        /// </summary>
        private static BlockType[,,] ReadChunkBlocksRLE(BinaryReader reader)
        {
            var blocks = new BlockType[Chunk.CHUNK_SIZE, Chunk.CHUNK_HEIGHT, Chunk.CHUNK_SIZE];

            int runCount = reader.ReadInt32();
            int x = 0, y = 0, z = 0;

            for (int r = 0; r < runCount; r++)
            {
                byte blockType = reader.ReadByte();
                ushort count = reader.ReadUInt16();

                for (int i = 0; i < count; i++)
                {
                    blocks[x, y, z] = (BlockType)blockType;
                    z++;
                    if (z >= Chunk.CHUNK_SIZE)
                    {
                        z = 0;
                        y++;
                        if (y >= Chunk.CHUNK_HEIGHT)
                        {
                            y = 0;
                            x++;
                        }
                    }
                }
            }

            return blocks;
        }
    }
}
