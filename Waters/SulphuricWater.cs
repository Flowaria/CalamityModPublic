﻿using CalamityMod.Particles;
using System;
using CalamityMod.Systems;
using CalamityMod.Tiles.Abyss;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics;
using Terraria.ID;
using CalamityMod.Dusts.WaterSplash;
using CalamityMod.Gores.WaterDroplet;

namespace CalamityMod.Waters
{
    public class SulphuricWaterflow : ModWaterfallStyle { }

    public class SulphuricWater : CalamityModWaterStyle
    {
        public static CalamityModWaterStyle Instance { get; private set; }
        public static ModWaterfallStyle WaterfallStyle { get; private set; }
        public static int SplashDust { get; private set; }
        public static int DropletGore { get; private set; }
        public static Asset<Texture2D> RainTexture { get; private set; }

        private ushort _RustyChestTile;

        public override void SetStaticDefaults()
        {
            Instance = this;
            WaterfallStyle = ModContent.Find<ModWaterfallStyle>("CalamityMod/SulphuricWaterflow");
            SplashDust = ModContent.DustType<SulphuricSplash>();
            DropletGore = ModContent.GoreType<SulphuricWaterDroplet>();
            _RustyChestTile = (ushort)ModContent.TileType<RustyChestTile>();
        }

        public override void Unload()
        {
            Instance = null;
            WaterfallStyle = null;
            SplashDust = 0;
            DropletGore = 0;
            RainTexture = null;
            _RustyChestTile = 0;
        }

        public override int ChooseWaterfallStyle() => WaterfallStyle.Slot;
        public override int GetSplashDust() => SplashDust;
        public override int GetDropletGore() => DropletGore;
        public override Asset<Texture2D> GetRainTexture() => RainTexture ??= ModContent.Request<Texture2D>("CalamityMod/Waters/SulphuricRain");
        public override byte GetRainVariant() => (byte)Main.rand.Next(3);
        public override Color BiomeHairColor() => new Color(43, 168, 110);
        public override void DrawColor(int x, int y, ref VertexColors liquidColor, bool isSlope) => ILEditing.ILChanges.SelectSulphuricWaterColor(x, y, ref liquidColor, isSlope);
        public override void ModifyLight(ref readonly Tile tile, int i, int j, ref float r, ref float g, ref float b)
        {
            Vector3 outputColor = new Vector3(r, g, b);
            if (outputColor == Vector3.One || outputColor == new Vector3(0.25f, 0.25f, 0.25f) || outputColor == new Vector3(0.5f, 0.5f, 0.5f))
                return;
            Tile above = CalamityUtils.ParanoidTileRetrieval(i, j - 1);
            if (!Main.gamePaused && !above.HasTile && above.LiquidAmount <= 0 && Main.rand.NextBool(9))
            {
                MediumMistParticle acidFoam = new(new(i * 16f + Main.rand.NextFloat(16f), j * 16f + 8f), -Vector2.UnitY.RotatedByRandom(0.67f) * Main.rand.NextFloat(1f, 2.4f), Color.LightSeaGreen, Color.White, 0.16f, 128f, 0.02f);
                GeneralParticleHandler.SpawnParticle(acidFoam);
            }

            if (tile.TileType != _RustyChestTile)
            {
                if (Main.dayTime && !Main.raining)
                {
                    float brightness = MathHelper.Clamp(0.2f - (j / 680), 0f, 0.2f);
                    if (j > 580)
                        brightness *= 1f - (j - 580) / 100f;

                    float time = Main.GameUpdateCount;
                    float waveScale1 = time * 0.014f;
                    float waveScale2 = time * 0.1f;
                    int scalar = i + (-j / 2);
                    float wave1 = waveScale1 * -50 + scalar * 15;
                    float wave2 = waveScale2 * -10 + scalar * 14;
                    float wave3 = waveScale1 * -100 + scalar * 13;
                    float wave4 = waveScale2 * 10 + scalar * 25;
                    float wave5 = waveScale1 * -70 + scalar * 5;
                    float wave1angle = 0.55f + 0.45f * MathF.Sin(MathHelper.ToRadians(wave1));
                    float wave2angle = 0.55f + 0.45f * MathF.Sin(MathHelper.ToRadians(wave2));
                    float wave3angle = 0.55f + 0.45f * MathF.Sin(MathHelper.ToRadians(wave3));
                    float wave4angle = 0.55f + 0.45f * MathF.Sin(MathHelper.ToRadians(wave4));
                    float wave5angle = 0.55f + 0.45f * MathF.Sin(MathHelper.ToRadians(wave5));
                    outputColor = Vector3.Lerp(outputColor, Color.LightSeaGreen.ToVector3(), 0.41f + wave1angle + wave2angle + wave3angle + wave4angle + wave5angle);
                    outputColor *= brightness;
                }

                if (!Main.dayTime && !Main.raining)
                {
                    float brightness = MathHelper.Clamp(0.17f - (j / 680), 0f, 0.17f);
                    if (j > 580)
                        brightness *= 1f - (j - 580) / 100f;

                    float waveScale1 = Main.GameUpdateCount * 0.014f;
                    float waveScale2 = Main.GameUpdateCount * 0.1f;
                    int scalar = i + (-j / 2);
                    float wave1 = waveScale1 * -50 + scalar * 15;
                    float wave2 = waveScale2 * -10 + scalar * 14;
                    float wave3 = waveScale1 * -100 + scalar * 13;
                    float wave4 = waveScale2 * 10 + scalar * 25;
                    float wave5 = waveScale1 * -70 + scalar * 5;
                    float wave1angle = 0.55f + 0.45f * (float)Math.Sin(MathHelper.ToRadians(wave1));
                    float wave2angle = 0.55f + 0.45f * (float)Math.Sin(MathHelper.ToRadians(wave2));
                    float wave3angle = 0.55f + 0.45f * (float)Math.Sin(MathHelper.ToRadians(wave3));
                    float wave4angle = 0.55f + 0.45f * (float)Math.Sin(MathHelper.ToRadians(wave4));
                    float wave5angle = 0.55f + 0.45f * (float)Math.Sin(MathHelper.ToRadians(wave5));
                    outputColor = Vector3.Lerp(outputColor, Color.LightSeaGreen.ToVector3(), 0.41f + wave1angle + wave2angle + wave3angle + wave4angle + wave5angle);
                    outputColor *= brightness;
                }

                if (Main.raining)
                {
                    float brightness = MathHelper.Clamp(1f - (j / 680), 0f, 1f);
                    if (j > 580)
                        brightness *= 1f - (j - 580) / 100f;

                    outputColor = Vector3.Lerp(outputColor, Color.LightSeaGreen.ToVector3(), 0.41f);
                    outputColor *= brightness;
                }
            }
            r = outputColor.X;
            g = outputColor.Y;
            b = outputColor.Z;
        }
    }
}
