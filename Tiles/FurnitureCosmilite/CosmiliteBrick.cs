﻿
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FurnitureCosmilite
{
    public class CosmiliteBrick : ModTile
    {
        internal static FramedGlowMask GlowMask;

        public override void SetStaticDefaults()
        {
            GlowMask = new("CalamityMod/Tiles/FurnitureCosmilite/CosmiliteBrickGlow", 18, 18);

            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            HitSound = SoundID.Tink;
            MineResist = 3f;
            AddMapEntry(new Color(76, 79, 133));
        }

        public override void Unload()
        {
            GlowMask?.Unload();
            GlowMask = null;
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Firework_Blue, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Firework_Pink, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            return false;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            var tileCache = Main.tile[i, j];
            int xPos = tileCache.TileFrameX;
            int yPos = tileCache.TileFrameY;

            if (GlowMask.HasContentInFramePos(xPos, yPos))
            {
                Color drawColour = GetDrawColour(i, j, Color.White);
                TileFraming.SlopedGlowmask(in tileCache, i, j, GlowMask.Texture, null, GetDrawColour(i, j, drawColour), default);
            }
        }

        private Color GetDrawColour(int i, int j, Color colour)
        {
            int colType = Main.tile[i, j].TileColor;
            Color paintCol = WorldGen.paintColor(colType);
            if (colType >= 13 && colType <= 24)
            {
                colour.R = (byte)(paintCol.R / 255f * colour.R);
                colour.G = (byte)(paintCol.G / 255f * colour.G);
                colour.B = (byte)(paintCol.B / 255f * colour.B);
            }
            return colour;
        }
    }
}
