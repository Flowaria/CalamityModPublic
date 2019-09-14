﻿using Terraria;
using Terraria.ModLoader;
using CalamityMod.CalPlayer;

namespace CalamityMod.Buffs.Fabsol
{
    public class Fireball : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Fireball");
			Description.SetDefault("Fire weapon damage boosted, life regen reduced");
			Main.debuff[Type] = true;
			Main.pvpBuff[Type] = true;
			Main.buffNoSave[Type] = false;
			longerExpertDebuff = false;
            canBeCleared = false;
        }

		public override void Update(Player player, ref int buffIndex)
		{
			player.GetCalamityPlayer().fireball = true;
		}
	}
}
