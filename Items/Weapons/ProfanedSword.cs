using Microsoft.Xna.Framework;
using Terraria; using CalamityMod.Projectiles; using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ModLoader; using CalamityMod.Buffs; using CalamityMod.Items; using CalamityMod.NPCs; using CalamityMod.Projectiles; using CalamityMod.Tiles; using CalamityMod.Walls;

namespace CalamityMod.Items
{
    public class ProfanedSword : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Brimstone Sword");
            Tooltip.SetDefault("Summons brimstone geysers on hit\n" +
                "Right click to throw like a javelin that explodes on hit");
        }

        public override void SetDefaults()
        {
            item.damage = 61;
            item.melee = true;
            item.width = 42;
            item.height = 50;
            item.useTime = 23;
            item.useAnimation = 23;
            item.useTurn = true;
            item.useStyle = 1;
            item.knockBack = 7.5f;
            item.value = Item.buyPrice(0, 48, 0, 0);
            item.rare = 6;
            item.UseSound = SoundID.Item1;
            item.autoReuse = true;
            item.shootSpeed = 20f;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                item.noMelee = true;
                item.noUseGraphic = true;
                item.shoot = ModContent.ProjectileType<Projectiles.ProfanedSword>();
            }
            else
            {
                item.noMelee = false;
                item.noUseGraphic = false;
                item.shoot = 0;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Projectile.NewProjectile(position.X, position.Y, speedX, speedY, ModContent.ProjectileType<Projectiles.ProfanedSword>(), damage, knockBack, player.whoAmI, 0f, 0f);
            return false;
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 120);
            Projectile.NewProjectile(target.Center.X, target.Center.Y, 0f, 0f, ModContent.ProjectileType<Brimblast>(), (int)((float)item.damage * player.meleeDamage), knockback, Main.myPlayer);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(4))
            {
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 235);
            }
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(null, "UnholyCore", 6);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
