﻿using CalamityMod.Events;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Vanity;
using CalamityMod.Items.LoreItems;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Pets;
using CalamityMod.Items.Placeables.Furniture.Trophies;
using CalamityMod.Items.TreasureBags;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Projectiles.Boss;
using CalamityMod.Tiles.Ores;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
/* states:
 * 0 = slow drift
 * 1 = reelback and teleport after spawn enemy
 * 2 = reelback for spin lunge + death legacy
 * 3 = spin lunge
 * 4 = semicircle spawn arc
 * 5 = raindash
 * 6 = deceleration
 */

namespace CalamityMod.NPCs.HiveMind
{
    public class HiveMind : ModNPC
    {
        public static int normalIconIndex;
        public static int phase2IconIndex;

        internal static void LoadHeadIcons()
        {
            string normalIconPath = "CalamityMod/NPCs/HiveMind/HiveMind_Head_Boss";
            string phase2IconPath = "CalamityMod/NPCs/HiveMind/HiveMindP2_Head_Boss";

            CalamityMod.Instance.AddBossHeadTexture(normalIconPath, -1);
            normalIconIndex = ModContent.GetModBossHeadSlot(normalIconPath);

            CalamityMod.Instance.AddBossHeadTexture(phase2IconPath, -1);
            phase2IconIndex = ModContent.GetModBossHeadSlot(phase2IconPath);
        }

        // This block of values can be modified in SetDefaults() based on difficulty mode or something
        private int biomeEnrageTimer = CalamityGlobalNPC.biomeEnrageTimerMax;
        private int burrowTimer = 420;
        private int minimumDriftTime = 300;
        private int teleportRadius = 300;
        private int decelerationTime = 30;
        private int reelbackFade = 2; // Divide 255 by this for duration of reelback in ticks
        private float arcTime = 45f; // Ticks needed to complete movement for spawn and rain attacks (DEATH ONLY)
        private float driftSpeed = 1f; // Default speed when slowly floating at player
        private float driftBoost = 1f; // Max speed added as health decreases
        private int lungeDelay = 90; // # of ticks long hive mind spends sliding to a stop before lunging
        private int lungeTime = 33;
        private int lungeFade = 15; // Divide 255 by this for duration of hive mind spin before slowing for lunge
        private double lungeRots = 0.2; // Number of revolutions made while spinning/fading in for lunge
        private bool dashStarted = false;
        private int phase2timer = 360;
        private int rotationDirection;
        private double rotation;
        private double rotationIncrement;
        private int state = 0;
        private int previousState = 0;
        private int nextState = 0;
        private int reelCount = 0;
        private Vector2 deceleration;
        private int frameX = 0;
        private int frameY = 0;
        private const int maxFramesX_Phase2 = 2;
        private const int maxFramesY_Phase2 = 8;
        private const int height_Phase2 = 142;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("The Hive Mind");
            Main.npcFrameCount[NPC.type] = 16;
            NPCID.Sets.TrailingMode[NPC.type] = 1;
            NPCID.Sets.TrailCacheLength[NPC.type] = NPC.oldPos.Length;
        }

        public override void SetDefaults()
        {
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.npcSlots = 5f;
            NPC.GetNPCDamage();
            NPC.width = 178;
            NPC.height = 122;
            NPC.defense = 8;
            NPC.LifeMaxNERB(8500, 10200, 350000);
            double HPBoost = CalamityConfig.Instance.BossHealthBoost * 0.01;
            NPC.lifeMax += (int)(NPC.lifeMax * HPBoost);
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 15, 0, 0);
            NPC.boss = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            Music = CalamityMod.Instance.GetMusicFromMusicMod("HiveMind") ?? MusicID.Boss2;
            bossBag = ModContent.ItemType<HiveMindBag>();

            if (Main.expertMode)
            {
                minimumDriftTime = 120;
                reelbackFade = 4;
            }

            if (CalamityWorld.revenge)
            {
                lungeRots = 0.3;
                minimumDriftTime = 90;
                reelbackFade = 5;
                lungeTime = 28;
                driftSpeed = 2f;
                driftBoost = 2f;
            }

            if (CalamityWorld.death)
            {
                lungeRots = 0.4;
                minimumDriftTime = 60;
                reelbackFade = 6;
                lungeTime = 23;
                driftSpeed = 3f;
                driftBoost = 1f;
            }

            if (CalamityWorld.malice || BossRushEvent.BossRushActive)
            {
                lungeRots = 0.4;
                minimumDriftTime = 40;
                reelbackFade = 10;
                lungeTime = 16;
                driftSpeed = 6f;
                driftBoost = 1f;
            }

