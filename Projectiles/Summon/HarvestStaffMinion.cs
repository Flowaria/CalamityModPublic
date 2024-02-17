﻿using System;
using System.IO;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.Items.Weapons.Summon.HarvestStaff;

namespace CalamityMod.Projectiles.Summon
{
    public class HarvestStaffMinion : ModProjectile, ILocalizedModType
    {
        #region Properties and Enums

        public new string LocalizationCategory => "Projectiles.Summon";

        /// <summary>
        /// The size of the pumpkin.
        /// </summary>
        public ref float Variant => ref Projectile.ai[0];

        /// <summary>
        /// The states of behaviour of this pumpkin.
        /// </summary>
        public enum AIState { Still, Idle, Attack }

        /// <summary>
        /// The current state of behaviour of this pumpkin.
        /// </summary>
        public AIState State
        {
            get => (AIState)Projectile.ai[1];
            set
            {
                Projectile.ai[1] = (float)value;

                if (value == AIState.Attack)
                    Animation = AnimationState.Run;
                else if (value == AIState.Idle)
                    Animation = AnimationState.Idle;

                NetUpdate();
            }
        }

        /// <summary>
        /// The maximum time that the pumpkin will walk on its idle state.<br/>
        /// It's set to a random amount at a random moment.
        /// </summary>
        public int IdleWalkingTime { get; set; }

        /// <summary>
        /// The timer on which the pumpkin is while walking.<br/>
        /// When the timer reaches 0, the pumpkin will stop walking.
        /// </summary>
        public int IdleWalkingTimer { get; set; }

        /// <summary>
        /// The direction in which the pumpkin walks while idle.<br/>
        /// If the pumpkin is within a certain range from the sentry (Or if it doesn't exist), it'll go to a random direction.<br/>
        /// Otherwise, it'll try to always be nearby the sentry.
        /// </summary>
        public int IdleWalkingDirection { get; set; }

        /// <summary>
        /// Keeps track of the amount of jumps that the pumpkin has done when the owner is nearby.<br/>
        /// When it reaches a certain amount, the minion will go on a cooldown before jumping again.
        /// </summary>
        public int IdleJumpCount { get; set; }

        /// <summary>
        /// The cooldown of the pumpkin for jumping again.<br/>
        /// When it reaches 0, the pumpkin will stop walking.
        /// </summary>
        public int IdleJumpCooldown { get; set; }

        /// <summary>
        /// The states of the animation that the pumpkin has.<br/>
        /// Their indeces are used when selecting which spritesheet it uses.
        /// </summary>
        public enum AnimationState { None = -1, Grow, Rise, Idle, Run, Jump }

        /// <summary>
        /// The current state of animation of the pumpkin.
        /// </summary>
        public AnimationState Animation
        {
            get => (AnimationState)Projectile.ai[2];
            set
            {
                if (value != Animation)
                {
                    Projectile.frame = 0;
                    Projectile.frameCounter = 0;
                }

                Projectile.ai[2] = (float)value;

                switch (value)
                {
                    case AnimationState.Grow:
                        AnimationFrames = Variant == 0 ? 6 : 4;
                        FramesUntilNextAnimationFrame = 6;
                        break;

                    case AnimationState.Rise:
                        AnimationFrames = 10;
                        FramesUntilNextAnimationFrame = 5;
                        break;

                    case AnimationState.Idle:
                    case AnimationState.Jump:
                        AnimationFrames = 1;
                        FramesUntilNextAnimationFrame = 0;
                        break;

                    case AnimationState.Run:
                        AnimationFrames = 6;
                        FramesUntilNextAnimationFrame = 5;
                        break;
                }

                NetUpdate();
            }
        }

        /// <summary>
        /// The amount of frames that the current animation's spritesheet has.
        /// </summary>
        public int AnimationFrames { get; set; }

        /// <summary>
        /// The amount of time, in frames, that it'll take to go to the next frame of animation.
        /// </summary>
        public int FramesUntilNextAnimationFrame { get; set; }

        /// <summary>
        /// A convienent bool for when the animation has been completed.<br/>
        /// Ends at <see cref="AnimationFrames"/> - 1 because <see cref="Projectile.frame"/> starts at 0.
        /// </summary>
        public bool CompletedAnimation => Projectile.frame >= AnimationFrames - 1;

