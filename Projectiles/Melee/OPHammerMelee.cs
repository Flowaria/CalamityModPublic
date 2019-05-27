﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class OPHammerMelee : ModProjectile
    {
    	public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Hammer");
		}
    	
        public override void SetDefaults()
        {
            projectile.width = 62;
            projectile.height = 62;
            projectile.friendly = true;
            projectile.melee = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.extraUpdates = 2;
        }
        
        public override void AI()
        {
        	Lighting.AddLight(projectile.Center, ((255 - projectile.alpha) * 0.35f) / 255f, ((255 - projectile.alpha) * 0.35f) / 255f, ((255 - projectile.alpha) * 0f) / 255f);
        	if (projectile.soundDelay == 0)
			{
				projectile.soundDelay = 8;
				Main.PlaySound(SoundID.Item7, projectile.position);
			}
        	if (projectile.ai[0] == 0f)
			{
				projectile.ai[1] += 1f;
				if (projectile.ai[1] >= 20f)
				{
					projectile.ai[0] = 1f;
					projectile.ai[1] = 0f;
					projectile.netUpdate = true;
				}
        	}
        	else
			{
				projectile.tileCollide = false;
				float num42 = 16f;
				float num43 = 3.2f;
				Vector2 vector2 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
				float num44 = Main.player[projectile.owner].position.X + (float)(Main.player[projectile.owner].width / 2) - vector2.X;
				float num45 = Main.player[projectile.owner].position.Y + (float)(Main.player[projectile.owner].height / 2) - vector2.Y;
				float num46 = (float)Math.Sqrt((double)(num44 * num44 + num45 * num45));
				if (num46 > 3000f)
				{
					projectile.Kill();
				}
				num46 = num42 / num46;
				num44 *= num46;
				num45 *= num46;
				if (projectile.velocity.X < num44)
				{
					projectile.velocity.X = projectile.velocity.X + num43;
					if (projectile.velocity.X < 0f && num44 > 0f)
					{
						projectile.velocity.X = projectile.velocity.X + num43;
					}
				}
				else if (projectile.velocity.X > num44)
				{
					projectile.velocity.X = projectile.velocity.X - num43;
					if (projectile.velocity.X > 0f && num44 < 0f)
					{
						projectile.velocity.X = projectile.velocity.X - num43;
					}
				}
				if (projectile.velocity.Y < num45)
				{
					projectile.velocity.Y = projectile.velocity.Y + num43;
					if (projectile.velocity.Y < 0f && num45 > 0f)
					{
						projectile.velocity.Y = projectile.velocity.Y + num43;
					}
				}
				else if (projectile.velocity.Y > num45)
				{
					projectile.velocity.Y = projectile.velocity.Y - num43;
					if (projectile.velocity.Y > 0f && num45 < 0f)
					{
						projectile.velocity.Y = projectile.velocity.Y - num43;
					}
				}
				if (Main.myPlayer == projectile.owner)
				{
					Rectangle rectangle = new Rectangle((int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height);
					Rectangle value2 = new Rectangle((int)Main.player[projectile.owner].position.X, (int)Main.player[projectile.owner].position.Y, Main.player[projectile.owner].width, Main.player[projectile.owner].height);
					if (rectangle.Intersects(value2))
					{
						projectile.Kill();
					}
				}
        	}
        	projectile.rotation += 0.5f;
			return;
        }
        
        public override Color? GetAlpha(Color lightColor)
        {
        	return new Color(250, 250, 250, 50);
        }
        
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
        	target.AddBuff(mod.BuffType("BrimstoneFlames"), 300);
        	if (projectile.owner == Main.myPlayer)
        	{
        		Projectile.NewProjectile(projectile.Center.X, projectile.Center.Y, 0f, 0f, 612, projectile.damage, projectile.knockBack, projectile.owner, 0f, 0f);
        	}
        }
    }
}