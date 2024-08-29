﻿
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
    public class Elumplate : GlowMaskTile
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/Plates/ElumplateGlow";

        public static readonly SoundStyle MinePlatingSound = new("CalamityMod/Sounds/Custom/PlatingMine", 3);
        internal static GradientTexture PulseGradient;

        public override void SetupStatic()
        {
            PulseGradient = new("CalamityMod/Tiles/Plates/ElumplatePulse");

            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);

            HitSound = MinePlatingSound;
            MineResist = 1f;
            AddMapEntry(new Color(121, 180, 212));
        }
        public override void OnUnload()
        {
            PulseGradient?.Unload();
            PulseGradient = null;
        }


        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.PlatinumCoin, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Stone, 0f, 0f, 1, new Color(100, 100, 100), 1f);
            return false;
        }

        public override void RandomUpdate(int i, int j)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.PlatinumCoin, 0f, 0f, 1, new Color(255, 255, 255), 1f);
        }

        public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
        {
            int factor = (int)Main.GameUpdateCount;
            float brightness = PulseGradient.GetColorRepeat(factor).R / 255f;
            int drawBrightness = (int)(40 * brightness) + 10;
            return Color.White * drawBrightness;
        }
    }
}
