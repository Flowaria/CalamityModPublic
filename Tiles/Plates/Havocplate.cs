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
    [LegacyName("Chaosplate")]
    public class Havocplate : GlowMaskTile
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/Plates/HavocplateGlow";

        public static readonly SoundStyle MinePlatingSound = new("CalamityMod/Sounds/Custom/PlatingMine", 3);

        internal static GradientTexture PulseGradient;

        public override void SetupStatic()
        {
            PulseGradient = new("CalamityMod/Tiles/Plates/HavocplatePulse");

            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);

            HitSound = MinePlatingSound;
            MineResist = 1f;
            AddMapEntry(new Color(235, 108, 108));
        }

        public override void OnUnload()
        {
            PulseGradient?.Unload();
            PulseGradient = null;
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.RedTorch, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Stone, 0f, 0f, 1, new Color(100, 100, 100), 1f);
            return false;
        }

        public override void RandomUpdate(int i, int j)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.RedTorch, 0f, 0f, 1, new Color(255, 255, 255), 1f);
        }

        public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
        {
            float brightness = PulseGradient.GetColorRepeat((int)Main.GameUpdateCount).R / 255f;
            brightness = 0.04f + (brightness * 0.156f);
            return Color.White * brightness;
        }
    }
}
