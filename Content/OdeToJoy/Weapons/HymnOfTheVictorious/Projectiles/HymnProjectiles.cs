using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Projectiles
{
    // ═══════════════════════════════════════════════════════════
    // OrbitalNoteProjectile — glowing golden music note that orbits
    // the player then launches toward cursor with enemy homing.
    // Phase 1 (ai[0] < 30): orbit player at 80px radius.
    // Phase 2 (ai[0] >= 30): launch toward cursor, home 0.06.
    // On hit: note particle burst + Poisoned 120.
    // ═══════════════════════════════════════════════════════════
    public class OrbitalNoteProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;
        private static Asset<Texture2D> _softBloomTex;
        private static Asset<Texture2D> _noteTex;

        /// <summary>ai[0] = timer, ai[1] = orbit index (0-7), ai[2] = launch direction angle</summary>
        private bool launched = false;
        private Vector2 launchDir;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            float timer = Projectile.ai[0];
            float orbitIndex = Projectile.ai[1];

            // ── Phase 1: Orbit player ──
            if (timer < 30f)
            {
                float baseAngle = MathHelper.TwoPi / 8f * orbitIndex;
                float currentAngle = baseAngle + timer * 0.12f; // rotate around player
                float radius = 80f;

                // Spiral inward slightly as it gathers energy then expand
                float radMod = 1f + (float)Math.Sin(timer * 0.2f) * 0.15f;
                Vector2 targetPos = owner.Center + currentAngle.ToRotationVector2() * radius * radMod;

                // Smooth movement to orbit position
                Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.3f);
                Projectile.velocity = Vector2.Zero;
                Projectile.rotation = currentAngle + MathHelper.PiOver2;

                // Spawn orbital glow particles
                if (!Main.dedServ && timer % 3 == 0)
                {
                    var glow = new OrbitalNoteGlowParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                        Main.rand.NextVector2Circular(0.5f, 0.5f),
                        Main.rand.NextFloat(0.15f, 0.3f),
                        Main.rand.Next(8, 16));
                    HymnParticleHandler.SpawnParticle(glow);
                }

                Lighting.AddLight(Projectile.Center, 0.4f, 0.35f, 0.08f);
            }
            // ── Phase 2: Launch toward cursor and home ──
            else
            {
                if (!launched)
                {
                    launched = true;
                    // ai[2] stores the launch angle set by the weapon's Shoot()
                    launchDir = Projectile.ai[2].ToRotationVector2();
                    Projectile.velocity = launchDir * 14f;
                    Projectile.tileCollide = true;
                    Projectile.timeLeft = 240;
                    Projectile.netUpdate = true;
                }

                // Home toward closest enemy
                NPC target = HymnUtils.ClosestNPC(Projectile.Center, 800f);
                if (target != null)
                {
                    Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.06f);
                }

                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                // Spawn melody trail particles
                if (!Main.dedServ && (int)timer % 2 == 0)
                {
                    var trail = new MelodyTrailParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                        -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                        Main.rand.NextFloat(0.2f, 0.4f),
                        Main.rand.Next(10, 18));
                    HymnParticleHandler.SpawnParticle(trail);
                }

                Lighting.AddLight(Projectile.Center, 0.5f, 0.4f, 0.1f);
            }

            Projectile.ai[0]++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 120);

            // Impact VFX: note particle burst
            if (!Main.dedServ)
            {
                for (int i = 0; i < 6; i++)
                {
                    var glow = new OrbitalNoteGlowParticle(
                        target.Center + Main.rand.NextVector2Circular(10f, 10f),
                        Main.rand.NextVector2Circular(3f, 3f),
                        Main.rand.NextFloat(0.2f, 0.45f),
                        Main.rand.Next(12, 24));
                    HymnParticleHandler.SpawnParticle(glow);
                }

                // Small melody burst
                for (int i = 0; i < 3; i++)
                {
                    var trail = new MelodyTrailParticle(
                        target.Center,
                        Main.rand.NextVector2Circular(4f, 4f),
                        Main.rand.NextFloat(0.15f, 0.3f),
                        Main.rand.Next(8, 14));
                    HymnParticleHandler.SpawnParticle(trail);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (!Main.dedServ)
            {
                for (int i = 0; i < 4; i++)
                {
                    var glow = new OrbitalNoteGlowParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                        Main.rand.NextVector2Circular(2f, 2f),
                        Main.rand.NextFloat(0.15f, 0.3f),
                        Main.rand.Next(10, 18));
                    HymnParticleHandler.SpawnParticle(glow);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            _bloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _softBloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _noteTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/Particles Asset Library/MusicNote", AssetRequestMode.ImmediateLoad);

            Texture2D bloom = _bloomTex.Value;
            Texture2D softBloom = _softBloomTex.Value;
            Texture2D note = _noteTex.Value;
            Vector2 bOrigin = bloom.Size() / 2f;
            Vector2 sOrigin = softBloom.Size() / 2f;
            Vector2 noteOrigin = note.Size() / 2f;
            SpriteBatch sb = Main.spriteBatch;
            float time = (float)Main.GameUpdateCount / 60f;

            sb.End();

            float pulse = 1f + (float)Math.Sin(Projectile.ai[0] * 0.25f) * 0.2f;
            float gatherIntensity = Projectile.ai[0] < 30f ? Projectile.ai[0] / 30f : 1f;

            // ═══ Layer 1: SymphonicAura shader — harmonic rings orbiting the note ═══
            Effect harmonyShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyJubilantHarmonyShader);

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (harmonyShader != null)
            {
                harmonyShader.Parameters["uTime"]?.SetValue(time);
                harmonyShader.Parameters["uColor"]?.SetValue(HymnUtils.BrilliantGold.ToVector3());
                harmonyShader.Parameters["uSecondaryColor"]?.SetValue(HymnUtils.HealGreen.ToVector3());
                harmonyShader.Parameters["uOpacity"]?.SetValue(0.5f * gatherIntensity);
                harmonyShader.Parameters["uIntensity"]?.SetValue(1.2f);
                harmonyShader.Parameters["uRadius"]?.SetValue(0.4f);
                harmonyShader.Parameters["uHarmonicFreq"]?.SetValue(1.5f);
                harmonyShader.CurrentTechnique = harmonyShader.Techniques["SymphonicAuraTechnique"];
                harmonyShader.CurrentTechnique.Passes[0].Apply();

                sb.Draw(softBloom, Projectile.Center - Main.screenPosition, null, Color.White,
                    0f, sOrigin, 0.8f * pulse, SpriteEffects.None, 0f);
            }

            // Outer golden glow
            Color outerColor = HymnUtils.Additive(HymnUtils.WarmAmber, 0.4f * gatherIntensity);
            sb.Draw(bloom, Projectile.Center - Main.screenPosition, null, outerColor, Projectile.rotation, bOrigin,
                0.5f * pulse, SpriteEffects.None, 0f);

            // Core brilliant gold
            Color coreColor = HymnUtils.Additive(HymnUtils.BrilliantGold, 0.65f * gatherIntensity);
            sb.Draw(bloom, Projectile.Center - Main.screenPosition, null, coreColor, Projectile.rotation, bOrigin,
                0.28f * pulse, SpriteEffects.None, 0f);

            // Inner white hot center
            Color whiteCore = HymnUtils.Additive(HymnUtils.DivineLight, 0.5f * gatherIntensity);
            sb.Draw(bloom, Projectile.Center - Main.screenPosition, null, whiteCore, Projectile.rotation, bOrigin,
                0.14f * pulse, SpriteEffects.None, 0f);

            // Music note overlay
            Color noteCol = HymnUtils.Additive(HymnUtils.BrilliantGold, 0.55f * gatherIntensity);
            sb.Draw(note, Projectile.Center - Main.screenPosition, null, noteCol, Projectile.rotation * 0.3f, noteOrigin,
                0.2f * pulse, SpriteEffects.None, 0f);

            sb.End();
            HymnUtils.BeginDefault(sb);

            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // SymphonicExplosionProjectile — massive 250x250 AoE explosion
    // that heals the player and poisons enemies.
    // Deals 1.5x weapon damage. Heals player 30 HP on first hit.
    // Applies Poisoned 240 + Venom 120.
    // ═══════════════════════════════════════════════════════════
    public class SymphonicExplosionProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;
        private static Asset<Texture2D> _softBloomTex;
        private static Asset<Texture2D> _noteTex;

        /// <summary>ai[0] = timer, ai[1] = has healed flag (0 or 1)</summary>

        public override void SetDefaults()
        {
            Projectile.width = 250;
            Projectile.height = 250;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            // Spawn massive explosion VFX on first frame
            if (Projectile.ai[0] == 0f && !Main.dedServ)
            {
                // Massive golden bloom
                var bloom = new HymnBloomParticle(
                    Projectile.Center,
                    3.5f,
                    25);
                HymnParticleHandler.SpawnParticle(bloom);

                // Multiple expanding symphonic wave rings
                for (int i = 0; i < 3; i++)
                {
                    var wave = new SymphonicWaveParticle(
                        Projectile.Center,
                        2.0f + i * 0.8f,
                        20 + i * 5);
                    HymnParticleHandler.SpawnParticle(wave);
                }

                // Cascading music notes
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi / 12f * i + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                    var glow = new OrbitalNoteGlowParticle(
                        Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(10f, 30f),
                        noteVel,
                        Main.rand.NextFloat(0.3f, 0.6f),
                        Main.rand.Next(20, 40));
                    HymnParticleHandler.SpawnParticle(glow);
                }

                // Scatter melody trails outward
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi / 8f * i;
                    var trail = new MelodyTrailParticle(
                        Projectile.Center,
                        angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f),
                        Main.rand.NextFloat(0.3f, 0.55f),
                        Main.rand.Next(15, 25));
                    HymnParticleHandler.SpawnParticle(trail);
                }
            }

            // Continue spawning healing motes throughout duration
            if (!Main.dedServ && Projectile.ai[0] % 3 == 0)
            {
                var mote = new HealingMoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(60f, 60f),
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2f, -0.5f)),
                    Main.rand.NextFloat(0.15f, 0.3f),
                    Main.rand.Next(25, 45));
                HymnParticleHandler.SpawnParticle(mote);
            }

            Projectile.ai[0]++;

            // Pulsing golden light
            float intensity = 1f - (Projectile.ai[0] / 30f);
            Lighting.AddLight(Projectile.Center, 1.2f * intensity, 1.0f * intensity, 0.3f * intensity);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 1.5x weapon damage
            modifiers.SourceDamage *= 1.5f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 240);
            target.AddBuff(BuffID.Venom, 120);

            // Heal player 30 HP on first hit (use ai[1] as flag)
            if (Projectile.ai[1] == 0f)
            {
                Projectile.ai[1] = 1f;
                Player owner = Main.player[Projectile.owner];
                if (owner.active && !owner.dead)
                {
                    owner.Heal(30);

                    // Heal VFX: green sparkle motes rising from player
                    if (!Main.dedServ)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            var mote = new HealingMoteParticle(
                                owner.Center + Main.rand.NextVector2Circular(20f, 20f),
                                new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-3f, -1f)),
                                Main.rand.NextFloat(0.15f, 0.35f),
                                Main.rand.Next(30, 50));
                            HymnParticleHandler.SpawnParticle(mote);
                        }
                    }
                }
            }

            // Hit VFX per enemy
            if (!Main.dedServ)
            {
                for (int i = 0; i < 4; i++)
                {
                    var glow = new OrbitalNoteGlowParticle(
                        target.Center + Main.rand.NextVector2Circular(15f, 15f),
                        Main.rand.NextVector2Circular(3f, 3f),
                        Main.rand.NextFloat(0.2f, 0.4f),
                        Main.rand.Next(10, 20));
                    HymnParticleHandler.SpawnParticle(glow);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            _bloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _softBloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _noteTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/Particles Asset Library/MusicNote", AssetRequestMode.ImmediateLoad);

            Texture2D bloom = _bloomTex.Value;
            Texture2D softBloom = _softBloomTex.Value;
            Texture2D note = _noteTex.Value;
            Vector2 bOrigin = bloom.Size() / 2f;
            Vector2 sOrigin = softBloom.Size() / 2f;
            Vector2 noteOrigin = note.Size() / 2f;
            SpriteBatch sb = Main.spriteBatch;

            float progress = Projectile.ai[0] / 30f;
            float fade = 1f - progress;
            float fadeAlpha = fade * fade;
            float time = (float)Main.GameUpdateCount / 60f;

            sb.End();

            // ═══ Layer 1: CelebrationAura — expanding golden ring waves ═══
            Effect auraShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyCelebrationAuraShader);

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (auraShader != null)
            {
                auraShader.Parameters["uTime"]?.SetValue(time);
                auraShader.Parameters["uColor"]?.SetValue(HymnUtils.BrilliantGold.ToVector3());
                auraShader.Parameters["uSecondaryColor"]?.SetValue(HymnUtils.HealGreen.ToVector3());
                auraShader.Parameters["uOpacity"]?.SetValue(fadeAlpha * 0.65f);
                auraShader.Parameters["uIntensity"]?.SetValue(1.6f);
                auraShader.Parameters["uRadius"]?.SetValue(0.45f);
                auraShader.Parameters["uRingCount"]?.SetValue(6f);
                auraShader.CurrentTechnique = auraShader.Techniques["CelebrationAuraTechnique"];
                auraShader.CurrentTechnique.Passes[0].Apply();

                float auraScale = MathHelper.Lerp(1.5f, 5f, (float)Math.Sqrt(progress));
                sb.Draw(softBloom, Projectile.Center - Main.screenPosition, null, Color.White,
                    0f, sOrigin, auraScale, SpriteEffects.None, 0f);
            }

            sb.End();

            // ═══ Layer 2: Additive bloom body + notes + green heal shimmer ═══
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Massive expanding golden bloom
            float bloomScale = MathHelper.Lerp(0.5f, 4f, (float)Math.Sqrt(progress));
            Color bloomCol = HymnUtils.Additive(HymnUtils.BrilliantGold, fadeAlpha * 0.6f);
            sb.Draw(bloom, Projectile.Center - Main.screenPosition, null, bloomCol,
                progress * 0.5f, bOrigin, bloomScale, SpriteEffects.None, 0f);

            // Divine white core
            Color coreCol = HymnUtils.Additive(HymnUtils.DivineLight, fadeAlpha * 0.8f);
            sb.Draw(bloom, Projectile.Center - Main.screenPosition, null, coreCol,
                -progress * 0.3f, bOrigin, bloomScale * 0.35f, SpriteEffects.None, 0f);

            // Warm amber ring cross
            float ringScale = MathHelper.Lerp(0.3f, 5f, progress);
            Color ringCol = HymnUtils.Additive(HymnUtils.WarmAmber, fadeAlpha * 0.35f);
            sb.Draw(bloom, Projectile.Center - Main.screenPosition, null, ringCol,
                progress * 0.8f, bOrigin, new Vector2(ringScale, ringScale * 0.2f), SpriteEffects.None, 0f);
            sb.Draw(bloom, Projectile.Center - Main.screenPosition, null, ringCol,
                progress * 0.8f + MathHelper.PiOver2, bOrigin, new Vector2(ringScale, ringScale * 0.2f), SpriteEffects.None, 0f);

            // Rose harmony accent flash (early frames)
            if (progress < 0.4f)
            {
                Color roseCol = HymnUtils.Additive(HymnUtils.RoseHarmony, fadeAlpha * 0.3f);
                sb.Draw(softBloom, Projectile.Center - Main.screenPosition, null, roseCol,
                    progress * 2f, sOrigin, bloomScale * 0.55f, SpriteEffects.None, 0f);
            }

            // Cascading music notes spinning outward
            int noteCount = 6;
            for (int i = 0; i < noteCount; i++)
            {
                float angle = MathHelper.TwoPi / noteCount * i + progress * MathHelper.TwoPi;
                float dist = 30f + progress * 120f;
                Vector2 notePos = Projectile.Center + angle.ToRotationVector2() * dist;
                Color noteCol = HymnUtils.Additive(HymnUtils.BrilliantGold, fadeAlpha * 0.45f);
                sb.Draw(note, notePos - Main.screenPosition, null, noteCol,
                    angle + MathHelper.PiOver4, noteOrigin, 0.25f * fade, SpriteEffects.None, 0f);
            }

            // Heal green shimmer
            Color greenShimmer = HymnUtils.Additive(HymnUtils.HealGreen, fadeAlpha * 0.22f);
            sb.Draw(softBloom, Projectile.Center - Main.screenPosition, null, greenShimmer,
                progress * -1.2f, sOrigin, bloomScale * 0.65f, SpriteEffects.None, 0f);

            sb.End();
            HymnUtils.BeginDefault(sb);

            return false;
        }
    }
}
