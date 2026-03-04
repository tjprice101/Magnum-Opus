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
using MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Particles;

namespace MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Projectiles
{
    /// <summary>
    /// The flying homing sword projectile for Coda of Annihilation.
    /// Zenith-style: flies out, homes to enemies, returns to player if no target.
    /// ai[0] = WeaponIndex (0-13), ai[1] = TargetIndex (+1 offset, 0 = none).
    /// Each index renders a different weapon texture and color from all musical themes.
    /// </summary>
    public class CodaZenithSword : ModProjectile
    {
        /// <summary>Which weapon this projectile displays (0-13).</summary>
        public int WeaponIndex
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        /// <summary>Target NPC index (-1 = no target).</summary>
        public int TargetIndex
        {
            get => (int)Projectile.ai[1] - 1;
            set => Projectile.ai[1] = value + 1;
        }

        // Custom trail array (12 positions)
        private Vector2[] trailPositions = new Vector2[12];
        private float[] trailRotations = new float[12];
        private int trailIndex = 0;
        private int attackTimer = 0;
        private float homingStrength = 0f;
        private float rotationSpeed = 0f;

        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/CodaOfAnnihilation";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Initialize trail positions to center
            for (int i = 0; i < trailPositions.Length; i++)
            {
                trailPositions[i] = Projectile.Center;
                trailRotations[i] = Projectile.rotation;
            }

            // Random rotation speed (0.06 to 0.12) with random direction
            rotationSpeed = Main.rand.NextFloat(0.06f, 0.12f) * (Main.rand.NextBool() ? 1 : -1);

            // Spawn flare VFX
            Color weaponColor = CodaUtils.GetWeaponColor(WeaponIndex);
            CodaParticleHandler.SpawnParticle(
                new AnnihilationFlareParticle(Projectile.Center, Color.White, 0.6f, 15));
            CodaParticleHandler.SpawnParticle(
                new AnnihilationFlareParticle(Projectile.Center, weaponColor, 0.5f, 12));
        }

        public override void AI()
        {
            attackTimer++;
            Player owner = Main.player[Projectile.owner];

            // Update trail every 2 frames
            if (attackTimer % 2 == 0)
            {
                trailIndex = (trailIndex + 1) % trailPositions.Length;
                trailPositions[trailIndex] = Projectile.Center;
                trailRotations[trailIndex] = Projectile.rotation;
            }

            // Zenith-style rotation
            Projectile.rotation += rotationSpeed;

            // Homing behavior — 600f range, max 0.25 homing
            NPC target = FindTarget();

            if (target != null)
            {
                TargetIndex = target.whoAmI;

                // Ramp homing strength
                homingStrength = Math.Min(homingStrength + 0.008f, 0.25f);

                Vector2 toTarget = target.Center - Projectile.Center;
                float targetDistance = toTarget.Length();

                if (targetDistance > 0)
                {
                    toTarget.Normalize();
                    float speed = Projectile.velocity.Length();
                    Vector2 desiredVelocity = toTarget * Math.Max(speed, 18f);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, homingStrength);
                }

                // Accelerate when close
                if (targetDistance < 200f)
                {
                    rotationSpeed *= 1.005f;
                    if (Projectile.velocity.Length() < 25f)
                        Projectile.velocity *= 1.03f;
                }
            }
            else
            {
                TargetIndex = -1;
                float distToPlayer = Vector2.Distance(Projectile.Center, owner.Center);

                // Return to player after 60 frames or >800 distance
                if (attackTimer > 60 || distToPlayer > 800f)
                {
                    Vector2 toPlayer = owner.Center - Projectile.Center;
                    toPlayer.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toPlayer * 20f, 0.1f);
                }

