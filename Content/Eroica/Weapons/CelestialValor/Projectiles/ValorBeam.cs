using System;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Particles;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Utilities;
using MagnumOpus.Content.SandboxExoblade.Primitives;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Projectiles
{
    /// <summary>
    /// Homing valor beam — a blazing scarlet-gold energy bolt that seeks enemies.
    /// Spawned during swings (2/3/4 per phase). Each has a GPU primitive trail
    /// rendered via the ExobladePierce shader with heroic fire colors.
    /// On hit: spawns timed cross-slash bursts + applies MusicsDissonance.
    /// </summary>
    public class ValorBeam : ModProjectile
    {
        private int TargetIndex = -1;
        private const int NoHomeTime = 20;
        private const float MaxTrailWidth = 26f;

        private ref float Time => ref Projectile.ai[0];

        private static Asset<Texture2D> BloomTex;
        private static Asset<Texture2D> TrailTex;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 30;
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
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * 12;
        }

        public override void AI()
        {
            // Homing after initial delay
            if (Time >= NoHomeTime)
            {
                if (TargetIndex >= 0)
                {
                    if (!Main.npc[TargetIndex].active || !Main.npc[TargetIndex].CanBeChasedBy())
                        TargetIndex = -1;
                    else
                    {
                        Vector2 ideal = Projectile.SafeDirectionTo(Main.npc[TargetIndex].Center) * (Projectile.velocity.Length() + 5.5f);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, ideal, 0.09f);
                    }
                }

                if (TargetIndex == -1)
                {
                    NPC target = Projectile.Center.ClosestNPCAt(1400f);
                    if (target != null) TargetIndex = target.whoAmI;
                    else Projectile.velocity *= 0.99f;
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Heroic ember dust trail
            if (Main.rand.NextBool())
            {
                Color dustColor = ValorUtils.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                    DustID.GoldFlame, Projectile.velocity * -1.8f, 0, dustColor);
                d.scale = 0.35f;
                d.fadeIn = Main.rand.NextFloat() * 1f;
                d.noGravity = true;
            }

            Projectile.scale = Utils.GetLerpValue(0f, 0.12f, Projectile.timeLeft / 400f, true);

            if (Projectile.numUpdates == 0) Time++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item60 with { Volume = 0.5f, Pitch = 0.3f }, target.Center);

            // Spawn cross-slash burst
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center,
                    Projectile.velocity * 0.1f, ModContent.ProjectileType<ValorSlashCreator>(),
                    Projectile.damage, 0f, Projectile.owner, target.whoAmI,
                    Projectile.velocity.ToRotation());
            }

            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 180);

            // Impact spark particles
            for (int i = 0; i < 5; i++)
            {
                ValorSparkParticle spark = new(target.Center, Main.rand.NextVector2Circular(6f, 6f),
                    Main.rand.NextBool() ? ValorUtils.Gold : ValorUtils.Scarlet,
                    Main.rand.NextFloat(0.04f, 0.08f), 14);
                ValorParticleHandler.SpawnParticle(spark);
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White with { A = 0 } * Projectile.Opacity;

        #region ── GPU Primitive Trail ──

        private float TrailWidth(float ratio, Vector2 _)
        {
            float w = Utils.GetLerpValue(1f, 0.4f, ratio, true) *
                      (float)Math.Sin(Math.Acos(1 - Utils.GetLerpValue(0f, 0.15f, ratio, true)));
            w *= Utils.GetLerpValue(0f, 0.12f, Projectile.timeLeft / 400f, true);
            return w * MaxTrailWidth;
        }

        private Color TrailColor(float ratio, Vector2 _)
            => Color.Lerp(ValorUtils.Scarlet, ValorUtils.DeepScarlet, ratio);

        private float InnerTrailWidth(float ratio, Vector2 v) => TrailWidth(ratio, v) * 0.75f;
        private Color InnerTrailColor(float ratio, Vector2 _) => Color.White;

        #endregion

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.timeLeft > 295) return false;

            // Core bloom
            BloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Content/SandboxExoblade/Textures/BloomCircle");
            Texture2D bloom = BloomTex.Value;
            Vector2 corePos = Projectile.oldPos[2] + Projectile.Size / 2f - Main.screenPosition;

            Color mainColor = ValorUtils.MulticolorLerp(
                (Main.GlobalTimeWrappedHourly * 1.5f + Projectile.whoAmI * 0.15f) % 1,
                ValorUtils.Scarlet, ValorUtils.Flame, ValorUtils.Gold, ValorUtils.HeroicBlaze);

            Main.EntitySpriteDraw(bloom, corePos, null, (mainColor * 0.12f) with { A = 0 }, 0,
                bloom.Size() / 2f, 1.1f * Projectile.scale, 0, 0);
            Main.EntitySpriteDraw(bloom, corePos, null, (mainColor * 0.5f) with { A = 0 }, 0,
                bloom.Size() / 2f, 0.3f * Projectile.scale, 0, 0);

            // GPU trail
            Main.spriteBatch.EnterShaderRegion();

            TrailTex ??= ModContent.Request<Texture2D>("MagnumOpus/Content/SandboxExoblade/Textures/BasicTrail");
            Color secColor = ValorUtils.MulticolorLerp(
                (Main.GlobalTimeWrappedHourly * 1.5f + Projectile.whoAmI * 0.15f + 0.2f) % 1,
                ValorUtils.Scarlet, ValorUtils.Flame, ValorUtils.Gold, ValorUtils.HeroicBlaze);

            GameShaders.Misc["MagnumOpus:ExobladePierce"].UseImage1(TrailTex);
            GameShaders.Misc["MagnumOpus:ExobladePierce"].UseImage2("Images/Extra_189");
            GameShaders.Misc["MagnumOpus:ExobladePierce"].UseColor(mainColor);
            GameShaders.Misc["MagnumOpus:ExobladePierce"].UseSecondaryColor(secColor);
            GameShaders.Misc["MagnumOpus:ExobladePierce"].Apply();

            Vector2 offset = Projectile.Size * 0.5f;
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(TrailWidth, TrailColor,
                (_, _) => offset, shader: GameShaders.Misc["MagnumOpus:ExobladePierce"]), 30);

            // Inner white-hot core trail
            GameShaders.Misc["MagnumOpus:ExobladePierce"].UseColor(Color.White);
            GameShaders.Misc["MagnumOpus:ExobladePierce"].UseSecondaryColor(Color.White);
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(InnerTrailWidth, InnerTrailColor,
                (_, _) => offset, shader: GameShaders.Misc["MagnumOpus:ExobladePierce"]), 30);

            Main.spriteBatch.ExitShaderRegion();

            // White bloom highlight
            Main.EntitySpriteDraw(bloom, corePos, null, (Color.White * 0.2f) with { A = 0 }, 0,
                bloom.Size() / 2f, 0.6f * Projectile.scale, 0, 0);

            return false;
        }
    }
}
