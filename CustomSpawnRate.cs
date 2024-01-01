using System;
using System.ComponentModel;
using System.Drawing.Text;
using System.Security.Cryptography.X509Certificates;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace CustomSpawnRate
{
	public class CustomSpawnRate : Mod
	{

	}

	public class ModConfigSpawnRates : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

        [DefaultValue(1)]
        [Range(1, 10000)]
        [LabelKey("$Mods.CustomSpawnRate.Configs.Common.SpawnRateLabel")]
        [TooltipKey("$Mods.CustomSpawnRate.Configs.Common.SpawnRateTooltip")]
        public int SpawnRate;

        [DefaultValue(false)]
        [LabelKey("$Mods.CustomSpawnRate.Configs.Common.DisableOnBossLabel")]
        [TooltipKey("$Mods.CustomSpawnRate.Configs.Common.DisableOnBossTooltip")]
        public bool DisableOnBoss;

        [DefaultValue(false)]
        [LabelKey("$Mods.CustomSpawnRate.Configs.Common.CustomMaxSpawnsToggleLabel")]
        [TooltipKey("$Mods.CustomSpawnRate.Configs.Common.CustomMaxSpawnsToggleTooltip")]
        public bool CustomMaxSpawnsToggle;

        [DefaultValue(5)]
        [Range(1, 10000)]
        [LabelKey("$Mods.CustomSpawnRate.Configs.Common.CustomMaxSpawnsLabel")]
        [TooltipKey("$Mods.CustomSpawnRate.Configs.Common.CustomMaxSpawnsTooltip")]
        public int CustomMaxSpawns;

        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message)
        {
            if (!NetMessage.DoesPlayerSlotCountAsAHost(whoAmI)) // Player is not host
            {
                message = NetworkText.FromKey("tModLoader.ModConfigRejectChangesNotHost");
                return false;
            }

            return true;
        }
    }

	public class GlobalNPCRateModifier : GlobalNPC
	{
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            ModConfigSpawnRates config = ModContent.GetInstance<ModConfigSpawnRates>(); // Get the mod's config.

            // TODO: Optimize following code
            if (config.DisableOnBoss) // If disableOnBoss is true, check if a boss is alive.
            {
                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && npc.boss)
                    {
                        return; // A boss is alive, don't execute the following code.
                    }
                }
            }

            // We're setting the max spawns here to prevent it being wrong due to setting it later overwriting the current value.
           maxSpawns = (config.CustomMaxSpawnsToggle ? config.CustomMaxSpawns : spawnRate * config.SpawnRate); // If the user has enabled custom max spawns, use that value instead

            /* 
             * This would add the Calming/Battle potion multiplier. Priority was Battle > Calming > Neither.
             * However, Terraria does this with the EditSpawnRate() call, rendering this useless (it multiplies enemy spawns by 4x | .25x instead of 2x | 0.5x).
             double calmingOrBattle = (player.HasBuff(13) ? 2 : player.HasBuff(106) ? 0.5 : 1); // Battle Buff ID is 13, Calming Buff ID is 106.
            */


            /* 
             * Set spawn rate.
             * We're dividing here because of how spawn rate works, by example, a spawn rate of 600 actually means 1/600 (~0.17%) chance for a new enemy to spawn.
             * E.G. Spawn rate of 5, on pre-hardmode forest (600) = 600 / 5, 
             * which returns 120 w/o Calming/Battle buffs, 60 w/ Battle buff, and 240 w/ Calming buff.
            */
            spawnRate /= System.Math.Max(config.SpawnRate, 1); // Since the return value is an integer, make sure to have a whole number.
        }
    }
}
