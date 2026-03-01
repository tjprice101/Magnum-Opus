using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Buffs
{
    /// <summary>
    /// Lunar Impact — debuff inflicted by Resurrection of the Moon projectiles.
    /// Enemies struck by comet rounds experience celestial fracture:
    /// defense reduction + periodic lunar damage bursts.
    /// </summary>
    public class LunarImpact : ModBuff
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
    /// GlobalNPC that applies Lunar Impact effects to affected enemies.
    /// </summary>
    public class LunarImpactNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        /// <summary>Tracks accumulated impact resonance for burst damage timing.</summary>
        public int ImpactTimer;

        /// <summary>Stacking impact intensity — increases with multiple hits.</summary>
        public int ImpactStacks;

        /// <summary>Maximum impact stacks.</summary>
        public const int MaxStacks = 5;

        public override void ResetEffects(NPC npc)
        {
            if (!npc.HasBuff(ModContent.BuffType<LunarImpact>()))
            {
                ImpactTimer = 0;
                ImpactStacks = 0;
            }
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (!npc.HasBuff(ModContent.BuffType<LunarImpact>()))
                return;

            // Base: 40 DPS scaling with stacks — up to 200 DPS at max stacks
            int dps = 40 + (ImpactStacks * 32);
            npc.lifeRegen -= dps * 2;
            damage = dps / 2;

            ImpactTimer++;

            // Every 30 ticks: lunar fracture burst — visual indicator
            if (ImpactTimer % 30 == 0)
            {
                // Spawn comet dust burst around the enemy
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 dustVel = angle.ToRotationVector2() * 4f;
                    int d = Dust.NewDust(npc.position, npc.width, npc.height,
                        ModContent.DustType<Dusts.CometDust>(), dustVel.X, dustVel.Y);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].scale = 1.4f;
                }
            }
        }

        /// <summary>Add a stack of Lunar Impact (called on each hit).</summary>
        public void AddStack()
        {
            ImpactStacks = System.Math.Min(ImpactStacks + 1, MaxStacks);
        }
    }
}
