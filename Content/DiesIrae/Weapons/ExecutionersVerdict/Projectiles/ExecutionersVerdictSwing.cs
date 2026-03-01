using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Utilities;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Primitives;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Buffs;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Shaders;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Projectiles
{
    /// <summary>
    /// Executioner's Verdict main swing projectile.
    /// 3-phase combo: Horizontal Cleave → Overhead Slam → GUILLOTINE DROP.
    /// Each phase heavier than the last. Phase 3 is a devastating vertical slam
    /// with screen shake, dark smoke eruption, and execution-mark application.
    /// Uses CurveSegment animation and GPU primitive trail rendering.
    /// </summary>
    public class ExecutionersVerdictSwing : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private enum SwingPhase { Cleave, Overhead, GuillotineDrop }

        // Trail cache for primitive rendering
        private readonly List<Vector2> trailCache = new List<Vector2>(20);
        private const int TrailLength = 20;

        // Textures
        private static Asset<Texture2D> bloomTex;
        private static Asset<Texture2D> glowTex;

        // Swing timing per combo phase (in frames)
        private static readonly int[] SwingDurations = { 28, 32, 40 };

        // Arc angles per combo phase (radians)
        private static readonly float[] SwingArcs = { MathHelper.Pi * 1.1f, MathHelper.Pi * 1.3f, MathHelper.Pi * 1.6f };

        // Start offsets (rotation relative to mouse direction)
        private static readonly float[] StartOffsets = { -MathHelper.PiOver2 * 1.1f, MathHelper.PiOver2 * 1.0f, -MathHelper.PiOver4 };

        // References
        private Player Owner => Main.player[Projectile.owner];
        private ref float Timer => ref Projectile.ai[0];
        private ref float ComboIndex => ref Projectile.ai[1];
        private SwingPhase Phase => (SwingPhase)(int)MathHelper.Clamp(ComboIndex, 0, 2);
        private int Duration => SwingDurations[(int)Phase];
        private float SwingProgress => Timer / Duration;
        private float BladeLength => 120f + (int)Phase * 15f; // Gets bigger each phase

        public override void SetStaticDefaults() { }

        public override void SetDefaults()
        {
            Projectile.width = 160;
            Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 18;
            Projectile.extraUpdates = 0;
            Projectile.timeLeft = 200;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            if (!Owner.active || Owner.dead)
            {
                Projectile.Kill();
                return;
            }

            // Lock player to this swing
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            // Compute current blade rotation using CurveSegment animation
            float currentAngle = ComputeSwingAngle();

            // Position at player center
            Projectile.Center = Owner.MountedCenter;
            Projectile.rotation = currentAngle;

            // Direction facing
            float mouseAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();
            Owner.ChangeDir(Math.Cos(mouseAngle) >= 0 ? 1 : -1);

            // Visual effects per phase
            SpawnSwingParticles(currentAngle);

            // Cache trail points (blade tip positions)
            Vector2 tipPos = Owner.MountedCenter + currentAngle.ToRotationVector2() * BladeLength;
            trailCache.Add(tipPos);
            if (trailCache.Count > TrailLength)
                trailCache.RemoveAt(0);

            // Phase 3 (Guillotine Drop) specific: heavy screen distortion during downswing
            if (Phase == SwingPhase.GuillotineDrop && SwingProgress > 0.4f && SwingProgress < 0.8f)
            {
                var vPlayer = Owner.ExecutionersVerdict();
                vPlayer.ScreenShakeIntensity = Math.Max(vPlayer.ScreenShakeIntensity, 4f + SwingProgress * 6f);
            }

            Timer++;
            if (Timer >= Duration)
            {
                // Sound on completion, heavier for later phases
                float pitch = -0.2f - (int)Phase * 0.15f;
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = pitch, Volume = 0.8f + (int)Phase * 0.2f }, Projectile.Center);

                // Phase 3: massive slam impact
                if (Phase == SwingPhase.GuillotineDrop)
                    TriggerGuillotineImpact(tipPos);

                // Advance combo
                Owner.ExecutionersVerdict().AdvanceCombo();
                Projectile.Kill();
            }
        }

        private float ComputeSwingAngle()
        {
            int phase = (int)Phase;
            float arc = SwingArcs[phase];
            float startOffset = StartOffsets[phase];
            float mouseAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();
            int dir = Owner.direction;

            float startAngle = mouseAngle + startOffset * dir;
            float progress;

            if (Phase == SwingPhase.GuillotineDrop)
            {
                // Heavy guillotine easing: slow wind-up, FAST drop
                progress = ExecutionersVerdictUtils.GuillotineDrop(SwingProgress);
            }
            else if (Phase == SwingPhase.Overhead)
            {
                // Overhead: medium windup, strong follow-through
                var segments = new ExecutionersVerdictUtils.CurveSegment[]
                {
                    new(0f, 0.3f, 0f, 0.15f, t => ExecutionersVerdictUtils.EaseInPoly(t)),
                    new(0.3f, 0.4f, 0.15f, 0.85f, t => ExecutionersVerdictUtils.EaseOutPoly(t, 3f)),
                    new(0.7f, 0.3f, 0.85f, 1f, t => ExecutionersVerdictUtils.EaseOutPoly(t)),
                };
                progress = ExecutionersVerdictUtils.PiecewiseAnimation(SwingProgress, segments);
            }
            else
            {
                // Cleave: quick, clean horizontal slash
                var segments = new ExecutionersVerdictUtils.CurveSegment[]
                {
                    new(0f, 0.2f, 0f, 0.1f, t => ExecutionersVerdictUtils.EaseInPoly(t)),
                    new(0.2f, 0.5f, 0.1f, 0.9f, t => ExecutionersVerdictUtils.EaseOutPoly(t, 2.5f)),
                    new(0.7f, 0.3f, 0.9f, 1f, t => ExecutionersVerdictUtils.EaseOutPoly(t)),
                };
                progress = ExecutionersVerdictUtils.PiecewiseAnimation(SwingProgress, segments);
            }

            return startAngle + arc * progress * dir;
        }

        private void SpawnSwingParticles(float angle)
        {
            Vector2 tipPos = Owner.MountedCenter + angle.ToRotationVector2() * BladeLength;
            Vector2 midPos = Owner.MountedCenter + angle.ToRotationVector2() * (BladeLength * 0.6f);

            // Blood drips from blade
            if (SwingProgress > 0.15f && Main.rand.NextBool(3))
            {
                VerdictParticleHandler.SpawnParticle(new BloodDripParticle(
                    tipPos + Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextVector2Circular(1f, 1f) + new Vector2(0, 1f),
                    Main.rand.NextFloat(0.3f, 0.6f),
                    Main.rand.Next(30, 50)));
            }

            // Heavy smoke behind blade on fast-moving sections
            if (SwingProgress > 0.2f && SwingProgress < 0.8f && Main.rand.NextBool(2))
            {
                VerdictParticleHandler.SpawnParticle(new JudgmentSmokeParticle(
                    midPos + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(1.5f, 1.5f),
                    Main.rand.NextFloat(0.6f, 1.2f),
                    Main.rand.Next(35, 55)));
            }

            // Ember shards at blade tip in fast swing
            if (SwingProgress > 0.25f && SwingProgress < 0.75f && Main.rand.NextBool(3))
            {
                Vector2 shardVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f)
                    + Main.rand.NextVector2Circular(2f, 2f);
                VerdictParticleHandler.SpawnParticle(new EmberShardParticle(
                    tipPos, shardVel, Main.rand.NextFloat(0.3f, 0.7f), Main.rand.Next(20, 35)));
            }

            // Guillotine drop: extra heavy smoke and sparks
            if (Phase == SwingPhase.GuillotineDrop && SwingProgress > 0.35f)
            {
                for (int i = 0; i < 2; i++)
                {
                    VerdictParticleHandler.SpawnParticle(new JudgmentSmokeParticle(
                        tipPos + Main.rand.NextVector2Circular(20f, 20f),
                        Main.rand.NextVector2Circular(3f, 2f) + new Vector2(0, -1),
                        Main.rand.NextFloat(0.8f, 1.5f),
                        Main.rand.Next(30, 50)));
                }
            }
        }

        private void TriggerGuillotineImpact(Vector2 impactPos)
        {
            // Massive execution bloom
            VerdictParticleHandler.SpawnParticle(new ExecutionBloomParticle(impactPos, 3.5f, 30));

            // Ground-pound smoke eruption
            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi / 15f * i;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 8f);
                VerdictParticleHandler.SpawnParticle(new JudgmentSmokeParticle(
                    impactPos + Main.rand.NextVector2Circular(10f, 10f),
                    vel, Main.rand.NextFloat(1f, 2f), Main.rand.Next(40, 60)));
            }

            // Ember shard spray
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(6f, 6f) + new Vector2(0, -3);
                VerdictParticleHandler.SpawnParticle(new EmberShardParticle(
                    impactPos, vel, Main.rand.NextFloat(0.5f, 0.9f), Main.rand.Next(25, 40)));
            }

            // Judgment notes rising from impact
            for (int i = 0; i < 6; i++)
            {
                VerdictParticleHandler.SpawnParticle(new JudgmentNoteParticle(
                    impactPos + Main.rand.NextVector2Circular(30f, 30f),
                    new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-4f, -2f)),
                    ExecutionersVerdictUtils.BloodRed,
                    Main.rand.NextFloat(0.6f, 1f), Main.rand.Next(45, 65)));
            }

            // Blood drip shower
            for (int i = 0; i < 10; i++)
            {
                VerdictParticleHandler.SpawnParticle(new BloodDripParticle(
                    impactPos + Main.rand.NextVector2Circular(20f, 10f),
                    new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-6f, -1f)),
                    Main.rand.NextFloat(0.4f, 0.7f), Main.rand.Next(35, 55)));
            }

            // Heavy screen shake
            Owner.ExecutionersVerdict().ScreenShakeIntensity = 15f;

            // Sound
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = -0.5f, Volume = 1.2f }, impactPos);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply debuffs
            target.AddBuff(ModContent.BuffType<ExecutionBrand>(), 300);
            target.AddBuff(ModContent.BuffType<PyreImmolation>(), 180);
            target.AddBuff(BuffID.OnFire3, 240);

            // Check execution threshold
            float healthPercent = (float)target.life / target.lifeMax;
            if (healthPercent < 0.15f && !target.boss)
            {
                // EXECUTE
                target.life = 0;
                target.HitEffect();
                target.checkDead();

                Owner.ExecutionersVerdict().RegisterExecution();

                // Massive execution VFX
                VerdictParticleHandler.SpawnParticle(new ExecutionBloomParticle(target.Center, 4f, 35));

                for (int i = 0; i < 20; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(5f, 5f);
                    VerdictParticleHandler.SpawnParticle(new EmberShardParticle(
                        target.Center, vel, Main.rand.NextFloat(0.5f, 1f), Main.rand.Next(30, 50)));
                }

                for (int i = 0; i < 8; i++)
                {
                    VerdictParticleHandler.SpawnParticle(new JudgmentNoteParticle(
                        target.Center + Main.rand.NextVector2Circular(20f, 20f),
                        Main.rand.NextVector2Circular(3f, 3f),
                        ExecutionersVerdictUtils.AshWhite,
                        Main.rand.NextFloat(0.7f, 1.2f), Main.rand.Next(40, 60)));
                }

                SoundEngine.PlaySound(SoundID.NPCDeath59 with { Pitch = -0.5f, Volume = 1f }, target.Center);
            }
            else
            {
                // Standard hit VFX
                VerdictParticleHandler.SpawnParticle(new ExecutionBloomParticle(target.Center, 1.2f, 18));

                for (int i = 0; i < 5; i++)
                {
                    VerdictParticleHandler.SpawnParticle(new EmberShardParticle(
                        target.Center, Main.rand.NextVector2Circular(4f, 4f),
                        Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(15, 30)));
                }
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 50% more damage below 30% HP
            float healthPercent = (float)target.life / target.lifeMax;
            if (healthPercent < 0.30f)
                modifiers.FinalDamage *= 1.5f;

            // Guillotine Drop deals 25% bonus damage
            if (Phase == SwingPhase.GuillotineDrop)
                modifiers.FinalDamage *= 1.25f;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Arc collision along blade
            float angle = Projectile.rotation;
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + angle.ToRotationVector2() * BladeLength;
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 30f + (int)Phase * 5f, ref collisionPoint);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // Draw GPU primitive trail
            DrawGuillotineTrail(sb);

            // Draw blade glow layers
            DrawBladeGlow(sb);

            // Draw execution mark intensity indicator near low-HP targets
            DrawNearbyExecutionMarks(sb);

            return false;
        }

        private void DrawGuillotineTrail(SpriteBatch sb)
        {
            if (trailCache.Count < 3) return;

            try
            {
                sb.End();

                var settings = new VerdictTrailSettings(
                    widthFunc: p =>
                    {
                        float baseWidth = 40f + (int)Phase * 10f;
                        float fade = (float)Math.Sin(p * MathHelper.Pi) * (1f - p * 0.3f);
                        return baseWidth * fade;
                    },
                    colorFunc: p =>
                    {
                        Color trailColor = ExecutionersVerdictUtils.PaletteLerp(p * 0.7f + 0.2f);
                        float alpha = (1f - p) * (SwingProgress < 0.3f ? SwingProgress / 0.3f : 1f);
                        return trailColor * alpha;
                    },
                    smoothing: 4,
                    shaderSetup: () =>
                    {
                        var device = Main.graphics.GraphicsDevice;
                        device.BlendState = BlendState.Additive;
                        device.RasterizerState = RasterizerState.CullNone;

                        // Apply GuillotineBlade shader for dark-blade-with-blood-edge character
                        if (ExecutionersVerdictShaderLoader.HasGuillotine)
                        {
                            var shader = ExecutionersVerdictShaderLoader.GuillotineShader.Value;
                            shader.Parameters["uTime"]?.SetValue((float)Main.GameUpdateCount * 0.04f);
                            shader.Parameters["uColor"]?.SetValue(ExecutionersVerdictUtils.VoidBlack.ToVector3());
                            shader.Parameters["uSecondaryColor"]?.SetValue(ExecutionersVerdictUtils.BloodRed.ToVector3());
                            shader.Parameters["uOpacity"]?.SetValue(1f);
                            shader.Parameters["uIntensity"]?.SetValue(1.5f + (int)Phase * 0.3f);
                            shader.Parameters["uScrollSpeed"]?.SetValue(1.2f);
                            shader.Parameters["uDistortionAmt"]?.SetValue(0.06f + (Phase == SwingPhase.GuillotineDrop ? 0.08f : 0f));
                            shader.Parameters["uOverbrightMult"]?.SetValue(2.8f + (int)Phase * 0.4f);
                            shader.Parameters["uExecuteThreshold"]?.SetValue(Phase == SwingPhase.GuillotineDrop ? 0.9f : 0.2f + (int)Phase * 0.15f);
                            shader.CurrentTechnique = shader.Techniques["GuillotineSlashTechnique"];
                            shader.CurrentTechnique.Passes[0].Apply();
                        }
                    });

                VerdictTrailRenderer.RenderTrail(trailCache, settings);

                Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            }
            catch { }
            finally
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        private void DrawBladeGlow(SpriteBatch sb)
        {
            bloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!bloomTex.IsLoaded || !glowTex.IsLoaded) return;

            float angle = Projectile.rotation;
            Vector2 tipPos = Owner.MountedCenter + angle.ToRotationVector2() * BladeLength;
            Vector2 midPos = Owner.MountedCenter + angle.ToRotationVector2() * (BladeLength * 0.6f);
            Vector2 rootPos = Owner.MountedCenter + angle.ToRotationVector2() * (BladeLength * 0.25f);

            float swingIntensity = (float)Math.Sin(SwingProgress * MathHelper.Pi);
            float time = Main.GlobalTimeWrappedHourly;

            // End current batch, start additive
            sb.End();
            ExecutionersVerdictUtils.BeginAdditive(sb);

            var tipBloom = bloomTex.Value;
            var coreGlow = glowTex.Value;

            // Layer 1: Blood-red outer glow at blade tip
            float tipScale = (0.4f + swingIntensity * 0.4f) * (1f + (int)Phase * 0.15f);
            sb.Draw(tipBloom, tipPos - Main.screenPosition, null,
                ExecutionersVerdictUtils.Additive(ExecutionersVerdictUtils.BloodRed, swingIntensity * 0.5f),
                0f, tipBloom.Size() / 2f, tipScale, SpriteEffects.None, 0f);

            // Layer 2: Burning core at blade mid — stretched along blade
            float coreScale = 0.3f + swingIntensity * 0.3f;
            sb.Draw(coreGlow, midPos - Main.screenPosition, null,
                ExecutionersVerdictUtils.Additive(ExecutionersVerdictUtils.BurningCrimson, swingIntensity * 0.4f),
                angle, coreGlow.Size() / 2f, new Vector2(coreScale * 2f, coreScale), SpriteEffects.None, 0f);

            // Layer 3: Dark edge aura along blade root — unique void-black radiance
            float rootPulse = 0.2f + swingIntensity * 0.15f;
            sb.Draw(tipBloom, rootPos - Main.screenPosition, null,
                ExecutionersVerdictUtils.Additive(ExecutionersVerdictUtils.DarkCrimson, rootPulse),
                angle, tipBloom.Size() / 2f, new Vector2(coreScale * 1.5f, coreScale * 0.5f), SpriteEffects.None, 0f);

            // Layer 4: Guillotine Drop — escalating heavy bloom with ash-white flash
            if (Phase == SwingPhase.GuillotineDrop && SwingProgress > 0.3f)
            {
                float dropIntensity = (SwingProgress - 0.3f) / 0.7f;
                float dropPulse = 1f + (float)Math.Sin(time * 12f) * 0.1f * dropIntensity;
                sb.Draw(tipBloom, tipPos - Main.screenPosition, null,
                    ExecutionersVerdictUtils.Additive(ExecutionersVerdictUtils.AshWhite, dropIntensity * 0.5f),
                    0f, tipBloom.Size() / 2f, tipScale * 1.6f * dropPulse, SpriteEffects.None, 0f);

                // Ember glow haze from blade — widens as drop accelerates
                sb.Draw(tipBloom, midPos - Main.screenPosition, null,
                    ExecutionersVerdictUtils.Additive(ExecutionersVerdictUtils.EmberGlow, dropIntensity * 0.3f),
                    angle, tipBloom.Size() / 2f, new Vector2(tipScale * 2f, tipScale * 0.8f) * dropPulse, SpriteEffects.None, 0f);
            }

            // Layer 5: Blade tip cross-flare (judgment glint)
            if (swingIntensity > 0.3f)
            {
                float flareAngle = time * 3f;
                float flareAlpha = (swingIntensity - 0.3f) / 0.7f * 0.35f;
                float flareLen = 15f + swingIntensity * 10f;
                sb.Draw(coreGlow, tipPos - Main.screenPosition, null,
                    ExecutionersVerdictUtils.Additive(ExecutionersVerdictUtils.ExecutionGold, flareAlpha),
                    flareAngle, coreGlow.Size() / 2f, new Vector2(flareLen / coreGlow.Width, 2f / coreGlow.Height) * 4f, SpriteEffects.None, 0f);
                sb.Draw(coreGlow, tipPos - Main.screenPosition, null,
                    ExecutionersVerdictUtils.Additive(ExecutionersVerdictUtils.ExecutionGold, flareAlpha),
                    flareAngle + MathHelper.PiOver2, coreGlow.Size() / 2f, new Vector2(flareLen / coreGlow.Width, 2f / coreGlow.Height) * 4f, SpriteEffects.None, 0f);
            }

            sb.End();
            ExecutionersVerdictUtils.ResetSpriteBatch(sb);
        }

        private void DrawNearbyExecutionMarks(SpriteBatch sb)
        {
            bloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!bloomTex.IsLoaded) return;

            bool hasMarks = false;

            // Show execution marks above low-HP enemies in range
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy() || npc.boss) continue;
                if (Vector2.Distance(Owner.Center, npc.Center) > 600f) continue;

                float hp = (float)npc.life / npc.lifeMax;
                if (hp > 0.30f) continue;

                Color markColor = ExecutionersVerdictUtils.GetExecutionColor(hp);
                if (markColor == Color.Transparent) continue;

                if (!hasMarks)
                {
                    sb.End();
                    ExecutionersVerdictUtils.BeginAdditive(sb);
                    hasMarks = true;
                }

                Vector2 markPos = npc.Top + new Vector2(0, -20f);
                float executeUrgency = 1f - hp / 0.30f;
                float pulse = 0.3f + executeUrgency * 0.5f;
                float pulseMod = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * (4f + executeUrgency * 6f)) * (0.1f + executeUrgency * 0.2f);

                var tex = bloomTex.Value;

                // Outer ring — expands and contracts rhythmically
                float ringScale = pulse * pulseMod * 0.6f;
                sb.Draw(tex, markPos - Main.screenPosition, null,
                    ExecutionersVerdictUtils.Additive(markColor, 0.6f),
                    0f, tex.Size() / 2f, ringScale, SpriteEffects.None, 0f);

                // Core glow — intensifies as HP drops
                sb.Draw(tex, markPos - Main.screenPosition, null,
                    ExecutionersVerdictUtils.Additive(ExecutionersVerdictUtils.EmberGlow, 0.3f * executeUrgency),
                    0f, tex.Size() / 2f, ringScale * 0.5f, SpriteEffects.None, 0f);

                // Below execute threshold — WHITE flash with cross-flare
                if (hp < 0.15f)
                {
                    float flashUrgency = 1f - hp / 0.15f;
                    float flashPulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 12f) * 0.3f;
                    sb.Draw(tex, markPos - Main.screenPosition, null,
                        ExecutionersVerdictUtils.Additive(ExecutionersVerdictUtils.AshWhite, 0.5f * flashUrgency * flashPulse),
                        0f, tex.Size() / 2f, pulse * pulseMod * 0.3f, SpriteEffects.None, 0f);

                    // Vertical execution line above target
                    if (glowTex.IsLoaded)
                    {
                        var glow = glowTex.Value;
                        float lineAlpha = flashUrgency * 0.4f * flashPulse;
                        sb.Draw(glow, markPos - Main.screenPosition, null,
                            ExecutionersVerdictUtils.Additive(ExecutionersVerdictUtils.BloodRed, lineAlpha),
                            0f, glow.Size() / 2f, new Vector2(0.06f, 0.8f + flashUrgency * 0.4f), SpriteEffects.None, 0f);
                    }
                }
            }

            if (hasMarks)
            {
                sb.End();
                ExecutionersVerdictUtils.ResetSpriteBatch(sb);
            }
        }

        public override bool ShouldUpdatePosition() => false;
    }
}
