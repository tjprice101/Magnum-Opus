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
using MagnumOpus.Content.Nachtmusik.Weapons.RequiemOfTheCosmos.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.RequiemOfTheCosmos.Projectiles
{
    public class CosmicRequiemOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle";
        
        // ai[0]: 0 = normal orb, 1 = gravity well, 2 = Event Horizon
        private float Mode => Projectile.ai[0];
        private ref float Timer => ref Projectile.ai[1];
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 200;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            Timer++;
            Projectile.rotation += 0.05f * (Mode == 2f ? 2f : 1f);
            
            // Scale based on mode
            float baseScale = Mode == 2f ? 2.5f : (Mode == 1f ? 1.4f : 1f);
            if (Mode == 2f)
            {
                Projectile.width = 60;
                Projectile.height = 60;
                Projectile.penetrate = -1; // Event Horizon pierces everything
            }
            
            // Gravity well mode: pull nearby enemies
            if (Mode == 1f && Timer > 15)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < 200f && dist > 20f)
                        {
                            Vector2 pull = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                            npc.velocity += pull * (1f - dist / 200f) * 0.4f;
                        }
                    }
                }
            }
            
            // Event Horizon: massive gravity + slow
            if (Mode == 2f && Timer > 8)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < 350f && dist > 20f)
                        {
                            Vector2 pull = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                            npc.velocity += pull * (1f - dist / 350f) * 0.8f;
                        }
                    }
                }
                
                // Slow down and expand
                Projectile.velocity *= 0.985f;
            }
            
            // Decelerate normal orbs
            if (Mode == 0f && Timer > 30)
                Projectile.velocity *= 0.99f;

            // Lighting
            float lightIntensity = Mode == 2f ? 0.8f : (Mode == 1f ? 0.5f : 0.35f);
            Lighting.AddLight(Projectile.Center, 0.2f * lightIntensity, 0.25f * lightIntensity, 0.6f * lightIntensity);
            
            // Cosmic dust trail
            int dustCount = Mode == 2f ? 4 : 2;
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(Projectile.width * 0.4f, Projectile.height * 0.4f);
                Color dustColor = Main.rand.NextBool() ? new Color(40, 30, 100) : new Color(60, 80, 180);
                int d = Dust.NewDust(Projectile.Center + offset, 0, 0, DustID.MagicMirror, 
                    -Projectile.velocity.X * 0.1f, -Projectile.velocity.Y * 0.1f, 150, dustColor, Mode == 2f ? 1.2f : 0.8f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.2f;
            }
            
            // Event Horizon: swirling accretion disk particles
            if (Mode == 2f && Timer % 2 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = Timer * 0.15f + i * MathHelper.TwoPi / 3f;
                    float radius = 25f + (float)Math.Sin(Timer * 0.08f + i) * 10f;
                    Vector2 diskPos = Projectile.Center + new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius * 0.5f);
                    int d = Dust.NewDust(diskPos - new Vector2(2), 4, 4, DustID.PurificationPowder, 0f, 0f, 100, new Color(180, 200, 230), 0.7f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity = (Projectile.Center - diskPos) * 0.02f;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), Mode == 2f ? 600 : 300);
            
            if (Mode == 2f)
                RequiemOfTheCosmosVFX.EventHorizonImpactVFX(target.Center);
            else
                RequiemOfTheCosmosVFX.OrbImpactVFX(target.Center, Mode == 1f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;

            float time = (float)Main.timeForVisualEffects * 0.03f;
            float modeScale = Mode == 2f ? 2.5f : (Mode == 1f ? 1.4f : 1f);

            // ═══════════════════════════════════════════════════════════════
            //  SHADER LAYER 1: CosmicRequiem GPU trail
            //  Dark void energy trail — wider and denser for Event Horizon mode
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

                    float trailWidth = (Mode == 2f ? 18f : (Mode == 1f ? 10f : 7f));
                    CalamityStyleTrailRenderer.DrawDualLayerTrail(
                        trailPositions, null, CalamityStyleTrailRenderer.TrailStyle.Cosmic,
                        trailWidth, NachtmusikPalette.CosmicVoid * 0.5f, NachtmusikPalette.DeepBlue * 0.4f,
                        Mode == 2f ? 0.7f : 0.45f,
                        bodyOverbright: 2f + (Mode == 2f ? 1.5f : 0f),
                        coreOverbright: 3.5f + (Mode == 2f ? 2f : 0f),
                        coreWidthRatio: 0.3f);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            //  SHADER LAYER 2: Serenade void aura — gravitational presence
            //  Visible for Gravity Well and Event Horizon modes
            // ═══════════════════════════════════════════════════════════════
            if (NachtmusikShaderManager.HasSerenade && Mode >= 1f)
            {
                Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
                if (glowTex != null)
                {
                    Vector2 drawPos = Projectile.Center - Main.screenPosition;
                    float phase = Mode == 2f ? 0.9f : 0.5f;

                    NachtmusikShaderManager.BeginShaderAdditive(spriteBatch);
                    NachtmusikShaderManager.ApplySerenade(time, NachtmusikPalette.CosmicVoid,
                        NachtmusikPalette.ConstellationBlue, phase: phase);

                    float auraScale = (0.3f * modeScale) * (0.85f + 0.15f * MathF.Sin(Timer * 0.08f));
                    spriteBatch.Draw(glowTex, drawPos, null,
                        NachtmusikPalette.CosmicPurple with { A = 0 } * 0.35f,
                        Timer * 0.01f, glowTex.Size() / 2f, auraScale, SpriteEffects.None, 0f);

                    NachtmusikShaderManager.RestoreSpriteBatch(spriteBatch);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            //  BLOOM LAYER: Multi-scale additive void core — palette-driven
            // ═══════════════════════════════════════════════════════════════
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float pulse = 0.85f + 0.15f * MathF.Sin(Timer * 0.08f);

                // Void outer — palette-driven
                spriteBatch.Draw(texture, drawPos, null, NachtmusikPalette.CosmicVoid with { A = 0 } * 0.7f * pulse,
                    Projectile.rotation, origin, 0.5f * modeScale * 2f, SpriteEffects.None, 0f);
                // Deep indigo
                spriteBatch.Draw(texture, drawPos, null, NachtmusikPalette.DeepBlue with { A = 0 } * 0.6f * pulse,
                    Projectile.rotation * 0.7f, origin, 0.5f * modeScale * 1.5f, SpriteEffects.None, 0f);
                // Cosmic blue mid
                spriteBatch.Draw(texture, drawPos, null, NachtmusikPalette.ConstellationBlue with { A = 0 } * 0.7f * pulse,
                    Projectile.rotation * 0.4f, origin, 0.5f * modeScale * 1.1f, SpriteEffects.None, 0f);
                // Starlight core
                spriteBatch.Draw(texture, drawPos, null, NachtmusikPalette.StarWhite with { A = 0 } * 0.8f,
                    0f, origin, 0.5f * modeScale * 0.6f, SpriteEffects.None, 0f);

                // Event Horizon: extra accretion ring glow + bloom halo
                if (Mode == 2f)
                {
                    float ringPulse = 0.6f + 0.4f * MathF.Sin(Timer * 0.12f);
                    spriteBatch.Draw(texture, drawPos, null, NachtmusikPalette.ConstellationBlue with { A = 0 } * 0.3f * ringPulse,
                        Timer * 0.02f, origin, modeScale * 1.8f, SpriteEffects.None, 0f);

                    // Extra void bloom from registry
                    Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
                    if (bloomTex != null)
                    {
                        spriteBatch.Draw(bloomTex, drawPos, null,
                            NachtmusikPalette.CosmicPurple with { A = 0 } * 0.2f * ringPulse,
                            0f, bloomTex.Size() / 2f, modeScale * 0.8f, SpriteEffects.None, 0f);
                    }

                    // Star flare for Event Horizon gravity well
                    Texture2D flareTex = MagnumTextureRegistry.GetRadialBloom();
                    if (flareTex != null)
                    {
                        spriteBatch.Draw(flareTex, drawPos, null,
                            NachtmusikPalette.Violet with { A = 0 } * 0.15f,
                            time * 0.2f, flareTex.Size() / 2f, modeScale * 0.5f * ringPulse, SpriteEffects.None, 0f);
                    }
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
            int burstCount = Mode == 2f ? 30 : 15;
            for (int i = 0; i < burstCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(6f, 6f) * Main.rand.NextFloat(0.5f, 1f);
                Color color = Main.rand.NextBool() ? new Color(40, 30, 100) : new Color(60, 80, 180);
                int d = Dust.NewDust(Projectile.Center, 0, 0, DustID.MagicMirror, vel.X, vel.Y, 80, color, Mode == 2f ? 1.4f : 0.9f);
                Main.dust[d].noGravity = true;
            }
        }
    }
}
