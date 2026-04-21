using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.OdeToJoy.Systems
{
    /// <summary>
    /// GlobalNPC tracking Ode to Joy weapon-specific debuffs and stacks per NPC.
    /// </summary>
    public class OdeToJoyGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        // ═══════════════════════════════════════════════════════
        // Elysian Verdict — Elysian Mark
        // ═══════════════════════════════════════════════════════
        public int ElysianMarkTier; // 0-3
        public int ElysianMarkTimer;
        public int ElysianMarkOwner = -1;
        private const int MarkDuration = 300; // 5 seconds

        public void AddElysianMark(int owner, int tiersToAdd = 1)
        {
            ElysianMarkOwner = owner;
            ElysianMarkTier = System.Math.Min(ElysianMarkTier + tiersToAdd, 3);
            ElysianMarkTimer = MarkDuration;
        }

        public bool ShouldDetonate => ElysianMarkTier >= 3;

        public int ConsumeMarks()
        {
            int tiers = ElysianMarkTier;
            ElysianMarkTier = 0;
            ElysianMarkTimer = 0;
            return tiers;
        }

        // ═══════════════════════════════════════════════════════
        // Thorn Spray Repeater — Embedded Thorns
        // ═══════════════════════════════════════════════════════
        public int EmbeddedThorns;
        public int EmbeddedThornsOwner = -1;
        public const int MaxEmbeddedThorns = 25;

        public bool AddEmbeddedThorn(int owner)
        {
            EmbeddedThornsOwner = owner;
            EmbeddedThorns = System.Math.Min(EmbeddedThorns + 1, MaxEmbeddedThorns);
            return EmbeddedThorns >= MaxEmbeddedThorns;
        }

        public int DetonateEmbeddedThorns()
        {
            int thorns = EmbeddedThorns;
            EmbeddedThorns = 0;
            return thorns;
        }

        // ═══════════════════════════════════════════════════════
        // The Pollinator — Pollinated DoT
        // ═══════════════════════════════════════════════════════
        public int PollinatedTimer;
        public int PollinatedOwner = -1;
        public const int PollinateDuration = 300; // 5 seconds
        public const float PollinateDamagePercent = 0.01f; // 1% HP/s

        public bool IsPollinated => PollinatedTimer > 0;

        public void ApplyPollinated(int owner)
        {
            PollinatedOwner = owner;
            PollinatedTimer = PollinateDuration;
        }

        // ═══════════════════════════════════════════════════════
        // HOOKS
        // ═══════════════════════════════════════════════════════

        public override void ResetEffects(NPC npc)
        {
            // Elysian Mark decay
            if (ElysianMarkTimer > 0)
            {
                ElysianMarkTimer--;
                if (ElysianMarkTimer <= 0)
                    ElysianMarkTier = 0;
            }

            // Pollinated decay
            if (PollinatedTimer > 0)
                PollinatedTimer--;
        }

        public override void AI(NPC npc)
        {
            // Elysian Mark VFX
            if (ElysianMarkTier > 0 && Main.rand.NextBool(6 - ElysianMarkTier))
            {
                Color col = Color.Lerp(OdeToJoyPalette.LeafGreen, OdeToJoyPalette.GoldenPollen, (float)ElysianMarkTier / 3f);
                Dust d = Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width / 2, npc.height / 2),
                    DustID.GreenTorch, new Vector2(0, -1f), 0, col, 0.6f + ElysianMarkTier * 0.2f);
                d.noGravity = true;
            }

            // Embedded Thorns VFX
            if (EmbeddedThorns > 0 && Main.rand.NextBool(8))
            {
                float progress = (float)EmbeddedThorns / MaxEmbeddedThorns;
                Color col = Color.Lerp(OdeToJoyPalette.LeafGreen, OdeToJoyPalette.RosePink, progress);
                Vector2 offset = Main.rand.NextVector2Circular(npc.width / 2, npc.height / 2);
                Dust d = Dust.NewDustPerfect(npc.Center + offset, DustID.GreenTorch,
                    -offset.SafeNormalize(Vector2.Zero) * 0.5f, 0, col, 0.5f);
                d.noGravity = true;
            }

            // Pollinated DoT damage
            if (PollinatedTimer > 0)
            {
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;

                // 1% max HP per second = 2% of lifeRegen (since lifeRegen is in half-HP per second)
                int dotDamage = (int)(npc.lifeMax * PollinateDamagePercent * 2f);
                npc.lifeRegen -= System.Math.Max(dotDamage, 10);

                // Pollinated VFX - golden pollen
                if (Main.rand.NextBool(4))
                {
                    Color pollenCol = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.SunlightYellow, Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width / 2, npc.height / 2),
                        DustID.GoldFlame, new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f), 0, pollenCol, 0.7f);
                    d.noGravity = true;
                }
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            // Elysian Mark tint
            if (ElysianMarkTier > 0)
            {
                float markProgress = (float)ElysianMarkTier / 3f;
                Color tint = Color.Lerp(Color.White, OdeToJoyPalette.GoldenPollen, markProgress * 0.25f);
                drawColor = Color.Lerp(drawColor, tint, 0.15f);
            }

            // Embedded Thorns tint
            if (EmbeddedThorns > 0)
            {
                float thornProgress = (float)EmbeddedThorns / MaxEmbeddedThorns;
                Color tint = Color.Lerp(Color.White, OdeToJoyPalette.LeafGreen, thornProgress * 0.2f);
                drawColor = Color.Lerp(drawColor, tint, 0.1f);
            }

            // Pollinated golden tint
            if (IsPollinated)
            {
                drawColor = Color.Lerp(drawColor, OdeToJoyPalette.SunlightYellow, 0.15f);
            }
        }

        public override void OnKill(NPC npc)
        {
            // Pollinated death: Mass Bloom — fire 3 homing children toward other pollinated enemies
            if (IsPollinated && PollinatedOwner >= 0 && Main.myPlayer == PollinatedOwner)
            {
                Player owner = Main.player[PollinatedOwner];
                var combat = owner.GetModPlayer<OdeToJoyCombatPlayer>();

                // Track bloom kill
                bool harvestTriggered = combat.TrackBloomKill();

                // Find other pollinated enemies
                List<NPC> pollinatedTargets = new List<NPC>();
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC other = Main.npc[i];
                    if (other.active && other.whoAmI != npc.whoAmI && other.CanBeChasedBy())
                    {
                        var otherGlobal = other.GetGlobalNPC<OdeToJoyGlobalNPC>();
                        if (otherGlobal.IsPollinated)
                            pollinatedTargets.Add(other);
                    }
                }

                // Fire 3 homing children toward pollinated targets
                int childCount = System.Math.Min(3, pollinatedTargets.Count);
                for (int i = 0; i < childCount; i++)
                {
                    NPC target = pollinatedTargets[i];
                    Vector2 vel = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX) * 12f;

                    GenericHomingOrbChild.SpawnChild(
                        npc.GetSource_Death(),
                        npc.Center, vel,
                        50, 2f, PollinatedOwner,
                        homingStrength: 0.10f,
                        behaviorFlags: GenericHomingOrbChild.FLAG_ACCELERATE,
                        themeIndex: GenericHomingOrbChild.THEME_ODETOJOY,
                        scaleMult: harvestTriggered ? 1.5f : 1f,
                        timeLeft: 90);
                }

                // Mass Bloom VFX
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                    Color col = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.RosePink, Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(npc.Center, DustID.GoldFlame, vel, 0, col, 1.0f);
                    d.noGravity = true;
                }

                if (harvestTriggered)
                {
                    SoundEngine.PlaySound(SoundID.Item73 with { Pitch = 0.3f, Volume = 0.8f }, npc.Center);
                }
            }
        }
    }
}
