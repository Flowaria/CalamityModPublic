﻿using CalamityMod.CalPlayer;
using CalamityMod.Events;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace CalamityMod.Items.DifficultyItems
{
    public class Death : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            DisplayName.SetDefault("Death");
            Tooltip.SetDefault("Enables/disables Death Mode, can only be used in Revengeance Mode.\n" +

                // Rev Mode line
                "All effects from Revengeance Mode are enabled, including the following:\n" +

                // Biome effect lines
                "Greatly boosts enemy spawn rates during the Blood Moon.\n" +
                "Makes the Abyss more treacherous to navigate.\n" +

                // General enemy lines
                "Certain non-boss enemies and projectiles deal between 6% and 15% more damage.\n" +

                // Misc lines
                "Nerfs the effectiveness of life steal a bit more.\n" +
                "The Nurse no longer heals you while a boss is alive.\n" +
                "Increases damage done by 50% for several debuffs and all alcohols that reduce life regen.\n" +

                // Boss lines
                "Changes all boss AIs and most enemy AIs.\n" +
                "Increases the damage of all bosses a bit more.");
        }

        public override void SetDefaults()
        {
            Item.expert = true;
            Item.rare = ItemRarityID.Purple;
            Item.width = 28;
            Item.height = 28;
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = SoundID.Item119;
            Item.consumable = false;
        }

        // Can only be used in Revengeance Mode.
        public override bool CanUseItem(Player player) => CalamityWorld.revenge || CalamityWorld.death;

        public override bool? UseItem(Player player)
        {
            // This world syncing code should only be run by one entity- the server, to prevent a race condition
            // with the packets.
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return true;

            if (CalamityPlayer.areThereAnyDamnBosses || CalamityWorld.DoGSecondStageCountdown > 0 || BossRushEvent.BossRushActive)
            {
                string key = "Mods.CalamityMod.ChangingTheRules";
                Color messageColor = Color.Crimson;
                CalamityUtils.DisplayLocalizedText(key, messageColor);
                return true;
            }
            if (!CalamityWorld.death)
            {
                CalamityWorld.death = true;
                string key = "Mods.CalamityMod.DeathText";
                Color messageColor = Color.Crimson;
                CalamityUtils.DisplayLocalizedText(key, messageColor);
            }
            else
            {
                CalamityWorld.death = false;
                string key = "Mods.CalamityMod.DeathText2";
                Color messageColor = Color.Crimson;
                CalamityUtils.DisplayLocalizedText(key, messageColor);

                if (CalamityWorld.malice)
                {
                    CalamityWorld.malice = false;
                    key = "Mods.CalamityMod.MaliceText2";
                    messageColor = Color.Crimson;
                    CalamityUtils.DisplayLocalizedText(key, messageColor);
                }
            }
            CalamityWorld.DoGSecondStageCountdown = 0;
            CalamityNetcode.SyncWorld();

            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddTile(TileID.DemonAltar)
                .Register();
        }
    }
}
