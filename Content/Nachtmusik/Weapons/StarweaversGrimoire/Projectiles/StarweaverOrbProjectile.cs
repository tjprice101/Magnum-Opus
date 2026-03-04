using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Content.Nachtmusik.Weapons.StarweaversGrimoire.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.StarweaversGrimoire.Projectiles
{
    public class StarweaverOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle";
        
        // ai[0]: 0 = normal weave orb, 1 = tapestry bolt (homing), 2 = bonus seeking orb
        private float Mode => Projectile.ai[0];
        private ref float Timer => ref Projectile.ai[1];
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 18;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Timer++;
            Projectile.rotation += 0.08f;
            
            // Lighting
            Lighting.AddLight(Projectile.Center, 0.25f, 0.3f, 0.7f);
            
            // Mode-specific behavior
            if (Mode == 1f) // Tapestry bolt - strong homing
            {
                if (Timer > 10)
                {
                    NPC target = FindClosestNPC(600f);
                    if (target != null)
                    {
                        Vector2 toTarget = target.Center - Projectile.Center;
                        toTarget.Normalize();
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 16f, 0.08f);
                    }
                }
            }
            else if (Mode == 2f) // Bonus seeking orb - gentle homing
            {
                if (Timer > 20)
                {
                    NPC target = FindClosestNPC(500f);
                    if (target != null)
                    {
                        Vector2 toTarget = target.Center - Projectile.Center;
                        toTarget.Normalize();
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 12f, 0.04f);
                    }
                }
            }
            else // Normal weave orb - slight gravity curve
            {
                if (Timer > 40)
                    Projectile.velocity *= 0.98f;
            }
            
            // Cosmic dust trail
            if (Timer % 2 == 0)
            {
                Color dustColor = Timer % 4 == 0 ? new Color(40, 30, 100) : new Color(60, 80, 180);
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.MagicMirror, 
                    Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 150, dustColor, 0.8f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.3f;
            }
            
            // Orbiting constellation motes
            if (Timer % 6 == 0)
            {
                float angle = Timer * 0.1f;
                Vector2 orbitPos = Projectile.Center + new Vector2((float)Math.Cos(angle) * 16f, (float)Math.Sin(angle) * 16f);
                int d = Dust.NewDust(orbitPos - new Vector2(2), 4, 4, DustID.PurificationPowder, 0f, 0f, 200, new Color(180, 200, 230), 0.5f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity = Vector2.Zero;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 300);
            StarweaversGrimoireVFX.OrbImpactVFX(target.Center, Projectile.velocity);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;

            float time = (float)Main.timeForVisualEffects * 0.03f;

            // ═══════════════════════════════════════════════════════════════
            //  SHADER LAYER 1: ConstellationWeave GPU trail
            //  Replaces afterimage loop with proper primitive trail renderer
            // ═══════════════════════════════════════════════════════════════
            {
                int validCount = 0;
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] != Vector2.Zero) validCount++;
                    else break;
                }

                if (validCount > 2)
                {
                    var trailPositions = new Vector2[validCount];
                    for (int i = 0; i < validCount; i++)
                        trailPositions[i] = Projectile.oldPos[i] + Projectile.Size * 0.5f;

                    float trailWidth = Mode == 1f ? 10f : 7f;
                    CalamityStyleTrailRenderer.DrawDualLayerTrail(
                        trailPositions, null, CalamityStyleTrailRenderer.TrailStyle.Cosmic,
                        trailWidth, NachtmusikPalette.DeepBlue * 0.45f, NachtmusikPalette.Violet * 0.35f,
                        0.5f, bodyOverbright: 2.5f, coreOverbright: 4f, coreWidthRatio: 0.3f);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            //  SHADER LAYER 2: Serenade aura for constellation presence
            // ═══════════════════════════════════════════════════════════════
            if (NachtmusikShaderManager.HasSerenade)
            {
                Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
                if (glowTex != null)
                {
                    Vector2 drawPos = Projectile.Center - Main.screenPosition;
                    float chargeLevel = Mode == 1f ? 0.8f : (Mode == 2f ? 0.5f : 0.3f);

                    NachtmusikShaderManager.BeginShaderAdditive(spriteBatch);
                    NachtmusikShaderManager.ApplySerenade(time, NachtmusikPalette.DeepBlue,
                        NachtmusikPalette.Violet, phase: chargeLevel);

                    float auraScale = (0.3f + chargeLevel * 0.15f) * (0.9f + 0.1f * MathF.Sin(Timer * 0.1f));
                    spriteBatch.Draw(glowTex, drawPos, null,
                        NachtmusikPalette.DeepBlue with { A = 0 } * 0.3f,
                        Projectile.rotation * 0.3f, glowTex.Size() / 2f, auraScale, SpriteEffects.None, 0f);

                    NachtmusikShaderManager.RestoreSpriteBatch(spriteBatch);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            //  BLOOM LAYER: Multi-scale additive orb core — palette-driven
            // ═══════════════════════════════════════════════════════════════
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float pulse = 0.9f + 0.1f * MathF.Sin(Timer * 0.1f);
                float baseScale = Mode == 1f ? 0.45f : 0.35f;

                // Outer cosmic glow — palette-driven
                spriteBatch.Draw(texture, drawPos, null, NachtmusikPalette.DeepBlue with { A = 0 } * 0.5f * pulse,
                    Projectile.rotation, origin, baseScale * 2f, SpriteEffects.None, 0f);
                // Mid stellar glow
                spriteBatch.Draw(texture, drawPos, null, NachtmusikPalette.ConstellationBlue with { A = 0 } * 0.6f * pulse,
                    Projectile.rotation * 0.8f, origin, baseScale * 1.4f, SpriteEffects.None, 0f);
                // Hot core
                spriteBatch.Draw(texture, drawPos, null, NachtmusikPalette.Silver with { A = 0 } * 0.8f,
                    Projectile.rotation * 0.5f, origin, baseScale * 0.8f, SpriteEffects.None, 0f);
                // Stellar white center
                spriteBatch.Draw(texture, drawPos, null, NachtmusikPalette.StarWhite with { A = 0 } * 0.5f,
                    0f, origin, baseScale * 0.4f, SpriteEffects.None, 0f);

                // Extra bloom halo from registry
                Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
                if (bloomTex != null)
                {
                    spriteBatch.Draw(bloomTex, drawPos, null, NachtmusikPalette.Violet with { A = 0 } * 0.2f,
                        0f, bloomTex.Size() / 2f, baseScale * 1.2f * pulse, SpriteEffects.None, 0f);
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            }

            // Nachtmusik theme star flare accent
            NachtmusikShaderManager.BeginAdditive(spriteBatch);
            NachtmusikVFXLibrary.DrawThemeStarFlare(spriteBatch, Projectile.Center, 1f, 0.5f);
            NachtmusikShaderManager.RestoreSpriteBatch(spriteBatch);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Star burst dissipation
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f);
                int d = Dust.NewDust(Projectile.Center, 0, 0, DustID.MagicMirror, vel.X, vel.Y, 100, new Color(60, 80, 180), 0.9f);
                Main.dust[d].noGravity = true;
            }
        }

        private NPC FindClosestNPC(float maxRange)
        {
            NPC closest = null;
            float closestDist = maxRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
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
    }
}
