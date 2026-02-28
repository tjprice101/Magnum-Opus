using System;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Buffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Primitives;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Projectiles
{
    /// <summary>
    /// Homing tidal wave projectile spawned during the Eternal Moon swing.
    /// A crescent-shaped energy bolt that homes onto nearby enemies with a gentle,
    /// lunar arc. Renders with a double-pass primitive trail (glow + core) and bloom orb.
    /// Number spawned per swing scales with lunar phase (2 at New Moon → 6 at Full Moon).
    /// </summary>
    public class EternalMoonWave : ModProjectile
    {
        public int TargetIndex = -1;
        private const int HomeDelay = 20;
        private const float MaxWidth = 24f;

        public ref float Time => ref Projectile.ai[0];
        public int LunarPhase => (int)Projectile.ai[1];

        private static Asset<Texture2D> _bloomTex;
        private static Asset<Texture2D> _noiseTex;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 25;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 255;
            Projectile.timeLeft = 300;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            // Homing after delay
            if (Time >= HomeDelay)
            {
                if (TargetIndex >= 0)
                {
                    if (!Main.npc[TargetIndex].active || !Main.npc[TargetIndex].CanBeChasedBy())
                        TargetIndex = -1;
                    else
                    {
                        Vector2 idealVel = Projectile.SafeDirectionTo(Main.npc[TargetIndex].Center) * (Projectile.velocity.Length() + 5f);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealVel, 0.06f);
                    }
                }

                if (TargetIndex == -1)
                {
                    NPC target = Projectile.Center.ClosestNPCAt(1400f);
                    if (target != null)
                        TargetIndex = target.whoAmI;
                    else
                        Projectile.velocity *= 0.99f;
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Tidal dust trail
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.PurpleTorch, Projectile.velocity * -1.5f, 0,
                    Color.Lerp(EternalMoonUtils.Violet, EternalMoonUtils.IceBlue, Main.rand.NextFloat()));
                d.scale = 0.3f;
                d.noGravity = true;
                d.fadeIn = Main.rand.NextFloat() * 0.8f;
            }

            Projectile.scale = Utils.GetLerpValue(0f, 0.15f, Projectile.timeLeft / 300f, true);

            if (Projectile.numUpdates == 0)
                Time++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.4f, PitchVariance = 0.3f }, target.Center);
            target.AddBuff(ModContent.BuffType<TidalDrowning>(), 120);

            // Crescent spark burst on hit
            if (!Main.dedServ)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 7f);
                    LunarParticleHandler.SpawnParticle(new CrescentSparkParticle(
                        target.Center, sparkVel, Main.rand.NextFloat(0.3f, 0.6f),
                        EternalMoonUtils.IceBlue, 15));
                }

                LunarParticleHandler.SpawnParticle(new LunarBloomParticle(
                    target.Center, 0.5f, EternalMoonUtils.CrescentGlow, 15, 0.04f));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.alpha > 200) return false;

            _bloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/SoftCircularCaustics");

            // Bloom circle underlayer
            Texture2D bloom = _bloomTex.Value;
            float bloomScale = 0.3f * Projectile.scale;
            Color innerColor = EternalMoonUtils.CrescentGlow;
            innerColor.A = 0;
            Main.spriteBatch.Draw(bloom, Projectile.Center - Main.screenPosition, null,
                innerColor * 0.6f, 0f, bloom.Size() / 2f, bloomScale, SpriteEffects.None, 0f);

            // Trail rendering with shader
            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);

            var shader = GameShaders.Misc["MagnumOpus:EternalMoonTidalGlow"];
            shader.UseImage1(_noiseTex);
            shader.UseColor(EternalMoonUtils.DarkPurple);
            shader.UseSecondaryColor(EternalMoonUtils.IceBlue);
            shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
            shader.Shader.Parameters["uIntensity"]?.SetValue(0.7f);
            shader.Shader.Parameters["uOpacity"]?.SetValue(0.8f);
            shader.Shader.Parameters["uOverbrightMult"]?.SetValue(2.5f);
            shader.Shader.Parameters["uScrollSpeed"]?.SetValue(1.0f);
            shader.Shader.Parameters["uDistortionAmt"]?.SetValue(0.05f);
            shader.Shader.Parameters["uHasSecondaryTex"]?.SetValue(1.0f);
            shader.Shader.Parameters["uSecondaryTexScale"]?.SetValue(2.0f);
            shader.Shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.3f);
            shader.Apply();

            LunarTrailRenderer.RenderTrail(Projectile.oldPos, new(
                TrailWidth, TrailColor,
                (_, _) => Projectile.Size * 0.5f,
                shader: shader), 20);

            Main.spriteBatch.ExitShaderRegion();

            // Bloom circle overlayer
            Color outerColor = EternalMoonUtils.IceBlue;
            outerColor.A = 0;
            Main.spriteBatch.Draw(bloom, Projectile.Center - Main.screenPosition, null,
                outerColor * 0.3f, 0f, bloom.Size() / 2f, bloomScale * 1.4f, SpriteEffects.None, 0f);

            // Moonlight
            Lighting.AddLight(Projectile.Center, EternalMoonUtils.IceBlue.ToVector3() * 0.5f * Projectile.scale);

            return false;
        }

        private float TrailWidth(float completion, Vector2 pos)
        {
            float w = MaxWidth * (1f - completion) * Projectile.scale;
            return w * (0.3f + 0.7f * Utils.GetLerpValue(0f, 0.15f, completion, true));
        }

        private Color TrailColor(float completion, Vector2 pos)
        {
            Color c = Color.Lerp(EternalMoonUtils.IceBlue, EternalMoonUtils.DarkPurple, completion * 0.7f);
            c.A = 0;
            return c * (1f - completion) * 0.8f;
        }
    }
}
