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
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float time = (float)Main.timeForVisualEffects * 0.03f;
            float pulse = MathF.Sin(Main.GameUpdateCount * 0.1f) * 0.1f + 0.9f;

            // ═══════════════════════════════════════════════════════════════
            //  SHADER LAYER: ChorusSummonAura — spectral guardian presence
            //  Intensifies during dash attacks
            // ═══════════════════════════════════════════════════════════════
            if (NachtmusikShaderManager.HasSerenade)
            {
                Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
                if (glowTex != null)
                {
                    float auraIntensity = isAttacking ? 0.85f : 0.4f;

                    NachtmusikShaderManager.BeginShaderAdditive(sb);
                    NachtmusikShaderManager.ApplySerenade(time, NachtmusikPalette.CosmicPurple,
                        NachtmusikPalette.Violet, phase: auraIntensity);

                    float auraScale = (isAttacking ? 0.4f : 0.25f) * pulse;
                    sb.Draw(glowTex, drawPos, null,
                        NachtmusikPalette.CosmicPurple with { A = 0 } * auraIntensity * 0.4f,
                        0f, glowTex.Size() / 2f, auraScale, SpriteEffects.None, 0f);

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
