using Terraria; using CalamityMod.Projectiles; using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ModLoader; using CalamityMod.Buffs; using CalamityMod.Items; using CalamityMod.NPCs; using CalamityMod.Projectiles; using CalamityMod.Tiles; using CalamityMod.Walls;

namespace CalamityMod.Items
{
    public class ClamCrusher : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Clam Crusher");
            Tooltip.SetDefault("Launches a huge clam that stuns enemies for a short amount of time\n" +
                               "Starts being affected by gravity and does much more damage after being airborne for a while");
        }

        public override void SetDefaults()
        {
            item.damage = 140;
            item.melee = true;
            item.width = 40;
            item.height = 60;
            item.useTime = 50;
            item.useAnimation = 50;
            item.useStyle = 5;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.knockBack = 10f;
            item.value = Item.buyPrice(0, 36, 0, 0);
            item.rare = 5;
            item.UseSound = SoundID.Item1;
            item.autoReuse = true;
            item.channel = true;
            item.shoot = ModContent.ProjectileType<ClamCrusherFlail>();
            item.shootSpeed = 18f;
        }
    }
}
