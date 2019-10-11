using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.SummonBuffs
{
    public class BlackHawkBuff : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Black Hawk");
            Description.SetDefault("The fighter jet will protect you");
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            CalamityPlayer modPlayer = player.Calamity();
            if (player.ownedProjectileCounts[mod.ProjectileType("BlackHawkSummon")] > 0)
            {
                modPlayer.blackhawk = true;
            }
            if (!modPlayer.blackhawk)
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
            else
            {
                player.buffTime[buffIndex] = 18000;
            }
        }
    }
}
