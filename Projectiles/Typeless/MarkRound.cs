﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria; using CalamityMod.Projectiles; using Terraria.ModLoader; using CalamityMod.Dusts;
using Terraria.ModLoader; using CalamityMod.Dusts; using CalamityMod.Buffs; using CalamityMod.Items; using CalamityMod.NPCs; using CalamityMod.Projectiles; using CalamityMod.Tiles; using CalamityMod.Walls;

namespace CalamityMod.Projectiles
{
    public class MarkRound : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Laser");
        }

        public override void SetDefaults()
        {
            projectile.width = 5;
            projectile.height = 5;
            projectile.friendly = true;
            projectile.alpha = 255;
            projectile.penetrate = 1;
            projectile.extraUpdates = 2;
            projectile.timeLeft = 300;
        }

        public override void AI()
        {
            if (projectile.alpha > 0)
            {
                projectile.alpha -= 25;
            }
            if (projectile.alpha < 0)
            {
                projectile.alpha = 0;
            }
            Lighting.AddLight((int)projectile.Center.X / 16, (int)projectile.Center.Y / 16, 0.5f, 0f, 0.7f);
            float num55 = 30f;
            float num56 = 2f;
            if (projectile.ai[1] == 0f)
            {
                projectile.localAI[0] += num56;
                if (projectile.localAI[0] > num55)
                {
                    projectile.localAI[0] = num55;
                }
            }
            else
            {
                projectile.localAI[0] -= num56;
                if (projectile.localAI[0] <= 0f)
                {
                    projectile.Kill();
                }
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(200, 0, 250, 0);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Color color25 = Lighting.GetColor((int)((double)projectile.position.X + (double)projectile.width * 0.5) / 16, (int)(((double)projectile.position.Y + (double)projectile.height * 0.5) / 16.0));
            int num147 = 0;
            int num148 = 0;
            float num149 = (float)(Main.projectileTexture[projectile.type].Width - projectile.width) * 0.5f + (float)projectile.width * 0.5f;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            Rectangle value6 = new Rectangle((int)Main.screenPosition.X - 500, (int)Main.screenPosition.Y - 500, Main.screenWidth + 1000, Main.screenHeight + 1000);
            if (projectile.getRect().Intersects(value6))
            {
                Vector2 value7 = new Vector2(projectile.position.X - Main.screenPosition.X + num149 + (float)num148, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY);
                float num162 = 30f;
                float scaleFactor = 2f;
                if (projectile.ai[1] == 1f)
                {
                    num162 = (float)(int)projectile.localAI[0];
                }
                for (int num163 = 1; num163 <= (int)projectile.localAI[0]; num163++)
                {
                    Vector2 value8 = Vector2.Normalize(projectile.velocity) * (float)num163 * scaleFactor;
                    Color color29 = projectile.GetAlpha(color25);
                    color29 *= (num162 - (float)num163) / num162;
                    color29.A = 0;
                    Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], value7 - value8, null, color29, projectile.rotation, new Vector2(num149, (float)(projectile.height / 2 + num147)), projectile.scale, spriteEffects, 0f);
                }
            }
            return false;
        }

        public override void Kill(int timeLeft)
        {
            int num212 = Main.rand.Next(3, 7);
            for (int num213 = 0; num213 < num212; num213++)
            {
                int num214 = Dust.NewDust(projectile.Center - projectile.velocity / 2f, 0, 0, 173, 0f, 0f, 100, default, 2.1f);
                Main.dust[num214].velocity *= 2f;
                Main.dust[num214].noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 300);
        }
    }
}
