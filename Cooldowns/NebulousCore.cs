﻿using Microsoft.Xna.Framework;

namespace CalamityMod.Cooldowns
{
	public class NebulousCore : CooldownHandler
    {
        public static new string ID => "NebulousCore";
        public override bool ShouldDisplay => true;
        public override string DisplayName => "Nebulous Core Cooldown";
        public override string Texture => "CalamityMod/Cooldowns/NebulousCore";
        public override Color OutlineColor => Color.Lerp(new Color(252, 109, 203), new Color(58, 91, 146), instance.Completion);
        public override Color CooldownStartColor => new Color(148, 62, 216);
        public override Color CooldownEndColor => new Color(255, 187, 207);
    }
}