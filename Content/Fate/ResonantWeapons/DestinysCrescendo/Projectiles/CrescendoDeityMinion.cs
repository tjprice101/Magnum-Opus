using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.ResonantWeapons.DestinysCrescendo
{
    /// <summary>
    /// Cosmic Deity Minion — the conductor's divine instrument made manifest.
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
        private const int BeamCooldownMax = 120;

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

                // Cosmic beam attack
                if (beamCooldown <= 0)
                {
                    beamCooldown = BeamCooldownMax;
                    FireCosmicBeam(target);
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

        // ═══════════ SLASH ATTACK ═══════════

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

        // ═══════════ COSMIC BEAM ATTACK ═══════════

        private void FireCosmicBeam(NPC target)
        {
            Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                direction * 18f,
                ModContent.ProjectileType<CrescendoCosmicBeam>(),
                (int)(Projectile.damage * 1.5f),
                Projectile.knockBack,
                Projectile.owner
            );

            // === BEAM FIRE VFX ===
            // Glyph burst at emission point
            CrescendoParticleHandler.SpawnBurst(Projectile.Center, 8, 6f, 0.3f, CrescendoUtils.DeityPurple, CrescendoParticleType.GlyphCircle, 20);

            // Star sparkle explosion
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Color sparkCol = CrescendoUtils.GetCrescendoGradient((float)i / 12f);
                CrescendoParticleHandler.Spawn(CrescendoParticleFactory.DivineSpark(Projectile.Center, sparkVel, sparkCol, 0.2f, 16));
            }

            // Central flash
            CrescendoParticleHandler.Spawn(CrescendoParticleFactory.BeamFlare(Projectile.Center, Vector2.Zero, CrescendoUtils.CelestialWhite, 0.7f, 14));

            // Music notes cascade
            CrescendoParticleFactory.SpawnCosmicNotes(Projectile.Center, 4, 25f);

            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f }, Projectile.Center);
        }

        // ═══════════ PER-FRAME DEITY AURA ═══════════

        private void SpawnDeityAura()
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;

            // Orbiting glyphs — 4 glyphs in divine formation (every 4 frames)
            if (Main.GameUpdateCount % 4 == 0)
            {
                float orbitAngle = Main.GameUpdateCount * 0.06f;
                CrescendoParticleFactory.SpawnOrbitingGlyphs(Projectile.Center, 4, 40f, orbitAngle, 0.3f);
            }

            // Star sparkles — constant celestial shimmer
            if (Main.rand.NextBool(5))
            {
                Vector2 sparkPos = Projectile.Center + Main.rand.NextVector2Circular(25f, 25f);
                Color sparkCol = Main.rand.NextBool(3) ? CrescendoUtils.StarGold : CrescendoUtils.CelestialWhite;
                CrescendoParticleHandler.Spawn(CrescendoParticleFactory.OrbGlow(sparkPos,
                    Main.rand.NextVector2Circular(0.4f, 0.4f), sparkCol * 0.4f, 0.14f, 14));
            }

            // Cosmic cloud wisps — nebula aura
            if (Main.rand.NextBool(4))
            {
                CrescendoParticleFactory.SpawnAuraWisps(Projectile.Center, 1, 20f);
            }

            // Cosmic electricity — occasional divine sparks
            if (Main.rand.NextBool(10))
            {
                Vector2 elecPos = Projectile.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 elecVel = Main.rand.NextVector2Circular(3f, 3f);
                CrescendoParticleHandler.Spawn(CrescendoParticleFactory.DivineSpark(elecPos, elecVel,
                    CrescendoUtils.DivineCrimson * 0.7f, 0.12f, 10));
            }

            // Music notes — the deity hums the crescendo
            if (Main.rand.NextBool(12))
            {
                CrescendoParticleFactory.SpawnCosmicNotes(Projectile.Center, 1, 15f);
            }

            // Pulsing divine light
            float pulse = 0.35f + MathF.Sin(time * 0.1f) * 0.12f;
            Lighting.AddLight(Projectile.Center, CrescendoUtils.DivineCrimson.ToVector3() * pulse);
        }

        // ═══════════ ON HIT ═══════════

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 120);

            // Cosmic impact burst
            CrescendoParticleHandler.SpawnBurst(target.Center, 8, 5f, 0.18f, CrescendoUtils.CrescendoPink, CrescendoParticleType.DivineSpark, 14);
            CrescendoParticleFactory.SpawnCosmicNotes(target.Center, 3, 15f);

            // Impact flash
            CrescendoParticleHandler.Spawn(CrescendoParticleFactory.BeamFlare(target.Center, Vector2.Zero, CrescendoUtils.CelestialWhite * 0.7f, 0.4f, 10));
        }

        // ═══════════ PREDRAW — MULTI-LAYER BLOOM ═══════════

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;

            // Load textures
            Texture2D deityTex = ModContent.Request<Texture2D>("MagnumOpus/Content/Fate/Projectiles/CosmicDeityMinion").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/QuarterNote").Value;

            Vector2 deityOrigin = deityTex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.08f) * 0.1f;
            float breathe = 1f + MathF.Sin(time * 0.04f) * 0.06f;
            SpriteEffects effects = Projectile.velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // === PASS 1: Outer cosmic aura glow (additive, behind sprite) ===
            CrescendoUtils.BeginAdditive(spriteBatch);

            // Layer 1: Vast void purple — the cosmic shadow
            spriteBatch.Draw(glowTex, drawPos, null, CrescendoUtils.VoidBlack * 0.35f, 0f, glowOrigin, 2.8f * breathe, SpriteEffects.None, 0f);

            // Layer 2: Deep deity purple — the entity's resonance field
            spriteBatch.Draw(glowTex, drawPos, null, CrescendoUtils.DeityPurple * 0.45f, 0f, glowOrigin, 2.0f * pulse, SpriteEffects.None, 0f);

            // Layer 3: Crescendo pink — fate's heartbeat
            spriteBatch.Draw(glowTex, drawPos, null, CrescendoUtils.CrescendoPink * 0.55f, 0f, glowOrigin, 1.4f * pulse, SpriteEffects.None, 0f);

            // Layer 4: Divine crimson — the deity's inner fire
            spriteBatch.Draw(glowTex, drawPos, null, CrescendoUtils.DivineCrimson * 0.4f, 0f, glowOrigin, 0.9f * pulse, SpriteEffects.None, 0f);

            CrescendoUtils.BeginAlpha(spriteBatch);

            // === PASS 2: Main deity sprite ===
            spriteBatch.Draw(deityTex, drawPos, null, Color.White * 0.95f, 0f, deityOrigin, 1f, effects, 0f);

            // === PASS 3: Inner bright glow on top (additive) ===
            CrescendoUtils.BeginAdditive(spriteBatch);

            // Star gold divine radiance
            spriteBatch.Draw(glowTex, drawPos, null, CrescendoUtils.StarGold * 0.25f, 0f, glowOrigin, 0.7f * pulse, SpriteEffects.None, 0f);

            // Celestial white core
            spriteBatch.Draw(glowTex, drawPos, null, CrescendoUtils.CelestialWhite * 0.3f, 0f, glowOrigin, 0.4f * pulse, SpriteEffects.None, 0f);

            CrescendoUtils.BeginAlpha(spriteBatch);

            return false;
        }
    }
}
