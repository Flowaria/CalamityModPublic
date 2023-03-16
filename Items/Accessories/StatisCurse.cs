﻿using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace CalamityMod.Items.Accessories
{
    [AutoloadEquip(EquipType.Neck)]
    public class StatisCurse : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
            DisplayName.SetDefault("Statis' Curse");
            Tooltip.SetDefault("Increases max minions by 3, does not stack with downgrades\n" +
                "10% increased minion damage\n" +
                "Increased minion knockback\n" +
                "Minions inflict holy flames and shadowflames on hit\n" +
                "Grants immunity to Shadowflame");
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(8, 4));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 32;
            Item.value = CalamityGlobalItem.Rarity10BuyPrice;
            Item.rare = ItemRarityID.Red;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.shadowMinions = true;
            modPlayer.holyMinions = true;
            player.GetKnockback<SummonDamageClass>() += 2.75f;
            player.GetDamage<SummonDamageClass>() += 0.1f;
            player.buffImmune[ModContent.BuffType<Shadowflame>()] = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<StatisBlessing>().
                AddIngredient<TheFirstShadowflame>().
                AddIngredient(ItemID.FragmentStardust, 10).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            CalamityUtils.DrawInventoryCustomScale(
                spriteBatch,
                texture: TextureAssets.Item[Type].Value,
                position,
                frame,
                drawColor,
                itemColor,
                origin,
                scale,
                wantedScale: 1f,
                drawOffset: new(-1f, 0f)
            );
            return false;
        }
    }
}