        /// <summary>
        /// A convient way to set the direction of the sprite without typing it out the long way.
        /// </summary>
        public int Direction
        {
            get => Projectile.spriteDirection;
            set => Projectile.spriteDirection = Projectile.direction = value;
        }

        /// <summary>
        /// The owner of this minion.
        /// </summary>
        public Player Owner { get; set; }

        /// <summary>
        /// The target of this minion.
        /// </summary>
        public NPC Target { get; set; }

        /// <summary>
        /// The sentry of this minion.
        /// </summary>
        public Projectile MySentry
        {
            get
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj == null || !proj.active || proj.owner != Projectile.owner || proj.type != ModContent.ProjectileType<HarvestStaffSentry>())
                        continue;

                    return proj;
                }

                return null;
            }
        }

        #endregion

        #region AI and Collisions

        public override void AI()
        {
            Target = Projectile.Center.MinionHoming(State == AIState.Still ? PlantedEnemyDistanceDetection : NormalEnemyDistanceDetection, Owner, false);

            switch (State)
            {
                case AIState.Still:
                    StillState();
                    break;
                case AIState.Idle:
                    IdleState();
                    break;
                case AIState.Attack:
                    AttackState();
                    break;
            }

            if (IdleJumpCooldown > 0)
                IdleJumpCooldown--;

            Projectile.timeLeft = 2;
            DoGravity();
            DoAnimation();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // When the pumpkin touches the ground after spawning, start the growing animation.
            if (Animation == AnimationState.None)
                Animation = AnimationState.Grow;

            if (State == AIState.Idle && Projectile.velocity.Y == 0f)
            {
                // While the pumpkin's idle and standing still on the ground, when the player gets close it'll jump.
                if (Projectile.WithinRange(Owner.Center, 64f) && IdleJumpCooldown == 0)
                    NearOwnerJump();
                else
                    Animation = IdleWalkingTimer == 0f ? AnimationState.Idle : AnimationState.Run;
            }

            // If the minion's standing still at a certain distance from the target: jump.
            if (Target is not null && State == AIState.Attack && Projectile.velocity.Y == 0f)
            {
                if (MathF.Abs(Target.Center.X - Projectile.Center.X) < 160f && Target.Top.Y < Projectile.Bottom.Y)
                {
                    if (PlatformBetweenMinionAndTarget(out Vector2 platformPosition))
                        JumpTowards(platformPosition - Vector2.UnitY * 32f);
                    else
                        JumpTowards(Target.Top);
                }
                else
                    Animation = AnimationState.Run;
            }

            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            if (Target is not null)
                fallThrough = Projectile.Bottom.Y < Target.Top.Y;
            else
                fallThrough = false;

            return true;
        }

        public override void OnKill(int timeLeft)
        {
            Projectile.ExpandHitboxBy(3f);
            Projectile.Damage();

            for (int i = 0; i < (int)Utils.Remap(Variant, 0f, 2f, 4f, 2f); i++)
            {
                float angle = MathHelper.TwoPi / 4 * i;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Gore gore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.Center, velocity, Mod.Find<ModGore>($"PumpkinGore{Main.rand.Next(6) + 1}").Type, Utils.Remap(Variant, 0f, 2f, 1f, 0.5f));
                gore.timeLeft = 15;
            }

            if (Main.dedServ)
                return;

            Particle boomRing = new DirectionalPulseRing(Projectile.Center,
                Vector2.Zero,
                Color.Orange,
                Vector2.One,
                0f,
                0.05f,
                0.5f,
                20);
            GeneralParticleHandler.SpawnParticle(boomRing);

            for (int i = 0; i < 10; i++)
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch);
        }

        /// <summary>
        /// The behaviour of this summon while it is planted on the ground.
        /// </summary>
        public void StillState()
        {
            // When the pumpkin has done its growing animation and there's a target nearby or the owner's on top of it,
            // it'll do the jump out animation.
            if ((Target is not null || Projectile.getRect().Intersects(Owner.getRect())) && Animation == AnimationState.Grow && CompletedAnimation)
                Animation = AnimationState.Rise;

            // And when they have completed their jumping out animation, they idle until they find a target.
            else if (Animation == AnimationState.Rise && CompletedAnimation)
                State = AIState.Idle;
        }

        /// <summary>
        /// The behaviour of this usmmon while it's idle.
        /// </summary>
        public void IdleState()
        {
            // If a target has been detected, go to the attack state.
            if (Target is not null)
            {
                State = AIState.Attack;
                return;
            }

            // If the pumpkin's not already walking, at a random chance it'll decide to walk.
            if (IdleWalkingTimer == 0f && Main.rand.NextBool(400))
            {
                IdleWalkingTime = IdleWalkingTimer = Main.rand.Next(60, 180);
                IdleWalkingDirection = MySentry == null || Projectile.WithinRange(MySentry.Center, 960f) ? (Main.rand.NextBool() ? -1 : 1) : MathF.Sign(MySentry.Center.X - Projectile.Center.X);
                NetUpdate();
            }

            else if (IdleWalkingTimer != 0f)
            {
                // The pumpkin will accelerate when starting to walk and deaccelerate when it's about to end.
                Projectile.velocity.X = MathHelper.Lerp(0f, 3f, CalamityUtils.Convert01To010(Utils.GetLerpValue(0f, IdleWalkingTime, IdleWalkingTimer))) * IdleWalkingDirection;

                Direction = MathF.Sign(Projectile.velocity.X);

                IdleWalkingTimer--;
                if (IdleWalkingTimer == 0f)
                {
                    Projectile.velocity.X = 0f;
                    Animation = AnimationState.Idle;
                }
            }

            // When the pumpkin encounters a 1-tile-height obstacle, it'll climb it, like the player.
            Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
        }

        /// <summary>
        /// The behaviour of this summon in its attack state.
        /// </summary>
        public void AttackState()
        {
            if (Target is not null)
            {
                if (Projectile.WithinRange(Target.Center, 32f))
                {
                    Projectile.Kill();
                    return;
                }

                MoveToTarget();
                Direction = MathF.Sign(Projectile.velocity.X);

                // When the pumpkin encounters a 1-tile-height obstacle, it'll climb it, like the player.
                Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
            }
            else
            {
                State = AIState.Idle;
                Projectile.velocity.X = 0f;
            }
        }

        /// <summary>
        /// The minion does a small jump, to give it some flavor and cuteness.
        /// </summary>
        public void NearOwnerJump()
        {
            // Actually jumps.
            Projectile.velocity.Y = -8f;

            // After 2 hops the summon goes on a cooldown before jumping again.
            IdleJumpCount++;
            if (IdleJumpCount == 2)
            {
                IdleJumpCount = 0;
                IdleJumpCooldown = 30;
            }

            Direction = MathF.Sign(Owner.Center.X - Projectile.Center.X);
            Animation = AnimationState.Jump;
            NetUpdate();
        }

        /// <summary>
        /// The minion will start moving to the target with acceleration.
        /// </summary>
        public void MoveToTarget()
        {
            // If the variant's smaller (Variant 2 is the smallest), it has a faster acceleration and a higher max velocity.
            float maxVelocity = Utils.Remap(Variant, 0f, 2f, 5f, 8f);
            float acceleration = Utils.Remap(Variant, 0f, 2f, 0.1f, 0.3f);
            float accelerationDirection = MathF.Sign(Target.Center.X - Projectile.Center.X);

            Projectile.velocity.X += acceleration * accelerationDirection;
            if (MathF.Abs(Projectile.velocity.X) > maxVelocity)
                Projectile.velocity.X = maxVelocity * accelerationDirection;
        }

        /// <summary>
        /// The minion will jump towards a destination in the Y-axis.
        /// </summary>
        /// <param name="destination">The Y position at which the minion will jump towards</param>
        public void JumpTowards(Vector2 destination)
        {
            // Equation of a free fall independent of time: v = sqrt(2 * gravity * distance).
            // Because we want it to go up and now down, we need to negate the constant: v = sqrt(-2 * gravity * distance).
            // And now we need to negate the velocity to get it on Terraria's coordinate system: v = -sqrt(-2 * gravity * distance).
            Projectile.velocity.Y = -MathF.Sqrt(-2f * PumpkinGravityStrength * (destination.Y - Projectile.Bottom.Y));
            Animation = AnimationState.Jump;
            NetUpdate();
        }

        /// <summary>
        /// Detects a platform between the minion and the target.
        /// </summary>
        /// <param name="tilePosition">The position of said platform as a <see cref="Vector2"/>.</param>
        /// <returns>Whether there's a platform or not.</returns>
        public bool PlatformBetweenMinionAndTarget(out Vector2 tilePosition)
        {
            Point minionPosition = Projectile.Center.ToSafeTileCoordinates();
            Point targetPosition = Target.Center.ToSafeTileCoordinates();
            for (int coordY = minionPosition.Y; coordY > targetPosition.Y; coordY--)
            {
                if (Main.tile[targetPosition.X, coordY].IsTileSolidGround())
                {
                    tilePosition = new Vector2(targetPosition.X, coordY) * 16f;
                    return true;
                }
            }

            tilePosition = Vector2.Zero;
            return false;
        }

        /// <summary>
        /// Applies gravity to the minion.
        /// </summary>
        public void DoGravity()
        {
            float speed = Projectile.velocity.Y;
            if (speed < PumpkinMaxGravity)
                speed = MathF.Min(speed + PumpkinGravityStrength, PumpkinMaxGravity);
            Projectile.velocity.Y = speed;
        }

        /// <summary>
        /// Does the animation of the minion.
        /// </summary>
        public void DoAnimation()
        {
            // If the state of the animation's spritsheet is only 1 frame, no need to animate.
            if (Animation == AnimationState.None || Animation == AnimationState.Idle || Animation == AnimationState.Jump)
                return;

            Projectile.frameCounter++;
            if (Projectile.frameCounter % FramesUntilNextAnimationFrame == 0)
            {
                Projectile.frame = Math.Min(Projectile.frame + 1, AnimationFrames - 1);

                // If it's the run animation, loop it.
                if (Animation == AnimationState.Run && CompletedAnimation)
                    Projectile.frame = 0;
            }
        }

        /// <summary>
        /// A covenient way to do a <see cref="Projectile.netUpdate"/> while also handling <see cref="Projectile.netSpam"/>.
        /// </summary>
        public void NetUpdate()
        {
            Projectile.netUpdate = true;
            if (Projectile.netSpam >= 10)
                Projectile.netSpam = 9;
        }

        #endregion

        #region Other Overrides

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 68;
            ProjectileID.Sets.MinionShot[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Summon;
            Projectile.localNPCHitCooldown = 30;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.netImportant = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(IdleWalkingTime);
            writer.Write(IdleWalkingTimer);
            writer.Write(IdleJumpCount);
            writer.Write(IdleJumpCooldown);
            writer.Write(IdleWalkingDirection);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            IdleWalkingTime = reader.ReadInt32();
            IdleWalkingTimer = reader.ReadInt32();
            IdleJumpCount = reader.ReadInt32();
            IdleJumpCooldown = reader.ReadInt32();
            IdleWalkingDirection = reader.ReadInt32();
        }

        public override bool? CanDamage() => null;

        public override void OnSpawn(IEntitySource source)
        {
            Owner = Main.player[Projectile.owner];
            Projectile.width = Projectile.height = Variant == 0 ? 28 : (Variant == 1 ? 22 : 20);
            Direction = Main.rand.NextBool() ? -1 : 1;
            Animation = AnimationState.None;
        }

        #endregion

        #region Drawing

        public override bool PreDraw(ref Color lightColor)
        {
            if (Animation == AnimationState.None)
                return false;

            Asset<Texture2D> texture = ModContent.Request<Texture2D>(Texture);
            Vector2 drawPosition = Projectile.Bottom - Vector2.UnitY * (24f + Projectile.gfxOffY) - Main.screenPosition;
            Rectangle frame = texture.Frame(15, 10, (int)Variant * 5 + (int)Animation, Projectile.frame);
            Color drawColor = Projectile.GetAlpha(lightColor);
            Vector2 rotationPoint = frame.Size() * 0.5f;
            SpriteEffects flip = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture.Value, drawPosition, frame, drawColor, Projectile.rotation, rotationPoint, Projectile.scale, flip);

            return false;
        }

        #endregion
    }
}
