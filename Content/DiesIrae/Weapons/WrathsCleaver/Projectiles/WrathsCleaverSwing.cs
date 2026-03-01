using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Buffs;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Primitives;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Utilities;
using static MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Utilities.WrathsCleaverUtils;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Projectiles
{
    /// <summary>
    /// Wrath's Cleaver main swing projectile.
    /// 5-phase combo with CurveSegment animation, shader-driven trail rendering,
    /// infernal eruption on full wrath meter, and a lunge dash on combo finisher.
    /// Architecture follows the Exoblade/EternalMoon per-weapon pattern.
    /// </summary>
    public class WrathsCleaverSwing : ModProjectile
    {
        // ── SWING STATE ──
        private enum SwingState { Swinging, InfernalLunge }

        private SwingState State
        {
            get => (SwingState)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }

        private float SwingProgress => Projectile.ai[1];
        private ref float SwingTimer => ref Projectile.ai[1];
        private ref float ComboIndex => ref Projectile.ai[2];

        private Player Owner => Main.player[Projectile.owner];
        private int SwingDirection => Owner.direction * (ComboIndex % 2 == 0 ? 1 : -1);

        // Swing duration in ticks per combo step (gets faster in later steps)
        private int SwingDuration => ComboIndex switch
        {
            0 => 18,  // Opening — measured
            1 => 14,  // Riposte — quick
            2 => 16,  // Cross slash — standard
            3 => 12,  // Flurry — fast
            4 => 22,  // Finisher — slow but devastating
            _ => 16
        };

        // Swing arc (radians) per combo step
        private float SwingArc => ComboIndex switch
        {
            0 => MathHelper.Pi * 0.85f,
            1 => MathHelper.Pi * 0.75f,
            2 => MathHelper.Pi * 0.90f,
            3 => MathHelper.Pi * 0.70f,
            4 => MathHelper.Pi * 1.1f, // Big overhead finisher
            _ => MathHelper.Pi * 0.85f
        };

        // Trail cache for primitive rendering
        private readonly List<Vector2> trailPoints = new List<Vector2>();
        private const int MaxTrailPoints = 24;

        // ── ANIMATION CURVES ──
        // Following Exoblade pattern: Windup (slow) → Swing (fast) → Follow-through (decel)
        private static readonly CurveSegment WindupSlow = new CurveSegment(0f, 0.15f, t => EaseInPoly(t, 3f), 0f, 0.05f);
        private static readonly CurveSegment SwingFast = new CurveSegment(0.15f, 0.45f, t => EaseOutPoly(t, 2.5f), 0.05f, 0.85f);
        private static readonly CurveSegment Decelerate = new CurveSegment(0.60f, 0.40f, t => EaseInPoly(t, 2f), 0.85f, 1f);

        // Finisher combo step has different curves (bigger windup, heavier impact)
        private static readonly CurveSegment FinisherWindup = new CurveSegment(0f, 0.25f, t => EaseInPoly(t, 4f), 0f, 0.08f);
        private static readonly CurveSegment FinisherStrike = new CurveSegment(0.25f, 0.35f, t => EaseOutPoly(t, 2f), 0.08f, 0.90f);
        private static readonly CurveSegment FinisherFollow = new CurveSegment(0.60f, 0.40f, t => EaseInPoly(t, 3f), 0.90f, 1f);

        public override string Texture => "MagnumOpus/Content/DiesIrae/ResonantWeapons/WrathsCleaver";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = MaxTrailPoints;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.ownerHitCheck = true;
            Projectile.timeLeft = 600;
        }

        public override void AI()
        {
            if (!Owner.active || Owner.dead)
            {
                Projectile.Kill();
                return;
            }

            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            switch (State)
            {
                case SwingState.Swinging:
                    DoSwing();
                    break;
                case SwingState.InfernalLunge:
                    DoInfernalLunge();
                    break;
            }
        }

        private void DoSwing()
        {
            SwingTimer++;
            float progress = MathHelper.Clamp(SwingTimer / SwingDuration, 0f, 1f);

            // Compute swing angle from curves
            float animProgress;
            if (ComboIndex == 4)
                animProgress = PiecewiseAnimation(progress, FinisherWindup, FinisherStrike, FinisherFollow);
            else
                animProgress = PiecewiseAnimation(progress, WindupSlow, SwingFast, Decelerate);

            // Calculate blade position
            float baseAngle = Projectile.velocity.ToRotation();
            float startAngle = baseAngle - SwingArc * 0.5f * SwingDirection;
            float currentAngle = startAngle + SwingArc * animProgress * SwingDirection;

            float bladeLength = 110f + ComboIndex * 5f;
            Vector2 tipPos = Owner.Center + currentAngle.ToRotationVector2() * bladeLength;

            Projectile.Center = Owner.Center;
            Projectile.rotation = currentAngle;

            // Update trail cache
            trailPoints.Add(tipPos);
            if (trailPoints.Count > MaxTrailPoints)
                trailPoints.RemoveAt(0);

            // Spawn particles during swing
            SpawnSwingParticles(tipPos, currentAngle, progress);

            // Direction management
            Owner.ChangeDir(Math.Sign(Projectile.velocity.X));
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, currentAngle - MathHelper.PiOver2);

            // End swing
            if (progress >= 1f)
            {
                Projectile.Kill();
            }
        }

        private void DoInfernalLunge()
        {
            SwingTimer++;
            float progress = MathHelper.Clamp(SwingTimer / 20f, 0f, 1f);

            // Lunge forward
            if (SwingTimer <= 8)
            {
                float lungeSpeed = 28f * (1f - progress * 0.5f);
                Owner.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * lungeSpeed;
            }
            else
            {
                Owner.velocity *= 0.9f;
            }

            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 60f;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail during lunge
            trailPoints.Add(Projectile.Center);
            if (trailPoints.Count > MaxTrailPoints)
                trailPoints.RemoveAt(0);

            // Spawn lunge fire particles
            for (int i = 0; i < 3; i++)
            {
                var ember = new InfernalEmber(
                    Owner.Center + Main.rand.NextVector2Circular(20f, 20f),
                    -Owner.velocity * Main.rand.NextFloat(0.2f, 0.5f) + Main.rand.NextVector2Circular(3f, 3f),
                    Color.Lerp(WrathsCleaverUtils.EmberOrange, WrathsCleaverUtils.HellfireGold, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(15, 30));
                WrathParticleHandler.SpawnParticle(ember);
            }

            if (progress >= 1f)
                Projectile.Kill();
        }

        private void SpawnSwingParticles(Vector2 tipPos, float angle, float progress)
        {
            // Ember sparks along the blade edge
            if (Main.rand.NextBool(2))
            {
                float sparkT = Main.rand.NextFloat(0.3f, 1f);
                Vector2 sparkPos = Owner.Center + angle.ToRotationVector2() * (110f * sparkT);
                Vector2 sparkVel = (angle + MathHelper.PiOver2 * SwingDirection).ToRotationVector2() * Main.rand.NextFloat(2f, 6f);

                var ember = new InfernalEmber(sparkPos, sparkVel,
                    PaletteLerp(sparkT), Main.rand.NextFloat(0.2f, 0.45f), Main.rand.Next(12, 22));
                WrathParticleHandler.SpawnParticle(ember);
            }

            // Smoke at the trail head during fast portion of swing
            if (progress > 0.15f && progress < 0.7f && Main.rand.NextBool(3))
            {
                var smoke = new WrathSmoke(tipPos, Main.rand.NextVector2Circular(2f, 2f),
                    WrathsCleaverUtils.CharcoalBlack * 0.6f, Main.rand.NextFloat(0.3f, 0.5f),
                    Main.rand.Next(30, 50));
                WrathParticleHandler.SpawnParticle(smoke);
            }

            // Music notes at certain intervals
            if (Main.rand.NextBool(8))
            {
                var note = new HellfireNote(tipPos, Main.rand.NextVector2Circular(2f, 2f),
                    Color.Lerp(WrathsCleaverUtils.BloodRed, WrathsCleaverUtils.HellfireGold, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(25, 45));
                WrathParticleHandler.SpawnParticle(note);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Hellfire Immolation debuff
            target.AddBuff(ModContent.BuffType<HellfireImmolation>(), 300);
            target.AddBuff(BuffID.OnFire3, 300);

            if (hit.Crit)
            {
                target.AddBuff(BuffID.Daybreak, 180);
                // Extra ember burst on crit
                for (int i = 0; i < 12; i++)
                {
                    var spark = new CrystallizedFlameSpark(
                        target.Center, Main.rand.NextVector2Circular(8f, 8f),
                        Color.Lerp(Color.White, WrathsCleaverUtils.EmberOrange, Main.rand.NextFloat(0.3f, 0.7f)),
                        Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(15, 30));
                    WrathParticleHandler.SpawnParticle(spark);
                }
            }

            // Impact bloom
            var bloom = new WrathBloom(target.Center,
                Color.Lerp(WrathsCleaverUtils.BloodRed, WrathsCleaverUtils.InfernalRed, 0.5f),
                1.5f, 12);
            WrathParticleHandler.SpawnParticle(bloom);

            // Impact embers
            for (int i = 0; i < 6; i++)
            {
                var ember = new InfernalEmber(
                    target.Center,
                    Main.rand.NextVector2Circular(7f, 7f) - Vector2.UnitY * 2f,
                    PaletteLerp(Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(15, 25));
                WrathParticleHandler.SpawnParticle(ember);
            }

            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 0.6f, Pitch = -0.2f }, target.Center);

            // Wrath meter builds with hits
            var wrathPlayer = Owner.WrathsCleaver();
            if (wrathPlayer.AddWrath(hit.Crit ? 20f : 10f))
            {
                // INFERNAL ERUPTION — wrath meter full!
                TriggerInfernalEruption(target.Center);
            }
        }

        private void TriggerInfernalEruption(Vector2 center)
        {
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = -0.5f, Volume = 1.2f }, center);

            // Apply Wrath Mark to all nearby enemies
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && Vector2.Distance(center, npc.Center) < 400f)
                    npc.AddBuff(ModContent.BuffType<WrathMark>(), 300);
            }

            // Massive particle eruption
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 12f);
                var ember = new InfernalEmber(center, vel,
                    PaletteLerp(Main.rand.NextFloat(0.3f, 0.9f)),
                    Main.rand.NextFloat(0.4f, 0.8f), Main.rand.Next(20, 40));
                WrathParticleHandler.SpawnParticle(ember);
            }

            for (int i = 0; i < 8; i++)
            {
                var smoke = new WrathSmoke(center + Main.rand.NextVector2Circular(30f, 30f),
                    Main.rand.NextVector2Circular(3f, 3f) - Vector2.UnitY * 2f,
                    WrathsCleaverUtils.CharcoalBlack * 0.8f, Main.rand.NextFloat(0.5f, 0.8f),
                    Main.rand.Next(40, 70));
                WrathParticleHandler.SpawnParticle(smoke);
            }

            for (int i = 0; i < 10; i++)
            {
                var note = new HellfireNote(center, Main.rand.NextVector2Circular(5f, 5f),
                    WrathsCleaverUtils.HellfireGold, Main.rand.NextFloat(0.4f, 0.7f),
                    Main.rand.Next(30, 55));
                WrathParticleHandler.SpawnParticle(note);
            }

            var bigBloom = new WrathBloom(center, WrathsCleaverUtils.InfernalRed, 3f, 20);
            WrathParticleHandler.SpawnParticle(bigBloom);
        }

        // ── RENDERING ──

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            if (State == SwingState.Swinging)
            {
                DrawSlashTrail(sb);
                DrawBlade(sb, lightColor);
                DrawBladeGlow(sb);
            }
            else if (State == SwingState.InfernalLunge)
            {
                DrawLungeTrail(sb);
            }

            return false;
        }

        private void DrawSlashTrail(SpriteBatch sb)
        {
            if (trailPoints.Count < 3) return;

            // End SpriteBatch to enter shader/primitive mode
            sb.End();

            var settings = new WrathTrailSettings(
                widthFunc: t =>
                {
                    float baseWidth = 35f + ComboIndex * 3f;
                    float taper = (float)Math.Sin(t * MathHelper.Pi);
                    return baseWidth * taper;
                },
                colorFunc: t =>
                {
                    Color trailColor = PaletteLerp(t * 0.8f + 0.1f);
                    float fade = (float)Math.Sin(t * MathHelper.Pi);
                    return Additive(trailColor, fade * 0.85f);
                },
                smoothingSteps: 8,
                shaderSetup: () =>
                {
                    // Try to use our custom shader; fall back to basic rendering
                    if (Shaders.WrathsCleaverShaderLoader.HasSlash)
                    {
                        var shader = Shaders.WrathsCleaverShaderLoader.WrathSlashShader.Value;
                        shader.Parameters["uTime"]?.SetValue((float)Main.GameUpdateCount * 0.04f);
                        shader.Parameters["uColor"]?.SetValue(WrathsCleaverUtils.BloodRed.ToVector3());
                        shader.Parameters["uSecondaryColor"]?.SetValue(WrathsCleaverUtils.HellfireGold.ToVector3());
                        shader.Parameters["uOpacity"]?.SetValue(1f);
                        shader.Parameters["uIntensity"]?.SetValue(1.5f);
                        shader.Parameters["uScrollSpeed"]?.SetValue(1.5f);
                        shader.Parameters["uDistortionAmt"]?.SetValue(0.12f);
                        shader.Parameters["uOverbrightMult"]?.SetValue(3.2f);
                        shader.CurrentTechnique = shader.Techniques["WrathSlashTechnique"];
                        shader.CurrentTechnique.Passes[0].Apply();
                    }
                }
            );

            WrathTrailRenderer.RenderTrail(trailPoints, settings);

            // Restart SpriteBatch
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawBlade(SpriteBatch sb, Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Owner.Center - Main.screenPosition;

            float progress = MathHelper.Clamp(SwingTimer / SwingDuration, 0f, 1f);
            float bladeAngle = Projectile.rotation + (SwingDirection < 0 ? MathHelper.Pi : 0f);

            // Scale pulse at impact moments
            float impactPulse = 1f;
            if (progress > 0.3f && progress < 0.5f)
                impactPulse = 1f + (float)Math.Sin((progress - 0.3f) / 0.2f * MathHelper.Pi) * 0.08f;

            float scale = 1.1f * impactPulse;
            SpriteEffects flip = SwingDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            sb.Draw(tex, drawPos, null, lightColor, bladeAngle, origin, scale, flip, 0f);
        }

        private void DrawBladeGlow(SpriteBatch sb)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Owner.Center - Main.screenPosition;

            float bladeAngle = Projectile.rotation + (SwingDirection < 0 ? MathHelper.Pi : 0f);
            SpriteEffects flip = SwingDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.08f;

            // Additive glow layers
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Blood red outer glow
            sb.Draw(tex, drawPos, null, Additive(WrathsCleaverUtils.BloodRed, 0.35f),
                bladeAngle, origin, 1.15f * pulse, flip, 0f);
            // Infernal mid glow
            sb.Draw(tex, drawPos, null, Additive(WrathsCleaverUtils.InfernalRed, 0.25f),
                bladeAngle, origin, 1.08f * pulse, flip, 0f);
            // Gold hot edge
            sb.Draw(tex, drawPos, null, Additive(WrathsCleaverUtils.HellfireGold, 0.15f),
                bladeAngle, origin, 1.03f * pulse, flip, 0f);

            // Blade tip glint (lens flare at tip)
            float tipDist = 100f;
            Vector2 tipPos = drawPos + Projectile.rotation.ToRotationVector2() * tipDist;
            float flareAngle = Main.GameUpdateCount * 0.08f;
            Texture2D flareTex = MagnumTextureRegistry.GetPointBloom();
            Texture2D flareGlowTex = MagnumTextureRegistry.GetSoftGlow();
            if (flareTex != null)
                sb.Draw(flareTex, tipPos, new Rectangle(0, 0, 1, 1),
                    Additive(WrathsCleaverUtils.HellfireGold, 0.5f * pulse), flareAngle,
                    new Vector2(0.5f), new Vector2(20f, 3f) * pulse, SpriteEffects.None, 0f);
            if (flareGlowTex != null)
                sb.Draw(flareGlowTex, tipPos, new Rectangle(0, 0, 1, 1),
                    Additive(WrathsCleaverUtils.HellfireGold, 0.5f * pulse), flareAngle + MathHelper.PiOver2,
                    new Vector2(0.5f), new Vector2(20f, 3f) * pulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawLungeTrail(SpriteBatch sb)
        {
            if (trailPoints.Count < 3) return;

            sb.End();

            var settings = new WrathTrailSettings(
                widthFunc: t => 20f * (float)Math.Sin(t * MathHelper.Pi),
                colorFunc: t =>
                {
                    Color c = MulticolorLerp(t, WrathsCleaverUtils.HellfireGold, WrathsCleaverUtils.InfernalRed, WrathsCleaverUtils.BloodRed);
                    return Additive(c, (1f - t) * 0.9f);
                },
                smoothingSteps: 6,
                shaderSetup: () =>
                {
                    if (Shaders.WrathsCleaverShaderLoader.HasInferno)
                    {
                        var shader = Shaders.WrathsCleaverShaderLoader.InfernoTrailShader.Value;
                        shader.Parameters["uTime"]?.SetValue((float)Main.GameUpdateCount * 0.05f);
                        shader.Parameters["uColor"]?.SetValue(WrathsCleaverUtils.InfernalRed.ToVector3());
                        shader.Parameters["uSecondaryColor"]?.SetValue(WrathsCleaverUtils.EmberOrange.ToVector3());
                        shader.Parameters["uOpacity"]?.SetValue(1f);
                        shader.Parameters["uIntensity"]?.SetValue(1.2f);
                        shader.Parameters["uScrollSpeed"]?.SetValue(2f);
                        shader.Parameters["uOverbrightMult"]?.SetValue(3f);
                        shader.CurrentTechnique = shader.Techniques["InfernoTrailTechnique"];
                        shader.CurrentTechnique.Passes[0].Apply();
                    }
                }
            );

            WrathTrailRenderer.RenderTrail(trailPoints, settings);

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Arc collision — check if enemy is within the swing arc
            float bladeLength = 120f;
            Vector2 start = Owner.Center;
            Vector2 end = Owner.Center + Projectile.rotation.ToRotationVector2() * bladeLength;
            float unused = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 30f, ref unused);
        }

        public override void OnKill(int timeLeft)
        {
            trailPoints.Clear();
        }
    }
}
