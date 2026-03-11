using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Dusts;

namespace MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Buffs
{
    /// <summary>
    /// Jubilant Burn — 4% weapon damage/s DoT from Verse 2 Rising.
    /// Warm golden flames visually mark the target.
    /// </summary>
    public class JubilantBurnDebuff : ModBuff
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
    /// Hymn Resonance — +25% magic damage taken when hit by 3+ verse types within 5s.
    /// Golden resonance aura marks the target.
    /// </summary>
    public class HymnResonanceDebuff : ModBuff
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
    /// GlobalNPC handling Jubilant Burn DoT and Hymn Resonance vulnerability tracking.
    /// </summary>
    public class HymnDebuffNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        /// <summary>Tracks which verse types have hit this NPC (bitfield, 4 bits).</summary>
        public int VerseTypesHit;

        /// <summary>Timer to reset verse type tracking (300 frames = 5s window).</summary>
        public int VerseResetTimer = 300;

        /// <summary>Base damage for Jubilant Burn scaling.</summary>
        public int BurnBaseDamage = 100;

        /// <summary>
        /// Register a verse hit. Returns true when Hymn Resonance should trigger.
        /// </summary>
        public bool RegisterVerseHit(NPC npc, int verseType)
        {
            VerseTypesHit |= (1 << verseType);
            VerseResetTimer = 300;

            // Count distinct verse types
            int count = 0;
            for (int i = 0; i < 4; i++)
                if ((VerseTypesHit & (1 << i)) != 0) count++;

            if (count >= 3 && !npc.HasBuff(ModContent.BuffType<HymnResonanceDebuff>()))
            {
                npc.AddBuff(ModContent.BuffType<HymnResonanceDebuff>(), 240);
                return true;
            }
            return false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            // Jubilant Burn: 4% weapon damage/s DoT
            if (npc.HasBuff(ModContent.BuffType<JubilantBurnDebuff>()))
            {
                int burnDamage = (int)(BurnBaseDamage * 0.04f);
                npc.lifeRegen -= burnDamage * 2; // lifeRegen counts per 0.5s
                if (damage < burnDamage) damage = burnDamage;
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // Hymn Resonance: +25% magic damage taken
            if (npc.HasBuff(ModContent.BuffType<HymnResonanceDebuff>()) && projectile.DamageType == DamageClass.Magic)
            {
                modifiers.FinalDamage *= 1.25f;
            }
        }

        public override void ResetEffects(NPC npc)
        {
            if (VerseResetTimer > 0)
            {
                VerseResetTimer--;
                if (VerseResetTimer <= 0)
                    VerseTypesHit = 0;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (npc.HasBuff(ModContent.BuffType<JubilantBurnDebuff>()))
            {
                drawColor = Color.Lerp(drawColor, new Color(255, 200, 50), 0.15f);
                if (Main.rand.NextBool(5))
                {
                    Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, ModContent.DustType<JubilantEmberDust>(), 0f, -1.5f, 120, new Color(255, 200, 50), 0.6f);
                    d.noGravity = true;
                }
            }
            if (npc.HasBuff(ModContent.BuffType<HymnResonanceDebuff>()))
            {
                drawColor = Color.Lerp(drawColor, new Color(255, 250, 200), 0.1f);
                if (Main.rand.NextBool(8))
                {
                    Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, ModContent.DustType<JubilantEmberDust>(), 0f, -0.5f, 100, new Color(255, 250, 200), 0.4f);
                    d.noGravity = true;
                }
            }
        }
    }
}
