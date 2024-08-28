﻿using System;
using System.Collections.Generic;
using CalamityMod.Tiles.Abyss.AbyssAmbient;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Abyss
{
    public class Voidstone : GlowMaskTile
    {
        public static readonly SoundStyle MineSound = new("CalamityMod/Sounds/Custom/VoidstoneMine", 3) { Volume = 0.4f };

        public override string GlowMaskAsset => "CalamityMod/Tiles/Abyss/Voidstone_Glowmask";

        public override void SetupStatic()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileBrick[Type] = true; 

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithAbyss(Type);

            TileID.Sets.ChecksForMerge[Type] = true;
            HitSound = MineSound;
            MineResist = 15f;
            MinPick = 180;
            AddMapEntry(new Color(15, 15, 15));

            this.RegisterUniversalMerge(TileID.Dirt, "CalamityMod/Tiles/Merges/DirtMerge");
            this.RegisterUniversalMerge(TileID.Stone, "CalamityMod/Tiles/Merges/StoneMerge");
            this.RegisterUniversalMerge(ModContent.TileType<PyreMantle>(), "CalamityMod/Tiles/Merges/PyreMantleMerge");
        }

        int animationFrameWidth = 234;

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.DungeonSpirit, 0f, 0f, 1, new Color(128, 128, 128), 1f);
            return false;
        }

        public override bool CanExplode(int i, int j)
        {
            return false;
        }

        public override void RandomUpdate(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            Tile up = Main.tile[i, j - 1];
            Tile up2 = Main.tile[i, j - 2];

            // Place Tenebris
            if (WorldGen.genRand.NextBool(12) && !up.HasTile && !up2.HasTile && up.LiquidAmount > 0 && up2.LiquidAmount > 0 && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
            {
                up.TileType = (ushort)ModContent.TileType<TenebrisRemnant>();
                up.HasTile = true;
                up.TileFrameY = 0;

                // 6 different frames, choose a random one
                up.TileFrameX = (short)(WorldGen.genRand.Next(6) * 18);
                WorldGen.SquareTileFrame(i, j - 1, true);

                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendTileSquare(-1, i, j - 1, 3, TileChangeType.None);
            }
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameXOffset = animationFrameWidth * TileFraming.GetVariation4x4_012_Low0(i, j);
        }

        public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
        {
            float brightness = 1f;
            float timeFactor = (float)(Main.timeForVisualEffects * 0.007);
            brightness *= (float)MathF.Sin(i / 18f + timeFactor);
            brightness *= (float)MathF.Sin(j / 18f + timeFactor);
            brightness *= (float)MathF.Sin(i * 18f + timeFactor);
            brightness *= (float)MathF.Sin(j * 18f + timeFactor);
            //brightness *= 0.95f;
            return Color.White * brightness;
        }
    }
}
