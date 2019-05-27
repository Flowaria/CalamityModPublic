using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.RareVariants
{
	public class Carnage : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Carnage");
			Tooltip.SetDefault("Enemies explode into homing blood on death");
		}

		public override void SetDefaults()
		{
			item.width = 46;
			item.damage = 55;
			item.melee = true;
			item.useAnimation = 21;
			item.useStyle = 1;
			item.useTime = 21;
			item.knockBack = 5.25f;
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;
			item.useTurn = true;
			item.height = 60;
			item.value = Item.buyPrice(0, 4, 0, 0);
			item.rare = 3;
			item.GetGlobalItem<CalamityGlobalItem>(mod).postMoonLordRarity = 22;
		}

		public override void MeleeEffects(Player player, Rectangle hitbox)
		{
			if (Main.rand.Next(3) == 0)
			{
				int dust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 5);
			}
		}

		public override void OnHitNPC(Player player, NPC target, int damage, float knockback, bool crit)
		{
			if (target.life <= 0)
			{
				Main.PlaySound(2, (int)target.position.X, (int)target.position.Y, 74);
				target.position.X = target.position.X + (float)(target.width / 2);
				target.position.Y = target.position.Y + (float)(target.height / 2);
				target.position.X = target.position.X - (float)(target.width / 2);
				target.position.Y = target.position.Y - (float)(target.height / 2);
				for (int num621 = 0; num621 < 15; num621++)
				{
					int num622 = Dust.NewDust(new Vector2(target.position.X, target.position.Y), target.width, target.height, 5, 0f, 0f, 100, default(Color), 2f);
					Main.dust[num622].velocity *= 3f;
					if (Main.rand.Next(2) == 0)
					{
						Main.dust[num622].scale = 0.5f;
						Main.dust[num622].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
					}
				}
				for (int num623 = 0; num623 < 25; num623++)
				{
					int num624 = Dust.NewDust(new Vector2(target.position.X, target.position.Y), target.width, target.height, 5, 0f, 0f, 100, default(Color), 3f);
					Main.dust[num624].noGravity = true;
					Main.dust[num624].velocity *= 5f;
					num624 = Dust.NewDust(new Vector2(target.position.X, target.position.Y), target.width, target.height, 5, 0f, 0f, 100, default(Color), 2f);
					Main.dust[num624].velocity *= 2f;
				}
				int num251 = Main.rand.Next(4, 6);
				for (int num252 = 0; num252 < num251; num252++)
				{
					Vector2 value15 = new Vector2((float)Main.rand.Next(-100, 101), (float)Main.rand.Next(-100, 101));
					while (value15.X == 0f && value15.Y == 0f)
					{
						value15 = new Vector2((float)Main.rand.Next(-100, 101), (float)Main.rand.Next(-100, 101));
					}
					value15.Normalize();
					value15 *= (float)Main.rand.Next(70, 101) * 0.1f;
					Projectile.NewProjectile(target.Center.X, target.Center.Y, value15.X, value15.Y, mod.ProjectileType("Blood"), (int)((float)item.damage * player.meleeDamage), knockback, player.whoAmI, 0f, 0f);
				}
			}
		}
	}
}
