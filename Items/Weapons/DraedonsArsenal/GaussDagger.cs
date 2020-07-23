using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.DraedonsArsenal;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.DraedonsArsenal
{
    public class GaussDagger : ModItem
    {
        public const int HitsRequiredForFlux = 3;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Gauss Dagger");
            Tooltip.SetDefault("Hitting enemies in rapid succession causes them to become enveloped in a magnetic flux");
        }
        public override void SetDefaults()
        {
            item.damage = 30;
            item.melee = true;
            item.width = 26;
            item.height = 26;
            item.useTime = 24;
            item.useAnimation = 24;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.useTurn = false;
            item.knockBack = 7f;

            item.value = CalamityGlobalItem.Rarity3BuyPrice;
            item.rare = ItemRarityID.Red;
            item.Calamity().customRarity = CalamityRarity.DraedonRust;

            item.UseSound = SoundID.Item1;
            item.autoReuse = true;

            item.Calamity().Chargeable = true;
            item.Calamity().ChargeMax = 50;
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            target.Calamity().GaussFluxTimer += 50;
            if (target.Calamity().GaussFluxTimer >= 30 * HitsRequiredForFlux)
            {
                target.Calamity().GaussFluxTimer = 0;
                if (player.whoAmI == Main.myPlayer)
                {
                    Projectile.NewProjectile(target.Center, Vector2.Zero, ModContent.ProjectileType<GaussFlux>(), damage, 0f, player.whoAmI, 0f, target.whoAmI);
                }
            }
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<MysteriousCircuitry>(), 5);
            recipe.AddIngredient(ModContent.ItemType<DubiousPlating>(), 7);
            recipe.AddIngredient(ModContent.ItemType<AerialiteBar>(), 10);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
