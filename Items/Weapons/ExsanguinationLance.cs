using Terraria; using CalamityMod.Projectiles; using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ModLoader; using CalamityMod.Buffs; using CalamityMod.Items; using CalamityMod.NPCs; using CalamityMod.Projectiles; using CalamityMod.Tiles; using CalamityMod.Walls;

namespace CalamityMod.Items
{
    public class ExsanguinationLance : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Exsanguination Lance");
            Tooltip.SetDefault("Explodes on enemy hits and summons homing flares on critical hits");
        }

        public override void SetDefaults()
        {
            item.width = 44;
            item.damage = 77;
            item.melee = true;
            item.noMelee = true;
            item.useTurn = true;
            item.noUseGraphic = true;
            item.useAnimation = 22;
            item.useStyle = 5;
            item.useTime = 22;
            item.knockBack = 6.75f;
            item.UseSound = SoundID.Item1;
            item.autoReuse = true;
            item.height = 44;
            item.value = Item.buyPrice(0, 80, 0, 0);
            item.rare = 8;
            item.shoot = ModContent.ProjectileType<ExsanguinationLanceProjectile>();
            item.shootSpeed = 8f;
        }

        public override bool CanUseItem(Player player)
        {
            for (int i = 0; i < 1000; ++i)
            {
                if (Main.projectile[i].active && Main.projectile[i].owner == Main.myPlayer && Main.projectile[i].type == item.shoot)
                {
                    return false;
                }
            }
            return true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(null, "CruptixBar", 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
