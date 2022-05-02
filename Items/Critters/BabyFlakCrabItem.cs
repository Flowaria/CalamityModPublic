﻿using CalamityMod.NPCs.AcidRain;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;

namespace CalamityMod.Items.Critters
{
    public class BabyFlakCrabItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 5;
            DisplayName.SetDefault("Baby Flak Crab");
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.maxStack = 999;
            Item.consumable = true;
            Item.noUseGraphic = true;
            Item.value = Item.buyPrice(0, 0, 30, 0);
            //item.CloneDefaults(2004); //Lightning Bug item
            Item.width = 26;
            Item.height = 24;
            Item.makeNPC = (short)ModContent.NPCType<BabyFlakCrab>();
            Item.rare = ItemRarityID.LightPurple;
        }
    }
}