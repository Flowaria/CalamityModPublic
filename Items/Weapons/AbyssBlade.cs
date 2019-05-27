using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons 
{
	public class AbyssBlade : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Abyss Blade");
			Tooltip.SetDefault("Hitting enemies will cause the crush depth debuff\n" +
				"The lower the enemies' defense the more damage they take from this debuff\n" +
				"Fires short-range water orbs");
		}

		public override void SetDefaults()
		{
			item.width = 60;
			item.damage = 110;
			item.melee = true;
			item.useAnimation = 17;
			item.useTime = 17;
			item.useTurn = true;
			item.useStyle = 1;
			item.knockBack = 8f;
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;
			item.height = 60;
            item.value = Item.buyPrice(0, 60, 0, 0);
            item.rare = 7;
			item.shoot = mod.ProjectileType("DepthOrb");
			item.shootSpeed = 9f;
		}
		
		public override void AddRecipes()
	    {
	        ModRecipe recipe = new ModRecipe(mod);
	        recipe.AddIngredient(null, "DepthBlade");
	        recipe.AddIngredient(ItemID.BrokenHeroSword);
            recipe.AddIngredient(null, "DepthCells", 15);
            recipe.AddIngredient(null, "Lumenite", 10);
            recipe.AddIngredient(null, "Tenebris", 5);
            recipe.AddTile(TileID.MythrilAnvil);
	        recipe.SetResult(this);
	        recipe.AddRecipe();
	    }
	
	    public override void MeleeEffects(Player player, Rectangle hitbox)
	    {
	        if (Main.rand.Next(3) == 0)
	        {
	        	int dust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 33);
	        }
	    }
	    
	    public override void OnHitNPC(Player player, NPC target, int damage, float knockback, bool crit)
	    {
	    	target.AddBuff(mod.BuffType("CrushDepth"), 300);
		}
	}
}
