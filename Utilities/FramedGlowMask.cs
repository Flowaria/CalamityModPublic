using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod
{
    public sealed class FramedGlowMask
    {
        /// <summary>
        /// Cached Texture2D reference, null on server
        /// </summary>
        public Texture2D Texture; // Leave this as field for performances sake
        
        /// <summary>
        /// X axis's frame count, 0 on server
        /// </summary>
        public int FrameXCount => _FrameXCount;

        /// <summary>
        /// Y axis's frame count, 0 on server
        /// </summary>
        public int FrameYCount => _FrameYCount;

        /// <summary>
        /// Pixel width for each frame
        /// </summary>
        public int FrameWidth => _FrameWidth;

        /// <summary>
        /// Pixel height for each frame
        /// </summary>
        public int FrameHeight => _FrameHeight;

        private int _FrameWidth, _FrameHeight, _FrameXCount, _FrameYCount;
        private readonly bool[,] _HasGlowContent;

        public FramedGlowMask(string asset, int frameWidth, int frameHeight, bool pretendEveryFrameHaveGlow = false)
        {
            _FrameWidth = frameWidth;
            _FrameHeight = frameHeight;

            // Don't do anything further on server
            if (Main.dedServ)
                return;

            Texture = ModContent.Request<Texture2D>(asset, AssetRequestMode.ImmediateLoad).Value;
            if (Texture is null)
                return;

            _FrameXCount = Texture.Width / frameWidth;
            _FrameYCount = Texture.Height / frameHeight;

            _HasGlowContent = new bool[FrameXCount, FrameYCount];

            
            if (pretendEveryFrameHaveGlow)
            {
                for(int x = 0; x<FrameXCount; x++)
                {
                    for (int y = 0; y<FrameYCount; y++)
                    {
                        _HasGlowContent[x, y] = true;
                    }
                }
            }
            else
            {
                Main.QueueMainThreadAction(() =>
                {
                    var colData = Texture.GetColorsFromTexture();
                    Parallel.For(0, FrameXCount * FrameYCount, (i) =>
                    {
                        int xFrame = i % FrameXCount;
                        int yFrame = i / FrameXCount;

                        int xStart = xFrame * frameWidth;
                        int xEnd = xStart + frameWidth;

                        int yStart = yFrame * frameHeight;
                        int yEnd = yStart + frameHeight;

                        bool frameHasData = false;
                        for (int x = xStart; x < xEnd; x++)
                        {
                            if (frameHasData)
                            {
                                break;
                            }

                            for (int y = yStart; y < yEnd; y++)
                            {
                                Color col = colData[x, y];
                                if (col.A >= 1)
                                {
                                    frameHasData = true;
                                    break;
                                }
                            }
                        }

                        _HasGlowContent[xFrame, yFrame] = frameHasData;
                    });
                });
            }
        }

        public void Unload()
        {
            Texture = null;
            _FrameWidth = 0;
            _FrameHeight = 0;
            _FrameXCount = 0;
            _FrameYCount = 0;
        }

        public bool HasContentInFrameIndex(int xFrame, int yFrame)
        {
            if (Texture is null)
                return false;

            if (xFrame < 0 || xFrame >= _FrameXCount)
                return false;

            if (yFrame < 0 || yFrame >= _FrameYCount)
                return false;

            return _HasGlowContent[xFrame, yFrame];
        }

        public bool HasContentInFramePos(int xPos, int yPos)
        {
            if (Texture is null)
                return false;

            int xFrame = xPos / _FrameWidth;
            int yFrame = yPos / _FrameHeight;

            if (xFrame < 0 || xFrame >= _FrameXCount)
                return false;

            if (yFrame < 0 || yFrame >= _FrameYCount)
                return false;

            return _HasGlowContent[xFrame, yFrame];
        }
    }
}
