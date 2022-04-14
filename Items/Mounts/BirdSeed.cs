﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace CalamityMod.Items.Mounts
{
    public class BirdSeed : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Folly Feed");
            Tooltip.SetDefault("Summons a monstrosity");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 36;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;

            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ItemRarityID.Purple;
            Item.Calamity().donorItem = true;

            Item.UseSound = SoundID.NPCHit51;
            Item.noMelee = true;
            Item.mountType = ModContent.MountType<BUMBLEDOGE>();
        }
    }
}
