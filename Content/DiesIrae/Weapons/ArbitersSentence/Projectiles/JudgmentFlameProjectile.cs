using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Systems;

namespace MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence.Projectiles
{
    /// <summary>
    /// Judgment Flame — Arbiter's Sentence precision flame projectile.
    /// Applies stacking Judgment Flame debuff via GlobalNPC.
    /// ai[0] = IsFocusShot (0 or 1) — +40% damage from Arbiter's Focus.
    /// </summary>
    public class JudgmentFlameProjectile : ModProjectile
    {
        private bool IsFocusShot => Projectile.ai[0] > 0;
        private VertexStrip _strip;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = IsFocusShot ? 3 : 1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 3;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Enhanced penetrate for focus shots
            if (IsFocusShot && Projectile.penetrate == 1)
                Projectile.penetrate = 3;

            // Slight spread/wiggle for flamethrower effect
            if (Main.rand.NextBool(8))
            {
                Projectile.velocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.03f, 0.03f));
            }

            // Flame dust trail
            if (Main.rand.NextBool(2))
            {
                Color flameCol = IsFocusShot
                    ? Color.Lerp(DiesIraePalette.JudgmentGold, DiesIraePalette.WrathWhite, Main.rand.NextFloat())
                    : Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.EmberOrange, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.Torch, -Projectile.velocity * 0.1f, 0, flameCol, 0.6f + (IsFocusShot ? 0.3f : 0f));
                d.noGravity = true;
            }

            // Focus shot sparks
            if (IsFocusShot && Main.rand.NextBool(3))
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare,
                    Main.rand.NextVector2Circular(1f, 1f), 0, default, 0.5f);
                spark.noGravity = true;
            }

            // Lighting
            float intensity = IsFocusShot ? 0.35f : 0.25f;
            Color lightCol = IsFocusShot ? DiesIraePalette.JudgmentGold : DiesIraePalette.InfernalRed;
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * intensity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            var combat = owner.GetModPlayer<DiesIraeCombatPlayer>();
            var flameNPC = target.GetGlobalNPC<JudgmentFlameGlobalNPC>();

            // Check for Sentence Cage double damage (already applied via ModifyHitNPC)
            bool wasCaged = flameNPC.ConsumeCageBonus();
            if (wasCaged)
            {
                // Cage break VFX
                SoundEngine.PlaySound(SoundID.Item119 with { Pitch = 0.2f, Volume = 0.8f }, target.Center);
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                    Dust gold = Dust.NewDustPerfect(target.Center, DustID.GoldFlame, vel, 0, DiesIraePalette.JudgmentGold, 1.0f);
                    gold.noGravity = true;
                }
            }

            // Add flame stack
            bool cageTriggered = flameNPC.AddStack(Projectile.owner);
            if (cageTriggered)
            {
                // Cage activation VFX
                SoundEngine.PlaySound(SoundID.Item73 with { Pitch = -0.2f, Volume = 0.7f }, target.Center);
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8;
                    Vector2 offset = angle.ToRotationVector2() * 35f;
                    Dust cage = Dust.NewDustPerfect(target.Center + offset, DustID.GoldFlame,
                        -offset * 0.05f, 0, DiesIraePalette.JudgmentGold, 0.9f);
                    cage.noGravity = true;
                }
            }

            // Track consecutive hits for Arbiter's Focus
            if (owner.whoAmI == Main.myPlayer)
                combat.IncrementArbiterHits();

            // Apply fire debuff
            target.AddBuff(BuffID.OnFire3, 120);

            // Impact VFX
            int burstCount = IsFocusShot ? 6 : 3;
            for (int i = 0; i < burstCount; i++)
            {
                Color col = IsFocusShot ? DiesIraePalette.JudgmentGold : DiesIraePalette.InfernalRed;
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Torch,
                    Main.rand.NextVector2Circular(3f, 3f), 0, col, 0.6f);
                d.noGravity = true;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Sentence Cage double damage
            var flameNPC = target.GetGlobalNPC<JudgmentFlameGlobalNPC>();
            if (flameNPC.SentenceCageActive)
            {
                modifiers.FinalDamage *= 2f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 3; i++)
            {
                Color col = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.EmberOrange, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
                    Main.rand.NextVector2Circular(2f, 2f), 0, col, 0.5f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.DiesIrae, ref _strip);

            // Focus-shot indicator: bright golden ring overlay when IsFocusShot
            if (IsFocusShot)
            {
                SpriteBatch sb = Main.spriteBatch;
                try
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                        Main.DefaultSamplerState, DepthStencilState.None,
                        RasterizerState.CullCounterClockwise, null,
                        Main.GameViewMatrix.TransformationMatrix);

                    Texture2D ring = ModContent.Request<Texture2D>(
                        "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    Vector2 origin = ring.Size() / 2f;
                    Vector2 drawPos = Projectile.Center - Main.screenPosition;

                    sb.Draw(ring, drawPos, null, (DiesIraePalette.JudgmentGold with { A = 0 }) * 0.40f,
                        0f, origin, 0.16f * Projectile.scale, SpriteEffects.None, 0f);
                }
                catch { }
                finally
                {
                    try { sb.End(); } catch { }
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            return false;
        }
    }
}
