using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Utilities;
using MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Shaders;

namespace MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Projectiles
{
    /// <summary>
    /// Death Tolling Bell Minion — A spectral bell that hovers near the player and periodically
    /// unleashes devastating toll shockwave rings. The bell charges by vibrating and glowing,
    /// then releases 3 concentric rings of toll wave projectiles.
    ///
    /// VFX Layers:
    /// - Ambient: Dark crimson smoke wisps + faint ember sparks
    /// - Charge: Intensifying bloom + vibrating position offset + gathering ring sparks
    /// - Toll: Massive bloom flash + 3 rings × 12 projectiles + screen shake + note cascade
    /// - Passive: Faint aura glow around the bell, pulsing with ResonancePalette colors
    /// </summary>
    public class BellTollingMinion : ModProjectile
    {
        // ─── Textures ───
        private static Asset<Texture2D> glowTexture;
        private static Asset<Texture2D> bloomTexture;
        private static Asset<Texture2D> maskTexture;

        // ─── Toll flash state ───
        private int tollFlashTimer = 0;
        private const int TollFlashDuration = 18;

        // ─── AI State ───
        private ref float TollCooldown => ref Projectile.ai[0];
        private ref float ChargeProgress => ref Projectile.ai[1];

        // ─── Constants ───
        private const int MaxTollCooldown = 90;
        private const int ChargeTime = 45;
        private const int TollRings = 3;
        private const int ProjectilesPerRing = 12;
        private static readonly float[] RingSpeeds = { 8f, 12f, 16f };

        // ─── Hover Parameters ───
        private const float HoverOffsetX = 50f;
        private const float HoverOffsetY = -80f;
        private const float BobAmplitude = 6f;
        private const float BobFrequency = 0.03f;
        private const float HoverLerpSpeed = 0.08f;

        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 18000;
            Projectile.netImportant = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // ── Buff check ──
            if (!CheckActive(owner)) return;

            // ── Hover movement ──
            UpdateHoverPosition(owner);

            // ── Toll cycle ──
            UpdateTollCycle(owner);

            // ── Ambient VFX ──
            SpawnAmbientParticles();

