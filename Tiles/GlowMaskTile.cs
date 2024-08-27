using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CalamityMod.Tiles
{
    public abstract class GlowMaskTile : ModTile
    {
        public FramedGlowMask GlowMask;

        public abstract string GlowMaskAsset { get; }

        public override void SetStaticDefaults()
        {
            GlowMask = new(GlowMaskAsset, 18, 18);
        }

        public override void Unload()
        {
            GlowMask?.Unload();
            GlowMask = null;
        }

        public abstract float GetGlowMaskBrightness(int i, int j, TileDrawInfo drawData);
    }
}
