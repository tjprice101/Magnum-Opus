using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.Debuffs;
using ReLogic.Content;

namespace MagnumOpus.Content.Fate.ResonantWeapons.DestinysCrescendo
{
    /// <summary>
    /// Cosmic Deity Minion 遯ｶ繝ｻthe conductor's divine instrument made manifest.
    /// 
    /// Idle: Floats near owner with orbiting glyph particles, cosmic mist, and divine light pulse.
    /// Combat: Seeks nearest enemy within 800f, approaches at offset, then:
    ///   - Slash Attack (12-tick cooldown, range &lt; 150f): Rapid multihit melee slash with arc particles
    ///   - Cosmic Beam (120-tick cooldown): Fires CrescendoCosmicBeam at 1.5x damage, 18f speed
    /// On Hit: Applies DestinyCollapse debuff.
    /// PreDraw: Multi-layer additive bloom (4 layers) on the deity sprite.
    /// Per-frame: Orbiting glyphs, star sparkles, cosmic cloud wisps, pulsing divine light.
    /// 
    /// Uses texture: MagnumOpus/Content/Fate/Projectiles/CosmicDeityMinion (120x68 single frame)
    /// ZERO shared VFX system references.
    /// </summary>
    public class CrescendoDeityMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Fate/Projectiles/CosmicDeityMinion";

        private int attackCooldown = 0;
        private int beamCooldown = 0;
        private const int SlashCooldown = 12;
        private const int BeamCooldownBase = 120;
        private int lastPhase = -1; // Track phase for transition VFX

        // === Foundation Bloom Textures ===
        private static Asset<Texture2D> _pointBloomTex;
        private static Asset<Texture2D> _softRadialBloomTex;
        private static Asset<Texture2D> _starFlareTex;

        private static Color Additive(Color c, float opacity) => new Color(c.R, c.G, c.B, 0) * opacity;

        /// <summary>Beam cooldown scales with Escalation Phase: Pianissimo=120, Piano=100, Forte=80, Fortissimo=60.</summary>
        private int BeamCooldownMax
        {
            get
            {
                int phase = Main.player[Projectile.owner].Crescendo().EscalationPhase;
                return phase switch
                {
                    0 => 120,
                    1 => 100,
                    2 => 80,
                    _ => 60
                };
            }
        }

