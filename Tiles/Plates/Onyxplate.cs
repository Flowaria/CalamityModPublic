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
    public class Onyxplate : GlowMaskTile
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/Plates/OnyxplateGlow";

        public static readonly SoundStyle MinePlatingSound = new("CalamityMod/Sounds/Custom/PlatingMine", 3);
        internal static GradientTexture PulseGradient;
        public override void SetupStatic()
        {
            PulseGradient = new("CalamityMod/Tiles/Plates/OnyxplatePulse");

            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);

            HitSound = MinePlatingSound;
            MineResist = 1f;
            DustType = 173;
            AddMapEntry(new Color(182, 28, 232));
        }

        public override void OnUnload()
        {
            PulseGradient?.Unload();
            PulseGradient = null;
        }

        public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
        {
            int factor = (int)Main.GameUpdateCount;
            float brightness = PulseGradient.GetColorRepeat(factor).R / 255f;
            int drawBrightness = (int)(80 * brightness) + 10;
            return Color.White * drawBrightness;
        }
    }
}
