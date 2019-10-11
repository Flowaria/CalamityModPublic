using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Plaguebringer
{
    public class MepheticSprayer : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Blight Spewer");
        }

        public override void SetDefaults()
        {
            item.damage = 110;
            item.ranged = true;
            item.width = 76;
            item.height = 36;
            item.useTime = 10;
            item.useAnimation = 30;
            item.useStyle = 5;
            item.noMelee = true;
            item.knockBack = 2f;
            item.UseSound = SoundID.Item34;
            item.value = Item.buyPrice(0, 80, 0, 0);
            item.rare = 8;
            item.autoReuse = true;
            item.shoot = mod.ProjectileType("CorossiveFlames");
            item.shootSpeed = 7.5f;
            item.useAmmo = 23;
        }
    }
}
