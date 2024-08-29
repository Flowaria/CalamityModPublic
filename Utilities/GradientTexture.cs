using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace CalamityMod
{
    /// <summary>
    /// Utility Class for using Gradient Texture (X-axis)
    /// </summary>
    public sealed class GradientTexture
    {
        private int _Width = 0;
        private Color[] _ColorCache;

        public GradientTexture(string assetName)
        {
            if (Main.dedServ)
                return;

            var texture = ModContent.Request<Texture2D>(assetName, AssetRequestMode.ImmediateLoad).Value;
            _ColorCache = new Color[texture.Width];

            Main.QueueMainThreadAction(() =>
            {
                texture.GetData(_ColorCache);
                _Width = texture.Width;
            });
        }

        public void Unload()
        {
            _Width = 0;
            _ColorCache = null;
        }

        public Color GetColorClamped(int x)
        {
            if (_Width == 0)
                return default;

            x = Math.Clamp(x, 0, _Width - 1);
            return _ColorCache[x];
        }

        public Color GetColorRepeat(int x)
        {
            if (_Width == 0)
                return default;

            x %= _Width;
            return _ColorCache[x];
        }
    }
}
