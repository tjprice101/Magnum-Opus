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
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.ConductorOfConstellations.Buffs;
using MagnumOpus.Content.Nachtmusik.Weapons.ConductorOfConstellations.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.ConductorOfConstellations.Projectiles
{
    /// <summary>
    /// Stellar Conductor minion — commands multiple smaller star spirits.
    /// The most powerful summon from Nachtmusik. Takes 2 minion slots.
    /// Hovers above the player, fires 3-star barrages every 25 ticks,
    /// and every 180 ticks performs an orchestra burst of 8 ring stars.
    /// </summary>
    public class StellarConductorMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Nachtmusik/Weapons/ConductorOfConstellations/StellarConductorMinion";

        private float conductAngle;
        private int attackCooldown;
        private int orchestraTimer;

        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 2f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override bool? CanCutTiles() => false;

        public override bool MinionContactDamage() => false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner))
                return;

            conductAngle += 0.015f;
            attackCooldown = Math.Max(0, attackCooldown - 1);
            orchestraTimer++;

            // Hover above player with gentle bob
            float hoverY = (float)Math.Sin(conductAngle * 2f) * 15f;
            Vector2 idealPos = owner.Center + new Vector2(0, -100f + hoverY);
            Vector2 toIdeal = idealPos - Projectile.Center;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.08f, 0.1f);

            NPC target = FindTarget(owner, 900f);

            // Regular attack: Fire 3 star projectiles with angular spread
            if (target != null && attackCooldown == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angleOffset = MathHelper.ToRadians(-15f + 15f * i);
                    Vector2 baseDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Vector2 dir = baseDir.RotatedBy(angleOffset);

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + dir * 20f,
                        dir * 16f, ModContent.ProjectileType<ConductorStarProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                }

                attackCooldown = 25;

                // Conducting gesture VFX
                ConductorOfConstellationsVFX.MinionAttackVFX(Projectile.Center, (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX));
                SoundEngine.PlaySound(SoundID.Item25 with { Pitch = 0.5f, Volume = 0.6f }, Projectile.Center);
            }

            // Periodic orchestra burst — ring of 8 stars
            if (orchestraTimer % 180 == 0 && target != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 dir = angle.ToRotationVector2();
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                        dir * 12f, ModContent.ProjectileType<ConductorStarProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                }

                NachtmusikVFXLibrary.ProjectileImpact(Projectile.Center, 0.7f);
                NachtmusikVFXLibrary.SpawnMusicNotes(Projectile.Center, 12, 6f, 0.7f, 0.9f, 25);

                // Orchestra burst: palette-ramped sparkle explosion
                NachtmusikVFXLibrary.SpawnGradientSparkleExplosion(Projectile.Center, 12, 6f, 0.35f);
            }

            // Ambient VFX
            ConductorOfConstellationsVFX.MinionAmbientVFX(Projectile.Center, 1f);

            Lighting.AddLight(Projectile.Center, NachtmusikPalette.RadianceGold.ToVector3() * 0.6f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Texture2D tex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float time = (float)Main.timeForVisualEffects * 0.03f;
            float pulse = MathF.Sin(Main.GameUpdateCount * 0.08f) * 0.2f + 0.8f;

            // ==============================================================
            //  SHADER LAYER: StellarConductorAura — commanding cosmic presence
            //  Uses dedicated shader. The most powerful NK summoner gets rich aura.
            // ==============================================================
            float phase = (float)(Main.timeForVisualEffects * 0.006f) % 1f;

            if (NachtmusikShaderManager.HasStellarConductorAura)
            {
                Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
                if (glowTex != null)
                {
                    NachtmusikShaderManager.BeginShaderAdditive(sb);
                    NachtmusikShaderManager.ApplyStellarConductorAura(time, phase);

                    float auraScale = 0.5f * pulse;
                    sb.Draw(glowTex, drawPos, null,
                        NachtmusikPalette.MidnightBlue with { A = 0 } * 0.35f,
                        conductAngle * 0.2f, glowTex.Size() / 2f, auraScale, SpriteEffects.None, 0f);

                    // Inner stellar white core
                    sb.Draw(glowTex, drawPos, null,
                        NachtmusikPalette.StarWhite with { A = 0 } * 0.15f,
                        0f, glowTex.Size() / 2f, auraScale * 0.3f, SpriteEffects.None, 0f);

                    NachtmusikShaderManager.RestoreSpriteBatch(sb);
                }

                // NK Power Effect Ring — conductor's commanding aura ring
                Texture2D ringTex = NachtmusikThemeTextures.NKPowerEffectRing?.Value;
                if (ringTex != null)
                {
                    NachtmusikShaderManager.BeginAdditive(sb);
                    float ringScale = 0.15f + MathF.Sin(Main.GameUpdateCount * 0.06f) * 0.02f;
                    sb.Draw(ringTex, drawPos, null,
                        NachtmusikPalette.RadianceGold with { A = 0 } * 0.18f,
                        conductAngle * 0.3f, ringTex.Size() / 2f, ringScale * pulse, SpriteEffects.None, 0f);
                    NachtmusikShaderManager.RestoreSpriteBatch(sb);
                }

                // NK Constellation Noise — nebula corona around conductor
                Texture2D noiseTex = NachtmusikThemeTextures.NKConstellationNoise?.Value;
                if (noiseTex != null)
                {
                    NachtmusikShaderManager.BeginAdditive(sb);
                    sb.Draw(noiseTex, drawPos, null,
                        NachtmusikPalette.CosmicPurple with { A = 0 } * 0.07f,
                        time * 0.1f, noiseTex.Size() / 2f, 0.1f * pulse, SpriteEffects.None, 0f);
                    NachtmusikShaderManager.RestoreSpriteBatch(sb);
                }

                // NK Comet — radial accent flare for the most powerful summoner
                Texture2D cometTex = NachtmusikThemeTextures.NKComet?.Value;
                if (cometTex != null)
                {
                    NachtmusikShaderManager.BeginAdditive(sb);
                    sb.Draw(cometTex, drawPos, null,
                        NachtmusikPalette.StarGold with { A = 0 } * 0.08f,
                        -time * 0.3f, cometTex.Size() / 2f, 0.06f, SpriteEffects.None, 0f);
                    NachtmusikShaderManager.RestoreSpriteBatch(sb);
                }
            }
            else
            {
                // Non-shader fallback — TrueAdditive bloom only
                Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
                if (glowTex != null)
                {
                    float auraScale = 0.5f * pulse;
                    NachtmusikShaderManager.BeginAdditive(sb);
                    sb.Draw(glowTex, drawPos, null,
                        NachtmusikPalette.MidnightBlue with { A = 0 } * 0.3f,
                        conductAngle * 0.2f, glowTex.Size() / 2f, auraScale, SpriteEffects.None, 0f);
                    sb.Draw(glowTex, drawPos, null,
                        NachtmusikPalette.StarWhite with { A = 0 } * 0.12f,
                        0f, glowTex.Size() / 2f, auraScale * 0.3f, SpriteEffects.None, 0f);
                    NachtmusikShaderManager.RestoreSpriteBatch(sb);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            //  BLOOM LAYER: Triple glow — cosmic purple, radiance gold, violet
            //  Enhanced from original simple glow layering
            // ═══════════════════════════════════════════════════════════════
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
            {
                Vector2 glowOrigin = glow.Size() / 2f;
                // Outer cosmic halo
                sb.Draw(glow, drawPos, null, NachtmusikPalette.CosmicPurple with { A = 0 } * 0.4f,
                    0f, glowOrigin, 0.55f * pulse, SpriteEffects.None, 0f);
                // Radiance gold mid
                sb.Draw(glow, drawPos, null, NachtmusikPalette.RadianceGold with { A = 0 } * 0.35f,
                    0f, glowOrigin, 0.4f * pulse, SpriteEffects.None, 0f);
                // Violet inner
                sb.Draw(glow, drawPos, null, NachtmusikPalette.Violet with { A = 0 } * 0.25f,
                    0f, glowOrigin, 0.5f * pulse, SpriteEffects.None, 0f);
                // Stellar white core
                sb.Draw(glow, drawPos, null, NachtmusikPalette.StarWhite with { A = 0 } * 0.12f,
                    0f, glowOrigin, 0.25f * pulse, SpriteEffects.None, 0f);
            }

            // Star flare accent for the conductor
            Texture2D flareTex = MagnumTextureRegistry.GetRadialBloom();
            if (flareTex != null)
            {
                sb.Draw(flareTex, drawPos, null,
                    NachtmusikPalette.RadianceGold with { A = 0 } * 0.12f,
                    time * 0.2f, flareTex.Size() / 2f, 0.12f * pulse, SpriteEffects.None, 0f);
            }

            // Main sprite
            sb.Draw(tex, drawPos, null, Color.White, 0f, origin, Projectile.scale, SpriteEffects.None, 0f);

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
                owner.ClearBuff(ModContent.BuffType<ConductorOfConstellationsBuff>());
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<ConductorOfConstellationsBuff>()))
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
