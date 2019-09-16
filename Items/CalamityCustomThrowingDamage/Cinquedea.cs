﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CalamityMod.Items.CalamityCustomThrowingDamage
{
    public class Cinquedea : CalamityDamageItem
    {
        public static int BaseDamage = 16;
        public static float Knockback = 5f;
        public static float Speed = 8f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cinquedea");
            Tooltip.SetDefault("Stealth strikes home in after hitting an enemy");
        }

        public override void SafeSetDefaults()
        {
            item.damage = BaseDamage;
            item.rare = 3;
            item.knockBack = Knockback;
            item.crit = 8;
            item.autoReuse = true;
            item.useTime = 15;
            item.useAnimation = 15;
            item.useStyle = 1;
            item.width = 32;
            item.height = 32;
            item.UseSound = SoundID.Item1;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.shoot = mod.ProjectileType("CinquedeaProj");
            item.shootSpeed = Speed;
            item.value = Item.buyPrice(0, 9, 0, 0);
            item.GetGlobalItem<CalamityGlobalItem>(mod).rogue = true;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.GetCalamityPlayer().StealthStrikeAvailable())
            {
                Projectile.NewProjectile(position, new Vector2(speedX, speedY), mod.ProjectileType("CinquedeaProj"), damage, knockBack, player.whoAmI, 0f, 1f);
                return false;
            }
            return true;
        }
    }
}
