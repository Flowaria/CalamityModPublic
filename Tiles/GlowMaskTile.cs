using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles
{
    public abstract class GlowMaskTile : ModTile
    {
        public enum PaintColorTint
        {
            OnlyByDeepPaint,
            ByEveryPaint,
            None,
        }

        public FramedGlowMask GlowMask;

        internal static GlowMaskTile[] InstanceLookup; // This Lookup is Array for performances sake

        public abstract string GlowMaskAsset { get; }

        public sealed override void SetStaticDefaults()
        {
            GlowMask = new(GlowMaskAsset, 18, 18);

            InstanceLookup ??= new GlowMaskTile[TileLoader.TileCount];
            InstanceLookup[Type] = this;

            SetupStatic();
        }

        public sealed override void Unload()
        {
            GlowMask?.Unload();
            GlowMask = null;

            InstanceLookup = null;

            OnUnload();
        }

        public virtual void SetupStatic() { }

        public virtual void OnUnload() { }

        public virtual PaintColorTint ColorTint => PaintColorTint.OnlyByDeepPaint;
        public virtual bool GlowMaskAffectedByOnlyDeepPaint => true;

        public abstract Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData);

        public static Color ApplyPaint(int paintType, Color color, bool deepPaintOnly = true)
        {
            if (deepPaintOnly && !IsDeepPaint(paintType))
                return color;

            Color paintCol = WorldGen.paintColor(paintType);
            color.R = (byte)(paintCol.R / 255f * color.R);
            color.G = (byte)(paintCol.G / 255f * color.G);
            color.B = (byte)(paintCol.B / 255f * color.B);

            return color;
        }

        private static bool IsDeepPaint(int paintType)
        {
            return PaintID.DeepRedPaint <= paintType && paintType <= PaintID.DeepPinkPaint;
        }
    }
}