            phase2timer = minimumDriftTime;
            rotationIncrement = 0.0246399424 * lungeRots * lungeFade;
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToSickness = true;
        }

        public override void BossHeadSlot(ref int index)
        {
            if (NPC.life / (float)NPC.lifeMax < 0.8f)
                index = phase2IconIndex;
            else
                index = normalIconIndex;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(biomeEnrageTimer);
            writer.Write(NPC.dontTakeDamage);
            writer.Write(NPC.noTileCollide);
            writer.Write(NPC.noGravity);
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[3]);
            writer.Write(burrowTimer);
            writer.Write(state);
            writer.Write(nextState);
            writer.Write(phase2timer);
            writer.Write(dashStarted);
            writer.Write(rotationDirection);
            writer.Write(rotation);
            writer.Write(previousState);
            writer.Write(reelCount);
            writer.Write(frameX);
            writer.Write(frameY);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            biomeEnrageTimer = reader.ReadInt32();
            NPC.dontTakeDamage = reader.ReadBoolean();
            NPC.noTileCollide = reader.ReadBoolean();
            NPC.noGravity = reader.ReadBoolean();
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[3] = reader.ReadSingle();
            burrowTimer = reader.ReadInt32();
            state = reader.ReadInt32();
            nextState = reader.ReadInt32();
            phase2timer = reader.ReadInt32();
            dashStarted = reader.ReadBoolean();
            rotationDirection = reader.ReadInt32();
            rotation = reader.ReadDouble();
            previousState = reader.ReadInt32();
            reelCount = reader.ReadInt32();
            frameX = reader.ReadInt32();
            frameY = reader.ReadInt32();
        }

        public override void FindFrame(int frameHeight)
        {
            // When Hive Mind starts flying around
            bool phase2 = NPC.life / (float)NPC.lifeMax < 0.8f;

            if (phase2)
            {
                NPC.frameCounter++;
                if (NPC.frameCounter >= 6D)
                {
                    // Reset frame counter
                    NPC.frameCounter = 0D;

                    // Increment the Y frame
                    frameY++;

                    // Reset the Y frame if greater than 8
                    if (frameY == maxFramesY_Phase2)
                    {
                        frameX++;
                        frameY = 0;
                    }

                    // Reset the frames to frame 0
                    if ((frameX * maxFramesY_Phase2) + frameY > 15)
                        frameX = frameY = 0;
                }
            }
            else
            {
                NPC.frameCounter += 1f / 6f;
                NPC.frameCounter %= Main.npcFrameCount[NPC.type];
                int frame = (int)NPC.frameCounter;
                NPC.frame.Y = frame * frameHeight;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            // When Hive Mind starts flying around
            bool phase2 = NPC.life / (float)NPC.lifeMax < 0.8f;

            if (phase2)
            {
                SpriteEffects spriteEffects = NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/NPCs/HiveMind/HiveMindP2").Value;
                Rectangle frame = new Rectangle(NPC.width * frameX, NPC.height * frameY, NPC.width, NPC.height);
                Vector2 vector = new Vector2(NPC.width / 2, NPC.height / 2);
                Color afterimageBaseColor = Color.White;
                int numAfterimages = 5;

                if (CalamityConfig.Instance.Afterimages && state != 0)
                {
                    for (int i = 1; i < numAfterimages; i += 2)
                    {
                        Color afterimageColor = drawColor;
                        afterimageColor = Color.Lerp(afterimageColor, afterimageBaseColor, 0.5f);
                        afterimageColor = NPC.GetAlpha(afterimageColor);
                        afterimageColor *= (numAfterimages - i) / 15f;
                        Vector2 afterimageCenter = NPC.oldPos[i] + new Vector2(NPC.width, NPC.height) / 2f - Main.screenPosition;
                        afterimageCenter -= new Vector2(texture.Width, texture.Height) / new Vector2(maxFramesX_Phase2, maxFramesY_Phase2) * NPC.scale / 2f;
                        afterimageCenter += vector * NPC.scale + new Vector2(0f, NPC.gfxOffY);
                        spriteBatch.Draw(texture, afterimageCenter, NPC.frame, afterimageColor, NPC.oldRot[i], vector, NPC.scale, spriteEffects, 0f);
                    }
                }

                Vector2 center = NPC.Center - Main.screenPosition;
                spriteBatch.Draw(texture, center, frame, NPC.GetAlpha(drawColor), NPC.rotation, vector, NPC.scale, spriteEffects, 0f);

                return false;
            }

            return true;
        }

        private void SpawnStuff()
        {
            int maxSpawns = (CalamityWorld.death || BossRushEvent.BossRushActive) ? 5 : CalamityWorld.revenge ? 4 : Main.expertMode ? Main.rand.Next(3, 5) : Main.rand.Next(2, 4);
            for (int i = 0; i < maxSpawns; i++)
            {
                int type = NPCID.EaterofSouls;
                int choice = -1;
                do
                {
                    choice++;
                    switch (choice)
                    {
                        case 0:
                        case 1:
                            type = NPCID.EaterofSouls;
                            break;
                        case 2:
                            type = NPCID.DevourerHead;
                            break;
                        case 3:
                        case 4:
                            type = ModContent.NPCType<DankCreeper>();
                            break;
                        default:
                            break;
                    }
                }
                while (NPC.AnyNPCs(type) && choice < 5);

                if (choice < 5)
                    NPC.NewNPC((int)NPC.position.X + Main.rand.Next(NPC.width), (int)NPC.position.Y + Main.rand.Next(NPC.height), type);
            }
        }

        private void ReelBack()
        {
            NPC.alpha = 0;
            phase2timer = 0;
            deceleration = NPC.velocity / 255f * reelbackFade;

            if (CalamityWorld.revenge || BossRushEvent.BossRushActive)
            {
                state = 2;
                SoundEngine.PlaySound(SoundID.ForceRoar, (int)NPC.Center.X, (int)NPC.Center.Y, -1, 1f, 0f);
            }
            else
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    SpawnStuff();

                state = nextState;
                nextState = 0;

                if (state == 2)
                    SoundEngine.PlaySound(SoundID.Roar, (int)NPC.Center.X, (int)NPC.Center.Y, 0);
                else
                    SoundEngine.PlaySound(SoundID.ForceRoar, (int)NPC.Center.X, (int)NPC.Center.Y, -1, 1f, 0f);
            }
        }

        public override void AI()
        {
            // Get a target
            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            // Despawn safety, make sure to target another player if the current player target is too far away
            if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles)
                NPC.TargetClosest();

            Player player = Main.player[NPC.target];

            bool malice = CalamityWorld.malice || BossRushEvent.BossRushActive;
            bool expertMode = Main.expertMode || BossRushEvent.BossRushActive;
            bool revenge = CalamityWorld.revenge || BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;

            // Percent life remaining
            float lifeRatio = NPC.life / (float)NPC.lifeMax;

            // Enrage
            if ((!player.ZoneCorrupt || (NPC.position.Y / 16f) < Main.worldSurface) && !BossRushEvent.BossRushActive)
            {
                if (biomeEnrageTimer > 0)
                    biomeEnrageTimer--;
            }
            else
                biomeEnrageTimer = CalamityGlobalNPC.biomeEnrageTimerMax;

            bool biomeEnraged = biomeEnrageTimer <= 0 || malice;

            float enrageScale = BossRushEvent.BossRushActive ? 1f : 0f;
            if (biomeEnraged && (!player.ZoneCorrupt || malice))
            {
                NPC.Calamity().CurrentlyEnraged = !BossRushEvent.BossRushActive;
                enrageScale += 1f;
            }
            if (biomeEnraged && ((NPC.position.Y / 16f) < Main.worldSurface || malice))
            {
                NPC.Calamity().CurrentlyEnraged = !BossRushEvent.BossRushActive;
                enrageScale += 1f;
            }

            // When Hive Mind starts flying around
            bool phase2 = lifeRatio < 0.8f;

            // Phase 2 settings
            if (phase2)
            {
                // Spawn gores, play sound and reset every crucial variable at the start
                if (NPC.localAI[1] == 0f)
                {
                    NPC.localAI[1] = 1f;

                    if (Main.netMode != NetmodeID.Server)
                    {
                        int goreAmount = 7;
                        for (int i = 1; i <= goreAmount; i++)
                            Gore.NewGore(NPC.position, NPC.velocity, Mod.Find<ModGore>("Gores/HiveMindGores/HiveMindGore" + i).Type, 1f);
                    }

                    SoundEngine.PlaySound(SoundID.NPCDeath1, (int)NPC.Center.X, (int)NPC.Center.Y);

                    NPC.position = NPC.Center;
                    NPC.height = height_Phase2;
                    NPC.position -= NPC.Size * 0.5f;

                    NPC.frame.Y = 0;
                    NPC.noGravity = true;
                    NPC.noTileCollide = true;
                    NPC.scale = 1f;
                    NPC.alpha = 0;
                    NPC.dontTakeDamage = false;
                    NPC.damage = NPC.defDamage;
                }

                NPC.frame = new Rectangle(NPC.width * frameX, NPC.height * frameY, NPC.width, NPC.height);
            }
            else
            {
                CalamityGlobalNPC.hiveMind = NPC.whoAmI;

                if (!player.active || player.dead)
                {
                    NPC.TargetClosest(false);
                    player = Main.player[NPC.target];
                    if (!player.active || player.dead)
                    {
                        if (NPC.timeLeft > 60)
                            NPC.timeLeft = 60;

                        if (NPC.localAI[3] < 120f)
                            NPC.localAI[3] += 1f;

                        if (NPC.localAI[3] > 60f)
                        {
                            NPC.velocity.Y += (NPC.localAI[3] - 60f) * 0.5f;

                            NPC.noGravity = true;
                            NPC.noTileCollide = true;

                            if (burrowTimer > 30)
                                burrowTimer = 30;
                        }

                        return;
                    }
                }
                else if (NPC.timeLeft < 1800)
                    NPC.timeLeft = 1800;

                if (NPC.localAI[3] > 0f)
                {
                    NPC.localAI[3] -= 1f;
                    return;
                }

                NPC.noGravity = false;
                NPC.noTileCollide = false;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (NPC.localAI[0] == 0f)
                    {
                        NPC.localAI[0] = 1f;
                        int maxBlobs = death ? 15 : revenge ? 7 : expertMode ? 6 : 5;
                        for (int i = 0; i < maxBlobs; i++)
                            NPC.NewNPC((int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<HiveBlob>(), NPC.whoAmI);
                    }
                }

                if (NPC.ai[3] == 0f && NPC.life > 0)
                    NPC.ai[3] = NPC.lifeMax;

                if (NPC.life > 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int num660 = (int)(NPC.lifeMax * 0.05);
                        if ((NPC.life + num660) < NPC.ai[3])
                        {
                            NPC.ai[3] = NPC.life;

                            int maxSpawns = malice ? 10 : death ? 5 : revenge ? 4 : expertMode ? Main.rand.Next(3, 5) : Main.rand.Next(2, 4);
                            int maxDankSpawns = malice ? 4 : death ? Main.rand.Next(2, 4) : revenge ? 2 : expertMode ? Main.rand.Next(1, 3) : 1;

                            for (int num662 = 0; num662 < maxSpawns; num662++)
                            {
                                int x = (int)(NPC.position.X + Main.rand.Next(NPC.width - 32));
                                int y = (int)(NPC.position.Y + Main.rand.Next(NPC.height - 32));

                                int type = ModContent.NPCType<HiveBlob>();
                                if (NPC.CountNPCS(ModContent.NPCType<DankCreeper>()) < maxDankSpawns)
                                    type = ModContent.NPCType<DankCreeper>();

                                int num664 = NPC.NewNPC(x, y, type);
                                Main.npc[num664].SetDefaults(type);
                                if (Main.netMode == NetmodeID.Server && num664 < Main.maxNPCs)
                                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, num664, 0f, 0f, 0f, 0, 0, 0);
                            }

                            return;
                        }
                    }
                }

                burrowTimer--;
                if (burrowTimer < -120)
                {
                    burrowTimer = (death ? 180 : revenge ? 300 : expertMode ? 360 : 420) - (int)enrageScale * 55;
                    if (burrowTimer < 30)
                        burrowTimer = 30;

                    NPC.scale = 1f;
                    NPC.alpha = 0;
                    NPC.dontTakeDamage = false;
                    NPC.damage = NPC.defDamage;
                }
                else if (burrowTimer < -60)
                {
                    NPC.scale += 0.0165f;
                    NPC.alpha -= 4;

                    int num622 = Dust.NewDust(new Vector2(NPC.position.X, NPC.Center.Y), NPC.width, NPC.height / 2, 14, 0f, -3f, 100, default, 2.5f * NPC.scale);
                    Main.dust[num622].velocity *= 2f;
                    if (Main.rand.NextBool(2))
                    {
                        Main.dust[num622].scale = 0.5f;
                        Main.dust[num622].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        int num624 = Dust.NewDust(new Vector2(NPC.position.X, NPC.Center.Y), NPC.width, NPC.height / 2, 14, 0f, -3f, 100, default, 3.5f * NPC.scale);
                        Main.dust[num624].noGravity = true;
                        Main.dust[num624].velocity *= 3.5f;
                        num624 = Dust.NewDust(new Vector2(NPC.position.X, NPC.Center.Y), NPC.width, NPC.height / 2, 14, 0f, -3f, 100, default, 2.5f * NPC.scale);
                        Main.dust[num624].velocity *= 1f;
                    }
                }
                else if (burrowTimer == -60)
                {
                    NPC.scale = 0.01f;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.Center = player.Center;
                        NPC.position.Y = player.position.Y - NPC.height;
                        int tilePosX = (int)NPC.Center.X / 16;
                        int tilePosY = (int)(NPC.position.Y + NPC.height) / 16 + 1;

                        if (Main.tile[tilePosX, tilePosY] == null)
                            Main.tile[tilePosX, tilePosY] = new Tile();

                        while (!(Main.tile[tilePosX, tilePosY].HasUnactuatedTile && Main.tileSolid[Main.tile[tilePosX, tilePosY].TileType]))
                        {
                            tilePosY++;
                            NPC.position.Y += 16;
                            if (Main.tile[tilePosX, tilePosY] == null)
                                Main.tile[tilePosX, tilePosY] = new Tile();
                        }
                    }
                    NPC.netUpdate = true;
                }
                else if (burrowTimer < 0)
                {
                    NPC.scale -= 0.0165f;
                    NPC.alpha += 4;

                    int num622 = Dust.NewDust(new Vector2(NPC.position.X, NPC.Center.Y), NPC.width, NPC.height / 2, 14, 0f, -3f, 100, default, 2.5f * NPC.scale);
                    Main.dust[num622].velocity *= 2f;
                    if (Main.rand.NextBool(2))
                    {
                        Main.dust[num622].scale = 0.5f;
                        Main.dust[num622].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        int num624 = Dust.NewDust(new Vector2(NPC.position.X, NPC.Center.Y), NPC.width, NPC.height / 2, 14, 0f, -3f, 100, default, 3.5f * NPC.scale);
                        Main.dust[num624].noGravity = true;
                        Main.dust[num624].velocity *= 3.5f;
                        num624 = Dust.NewDust(new Vector2(NPC.position.X, NPC.Center.Y), NPC.width, NPC.height / 2, 14, 0f, -3f, 100, default, 2.5f * NPC.scale);
                        Main.dust[num624].velocity *= 1f;
                    }
                }
                else if (burrowTimer == 0)
                {
                    if (!player.active || player.dead)
                    {
                        burrowTimer = 30;
                    }
                    else
                    {
                        NPC.TargetClosest();
                        NPC.dontTakeDamage = true;
                        NPC.damage = 0;
                    }
                }

                return;
            }

            if (NPC.alpha != 0)
            {
                if (NPC.damage != 0)
                    NPC.damage = 0;
            }
            else
                NPC.damage = NPC.defDamage;

            switch (state)
            {
                case 0: // Slowdrift

                    if (NPC.alpha > 0)
                        NPC.alpha -= 3;

                    if (nextState == 0)
                    {
                        NPC.TargetClosest();
                        if (revenge && lifeRatio < 0.53f)
                        {
                            if (death)
                            {
                                do
                                    nextState = Main.rand.Next(3, 6);
                                while (nextState == previousState);
                                previousState = nextState;
                            }
                            else if (lifeRatio < 0.27f)
                            {
                                do
                                    nextState = Main.rand.Next(3, 6);
                                while (nextState == previousState);
                                previousState = nextState;
                            }
                            else
                            {
                                do
                                    nextState = Main.rand.Next(3, 5);
                                while (nextState == previousState);
                                previousState = nextState;
                            }
                        }
                        else
                        {
                            if (revenge && (Main.rand.NextBool(3) || reelCount == 2))
                            {
                                reelCount = 0;
                                nextState = 2;
                            }
                            else
                            {
                                reelCount++;
                                if (Main.expertMode && reelCount == 2)
                                {
                                    reelCount = 0;
                                    nextState = 2;
                                }
                                else
                                    nextState = 1;

                                NPC.ai[1] = 0f;
                                NPC.ai[2] = 0f;
                            }
                        }

                        if (nextState == 3)
                            rotation = MathHelper.ToRadians(Main.rand.Next(360));

                        NPC.netUpdate = true;
                    }

                    if (!player.active || player.dead || Vector2.Distance(NPC.Center, player.Center) > 5000f)
                    {
                        NPC.TargetClosest(false);
                        player = Main.player[NPC.target];
                        if (!player.active || player.dead || Vector2.Distance(NPC.Center, player.Center) > 5000f)
                        {
                            if (NPC.timeLeft > 60)
                                NPC.timeLeft = 60;

                            if (NPC.localAI[3] < 120f)
                                NPC.localAI[3] += 1f;

                            if (NPC.localAI[3] > 60f)
                                NPC.velocity.Y += (NPC.localAI[3] - 60f) * 0.5f;

                            return;
                        }
                    }
                    else if (NPC.timeLeft < 1800)
                        NPC.timeLeft = 1800;

                    if (NPC.localAI[3] > 0f)
                    {
                        NPC.localAI[3] -= 1f;
                        return;
                    }

                    NPC.velocity = player.Center - NPC.Center;

                    phase2timer--;
                    if (phase2timer <= -180) // No stalling drift mode forever
                    {
                        NPC.velocity *= 2f / 255f * (reelbackFade + 2 * (int)enrageScale);
                        ReelBack();
                        NPC.netUpdate = true;
                    }
                    else
                    {
                        NPC.velocity.Normalize();
                        if (expertMode) // Variable velocity in expert and up
                            NPC.velocity *= driftSpeed + enrageScale + driftBoost * lifeRatio;
                        else
                            NPC.velocity *= driftSpeed + enrageScale;
                    }

                    break;

                case 1: // Reelback and teleport

                    NPC.alpha += reelbackFade + 2 * (int)enrageScale;
                    NPC.velocity -= deceleration;

                    if (NPC.alpha >= 255)
                    {
                        NPC.alpha = 255;
                        NPC.velocity = Vector2.Zero;
                        state = 0;

                        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[1] != 0f && NPC.ai[2] != 0f)
                        {
                            NPC.position.X = NPC.ai[1] * 16 - NPC.width / 2;
                            NPC.position.Y = NPC.ai[2] * 16 - NPC.height / 2;
                        }

                        phase2timer = minimumDriftTime + Main.rand.Next(121);
                        NPC.netUpdate = true;
                    }
                    else if (NPC.ai[1] == 0f && NPC.ai[2] == 0f)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            int posX = (int)player.Center.X / 16 + Main.rand.Next(15, 46) * (Main.rand.NextBool(2) ? -1 : 1);
                            int posY = (int)player.Center.Y / 16 + Main.rand.Next(15, 46) * (Main.rand.NextBool(2) ? -1 : 1);
                            if (!WorldGen.SolidTile(posX, posY) && Collision.CanHit(new Vector2(posX * 16, posY * 16), 1, 1, player.position, player.width, player.height))
                            {
                                NPC.ai[1] = posX;
                                NPC.ai[2] = posY;
                                NPC.netUpdate = true;
                                break;
                            }
                        }
                    }

                    break;

                case 2: // Reelback for lunge + death legacy

                    NPC.alpha += reelbackFade + 2 * (int)enrageScale;
                    NPC.velocity -= deceleration;

                    if (NPC.alpha >= 255)
                    {
                        NPC.alpha = 255;
                        NPC.velocity = Vector2.Zero;
                        dashStarted = false;

                        if (revenge && lifeRatio < 0.53f)
                        {
                            state = nextState;
                            nextState = 0;
                            previousState = state;
                        }
                        else
                            state = 3;

                        if (player.velocity.X > 0)
                            rotationDirection = 1;
                        else if (player.velocity.X < 0)
                            rotationDirection = -1;
                        else
                            rotationDirection = player.direction;
                    }

                    break;

                case 3: // Lunge

                    NPC.netUpdate = true;
                    if (NPC.alpha > 0)
                    {
                        NPC.alpha -= lungeFade;

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            NPC.Center = player.Center + new Vector2(teleportRadius, 0).RotatedBy(rotation);

                        rotation += rotationIncrement * rotationDirection;
                        phase2timer = lungeDelay;
                    }
                    else
                    {
                        phase2timer--;
                        if (!dashStarted)
                        {
                            if (phase2timer <= 0)
                            {
                                phase2timer = lungeTime - 4 * (int)enrageScale;
                                NPC.velocity = player.Center + (malice ? player.velocity * 20f : Vector2.Zero) - NPC.Center;
                                NPC.velocity.Normalize();
                                NPC.velocity *= teleportRadius / (lungeTime - (int)enrageScale);
                                dashStarted = true;
                                SoundEngine.PlaySound(SoundID.Roar, (int)NPC.Center.X, (int)NPC.Center.Y, 0);
                            }
                            else
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    NPC.Center = player.Center + new Vector2(teleportRadius, 0).RotatedBy(rotation);

                                rotation += rotationIncrement * rotationDirection * phase2timer / lungeDelay;
                            }
                        }
                        else
                        {
                            if (phase2timer <= 0)
                            {
                                state = 6;
                                phase2timer = 0;
                                deceleration = NPC.velocity / decelerationTime;
                            }
                        }
                    }

                    break;

                case 4: // Enemy spawn arc

                    if (NPC.alpha > 0)
                    {
                        NPC.alpha -= 5;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.Center = player.Center;
                            NPC.position.Y += teleportRadius;
                        }
                        NPC.netUpdate = true;
                    }
                    else
                    {
                        if (!dashStarted)
                        {
                            dashStarted = true;
                            SoundEngine.PlaySound(SoundID.Roar, (int)NPC.Center.X, (int)NPC.Center.Y, 0);
                            NPC.velocity.X = MathHelper.Pi * teleportRadius / arcTime;
                            NPC.velocity *= rotationDirection;
                            NPC.netUpdate = true;
                        }
                        else
                        {
                            NPC.velocity = NPC.velocity.RotatedBy(MathHelper.Pi / arcTime * -rotationDirection);

                            phase2timer++;
                            if (phase2timer == (int)arcTime / 6)
                            {
                                phase2timer = 0;
                                NPC.ai[0]++;
                                if (Main.netMode != NetmodeID.MultiplayerClient && Collision.CanHit(NPC.Center, 1, 1, player.position, player.width, player.height))
                                {
                                    if (NPC.ai[0] == 2 || NPC.ai[0] == 4)
                                    {
                                        if (expertMode && !NPC.AnyNPCs(ModContent.NPCType<DarkHeart>()))
                                            NPC.NewNPC((int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DarkHeart>());
                                    }
                                    else if (!NPC.AnyNPCs(NPCID.EaterofSouls))
                                        NPC.NewNPC((int)NPC.Center.X, (int)NPC.Center.Y, NPCID.EaterofSouls);
                                }

                                if (NPC.ai[0] == 6)
                                {
                                    NPC.velocity = NPC.velocity.RotatedBy(MathHelper.Pi / arcTime * -rotationDirection);
                                    SpawnStuff();
                                    state = 6;
                                    NPC.ai[0] = 0;
                                    deceleration = NPC.velocity / decelerationTime;
                                }
                            }
                        }
                    }

                    break;

                case 5: // Rain dash

                    if (NPC.alpha > 0)
                    {
                        NPC.alpha -= 5;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.Center = player.Center;
                            NPC.position.Y -= teleportRadius;
                            NPC.position.X += teleportRadius * rotationDirection;
                        }
                        NPC.netUpdate = true;
                    }
                    else
                    {
                        if (!dashStarted)
                        {
                            dashStarted = true;
                            SoundEngine.PlaySound(SoundID.Roar, (int)NPC.Center.X, (int)NPC.Center.Y, 0);
                            NPC.velocity.X = teleportRadius / arcTime * 3;
                            NPC.velocity *= -rotationDirection;
                            NPC.netUpdate = true;
                        }
                        else
                        {
                            phase2timer++;
                            if (phase2timer == (int)arcTime / 20)
                            {
                                phase2timer = 0;
                                NPC.ai[0]++;
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    int type = ModContent.ProjectileType<ShadeNimbusHostile>();
                                    int damage = NPC.GetProjectileDamage(type);
                                    Projectile.NewProjectile(NPC.GetSpawnSource_ForProjectile(), NPC.position.X + Main.rand.Next(NPC.width), NPC.position.Y + Main.rand.Next(NPC.height), 0, 0, type, damage, 0, Main.myPlayer, 11, 0);
                                }

                                if (NPC.ai[0] == 10)
                                {
                                    state = 6;
                                    NPC.ai[0] = 0;
                                    deceleration = NPC.velocity / decelerationTime;
                                }
                            }
                        }
                    }

                    break;

                case 6: // Deceleration

                    NPC.velocity -= deceleration;
                    phase2timer++;
                    if (phase2timer == decelerationTime)
                    {
                        phase2timer = minimumDriftTime + Main.rand.Next(121);
                        state = 0;
                        NPC.netUpdate = true;
                    }

                    break;
            }
        }

        public override bool? CanHitNPC(NPC target) => NPC.alpha == 0; // Can only be hit while fully visible

        // Can only hit the target if within certain distance
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            Rectangle targetHitbox = target.Hitbox;

            float dist1 = Vector2.Distance(NPC.Center, targetHitbox.TopLeft());
            float dist2 = Vector2.Distance(NPC.Center, targetHitbox.TopRight());
            float dist3 = Vector2.Distance(NPC.Center, targetHitbox.BottomLeft());
            float dist4 = Vector2.Distance(NPC.Center, targetHitbox.BottomRight());

            float minDist = dist1;
            if (dist2 < minDist)
                minDist = dist2;
            if (dist3 < minDist)
                minDist = dist3;
            if (dist4 < minDist)
                minDist = dist4;

            return minDist <= 60f && NPC.alpha == 0 && NPC.scale == 1f; // No damage while not fully visible or shrunk
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => NPC.scale == 1f; // Only draw HP bar while at full size

        public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
            if (phase2timer < 0 && damage > 1)
            {
                NPC.velocity *= -4f;
                ReelBack();
                NPC.netUpdate = true;
            }
            return true;
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * bossLifeScale);
            NPC.damage = (int)(NPC.damage * NPC.GetExpertDamageMultiplier());
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            for (int k = 0; k < damage / NPC.lifeMax * 100.0; k++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, 14, hitDirection, -1f, 0, default, 1f);

            // When Hive Mind starts flying around
            bool phase2 = NPC.life / (float)NPC.lifeMax < 0.8f;

            if (phase2)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(15) && NPC.CountNPCS(ModContent.NPCType<HiveBlob2>()) < 2)
                {
                    Vector2 spawnAt = NPC.Center + new Vector2(0f, NPC.height / 2f);
                    NPC.NewNPC((int)spawnAt.X, (int)spawnAt.Y, ModContent.NPCType<HiveBlob2>());
                }
            }
            else
            {
                if (NPC.CountNPCS(NPCID.EaterofSouls) < 3 && NPC.CountNPCS(NPCID.DevourerHead) < 1)
                {
                    if (Main.rand.NextBool(60) && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 spawnAt = NPC.Center + new Vector2(0f, NPC.height / 2f);
                        NPC.NewNPC((int)spawnAt.X, (int)spawnAt.Y, NPCID.EaterofSouls);
                    }

                    if (Main.rand.NextBool(150) && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 spawnAt = NPC.Center + new Vector2(0f, NPC.height / 2f);
                        NPC.NewNPC((int)spawnAt.X, (int)spawnAt.Y, NPCID.DevourerHead);
                    }
                }
            }

            if (NPC.life <= 0)
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    int goreAmount = 10;
                    for (int i = 1; i <= goreAmount; i++)
                        Gore.NewGore(NPC.position, NPC.velocity, Mod.Find<ModGore>("Gores/HiveMindGores/HiveMindP2Gore" + i).Type, 1f);
                }

                NPC.position.X = NPC.position.X + (NPC.width / 2);
                NPC.position.Y = NPC.position.Y + (NPC.height / 2);
                NPC.width = 200;
                NPC.height = 150;
                NPC.position.X = NPC.position.X - (NPC.width / 2);
                NPC.position.Y = NPC.position.Y - (NPC.height / 2);
                for (int num621 = 0; num621 < 40; num621++)
                {
                    int num622 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, 14, 0f, 0f, 100, default, 2f);
                    Main.dust[num622].velocity *= 3f;
                    if (Main.rand.NextBool(2))
                    {
                        Main.dust[num622].scale = 0.5f;
                        Main.dust[num622].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                }
                for (int num623 = 0; num623 < 70; num623++)
                {
                    int num624 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, 14, 0f, 0f, 100, default, 3f);
                    Main.dust[num624].noGravity = true;
                    Main.dust[num624].velocity *= 5f;
                    num624 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, 14, 0f, 0f, 100, default, 2f);
                    Main.dust[num624].velocity *= 2f;
                }
            }
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.HealingPotion;
        }

        public override void NPCLoot()
        {
            CalamityGlobalNPC.SetNewBossJustDowned(NPC);

            DropHelper.DropBags(NPC);

            DropHelper.DropItemChance(NPC, ModContent.ItemType<HiveMindTrophy>(), 10);
            DropHelper.DropItemCondition(NPC, ModContent.ItemType<KnowledgeHiveMind>(), true, !DownedBossSystem.downedHiveMind);

            CalamityGlobalNPC.SetNewShopVariable(new int[] { NPCID.Dryad }, DownedBossSystem.downedHiveMind);

            // All other drops are contained in the bag, so they only drop directly on Normal
            if (!Main.expertMode)
            {
                // Materials
                DropHelper.DropItemSpray(NPC, ModContent.ItemType<TrueShadowScale>(), 25, 30, 5);
                DropHelper.DropItemSpray(NPC, ItemID.DemoniteBar, 8, 12, 2);
                DropHelper.DropItemSpray(NPC, ItemID.RottenChunk, 9, 15, 3);
                if (Main.hardMode)
                    DropHelper.DropItemSpray(NPC, ItemID.CursedFlame, 10, 20, 2);
                DropHelper.DropItem(NPC, ItemID.CorruptSeeds, 10, 15);

                // Weapons
                float w = DropHelper.NormalWeaponDropRateFloat;
                DropHelper.DropEntireWeightedSet(NPC,
                    DropHelper.WeightStack<PerfectDark>(w),
                    DropHelper.WeightStack<LeechingDagger>(w),
                    DropHelper.WeightStack<Shadethrower>(w),
                    DropHelper.WeightStack<ShadowdropStaff>(w),
                    DropHelper.WeightStack<ShaderainStaff>(w),
                    DropHelper.WeightStack<DankStaff>(w),
                    DropHelper.WeightStack<RotBall>(w, 30, 50),
                    DropHelper.WeightStack<FilthyGlove>(w)
                );

                // Equipment
                DropHelper.DropItem(NPC, ModContent.ItemType<RottenBrain>(), true);

                // Vanity
                DropHelper.DropItemChance(NPC, ModContent.ItemType<HiveMindMask>(), 7);
                DropHelper.DropItemChance(NPC, ModContent.ItemType<RottingEyeball>(), 10);
            }

            // If neither The Hive Mind nor The Perforator Hive have been killed yet, notify players of Aerialite Ore
            if (!DownedBossSystem.downedHiveMind && !DownedBossSystem.downedPerforator)
            {
                string key = "Mods.CalamityMod.SkyOreText";
                Color messageColor = Color.Cyan;
                CalamityUtils.SpawnOre(ModContent.TileType<AerialiteOre>(), 12E-05, 0.4f, 0.6f, 3, 8);

                CalamityUtils.DisplayLocalizedText(key, messageColor);
            }

            // Mark The Hive Mind as dead
            DownedBossSystem.downedHiveMind = true;
            CalamityNetcode.SyncWorld();
        }
    }
}