        /// <summary>Deity visual scale per phase: grows larger as it escalates.</summary>
        private float PhaseScale
        {
            get
            {
                int phase = Main.player[Projectile.owner].Crescendo().EscalationPhase;
                return phase switch
                {
                    0 => 1.0f,
                    1 => 1.15f,
                    2 => 1.3f,
                    _ => 1.5f
                };
            }
        }

        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.minion = true;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 2f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner))
                return;

            // Track active deities
            if (owner.whoAmI == Main.myPlayer)
                owner.Crescendo().ActiveDeityCount = owner.ownedProjectileCounts[Projectile.type];

            var cp = owner.Crescendo();

            // === PHASE TRANSITION VFX ===
            if (cp.EscalationPhase != lastPhase && lastPhase >= 0 && !Main.dedServ)
            {
                // Phase escalation burst 遯ｶ繝ｻexpanding ring of all Fate colors
                CrescendoParticleHandler.SpawnBurst(Projectile.Center, 15, 8f, 0.35f,
                    CrescendoUtils.StarGold, CrescendoParticleType.DivineSpark, 18);
                CrescendoParticleHandler.SpawnBurst(Projectile.Center, 8, 5f, 0.4f,
                    CrescendoUtils.CrescendoPink, CrescendoParticleType.GlyphCircle, 22);
                CrescendoParticleFactory.SpawnCosmicNotes(Projectile.Center, 5, 30f);
                CrescendoParticleHandler.Spawn(CrescendoParticleFactory.BeamFlare(
                    Projectile.Center, Vector2.Zero, CrescendoUtils.CelestialWhite, 0.9f, 16));
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.4f * cp.EscalationPhase, Volume = 0.7f }, Projectile.Center);
            }
            lastPhase = cp.EscalationPhase;

            attackCooldown = Math.Max(0, attackCooldown - 1);
            beamCooldown = Math.Max(0, beamCooldown - 1);

            NPC target = FindTarget();

            if (target != null)
            {
                // === COMBAT MODE ===
                // Move toward target at offset
                Vector2 desiredPos = target.Center - new Vector2(target.direction * 80f, 0);
                Vector2 toDesired = desiredPos - Projectile.Center;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toDesired * 0.15f, 0.1f);

                // Face target
                Projectile.rotation = (target.Center - Projectile.Center).ToRotation();

                // Rapid slash attacks
                if (attackCooldown <= 0 && Vector2.Distance(Projectile.Center, target.Center) < 150f)
                {
                    attackCooldown = SlashCooldown;
                    PerformSlash(target);
                }

                // Cosmic beam attack 遯ｶ繝ｻfires BeamsPerVolley beams based on Escalation Phase
                if (beamCooldown <= 0)
                {
                    beamCooldown = BeamCooldownMax;
                    FireCosmicBeamVolley(target, cp);
                }
            }
            else
            {
                // === IDLE MODE ===
                // Float near owner with gentle bobbing
                float bobOffset = MathF.Sin((float)Main.timeForVisualEffects * 0.04f) * 8f;
                Vector2 idlePos = owner.Center + new Vector2(owner.direction * -60f, -40f + bobOffset);
                Projectile.velocity = (idlePos - Projectile.Center) * 0.05f;
                Projectile.rotation += 0.02f;
            }

            // === PER-FRAME VFX ===
            SpawnDeityAura();
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<CrescendoDeityBuff>());
                Projectile.Kill();
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<CrescendoDeityBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            return true;
        }

        private NPC FindTarget()
        {
            // Check for manual target
            if (Main.player[Projectile.owner].HasMinionAttackTargetNPC)
            {
                NPC target = Main.npc[Main.player[Projectile.owner].MinionAttackTargetNPC];
                if (target.active && target.CanBeChasedBy(Projectile))
                    return target;
            }

            // Find closest within 800f
            float range = 800f;
            NPC closest = null;
            float closestDist = range;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }

            return closest;
        }

        // 隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨翫・SLASH ATTACK 隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨翫・

        private void PerformSlash(NPC target)
        {
            Vector2 slashDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            Vector2 slashPos = Projectile.Center + slashDir * 30f;

            // 12-point cosmic spark arc
            CrescendoParticleFactory.SpawnSlashSparks(slashPos, slashDir, 12);

            // Glyph circle at slash point
            CrescendoParticleHandler.Spawn(CrescendoParticleFactory.GlyphCircle(slashPos, CrescendoUtils.StarGold * 0.6f, 0.25f, 18));

            // Star sparkles
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkPos = slashPos + Main.rand.NextVector2Circular(20f, 20f);
                CrescendoParticleHandler.Spawn(CrescendoParticleFactory.DivineSpark(sparkPos,
                    Main.rand.NextVector2Circular(2f, 2f), CrescendoUtils.StarGold * 0.5f, 0.15f, 14));
            }

            // Impact flash
            CrescendoParticleHandler.Spawn(CrescendoParticleFactory.BeamFlare(slashPos, Vector2.Zero, CrescendoUtils.CelestialWhite, 0.5f, 10));

            // Cosmic notes on slash
            if (Main.rand.NextBool(3))
                CrescendoParticleFactory.SpawnCosmicNotes(slashPos, 1, 10f);

            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.3f, Volume = 0.6f }, Projectile.Center);
        }

        // 隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨翫・COSMIC BEAM VOLLEY (Phase-Scaled) 隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨翫・

        private void FireCosmicBeamVolley(NPC primaryTarget, CrescendoPlayer cp)
        {
            int beamCount = cp.BeamsPerVolley;
            float phaseMultiplier = 1f + cp.EscalationPhase * 0.15f; // 1x/1.15x/1.3x/1.45x damage

            // Gather potential targets for multi-beam spread
            var targets = new System.Collections.Generic.List<NPC>();
            targets.Add(primaryTarget);

            // For multi-beam phases, find alternate targets
            if (beamCount > 1)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.CanBeChasedBy(Projectile) && npc.whoAmI != primaryTarget.whoAmI
                        && Vector2.Distance(Projectile.Center, npc.Center) < 900f)
                    {
                        targets.Add(npc);
                        if (targets.Count >= beamCount) break;
                    }
                }
            }

            // Fire beams 遯ｶ繝ｻdistribute across available targets
            for (int b = 0; b < beamCount; b++)
            {
                NPC beamTarget = targets[b % targets.Count];
                Vector2 direction = (beamTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

                // Slight angular spread for multi-beams aimed at same target
                if (beamCount > 1 && targets.Count < beamCount)
                {
                    float spreadAngle = MathHelper.ToRadians(8f) * (b - beamCount / 2f);
                    direction = direction.RotatedBy(spreadAngle);
                }

                int beamProj = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    direction * 18f,
                    ModContent.ProjectileType<CrescendoCosmicBeam>(),
                    (int)(Projectile.damage * 1.5f * phaseMultiplier),
                    Projectile.knockBack,
                    Projectile.owner,
                    ai0: cp.EscalationPhase // Pass phase to beam for visual scaling
                );

                // Stagger fire timing for Fortissimo rapid succession feel
                if (cp.EscalationPhase >= 3 && beamProj >= 0 && beamProj < Main.maxProjectiles)
                    Main.projectile[beamProj].timeLeft -= b * 5;
            }

            // === BEAM FIRE VFX (scaled with phase) ===
            int burstCount = 8 + cp.EscalationPhase * 3;
            CrescendoParticleHandler.SpawnBurst(Projectile.Center, burstCount, 6f + cp.EscalationPhase * 1.5f,
                0.3f + cp.EscalationPhase * 0.05f, CrescendoUtils.DeityPurple, CrescendoParticleType.GlyphCircle, 20);

            for (int i = 0; i < 12 + cp.EscalationPhase * 4; i++)
            {
                float angle = MathHelper.TwoPi * i / (12 + cp.EscalationPhase * 4);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f + cp.EscalationPhase * 2f);
                Color sparkCol = CrescendoUtils.GetCrescendoGradient((float)i / (12 + cp.EscalationPhase * 4));
                CrescendoParticleHandler.Spawn(CrescendoParticleFactory.DivineSpark(Projectile.Center, sparkVel, sparkCol, 0.2f + cp.EscalationPhase * 0.04f, 16));
            }

            CrescendoParticleHandler.Spawn(CrescendoParticleFactory.BeamFlare(Projectile.Center, Vector2.Zero,
                CrescendoUtils.CelestialWhite, 0.7f + cp.EscalationPhase * 0.15f, 14));

            CrescendoParticleFactory.SpawnCosmicNotes(Projectile.Center, 4 + cp.EscalationPhase, 25f);

            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f + cp.EscalationPhase * 0.1f }, Projectile.Center);
        }

        // 隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨翫・PER-FRAME DEITY AURA 隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨翫・

        private void SpawnDeityAura()
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;
            int phase = Main.player[Projectile.owner].Crescendo().EscalationPhase;
            float phaseIntensity = 1f + phase * 0.3f; // Aura density scales with phase

            // Orbiting glyphs 遯ｶ繝ｻcount scales with phase (4 base + 2 per phase)
            int glyphInterval = Math.Max(2, 4 - phase);
            if (Main.GameUpdateCount % glyphInterval == 0)
            {
                float orbitAngle = Main.GameUpdateCount * 0.06f;
                int glyphCount = 4 + phase * 2;
                CrescendoParticleFactory.SpawnOrbitingGlyphs(Projectile.Center, glyphCount, 40f + phase * 8f, orbitAngle, 0.3f + phase * 0.05f);
            }

            // Star sparkles 遯ｶ繝ｻintensity scales with phase
            if (Main.rand.NextFloat() < 0.2f * phaseIntensity)
            {
                Vector2 sparkPos = Projectile.Center + Main.rand.NextVector2Circular(25f + phase * 5f, 25f + phase * 5f);
                Color sparkCol = Main.rand.NextBool(3) ? CrescendoUtils.StarGold : CrescendoUtils.CelestialWhite;
                CrescendoParticleHandler.Spawn(CrescendoParticleFactory.OrbGlow(sparkPos,
                    Main.rand.NextVector2Circular(0.4f, 0.4f), sparkCol * (0.4f + phase * 0.1f), 0.14f + phase * 0.03f, 14));
            }

            // Cosmic cloud wisps 遯ｶ繝ｻmore frequent at higher phases
            if (Main.rand.NextFloat() < 0.25f * phaseIntensity * 0.4f)
            {
                CrescendoParticleFactory.SpawnAuraWisps(Projectile.Center, 1, 20f + phase * 5f);
            }

            // Cosmic electricity 遯ｶ繝ｻmore frequent and larger at higher phases
            if (Main.rand.NextFloat() < 0.1f * phaseIntensity)
            {
                Vector2 elecPos = Projectile.Center + Main.rand.NextVector2Circular(30f + phase * 8f, 30f + phase * 8f);
                Vector2 elecVel = Main.rand.NextVector2Circular(3f + phase, 3f + phase);
                CrescendoParticleHandler.Spawn(CrescendoParticleFactory.DivineSpark(elecPos, elecVel,
                    CrescendoUtils.DivineCrimson * (0.7f + phase * 0.1f), 0.12f + phase * 0.03f, 10));
            }

            // Music notes 遯ｶ繝ｻthe deity's hum grows louder with phase
            if (Main.rand.NextFloat() < (0.08f + phase * 0.04f))
            {
                CrescendoParticleFactory.SpawnCosmicNotes(Projectile.Center, 1 + (phase >= 3 ? 1 : 0), 15f + phase * 3f);
            }

            // Pulsing divine light 遯ｶ繝ｻbrighter per phase
            float pulse = (0.35f + phase * 0.12f) + MathF.Sin(time * 0.1f) * (0.12f + phase * 0.04f);
            Lighting.AddLight(Projectile.Center, CrescendoUtils.DivineCrimson.ToVector3() * pulse);
        }

        // 隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨翫・ON HIT 隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨翫・

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 120);

            if (Main.dedServ) return;

            Vector2 hitPos = target.Center;
            int phase = Main.player[Projectile.owner].Crescendo().EscalationPhase;

            // ═══ MULTI-LAYER SPRITEBATCH BLOOM FLASH ═══
            try
            {
                _pointBloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
                _softRadialBloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
                _starFlareTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare");

                SpriteBatch sb = Main.spriteBatch;
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 screenPos = hitPos - Main.screenPosition;
                float time = (float)Main.timeForVisualEffects;
                float phaseGlow = 1f + phase * 0.2f;

                // Layer 1: Deity purple outer haze (scales with phase)
                if (_softRadialBloomTex?.IsLoaded == true)
                {
                    var radTex = _softRadialBloomTex.Value;
                    sb.Draw(radTex, screenPos, null,
                        CrescendoUtils.Additive(CrescendoUtils.DeityPurple, 0.12f * phaseGlow),
                        0f, radTex.Size() * 0.5f, MathHelper.Min(0.49f + phase * 0.07f, 0.139f), SpriteEffects.None, 0f);
                }

                // Layer 2: Crescendo pink mid glow
                if (_softRadialBloomTex?.IsLoaded == true)
                {
                    var radTex = _softRadialBloomTex.Value;
                    sb.Draw(radTex, screenPos, null,
                        CrescendoUtils.Additive(CrescendoUtils.CrescendoPink, 0.14f * phaseGlow),
                        0f, radTex.Size() * 0.5f, MathHelper.Min(0.35f + phase * 0.05f, 0.139f), SpriteEffects.None, 0f);
                }

                // Layer 3: Divine crimson inner
                if (_pointBloomTex?.IsLoaded == true)
                {
                    var ptTex = _pointBloomTex.Value;
                    sb.Draw(ptTex, screenPos, null,
                        CrescendoUtils.Additive(CrescendoUtils.DivineCrimson, 0.18f * phaseGlow),
                        0f, ptTex.Size() * 0.5f, MathHelper.Min(0.21f + phase * 0.04f, 0.139f), SpriteEffects.None, 0f);
                }

                // Layer 4: Star gold hot core
                if (_pointBloomTex?.IsLoaded == true)
                {
                    var ptTex = _pointBloomTex.Value;
                    sb.Draw(ptTex, screenPos, null,
                        CrescendoUtils.Additive(CrescendoUtils.StarGold, 0.21f * phaseGlow),
                        0f, ptTex.Size() * 0.5f, (0.12f + phase * 0.03f), SpriteEffects.None, 0f);
                }

                // Layer 5: StarFlare divine cross
                if (_starFlareTex?.IsLoaded == true)
                {
                    var starTex = _starFlareTex.Value;
                    sb.Draw(starTex, screenPos, null,
                        CrescendoUtils.Additive(CrescendoUtils.DivineCrimson, 0.12f * phaseGlow),
                        time * 0.1f, starTex.Size() * 0.5f, (0.14f + phase * 0.03f), SpriteEffects.None, 0f);
                    sb.Draw(starTex, screenPos, null,
                        CrescendoUtils.Additive(CrescendoUtils.StarGold, 0.09f * phaseGlow),
                        -time * 0.07f, starTex.Size() * 0.5f, (0.1f + phase * 0.02f), SpriteEffects.None, 0f);
                }

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
                    null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch
            {
                try
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                        Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
                        null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }

            // ═══ ENHANCED PARTICLE BURST ═══
            // Cosmic impact burst (original, enhanced count)
            CrescendoParticleHandler.SpawnBurst(hitPos, 12 + phase * 2, 6f + phase, 0.22f + phase * 0.04f,
                CrescendoUtils.CrescendoPink, CrescendoParticleType.DivineSpark, 16);
            CrescendoParticleFactory.SpawnCosmicNotes(hitPos, 4 + phase, 18f);

            // 8 directional slash sparks
            Vector2 hitDir = (hitPos - Projectile.Center).SafeNormalize(Vector2.UnitX);
            Vector2 hitPerp = new Vector2(-hitDir.Y, hitDir.X);
            for (int i = 0; i < 8; i++)
            {
                float spread = (i - 3.5f) / 3.5f;
                Vector2 slashVel = (hitDir * 5f + hitPerp * spread * 6f) * Main.rand.NextFloat(0.8f, 1.2f);
                Color slashCol = CrescendoUtils.GetCrescendoGradient(MathF.Abs(spread));
                CrescendoParticleHandler.Spawn(CrescendoParticleFactory.DivineSpark(
                    hitPos, slashVel, slashCol * 0.7f, 0.12f, 14));
            }

            // Impact flash (original enhanced)
            CrescendoParticleHandler.Spawn(CrescendoParticleFactory.BeamFlare(
                hitPos, Vector2.Zero, CrescendoUtils.CelestialWhite * 0.8f, 0.5f + phase * 0.1f, 12));

            // Dual lighting
            Lighting.AddLight(hitPos, CrescendoUtils.DivineCrimson.ToVector3() * (0.8f + phase * 0.2f));
            Lighting.AddLight(hitPos + hitDir * 16f, CrescendoUtils.StarGold.ToVector3() * 0.5f);
        }

        // 隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨翫・PREDRAW 遯ｶ繝ｻMULTI-LAYER BLOOM 隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨顔ｵｶ豁ｦ隨翫・

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;

            try
            {
                // Load textures
                Texture2D deityTex = ModContent.Request<Texture2D>("MagnumOpus/Content/Fate/Projectiles/CosmicDeityMinion", AssetRequestMode.ImmediateLoad).Value;
                _pointBloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
                _softRadialBloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
                _starFlareTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare");

                Vector2 deityOrigin = deityTex.Size() / 2f;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;

                float time = (float)Main.timeForVisualEffects;
                float pulse = 1f + MathF.Sin(time * 0.08f) * 0.1f;
                float breathe = 1f + MathF.Sin(time * 0.04f) * 0.06f;
                SpriteEffects effects = Projectile.velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                // Phase-based scaling — deity grows larger and brighter per Escalation Phase
                float pScale = PhaseScale;
                float phaseGlow = 1f + (pScale - 1f) * 0.8f; // Glow intensity tracks phase

                // === PASS 1: Outer cosmic aura glow (additive, behind sprite) ===
                CrescendoUtils.BeginAdditive(spriteBatch);

                // Layer 1: Vast void nebula haze (SoftRadialBloom)
                if (_softRadialBloomTex?.IsLoaded == true)
                {
                    var radTex = _softRadialBloomTex.Value;
                    var radOrigin = radTex.Size() * 0.5f;
                    spriteBatch.Draw(radTex, drawPos, null,
                        CrescendoUtils.Additive(CrescendoUtils.VoidBlack, 0.09f * phaseGlow),
                        0f, radOrigin, MathHelper.Min(1.23f * breathe * pScale, 0.139f), SpriteEffects.None, 0f);
                }

                // Layer 2: Deep deity purple resonance field (SoftRadialBloom)
                if (_softRadialBloomTex?.IsLoaded == true)
                {
                    var radTex = _softRadialBloomTex.Value;
                    var radOrigin = radTex.Size() * 0.5f;
                    spriteBatch.Draw(radTex, drawPos, null,
                        CrescendoUtils.Additive(CrescendoUtils.DeityPurple, 0.14f * phaseGlow),
                        0f, radOrigin, MathHelper.Min(0.84f * pulse * pScale, 0.139f), SpriteEffects.None, 0f);
                }

                // Layer 3: Crescendo pink heartbeat (SoftRadialBloom)
                if (_softRadialBloomTex?.IsLoaded == true)
                {
                    var radTex = _softRadialBloomTex.Value;
                    var radOrigin = radTex.Size() * 0.5f;
                    spriteBatch.Draw(radTex, drawPos, null,
                        CrescendoUtils.Additive(CrescendoUtils.CrescendoPink, 0.16f * phaseGlow),
                        0f, radOrigin, MathHelper.Min(0.56f * pulse * pScale, 0.139f), SpriteEffects.None, 0f);
                }

                // Layer 4: Divine crimson inner fire (PointBloom)
                if (_pointBloomTex?.IsLoaded == true)
                {
                    var ptTex = _pointBloomTex.Value;
                    var ptOrigin = ptTex.Size() * 0.5f;
                    spriteBatch.Draw(ptTex, drawPos, null,
                        CrescendoUtils.Additive(CrescendoUtils.DivineCrimson, 0.14f * phaseGlow),
                        0f, ptOrigin, MathHelper.Min(0.35f * pulse * pScale, 0.139f), SpriteEffects.None, 0f);
                }

                CrescendoUtils.BeginAlpha(spriteBatch);

                // === PASS 2: Main deity sprite (scaled with phase) ===
                spriteBatch.Draw(deityTex, drawPos, null, Color.White * 0.95f, 0f, deityOrigin, pScale, effects, 0f);

                // === PASS 3: Inner bright glow on top (additive) ===
                CrescendoUtils.BeginAdditive(spriteBatch);

                // Layer 5: Star gold divine radiance (PointBloom)
                if (_pointBloomTex?.IsLoaded == true)
                {
                    var ptTex = _pointBloomTex.Value;
                    var ptOrigin = ptTex.Size() * 0.5f;
                    spriteBatch.Draw(ptTex, drawPos, null,
                        CrescendoUtils.Additive(CrescendoUtils.StarGold, 0.1f * phaseGlow),
                        0f, ptOrigin, MathHelper.Min(0.25f * pulse * pScale, 0.139f), SpriteEffects.None, 0f);
                }

                // Layer 6: Celestial white core (PointBloom)
                if (_pointBloomTex?.IsLoaded == true)
                {
                    var ptTex = _pointBloomTex.Value;
                    var ptOrigin = ptTex.Size() * 0.5f;
                    spriteBatch.Draw(ptTex, drawPos, null,
                        CrescendoUtils.Additive(CrescendoUtils.CelestialWhite, 0.12f * phaseGlow),
                        0f, ptOrigin, MathHelper.Min(0.14f * pulse * pScale, 0.139f), SpriteEffects.None, 0f);
                }

                // Layer 7: StarFlare rotating divine cross — the deity's signature radiance
                if (_starFlareTex?.IsLoaded == true)
                {
                    var starTex = _starFlareTex.Value;
                    var starOrigin = starTex.Size() * 0.5f;
                    spriteBatch.Draw(starTex, drawPos, null,
                        CrescendoUtils.Additive(CrescendoUtils.DivineCrimson, 0.07f * phaseGlow),
                        time * 0.025f, starOrigin, MathHelper.Min(0.19f * pulse * pScale, 0.293f), SpriteEffects.None, 0f);
                    spriteBatch.Draw(starTex, drawPos, null,
                        CrescendoUtils.Additive(CrescendoUtils.StarGold, 0.05f * phaseGlow),
                        -time * 0.018f, starOrigin, 0.13f * pulse * pScale, SpriteEffects.None, 0f);
                }

                CrescendoUtils.BeginAlpha(spriteBatch);
            }
            catch
            {
                try
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }

            // Theme accents (additive pass)
            try
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                CrescendoUtils.DrawThemeAccents(spriteBatch, Projectile.Center, 1f, 0.6f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch { }

            return false;
        }
    }
}