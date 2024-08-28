﻿using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Abyss.AbyssAmbient
{
    public class AbyssGiantKelp3 : GlowMaskTile
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/Abyss/AbyssAmbient/AbyssGiantKelp3Glow";

        public override void SetupStatic()
        {
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.Origin = new Point16(1, 3);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.WaterDeath = false;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(92, 93, 42));
            DustType = DustID.Grass;
            HitSound = SoundID.Grass;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.TileFrameY <= 36)
            {
                r = 0.46f;
                g = 0.22f;
                b = 0.05f;
            }
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (closer && Main.rand.NextBool(100) && j > Main.worldSurface)
            {
                Dust dust;
                dust = Main.dust[Dust.NewDust(new Vector2(i * 16f, j * 16f), 274, 279, DustID.Firefly, 0.23255825f, 10f, 0, new Color(117, 55, 15), 1.5116279f)];
                dust.noGravity = true;
                dust.noLight = true;
                dust.fadeIn = 2.5813954f;
            }
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 2;
        }

        public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
        {
            return Color.White;
        }
    }
}
