using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
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
        private VertexStrip _vertexStrip;
        
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

            // Palette-ramped trail sparkles (scales with mode)
            if (Timer % 5 == 0)
            {
                float sparkleScale = Mode == 2f ? 0.3f : (Mode == 1f ? 0.25f : 0.2f);
                NachtmusikVFXLibrary.SpawnGradientSparkles(Projectile.Center, Projectile.velocity, 1, sparkleScale, 16, 8f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), Mode == 2f ? 600 : 300);
            
            if (Mode == 2f)
                RequiemOfTheCosmosVFX.EventHorizonImpactVFX(target.Center);
            else
                RequiemOfTheCosmosVFX.OrbImpactVFX(target.Center, Mode == 1f);

            // Palette-ramped sparkle explosion (scales with mode)
            float modeIntensity = Mode == 2f ? 1.5f : (Mode == 1f ? 1.2f : 1f);
            NachtmusikVFXLibrary.SpawnGradientSparkleExplosion(target.Center, (int)(8 * modeIntensity), 5f * modeIntensity, 0.3f * modeIntensity);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.Nachtmusik, ref _vertexStrip);

                // Cosmic Requiem accent: CosmicRequiem shader — mode-dependent orbiting ring
                float shaderTime = (float)Main.timeForVisualEffects * 0.03f;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null && NachtmusikShaderManager.HasCosmicRequiem)
                {
                    float modeScale = Mode == 2f ? 1.5f : (Mode == 1f ? 1.2f : 1f);
                    float pulse = 0.85f + 0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.08f);
                    float ringRot = (float)Main.timeForVisualEffects * 0.04f;
                    Vector2 origin = glow.Size() / 2f;

                    NachtmusikShaderManager.BeginShaderAdditive(sb);

                    // Gravitational ring with CosmicRequiem shader
                    float phase = Mode == 2f ? 1f : (Mode == 1f ? 0.6f : 0.3f);
                    NachtmusikShaderManager.ApplyCosmicRequiem(shaderTime, phase);
                    Color ringColor = Mode == 2f ? NachtmusikPalette.CosmicPurple : NachtmusikPalette.Violet;
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = ringRot + MathHelper.TwoPi * i / 3f;
                        sb.Draw(glow, drawPos, null,
                            (ringColor with { A = 0 }) * 0.25f * pulse * modeScale,
                            angle, origin, new Vector2(0.16f * modeScale, 0.04f), SpriteEffects.None, 0f);
                    }

                    // Glow pass for inner radiance
                    NachtmusikShaderManager.ApplyCosmicRequiemGlow(shaderTime, phase);
                    Color coreColor = Color.Lerp(NachtmusikPalette.Violet, NachtmusikPalette.StarWhite, Mode == 2f ? 0.5f : 0.2f) with { A = 0 };
                    sb.Draw(glow, drawPos, null, coreColor * 0.15f * pulse * modeScale,
                        0f, origin, 0.05f * modeScale, SpriteEffects.None, 0f);

                    // Event Horizon mode — cosmic noise corona
                    if (Mode == 2f)
                    {
                        Texture2D noiseTex = NachtmusikThemeTextures.NKConstellationNoise?.Value;
                        if (noiseTex != null)
                        {
                            Vector2 noiseOrigin = noiseTex.Size() / 2f;
                            Color coronaColor = NachtmusikPalette.CosmicPurple with { A = 0 } * 0.12f * pulse;
                            sb.Draw(noiseTex, drawPos, null, coronaColor,
                                shaderTime * 0.2f, noiseOrigin, 0.08f * modeScale, SpriteEffects.None, 0f);
                        }
                    }

                    NachtmusikShaderManager.RestoreSpriteBatch(sb);
                }
                else if (glow != null)
                {
                    // Fallback without shader
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                        SamplerState.LinearClamp, DepthStencilState.None,
                        RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                    float modeScale = Mode == 2f ? 1.5f : (Mode == 1f ? 1.2f : 1f);
                    float pulse = 0.85f + 0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.08f);
                    float ringRot = (float)Main.timeForVisualEffects * 0.04f;
                    Vector2 origin = glow.Size() / 2f;
                    Color ringColor = Mode == 2f ? NachtmusikPalette.CosmicPurple : NachtmusikPalette.Violet;
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = ringRot + MathHelper.TwoPi * i / 3f;
                        sb.Draw(glow, drawPos, null,
                            (ringColor with { A = 0 }) * 0.2f * pulse * modeScale,
                            angle, origin, new Vector2(0.14f * modeScale, 0.035f), SpriteEffects.None, 0f);
                    }
                }
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
