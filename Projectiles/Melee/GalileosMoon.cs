using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Buffs.DamageOverTime;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class GalileosMoon : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/Magic/Crescent";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crescent Moon");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.width = 50;
            projectile.height = 50;
            projectile.alpha = 100;
            projectile.friendly = true;
            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.penetrate = 10;
            projectile.timeLeft = 300;
            projectile.ignoreWater = true;
            projectile.usesLocalNPCImmunity = true;
            projectile.extraUpdates = 1;
            projectile.localNPCHitCooldown = projectile.MaxUpdates * 10;
        }

        public override void AI()
        {
            Lighting.AddLight(projectile.Center, 0f, 0f, 0.6f);
            if (projectile.soundDelay == 0)
            {
                projectile.soundDelay = 20 + Main.rand.Next(40);
                if (Main.rand.NextBool(5))
                {
                    Main.PlaySound(SoundID.Item9, projectile.position);
                }
            }
			projectile.rotation += projectile.direction * 0.55f;
            CalamityGlobalProjectile.HomeInOnNPC(projectile, true, 250f, 12f, 20f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(projectile, ProjectileID.Sets.TrailingMode[projectile.type], lightColor, 1);
            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<Nightwither>(), 180);
        }
    }
}
