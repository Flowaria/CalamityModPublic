﻿using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Vanity;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.NPCs.Signus;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.TreasureBags
{
    public class SignusBag : ModItem
    {
        public override int BossBagNPC => ModContent.NPCType<Signus>();

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Treasure Bag");
            Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}");
        }

        public override void SetDefaults()
        {
            Item.maxStack = 999;
            Item.consumable = true;
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Cyan;
            Item.expert = true;
        }

        public override bool CanRightClick() => true;

        public override void OpenBossBag(Player player)
        {
            // IEntitySource my beloathed
            var s = player.GetItemSource_OpenItem(Item.type);

            player.TryGettingDevArmor(s);

            // Materials
            DropHelper.DropItem(s, player, ModContent.ItemType<TwistingNether>(), 3, 4);

            // Weapons
            DropHelper.DropItemChance(s, player, ModContent.ItemType<CosmicKunai>(), DropHelper.BagWeaponDropRateInt);
            DropHelper.DropItemChance(s, player, ModContent.ItemType<Cosmilamp>(), DropHelper.BagWeaponDropRateInt);

            // Equipment
            DropHelper.DropItem(s, player, ModContent.ItemType<SpectralVeil>());

            // Vanity
            DropHelper.DropItemChance(s, player, ModContent.ItemType<SignusMask>(), 7);
            if (Main.rand.NextBool(20))
            {
                DropHelper.DropItem(s, player, ModContent.ItemType<AncientGodSlayerHelm>());
                DropHelper.DropItem(s, player, ModContent.ItemType<AncientGodSlayerChestplate>());
                DropHelper.DropItem(s, player, ModContent.ItemType<AncientGodSlayerLeggings>());
            }
        }
    }
}