                // Kill when close while returning
                if (attackTimer > 90 && distToPlayer < 50f)
                {
                    Projectile.Kill();
                    return;
                }
            }

            // Cap velocity
            float maxSpeed = 28f;
            if (Projectile.velocity.Length() > maxSpeed)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= maxSpeed;
            }

            // === Trail Particles ===
            SpawnTrailParticles();

            // Lighting
            Color lightColor = CodaUtils.GetWeaponColor(WeaponIndex);
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * 0.6f);
        }

        private NPC FindTarget()
        {
            // Check existing target
            if (TargetIndex >= 0 && TargetIndex < Main.maxNPCs)
            {
                NPC existing = Main.npc[TargetIndex];
                if (existing.active && !existing.friendly && existing.CanBeChasedBy())
                    return existing;
            }

            // Find new target within 600f
            float closestDist = 600f;
            NPC closest = null;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy())
                    continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }

            return closest;
        }

        private void SpawnTrailParticles()
        {
            Color weaponColor = CodaUtils.GetWeaponColor(WeaponIndex);

            // Cosmic mote trail
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(10f, 10f);
                Color trailColor = Color.Lerp(weaponColor, CodaUtils.AnnihilationWhite, Main.rand.NextFloat(0.3f));
                CodaParticleHandler.SpawnParticle(new CosmicMoteParticle(
                    Projectile.Center + offset,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f),
                    trailColor * 0.7f,
                    Main.rand.NextFloat(0.2f, 0.35f),
                    Main.rand.Next(12, 20)));
            }

            // Star sparkles
            if (Main.rand.NextBool(5))
            {
                Color starCol = Main.rand.NextBool(3) ? CodaUtils.StarGold : CodaUtils.AnnihilationWhite;
                CodaParticleHandler.SpawnParticle(new CosmicMoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    starCol * 0.5f,
                    0.15f,
                    15));
            }

            // Glyphs (rare)
            if (Main.rand.NextBool(15))
            {
                CodaParticleHandler.SpawnParticle(new GlyphBurstParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    weaponColor * 0.6f,
                    0.25f,
                    18));
            }

            // Music notes — the coda's signature
            if (Main.rand.NextBool(6))
            {
                Color noteColor = Color.Lerp(weaponColor, CodaUtils.CodaPink, Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                CodaParticleHandler.SpawnParticle(new ZenithNoteParticle(
                    Projectile.Center, noteVel, noteColor, 0.35f, 35));
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color weaponColor = CodaUtils.GetWeaponColor(WeaponIndex);

            // Flare burst at impact
            CodaParticleHandler.SpawnParticle(new AnnihilationFlareParticle(target.Center, Color.White, 0.7f, 15));
            CodaParticleHandler.SpawnParticle(new AnnihilationFlareParticle(target.Center, weaponColor, 0.5f, 12));

            // Glyph burst
            for (int i = 0; i < 4; i++)
            {
                CodaParticleHandler.SpawnParticle(new GlyphBurstParticle(
                    target.Center + Main.rand.NextVector2Circular(15f, 15f),
                    weaponColor * 0.7f,
                    0.3f + Main.rand.NextFloat(0.1f),
                    18));
            }

            // Star particles in a ring
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Color starCol = Color.Lerp(weaponColor, CodaUtils.StarGold, (float)i / 6f);
                CodaParticleHandler.SpawnParticle(new ArcSparkParticle(
                    target.Center, vel, starCol * 0.7f, 0.3f, 18));
            }

            // Music note burst on impact
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color noteColor = Color.Lerp(weaponColor, CodaUtils.CodaCrimson, Main.rand.NextFloat());
                CodaParticleHandler.SpawnParticle(new ZenithNoteParticle(
                    target.Center, noteVel, noteColor, 0.4f, 30));
            }

            // Apply DestinyCollapse 180 ticks
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);

            // === ANNIHILATION MARK STACKING ===
            target.AddBuff(ModContent.BuffType<AnnihilationMark>(), 300);
            var annihilationNPC = target.GetGlobalNPC<AnnihilationMarkNPC>();
            bool detonated = annihilationNPC.AddStack(damageDone);
            if (detonated && !Main.dedServ)
            {
                // Massive detonation VFX at max stacks
                for (int j = 0; j < 12; j++)
                {
                    float detonAngle = MathHelper.TwoPi * j / 12f;
                    Vector2 detonVel = detonAngle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                    Color detonCol = CodaUtils.GetAnnihilationGradient((float)j / 12f);
                    CodaParticleHandler.SpawnParticle(new ArcSparkParticle(
                        target.Center, detonVel, detonCol, 0.5f, 20));
                }
                CodaParticleHandler.SpawnParticle(new AnnihilationFlareParticle(
                    target.Center, Color.White, 1.0f, 20));
                CodaParticleHandler.SpawnParticle(new AnnihilationFlareParticle(
                    target.Center, CodaUtils.CodaCrimson, 0.8f, 18));
            }

            // Impact sound
            SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.6f, Pitch = 0.2f }, target.Center);
        }

        public override void OnKill(int timeLeft)
        {
            Color weaponColor = CodaUtils.GetWeaponColor(WeaponIndex);

            // Fade out VFX
            CodaParticleHandler.SpawnParticle(new AnnihilationFlareParticle(
                Projectile.Center, weaponColor, 0.5f, 12));

            for (int i = 0; i < 4; i++)
            {
                CodaParticleHandler.SpawnParticle(new CosmicMoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(2f, 2f),
                    weaponColor * 0.5f,
                    0.25f,
                    15));
            }

            // Final note burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                CodaParticleHandler.SpawnParticle(new ZenithNoteParticle(
                    Projectile.Center, noteVel, Color.White, 0.35f, 30));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Color weaponColor = CodaUtils.GetWeaponColor(WeaponIndex);

            Texture2D weaponTex = CodaUtils.GetWeaponTexture(WeaponIndex);
            if (weaponTex == null)
                weaponTex = TextureAssets.Projectile[Projectile.type].Value;
            if (weaponTex == null) return false;

            Vector2 origin = weaponTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // === Layer 1: Trail afterimages ===
            for (int i = 0; i < trailPositions.Length; i++)
            {
                int actualIndex = (trailIndex - i + trailPositions.Length) % trailPositions.Length;
                Vector2 trailPos = trailPositions[actualIndex] - Main.screenPosition;
                float trailRot = trailRotations[actualIndex];

                float progress = (float)i / trailPositions.Length;
                float trailAlpha = (1f - progress) * 0.5f;
                float trailScale = 1f - progress * 0.3f;

                Color trailColor = Color.Lerp(weaponColor, CodaUtils.AnnihilationWhite, progress * 0.3f) * trailAlpha;
                trailColor.A = 0; // Additive

                spriteBatch.Draw(weaponTex, trailPos, null, trailColor, trailRot, origin, trailScale, SpriteEffects.None, 0f);
            }

            // === Layer 2: Outer glow ===
            Color outerGlow = weaponColor with { A = 0 } * 0.3f;
            spriteBatch.Draw(weaponTex, drawPos, null, outerGlow, Projectile.rotation, origin, 1.15f, SpriteEffects.None, 0f);

            // === Layer 3: Main weapon sprite ===
            spriteBatch.Draw(weaponTex, drawPos, null, Color.White, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);

            // === Layer 4: Inner glow bloom ===
            Color innerGlow = Color.Lerp(weaponColor, Color.White, 0.5f) with { A = 0 } * 0.4f;
            spriteBatch.Draw(weaponTex, drawPos, null, innerGlow, Projectile.rotation, origin, 0.9f, SpriteEffects.None, 0f);

            return false;
        }
    }
}
