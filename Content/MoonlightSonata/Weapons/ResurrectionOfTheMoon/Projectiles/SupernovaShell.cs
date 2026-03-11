using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Utilities;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Primitives;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Buffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Dusts;
using MagnumOpus.Content.LaCampanella.Debuffs;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.FoundationWeapons.RibbonFoundation;
using ReLogic.Content;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Projectiles
{
    /// <summary>
    /// Supernova Chamber  EArcing artillery shell that detonates in massive AoE.
    /// Gravity-affected arc trajectory.
    /// On first tile or enemy contact: massive radial explosion using SupernovaBlast shader.
    /// Spawns expanding shockwave ring + crater ring particles + screen shake.
    /// Spawns 6 secondary lunar fragment projectiles on explosion.
    ///
    /// FOUNDATION VFX INTEGRATION:
    /// - FLIGHT: Energy Surge ribbon overlay (RibbonFoundation Mode 6) atop CometTrail shaders
    /// - DETONATION: SupernovaSparks + SupernovaSmokeRing + SupernovaRipple spawned in Explode()
    /// </summary>
    public class SupernovaShell : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1";

        // =================================================================
        // CONSTANTS
        // =================================================================

        public const float Gravity = 0.12f;
        public const float MaxFallSpeed = 14f;
        public const float ShellWidth = 18f;
        public const int TrailLength = 20;
        public const float ExplosionRadius = 235f;
        public const float ExplosionDamageMultiplier = 1.5f;
        public const int FragmentCount = 6;
        public const float FragmentDamageMultiplier = 0.3f;

        // =================================================================
        // AI FIELDS
        // =================================================================

        /// <summary>localAI[0] = alive time.</summary>
        public ref float AliveTime => ref Projectile.localAI[0];

        /// <summary>localAI[1] = has exploded flag.</summary>
        public ref float HasExploded => ref Projectile.localAI[1];

        // =================================================================
        // SETUP
        // =================================================================

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1; // explodes manually
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 0;
            Projectile.light = 0.5f;
        }

        // =================================================================
        // AI  EGravity Arc
        // =================================================================

        public override void AI()
        {
            AliveTime++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Gravity  Earcing trajectory
            if (Projectile.velocity.Y < MaxFallSpeed)
                Projectile.velocity.Y += Gravity;

            // Lighting
            Color lightCol = CometUtils.SupernovaColor;
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * 0.5f);

            SpawnFlightParticles();
        }

        private void SpawnFlightParticles()
        {
            if (Main.dedServ) return;

            // Ember trail  Eevery tick
            Vector2 offset = Main.rand.NextVector2Circular(4f, 4f);
            Vector2 emberVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f)
                + Main.rand.NextVector2Circular(1f, 1f);
            CometParticleHandler.Spawn(new EmberTrailParticle(
                Projectile.Center + offset, emberVel,
                0.35f + Main.rand.NextFloat(0.2f), 15 + Main.rand.Next(8)));

            // Dust  Eevery 2 ticks
            if (AliveTime % 2 == 0)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<CometDust>(), -Projectile.velocity.X * 0.1f, -Projectile.velocity.Y * 0.1f);
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 0.9f;
            }

            // Violet mist around shell
            if (AliveTime % 5 == 0)
            {
                CometParticleHandler.Spawn(new CometMistParticle(
                    Projectile.Center, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    CometUtils.SupernovaColor * 0.4f, 0.4f, 20));
            }
        }

        // =================================================================
        // TILE COLLISION  EExplode
        // =================================================================

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explode();
            return true; // kill projectile
        }

        // =================================================================
        // ENEMY HIT  EExplode
        // =================================================================

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (HasExploded == 0f)
                Explode();
        }

        // =================================================================
        // EXPLOSION
        // =================================================================

        private void Explode()
        {
            if (HasExploded != 0f) return;
            HasExploded = 1f;

            // Screen shake
            if (Projectile.owner == Main.myPlayer)
            {
                var shakePlayer = Main.LocalPlayer.GetModPlayer<ScreenShakePlayer>();
                shakePlayer.AddShake(10f, 30);
            }

            // AoE damage
            if (Projectile.owner == Main.myPlayer)
                ApplyExplosionDamage();

            // Spawn lunar fragments
            if (Projectile.owner == Main.myPlayer)
                SpawnFragments();

            // Sound
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.8f, Pitch = -0.4f }, Projectile.Center);

            // VFX
            SpawnExplosionVFX();

            // Add charges to player
            if (Projectile.owner == Main.myPlayer)
                Main.LocalPlayer.Resurrection().AddCharge(3);
        }

        private void ApplyExplosionDamage()
        {
            int explosionDamage = (int)(Projectile.damage * ExplosionDamageMultiplier);

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy(Projectile)) continue;
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist > ExplosionRadius) continue;

                // Distance-based damage falloff
                float falloff = 1f - (dist / ExplosionRadius) * 0.4f;
                int scaledDamage = (int)(explosionDamage * falloff);

                Player owner = Main.player[Projectile.owner];
                NPC.HitInfo hitInfo = npc.CalculateHitInfo(scaledDamage, Projectile.velocity.X > 0 ? 1 : -1,
                    false, Projectile.knockBack * 1.5f, Projectile.DamageType);
                npc.StrikeNPC(hitInfo, false, false);

                // Heavy Lunar Impact debuff
                npc.AddBuff(ModContent.BuffType<LunarImpact>(), 600); // 10 seconds
                var impactNpc = npc.GetGlobalNPC<LunarImpactNPC>();
                impactNpc.AddStack();
                impactNpc.AddStack();
                impactNpc.AddStack(); // Triple stack for Supernova
            }
        }

        private void SpawnFragments()
        {
            int fragmentDamage = (int)(Projectile.damage * FragmentDamageMultiplier);

            for (int i = 0; i < FragmentCount; i++)
            {
                float angle = MathHelper.TwoPi * i / FragmentCount + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * (8f + Main.rand.NextFloat(4f));

                // Fragments use the Standard ricochet projectile with pre-set bounces
                int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                    vel, ModContent.ProjectileType<ResurrectionProjectile>(),
                    fragmentDamage, Projectile.knockBack * 0.5f, Projectile.owner);

                if (proj >= 0 && proj < Main.maxProjectiles)
                {
                    Main.projectile[proj].ai[0] = 5; // Start at bounce 5 (already heated up)
                    Main.projectile[proj].timeLeft = 300;
                    Main.projectile[proj].penetrate = 6;
                }
            }
        }

        private void SpawnExplosionVFX()
        {
            if (Main.dedServ) return;

            // Get lunar phase for VFX scaling
            float lunarMult = 1f;
            Color lunarTint = CometUtils.SupernovaColor;
            if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers && Main.player[Projectile.owner].active)
            {
                var cp = Main.player[Projectile.owner].Resurrection();
                lunarMult = CometPlayer.LunarPhaseAoEMultiplier[cp.LunarCyclePhase];
                lunarTint = Color.Lerp(CometUtils.SupernovaColor, cp.CurrentLunarColor, 0.4f);
            }

            // === FOUNDATION VFX: Shader-driven base layers ===
            // SupernovaSparks  Eradial burst of 55+ sparks (ExplosionParticlesFoundation)
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                    Vector2.Zero, ModContent.ProjectileType<SupernovaSparks>(),
                    0, 0f, Projectile.owner, ai0: lunarMult);
            }

            // SupernovaSmokeRing  E30 animated smoke puffs (SmokeFoundation)
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                    Vector2.Zero, ModContent.ProjectileType<SupernovaSmokeRing>(),
                    0, 0f, Projectile.owner, ai0: lunarMult);
            }

            // SupernovaRipple  E5-ring concentric shockwave (ImpactFoundation + RippleShader)
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                    Vector2.Zero, ModContent.ProjectileType<SupernovaRipple>(),
                    0, 0f, Projectile.owner, ai0: lunarMult);
            }

            // === LUNAR CYCLE INDICATOR RINGS === (SoftCircle is 2160px)
            CometParticleHandler.Spawn(new LunarCycleRingParticle(
                Projectile.Center, lunarTint, 0.12f * lunarMult, 30));
            CometParticleHandler.Spawn(new LunarCycleRingParticle(
                Projectile.Center, lunarTint * 0.6f, 0.18f * lunarMult, 40));

            // Multi-layer crater bloom cascade (5 layers, staggered, scaled by lunar phase)
            // SoftRadialBloom is 2160px  Escales must stay very small
            for (int layer = 0; layer < 5; layer++)
            {
                float layerPhase = layer / 4f;
                Color layerColor = Color.Lerp(CometUtils.GetCometGradient(layerPhase), lunarTint, 0.25f);
                float layerScale = (0.033f + layer * 0.012f) * lunarMult;
                int layerLife = 25 + layer * 5;
                CometParticleHandler.Spawn(new CraterBloomParticle(
                    Projectile.Center, layerColor, layerScale, layerLife));
            }

            // Core flash ( Ebrilliant white on Full Moon)
            // SoftRadialBloom 2160px: 0.07 ↁE~150px, 0.055 ↁE~120px
            float coreScale = lunarMult > 1.2f ? 0.058f : 0.046f;
            CometParticleHandler.Spawn(new CraterBloomParticle(
                Projectile.Center, CometUtils.FrigidImpact, coreScale, 15));

            // Shockwave rings (3 expanding rings, scaled by lunar phase)
            // SoftCircle is 2160px  Ekeep scales small
            for (int ring = 0; ring < 3; ring++)
            {
                float ringScale = (0.12f + ring * 0.05f) * lunarMult;
                Color ringCol = Color.Lerp(CometUtils.CometCoreWhite, lunarTint, ring / 2f);
                CometParticleHandler.Spawn(new ShockwaveRingParticle(
                    Projectile.Center, ringCol, ringScale, 30 + ring * 8));
            }

            // Massive radial ember burst (count scaled by lunar phase)
            int emberCount = (int)(40 * lunarMult);
            for (int i = 0; i < emberCount; i++)
            {
                float angle = MathHelper.TwoPi * i / emberCount + Main.rand.NextFloat(-0.1f, 0.1f);
                float speed = 4f + Main.rand.NextFloat(8f);
                Vector2 vel = angle.ToRotationVector2() * speed * lunarMult;
                CometParticleHandler.Spawn(new EmberTrailParticle(
                    Projectile.Center, vel,
                    0.5f + Main.rand.NextFloat(0.4f),
                    25 + Main.rand.Next(20)));
            }

            // Lunar shards flung outward
            int shardCount = (int)(15 * lunarMult);
            for (int i = 0; i < shardCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(6f, 6f) * lunarMult
                    + Main.rand.NextVector2Circular(2f, 2f);
                CometParticleHandler.Spawn(new LunarShardParticle(
                    Projectile.Center, vel,
                    CometUtils.GetCometGradient(Main.rand.NextFloat()),
                    0.5f + Main.rand.NextFloat(0.4f), 30 + Main.rand.Next(15)));
            }

            // === NEW: Supernova debris  Etumbling lunar rock fragments ===
            int debrisCount = 8 + (int)(6 * lunarMult);
            for (int i = 0; i < debrisCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(5f, 5f) + new Vector2(0, -Main.rand.NextFloat(2f, 5f));
                Color debrisColor = Color.Lerp(CometUtils.ImpactCrater, lunarTint, Main.rand.NextFloat(0.4f));
                CometParticleHandler.Spawn(new SupernovaDebrisParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), vel,
                    debrisColor, 0.4f + Main.rand.NextFloat(0.3f), 35 + Main.rand.Next(20)));
            }

            // === THEME-SPECIFIC: MS Star Flare corona burst === (MSStarFlare 1024px, draw ÁE.3)
            CometParticleHandler.Spawn(new SupernovaStarFlareParticle(
                Projectile.Center, 0.75f * lunarMult, lunarTint, 25));
            CometParticleHandler.Spawn(new SupernovaStarFlareParticle(
                Projectile.Center, 0.46f * lunarMult, CometUtils.CometCoreWhite, 20));

            // === THEME-SPECIFIC: MS Lens Flare cinematic flash === (MSLensFlare 1024px, draw ÁE.4)
            CometParticleHandler.Spawn(new LunarLensFlareParticle(
                Projectile.Center, 0.5f * lunarMult, lunarTint, 30));

            // === THEME-SPECIFIC: MS Harmonic Resonance Wave expanding impact === (1024px, draw ÁE.5)
            CometParticleHandler.Spawn(new HarmonicResonanceWaveParticle(
                Projectile.Center, lunarTint, 0.5f * lunarMult, 35));
            CometParticleHandler.Spawn(new HarmonicResonanceWaveParticle(
                Projectile.Center, CometUtils.CometCoreWhite * 0.5f, 0.32f * lunarMult, 25));

            // === THEME-SPECIFIC: MS Power Effect Ring concentrated burst === (1024px, draw ÁE.4)
            CometParticleHandler.Spawn(new LunarPowerRingParticle(
                Projectile.Center, lunarTint, 0.42f * lunarMult, 30));

            // === THEME-SPECIFIC: Crescent moon fragments scattering outward ===
            int crescentCount = (int)(6 * lunarMult);
            for (int i = 0; i < crescentCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f) * lunarMult;
                Color crescCol = Color.Lerp(CometUtils.DeepSpaceViolet, lunarTint, Main.rand.NextFloat(0.6f));
                CometParticleHandler.Spawn(new ResurrectionCrescentParticle(
                    Projectile.Center, vel, 0.6f + Main.rand.NextFloat(0.3f), crescCol, 40 + Main.rand.Next(15)));
            }

            // === THEME-SPECIFIC: Tidal mist wisps drifting from detonation ===
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color mistCol = Color.Lerp(CometUtils.DeepSpaceViolet, lunarTint, Main.rand.NextFloat(0.3f));
                CometParticleHandler.Spawn(new TidalMistWispParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(30f, 30f), vel,
                    0.8f + Main.rand.NextFloat(0.5f), mistCol, 50 + Main.rand.Next(20)));
            }

            // === THEME-SPECIFIC: Moonlight music notes ascending from explosion ===
            int noteCount = 3 + (int)(3 * lunarMult);
            for (int i = 0; i < noteCount; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), -Main.rand.NextFloat(1f, 3f));
                Color noteCol = Color.Lerp(CometUtils.GetCometGradient(Main.rand.NextFloat()), CometUtils.CometCoreWhite, 0.3f);
                CometParticleHandler.Spawn(new MoonlightMusicNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), vel,
                    0.5f + Main.rand.NextFloat(0.3f), noteCol, 45 + Main.rand.Next(15)));
            }

            // === THEME-SPECIFIC: MS Glow Orb ethereal bloom overlay === (MSGlowOrb 1024px, draw ÁE.35)
            CometParticleHandler.Spawn(new MoonlightGlowOrbParticle(
                Projectile.Center, 0.46f * lunarMult, lunarTint, 25));

            // Comet mist cloud
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                CometParticleHandler.Spawn(new CometMistParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(30f, 30f),
                    vel, CometUtils.GetCometGradient(Main.rand.NextFloat()) * 0.4f,
                    1.5f + Main.rand.NextFloat(1f), 40 + Main.rand.Next(20)));
            }

            // Music notes cascading upward from destruction  Ethe supernova sings
            int noteCount2 = 6 + (int)(4 * lunarMult);
            MoonlightVFXLibrary.SpawnMusicNotes(Projectile.Center, count: noteCount2,
                spread: 35f * lunarMult, minScale: 0.5f, maxScale: 1.1f,
                lifetime: 50 + (int)(lunarMult * 15));

            // Heavy dust explosion
            for (int i = 0; i < 30; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(8f, 8f);
                int d = Dust.NewDust(Projectile.Center - new Vector2(8), 16, 16,
                    ModContent.DustType<CometDust>(), dustVel.X, dustVel.Y);
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 1.5f + Main.rand.NextFloat(0.8f);
            }
        }

        // =================================================================
        // RENDERING
        // =================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            SpriteBatch sb = Main.spriteBatch;
            try
            {

            // Pass 1: Glow trail (CometTrailGlow shader)
            DrawGlowTrail();

            // Pass 2: Main shell trail (CometTrailMain shader)
            DrawMainTrail();

            // Pass 3: Energy Surge ribbon overlay (RibbonFoundation Mode 6)
            DrawEnergySurgeRibbon();

            // Pass 4: Head glow bloom stacking
            DrawHeadGlow();

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

        // =================================================================
        // ENERGY SURGE RIBBON (Foundation: RibbonFoundation Mode 6)
        // =================================================================

        private static Asset<Texture2D> _energySurgeTex;

        /// <summary>
        /// Draws an Energy Surge texture strip ribbon over the CometTrail.
        /// Adapted from RibbonFoundation's DrawTextureStripRibbon pattern.
        /// Uses the EnergySurgeBeam texture UV-mapped along Projectile.oldPos.
        /// Lunar palette: purple outer ↁEice blue body ↁEwhite-hot core near head.
        /// </summary>
        private void DrawEnergySurgeRibbon()
        {
            _energySurgeTex ??= ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/BeamTextures/EnergySurgeBeam");
            if (!_energySurgeTex.IsLoaded) return;

            Texture2D stripTex = _energySurgeTex.Value;
            int texW = stripTex.Width;
            int texH = stripTex.Height;
            float time = (float)Main.timeForVisualEffects * 0.005f;

            // Collect valid trail positions
            Vector2[] positions = Projectile.oldPos;
            int validCount = 0;
            for (int i = 0; i < positions.Length; i++)
            {
                if (positions[i] != Vector2.Zero) validCount++;
                else break;
            }
            if (validCount < 3) return;

            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Lunar colors: purple outer ↁEice blue mid ↁEwhite-hot core
            Color outerColor = CometUtils.DeepSpaceViolet;
            Color midColor = new Color(120, 190, 255); // LunarShine
            Color coreColor = CometUtils.CometCoreWhite;

            const float RibbonWidthHead = 24f;
            const float RibbonWidthTail = 3f;
            int srcWidth = Math.Max(1, texW / validCount);

            for (int i = 0; i < validCount - 1; i++)
            {
                float progress = (float)i / validCount;
                float fade = progress * progress;
                if (fade < 0.01f) continue;

                float width = MathHelper.Lerp(RibbonWidthTail, RibbonWidthHead, progress);

                Vector2 segStart = positions[i] + Projectile.Size * 0.5f;
                Vector2 segEnd = positions[i + 1] + Projectile.Size * 0.5f;
                Vector2 segDir = segEnd - segStart;
                float segLength = segDir.Length();
                if (segLength < 0.5f) continue;
                float segAngle = segDir.ToRotation();

                float uStart = (progress + time * 2f) % 1f;
                int srcX = (int)(uStart * texW) % texW;
                Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                float scaleX = segLength / (float)srcWidth;
                float scaleY = width / (float)texH;

                Vector2 pos = segStart - Main.screenPosition;
                Vector2 origin = new Vector2(0, texH / 2f);

                // Body  Elunar tinted
                Color bodyCol = Color.Lerp(outerColor, midColor, progress) * (fade * 0.45f);
                sb.Draw(stripTex, pos, srcRect, bodyCol, segAngle, origin,
                    new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);

                // Hot core near the head
                if (progress > 0.5f)
                {
                    float coreFade = (progress - 0.5f) / 0.5f;
                    Color cc = Color.Lerp(midColor, coreColor, coreFade * 0.5f) * (fade * coreFade * 0.3f);
                    sb.Draw(stripTex, pos, srcRect, cc, segAngle, origin,
                        new Vector2(scaleX * 0.5f, scaleY * 0.5f), SpriteEffects.None, 0f);
                }
            }

            // Restore for next pass
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawGlowTrail()
        {
            MiscShaderData glowShader = null;
            if (GameShaders.Misc.TryGetValue("MagnumOpus:CometTrailGlow", out var shader))
            {
                glowShader = shader;
                glowShader.UseColor(CometUtils.SupernovaColor);
                glowShader.UseSecondaryColor(CometUtils.DeepSpaceViolet);
                glowShader.UseOpacity(0.5f);
                glowShader.UseSaturation(0.5f); // mid-phase (not yet detonated)
            }

            CometTrailRenderer.RenderTrail(Projectile.oldPos, new CometTrailSettings(
                widthFunction: (completion, _) =>
                {
                    float taper = 1f - completion;
                    return ShellWidth * 2f * taper * CometUtils.SineOut(1f - completion);
                },
                colorFunction: (completion, _) =>
                {
                    Color col = Color.Lerp(CometUtils.SupernovaColor, CometUtils.DeepSpaceViolet, completion);
                    return col * 0.35f * (1f - completion * 0.4f);
                },
                offsetFunction: (_, _) => Projectile.Size * 0.5f,
                smoothen: true,
                shader: glowShader
            ), TrailLength);
        }

        private void DrawMainTrail()
        {
            MiscShaderData mainShader = null;
            if (GameShaders.Misc.TryGetValue("MagnumOpus:CometTrailMain", out var shader))
            {
                mainShader = shader;
                mainShader.UseColor(CometUtils.CometCoreWhite);
                mainShader.UseSecondaryColor(CometUtils.SupernovaColor);
                mainShader.UseOpacity(0.7f);
                mainShader.UseSaturation(0.5f);
            }

            CometTrailRenderer.RenderTrail(Projectile.oldPos, new CometTrailSettings(
                widthFunction: (completion, _) =>
                {
                    float taper = 1f - completion;
                    return ShellWidth * taper * CometUtils.SineOut(1f - completion);
                },
                colorFunction: (completion, _) =>
                {
                    Color col = Color.Lerp(CometUtils.CometCoreWhite, CometUtils.SupernovaColor, completion);
                    return col * 0.7f * (1f - completion * 0.3f);
                },
                offsetFunction: (_, _) => Projectile.Size * 0.5f,
                smoothen: true,
                shader: mainShader
            ), TrailLength);
        }

        private void DrawHeadGlow()
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D bloom = CometTextures.SoftRadialBloom;
            Vector2 origin = bloom.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Lunar-phase-tinted glow
            Color lunarTint = CometUtils.SupernovaColor;
            if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers && Main.player[Projectile.owner].active)
                lunarTint = Color.Lerp(CometUtils.SupernovaColor, Main.player[Projectile.owner].Resurrection().CurrentLunarColor, 0.3f);

            // Switch to Additive for bloom rendering
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Pulsing glow with lunar influence  Etexture is 2160px, keep scales tiny
            float pulse = 0.8f + MathF.Sin(AliveTime * 0.15f) * 0.2f;
            sb.Draw(bloom, drawPos, null, lunarTint with { A = 0 } * 0.25f * pulse, 0f, origin, 0.055f, SpriteEffects.None, 0f);

            // Mid glow layer
            sb.Draw(bloom, drawPos, null, CometUtils.SupernovaColor with { A = 0 } * 0.12f, 0f, origin, 0.035f, SpriteEffects.None, 0f);

            // Core glow
            sb.Draw(bloom, drawPos, null, CometUtils.CometCoreWhite with { A = 0 } * 0.25f, 0f, origin, 0.025f, SpriteEffects.None, 0f);

            // Restore to AlphaBlend
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // =================================================================
        // DEATH VFX (in case of timeout without hitting anything)
        // =================================================================

        public override void OnKill(int timeLeft)
        {
            // If we haven't exploded yet (killed by timeout), do a smaller burst
            if (HasExploded == 0f && !Main.dedServ)
            {
                // SoftRadialBloom 2160px: 0.05 ↁE~108px
                CometParticleHandler.Spawn(new CraterBloomParticle(
                    Projectile.Center, CometUtils.SupernovaColor, 0.05f, 20));

                for (int i = 0; i < 8; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f);
                    CometParticleHandler.Spawn(new EmberTrailParticle(
                        Projectile.Center, vel, 0.4f, 15));
                }
            }
        }
    }
}
