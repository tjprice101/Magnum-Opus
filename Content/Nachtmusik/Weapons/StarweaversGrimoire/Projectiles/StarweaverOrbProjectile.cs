using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.Graphics;
using System;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
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
        private VertexStrip _vertexStrip;
        
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

            // Palette-ramped trail sparkles
            if (Timer % 5 == 0)
                NachtmusikVFXLibrary.SpawnGradientSparkles(Projectile.Center, Projectile.velocity, 1, 0.2f, 16, 8f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 300);
            StarweaversGrimoireVFX.OrbImpactVFX(target.Center, Projectile.velocity);

            // Palette-ramped sparkle explosion
            NachtmusikVFXLibrary.SpawnGradientSparkleExplosion(target.Center, 8, 5f, 0.3f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.Nachtmusik, ref _vertexStrip);

                // Starweaver accent: ConstellationWeave shader — orbiting constellation web
                float time = (float)Main.timeForVisualEffects * 0.05f;
                float shaderTime = (float)Main.timeForVisualEffects * 0.03f;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null && NachtmusikShaderManager.HasConstellationWeave)
                {
                    Vector2 origin = glow.Size() / 2f;

                    NachtmusikShaderManager.BeginShaderAdditive(sb);

                    // Constellation star points with ConstellationWeave shader
                    NachtmusikShaderManager.ApplyConstellationWeave(shaderTime, 0.7f);
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = time + MathHelper.TwoPi * i / 3f;
                        Vector2 offset = angle.ToRotationVector2() * 12f;
                        sb.Draw(glow, drawPos + offset, null,
                            (NachtmusikPalette.StarlitBlue with { A = 0 }) * 0.4f,
                            0f, origin, 0.03f, SpriteEffects.None, 0f);
                    }

                    // Connecting lines with glow shader pass
                    NachtmusikShaderManager.ApplyConstellationWeaveGlow(shaderTime, 0.7f);
                    for (int i = 0; i < 3; i++)
                    {
                        float a1 = time + MathHelper.TwoPi * i / 3f;
                        float a2 = time + MathHelper.TwoPi * ((i + 1) % 3) / 3f;
                        Vector2 mid = (a1.ToRotationVector2() + a2.ToRotationVector2()) * 6f;
                        float lineAngle = (a2.ToRotationVector2() - a1.ToRotationVector2()).ToRotation();
                        sb.Draw(glow, drawPos + mid, null,
                            (NachtmusikPalette.Silver with { A = 0 }) * 0.2f,
                            lineAngle, origin, new Vector2(0.1f, 0.01f), SpriteEffects.None, 0f);
                    }

                    // NK Lens Flare at center
                    Texture2D flareTex = NachtmusikThemeTextures.NKLensFlare?.Value;
                    if (flareTex != null)
                    {
                        Vector2 flareOrigin = flareTex.Size() / 2f;
                        float pulse = 0.85f + 0.15f * MathF.Sin(shaderTime * 4f);
                        Color flareColor = NachtmusikPalette.StarlitBlue with { A = 0 } * 0.18f * pulse;
                        sb.Draw(flareTex, drawPos, null, flareColor,
                            shaderTime * 0.3f, flareOrigin, 0.04f * pulse, SpriteEffects.None, 0f);
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

                    Vector2 origin = glow.Size() / 2f;
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = time + MathHelper.TwoPi * i / 3f;
                        Vector2 offset = angle.ToRotationVector2() * 12f;
                        sb.Draw(glow, drawPos + offset, null,
                            (NachtmusikPalette.StarlitBlue with { A = 0 }) * 0.35f,
                            0f, origin, 0.025f, SpriteEffects.None, 0f);
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        float a1 = time + MathHelper.TwoPi * i / 3f;
                        float a2 = time + MathHelper.TwoPi * ((i + 1) % 3) / 3f;
                        Vector2 mid = (a1.ToRotationVector2() + a2.ToRotationVector2()) * 6f;
                        float lineAngle = (a2.ToRotationVector2() - a1.ToRotationVector2()).ToRotation();
                        sb.Draw(glow, drawPos + mid, null,
                            (NachtmusikPalette.Silver with { A = 0 }) * 0.15f,
                            lineAngle, origin, new Vector2(0.08f, 0.008f), SpriteEffects.None, 0f);
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
