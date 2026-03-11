using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Dusts;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Buffs
{
    /// <summary>
    /// Elysian Mark debuff — stacks up to 3. Each tier increases damage vulnerability.
    /// 3 marks = Elysian Verdict detonation.
    /// </summary>
    public class ElysianMarkDebuff : ModBuff
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
    /// Elysian Burn — 3% max HP/s DoT from tier 2+ marks.
    /// </summary>
    public class ElysianBurnDebuff : ModBuff
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
    /// GlobalNPC handling Elysian Mark tiers and Elysian Burn DoT.
    /// </summary>
    public class ElysianMarkNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        /// <summary>Current mark count (0-3).</summary>
        public int MarkCount;

        /// <summary>Timer to decay marks (600 frames = 10s).</summary>
        public int MarkDecayTimer = 600;

        /// <summary>
        /// Add a mark. Returns true when Elysian Verdict detonation should trigger (reaches 3).
        /// </summary>
        public bool AddMark(NPC npc, bool isCrit)
        {
            int marksToAdd = isCrit ? 2 : 1;
            MarkCount = System.Math.Min(MarkCount + marksToAdd, 3);
            MarkDecayTimer = 600;

            // Apply debuffs based on tier
            npc.AddBuff(ModContent.BuffType<ElysianMarkDebuff>(), 600);

            if (MarkCount >= 2)
                npc.AddBuff(ModContent.BuffType<ElysianBurnDebuff>(), 300);

            return MarkCount >= 3;
        }

        /// <summary>
        /// Reset marks after Verdict detonation.
        /// </summary>
        public void ResetMarks()
        {
            MarkCount = 0;
            MarkDecayTimer = 0;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            // Elysian Burn: 3% max HP/s
            if (npc.HasBuff(ModContent.BuffType<ElysianBurnDebuff>()))
            {
                int burnDmg = (int)(npc.lifeMax * 0.03f);
                npc.lifeRegen -= burnDmg * 2;
                if (damage < burnDmg) damage = burnDmg;
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (!npc.HasBuff(ModContent.BuffType<ElysianMarkDebuff>())) return;

            // Tier damage vulnerability
            float bonus = MarkCount switch
            {
                1 => 1.10f,
                2 => 1.20f,
                _ => 1f
            };
            if (bonus > 1f && projectile.DamageType == DamageClass.Magic)
                modifiers.FinalDamage *= bonus;
        }

        public override void ResetEffects(NPC npc)
        {
            if (MarkCount > 0)
            {
                MarkDecayTimer--;
                if (MarkDecayTimer <= 0)
                {
                    MarkCount = 0;
                }
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (MarkCount <= 0) return;

            // Golden glow intensity scales with marks
            float intensity = MarkCount / 3f;
            drawColor = Color.Lerp(drawColor, new Color(255, 200, 50), intensity * 0.25f);

            if (Main.rand.NextBool(8 - MarkCount * 2))
            {
                Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, ModContent.DustType<ElysianMarkGlowDust>(), 0f, -0.8f, 100,
                    Color.Lerp(new Color(255, 200, 50), new Color(255, 255, 240), intensity), 0.4f + intensity * 0.3f);
                d.noGravity = true;
            }
        }
    }
}
