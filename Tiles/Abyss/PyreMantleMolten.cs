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
    public class PyreMantleMolten : GlowMaskTile
    {
        public static readonly SoundStyle MineSound = new("CalamityMod/Sounds/Custom/VoidstoneMine", 3) { Volume = 0.4f };

        public override string GlowMaskAsset => "CalamityMod/Tiles/Abyss/PyreMantleMolten_Glowmask";

        public override void SetupStatic()
        {
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileBrick[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithAbyss(Type);

            TileID.Sets.ChecksForMerge[Type] = true;
            HitSound = MineSound;
            MineResist = 10f;
            MinPick = 180;
            AddMapEntry(new Color(113, 49, 16));

            this.RegisterUniversalMerge(TileID.Dirt, "CalamityMod/Tiles/Merges/DirtMerge");
            this.RegisterUniversalMerge(TileID.Stone, "CalamityMod/Tiles/Merges/StoneMerge");
            this.RegisterUniversalMerge(ModContent.TileType<AbyssGravel>(), "CalamityMod/Tiles/Merges/AbyssGravelMerge");
            this.RegisterUniversalMerge(ModContent.TileType<PyreMantle>(), "CalamityMod/Tiles/Merges/PyreMantleMerge");
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.HeatRay, 0f, 0f, 1, new Color(128, 128, 128), 1f);
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

            // Place PhoviamareHalm
            if (WorldGen.genRand.NextBool(12) && !up.HasTile && !up2.HasTile && up.LiquidAmount > 0 && up2.LiquidAmount > 0 && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
            {
                up.TileType = (ushort)ModContent.TileType<PhoviamareHalm>();
                up.HasTile = true;
                up.TileFrameY = 0;

                // 16 different frames, choose a random one
                up.TileFrameX = (short)(WorldGen.genRand.Next(16) * 18);
                WorldGen.SquareTileFrame(i, j - 1, true);

                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendTileSquare(-1, i, j - 1, 3, TileChangeType.None);
            }
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            float brightness = 0.7f;
            float lightspeed = (float)(Main.timeForVisualEffects * 0.01);
            brightness *= (float)MathF.Sin(i / 60f + lightspeed);
            brightness += 0.3f;
            r = 1f;
            g = 0.33f;
            b = 0f;
            r *= brightness;
            g *= brightness;
            b *= brightness;
        }

        public override float GetGlowMaskBrightness(int i, int j, TileDrawInfo drawData)
        {
            float glowbrightness = 1f;
            float glowspeed = (float)(Main.timeForVisualEffects * 0.01);
            glowbrightness *= (float)MathF.Sin(i / 60f + glowspeed);
            return glowbrightness;
        }
    }
}
