using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.Debuffs;
using MagnumOpus.Common.Systems.VFX.Sparkle;
using ReLogic.Content;

namespace MagnumOpus.Content.Fate.ResonantWeapons.SymphonysEnd
{
    /// <summary>
    /// Spiraling spectral blade — the primary projectile of Symphony's End.
    /// 
    /// Behaviour:
    ///   • Corkscrews toward the cursor position stored in ai[0]/ai[1]
    ///   • Accelerates over its lifetime
    ///   • Deals bonus damage at high velocity
    ///   • Shatters into 4 <see cref="SymphonyBladeFragment"/>s on kill/expiry
    ///   • Shader-driven trail + self-contained particle VFX
    /// </summary>
    public class SymphonySpiralBlade : ModProjectile
    {
        // Use the item sprite — custom rendering handles visuals
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/SymphonysEnd";

        // ─── Constants ────────────────────────────────────────────
        private const int MaxLifetime  = 180;  // 3 seconds
        private const float BaseSpeed  = 10f;
        private const float MaxBoost   = 14f;
        private const float HelixSpeed = 0.25f;
        private const float BaseHelixRadius = 35f;

        // ─── State ────────────────────────────────────────────────
        private float helixAngle;

        private Vector2 TargetPos => new Vector2(Projectile.ai[0], Projectile.ai[1]);
        private float Age => 1f - (float)Projectile.timeLeft / MaxLifetime;

        // ─── Setup ────────────────────────────────────────────────

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width  = 24;
            Projectile.height = 24;
            Projectile.friendly    = true;
            Projectile.DamageType  = DamageClass.Magic;
            Projectile.penetrate   = 3;
            Projectile.timeLeft    = MaxLifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown  = 10;
            Projectile.extraUpdates = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            helixAngle = Main.rand.NextFloat(MathHelper.TwoPi);

            // Track active blade count
            if (Main.player[Projectile.owner] is Player owner)
                owner.Symphony().ActiveBladeCount++;
        }

        // ─── AI: Helix Steering ───────────────────────────────────

        public override void AI()
        {
            float age   = Age;
            float speed = BaseSpeed + age * MaxBoost;

            // Helix phase advance
            helixAngle += HelixSpeed;
            float helixRadius = BaseHelixRadius * (1f - age * 0.6f);

            // Steering toward cursor
            Vector2 toTarget = TargetPos - Projectile.Center;
            float dist = toTarget.Length();
            Vector2 dir  = toTarget.SafeNormalize(Vector2.UnitX);
            Vector2 perp = new Vector2(-dir.Y, dir.X);

            // Helix component — perpendicular oscillation
            Vector2 helixVel = perp * MathF.Cos(helixAngle) * helixRadius * 0.15f;

            // Desired velocity
            Vector2 desired = dir * speed + helixVel;

            // Smooth steering (more responsive as blade ages)
            float steerFactor = 0.08f + age * 0.12f;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, steerFactor);

            // Close-range burst-through
            if (dist < 32f && age > 0.3f)
                Projectile.velocity *= 1.05f;

