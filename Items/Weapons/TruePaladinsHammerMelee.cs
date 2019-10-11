using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons
{
    public class TruePaladinsHammerMelee : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fallen Paladin's Hammer");
            Tooltip.SetDefault("Explodes on enemy hits");
        }

        public override void SetDefaults()
        {
            item.width = 14;
            item.damage = 160;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.autoReuse = true;
            item.useAnimation = 13;
            item.useStyle = 1;
            item.useTime = 13;
            item.knockBack = 20f;
            item.UseSound = SoundID.Item1;
            item.melee = true;
            item.height = 28;
            item.value = Item.buyPrice(0, 80, 0, 0);
            item.rare = 8;
            item.shoot = mod.ProjectileType("OPHammerMelee");
            item.shootSpeed = 14f;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.PaladinsHammer);
            recipe.AddIngredient(null, "CalamityDust", 5);
            recipe.AddIngredient(null, "CoreofChaos", 5);
            recipe.AddIngredient(null, "CruptixBar", 5);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
