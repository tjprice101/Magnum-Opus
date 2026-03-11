using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Dusts;
using MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Projectiles
{
    /// <summary>
    /// V1 Exordium — Pure gold piercing bolt. High damage, clean lines.
    /// FBM noise internally, 3-layer additive rendering.
    /// </summary>
    public class ExordiumBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TrailLength = 20;
        private Vector2[] _trail = new Vector2[TrailLength];
        private int _head;
        private int _timer;
        private VertexStrip _vertexStrip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            _timer++;
            _trail[_head] = Projectile.Center;
            _head = (_head + 1) % TrailLength;
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<HymnVerseDust>(), -Projectile.velocity.X * 0.1f, -Projectile.velocity.Y * 0.1f, 100, HymnTextures.BloomGold, 0.5f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<HymnDebuffNPC>().RegisterVerseHit(target, 0);
            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(target.Center, 3, 4f, 1f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _vertexStrip);

                // Exordium: pure gold directional streak
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;
                    float rot = Projectile.velocity.ToRotation();

                    sb.Draw(glow, drawPos, null,
                        (OdeToJoyPalette.GoldenPollen with { A = 0 }) * 0.22f,
                        rot, origin, new Vector2(0.07f, 0.025f), SpriteEffects.None, 0f);
                }

                sb.End();
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
    }

    /// <summary>
    /// V2 Rising — Petal pink spreading bolt. 3-way fan, applies Jubilant Burn.
    /// Warmer tones, flowing FBM noise.
    /// </summary>
    public class RisingBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";
        private int _timer;
        private VertexStrip _vertexStrip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            _timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<HymnVerseDust>(), 0f, 0f, 120, HymnTextures.PetalPink, 0.4f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<JubilantBurnDebuff>(), 240);
            var hymnNPC = target.GetGlobalNPC<HymnDebuffNPC>();
            hymnNPC.BurnBaseDamage = hit.Damage;
            hymnNPC.RegisterVerseHit(target, 1);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _vertexStrip);

                // Rising bolt: warmly colored petal-pink directional glow
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;
                    float rot = Projectile.velocity.ToRotation();

                    sb.Draw(glow, drawPos, null,
                        (OdeToJoyPalette.PetalPink with { A = 0 }) * 0.2f,
                        rot, origin, new Vector2(0.06f, 0.025f), SpriteEffects.None, 0f);
                }

                sb.End();
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
    }

    /// <summary>
    /// V3 Apex — Largest orb. Hovers at target position for 60 frames, then detonates.
    /// PerlinFlow noise, golden + jubilant colors.
    /// </summary>
    public class ApexOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";
        private int _timer;
        private bool _hovering;
        private Vector2 _hoverPos;
        private VertexStrip _vertexStrip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            _timer++;

            if (!_hovering)
            {
                // Travel toward initial target
                if (_timer > 30 || Projectile.velocity.Length() < 2f)
                {
                    _hovering = true;
                    _hoverPos = Projectile.Center;
                    Projectile.velocity = Vector2.Zero;
                    Projectile.timeLeft = 80; // 60 hover + 20 detonation
                }
            }
            else
            {
                Projectile.Center = _hoverPos;
                Projectile.velocity = Vector2.Zero;

                // Pulsing glow particles
                float pulse = 0.7f + 0.3f * (float)Math.Sin(_timer * 0.15f);
                if (Main.rand.NextBool(2))
                {
                    Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                    Dust d = Dust.NewDustDirect(Projectile.Center - new Vector2(15), 30, 30, ModContent.DustType<HymnVerseDust>(), vel.X, vel.Y, 100, HymnTextures.JubilantLight, 0.5f * pulse);
                    d.noGravity = true;
                }

                // Detonate at end of hover
                if (Projectile.timeLeft <= 20 && Projectile.timeLeft == 20)
                {
                    // Set hitbox large for AoE
                    Projectile.Resize(200, 200);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<HymnDebuffNPC>().RegisterVerseHit(target, 2);
        }

        public override void OnKill(int timeLeft)
        {
            // AoE detonation VFX
            for (int i = 0; i < 45; i++)
            {
                float angle = MathHelper.TwoPi * i / 45f;
                float speed = 4f + Main.rand.NextFloat() * 3f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                Color c = Color.Lerp(HymnTextures.BloomGold, HymnTextures.JubilantLight, Main.rand.NextFloat());
                Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, ModContent.DustType<HymnVerseDust>(), vel.X, vel.Y, 80, c, 0.9f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            // Apex detonation screen effects
            OdeToJoyVFXLibrary.ScreenShake(8f, 16);
            OdeToJoyVFXLibrary.ScreenFlash(OdeToJoyPalette.SunlightYellow, 1.2f);
            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(Projectile.Center, 5, 4f, 1f);
            OdeToJoyVFXLibrary.RhythmicPulse(Projectile.Center, 1.0f, OdeToJoyPalette.GoldenPollen);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _vertexStrip);

                // Apex orb: jubilant radiance halo (pulses while hovering)
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;
                    float pulse = 0.7f + 0.3f * MathF.Sin((float)Main.timeForVisualEffects * 0.12f);
                    float hoverIntensity = _hovering ? 1.3f : 1f;

                    sb.Draw(glow, drawPos, null,
                        (OdeToJoyPalette.SunlightYellow with { A = 0 }) * 0.2f * pulse * hoverIntensity,
                        0f, origin, 0.05f * hoverIntensity, SpriteEffects.None, 0f);
                }

                sb.End();
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
    }

    /// <summary>
    /// V4 Gloria — Fracturing bolt that splits into 6 homing fragments on contact.
    /// VoronoiCell noise, amber → white. Encore triggers on kills from Complete Hymn.
    /// </summary>
    public class GloriaBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";
        private int _timer;
        private VertexStrip _vertexStrip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            _timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<HymnVerseDust>(), 0f, 0f, 100, HymnTextures.RadiantAmber, 0.5f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<HymnDebuffNPC>().RegisterVerseHit(target, 3);

            // Split into 6 homing fragments
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 8f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel,
                        ModContent.ProjectileType<GloriaFragmentProjectile>(), Projectile.damage / 3, 1f, Projectile.owner);
                }
            }

            // Split VFX
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, ModContent.DustType<HymnVerseDust>(), vel.X, vel.Y, 80, HymnTextures.PureJoyWhite, 0.7f);
                d.noGravity = true;
            }

            // Gloria split sparkle burst
            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(Projectile.Center, 4, 4f, 1f);

            // Encore check — if part of Complete Hymn and kill happens
            if (target.life <= 0)
            {
                Main.player[Projectile.owner].GetModPlayer<HymnPlayer>().TriggerEncore();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _vertexStrip);

                // Gloria bolt: amber fracture glow
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;
                    float rot = Projectile.velocity.ToRotation();

                    // Amber directional streak
                    sb.Draw(glow, drawPos, null,
                        (OdeToJoyPalette.WarmAmber with { A = 0 }) * 0.22f,
                        rot, origin, new Vector2(0.07f, 0.028f), SpriteEffects.None, 0f);
                    // White-hot core accent
                    sb.Draw(glow, drawPos, null,
                        (OdeToJoyPalette.WhiteBloom with { A = 0 }) * 0.1f,
                        0f, origin, 0.025f, SpriteEffects.None, 0f);
                }

                sb.End();
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
    }

    /// <summary>
    /// Gloria Fragment — Small homing fragment from Gloria split.
    /// AttackFoundation Mode 4. Golden sparkle trail.
    /// </summary>
    public class GloriaFragmentProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";
        private int _timer;
        private VertexStrip _vertexStrip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            _timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Homing after brief scatter
            if (_timer > 10)
            {
                NPC closest = null;
                float closestDist = 500f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
                if (closest != null)
                {
                    Vector2 toTarget = (closest.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 12f, 0.06f);
                }
            }

            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<HymnVerseDust>(), 0f, 0f, 120, HymnTextures.BloomGold, 0.3f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _vertexStrip);

                // Gloria fragment: golden directional streak
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;
                    float rot = Projectile.velocity.ToRotation();

                    sb.Draw(glow, drawPos, null,
                        (OdeToJoyPalette.GoldenPollen with { A = 0 }) * 0.18f,
                        rot, origin, new Vector2(0.04f, 0.018f), SpriteEffects.None, 0f);
                }

                sb.End();
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
    }
}
