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
        //TODO: Toggle to set spawnrates to normal when a boss is spawned

		public override ConfigScope Mode => ConfigScope.ServerSide;

        [DefaultValue(1)]
        [Range(1, 1000)]
        [LabelKey("$Mods.CustomSpawnRate.Configs.Common.SpawnRateLabel")]
        [TooltipKey("$Mods.CustomSpawnRate.Configs.Common.SpawnRateTooltip")]
        public int SpawnRate;

        [DefaultValue(false)]
        [LabelKey("$Mods.CustomSpawnRate.Configs.Common.DisableOnBossLabel")]
        [TooltipKey("$Mods.CustomSpawnRate.Configs.Common.DisableOnBossTooltip")]
        public bool DisableOnBoss;

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
            if (config.DisableOnBoss) // If disableOnBoss is true, don't alter spawnRate and maxSpawns.
            {
                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && npc.boss)
                    {
                        return; // A boss is alive, return.
                    }
                }
            }

            // We're setting the max spawns here to prevent it being wrong due to setting it later overwriting the current value.
            maxSpawns = spawnRate * 5;

            // This adds the Calming/Battle potion multiplier. Priority is Battle > Calming > Neither.
            double calmingOrBattle = (player.HasBuff(13) ? 2 : player.HasBuff(106) ? 0.5 : 1);

            /* 
             * Set spawn rate.
             * We're dividing here because of how spawn rate works, by example, a spawn rate of 600 actually means 1/600 (~0.17%) chance for a new enemy to spawn.
             * E.G. Spawn rate of 5, on pre-hardmode forest (600) = 600 / (5 * calmingOrBattle), 
             * which returns 120 w/o Calming/Battle buffs, 60 w/ Battle buff, and 240 w/ Calming buff.
            */
            spawnRate /= System.Math.Max((int)System.Math.Floor(config.SpawnRate * calmingOrBattle), 1); // Since the return value is an integer, make sure to have a whole number.

        }
    }
}