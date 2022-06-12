﻿using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Potions
{
    public class CadancesGrace : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cadance's Grace");
            Description.SetDefault("Your heart is pure");
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().cadence = true;
        }
    }
}