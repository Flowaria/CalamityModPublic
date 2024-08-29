using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Plates
{
    public class PlagueContainmentCells : GlowMaskTile
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/Plates/PlagueContainmentCellsGlow";

        public static readonly SoundStyle MinePlatingSound = new("CalamityMod/Sounds/Custom/PlatingMine", 3);
        internal static GradientTexture PulseGradient;
        public override void SetupStatic()
        {
            PulseGradient = new("CalamityMod/Tiles/Plates/PlagueContainmentCellsPulse");

            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);

            HitSound = MinePlatingSound;
            MineResist = 1f;
            AddMapEntry(new Color(128, 188, 67));
        }

        public override void OnUnload()
        {
            PulseGradient?.Unload();
            PulseGradient = null;
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            int dust = Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.GemEmerald, 0f, 0f, 100, default, 2f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity.Y = -0.15f;

            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Stone, 0f, 0f, 1, new Color(100, 100, 100), 1f);
            return false;
        }

        public override void RandomUpdate(int i, int j)
        {
            int dust = Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.GemEmerald, 0f, 0f, 100, default, 2f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity.Y = -0.15f;
        }

        public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
        {
            int factor = (int)Main.GameUpdateCount;
            float brightness = PulseGradient.GetColorRepeat(factor).R / 255f;
            int drawBrightness = (int)(40 * brightness) + 10;
            return Color.White * drawBrightness;
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
