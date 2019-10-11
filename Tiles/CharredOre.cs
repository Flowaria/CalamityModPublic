using CalamityMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Tiles
{
    public class CharredOre : ModTile
    {
        private int sheetWidth = 288;
        private int sheetHeight = 270;

        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileValue[Type] = 710;

            TileMerge.MergeGeneralTiles(Type);
            TileMerge.MergeHellTiles(Type);

            drop = mod.ItemType("CharredOre");
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Charred Ore");
            AddMapEntry(new Color(17, 16, 26), name);
            mineResist = 6f;
            minPick = 200;
            soundType = 21;
            dustType = 235;
            Main.tileSpelunker[Type] = true;
        }

        public override bool CanExplode(int i, int j)
        {
            return NPC.downedPlantBoss;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameXOffset = (i % 2) * sheetWidth;
            frameYOffset = (j % 2) * sheetHeight;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.50f;
            g = 0.00f;
            b = 0.00f;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            CustomTileFraming.FrameTileForCustomMerge(i, j, Type, mod.TileType("BrimstoneSlag"), false, false, false, false, resetFrame);
            return false;
        }
    }
}
