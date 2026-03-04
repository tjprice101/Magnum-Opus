using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Buffs
{
    /// <summary>
    /// Thorn Accumulation debuff — stacking system:
    /// 10 stacks: Thorn Bleed DoT (2% weapon damage/s)
    /// 15 stacks: 20% movement slow
    /// 20 stacks: +15% damage taken from all sources
    /// 25 stacks: Thorn Detonation — massive burst, resets stacks
    /// </summary>
    public class ThornAccumulationDebuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_20";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    public class ThornAccumulationNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int ThornStacks;
        public int StackRefreshTimer;
        private bool _detonated;

        public override void ResetEffects(NPC npc)
        {
            if (!npc.HasBuff(ModContent.BuffType<ThornAccumulationDebuff>()))
            {
                ThornStacks = 0;
                _detonated = false;
            }
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (!npc.HasBuff(ModContent.BuffType<ThornAccumulationDebuff>())) return;

            // 10+ stacks: Thorn Bleed
            if (ThornStacks >= 10)
            {
                int bleedDamage = (int)(npc.lifeMax * 0.02f / 60f * 2f);
                if (bleedDamage < 1) bleedDamage = 1;
                npc.lifeRegen -= bleedDamage * 2;
                if (damage < bleedDamage) damage = bleedDamage;
            }

            // 15+ stacks: movement slow
            if (ThornStacks >= 15)
            {
                npc.velocity *= 0.8f;
            }
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            // 20+ stacks: +15% damage vulnerability
            if (ThornStacks >= 20 && npc.HasBuff(ModContent.BuffType<ThornAccumulationDebuff>()))
            {
                modifiers.FinalDamage += 0.15f;
            }
        }

        /// <summary>
        /// Add a thorn stack. Returns true if detonation threshold reached (25).
        /// </summary>
        public bool AddStack(NPC npc)
        {
            ThornStacks++;
            if (ThornStacks > 25) ThornStacks = 25;
            StackRefreshTimer = 300; // 5s duration per refresh
            npc.AddBuff(ModContent.BuffType<ThornAccumulationDebuff>(), 300);

            // Visual: thorn embed dust
            for (int i = 0; i < 3; i++)
            {
                Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.WoodFurniture, 0f, 0f, 100, ThornSprayTextures.RadiantAmber, 0.8f);
                d.noGravity = true;
                d.velocity *= 0.5f;
            }

            // 25 stacks: detonation!
            if (ThornStacks >= 25 && !_detonated)
            {
                _detonated = true;
                ThornStacks = 0;
                return true;
            }
            return false;
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (!npc.HasBuff(ModContent.BuffType<ThornAccumulationDebuff>())) return;

            // Glow intensity based on stacks
            float intensity = ThornStacks / 25f;
            Color glowColor = Color.Lerp(ThornSprayTextures.PetalPink, ThornSprayTextures.BloomGold, intensity);
            drawColor = Color.Lerp(drawColor, glowColor, intensity * 0.3f);

            // Ambient thorn dust
            if (Main.rand.NextBool(8 - (ThornStacks / 4)))
            {
                Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.GoldFlame, 0f, -1f, 150, default, 0.5f + intensity * 0.5f);
                d.noGravity = true;
            }
        }
    }
}
