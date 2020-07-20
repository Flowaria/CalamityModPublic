using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.DraedonsArsenal;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.DraedonsArsenal
{
    public class HydraulicVoltCrasher : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hydraulic Volt Crasher");
            Tooltip.SetDefault("Zaps nearby enemies on hit");
        }

        public override void SetDefaults()
        {
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useAnimation = 16;
            item.useTime = 4;
            item.shootSpeed = 46f;
            item.knockBack = 12f;
            item.width = 56;
            item.height = 24;
            item.damage = 99;
            item.hammer = 230;
            item.UseSound = SoundID.Item23;

            item.shoot = ModContent.ProjectileType<HydraulicVoltCrasherProjectile>();
            item.rare = 5;
            item.value = CalamityGlobalItem.Rarity5BuyPrice;

            item.noMelee = true;
            item.noUseGraphic = true;
            item.melee = true;
            item.channel = true;

            item.Calamity().Chargeable = true;
            item.Calamity().ChargeMax = 85;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<MysteriousCircuitry>(), 10);
            recipe.AddIngredient(ModContent.ItemType<DubiousPlating>(), 10);
            recipe.AddIngredient(ItemID.HallowedBar, 5);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
