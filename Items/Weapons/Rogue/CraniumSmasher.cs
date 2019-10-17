using CalamityMod.Projectiles;
using Terraria; using CalamityMod.Projectiles; using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items
{
    public class CraniumSmasher : RogueWeapon
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cranium Smasher");
            Tooltip.SetDefault("Throws disks that roll on the ground, occasionally launches an explosive disk");
        }

        public override void SafeSetDefaults()
        {
            item.width = 50;
            item.height = 50;
            item.damage = 140;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useStyle = 1;
            item.useTime = 15;
            item.knockBack = 4f;
            item.UseSound = SoundID.Item1;
            item.value = Item.buyPrice(0, 80, 0, 0);
            item.rare = 8;
            item.shoot = ModContent.ProjectileType<Projectiles.CraniumSmasher>();
            item.shootSpeed = 20f;
            item.Calamity().rogue = true;
        }

        public override bool Shoot(Player player, ref Microsoft.Xna.Framework.Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (Main.rand.Next(0, 5) == 0)
            {
                damage = (int)(damage * 1.25f);
                type = ModContent.ProjectileType<CraniumSmasherExplosive>();
            }
            else
            {
                type = ModContent.ProjectileType<Projectiles.CraniumSmasher>();
            }
            Projectile.NewProjectile(position.X, position.Y, speedX, speedY, type, damage, knockBack, player.whoAmI, 0.0f, 0.0f);
            return false;
        }
    }
}
