using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Buffs
{
    /// <summary>
    /// Musical Dissonance — debuff inflicted by Moonlight's Calling beams.
    /// Enemies struck by prismatic light experience harmonic disruption:
    /// defense reduction + periodic bursts of prismatic damage.
    /// </summary>
    public class MusicalDissonance : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_24";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    /// <summary>
    /// GlobalNPC that applies Musical Dissonance effects to affected enemies.
    /// </summary>
    public class MusicalDissonanceNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        /// <summary>Tracks accumulated prismatic resonance for burst damage timing.</summary>
        public int ResonanceTimer;

        public override void ResetEffects(NPC npc)
        {
            if (!npc.HasBuff(ModContent.BuffType<MusicalDissonance>()))
                ResonanceTimer = 0;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (!npc.HasBuff(ModContent.BuffType<MusicalDissonance>()))
                return;

            // Defense reduction effect (handled via ModifyHitNPC in the weapon)
            // Periodic resonance damage: 30 DPS base
            npc.lifeRegen -= 60; // 30 DPS
            damage = 15;

            ResonanceTimer++;

            // Every 45 ticks: prismatic resonance burst — visual indicator
            if (ResonanceTimer % 45 == 0)
            {
                // Spawn spectral dust burst around the enemy
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 dustVel = angle.ToRotationVector2() * 3f;
                    int d = Dust.NewDust(npc.position, npc.width, npc.height,
                        ModContent.DustType<Dusts.PrismaticDust>(), dustVel.X, dustVel.Y);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].scale = 1.2f;
                }
            }
        }
    }
}
