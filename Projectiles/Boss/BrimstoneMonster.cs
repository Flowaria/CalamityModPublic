﻿using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer;
using CalamityMod.Events;
using CalamityMod.NPCs;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Particles;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.GameContent;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Summon;

namespace CalamityMod.Projectiles.Boss
{
    public class BrimstoneMonster : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public static readonly SoundStyle SpawnSound = new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneMonsterSpawn");
        public static readonly SoundStyle DroneSound = new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneMonsterDrone");
        public SlotId RumbleSlot;
        public Texture2D screamTex;

        private float speedAdd = 0f;
        private float speedLimit = 0f;
        private int time = 0;
        private int sitStill = 90;

        public override void SetDefaults()
        {
            Projectile.Calamity().DealsDefenseDamage = true;
            Projectile.width = 320;
            Projectile.height = 320;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.hide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 36000;
            Projectile.Opacity = 0f;
            CooldownSlot = ImmunityCooldownID.Bosses;
            screamTex = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/ScreamyFace").Value;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(speedAdd);
            writer.Write(Projectile.localAI[0]);
            writer.Write(speedLimit);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            speedAdd = reader.ReadSingle();
            Projectile.localAI[0] = reader.ReadSingle();
            speedLimit = reader.ReadSingle();
        }

        public override void AI()
        {
            if (time == 0)
            {
                Projectile.scale = 0.1f;
                for (int i = 0; i < 2; i++)
                {
                    Particle bloom = new BloomParticle(Projectile.Center, Vector2.Zero, Color.Red, 1.45f, 0, 120, false);
                    GeneralParticleHandler.SpawnParticle(bloom);
                }
                Particle bloom2 = new BloomParticle(Projectile.Center, Vector2.Zero, Color.White, 1.35f, 0, 120, false);
                GeneralParticleHandler.SpawnParticle(bloom2);
            }

            time++;

            if (Projectile.scale < 1.9f && Projectile.timeLeft > 90)
            {
                if (Projectile.scale < 1.5f)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 dustVel = new Vector2(30, 30).RotatedByRandom(100) * Main.rand.NextFloat(0.05f, 1.2f);
                        Dust spawnDust = Dust.NewDustPerfect(Projectile.Center * (Projectile.scale * 5), (int)CalamityDusts.Brimstone, dustVel);
                        spawnDust.noGravity = true;
                        spawnDust.scale = Main.rand.NextFloat(1.7f, 2.8f) - Projectile.scale * 1.5f;
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 sparkVel = new Vector2(20, 20).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 1.1f);
                        GlowOrbParticle orb = new GlowOrbParticle(Projectile.Center + sparkVel * 2 * (Projectile.scale * 5), sparkVel, false, 120, Main.rand.NextFloat(1.55f, 2.75f) - Projectile.scale * 1.5f, Color.Red, true, true);
                        GeneralParticleHandler.SpawnParticle(orb);
                    }
                }
                Projectile.scale += 0.01f;
            }

            if (SoundEngine.TryGetActiveSound(RumbleSlot, out var RumbleSound) && RumbleSound.IsPlaying)
                RumbleSound.Position = Projectile.Center;

            if (!CalamityPlayer.areThereAnyDamnBosses)
            {
                if (Projectile.timeLeft > 90)
                    Projectile.timeLeft = 90;
                Projectile.netUpdate = true;
            }

            int choice = (int)Projectile.ai[1];
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.soundDelay = 1125 - (choice * 225);
                SoundEngine.PlaySound(SpawnSound, Projectile.Center);
                if (Projectile.ai[1] == 0f && Projectile.timeLeft >= 90)
                    RumbleSlot = Main.zenithWorld ? SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/SCalSounds/GFBDrone") with { IsLooped = true }, Projectile.Center) : SoundEngine.PlaySound(DroneSound with { IsLooped = true }, Projectile.Center);
                Projectile.localAI[0] += 1f;
                speedLimit = 23;
            }

            if (speedAdd < speedLimit)
                speedAdd += 0.04f;

            float targetDist;
            if (!Main.player[choice].dead && Main.player[choice].active && Main.player[choice] != null)
                targetDist = Vector2.Distance(Main.player[choice].Center, Projectile.Center);
            else
                targetDist = 2000;

            if (Projectile.ai[1] == 0f)
            {
                if (targetDist <= 1400)
                {
                    float targetPitchShift = Utils.GetLerpValue(1400, 700, targetDist);
                    if (SoundEngine.TryGetActiveSound(RumbleSlot, out var RumblePitch) && RumblePitch.IsPlaying)
                    {
                        RumblePitch.Pitch = MathHelper.Lerp(Main.zenithWorld ? -0.7f : 0f, Main.zenithWorld ? 0.2f : 0.7f, targetPitchShift);
                        RumblePitch.Volume = MathHelper.Lerp(0.3f, 0.8f, targetPitchShift);
                    }
                }

                Projectile.soundDelay--;
                if (SoundEngine.TryGetActiveSound(RumbleSlot, out var RumblePlaying) && RumblePlaying.IsPlaying)
                {
                    Projectile.soundDelay = 1;
                }

                if (Projectile.soundDelay <= 0 && Projectile.timeLeft >= 90)
                    RumbleSlot = Main.zenithWorld ? SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/SCalSounds/GFBDrone") with { IsLooped = true }, Projectile.Center) : SoundEngine.PlaySound(DroneSound with { IsLooped = true }, Projectile.Center);

                if (Projectile.timeLeft < 90)
                {
                    RumblePlaying?.Stop();
                }
                if (CalamityGlobalNPC.SCal == -1)
                {
                    RumblePlaying?.Stop();
                }
            }

            if (Projectile.timeLeft < 90)
            {
                Projectile.Opacity = MathHelper.Clamp(Projectile.timeLeft / 90f, 0f, 1f);
            }
            else
            {
                Projectile.Opacity = MathHelper.Clamp(1f - ((Projectile.timeLeft - 35910) / 90f), 0f, 1f);
            }

            if (Projectile.scale >= 1.9f)
                sitStill--;
            if (sitStill > 0)
                return;

            bool revenge = CalamityWorld.revenge || BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            Lighting.AddLight(Projectile.Center, 3f * Projectile.Opacity, 0f, 0f);

            float inertia = (revenge ? 4.5f : 5f) + speedAdd;
            float speed = (revenge ? 1.5f : 1.35f) + (speedAdd * 0.25f);
            float minDist = 160f;

            if (NPC.AnyNPCs(ModContent.NPCType<SoulSeekerSupreme>()))
            {
                inertia *= 1.5f;
                speed *= 0.5f;
            }

            int target = (int)Projectile.ai[0];
            if (target >= 0 && Main.player[target].active && !Main.player[target].dead)
            {
                if (Projectile.Distance(Main.player[target].Center) > minDist)
                {
                    Vector2 moveDirection = Projectile.SafeDirectionTo(Main.player[target].Center, Vector2.UnitY);
                    Projectile.velocity = (Projectile.velocity * (inertia - 1f) + moveDirection * speed) / inertia;
                }
            }
            else
            {
                Projectile.ai[0] = Player.FindClosest(Projectile.Center, 1, 1);
                Projectile.netUpdate = true;
            }

            if (death)
            {
                speedLimit = 15;
                return;
            }

            // Fly away from other brimstone monsters.
            float pushForce = 0.05f;
            for (int k = 0; k < Main.maxProjectiles; k++)
            {
                Projectile otherProj = Main.projectile[k];
                // Short circuits to make the loop as fast as possible.
                if (!otherProj.active || k == Projectile.whoAmI)
                    continue;

                // If the other projectile is indeed the same owned by the same player and they're too close, nudge them away.
                bool sameProjType = otherProj.type == Projectile.type;
                float taxicabDist = Vector2.Distance(Projectile.Center, otherProj.Center);
                float distancegate = Main.zenithWorld ? 360f : 320f;
                if (sameProjType && taxicabDist < distancegate)
                {
                    if (Projectile.position.X < otherProj.position.X)
                        Projectile.velocity.X -= pushForce;
                    else
                        Projectile.velocity.X += pushForce;

                    if (Projectile.position.Y < otherProj.position.Y)
                        Projectile.velocity.Y -= pushForce;
                    else
                        Projectile.velocity.Y += pushForce;
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 170f * Projectile.scale * Projectile.Opacity, targetHitbox);

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            lightColor.R = (byte)(255 * Projectile.Opacity);
            Main.spriteBatch.End();
            Effect shieldEffect = Filters.Scene["CalamityMod:HellBall"].GetShader().Shader;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, shieldEffect, Main.GameViewMatrix.TransformationMatrix);

            float noiseScale = 0.6f;

            // Define shader parameters

            shieldEffect.Parameters["time"].SetValue(Projectile.timeLeft / 60f * 0.24f);
            shieldEffect.Parameters["blowUpPower"].SetValue(3.2f);
            shieldEffect.Parameters["blowUpSize"].SetValue(0.4f);
            shieldEffect.Parameters["noiseScale"].SetValue(noiseScale);

            float opacity = Projectile.Opacity;
            shieldEffect.Parameters["shieldOpacity"].SetValue(opacity);
            shieldEffect.Parameters["shieldEdgeBlendStrenght"].SetValue(4f);

            Color edgeColor = Color.Black * opacity;
            Color shieldColor = Color.Lerp(Color.Red, Color.Magenta, 0.5f) * opacity;

            // Define shader parameters for ball color
            shieldEffect.Parameters["shieldColor"].SetValue(shieldColor.ToVector3());
            shieldEffect.Parameters["shieldEdgeColor"].SetValue(edgeColor.ToVector3());

            Vector2 pos = Projectile.Center - Main.screenPosition;

            float scale = 0.715f;
            Main.spriteBatch.Draw(screamTex, pos, null, Color.White, 0, screamTex.Size() * 0.5f, scale * Projectile.scale * Projectile.Opacity, 0, 0);

            //Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale * 0.3f, SpriteEffects.None, 0);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            bool isCirrus = CalamityGlobalNPC.SCal != -1 && Main.npc[CalamityGlobalNPC.SCal].active && Main.npc[CalamityGlobalNPC.SCal].ModNPC<SupremeCalamitas>().cirrus;
            if (isCirrus)
            {
                Texture2D hageTex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Boss/BrimstoneMonsterII").Value;
                lightColor.B = (byte)(255 * Projectile.Opacity);
                Main.EntitySpriteDraw(hageTex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, hageTex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            }
            else
            {
                Texture2D vortexTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/SoulVortex").Value;
                Texture2D centerTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/LargeBloom").Value;
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 3f + Main.GlobalTimeWrappedHourly * MathHelper.TwoPi;
                    Color outerColor = Color.Lerp(Color.Red, Color.Magenta, i * 0.15f);
                    Color drawColor = Color.Lerp(outerColor, Color.Black, i * 0.2f) * 0.5f;
                    drawColor.A = 0;
                    Vector2 drawPosition = Projectile.Center - Main.screenPosition;

                    drawPosition += (angle + Main.GlobalTimeWrappedHourly * i / 16f).ToRotationVector2() * 6f;
                    Main.EntitySpriteDraw(vortexTexture, drawPosition, null, drawColor * Projectile.Opacity, -angle + MathHelper.PiOver2, vortexTexture.Size() * 0.5f, (Projectile.scale * (1 - i * 0.05f)) * Projectile.Opacity, SpriteEffects.None, 0);
                }
                Main.EntitySpriteDraw(centerTexture, Projectile.Center - Main.screenPosition, null, Color.Black * Projectile.Opacity, Projectile.rotation, centerTexture.Size() * 0.5f, (Projectile.scale * 0.9f) * Projectile.Opacity, SpriteEffects.None, 0);
            }
            return false;
        }

        public override bool CanHitPlayer(Player target) => Projectile.Opacity == 1f;

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0 || Projectile.Opacity != 1f)
                return;

            target.AddBuff(ModContent.BuffType<VulnerabilityHex>(), 300, true);

            // Remove all positive buffs from the player if they're hit by HAGE while Cirrus is alive.
            if (CalamityGlobalNPC.SCal != -1)
            {
                if (Main.npc[CalamityGlobalNPC.SCal].active)
                {
                    if (Main.npc[CalamityGlobalNPC.SCal].ModNPC<SupremeCalamitas>().cirrus)
                    {
                        for (int l = 0; l < Player.MaxBuffs; l++)
                        {
                            int buffType = target.buffType[l];
                            if (target.buffTime[l] > 0 && CalamityLists.amalgamBuffList.Contains(buffType))
                            {
                                target.DelBuff(l);
                                l--;
                            }
                        }
                    }
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
            if (SoundEngine.TryGetActiveSound(RumbleSlot, out var RumblePlaying) && RumblePlaying.IsPlaying)
            {
                RumblePlaying?.Stop();
            }
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
    }
}
