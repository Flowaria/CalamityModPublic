﻿using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace CalamityMod.Items.Armor
{
    [AutoloadEquip(EquipType.Head)]
    public class ReaverHelm : ModItem
    {
        //Defense and DR Helm
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Reaver Helm");
            Tooltip.SetDefault("15% increased damage reduction but 30% decreased damage\n" +
                "+50 max life\n" +
                "Passively regenerates one health point every second");
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.value = Item.buyPrice(0, 30, 0, 0);
            Item.rare = ItemRarityID.Lime;
            Item.defense = 30; //63 => 73 w/ set bonus (+5 w/ Reaver Rage)
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<ReaverScaleMail>() && legs.type == ModContent.ItemType<ReaverCuisses>();
        }

        public override void ArmorSetShadows(Player player)
        {
            player.armorEffectDrawShadowSubtle = true;
            player.armorEffectDrawOutlines = true;
        }

        public override void UpdateArmorSet(Player player)
        {
            CalamityPlayer modPlayer = player.Calamity();
            player.thorns += 0.33f;
            player.moveSpeed -= 0.2f;
            player.statDefense += 10;
            player.lifeRegen += 3;
            player.aggro += 600;
            modPlayer.reaverDefense = true;
            modPlayer.wearingRogueArmor = true;
            player.setBonus = "+10 defense and +3 life regen\n" +
            "Enemies are more likely to target you\n" +
            "Reduces the life regen lost from damage over time debuffs by 20%\n" +
            "All attacks have a small chance to steal life and speed up the rate of life regen\n" +
            "20% decreased movement speed and flight time\n" +
            "Enemy damage is reflected and summons a thorn spike\n" +
            "Reaver Rage has a 25% chance to activate when you are damaged";
            //Reaver Rage provides 30% damage to offset the helm "bonus", 5 def, and 5% melee speed.
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<GenericDamageClass>() -= 0.3f;
            player.endurance += 0.15f;
            player.Calamity().reaverRegen = true;
            player.statLifeMax2 += 50;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<DraedonBar>(6).
                AddIngredient(ItemID.JungleSpores, 4).
                AddIngredient<EssenceofCinder>().
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