            // ── Toll flash decay ──
            if (tollFlashTimer > 0)
                tollFlashTimer--;
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<Buffs.DeathTollingBellBuff>());
                Projectile.Kill();
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<Buffs.DeathTollingBellBuff>()))
                Projectile.timeLeft = 2;

            return true;
        }

        private void UpdateHoverPosition(Player owner)
        {
            // Target position: offset from player with bob
            int dir = (owner.direction == -1) ? 1 : -1;
            float bob = (float)Math.Sin(Main.GameUpdateCount * BobFrequency) * BobAmplitude;
            Vector2 target = owner.Center + new Vector2(HoverOffsetX * dir, HoverOffsetY + bob);

            // Smooth lerp toward target
            Projectile.Center = Vector2.Lerp(Projectile.Center, target, HoverLerpSpeed);

            // Face the player's direction
            Projectile.spriteDirection = -owner.direction;

            // Charge vibration
            if (ChargeProgress > 0)
            {
                float intensityRatio = ChargeProgress / ChargeTime;
                float vibrate = intensityRatio * 2f;
                Projectile.Center += new Vector2(
                    Main.rand.NextFloat(-vibrate, vibrate),
                    Main.rand.NextFloat(-vibrate, vibrate));
            }
        }

        private void UpdateTollCycle(Player owner)
        {
            // Find nearest enemy for targeting
            NPC target = FindTarget(owner, 900f);

            if (target == null)
            {
                // No target: reset and wait
                TollCooldown = 0;
                ChargeProgress = 0;
                return;
            }

            TollCooldown++;

            if (TollCooldown >= MaxTollCooldown && ChargeProgress < ChargeTime)
            {
                // Charging phase
                ChargeProgress++;
                SpawnChargeParticles();

                if (ChargeProgress >= ChargeTime)
                {
                    // TOLL! Release shockwaves
                    ExecuteToll(owner, target);
                    TollCooldown = 0;
                    ChargeProgress = 0;
                }
            }
        }

        private NPC FindTarget(Player owner, float range)
        {
            NPC best = null;
            float closest = range * range;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float distSq = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                if (distSq < closest)
                {
                    closest = distSq;
                    best = npc;
                }
            }
            return best;
        }

        private void ExecuteToll(Player owner, NPC target)
        {
            if (Main.myPlayer != Projectile.owner) return;

            // Spawn toll wave projectiles in 3 rings
            for (int ring = 0; ring < TollRings; ring++)
            {
                float speed = RingSpeeds[ring];
                float angleOffset = ring * (MathHelper.TwoPi / (TollRings * ProjectilesPerRing));

                for (int i = 0; i < ProjectilesPerRing; i++)
                {
                    float angle = (MathHelper.TwoPi / ProjectilesPerRing) * i + angleOffset;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        vel,
                        ModContent.ProjectileType<BellTollWaveProjectile>(),
                        Projectile.damage,
                        Projectile.knockBack,
                        owner.whoAmI,
                        ai0: ring);
                }
            }

            // ── Toll VFX: massive bloom ──
            BellParticleHandler.SpawnParticle(new BellBloomParticle(
                Projectile.Center, BellUtils.EmberOrange, 3f, 30));
            BellParticleHandler.SpawnParticle(new BellBloomParticle(
                Projectile.Center, BellUtils.BellWhite, 1.5f, 20));

            // ── Expanding toll rings (visual only) ──
            for (int r = 0; r < 3; r++)
            {
                float maxRad = 120f + r * 80f;
                Color ringColor = BellUtils.MulticolorLerp(r / 2f,
                    BellUtils.TollCrimson, BellUtils.EmberOrange, BellUtils.HellfireGold);
                BellParticleHandler.SpawnParticle(new TollRingParticle(
                    Projectile.Center, maxRad, ringColor, 5f, 25 + r * 5));
            }

            // ── Music notes cascade ──
            for (int n = 0; n < 8; n++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -0.5f));
                Color noteColor = BellUtils.MulticolorLerp(Main.rand.NextFloat(),
                    BellUtils.TollCrimson, BellUtils.EmberOrange, BellUtils.EchoGold);
                BellParticleHandler.SpawnParticle(new BellNoteParticle(Projectile.Center, vel, noteColor, 0.5f, 50));
            }

            // ── Smoke burst ──
            for (int s = 0; s < 10; s++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                BellParticleHandler.SpawnParticle(new BellSmokeParticle(Projectile.Center, vel, 0.8f, 35));
            }

            // ── Ember shower ──
            for (int e = 0; e < 12; e++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f) + new Vector2(0, -1f);
                BellParticleHandler.SpawnParticle(new BellEmberParticle(Projectile.Center, vel, 0.3f, 30));
            }

            // ── Trigger toll flash overlay ──
            tollFlashTimer = TollFlashDuration;
        }

        private void SpawnChargeParticles()
        {
            float intensity = ChargeProgress / ChargeTime;

            // Gathering sparks spiraling inward
            if (Main.rand.NextFloat() < 0.3f + intensity * 0.5f)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = 60f * (1f - intensity * 0.5f);
                Vector2 spawnPos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                Vector2 vel = (Projectile.Center - spawnPos).SafeNormalize(Vector2.Zero) * 1.5f;
                Color c = BellUtils.MulticolorLerp(intensity,
                    BellUtils.DarkSmoke, BellUtils.TollCrimson, BellUtils.EmberOrange);
                BellParticleHandler.SpawnParticle(new BellEmberParticle(spawnPos, vel, 0.2f + intensity * 0.2f, 15));
            }

            // Charge bloom growing
            if ((int)ChargeProgress % 10 == 0)
            {
                BellParticleHandler.SpawnParticle(new BellBloomParticle(
                    Projectile.Center, BellUtils.BurningResonance * intensity, 0.5f + intensity * 1f, 15));
            }
        }

        private void SpawnAmbientParticles()
        {
            // Faint smoke wisps
            if (Main.rand.NextBool(8))
            {
                Vector2 off = Main.rand.NextVector2Circular(15f, 15f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.4f);
                BellParticleHandler.SpawnParticle(new BellSmokeParticle(Projectile.Center + off, vel, 0.3f, 30));
            }

            // Rare ember sparks
            if (Main.rand.NextBool(12))
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1f, -0.3f));
                BellParticleHandler.SpawnParticle(new BellEmberParticle(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), vel, 0.15f, 20));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawBellGlow();
            DrawBellBody(lightColor);
            DrawChargeAura();
            DrawTollFlash();
            return false;
        }

        private void DrawBellBody(Color lightColor)
        {
            var tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;

            // Main bell sprite
            Main.EntitySpriteDraw(tex, drawPos, null, lightColor, Projectile.rotation, origin, 1f,
                Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
        }

        private void DrawBellGlow()
        {
            bloomTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!bloomTexture.IsLoaded) return;
            var tex = bloomTexture.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Ambient aura pulse
            float pulse = 0.6f + 0.3f * (float)Math.Sin(Main.GameUpdateCount * 0.05f);
            Color auraColor = BellUtils.TollCrimson * pulse * 0.25f;
            Main.EntitySpriteDraw(tex, drawPos, null, auraColor, 0f, tex.Size() / 2f, 1.2f, SpriteEffects.None, 0);

            // Charge intensity glow
            if (ChargeProgress > 0)
            {
                float intensity = ChargeProgress / ChargeTime;
                Color chargeColor = Color.Lerp(BellUtils.TollCrimson, BellUtils.EmberOrange, intensity) * intensity * 0.5f;
                Main.EntitySpriteDraw(tex, drawPos, null, chargeColor, 0f, tex.Size() / 2f, 0.8f + intensity * 1.5f, SpriteEffects.None, 0);

                // White-hot core at max charge
                if (intensity > 0.7f)
                {
                    float whiteIntensity = (intensity - 0.7f) / 0.3f;
                    Main.EntitySpriteDraw(tex, drawPos, null, BellUtils.BellWhite * whiteIntensity * 0.3f,
                        0f, tex.Size() / 2f, 0.4f + whiteIntensity * 0.6f, SpriteEffects.None, 0);
                }
            }
        }

        private void DrawChargeAura()
        {
            if (ChargeProgress <= 0) return;
            glowTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle");
            if (!glowTexture.IsLoaded) return;
            var tex = glowTexture.Value;

            float intensity = ChargeProgress / ChargeTime;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Pulsing outer aura ring — dual-layer for richer look
            float ringScale = 1.5f + intensity * 0.5f + (float)Math.Sin(ChargeProgress * 0.3f) * 0.2f;
            Color ringColor = Color.Lerp(BellUtils.DarkSmoke, BellUtils.BurningResonance, intensity) * intensity * 0.3f;
            Main.EntitySpriteDraw(tex, drawPos, null, ringColor, 0f, tex.Size() / 2f, ringScale, SpriteEffects.None, 0);

            // Inner resonance ring — counter-rotates and pulses at different frequency
            float innerScale = 0.8f + intensity * 0.3f + (float)Math.Sin(ChargeProgress * 0.5f + 1.5f) * 0.15f;
            Color innerColor = Color.Lerp(BellUtils.TollCrimson, BellUtils.HellfireGold, intensity) * intensity * 0.2f;
            Main.EntitySpriteDraw(tex, drawPos, null, innerColor, -ChargeProgress * 0.1f, tex.Size() / 2f, innerScale, SpriteEffects.None, 0);

            // Gathering glyph cross-flare at high charge (>60%)
            if (intensity > 0.6f)
            {
                bloomTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
                if (bloomTexture.IsLoaded)
                {
                    var bloom = bloomTexture.Value;
                    float flareAlpha = (intensity - 0.6f) / 0.4f;
                    Color flareColor = BellUtils.Additive(BellUtils.EmberOrange, 0.35f * flareAlpha);
                    // Vertical cross
                    Main.EntitySpriteDraw(bloom, drawPos, null, flareColor, 0f, bloom.Size() / 2f,
                        new Vector2(0.15f, 1.5f * flareAlpha), SpriteEffects.None, 0);
                    // Horizontal cross
                    Main.EntitySpriteDraw(bloom, drawPos, null, flareColor, MathHelper.PiOver2, bloom.Size() / 2f,
                        new Vector2(0.15f, 1.5f * flareAlpha), SpriteEffects.None, 0);
                }
            }
        }

        /// <summary>
        /// Draws a dramatic flash overlay when the bell tolls — expanding concentric rings
        /// with shader-driven rendering if BellToll.fx is available, or fallback bloom rings.
        /// </summary>
        private void DrawTollFlash()
        {
            if (tollFlashTimer <= 0) return;

            bloomTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!bloomTexture.IsLoaded) return;
            var tex = bloomTexture.Value;

            float flashProgress = 1f - (float)tollFlashTimer / TollFlashDuration;
            float flashAlpha = (float)Math.Pow(1f - flashProgress, 1.5f); // fast fade
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Layer 1: Massive white flash burst (immediate)
            float burstScale = 1f + flashProgress * 4f;
            Main.EntitySpriteDraw(tex, drawPos, null, BellUtils.Additive(BellUtils.BellWhite, 0.6f * flashAlpha),
                0f, tex.Size() / 2f, burstScale, SpriteEffects.None, 0);

            // Layer 2: Crimson expanding ring
            float ring1Scale = 0.5f + flashProgress * 5f;
            float ring1Alpha = flashAlpha * 0.5f * (1f - flashProgress);
            Main.EntitySpriteDraw(tex, drawPos, null, BellUtils.Additive(BellUtils.TollCrimson, ring1Alpha),
                0f, tex.Size() / 2f, ring1Scale, SpriteEffects.None, 0);

            // Layer 3: Orange expanding ring (delayed)
            if (flashProgress > 0.15f)
            {
                float ring2T = (flashProgress - 0.15f) / 0.85f;
                float ring2Scale = 0.3f + ring2T * 4.5f;
                float ring2Alpha = (1f - ring2T) * 0.4f;
                Main.EntitySpriteDraw(tex, drawPos, null, BellUtils.Additive(BellUtils.EmberOrange, ring2Alpha),
                    0f, tex.Size() / 2f, ring2Scale, SpriteEffects.None, 0);
            }

            // Layer 4: Gold inner core that lingers
            float coreAlpha = flashAlpha * 0.8f;
            Main.EntitySpriteDraw(tex, drawPos, null, BellUtils.Additive(BellUtils.HellfireGold, coreAlpha),
                0f, tex.Size() / 2f, 0.4f + flashProgress * 0.3f, SpriteEffects.None, 0);

            // Layer 5: Cross-flare on initial impact
            if (flashProgress < 0.4f)
            {
                var pointBloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
                if (pointBloom.IsLoaded)
                {
                    var pb = pointBloom.Value;
                    float crossAlpha = (1f - flashProgress / 0.4f) * 0.5f;
                    Color crossColor = BellUtils.Additive(BellUtils.BellWhite, crossAlpha);
                    float crossScale = 1.5f + flashProgress * 3f;
                    // Vertical
                    Main.EntitySpriteDraw(pb, drawPos, null, crossColor, 0f, pb.Size() / 2f,
                        new Vector2(0.12f, crossScale), SpriteEffects.None, 0);
                    // Horizontal
                    Main.EntitySpriteDraw(pb, drawPos, null, crossColor, MathHelper.PiOver2, pb.Size() / 2f,
                        new Vector2(0.12f, crossScale), SpriteEffects.None, 0);
                    // Diagonal 45°
                    Main.EntitySpriteDraw(pb, drawPos, null, crossColor * 0.5f, MathHelper.PiOver4, pb.Size() / 2f,
                        new Vector2(0.08f, crossScale * 0.7f), SpriteEffects.None, 0);
                    // Diagonal -45°
                    Main.EntitySpriteDraw(pb, drawPos, null, crossColor * 0.5f, -MathHelper.PiOver4, pb.Size() / 2f,
                        new Vector2(0.08f, crossScale * 0.7f), SpriteEffects.None, 0);
                }
            }
        }
    }
}
