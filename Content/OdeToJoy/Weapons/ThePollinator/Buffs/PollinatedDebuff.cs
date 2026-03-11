using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Dusts;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Buffs
{
    /// <summary>
    /// Pollinated debuff — 1% HP/s DoT, spreads to nearby enemies after 2s,
    /// Mass Bloom on death (golden explosion + 3 homing seeds).
    /// Duration: 600 frames (10s), refreshes on re-application.
    /// </summary>
    public class PollinatedDebuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_20";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    public class PollinatedNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int PollinatedTimer;
        public bool IsHarvestSeason;
        public int HarvestTimer;

        public override void ResetEffects(NPC npc)
        {
            if (HarvestTimer > 0) HarvestTimer--;
            else IsHarvestSeason = false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (!npc.HasBuff(ModContent.BuffType<PollinatedDebuff>())) return;

            PollinatedTimer++;

            // 1% HP/s DoT (3x during Harvest Season)
            float dotMultiplier = IsHarvestSeason ? 3f : 1f;
            int dotDamage = (int)(npc.lifeMax * 0.01f * dotMultiplier / 30f);
            if (dotDamage < 1) dotDamage = 1;
            npc.lifeRegen -= dotDamage * 2;
            if (damage < dotDamage) damage = dotDamage;

            // Spread pollen to nearby enemies after 2s (120 frames)
            if (PollinatedTimer >= 120 && PollinatedTimer % 60 == 0)
            {
                float spreadRange = 64f; // 4 tiles
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC other = Main.npc[i];
                    if (!other.active || other.friendly || other.dontTakeDamage || other.whoAmI == npc.whoAmI) continue;
                    if (other.HasBuff(ModContent.BuffType<PollinatedDebuff>())) continue;
                    if (Vector2.Distance(npc.Center, other.Center) <= spreadRange)
                    {
                        other.AddBuff(ModContent.BuffType<PollinatedDebuff>(), 600);
                    }
                }
            }

            // Visual: golden pollen aura — custom pollen dust
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustDirect(npc.position - new Vector2(4), npc.width + 8, npc.height + 8, ModContent.DustType<PollenCloudDust>(), 0f, -0.5f, 150, default, 0.6f);
                d.noGravity = true;
                d.velocity *= 0.3f;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (!npc.HasBuff(ModContent.BuffType<PollinatedDebuff>())) return;
            drawColor = Color.Lerp(drawColor, PollinatorTextures.BloomGold, 0.2f);
        }
    }
}
