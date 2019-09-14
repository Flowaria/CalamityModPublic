﻿using Terraria;
using Terraria.ModLoader;
using CalamityMod.CalPlayer;

namespace CalamityMod.Items.LoreItems
{
    public class KnowledgeYharon : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Jungle Dragon, Yharon");
			Tooltip.SetDefault("I would not be able to bear a world without my faithful companion by my side.\n" +
				"Fortunately, fate will have it so that it is a world I shall never have to see, for better or for worse.\n" +
				"Place in your inventory to gain nearly-infinite wing flight time but at the cost of a 25% decrease to all damage.");
		}

		public override void SetDefaults()
		{
			item.width = 20;
			item.height = 20;
			item.rare = 10;
			item.consumable = false;
			item.GetGlobalItem<CalamityGlobalItem>(mod).postMoonLordRarity = 14;
		}

		public override bool CanUseItem(Player player)
		{
			return false;
		}

		public override void UpdateInventory(Player player)
		{
			CalamityPlayer modPlayer = player.GetCalamityPlayer();
			modPlayer.yharonLore = true;
		}
	}
}
