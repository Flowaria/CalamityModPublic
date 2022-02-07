﻿using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.CalPlayer;
using CalamityMod.DataStructures;
using CalamityMod.Particles;
using CalamityMod.Items.Weapons.Melee;
using Terraria.Graphics.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static CalamityMod.CalamityUtils;


namespace CalamityMod.Projectiles.Melee
{
    public class SolarNeedle : ModProjectile 
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("SolarNeedle");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        const float MaxTime = 30;
        public float Timer => MaxTime - projectile.timeLeft;
        public ref float Empowered => ref projectile.ai[0];

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 32;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.timeLeft = (int)MaxTime;
            projectile.melee = true;
            projectile.tileCollide = false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            float bladeLenght = 44f * projectile.scale;
            Vector2 start = -Utils.SafeNormalize(projectile.velocity, Vector2.Zero) * 16f;

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center + start, projectile.Center + start + Utils.SafeNormalize(projectile.velocity, Vector2.Zero) * bladeLenght, 24, ref collisionPoint);
        }

        public override void AI()
        {
            projectile.scale = 2.4f;
            projectile.Opacity = 0.6f;
            Lighting.AddLight(projectile.Center, 0.75f, 1f, 0.24f);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            projectile.velocity *= (1 - (float)Math.Pow(Timer / MaxTime, 3));

            if (Main.rand.NextBool(3))
            {
                int dustTrail = Dust.NewDust(projectile.Center, 14, 14, 66, projectile.velocity.X * 0.05f, projectile.velocity.Y * 0.05f, 150, new Color(Main.DiscoR, 100, 255), 1.2f);
                Main.dust[dustTrail].noGravity = true;
            }

            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.Next(3);
                dustType = dustType == 0 ? 15 : dustType == 1 ? 57 : 58;

                Dust.NewDust(projectile.Center, 14, 14, dustType, projectile.velocity.X * 0.1f, projectile.velocity.Y * 0.1f, 150, default, 1.3f);
            }

            if (projectile.velocity.Length() < 1.0f)
                projectile.Kill();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.timeLeft > 35)
                return false;

            Texture2D texture = GetTexture("CalamityMod/Projectiles/Melee/SolarNeedle");

            if (Empowered == 1f)
            {
                CalamityUtils.EnterShaderRegion(spriteBatch);
                Color outlineColor = Color.Lerp(Color.White, Color.OrangeRed, Timer / MaxTime);
                Vector3 outlineHSL = Main.rgbToHsl(outlineColor); //BasicTint uses the opposite hue i guess? or smth is fucked with the way shaders get their colors. anyways, we invert it
                float outlineThickness = MathHelper.Clamp(Timer / MaxTime * 4f, 0f, 3f);

                GameShaders.Misc["CalamityMod:BasicTint"].UseOpacity(1f);
                GameShaders.Misc["CalamityMod:BasicTint"].UseColor(Main.hslToRgb(1 - outlineHSL.X, outlineHSL.Y, outlineHSL.Z));
                GameShaders.Misc["CalamityMod:BasicTint"].Apply();

                for (float i = 0; i < 1; i += 0.125f)
                {
                    spriteBatch.Draw(texture, projectile.Center + (i * MathHelper.TwoPi + projectile.rotation).ToRotationVector2() * outlineThickness - Main.screenPosition, null, outlineColor, projectile.rotation, texture.Size() / 2f, projectile.scale, 0f, 0f);
                }
                CalamityUtils.ExitShaderRegion(spriteBatch);
            }


            DrawAfterimagesCentered(projectile, ProjectileID.Sets.TrailingMode[projectile.type], lightColor, 1);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D starTexture = GetTexture("CalamityMod/Particles/Sparkle");
            Texture2D bloomTexture = GetTexture("CalamityMod/Particles/BloomCircle");
            //Ajust the bloom's texture to be the same size as the star's
            float properBloomSize = (float)starTexture.Height / (float)bloomTexture.Height;

            Color color = Main.hslToRgb((Main.GlobalTime * 0.6f) % 1, 1, 0.85f);
            float rotation = Main.GlobalTime * 8f;

            Vector2 sparkCenter = projectile.Center - Utils.SafeNormalize(projectile.velocity, Vector2.Zero) * 30.5f - Main.screenPosition;

            spriteBatch.Draw(bloomTexture, sparkCenter, null, color* 0.5f, 0, bloomTexture.Size() / 2f, 4 * properBloomSize, SpriteEffects.None, 0);
            spriteBatch.Draw(starTexture, sparkCenter, null, color * 0.5f, rotation + MathHelper.PiOver4, starTexture.Size() / 2f, 2 * 0.75f, SpriteEffects.None, 0);
            spriteBatch.Draw(starTexture, sparkCenter, null, Color.White, rotation, starTexture.Size() / 2f, 2, SpriteEffects.None, 0);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);


            return false;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.DD2_WitherBeastDeath, projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Vector2 particleSpeed = Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(1.2f, 2.3f);
                Particle energyLeak = new SquishyLightParticle(projectile.Center + Utils.SafeNormalize(projectile.velocity, Vector2.Zero) * 40f, particleSpeed, Main.rand.NextFloat(0.3f, 0.6f), Color.Cyan, 60, 1, 1.5f, hueShift: 0.02f);
                GeneralParticleHandler.SpawnParticle(energyLeak);
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Vector2 particleOrigin = target.Hitbox.Size().Length() < 140 ? target.Center : projectile.Center + projectile.rotation.ToRotationVector2() * 60f;
            for (int i = 0; i < 10; i++)
            {
                Vector2 particleSpeed = Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(2.6f, 4f);
                Particle energyLeak = new SquishyLightParticle(particleOrigin, particleSpeed, Main.rand.NextFloat(0.3f, 0.6f), Color.Cyan, 60, 1, 1.5f, hueShift: 0.02f);
                GeneralParticleHandler.SpawnParticle(energyLeak);
            }
        }
    }
}