using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Utilities;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Projectiles
{
    /// <summary>
    /// Fast slash blade wave projectile fired by Twilight Severance.
    /// Normal variant: subtle glow trail, 1 CelestialHarmony stack, thin afterimage.
    /// Dimension Sever variant (ai[0]==1): intense cosmic trail, 2 stacks, chromatic glow, wider afterimages.
    /// </summary>
    public class TwilightSlashProjectile : ModProjectile
    {
        private static readonly Color NightVoid = new Color(10, 10, 30);
        private static readonly Color DeepIndigo = new Color(40, 30, 100);
        private static readonly Color CosmicBlue = new Color(60, 80, 180);
        private static readonly Color StarlightSilver = new Color(180, 200, 230);
        private static readonly Color MoonPearl = new Color(220, 225, 245);
        private static readonly Color StellarWhite = new Color(240, 245, 255);

        private bool IsDimensionSever => Projectile.ai[0] == 1f;

        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/QuarterNote";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 40;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            float time = (float)Main.timeForVisualEffects * 0.05f;

            if (IsDimensionSever)
            {
                // Dimension Sever: intense cosmic trail
                if (Main.rand.NextBool(1))
                {
                    float hue = 0.60f + (float)Math.Sin(time * 2.5f + Projectile.whoAmI * 0.7f) * 0.08f;
                    Color trailColor = Main.hslToRgb(hue, 0.75f, 0.6f);
                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                        DustID.PurpleTorch,
                        -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                        0, trailColor, 1.2f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }

                // Chromatic-like silver/pearl sparkle
                if (Main.rand.NextBool(2))
                {
                    Color sparkColor = Color.Lerp(StarlightSilver, MoonPearl, Main.rand.NextFloat());
                    Dust s = Dust.NewDustPerfect(Projectile.Center, DustID.Enchanted_Gold,
                        Main.rand.NextVector2Circular(2.5f, 2.5f), 0, sparkColor, 0.8f);
                    s.noGravity = true;
                }

                Lighting.AddLight(Projectile.Center, CosmicBlue.ToVector3() * 0.6f);
            }
            else
            {
                // Normal: subtle glow trail
                if (Main.rand.NextBool(2))
                {
                    Color trailColor = Color.Lerp(DeepIndigo, CosmicBlue, Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                        DustID.PurpleTorch,
                        -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                        0, trailColor, 0.85f);
                    d.noGravity = true;
                    d.fadeIn = 0.9f;
                }

                // Occasional silver accent
                if (Main.rand.NextBool(5))
                {
                    Dust s = Dust.NewDustPerfect(Projectile.Center, DustID.Enchanted_Gold,
                        Main.rand.NextVector2Circular(1.5f, 1.5f), 0, StarlightSilver, 0.5f);
                    s.noGravity = true;
                }

                Lighting.AddLight(Projectile.Center, CosmicBlue.ToVector3() * 0.3f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int stacks = IsDimensionSever ? 2 : 1;
            int duration = IsDimensionSever ? 480 : 300;

            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), duration);
            if (target.TryGetGlobalNPC(out CelestialHarmonyNPC harmonyNPC))
                harmonyNPC.AddStack(target, stacks);

            // Impact flash — scales with variant
            float intensity = IsDimensionSever ? 0.7f : 0.4f;
            CustomParticles.GenericFlare(target.Center, StarlightSilver, intensity, IsDimensionSever ? 16 : 10);
            CustomParticles.HaloRing(target.Center, CosmicBlue, intensity * 0.5f, IsDimensionSever ? 12 : 8);

            // Scattered starlight
            int sparkCount = IsDimensionSever ? 8 : 4;
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch, vel, 0,
                    Color.Lerp(DeepIndigo, StarlightSilver, Main.rand.NextFloat()), IsDimensionSever ? 1.1f : 0.8f);
                d.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (IsDimensionSever)
            {
                // Cosmic burst dissipation
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                    Color c = Color.Lerp(DeepIndigo, StellarWhite, Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, vel, 0, c, 1.0f);
                    d.noGravity = true;
                }
                CustomParticles.GenericFlare(Projectile.Center, CosmicBlue, 0.4f, 12);
                NachtmusikVFXLibrary.SpawnTwinklingStars(Projectile.Center, 3, 15f);
            }
            else
            {
                // Thin fade dissipation
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, vel, 0,
                        Color.Lerp(DeepIndigo, CosmicBlue, Main.rand.NextFloat()), 0.7f);
                    d.noGravity = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() / 2f;
            float baseScale = IsDimensionSever ? 1.0f : 0.8f;

            // Afterimage trail
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = 1f - (float)i / Projectile.oldPos.Length;

                Color trailColor;
                float trailScale;
                if (IsDimensionSever)
                {
                    // Wider, more intense afterimages for Dimension Sever
                    trailColor = Color.Lerp(DeepIndigo, CosmicBlue, progress) * progress * 0.65f;
                    trailScale = baseScale * (0.7f + progress * 0.3f);
                }
                else
                {
                    // Thin afterimages for normal variant
                    trailColor = Color.Lerp(NightVoid, DeepIndigo, progress) * progress * 0.4f;
                    trailScale = baseScale * (0.5f + progress * 0.3f);
                }
                trailColor.A = 0;

                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.EntitySpriteDraw(tex, drawPos, null, trailColor, Projectile.oldRot[i],
                    origin, trailScale, SpriteEffects.None, 0);
            }

            // Core additive glow
            {
                Vector2 pos = Projectile.Center - Main.screenPosition;

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                if (IsDimensionSever)
                {
                    // Chromatic-like multi-layer glow
                    Main.EntitySpriteDraw(tex, pos, null, CosmicBlue with { A = 0 } * 0.7f,
                        Projectile.rotation, origin, baseScale * 1.5f, SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(tex, pos, null, StarlightSilver with { A = 0 } * 0.45f,
                        Projectile.rotation, origin, baseScale * 1.25f, SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(tex, pos, null, MoonPearl with { A = 0 } * 0.25f,
                        Projectile.rotation, origin, baseScale * 1.1f, SpriteEffects.None, 0);
                }
                else
                {
                    // Simple indigo glow
                    Main.EntitySpriteDraw(tex, pos, null, CosmicBlue with { A = 0 } * 0.5f,
                        Projectile.rotation, origin, baseScale * 1.3f, SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(tex, pos, null, StarlightSilver with { A = 0 } * 0.25f,
                        Projectile.rotation, origin, baseScale * 1.1f, SpriteEffects.None, 0);
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }

            // Nachtmusik theme star flare accent
            NachtmusikShaderManager.BeginAdditive(Main.spriteBatch);
            NachtmusikVFXLibrary.DrawThemeStarFlare(Main.spriteBatch, Projectile.Center, 1f, 0.5f);
            NachtmusikShaderManager.RestoreSpriteBatch(Main.spriteBatch);

            return true;
        }
    }
}
