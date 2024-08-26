using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace CalamityMod.NPCs
{
    public sealed class CalamityNetImportantNPC : GlobalNPC
    {
        private static Dictionary<int, int> typesToUpdate;

        public override void Load()
        {
            typesToUpdate = new();
        }

        public override void Unload()
        {
            typesToUpdate?.Clear();
            typesToUpdate = null;
        }

        public override void SetStaticDefaults()
        {
            int uniqueNetOffsetID = 0;

            #region Vanilla Enemies
            MarkNPCToNetImportant(NPCID.EaterofWorldsHead, uniqueNetOffsetID);
            MarkNPCToNetImportant(NPCID.EaterofWorldsBody, uniqueNetOffsetID);
            MarkNPCToNetImportant(NPCID.EaterofWorldsTail, uniqueNetOffsetID);
            uniqueNetOffsetID++;

            MarkNPCToNetImportant(NPCID.TheDestroyer, uniqueNetOffsetID);
            MarkNPCToNetImportant(NPCID.TheDestroyerBody, uniqueNetOffsetID);
            MarkNPCToNetImportant(NPCID.TheDestroyerTail, uniqueNetOffsetID);
            uniqueNetOffsetID++;
            #endregion Vanilla Enemies

            var types = AssemblyManager.GetLoadableTypes(CalamityMod.Instance.Code)
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ModNPC)));

            // Caching this for better performance
            var npcTypeMethod = typeof(ModContent).GetMethod(nameof(ModContent.NPCType));
            var netOffsetTable = new Dictionary<Type, int>();
            foreach (var type in types)
            {
                try
                {
                    var syncAttribute = type.GetCustomAttribute<AlwaysSyncTransformAttribute>();

                    if (syncAttribute == null)
                        continue;

                    var npcTypeActualMethod = npcTypeMethod.MakeGenericMethod(typeArguments: type);

                    int npcType = (int)npcTypeActualMethod.Invoke(null, null);
                    int netOffset = uniqueNetOffsetID;

                    Type typeToCheck = syncAttribute.SyncWith ?? type;
                    if (netOffsetTable.TryGetValue(typeToCheck, out int savedUniqueID))
                    {
                        netOffset = savedUniqueID;
                    }
                    else
                    {
                        netOffsetTable[typeToCheck] = netOffset;
                        uniqueNetOffsetID++;
                    }

                    MarkNPCToNetImportant(npcType, netOffset);
                }
                catch (Exception e)
                {
                    CalamityMod.Instance.Logger.Error($"Exception thrown while evaluating type \"{type.Name}\": {e}");
                }
            }

            netOffsetTable?.Clear();
        }

        public override void PostAI(NPC npc)
        {
            // Only Server should update this!
            if (!Main.dedServ)
                return;

            // Obviously deactived npc is not on our interest (not sure if this is case though)
            if (!npc.active)
                return;

            if (!typesToUpdate.TryGetValue(npc.type, out var netUpdateTickOffset))
                return;

            if ((Main.GameUpdateCount + netUpdateTickOffset) % 45 != 0)
                return;

            foreach (var player in Main.ActivePlayers)
            {
                // distance between 1000~1500 update with 8 tick period
                // and distance over 1500 will never update
                // So we forcely update NPC distanced over 1500 with 45 tick period
                float distance = CalamityUtils.ManhattanDistance(player.position, npc.position);
                if (distance <= 1499.0f)
                    continue;

                npc.SyncNPCPosAndRotOnly(); //Light-weight version to sync it's position
            }
        }

        private static void MarkNPCToNetImportant<NPCType>(int netUpdateTickOffset = 0) where NPCType : ModNPC
        {
            MarkNPCToNetImportant(ModContent.NPCType<NPCType>(), netUpdateTickOffset);
        }

        private static void MarkNPCToNetImportant(int npcType, int netUpdateTickOffset = 0)
        {
            typesToUpdate[npcType] = netUpdateTickOffset;
        }
    }
}
