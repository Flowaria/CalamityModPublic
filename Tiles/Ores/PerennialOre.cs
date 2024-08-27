﻿
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Ores
{
    public class PerennialOre : GlowMaskTile
    {
        public override void SetupStatic()
        {
            GlowMask = new("CalamityMod/Tiles/Ores/PerennialOreGlow", 18, 18);

            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileOreFinderPriority[Type] = 710;
            Main.tileShine[Type] = 2500;
            Main.tileShine2[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);

            TileID.Sets.Ore[Type] = true;
            TileID.Sets.OreMergesWithMud[Type] = true;

            AddMapEntry(new Color(200, 250, 100), CreateMapEntryName());
            MineResist = 2f;
            MinPick = 200;
            HitSound = SoundID.Tink;
            Main.tileSpelunker[Type] = true;

            this.RegisterUniversalMerge(TileID.Dirt, "CalamityMod/Tiles/Merges/DirtMerge");
            this.RegisterUniversalMerge(TileID.Stone, "CalamityMod/Tiles/Merges/StoneMerge");
            this.RegisterUniversalMerge(TileID.Mud, "CalamityMod/Tiles/Merges/MudMerge");
        }

        int animationFrameWidth = 234;

        public override string GlowMaskAsset => throw new NotImplementedException();

        public override bool CanExplode(int i, int j)
        {
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            // The base green color glow
            r = 0.08f;
            g = 0.2f;
            b = 0.04f;
            var tile = Main.tile[i, j];
            var pos = new Vector2(tile.TileFrameX, tile.TileFrameY);

            Vector2[] positionsFlower = new Vector2[]
            {
                // Top row (always y = 0 on tile sheets)
                new Vector2(0, 0),
                new Vector2(36, 0),

                // Second row (always y = 18 on tile sheets)
                new Vector2(18, 18),
                new Vector2(54, 18),

                // Third row (always y = 36 on tile sheets)
                new Vector2(36, 36),

            };

            foreach (var positionFlower in positionsFlower)
            {
                if (pos == positionFlower)
                {
                    float timeScalar = Main.GameUpdateCount * 0.017f;
                    float jDiv14 = j / 14f;
                    float iDiv14 = i / 14f;
                    float brightness = 0.7f;
                    brightness *= (float)MathF.Sin(jDiv14 + timeScalar);
                    brightness *= (float)MathF.Sin(iDiv14 + timeScalar);
                    brightness += 0.3f;
                    float flowerPosBrightnessR = 0.83f * brightness;
                    float flowerPosBrightnessG = 0.16f * brightness;
                    float flowerPosBrightnessB = 0.31f * brightness;

                    // Adjust brightness for flowers
                    r = flowerPosBrightnessR;
                    g = flowerPosBrightnessG;
                    b = flowerPosBrightnessB;
                }
            }
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameXOffset = animationFrameWidth * TileFraming.GetVariation4x4_012_Low0(i, j);
        }

        public override float GetGlowMaskBrightness(int i, int j, TileDrawInfo drawData)
        {
            return 0.686f;
        }
    }
}
