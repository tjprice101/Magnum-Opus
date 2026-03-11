using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Content.Nachtmusik.Weapons.CelestialChorusBaton.Buffs;
using MagnumOpus.Content.Nachtmusik.Weapons.CelestialChorusBaton.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.CelestialChorusBaton.Projectiles
{
    /// <summary>
    /// Nocturnal Guardian minion — a spectral warrior that orbits the player.
    /// Aggressively slashes at enemies with celestial blades.
    /// Orbit + dash attack pattern with cloud trail VFX during dash.
    /// </summary>
    public class NocturnalGuardianMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Nachtmusik/Weapons/CelestialChorusBaton/NocturnalGuardianMinion";

        private float orbitAngle;
        private int attackCooldown;
        private bool isAttacking;
        private Vector2 attackTarget;
        private int attackTimer;

        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override bool? CanCutTiles() => false;

        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner))
                return;

            orbitAngle += 0.04f;
            attackCooldown = Math.Max(0, attackCooldown - 1);

            NPC target = FindTarget(owner, 800f);

            if (!isAttacking)
            {
                // Orbit around player with pulsing radius
                float orbitRadius = 80f + 30f * (float)Math.Sin(orbitAngle * 0.5f);
                Vector2 idealPos = owner.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                Vector2 toIdeal = idealPos - Projectile.Center;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.15f, 0.1f);

                // Check for attack opportunity
                if (target != null && attackCooldown == 0)
                {
                    isAttacking = true;
                    attackTarget = target.Center;
                    attackTimer = 0;
                    attackCooldown = 45;
                }
            }
            else
            {
                attackTimer++;

                // Dash attack
                if (attackTimer < 15)
                {
                    Vector2 toTarget = (attackTarget - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = toTarget * 22f;

                    // Dash trail VFX
                    NachtmusikVFXLibrary.SpawnCloudTrail(Projectile.Center, Projectile.velocity, 0.4f);

                    // Palette-ramped sparkles during dash
                    NachtmusikVFXLibrary.SpawnGradientSparkles(Projectile.Center, Projectile.velocity, 2, 0.2f, 14, 6f);
                }
                else
                {
                    // Return to orbit
                    isAttacking = false;
                }
            }

            Projectile.rotation = Projectile.velocity.X * 0.02f;
            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;

            // Ambient VFX
            CelestialChorusBatonVFX.MinionAmbientVFX(Projectile.Center, 1f);

            Lighting.AddLight(Projectile.Center, NachtmusikPalette.CosmicPurple.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 300);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);
            CelestialChorusBatonVFX.MinionImpactVFX(target.Center);

            // Palette-ramped sparkle explosion on hit
            NachtmusikVFXLibrary.SpawnGradientSparkleExplosion(target.Center, 6, 4f, 0.25f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Texture2D tex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float time = (float)Main.timeForVisualEffects * 0.03f;
            float pulse = MathF.Sin(Main.GameUpdateCount * 0.1f) * 0.1f + 0.9f;

            // ==============================================================
            //  SHADER LAYER: ChorusSummonAura — spectral guardian presence
            //  Uses dedicated shader. Intensifies during dash attacks.
            // ==============================================================
            float auraIntensity = isAttacking ? 0.85f : 0.4f;
            float auraScale = (isAttacking ? 0.4f : 0.25f) * pulse;

            if (NachtmusikShaderManager.HasChorusSummonAura)
            {
                Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
                if (glowTex != null)
                {
                    NachtmusikShaderManager.BeginShaderAdditive(sb);
                    NachtmusikShaderManager.ApplyChorusSummonAura(time, auraIntensity);

                    // Outer constellation aura
                    sb.Draw(glowTex, drawPos, null,
                        NachtmusikPalette.ConstellationBlue with { A = 0 } * auraIntensity * 0.45f,
                        0f, glowTex.Size() / 2f, auraScale, SpriteEffects.None, 0f);

                    // Inner star-gold core
                    sb.Draw(glowTex, drawPos, null,
                        NachtmusikPalette.StarGold with { A = 0 } * auraIntensity * 0.2f,
                        0f, glowTex.Size() / 2f, auraScale * 0.4f, SpriteEffects.None, 0f);

                    NachtmusikShaderManager.RestoreSpriteBatch(sb);
                }

                // NK Power Ring — rotating aura ring around guardian
                Texture2D ringTex = NachtmusikThemeTextures.NKPowerEffectRing?.Value;
                if (ringTex != null)
                {
                    NachtmusikShaderManager.BeginAdditive(sb);
                    float ringPulse = 0.12f + auraIntensity * 0.08f;
                    sb.Draw(ringTex, drawPos, null,
                        NachtmusikPalette.ConstellationBlue with { A = 0 } * auraIntensity * 0.2f,
                        time * 0.5f, ringTex.Size() / 2f, ringPulse * pulse, SpriteEffects.None, 0f);
                    NachtmusikShaderManager.RestoreSpriteBatch(sb);
                }

                // NK Lens Flare — bright accent during dash attacks
                if (isAttacking)
                {
                    Texture2D flareTex = NachtmusikThemeTextures.NKLensFlare?.Value;
                    if (flareTex != null)
                    {
                        NachtmusikShaderManager.BeginAdditive(sb);
                        sb.Draw(flareTex, drawPos, null,
                            NachtmusikPalette.StarGold with { A = 0 } * 0.15f,
                            -time * 0.6f, flareTex.Size() / 2f, 0.06f, SpriteEffects.None, 0f);
                        NachtmusikShaderManager.RestoreSpriteBatch(sb);
                    }
                }
            }
            else
            {
                // Non-shader fallback — TrueAdditive bloom only
                Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
                if (glowTex != null)
                {
                    NachtmusikShaderManager.BeginAdditive(sb);
                    sb.Draw(glowTex, drawPos, null,
                        NachtmusikPalette.ConstellationBlue with { A = 0 } * auraIntensity * 0.35f,
                        0f, glowTex.Size() / 2f, auraScale, SpriteEffects.None, 0f);
                    sb.Draw(glowTex, drawPos, null,
                        NachtmusikPalette.StarGold with { A = 0 } * auraIntensity * 0.15f,
                        0f, glowTex.Size() / 2f, auraScale * 0.4f, SpriteEffects.None, 0f);
                    NachtmusikShaderManager.RestoreSpriteBatch(sb);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            //  BLOOM LAYER: Glow behind sprite — intensified during attacks
            // ═══════════════════════════════════════════════════════════════
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
            {
                Vector2 glowOrigin = glow.Size() / 2f;
                float attackBoost = isAttacking ? 1.5f : 1f;
                sb.Draw(glow, drawPos, null, NachtmusikPalette.CosmicPurple with { A = 0 } * 0.35f * attackBoost,
                    0f, glowOrigin, 0.7f * pulse, SpriteEffects.None, 0f);
                sb.Draw(glow, drawPos, null, NachtmusikPalette.Violet with { A = 0 } * 0.25f * attackBoost,
                    0f, glowOrigin, 0.5f * pulse, SpriteEffects.None, 0f);
                sb.Draw(glow, drawPos, null, NachtmusikPalette.StarWhite with { A = 0 } * 0.1f * attackBoost,
                    0f, glowOrigin, 0.25f * pulse, SpriteEffects.None, 0f);
            }

            // Main sprite
            sb.Draw(tex, drawPos, null, Color.White, Projectile.rotation, origin, Projectile.scale, effects, 0f);

            // Nachtmusik theme star flare accent
            NachtmusikShaderManager.BeginAdditive(sb);
            NachtmusikVFXLibrary.DrawThemeStarFlare(sb, Projectile.Center, 1f, 0.5f);
            NachtmusikShaderManager.RestoreSpriteBatch(sb);

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

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<CelestialChorusBatonBuff>());
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<CelestialChorusBatonBuff>()))
                Projectile.timeLeft = 2;

            return true;
        }

        private NPC FindTarget(Player owner, float range)
        {
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC manual = Main.npc[owner.MinionAttackTargetNPC];
                if (manual.active && manual.CanBeChasedBy(Projectile) && Vector2.Distance(owner.Center, manual.Center) < range * 1.5f)
                    return manual;
            }

            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(owner.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
    }
}
