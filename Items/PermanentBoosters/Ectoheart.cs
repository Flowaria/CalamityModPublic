﻿using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using CalamityMod.CalPlayer;

namespace CalamityMod.Items.PermanentBoosters
{
    public class Ectoheart : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ectoheart");
			Tooltip.SetDefault("Permanently makes Adrenaline Mode take 5 less seconds to charge\n" +
                "Revengeance drop");
            Main.RegisterItemAnimation(item.type, new DrawAnimationVertical(5, 5));
        }

		public override void SetDefaults()
		{
			item.width = 20;
			item.height = 20;
			item.useAnimation = 30;
			item.useTime = 30;
			item.useStyle = 4;
			item.UseSound = SoundID.Item122;
			item.consumable = true;
			item.rare = 10;
			item.GetGlobalItem<CalamityGlobalItem>(mod).postMoonLordRarity = 13;
		}

        public override bool CanUseItem(Player player)
		{
			CalamityPlayer modPlayer = player.GetCalamityPlayer();
			if (modPlayer.adrenalineBoostThree)
			{
				return false;
			}
			return true;
		}

		public override bool UseItem(Player player)
		{
			if (player.itemAnimation > 0 && player.itemTime == 0)
			{
				player.itemTime = item.useTime;
				CalamityPlayer modPlayer = player.GetCalamityPlayer();
				modPlayer.adrenalineBoostThree = true;
			}
			return true;
		}
	}
}