            // Rotation follows velocity
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // ─── VFX ──────────────────────────────────────────────
            if (!Main.dedServ)
            {
                SymphonyParticleFactory.SpawnSpiralTrailParticles(Projectile.Center, Projectile.velocity);

                Color lightCol = SymphonyUtils.GetSymphonyGradient(age);
                Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * 0.4f);
            }
        }

        // ─── Damage Scaling ───────────────────────────────────────

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float speedFactor = Projectile.velocity.Length() / BaseSpeed;
            if (speedFactor > 1.5f)
            {
                float bonus = MathHelper.Clamp((speedFactor - 1.5f) * 0.3f, 0f, 0.5f);
                modifiers.SourceDamage += bonus;
            }

            // Diminuendo damage boost during cooldown
            if (Main.player[Projectile.owner] is Player owner)
            {
                var sym = owner.Symphony();
                if (sym.IsDiminuendo)
                    modifiers.FinalDamage *= sym.DiminuendoDamageMultiplier;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);

            if (Main.dedServ) return;

            float age = Age;
            Vector2 hitPos = target.Center;

            // ═══ ENHANCED PARTICLE BURST ═══
            // 14 radial sparks with chromatic gradient
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 9f);
                Color sparkCol = SymphonyUtils.GetSymphonyGradient((float)i / 14f);
                SymphonyParticleHandler.Spawn(SymphonyParticleFactory.Spark(
                    hitPos, sparkVel, sparkCol * 0.8f, 0.12f, 14));
            }

            // 6 directional slash marks along velocity
            Vector2 hitDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 hitPerp = new Vector2(-hitDir.Y, hitDir.X);
            for (int i = 0; i < 6; i++)
            {
                float spread = (i - 2.5f) / 2.5f;
                Vector2 slashVel = (hitDir * 4f + hitPerp * spread * 6f) * Main.rand.NextFloat(0.8f, 1.2f);
                Color slashCol = Color.Lerp(SymphonyUtils.SymphonyViolet, SymphonyUtils.FinalWhite, MathF.Abs(spread));
                SymphonyParticleHandler.Spawn(SymphonyParticleFactory.Spark(
                    hitPos, slashVel, slashCol * 0.7f, 0.1f, 12));
            }

            // Shatter burst (original)
            SymphonyParticleFactory.SpawnShatterBurst(hitPos, 12);

            // 5 cascading music notes — the impact chord
            for (int i = 0; i < 5; i++)
            {
                float noteAngle = MathHelper.TwoPi * i / 5f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color noteCol = i % 2 == 0 ? SymphonyUtils.SymphonyViolet : SymphonyUtils.SymphonyPink;
                SymphonyParticleHandler.Spawn(SymphonyParticleFactory.Note(
                    hitPos + Main.rand.NextVector2Circular(8f, 8f), noteVel, noteCol * 0.7f, 0.18f, 22));
            }

            // Harmony blue wisps at high age
            if (age > 0.4f)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 wispVel = Main.rand.NextVector2Circular(2f, 2f);
                    SymphonyParticleHandler.Spawn(SymphonyParticleFactory.Glow(
                        hitPos + Main.rand.NextVector2Circular(12f, 12f), wispVel,
                        SymphonyUtils.HarmonyBlue * 0.5f, 0.15f, 20));
                }
            }

            // Dual lighting for bloom visibility
            Lighting.AddLight(hitPos, SymphonyUtils.SymphonyViolet.ToVector3() * 0.9f);
            Lighting.AddLight(hitPos + hitDir * 16f, SymphonyUtils.SymphonyPink.ToVector3() * 0.6f);

            SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.3f, Volume = 0.6f }, hitPos);
        }

        // ─── Death → Shatter ──────────────────────────────────────

        /// <summary>Whether this blade is a Final Note (set via Projectile.localAI[1] = 1).</summary>
        private bool IsFinalNote => Projectile.localAI[1] >= 1f;

        public override void OnKill(int timeLeft)
        {
            if (IsFinalNote)
                FinalNoteDetonation();
            else
                SpawnFragments();

            // Decrement blade counter
            if (Main.player[Projectile.owner] is Player owner)
            {
                var sym = owner.Symphony();
                sym.ActiveBladeCount = System.Math.Max(0, sym.ActiveBladeCount - 1);
            }

            if (!Main.dedServ)
            {
                SymphonyParticleFactory.SpawnShatterBurst(Projectile.Center);
                SymphonyParticleHandler.Spawn(SymphonyParticleFactory.Ring(
                    Projectile.Center, SymphonyUtils.FinalWhite, IsFinalNote ? 0.6f : 0.2f, IsFinalNote ? 30 : 22));
                Lighting.AddLight(Projectile.Center, SymphonyUtils.SymphonyPink.ToVector3() * (IsFinalNote ? 2f : 0.8f));
            }
        }

        /// <summary>Final Note cosmic detonation — massive visual burst instead of fragments.</summary>
        private void FinalNoteDetonation()
        {
            if (Main.dedServ) return;

            // Massive multi-layer particle burst
            SymphonyParticleHandler.SpawnBurst(Projectile.Center, 20, 12f, 0.5f,
                SymphonyUtils.SymphonyViolet, SymphonyParticleType.Spark, 22);
            SymphonyParticleHandler.SpawnBurst(Projectile.Center, 16, 8f, 0.45f,
                SymphonyUtils.SymphonyPink, SymphonyParticleType.Spark, 20);
            SymphonyParticleHandler.SpawnBurst(Projectile.Center, 10, 5f, 0.55f,
                SymphonyUtils.FinalWhite, SymphonyParticleType.Spark, 18);

            // Music note burst — the final chord
            SymphonyParticleHandler.SpawnBurst(Projectile.Center, 8, 6f, 0.3f,
                SymphonyUtils.SymphonyViolet, SymphonyParticleType.Note, 24);

            // Additional harmony blue note cascade
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                SymphonyParticleHandler.Spawn(SymphonyParticleFactory.Note(
                    Projectile.Center, noteVel, SymphonyUtils.HarmonyBlue * 0.7f, 0.25f, 26));
            }

            // Shatter burst for dramatic fragment scattering
            SymphonyParticleFactory.SpawnShatterBurst(Projectile.Center, 18);

            // Multiple expanding rings
            for (int r = 0; r < 3; r++)
            {
                Color ringCol = r switch
                {
                    0 => SymphonyUtils.SymphonyViolet,
                    1 => SymphonyUtils.SymphonyPink,
                    _ => SymphonyUtils.FinalWhite
                };
                SymphonyParticleHandler.Spawn(SymphonyParticleFactory.Ring(
                    Projectile.Center, ringCol, 0.4f + r * 0.15f, 26 + r * 4));
            }

            // Discord red dissonance accents — the breaking strings
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 discVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                SymphonyParticleHandler.Spawn(SymphonyParticleFactory.Spark(
                    Projectile.Center, discVel, SymphonyUtils.DiscordRed * 0.6f, 0.15f, 16));
            }

            Lighting.AddLight(Projectile.Center, SymphonyUtils.FinalWhite.ToVector3() * 2.5f);
            Lighting.AddLight(Projectile.Center, SymphonyUtils.SymphonyPink.ToVector3() * 1.5f);

            SoundEngine.PlaySound(Terraria.ID.SoundID.Item14 with { Pitch = -0.3f, Volume = 1.2f }, Projectile.Center);
        }

        private void SpawnFragments()
        {
            if (Main.myPlayer != Projectile.owner) return;

            // Fragment count scales with Crescendo Mode (4 base, 6 in Crescendo)
            int fragCount = 4;
            if (Main.player[Projectile.owner] is Player owner)
            {
                var sym = owner.Symphony();
                fragCount = sym.FragmentCount;
            }

            for (int i = 0; i < fragCount; i++)
            {
                float angle = MathHelper.TwoPi * i / fragCount + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 fragVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    fragVel,
                    ModContent.ProjectileType<SymphonyBladeFragment>(),
                    Projectile.damage / 3,
                    Projectile.knockBack * 0.5f,
                    Projectile.owner);
            }
        }

        // ─── Custom Drawing ───────────────────────────────────────

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            try
            {
                // 1. End default SpriteBatch for custom rendering
                sb.End();

                // 2. Draw trail (shader-driven GPU primitives)
                DrawTrail();

                // 3. Graduated bloom aura around the blade body (behind sprite)
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                    null, Main.GameViewMatrix.TransformationMatrix);
                DrawProjectileBloom(sb);
                sb.End();

                // 4. Chromatic afterimage echoes + blade sprite (additive)
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                    null, Main.GameViewMatrix.TransformationMatrix);
                DrawAfterimages(sb);
                DrawBlade(sb);
                sb.End();

                // 5. Leading-edge CrescentBloom at the velocity tip
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                    null, Main.GameViewMatrix.TransformationMatrix);
                DrawLeadingBloom(sb);
                sb.End();

                // 6. Restart normal SpriteBatch
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
                    null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch
            {
                // Safety: ensure SpriteBatch is restored to Terraria's expected state
                try
                {
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                        Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
                        null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }

            // Theme accents (additive pass)
            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                SymphonyUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch { }

            return false;
        }

        /// <summary>Celestial sparkle bloom aura rendered behind the blade sprite.</summary>
        private void DrawProjectileBloom(SpriteBatch sb)
        {
            float age = Age;
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.08f) * 0.12f;

            // Celestial sparkle bloom — replaces 7-draw SoftRadialBloom/PointBloom/StarFlare stack
            Color[] bladeColors = new Color[] {
                SymphonyUtils.VoidBlack,
                SymphonyUtils.SymphonyViolet,
                SymphonyUtils.SymphonyPink,
                SymphonyUtils.HarmonyBlue,
                SymphonyUtils.FinalWhite,
            };
            float intensity = 0.4f + age * 0.15f;
            SparkleBloomHelper.DrawSparkleBloom(sb, Projectile.Center, SparkleTheme.Fate,
                bladeColors, MathHelper.Min(intensity, 0.8f), 22f * pulse, 7, time,
                seed: Projectile.identity * 0.55f, sparkleScale: 0.022f);
        }

        /// <summary>Celestial sparkle at the projectile's leading edge (velocity tip).</summary>
        private void DrawLeadingBloom(SpriteBatch sb)
        {
            float age = Age;
            float time = (float)Main.timeForVisualEffects;

            // Leading edge position — offset in velocity direction
            Vector2 leadDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 tipWorld = Projectile.Center + leadDir * 16f;
            float leadPulse = 1f + MathF.Sin(time * 0.12f) * 0.15f;

            // Celestial sparkle at leading edge — replaces 4-draw bloom stack
            Color[] leadColors = new Color[] {
                SymphonyUtils.SymphonyViolet,
                SymphonyUtils.SymphonyPink,
                SymphonyUtils.HarmonyBlue,
                SymphonyUtils.FinalWhite,
            };
            float intensity = 0.3f + age * 0.12f;
            SparkleBloomHelper.DrawSparkleBloom(sb, tipWorld, SparkleTheme.Fate,
                leadColors, MathHelper.Min(intensity, 0.65f), 14f * leadPulse, 4, time,
                seed: Projectile.identity * 0.88f, sparkleScale: 0.018f);
        }

        /// <summary>Chromatic-separated afterimage echoes along the spiral path.</summary>
        private void DrawAfterimages(SpriteBatch sb)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() * 0.5f;
            float age = Age;
            float time = (float)Main.timeForVisualEffects;

            // Draw RGB-separated echoes at old positions
            for (int i = 2; i < Projectile.oldPos.Length; i += 2)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                float t = (float)i / Projectile.oldPos.Length;
                float echoAlpha = (1f - t) * 0.25f;
                float echoScale = 0.45f * (1f - t * 0.3f);

                Vector2 basePos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float rot = Projectile.oldRot[i];

                // Chromatic separation: offset pink and violet slightly in opposite directions
                Vector2 chromOffset = new Vector2(MathF.Cos(rot), MathF.Sin(rot)) * (2f + t * 3f);

                // Violet echo (offset backward)
                sb.Draw(tex, basePos - chromOffset, null,
                    SymphonyUtils.Additive(SymphonyUtils.SymphonyViolet, echoAlpha * 0.6f),
                    rot, origin, echoScale * 1.1f, SpriteEffects.None, 0f);

                // Pink echo (offset forward)
                sb.Draw(tex, basePos + chromOffset, null,
                    SymphonyUtils.Additive(SymphonyUtils.SymphonyPink, echoAlpha * 0.5f),
                    rot, origin, echoScale, SpriteEffects.None, 0f);

                // Faint white core echo
                sb.Draw(tex, basePos, null,
                    SymphonyUtils.Additive(SymphonyUtils.FinalWhite, echoAlpha * 0.2f),
                    rot, origin, echoScale * 0.6f, SpriteEffects.None, 0f);
            }
        }

        private void DrawTrail()
        {
            Effect shader = SymphonyShaderLoader.SymphonySpiralTrail;
            if (shader != null)
            {
                try { shader.CurrentTechnique = shader.Techniques["SpiralMain"]; }
                catch { /* technique missing — fallback handles it */ }
            }

            SymphonyTrailRenderer.DrawTrail(
                Projectile.oldPos,
                Projectile.Size * 0.5f,
                SymphonyTrailSettings.SpiralBlade,
                shader);
        }

        private void DrawBlade(SpriteBatch sb)
        {
            Texture2D tex  = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin  = tex.Size() * 0.5f;

            float pulse = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.08f) * 0.1f;
            float scale = 0.5f * pulse;
            float age   = Age;
            float time  = (float)Main.timeForVisualEffects;

            // Harmonic overtone ring — small rotating satellites around the blade
            int harmonics = 3;
            float ringRadius = 12f + MathF.Sin(time * 0.06f) * 3f;
            for (int h = 0; h < harmonics; h++)
            {
                float hAngle = time * 0.1f * (1f + h * 0.5f) + h * MathHelper.TwoPi / harmonics;
                Vector2 hPos = drawPos + hAngle.ToRotationVector2() * ringRadius;
                float hAlpha = 0.2f + age * 0.15f;
                Color hCol = h switch
                {
                    0 => SymphonyUtils.SymphonyViolet,
                    1 => SymphonyUtils.SymphonyPink,
                    _ => SymphonyUtils.FinalWhite,
                };
                sb.Draw(tex, hPos, null,
                    SymphonyUtils.Additive(hCol, hAlpha * 0.4f),
                    Projectile.rotation + h * MathHelper.PiOver2, origin, scale * 0.35f, SpriteEffects.None, 0f);
            }

            // Outer violet glow
            sb.Draw(tex, drawPos, null,
                SymphonyUtils.Additive(SymphonyUtils.SymphonyViolet, 0.35f),
                Projectile.rotation, origin, scale * 1.3f, SpriteEffects.None, 0f);

            // Mid pink glow
            sb.Draw(tex, drawPos, null,
                SymphonyUtils.Additive(SymphonyUtils.SymphonyPink, 0.4f),
                Projectile.rotation, origin, scale * 1.1f, SpriteEffects.None, 0f);

            // Hot white core (brightens with age)
            sb.Draw(tex, drawPos, null,
                SymphonyUtils.Additive(SymphonyUtils.FinalWhite, 0.5f + age * 0.3f),
                Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
