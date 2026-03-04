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

            // Tidal mote trail particles (replaces vanilla dust for richer look)
            if (!Main.dedServ && Main.rand.NextBool(2))
            {
                Vector2 moteVel = -Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f);
                Color moteColor = Color.Lerp(EternalMoonUtils.Violet, EternalMoonUtils.IceBlue, Main.rand.NextFloat());
                LunarParticleHandler.SpawnParticle(new TidalMoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), moteVel,
                    Main.rand.NextFloat(0.2f, 0.4f), moteColor, Main.rand.Next(15, 30)));
            }

            // Tidal droplets falling from the wave projectile
            if (!Main.dedServ && Main.rand.NextBool(5))
            {
                Vector2 dropVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(0.2f, 0.8f));
                LunarParticleHandler.SpawnParticle(new TidalDropletParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), dropVel,
                    Main.rand.NextFloat(0.15f, 0.3f),
                    EternalMoonUtils.IceBlue * 0.5f, Main.rand.Next(15, 25)));
            }

            Projectile.scale = Utils.GetLerpValue(0f, 0.15f, Projectile.timeLeft / 300f, true);

            if (Projectile.numUpdates == 0)
                Time++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.4f, PitchVariance = 0.3f }, target.Center);
            target.AddBuff(ModContent.BuffType<TidalDrowning>(), 120);

            // Crescent spark burst + wave spray on hit
            if (!Main.dedServ)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 7f);
                    Color sparkColor = Color.Lerp(EternalMoonUtils.IceBlue, EternalMoonUtils.CrescentGlow, Main.rand.NextFloat());
                    LunarParticleHandler.SpawnParticle(new CrescentSparkParticle(
                        target.Center, sparkVel, Main.rand.NextFloat(0.3f, 0.6f),
                        sparkColor, 15));
                }

                // Wave spray burst on impact
                for (int i = 0; i < 4; i++)
                {
                    Vector2 sprayVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 5f);
                    LunarParticleHandler.SpawnParticle(new WaveSprayParticle(
                        target.Center, sprayVel, Main.rand.NextFloat(0.2f, 0.4f),
                        EternalMoonUtils.MoonWhite, Main.rand.Next(10, 18)));
                }

                // Moon glint at impact
                LunarParticleHandler.SpawnParticle(new MoonGlintParticle(
                    target.Center, Main.rand.NextFloat(0.2f, 0.4f),
                    EternalMoonUtils.MoonWhite, 12));

                LunarParticleHandler.SpawnParticle(new LunarBloomParticle(
                    target.Center, 0.5f, EternalMoonUtils.CrescentGlow, 15, 0.04f));
            }

            // === FOUNDATION VFX: Ripple + ThinSlash on wave impact ===
            if (Main.myPlayer == Projectile.owner)
            {
                // TidalRippleEffect — expanding concentric rings at wave impact
                Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                    target.Center, Vector2.Zero,
                    ModContent.ProjectileType<TidalRippleEffect>(),
                    0, 0f, Projectile.owner, 0.7f); // Moderate tidal phase for wave hits

                // TidalThinSlash — directional slash mark along wave travel direction
                float waveAngle = Projectile.velocity.ToRotation();
                Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                    target.Center, Vector2.Zero,
                    ModContent.ProjectileType<TidalThinSlash>(),
                    0, 0f, Projectile.owner, waveAngle, 0); // Style 0: Ice Cyan for wave impacts
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
            // Scale trail width with tidal phase for more dramatic waves
            float tidalMult = 1f;
            if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers && Main.player[Projectile.owner].active)
                tidalMult = Main.player[Projectile.owner].EternalMoon().TidalPhaseMultiplier;
            float w = MaxWidth * tidalMult * (1f - completion) * Projectile.scale;
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
