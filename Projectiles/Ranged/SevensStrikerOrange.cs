﻿using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class SevensStrikerOrange : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Orange");
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 1;
            Projectile.timeLeft = 480;
        }

        public override void AI()
        {
            Projectile.rotation += 0.1f * Projectile.direction;
            Projectile.velocity.Y += 0.2f;
            if (Projectile.velocity.Y > 16f)
            {
                Projectile.velocity.Y = 16f;
            }
        }

        public override void Kill(int timeLeft)
        {
            Projectile.position = Projectile.Center;
            Projectile.width = Projectile.height = 48;
            Projectile.position.X = Projectile.position.X - (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (float)(Projectile.height / 2);
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            //Explosion copied over from another ranged orange explosive gravity-affected plant projectile
            for (int i = 0; i < 15; i++)
            {
                Particle smoke = new SmallSmokeParticle(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), Vector2.Zero, Color.Orange, new Color(40, 40, 40), Main.rand.NextFloat(0.8f, 1.6f), 145 - Main.rand.Next(30));
                smoke.Velocity = (smoke.Position - Projectile.Center) * 0.2f + Projectile.velocity;
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }
    }
}