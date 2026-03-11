using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.ClairDeLune;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.GearDrivenArbiter.Projectiles
{
    /// <summary>
    /// Arbiter Gear — Spinning judgment gear fired by ArbiterMinion.
    /// Enhanced variant gains homing. Applies judgment stacks on hit,
    /// triggers ArbiterVerdict at 8 stacks.
    /// 3 render passes: (1) JudgmentMark JudgmentMarkSigil judgment aura,
    /// (2) GearSwing GearSwingTrail spinning body, (3) Multi-scale bloom + gear teeth.
    /// </summary>
    public class ArbiterGearProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const float GearRadius = 14f;
        private const float SpinRate = MathHelper.Pi / 15f;
        private const int ToothCount = 10;
        private const int MaxJudgmentStacks = 8;
        private const float HomingRange = 200f;
        private const float HomingStrength = 0.06f;

        // ai[0] = 1 for enhanced variant
        private bool IsEnhanced => Projectile.ai[0] == 1f;

        // --- Shader + texture caching ---
        private static Effect _judgmentShader;
        private static Effect _gearSwingShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _starFlare;
        private VertexStrip _vertexStrip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation += SpinRate;

            // Enhanced variant: gentle homing
            if (IsEnhanced)
            {
                NPC closest = null;
                float closestDist = HomingRange;
                for (int n = 0; n < Main.maxNPCs; n++)
                {
                    NPC npc = Main.npc[n];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }

                if (closest != null)
                {
                    Vector2 toTarget = (closest.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), HomingStrength);
                }
            }

            // Trail sparks
            if (Main.GameUpdateCount % 3 == 0)
            {
                float sparkAngle = Projectile.rotation + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * GearRadius * 0.7f;
                Color sparkCol = IsEnhanced ? ClairDeLunePalette.MoonbeamGold : ClairDeLunePalette.ClockworkBrass;
                var spark = new GenericGlowParticle(sparkPos,
                    sparkAngle.ToRotationVector2() * 1f,
                    sparkCol with { A = 0 } * 0.25f, 0.03f, 5, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            float lightIntensity = IsEnhanced ? 0.3f : 0.2f;
            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.ClockworkBrass.ToVector3() * lightIntensity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.15f, Volume = 0.35f }, Projectile.Center);

            // Track judgment stacks via npc.GetGlobalNPC (simplified: using ai[1] counter)
            // In a real implementation this would use a GlobalNPC to track per-NPC stacks.
            // For now, accumulate and check threshold:
            Projectile.ai[1]++;

            if (Projectile.ai[1] >= MaxJudgmentStacks)
            {
                // Trigger Verdict
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center, Vector2.Zero,
                    ModContent.ProjectileType<ArbiterVerdictProjectile>(),
                    (int)(Projectile.damage * 2f), 10f, Projectile.owner);
                Projectile.ai[1] = 0;
            }

            // Impact burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 2f;
                var imp = new GenericGlowParticle(target.Center, vel,
                    ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.35f, 0.04f, 6, true);
                MagnumParticleHandler.SpawnParticle(imp);
            }
        }

        private void LoadTextures()
        {
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _starFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.ClairDeLune, ref _vertexStrip);

                // --- Arbiter judgment moonbeam glow ---
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                var glowTex = MagnumTextureRegistry.GetSoftGlow();
                Vector2 origin = glowTex.Size() / 2f;
                Vector2 pos = Projectile.Center - Main.screenPosition;
                float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.timeForVisualEffects * 0.06);
                Color gold = (ClairDeLunePalette.MoonbeamGold with { A = 0 }) * 0.6f * pulse;
                Color silver = (ClairDeLunePalette.StarlightSilver with { A = 0 }) * 0.3f;
                sb.Draw(glowTex, pos, null, gold, 0f, origin, 0.045f, SpriteEffects.None, 0f);
                sb.Draw(glowTex, pos, null, silver, 0f, origin, 0.065f, SpriteEffects.None, 0f);

                sb.End();
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        // ---- PASS 1: JudgmentMark JudgmentMarkSigil judgment aura ----
        private void DrawJudgmentAura(SpriteBatch sb, Matrix matrix)
        {
            _judgmentShader ??= ShaderLoader.JudgmentMark;
            if (_judgmentShader == null) return;

            float judgmentIntensity = IsEnhanced ? 0.5f : 0.3f;

            sb.End();

            _judgmentShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _judgmentShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.ClockworkBrass.ToVector4());
            _judgmentShader.Parameters["uOpacity"]?.SetValue(judgmentIntensity);
            _judgmentShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _judgmentShader.Parameters["uIntensity"]?.SetValue(IsEnhanced ? 1.3f : 0.8f);
            _judgmentShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _judgmentShader.Parameters["uScrollSpeed"]?.SetValue(1.5f);
            _judgmentShader.Parameters["uDistortionAmt"]?.SetValue(0.015f);
            _judgmentShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _judgmentShader.CurrentTechnique = _judgmentShader.Techniques["JudgmentMarkSigil"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _judgmentShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float auraScale = GearRadius * 2.8f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, Main.GlobalTimeWrappedHourly, sc.Size() * 0.5f, auraScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: GearSwing GearSwingTrail spinning body ----
        private void DrawGearSwingBody(SpriteBatch sb, Matrix matrix)
        {
            _gearSwingShader ??= ShaderLoader.GearSwing;
            if (_gearSwingShader == null) return;

            sb.End();

            _gearSwingShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.ClockworkBrass.ToVector4());
            _gearSwingShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _gearSwingShader.Parameters["uOpacity"]?.SetValue(0.6f);
            _gearSwingShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _gearSwingShader.Parameters["uIntensity"]?.SetValue(1f);
            _gearSwingShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _gearSwingShader.Parameters["uScrollSpeed"]?.SetValue(2f);
            _gearSwingShader.Parameters["uDistortionAmt"]?.SetValue(0.015f);
            _gearSwingShader.Parameters["uHasSecondaryTex"]?.SetValue(false);
            _gearSwingShader.Parameters["uPhase"]?.SetValue(Projectile.rotation);

            _gearSwingShader.CurrentTechnique = _gearSwingShader.Techniques["GearSwingTrail"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _gearSwingShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float bodyScale = GearRadius * 2f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, Projectile.rotation, sc.Size() * 0.5f, bodyScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom + gear teeth ----
        private void DrawBloomTeeth(SpriteBatch sb, Matrix matrix)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            Texture2D sf = _starFlare.Value;
            float enhanceMult = IsEnhanced ? 1.3f : 1f;

            // Outer judgment haze
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.12f * enhanceMult, 0f, srb.Size() * 0.5f,
                GearRadius * 2.2f / srb.Width, SpriteEffects.None, 0f);

            // Brass glow
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.2f * enhanceMult, 0f, srb.Size() * 0.5f,
                GearRadius * 1.2f / srb.Width, SpriteEffects.None, 0f);

            // Gold core
            sb.Draw(pb, pos, null,
                ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.15f * enhanceMult, 0f, pb.Size() * 0.5f,
                GearRadius * 0.5f / pb.Width, SpriteEffects.None, 0f);

            // 10 gear teeth
            for (int t = 0; t < ToothCount; t++)
            {
                float toothAngle = Projectile.rotation + t * MathHelper.TwoPi / ToothCount;
                Vector2 toothPos = pos + toothAngle.ToRotationVector2() * GearRadius;

                sb.Draw(pb, toothPos, null,
                    ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.12f * enhanceMult, toothAngle, pb.Size() * 0.5f,
                    3.5f / pb.Width, SpriteEffects.None, 0f);
            }

            // Enhanced gold star flare
            if (IsEnhanced)
            {
                float flareRot = Main.GlobalTimeWrappedHourly * 2.5f;
                sb.Draw(sf, pos, null,
                    ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.12f, flareRot, sf.Size() * 0.5f,
                    GearRadius * 0.4f / sf.Width, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
