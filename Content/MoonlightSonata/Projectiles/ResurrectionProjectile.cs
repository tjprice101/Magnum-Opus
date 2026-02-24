using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Resurrection Projectile — "The Comet Round".
    /// Devastating bullet that ricochets 10 times to nearby enemies.
    /// Each hit creates crater detonations with escalating god ray bursts.
    ///
    /// VFX overhaul: Raw additive 3-layer trail replaced with:
    ///   - CalamityStyleTrailRenderer.DrawTrailWithBloom (Cosmic style, thick comet trail)
    ///   - 4-layer {A=0} bloom stack body via ResurrectionVFX.DrawProjectileBloom
    ///   - MotionBlurBloomRenderer for velocity-based stretch
    ///   - ResurrectionVFX for themed comet/crater/impact effects
    ///   - Lightning chains between ricochet points
    /// </summary>
    public class ResurrectionProjectile : ModProjectile
    {
        private int ricochetCount = 0;
        private const int MaxRicochets = 10;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = MaxRicochets + 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Comet trail VFX — dense, heavy trail befitting a sniper round
            ResurrectionVFX.CometTrailFrame(Projectile.Center, Projectile.velocity, ricochetCount);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Musical Dissonance debuff
            target.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 300);

            // Seeking crystals — 33% chance on hit
            if (Main.rand.NextBool(3))
            {
                SeekingCrystalHelper.SpawnMoonlightCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.18f),
                    Projectile.knockBack,
                    Projectile.owner,
                    4
                );
            }

            // Crater detonation with moonbeam lances
            ResurrectionVFX.OnHitExplosion(target.Center, ricochetCount, hit.Crit);

            // Explosion sound
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f, Pitch = 0.3f }, target.Center);

            // Attempt to ricochet to another enemy
            if (ricochetCount < MaxRicochets)
            {
                NPC newTarget = FindNearestEnemy(target.Center, 800f, target.whoAmI);
                if (newTarget != null)
                {
                    ricochetCount++;

                    // Redirect toward new target
                    Vector2 direction = (newTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    float speed = Projectile.velocity.Length();
                    if (speed < 20f) speed = 20f;
                    Projectile.velocity = direction * speed;

                    // Reset time left for continued flight
                    Projectile.timeLeft = Math.Max(Projectile.timeLeft, 60);

                    // Ricochet sound — escalating pitch
                    float pitch = -0.3f + (ricochetCount * 0.1f);
                    SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = pitch }, Projectile.Center);

                    // Lightning chain between ricochet points
                    MagnumVFX.DrawMoonlightLightning(Projectile.Center, newTarget.Center, 10, 30f, 3, 0.4f);

                    // Ricochet VFX — escalating god rays
                    ResurrectionVFX.OnRicochetVFX(Projectile.Center, Projectile.velocity, ricochetCount);
                }
            }
        }

        private NPC FindNearestEnemy(Vector2 position, float range, int excludeWhoAmI)
        {
            NPC closest = null;
            float closestDist = range;

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.whoAmI != excludeWhoAmI && npc.CanBeChasedBy(Projectile) &&
                    Collision.CanHitLine(position, 1, 1, npc.position, npc.width, npc.height))
                {
                    float dist = Vector2.Distance(position, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }

            return closest;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            ResurrectionVFX.WallHitVFX(Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.7f, Pitch = 0.2f }, Projectile.Center);
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            // Final comet detonation
            ResurrectionVFX.DeathVFX(Projectile.Center, ricochetCount);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // === CALAMITY-STYLE TRAIL RENDERING — thick comet trail ===
            if (Projectile.oldPos.Length > 1)
            {
                Vector2[] trailPositions = new Vector2[Projectile.oldPos.Length];
                float[] trailRotations = new float[Projectile.oldPos.Length];
                int validCount = 0;

                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) break;
                    trailPositions[i] = Projectile.oldPos[i] + Projectile.Size / 2f;
                    trailRotations[i] = Projectile.oldRot[i];
                    validCount++;
                }

                if (validCount > 1)
                {
                    if (validCount < trailPositions.Length)
                    {
                        Array.Resize(ref trailPositions, validCount);
                        Array.Resize(ref trailRotations, validCount);
                    }

                    float bounceIntensity = 1f + ricochetCount * 0.1f;

                    // Thick comet trail — Cosmic style, intensifies with ricochets
                    CalamityStyleTrailRenderer.DrawTrailWithBloom(
                        trailPositions, trailRotations,
                        CalamityStyleTrailRenderer.TrailStyle.Cosmic,
                        baseWidth: 16f + ricochetCount * 1.5f,
                        primaryColor: ResurrectionVFX.CometTrail,
                        secondaryColor: ResurrectionVFX.MoonriseGold,
                        intensity: 0.9f * bounceIntensity,
                        bloomMultiplier: 2.5f + ricochetCount * 0.2f);
                }
            }

            // === 4-LAYER {A=0} BLOOM STACK BODY ===
            ResurrectionVFX.DrawProjectileBloom(sb, Projectile.Center, Projectile.velocity.Length(), ricochetCount);

            // === MOTION BLUR BLOOM (velocity-based stretch) ===
            if (Projectile.velocity.LengthSquared() > 4f)
            {
                Texture2D texture = TextureAssets.Projectile[Type].Value;
                MotionBlurBloomRenderer.DrawProjectile(sb, texture, Projectile,
                    ResurrectionVFX.CometTrail, ResurrectionVFX.MoonriseGold, intensityMult: 0.6f);
            }

            return false;
        }
    }
}
