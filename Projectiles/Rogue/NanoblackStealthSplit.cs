using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
	public class NanoblackStealthSplit : ModProjectile
	{

		public override string Texture => "CalamityMod/Items/Weapons/Rogue/NanoblackReaperRogue";

		private static int Lifetime = 300;
		private static float MaxSpeed = 40f;
		private static float MaxRotationSpeed = 0.25f;

		private static float HomingStartRange = 4000f;
		private static float HomingBreakRange = 6000f;
		private static float HomingBonusRangeCap = 2000f;
		private static float BaseHomingFactor = 2.6f;
		private static float MaxHomingFactor = 8.6f;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Nanoblack Afterimage");
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 7;
			ProjectileID.Sets.TrailingMode[projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			projectile.width = 56;
			projectile.height = 56;
			projectile.friendly = true;
			projectile.ignoreWater = true;
			projectile.tileCollide = false;
			projectile.penetrate = 12;
			projectile.extraUpdates = 1;
			projectile.timeLeft = Lifetime;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 8;
			projectile.Calamity().rogue = true;
		}

		// ai[0] = Index of current NPC target. If 0 or negative, the projectile has no target
		// ai[1] = Current spin speed. When it reaches 0, starts homing into enemies. Negative speeds are also allowed.
		public override void AI()
		{

			// Produces electricity and green firework sparks constantly while in flight. (Like the main projectile)
			if (Main.rand.NextBool(3))
			{
				int dustType = Main.rand.NextBool(5) ? 226 : 220;
				float scale = 0.8f + Main.rand.NextFloat(0.3f);
				float velocityMult = Main.rand.NextFloat(0.3f, 0.6f);
				int idx = Dust.NewDust(projectile.position, projectile.width, projectile.height, dustType);
				Main.dust[idx].noGravity = true;
				Main.dust[idx].velocity = velocityMult * projectile.velocity;
				Main.dust[idx].scale = scale;
			}

			// Spin in the specified starting direction and slow down spin over time
			// Loses 1.66% of current speed every frame
			// Also update current orientation to reflect current spin direction
			if (projectile.timeLeft > 200)
			{
				float currentSpin = projectile.ai[1];
				projectile.direction = currentSpin <= 0f ? -1 : 1;
				projectile.spriteDirection = projectile.direction;
				projectile.rotation += currentSpin * MaxRotationSpeed;
				float spinReduction = 0.0166f * currentSpin;
				projectile.ai[1] -= spinReduction;
			}
			// Search for and home in on nearby targets after the given time, and stops spinning
			else
			{
				projectile.rotation = 0f;
				HomingAI();
			}
		}

		private void HomingAI()
		{
			// If we don't currently have a target, go try and get one!
			int targetID = (int)projectile.ai[0] - 1;
			if (targetID < 0)
				targetID = AcquireTarget();

			// Save the target, whether we have one or not.
			projectile.ai[0] = targetID + 1f;

			// If we don't have a target, then just slow down a bit.
			if (targetID < 0)
			{
				projectile.velocity *= 0.94f;
				return;
			}

			// Homing behavior depends on how far the blade is from its target.
			NPC target = Main.npc[targetID];
			float xDist = projectile.Center.X - target.Center.X;
			float yDist = projectile.Center.Y - target.Center.Y;
			float dist = (float)Math.Sqrt(xDist * xDist + yDist * yDist);

			// If the target is too far away, stop homing in on it.
			if (dist > HomingBreakRange)
			{
				projectile.ai[0] = 0f;
				return;
			}

			// Adds a multiple of the towards-target vector to its velocity every frame.
			float homingFactor = CalcHomingFactor(dist);
			Vector2 posDiff = target.Center - projectile.Center;
			posDiff = posDiff.SafeNormalize(Vector2.Zero);
			posDiff *= homingFactor;
			Vector2 newVelocity = projectile.velocity += posDiff;

			// Caps speed to make sure it doesn't go too fast.
			if (newVelocity.Length() >= MaxSpeed)
			{
				newVelocity = newVelocity.SafeNormalize(Vector2.Zero);
				newVelocity *= MaxSpeed;
			}

			projectile.velocity = newVelocity;

			//The afterimage's blade always tries to face its target
			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(45f);
		}

		// Returns the ID of the NPC to be targeted by this energy blade.
		// It chooses the closest target which can be chased, ignoring invulnerable NPCs.
		// Nanoblack Afterimages prefer to target bosses whenever possible.
		private int AcquireTarget()
		{
			bool bossFound = false;
			int target = -1;
			float minDist = HomingStartRange;
			for (int i = 0; i < 200; ++i)
			{
				NPC npc = Main.npc[i];
				if (!npc.active || npc.type == NPCID.TargetDummy)
					continue;

				// If we've found a valid boss target, ignore ALL targets which aren't bosses.
				if (bossFound && !npc.boss)
					continue;

				if (npc.CanBeChasedBy(projectile, false))
				{
					float xDist = projectile.Center.X - npc.Center.X;
					float yDist = projectile.Center.Y - npc.Center.Y;
					float distToNPC = (float)Math.Sqrt(xDist * xDist + yDist * yDist);
					if (distToNPC < minDist)
					{
						// If this target within range is a boss, set the boss found flag.
						if (npc.boss)
							bossFound = true;
						minDist = distToNPC;
						target = i;
					}
				}
			}
			return target;
		}

		// Afterimages home even more aggressively if they are very close to their target.
		private float CalcHomingFactor(float dist)
		{
			float baseFactor = BaseHomingFactor;
			float bonus = (MaxHomingFactor - BaseHomingFactor) * (1f - dist / HomingBonusRangeCap);
			if (bonus < 0f)
				bonus = 0f;
			return baseFactor + bonus;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			CalamityUtils.DrawAfterimagesCentered(projectile, ProjectileID.Sets.TrailingMode[projectile.type], lightColor, 1);
			return false;
		}

		// Spawns a tiny bit of dust when the afterimage vanishes.
		public override void Kill(int timeLeft)
		{
			SpawnDust();
		}

		// Spawns a small bit of Luminite themed dust.
		private void SpawnDust()
		{
			int dustCount = Main.rand.Next(3, 6);
			Vector2 corner = projectile.position;
			for (int i = 0; i < dustCount; ++i)
			{
				int dustType = 229;
				float scale = 0.6f + Main.rand.NextFloat(0.4f);
				int idx = Dust.NewDust(corner, projectile.width, projectile.height, dustType);
				Main.dust[idx].noGravity = true;
				Main.dust[idx].velocity *= 3f;
				Main.dust[idx].scale = scale;
			}
		}
	}
}