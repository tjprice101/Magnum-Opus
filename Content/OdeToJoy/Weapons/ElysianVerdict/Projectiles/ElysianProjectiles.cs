using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Projectiles
{
    // ═══════════════════════════════════════════════════════════
    // ElysianOrbProjectile — cursor-tracking golden-green orb
    // Phase 1: Travel to cursor, decelerate, lerp at 0.08
    // Phase 2: Hover at cursor, fire 2 VineMissile every 20 frames
    // Phase 3: Detonate after 180 frames or on player re-use
    // ═══════════════════════════════════════════════════════════
    public class ElysianOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;
        private static Asset<Texture2D> _softBloomTex;

        // AI state
        private bool reachedTarget = false;
        private int hoverTimer = 0;
        private int fireTimer = 0;
        private bool hasDetonated = false;

        // Ambient glow particles that orbit the orb
        private List<ElysianGlowParticle> ambientGlows = new List<ElysianGlowParticle>();

        /// <summary>
        /// ai[0] = general-purpose frame counter for VFX
        /// ai[1] = 1 if ordered to detonate externally
        /// </summary>

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 360;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.ai[0]++;

            // ── EXTERNAL DETONATE SIGNAL ──
            if (Projectile.ai[1] >= 1f && !hasDetonated)
            {
                Detonate();
                return;
            }

            Vector2 targetPos = Main.MouseWorld;

            // ── PHASE 1: TRAVEL TO CURSOR ──
            if (!reachedTarget)
            {
                float distToTarget = Vector2.Distance(Projectile.Center, targetPos);

                // Decelerate as we approach
                if (distToTarget < 60f)
                {
                    reachedTarget = true;
                    Projectile.velocity = Vector2.Zero;
                }
                else
                {
                    // Lerp toward cursor
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity,
                        (targetPos - Projectile.Center).SafeNormalize(Vector2.Zero) * 8f, 0.08f);
                }
            }
            // ── PHASE 2: HOVER + FIRE VINE MISSILES ──
            else
            {
                // Smoothly track cursor while hovering
                Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.08f);
                Projectile.velocity = Vector2.Zero;

                hoverTimer++;
                fireTimer++;

                // Fire 2 VineMissile every 20 frames at nearest enemy
                if (fireTimer >= 20)
                {
                    fireTimer = 0;
                    NPC target = ElysianUtils.ClosestNPC(Projectile.Center, 800f);
                    if (target != null)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                            dir = dir.RotatedByRandom(0.2f); // slight spread
                            float speed = 10f + Main.rand.NextFloat(2f);

                            int dmg = (int)(Projectile.damage / 3f);
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                                dir * speed, ModContent.ProjectileType<VineMissileProjectile>(),
                                dmg, Projectile.knockBack * 0.5f, Projectile.owner);
                        }

                        SoundEngine.PlaySound(SoundID.Item17 with { Pitch = 0.5f, Volume = 0.5f }, Projectile.Center);
                    }
                }

                // ── PHASE 3: AUTO-DETONATE after 180 frames of hovering ──
                if (hoverTimer >= 180 && !hasDetonated)
                {
                    Detonate();
                    return;
                }
            }

            // ── AMBIENT VFX ──
            if (!Main.dedServ)
            {
                // Spawn ambient glow particles
                if ((int)Projectile.ai[0] % 12 == 0)
                {
                    var glow = new ElysianGlowParticle(
                        Projectile.Center,
                        Main.rand.NextFloat(25f, 50f),
                        Main.rand.NextFloat(0.25f, 0.45f),
                        Main.rand.Next(40, 70));
                    ambientGlows.Add(glow);
                    ElysianParticleHandler.SpawnParticle(glow);
                }

                // Update anchors for orbiting glows
                for (int i = ambientGlows.Count - 1; i >= 0; i--)
                {
                    if (!ambientGlows[i].Active)
                        ambientGlows.RemoveAt(i);
                    else
                        ambientGlows[i].UpdateAnchor(Projectile.Center);
                }

                // Floating leaf particles
                if ((int)Projectile.ai[0] % 18 == 0)
                {
                    var leaf = new FloatingLeafParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(30f, 30f),
                        new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-1.5f, -0.5f)),
                        Main.rand.NextFloat(0.3f, 0.5f),
                        Main.rand.Next(30, 55));
                    ElysianParticleHandler.SpawnParticle(leaf);
                }
            }

            // Lighting
            float lightIntensity = 0.5f + (float)Math.Sin(Projectile.ai[0] * 0.1f) * 0.15f;
            Lighting.AddLight(Projectile.Center, 0.5f * lightIntensity, 0.45f * lightIntensity, 0.1f * lightIntensity);
        }

        private void Detonate()
        {
            if (hasDetonated)
                return;
            hasDetonated = true;

            SoundEngine.PlaySound(SoundID.Item105 with { Pitch = 0.3f, Volume = 0.9f }, Projectile.Center);

            // Expand hitbox for AoE damage
            Projectile.Resize(200, 200);
            Projectile.damage = (int)(Projectile.damage * 1.5f);
            Projectile.timeLeft = 5; // linger a few frames for AoE hits
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

            // 6 radial VineMissile burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi / 6f * i;
                Vector2 dir = angle.ToRotationVector2();
                int dmg = (int)(Projectile.damage / 3f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                    dir * 9f, ModContent.ProjectileType<VineMissileProjectile>(),
                    dmg, Projectile.knockBack * 0.5f, Projectile.owner);
            }

            // ── DETONATION VFX ──
            if (!Main.dedServ)
            {
                // Massive VerdictBloom
                var bloom = new VerdictBloomParticle(Projectile.Center, 3.5f, 40);
                ElysianParticleHandler.SpawnParticle(bloom);

                // Secondary bloom layer
                var bloom2 = new VerdictBloomParticle(Projectile.Center, 2.0f, 30);
                ElysianParticleHandler.SpawnParticle(bloom2);

                // Expanding ring of JudgmentNote music notes
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi / 12f * i + Main.rand.NextFloat(-0.15f, 0.15f);
                    Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4.5f);
                    var note = new JudgmentNoteParticle(
                        Projectile.Center + noteVel * 5f,
                        noteVel,
                        Main.rand.NextFloat(0.35f, 0.6f),
                        Main.rand.Next(40, 65));
                    ElysianParticleHandler.SpawnParticle(note);
                }

                // Leaf burst
                for (int i = 0; i < 15; i++)
                {
                    Vector2 leafVel = Main.rand.NextVector2CircularEdge(3f, 3f) + new Vector2(0f, -1f);
                    var leaf = new FloatingLeafParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                        leafVel,
                        Main.rand.NextFloat(0.3f, 0.6f),
                        Main.rand.Next(35, 60));
                    ElysianParticleHandler.SpawnParticle(leaf);
                }

                // Vine sparks burst
                for (int i = 0; i < 10; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                    var spark = new VineTrailParticle(
                        Projectile.Center,
                        sparkVel,
                        Main.rand.NextFloat(0.4f, 0.7f),
                        Main.rand.Next(15, 28));
                    ElysianParticleHandler.SpawnParticle(spark);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            // If killed without detonating (e.g., by weapon re-use), still detonate
            if (!hasDetonated)
            {
                Detonate();
            }
        }

        public override bool? CanHitNPC(NPC target)
        {
            // Only deal contact damage during detonation
            if (hasDetonated)
                return null;
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (hasDetonated)
                return false; // VFX handled by particles during detonation

            _bloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _softBloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);

            Texture2D bloom = _bloomTex.Value;
            Texture2D softBloom = _softBloomTex.Value;
            Vector2 bOrigin = bloom.Size() / 2f;
            Vector2 sOrigin = softBloom.Size() / 2f;
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float time = (float)Main.GameUpdateCount / 60f;
            float frameTime = Projectile.ai[0];
            float pulse = 1f + (float)Math.Sin(frameTime * 0.15f) * 0.18f;
            float rotSlow = frameTime * 0.02f;

            sb.End();

            // ═══ Layer 1: FloralSigil — rotating flower-of-life botanical pattern ═══
            Effect auraShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyCelebrationAuraShader);

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (auraShader != null)
            {
                auraShader.Parameters["uTime"]?.SetValue(time);
                auraShader.Parameters["uColor"]?.SetValue(ElysianUtils.ElysianGold.ToVector3());
                auraShader.Parameters["uSecondaryColor"]?.SetValue(ElysianUtils.VineGreen.ToVector3());
                auraShader.Parameters["uOpacity"]?.SetValue(0.55f);
                auraShader.Parameters["uIntensity"]?.SetValue(1.4f);
                auraShader.Parameters["uRadius"]?.SetValue(0.42f);
                auraShader.Parameters["uRotation"]?.SetValue(rotSlow);
                auraShader.CurrentTechnique = auraShader.Techniques["FloralSigilTechnique"];
                auraShader.CurrentTechnique.Passes[0].Apply();

                sb.Draw(softBloom, drawPos, null, Color.White, 0f, sOrigin,
                    1.6f * pulse, SpriteEffects.None, 0f);
            }

            sb.End();

            // ═══ Layer 2: GardenBloom — pulsing petal bloom overlay ═══
            Effect bloomShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyGardenBloomShader);

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (bloomShader != null)
            {
                bloomShader.Parameters["uTime"]?.SetValue(time);
                bloomShader.Parameters["uColor"]?.SetValue(ElysianUtils.GoldenVerdict.ToVector3());
                bloomShader.Parameters["uSecondaryColor"]?.SetValue(ElysianUtils.RoseJudgment.ToVector3());
                bloomShader.Parameters["uOpacity"]?.SetValue(0.5f);
                bloomShader.Parameters["uIntensity"]?.SetValue(1.2f);
                bloomShader.Parameters["uRadius"]?.SetValue(0.38f);
                bloomShader.Parameters["uPulseSpeed"]?.SetValue(2.5f);
                bloomShader.CurrentTechnique = bloomShader.Techniques["JubilantPulseTechnique"];
                bloomShader.CurrentTechnique.Passes[0].Apply();

                sb.Draw(softBloom, drawPos, null, Color.White, 0f, sOrigin,
                    1.0f * pulse, SpriteEffects.None, 0f);
            }

            sb.End();

            // ═══ Layer 3: Additive glow layers (fallback-safe) ═══
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer golden glow
            Color outerGlow = ElysianUtils.Additive(ElysianUtils.ElysianGold, 0.4f);
            sb.Draw(softBloom, drawPos, null, outerGlow, rotSlow, sOrigin, 1.1f * pulse, SpriteEffects.None, 0f);

            // Mid green shimmer
            float greenPulse = 0.6f + (float)Math.Sin(frameTime * 0.2f + 1.5f) * 0.25f;
            Color midGlow = ElysianUtils.Additive(ElysianUtils.VineGreen, 0.3f * greenPulse);
            sb.Draw(bloom, drawPos, null, midGlow, -rotSlow * 1.3f, bOrigin, 0.75f * pulse, SpriteEffects.None, 0f);

            // Inner bright core
            Color core = ElysianUtils.Additive(ElysianUtils.PureRadiance, 0.5f);
            sb.Draw(bloom, drawPos, null, core, rotSlow * 2f, bOrigin, 0.35f * pulse, SpriteEffects.None, 0f);

            // Rose accent shimmer
            float roseShimmer = (float)Math.Sin(frameTime * 0.25f) * 0.5f + 0.5f;
            Color roseGlow = ElysianUtils.Additive(ElysianUtils.RoseJudgment, 0.2f * roseShimmer);
            sb.Draw(bloom, drawPos, null, roseGlow, rotSlow * 0.7f, bOrigin, 0.6f * pulse, SpriteEffects.None, 0f);

            sb.End();
            ElysianUtils.BeginDefault(sb);

            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VineMissileProjectile — small homing green glow bolt
    // 12x12, pen 1, timeLeft 120, homing 0.07, 1/3 damage
    // Applies Poisoned 120. Leaves vine trail particles.
    // ═══════════════════════════════════════════════════════════
    public class VineMissileProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;
        private static Asset<Texture2D> _softBloomTex;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Projectile.ai[0]++;

            // ── HOMING ──
            NPC target = ElysianUtils.ClosestNPC(Projectile.Center, 600f);
            if (target != null)
            {
                Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.07f);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // ── VINE TRAIL PARTICLES ──
            if (!Main.dedServ && (int)Projectile.ai[0] % 2 == 0)
            {
                var trail = new VineTrailParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    Main.rand.NextFloat(0.2f, 0.4f),
                    Main.rand.Next(10, 20));
                ElysianParticleHandler.SpawnParticle(trail);
            }

            // Lighting
            Lighting.AddLight(Projectile.Center, 0.15f, 0.3f, 0.05f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 120);

            // Impact VFX
            if (!Main.dedServ)
            {
                for (int i = 0; i < 4; i++)
                {
                    var spark = new VineTrailParticle(
                        target.Center + Main.rand.NextVector2Circular(8f, 8f),
                        Main.rand.NextVector2Circular(3f, 3f),
                        Main.rand.NextFloat(0.3f, 0.5f),
                        Main.rand.Next(10, 18));
                    ElysianParticleHandler.SpawnParticle(spark);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Death burst
            if (!Main.dedServ)
            {
                for (int i = 0; i < 3; i++)
                {
                    var spark = new VineTrailParticle(
                        Projectile.Center,
                        Main.rand.NextVector2Circular(2f, 2f),
                        Main.rand.NextFloat(0.2f, 0.35f),
                        Main.rand.Next(8, 15));
                    ElysianParticleHandler.SpawnParticle(spark);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            _bloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _softBloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);

            Texture2D bloom = _bloomTex.Value;
            Texture2D softBloom = _softBloomTex.Value;
            Vector2 bOrigin = bloom.Size() / 2f;
            Vector2 sOrigin = softBloom.Size() / 2f;
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = (float)Main.GameUpdateCount / 60f;

            float pulse = 1f + (float)Math.Sin(Projectile.ai[0] * 0.25f) * 0.12f;
            float speed = Projectile.velocity.Length();
            float stretchFactor = 1f + speed * 0.015f;

            sb.End();

            // ═══ PollenDrift shader — drifting seed trail behind the missile ═══
            Effect pollenShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyPollenDriftShader);

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (pollenShader != null)
            {
                pollenShader.Parameters["uTime"]?.SetValue(time);
                pollenShader.Parameters["uColor"]?.SetValue(ElysianUtils.VineGreen.ToVector3());
                pollenShader.Parameters["uSecondaryColor"]?.SetValue(ElysianUtils.ElysianGold.ToVector3());
                pollenShader.Parameters["uOpacity"]?.SetValue(0.55f);
                pollenShader.Parameters["uIntensity"]?.SetValue(1.2f);
                pollenShader.CurrentTechnique = pollenShader.Techniques["PollenTrailTechnique"];
                pollenShader.CurrentTechnique.Passes[0].Apply();

                sb.Draw(bloom, drawPos, null, Color.White, Projectile.rotation, bOrigin,
                    new Vector2(0.6f * stretchFactor * pulse, 0.35f * pulse), SpriteEffects.None, 0f);
            }

            // Outer green glow
            Color outerGlow = ElysianUtils.Additive(ElysianUtils.VineGreen, 0.5f);
            sb.Draw(bloom, drawPos, null, outerGlow, Projectile.rotation, bOrigin,
                new Vector2(0.45f * pulse, 0.28f * pulse), SpriteEffects.None, 0f);

            // Soft golden halo
            Color haloColor = ElysianUtils.Additive(ElysianUtils.ElysianGold, 0.2f);
            sb.Draw(softBloom, drawPos, null, haloColor, 0f, sOrigin,
                0.3f * pulse, SpriteEffects.None, 0f);

            // Inner bright core
            Color core = ElysianUtils.Additive(ElysianUtils.PureRadiance, 0.45f);
            sb.Draw(bloom, drawPos, null, core, Projectile.rotation, bOrigin,
                new Vector2(0.22f * pulse, 0.13f * pulse), SpriteEffects.None, 0f);

            sb.End();
            ElysianUtils.BeginDefault(sb);

            return false;
        }
    }
}
