using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FurnitureVoid
{
    public class VoidstoneSlab : ModTile
    {
        internal static FramedGlowMask GlowMask;

        public override void SetStaticDefaults()
        {
            GlowMask = new("CalamityMod/Tiles/FurnitureVoid/VoidstoneSlabGlow", 18, 18);

            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            HitSound = SoundID.Tink;
            MineResist = 7f;
            MinPick = 180;
            AddMapEntry(new Color(27, 24, 31));
            AnimationFrameHeight = 270;
        }

        public override void Unload()
        {
            GlowMask?.Unload();
            GlowMask = null;
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.DungeonSpirit, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            return false;
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameYOffset = AnimationFrameHeight * TileFraming.GetVariation3x3_01234_Low3(i, j);
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            var tile = Main.tile[i, j];
            int xPos = tile.TileFrameX;
            int yPos = tile.TileFrameY;
            int yOffset = AnimationFrameHeight * TileFraming.GetVariation3x3_01234_Low3(i, j);
            yPos += yOffset;

            if (GlowMask.HasContentInFramePos(xPos, yPos))
            {
                Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
                Vector2 drawOffset = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y) + zero;
                Color drawColour = GetDrawColour(i, j, new Color(75, 75, 75, 75));
                if (!tile.IsHalfBlock && tile.Slope == 0)
                {
                    Main.spriteBatch.Draw(GlowMask.Texture, drawOffset, new Rectangle?(new Rectangle(xPos, yPos, 18, 18)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                }
                else if (tile.IsHalfBlock)
                {
                    Main.spriteBatch.Draw(GlowMask.Texture, drawOffset + new Vector2(0f, 8f), new Rectangle?(new Rectangle(xPos, yPos, 18, 8)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                }
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
