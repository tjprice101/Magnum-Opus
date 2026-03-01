using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Buffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Dusts;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Primitives;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Utilities;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Utilities.SerenadeUtils;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Projectiles
{
    /// <summary>
    /// SpectralChildBeam — smaller spectral beams that split off from the main
    /// SerenadeBeam after 3+ bounces. Each child has a unique spectral color
    /// (R/O/Y/G/B/I/V), a shorter lifetime, and single pierce.
    /// 
    /// ai[0] = spectral color index (0-6)
    /// </summary>
    public class SpectralChildBeam : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1";

        private const int TrailLength = 18;
        private const float ChildBeamWidth = 14f;

        private Player Owner => Main.player[Projectile.owner];
        private int SpectralIndex => Math.Clamp((int)Projectile.ai[0], 0, SpectralColors.Length - 1);
        private Color MyColor => SpectralColors[SpectralIndex];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Gentle homing after brief delay
            if (Projectile.timeLeft < 100)
            {
                NPC target = ClosestNPCAt(Projectile.Center, 800f);
                if (target != null)
                {
                    Vector2 desired = Projectile.SafeDirectionTo(target.Center);
                    float speed = Projectile.velocity.Length();
                    Projectile.velocity = Vector2.Lerp(Vector2.Normalize(Projectile.velocity), desired, 0.06f) * speed;
                }
            }

            // Spectral spark trail
            if (!Main.dedServ && Projectile.timeLeft % 3 == 0)
            {
                SerenadeParticleHandler.Spawn(new PrismaticSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(3, 3),
                    -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    MyColor, MoonWhite, 0.25f, 15
                ));
            }

            // Light
            Lighting.AddLight(Projectile.Center, MyColor.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicalDissonance>(), 180);

            if (!Main.dedServ)
            {
                // Small spectral bloom on hit
                SerenadeParticleHandler.Spawn(new RefractionBloomParticle(
                    target.Center, MyColor, 0.8f, 20
                ));

                for (int i = 0; i < 4; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(2f, 2f);
                    SerenadeParticleHandler.Spawn(new PrismaticSparkParticle(
                        target.Center, vel, MyColor, MoonWhite, 0.35f, 18
                    ));
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Small spectral burst on death
            for (int i = 0; i < 5; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<PrismaticDust>(),
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
                Main.dust[d].color = MyColor;
                Main.dust[d].scale = 0.8f;
                Main.dust[d].noGravity = true;
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;

        public override bool PreDraw(ref Color lightColor)
        {
            DrawChildTrail();
            DrawChildHead();
            return false;
        }

        private void DrawChildTrail()
        {
            List<Vector2> trail = new();
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) break;
                trail.Add(Projectile.oldPos[i] + Projectile.Size * 0.5f);
            }
            if (trail.Count < 3) return;

            // Use the prismatic glow shader with this child's spectral color
            MiscShaderData shader = GameShaders.Misc.TryGetValue("MagnumOpus:SerenadePrismaticGlow", out var s) ? s : null;
            if (shader != null)
            {
                shader.Shader?.Parameters["uColor"]?.SetValue(MyColor.ToVector3());
                shader.Shader?.Parameters["uSecondaryColor"]?.SetValue(MoonWhite.ToVector3());
                shader.Shader?.Parameters["uOpacity"]?.SetValue(0.8f);
                shader.Shader?.Parameters["uTime"]?.SetValue((float)Main.GameUpdateCount * 0.02f);
                shader.Shader?.Parameters["uIntensity"]?.SetValue(0.8f);
                shader.Shader?.Parameters["uPhase"]?.SetValue(0.3f);
                shader.Shader?.Parameters["uScrollSpeed"]?.SetValue(2f);
                shader.Shader?.Parameters["uOverbrightMult"]?.SetValue(1.2f);
            }

            var settings = new SerenadeTrailSettings(
                widthFunction: (t, _) => ChildBeamWidth * (1f - t * 0.6f),
                colorFunction: (t, _) => Color.Lerp(MyColor, MoonWhite, t * 0.3f) * (1f - t * 0.5f),
                smoothen: true,
                shader: shader
            );
            SerenadeTrailRenderer.RenderTrail(trail, settings);
        }

        private void DrawChildHead()
        {
            var tex = SerenadeTextures.PointBloom;
            if (tex == null) return;

            float pulse = 1f + MathF.Sin(Projectile.timeLeft * 0.2f) * 0.1f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            var origin = tex.Size() * 0.5f;

            Main.spriteBatch.Draw(tex, drawPos, null, MyColor * 0.6f, 0f, origin, 0.7f * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(tex, drawPos, null, MoonWhite * 0.5f, 0f, origin, 0.3f * pulse, SpriteEffects.None, 0f);
        }
    }
}
