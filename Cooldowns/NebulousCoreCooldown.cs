﻿using Microsoft.Xna.Framework;
using Terraria;

namespace CalamityMod.Cooldowns
{
    public class NebulousCoreCooldown : Cooldown
    {
        public override bool ShouldDisplay => true;
        public override string DisplayName => "Nebulous Core Cooldown";
        public override string Texture => "CalamityMod/UI/CooldownIndicators/NebulousCore";
        public override Color OutlineColor => Color.Lerp(new Color(252, 109, 203), new Color(58, 91, 146), Completion);
        public override Color CooldownStartColor => new Color(148, 62, 216);
        public override Color CooldownEndColor => new Color(255, 187, 207);

        public NebulousCoreCooldown(int duration, Player player) : base(duration, player)
        {
        }
    }
}