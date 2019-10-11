using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons
{
    public class ElementalExcalibur : ModItem
    {
        private static int BaseDamage = 10000;
        private int BeamType = 0;
        private int alpha = 50;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Elemental Excalibur");
            Tooltip.SetDefault("Freezes enemies and heals the player on hit\n" +
                "Fires rainbow beams that change their behavior based on their color");
        }

        public override void SetDefaults()
        {
            item.damage = BaseDamage;
            item.crit += 10;
            item.useAnimation = 14;
            item.useStyle = 1;
            item.useTime = 14;
            item.useTurn = true;
            item.melee = true;
            item.knockBack = 8f;
            item.UseSound = SoundID.Item1;
            item.autoReuse = true;
            item.width = 92;
            item.height = 92;
            item.value = Item.buyPrice(5, 0, 0, 0);
            item.rare = 10;
            item.shoot = mod.ProjectileType("ElementalExcaliburBeam");
            item.shootSpeed = 12f;
            item.Calamity().postMoonLordRarity = 20;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Vector2 origin = new Vector2(46f, 44f);
            spriteBatch.Draw(mod.GetTexture("Items/Weapons/ElementalExcaliburGlow"), item.Center - Main.screenPosition, null, Color.White, rotation, origin, 1f, SpriteEffects.None, 0f);
        }

        public override bool Shoot(Player player, ref Microsoft.Xna.Framework.Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Projectile.NewProjectile(position.X, position.Y, speedX, speedY, type, damage, knockBack, player.whoAmI, (float)BeamType, 0f);

            BeamType++;
            if (BeamType > 11)
                BeamType = 0;

            return false;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(4))
            {
                Color color = new Color(255, 0, 0, alpha);
                switch (BeamType)
                {
                    case 0: // Red
                        break;
                    case 1: // Orange
                        color = new Color(255, 128, 0, alpha);
                        break;
                    case 2: // Yellow
                        color = new Color(255, 255, 0, alpha);
                        break;
                    case 3: // Lime
                        color = new Color(128, 255, 0, alpha);
                        break;
                    case 4: // Green
                        color = new Color(0, 255, 0, alpha);
                        break;
                    case 5: // Turquoise
                        color = new Color(0, 255, 128, alpha);
                        break;
                    case 6: // Cyan
                        color = new Color(0, 255, 255, alpha);
                        break;
                    case 7: // Light Blue
                        color = new Color(0, 128, 255, alpha);
                        break;
                    case 8: // Blue
                        color = new Color(0, 0, 255, alpha);
                        break;
                    case 9: // Purple
                        color = new Color(128, 0, 255, alpha);
                        break;
                    case 10: // Fuschia
                        color = new Color(255, 0, 255, alpha);
                        break;
                    case 11: // Hot Pink
                        color = new Color(255, 0, 128, alpha);
                        break;
                    default:
                        break;
                }

                Dust dust24 = Main.dust[Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 267, 0f, 0f, alpha, color, 1.2f)];
                dust24.noGravity = true;
            }
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(mod.BuffType("ExoFreeze"), 60);
            target.AddBuff(mod.BuffType("BrimstoneFlames"), 240);
            target.AddBuff(mod.BuffType("GlacialState"), 240);
            target.AddBuff(mod.BuffType("Plague"), 240);
            target.AddBuff(mod.BuffType("HolyLight"), 240);
            target.AddBuff(BuffID.CursedInferno, 240);
            target.AddBuff(BuffID.Frostburn, 240);
            target.AddBuff(BuffID.OnFire, 240);
            target.AddBuff(BuffID.Ichor, 240);
            if (target.type == NPCID.TargetDummy || !target.canGhostHeal)
            {
                return;
            }
            int healAmount = (Main.rand.Next(10) + 10);
            player.statLife += healAmount;
            player.HealEffect(healAmount);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(null, "GreatswordofBlah");
            recipe.AddIngredient(ItemID.TrueExcalibur);
            recipe.AddIngredient(ItemID.LargeDiamond, 3);
            recipe.AddIngredient(ItemID.LightShard, 10);
            recipe.AddIngredient(ItemID.DarkShard, 10);
            recipe.AddIngredient(null, "LivingShard", 10);
            recipe.AddIngredient(null, "GalacticaSingularity", 10);
            recipe.AddIngredient(null, "NightmareFuel", 10);
            recipe.AddIngredient(null, "EndothermicEnergy", 10);
            recipe.AddIngredient(null, "CalamitousEssence", 10);
            recipe.AddIngredient(null, "DarksunFragment", 10);
            recipe.AddIngredient(null, "HellcasterFragment", 10);
            recipe.AddIngredient(ItemID.SoulofLight, 50);
            recipe.AddIngredient(ItemID.SoulofNight, 50);
            recipe.AddTile(null, "DraedonsForge");
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
