using System;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Buffs;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Utilities;
using MagnumOpus.Common.Systems.VFX;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Projectiles
{
    /// <summary>
    /// Crystallized Flame ? Sub-projectile spawned by Wrath's Cleaver swings.
    /// Launches outward, decelerates, then persists as a ground fire zone.
    ///
    /// Two phases:
    ///   1. LAUNCH (first 30 ticks): Flies toward target, trailing embers, shrinking velocity.
    ///   2. PERSIST (remaining ~150 ticks): Sits on ground as a burning damage zone with
    ///      pulsing fire glow, ash drift, and ground-level flame dust.
    ///
    /// VFX Layers:
    ///   - Core fire orb (additive soft glow, ember orange �� blood red)
    ///   - Outer bloom halo (pulsing, radial)
    ///   - Trailing ember dust during launch phase
    ///   - Ground flame dust during persist phase
    ///   - Impact flash on first contact / transition to persist
    /// </summary>
    public class WrathCrystallizedFlame : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private bool _inPersistPhase = false;
        private int _persistTimer = 0;

        private ref float LaunchTimer => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            LaunchTimer++;

            if (!_inPersistPhase)
            {
                // ���� LAUNCH PHASE ����
                // Decelerate over 30 ticks
                if (LaunchTimer < 30)
                {
                    float slowdown = MathHelper.Lerp(1f, 0.05f, LaunchTimer / 30f);
                    Projectile.velocity *= 0.96f + slowdown * 0.02f;
                    Projectile.rotation = Projectile.velocity.ToRotation();

                    // Trailing ember dust
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 dustVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f)
                            + Main.rand.NextVector2Circular(1f, 1f);
                        Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, dustVel, 0,
                            WrathsCleaverUtils.EmberOrange, Main.rand.NextFloat(1.0f, 1.8f));
                        d.noGravity = true;
                        d.fadeIn = 1.2f;
                    }

                    // Smoke trail
                    if (Main.rand.NextBool(3))
                    {
                        Dust smoke = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke,
                            Main.rand.NextVector2Circular(1f, 1f), 100,
                            DiesIraePalette.CharcoalBlack, Main.rand.NextFloat(0.8f, 1.2f));
                        smoke.noGravity = true;
                    }
                }
                else
                {
                    // Transition to persist phase
                    _inPersistPhase = true;
                    Projectile.velocity = Vector2.Zero;
                    Projectile.damage = (int)(Projectile.damage * 0.4f); // Zone deals reduced damage

                    // Impact flash burst
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                        Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, vel, 0,
                            WrathsCleaverUtils.JudgmentGold, Main.rand.NextFloat(1f, 1.6f));
                        d.noGravity = true;
                    }

                    SoundEngine.PlaySound(SoundID.Item34 with { Pitch = 0.3f, Volume = 0.5f }, Projectile.Center);
                }
            }
            else
            {
                // ���� PERSIST PHASE ����
                _persistTimer++;

                // Ground-level fire dust (low and horizontal)
                if (Main.rand.NextBool(3))
                {
                    Vector2 offset = new Vector2(Main.rand.NextFloat(-16f, 16f), Main.rand.NextFloat(-4f, 4f));
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-3f, -1f));
                    Dust d = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Torch, vel, 0,
                        WrathsCleaverUtils.GetPaletteColor(Main.rand.NextFloat(0.2f, 0.7f)),
                        Main.rand.NextFloat(0.8f, 1.4f));
                    d.noGravity = true;
                    d.fadeIn = 1.0f;
                }

                // Ash drift upward
                if (Main.rand.NextBool(6))
                {
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.5f, -0.5f));
                    Dust ash = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 6f),
                        DustID.Smoke, vel, 120, DiesIraePalette.AshGray, 0.6f);
                    ash.noGravity = false;
                }
            }

            // Dynamic lighting
            float lightFade = _inPersistPhase
                ? MathHelper.Clamp(1f - _persistTimer / 150f, 0.2f, 1f)
                : 1f;
            Lighting.AddLight(Projectile.Center, 0.8f * lightFade, 0.3f * lightFade, 0.05f * lightFade);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<HellfireImmolation>(), 180);
            target.AddBuff(BuffID.OnFire3, 240);

            // Hit flash
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Torch, vel, 0,
                    WrathsCleaverUtils.EmberOrange, 1.0f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return false;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glow.Size() * 0.5f;

            float lifeFraction = Projectile.timeLeft / 180f;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f + Projectile.whoAmI) * 0.15f;

            // Fade out in last 30 ticks
            float fadeMult = Projectile.timeLeft < 30 ? Projectile.timeLeft / 30f : 1f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            if (_inPersistPhase)
            {
                // ���� PERSIST RENDERING: Ground fire zone ����
                float zoneAlpha = fadeMult * MathHelper.Clamp(1f - _persistTimer / 160f, 0.3f, 1f);

                // Outer blood red halo (wide, elliptical for ground feel)
                Color outerColor = WrathsCleaverUtils.BloodRed * (0.25f * zoneAlpha);
                outerColor.A = 0;
                sb.Draw(glow, drawPos, null, outerColor, 0f, origin, new Vector2(0.14f, 0.06f) * pulse, SpriteEffects.None, 0f);

                // Mid ember glow
                Color midColor = WrathsCleaverUtils.EmberOrange * (0.4f * zoneAlpha);
                midColor.A = 0;
                sb.Draw(glow, drawPos, null, midColor, 0f, origin, new Vector2(0.08f, 0.04f) * pulse, SpriteEffects.None, 0f);

                // Hot core
                Color coreColor = WrathsCleaverUtils.JudgmentGold * (0.55f * zoneAlpha);
                coreColor.A = 0;
                sb.Draw(glow, drawPos, null, coreColor, 0f, origin, 0.03f * pulse, SpriteEffects.None, 0f);
            }
            else
            {
                // ���� LAUNCH RENDERING: Flying fire orb ����
                float orbScale = 0.04f + 0.02f * (float)Math.Sin(LaunchTimer * 0.3f);

                // Outer bloom
                Color outerColor = WrathsCleaverUtils.BloodRed * (0.35f * fadeMult);
                outerColor.A = 0;
                sb.Draw(glow, drawPos, null, outerColor, 0f, origin, orbScale * 2.2f, SpriteEffects.None, 0f);

                // Inner fire
                Color innerColor = WrathsCleaverUtils.EmberOrange * (0.6f * fadeMult);
                innerColor.A = 0;
                sb.Draw(glow, drawPos, null, innerColor, 0f, origin, orbScale * 1.2f, SpriteEffects.None, 0f);

                // Hot white core
                Color coreColor = WrathsCleaverUtils.BoneWhite * (0.7f * fadeMult);
                coreColor.A = 0;
                sb.Draw(glow, drawPos, null, coreColor, 0f, origin, orbScale * 0.5f, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}