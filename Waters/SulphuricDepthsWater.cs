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
    public class SulphuricDepthsWaterflow : ModWaterfallStyle { }

    public class SulphuricDepthsWater : CalamityModWaterStyle
    {
        public static CalamityModWaterStyle Instance { get; private set; }
        public static ModWaterfallStyle WaterfallStyle { get; private set; }
        public static int SplashDust { get; private set; }
        public static int DropletGore { get; private set; }

        private ushort _RustyChestTile;

        public override void SetStaticDefaults()
        {
            Instance = this;
            WaterfallStyle = ModContent.Find<ModWaterfallStyle>("CalamityMod/SulphuricDepthsWaterflow");
            SplashDust = ModContent.DustType<SulphuricDepthsSplash>();
            DropletGore = ModContent.GoreType<SulphuricDepthsWaterDroplet>();
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
        public override Color BiomeHairColor() => new Color(35, 117, 89);
        public override void DrawColor(int x, int y, ref VertexColors liquidColor, bool isSlope) => ILEditing.ILChanges.SelectSulphuricWaterColor(x, y, ref liquidColor, isSlope);
        public override void ModifyLight(ref readonly Tile tile, int i, int j, ref float r, ref float g, ref float b)
        {
            Vector3 outputColor = new Vector3(r, g, b);
            if (outputColor == Vector3.One || outputColor == new Vector3(0.25f, 0.25f, 0.25f) || outputColor == new Vector3(0.5f, 0.5f, 0.5f))
                return;
            if (tile.TileType != _RustyChestTile)
            {
                outputColor = Vector3.Lerp(outputColor, Color.MediumSeaGreen.ToVector3(), 0.18f);
            }
            r = outputColor.X;
            g = outputColor.Y;
            b = outputColor.Z;
        }
    }
}
