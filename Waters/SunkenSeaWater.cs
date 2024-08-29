﻿using CalamityMod.Particles;
using System;
using CalamityMod.Systems;
using CalamityMod.Tiles.Abyss;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics;
using CalamityMod.Dusts.WaterSplash;
using CalamityMod.Gores.WaterDroplet;

namespace CalamityMod.Waters
{
    public class SunkenSeaWaterflow : ModWaterfallStyle { }

    public class SunkenSeaWater : CalamityModWaterStyle
    {
        public static CalamityModWaterStyle Instance { get; private set; }
        public static ModWaterfallStyle WaterfallStyle { get; private set; }
        public static int SplashDust { get; private set; }
        public static int DropletGore { get; private set; }

        private ushort _RustyChestTile = 0;

        public override void SetStaticDefaults()
        {
            Instance = this;
            WaterfallStyle = ModContent.Find<ModWaterfallStyle>("CalamityMod/SunkenSeaWaterflow");
            SplashDust = ModContent.DustType<SunkenSeaSplash>();
            DropletGore = ModContent.GoreType<SunkenSeaWaterDroplet>();
            _RustyChestTile = (ushort)ModContent.TileType<RustyChestTile>();
        }

        public override void Unload()
        {
            Instance = null;
            WaterfallStyle = null;
            SplashDust = 0;
            DropletGore = 0;
            _RustyChestTile = 0;
        }

        public override int ChooseWaterfallStyle() => WaterfallStyle.Slot;
        public override int GetSplashDust() => SplashDust;
        public override int GetDropletGore() => DropletGore;
        public override Color BiomeHairColor() => new Color(46, 155, 171);
        public override void DrawColor(int x, int y, ref VertexColors liquidColor, bool isSlope) => ILEditing.ILChanges.SelectSulphuricWaterColor(x, y, ref liquidColor, isSlope);
        public override void ModifyLight(ref readonly Tile tile, int i, int j, ref float r, ref float g, ref float b)
        {
            Vector3 outputColor = new Vector3(r, g, b);
            if (outputColor == Vector3.One || outputColor == new Vector3(0.25f, 0.25f, 0.25f) || outputColor == new Vector3(0.5f, 0.5f, 0.5f))
                return;

            if (tile.TileType != _RustyChestTile)
            {
                float time = Main.GameUpdateCount;
                float brightness = MathHelper.Clamp(0.07f, 0f, 0.07f);
                float waveScale1 = time * 0.028f;
                float waveScale2 = time * 0.1f;
                int yScale = -j / 2;
                int xScale = i / 15;
                float wave1 = time * 0.024f * -50 + ((-i / 30) + (j / 30)) * 25;
                float wave2 = waveScale2 * -10 + ((-xScale) + yScale) * 45;
                float wave3 = waveScale1 * -100 + ((i / 7) + (j / 50)) * 25;
                float wave4 = time * 0.15f * 10 + ((i / 3) + yScale) * 45;
                float wave5 = waveScale1 * -70 + ((-i / 25) + (-j / 25)) * 20;
                float wave6 = waveScale2 * -10 + (xScale + yScale) * 45;
                float bigwave = time * 0.01f * -70 + ((-i / 2) + (-j / 40)) * 5;
                float wave1angle = 0.55f + 0.45f * MathF.Sin(MathHelper.ToRadians(wave1));
                float wave2angle = 0.55f + 0.45f * MathF.Sin(MathHelper.ToRadians(wave2));
                float wave3angle = 0.55f + 0.45f * MathF.Sin(MathHelper.ToRadians(wave3));
                float wave4angle = 0.55f + 0.45f * MathF.Sin(MathHelper.ToRadians(wave4));
                float wave5angle = 0.55f + 0.45f * MathF.Sin(MathHelper.ToRadians(wave5));
                float wave6angle = 0.55f + 0.45f * MathF.Sin(MathHelper.ToRadians(wave6));
                float bigwaveangle = 0.55f + 0.80f * MathF.Sin(MathHelper.ToRadians(bigwave));
                outputColor = Vector3.Lerp(outputColor, Color.DeepSkyBlue.ToVector3(), 0.07f + wave1angle + wave2angle + wave3angle + wave4angle + wave5angle + wave6angle + bigwaveangle);
                outputColor *= brightness;
            }

            r = outputColor.X;
            g = outputColor.Y;
            b = outputColor.Z;
        }
    }
}
