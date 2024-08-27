
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FurnitureVoid
{
    public class SmoothVoidstone : ModTile
    {
        internal static FramedGlowMask GlowMask;

        public override void SetStaticDefaults()
        {
            GlowMask = new("CalamityMod/Tiles/FurnitureVoid/SmoothVoidstoneGlow", 18, 18);

            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeSmoothTiles(Type);
            CalamityUtils.MergeDecorativeTiles(Type);
            CalamityUtils.MergeWithAbyss(Type);

            HitSound = SoundID.Tink;
            MineResist = 2.1f;
            AddMapEntry(new Color(27, 24, 31));
        }

        public override void Unload()
        {
            GlowMask?.Unload();
            GlowMask = null;
        }

        int animationFrameWidth = 288;

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.DungeonSpirit, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            return false;
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameXOffset = animationFrameWidth * TileFraming.GetVariation4x4_01_Low0(i, j);
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            var tileCache = Main.tile[i, j];
            int xPos = tileCache.TileFrameX;
            int yPos = tileCache.TileFrameY;
            int xOffset = animationFrameWidth * TileFraming.GetVariation4x4_01_Low0(i, j);
            xPos += xOffset;

            if (GlowMask.HasContentInFramePos(xPos, yPos))
            {
                Color drawColour = GetDrawColour(i, j, new Color(255, 255, 255, 255));
                float brightness = 1f;
                float declareThisHereToPreventRunningTheSameCalculationMultipleTimes = Main.GameUpdateCount * 0.007f;
                brightness *= (float)MathF.Sin(i / 18f + declareThisHereToPreventRunningTheSameCalculationMultipleTimes);
                brightness *= (float)MathF.Sin(j / 18f + declareThisHereToPreventRunningTheSameCalculationMultipleTimes);
                brightness *= (float)MathF.Sin(i * 18f + declareThisHereToPreventRunningTheSameCalculationMultipleTimes);
                brightness *= (float)MathF.Sin(j * 18f + declareThisHereToPreventRunningTheSameCalculationMultipleTimes);
                drawColour *= brightness;

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
