﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace CalamityMod.Items.Armor
{
    [AutoloadEquip(EquipType.Legs)]
    public class SnowRuffianGreaves : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Snow Ruffian Greaves");
            Tooltip.SetDefault("5% increased movement speed");
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.buyPrice(0, 0, 75, 0);
            Item.rare = ItemRarityID.Blue;
            Item.defense = 1; //4
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.05f;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddRecipeGroup("AnySnowBlock", 20).
                AddRecipeGroup("AnyIceBlock", 10).
                AddIngredient(ItemID.BorealWood, 30).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
