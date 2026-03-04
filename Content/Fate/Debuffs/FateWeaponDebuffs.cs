using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.Debuffs
{
    /// <summary>
    /// RealityFrayed — Applied while inside Requiem of Reality's reality tear zone.
    /// DoT: 15 damage per second. Visual: chromatic sparks shedding from enemy.
    /// </summary>
    public class RealityFrayed : ModBuff
    {
        public const int DamagePerSecond = 15;

        public override string Texture => "Terraria/Images/Buff_" + BuffID.Obstructed;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // DoT every 30 frames (twice per second, so 7-8 per tick = ~15/s)
            if (Main.GameUpdateCount % 30 == 0)
            {
                npc.SimpleStrikeNPC(DamagePerSecond / 2, 0, false, 0f, null, false, 0f, true);
            }

            // Chromatic spark VFX
            if (!Main.dedServ && Main.rand.NextBool(4))
            {
                Vector2 pos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.5f, npc.height * 0.5f);
                Color sparkCol = Main.rand.Next(3) switch
                {
                    0 => new Color(255, 60, 80),   // Crimson
                    1 => new Color(180, 50, 100),   // Dark Pink
                    _ => new Color(120, 30, 140),   // Purple
                };
                Dust d = Dust.NewDustPerfect(pos, DustID.GemRuby,
                    Main.rand.NextVector2Circular(2f, 2f), 0, sparkCol, 0.9f);
                d.noGravity = true;
            }
        }
    }

    /// <summary>
    /// AnnihilationMark — Coda of Annihilation's stacking debuff.
    /// Stacks up to 10. At 10: massive damage burst equal to 50% of all stacked damage.
    /// </summary>
    public class AnnihilationMark : ModBuff
    {
        public const int MaxStacks = 10;

        public override string Texture => "Terraria/Images/Buff_" + BuffID.Obstructed;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<AnnihilationMarkNPC>().HasMark = true;
        }
    }

    public class AnnihilationMarkNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool HasMark;
        public int Stacks;
        public int AccumulatedDamage;

        public override void ResetEffects(NPC npc)
        {
            if (!HasMark)
            {
                Stacks = 0;
                AccumulatedDamage = 0;
            }
            HasMark = false;
        }

        /// <summary>Add an annihilation stack. Returns true if detonation triggered.</summary>
        public bool AddStack(int damage)
        {
            Stacks++;
            AccumulatedDamage += damage;

            if (Stacks >= AnnihilationMark.MaxStacks)
            {
                return true; // Caller handles detonation
            }
            return false;
        }

        public void Detonate(NPC npc)
        {
            int burstDamage = (int)(AccumulatedDamage * 0.5f);
            npc.SimpleStrikeNPC(burstDamage, 0, false, 0f, null, false, 0f, true);

            // VFX: massive annihilation burst
            if (!Main.dedServ)
            {
                for (int i = 0; i < 25; i++)
                {
                    float angle = MathHelper.TwoPi * i / 25f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
                    Dust d = Dust.NewDustPerfect(npc.Center, DustID.GemRuby, vel, 0, default, 2f);
                    d.noGravity = true;
                }
                for (int i = 0; i < 10; i++)
                {
                    Dust flash = Dust.NewDustPerfect(npc.Center, DustID.PinkTorch,
                        Main.rand.NextVector2Circular(3f, 3f), 0, default, 2.5f);
                    flash.noGravity = true;
                }
            }

            Stacks = 0;
            AccumulatedDamage = 0;
        }

        public override void PostAI(NPC npc)
        {
            if (!HasMark || Stacks <= 0) return;

            // Per-stack visual: tightening crimson rings
            if (!Main.dedServ && Main.GameUpdateCount % 8 == 0)
            {
                float ringRadius = 30f - (Stacks * 2f); // Rings tighten with stacks
                if (ringRadius < 5f) ringRadius = 5f;
                for (int i = 0; i < Stacks; i++)
                {
                    float angle = Main.GameUpdateCount * 0.06f + MathHelper.TwoPi * i / Math.Max(Stacks, 1);
                    Vector2 pos = npc.Center + angle.ToRotationVector2() * ringRadius;
                    Dust d = Dust.NewDustPerfect(pos, DustID.GemRuby, Vector2.Zero, 0, default, 0.6f);
                    d.noGravity = true;
                }
            }
        }
    }

    /// <summary>
    /// BygoneEcho — Resonance of a Bygone Reality's marking debuff.
    /// When both spectral blade and rapid bullet hit within 0.5s, triggers Bygone Resonance explosion.
    /// </summary>
    public class BygoneEcho : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Obstructed;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<BygoneEchoNPC>().HasEcho = true;
        }
    }

    public class BygoneEchoNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool HasEcho;
        public bool HitByBullet;
        public bool HitByBlade;
        public int BulletHitTimer;
        public int BladeHitTimer;
        public int LastBulletDamage;
        public int LastBladeDamage;

        public override void ResetEffects(NPC npc)
        {
            if (!HasEcho)
            {
                HitByBullet = false;
                HitByBlade = false;
                BulletHitTimer = 0;
                BladeHitTimer = 0;
            }
            HasEcho = false;
        }

        public void OnBulletHit(int damage)
        {
            HitByBullet = true;
            BulletHitTimer = 30; // 0.5 second window
            LastBulletDamage = damage;
        }

        public void OnBladeHit(int damage)
        {
            HitByBlade = true;
            BladeHitTimer = 30;
            LastBladeDamage = damage;
        }

        /// <summary>Check if Bygone Resonance should trigger. Returns combined damage if so.</summary>
        public int CheckResonanceTrigger()
        {
            if (HitByBullet && HitByBlade && BulletHitTimer > 0 && BladeHitTimer > 0)
            {
                int combined = LastBulletDamage + LastBladeDamage;
                HitByBullet = false;
                HitByBlade = false;
                BulletHitTimer = 0;
                BladeHitTimer = 0;
                return combined;
            }
            return 0;
        }

        public override void PostAI(NPC npc)
        {
            if (BulletHitTimer > 0) BulletHitTimer--;
            if (BladeHitTimer > 0) BladeHitTimer--;

            if (BulletHitTimer <= 0) HitByBullet = false;
            if (BladeHitTimer <= 0) HitByBlade = false;
        }
    }
}
