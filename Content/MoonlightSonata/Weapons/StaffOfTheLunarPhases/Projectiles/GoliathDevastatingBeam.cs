using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Utilities;
using MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Primitives;
using MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Dusts;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Buffs;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Projectiles
{
    /// <summary>
    /// Goliath Devastating Beam — enhanced beam fired during Conductor Mode.
    /// Pierces through all enemies in its path, deals bonus damage,
    /// heals 15 HP per hit, inflicts Musical Dissonance, and creates heavy VFX.
    /// Fires every 3rd beam when Conductor Mode is active.
    /// </summary>
    public class GoliathDevastatingBeam : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1";

        // =================================================================
        // CONSTANTS
        // =================================================================

        public const float BeamSpeed = 18f;
        public const float BeamWidth = 18f;
        public const int TrailLength = 25;
        public const int HealAmount = 15;
        public const float DamageMultiplier = 1.5f;

        // =================================================================
        // AI FIELDS
        // =================================================================

        /// <summary>ai[0] = unused.</summary>
        public ref float Reserved0 => ref Projectile.ai[0];

        /// <summary>ai[1] = unused.</summary>
        public ref float Reserved1 => ref Projectile.ai[1];

        /// <summary>localAI[0] = alive time counter.</summary>
        public ref float AliveTime => ref Projectile.localAI[0];

        /// <summary>localAI[1] = beam intensity pulse (oscillates for visual drama).</summary>
        public ref float IntensityPulse => ref Projectile.localAI[1];

        // =================================================================
        // SETUP
        // =================================================================

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
            ProjectileID.Sets.MinionShot[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1; // pierces all
            Projectile.timeLeft = 240;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0.8f;
        }

        public override bool? CanCutTiles() => false;

        // =================================================================
        // DAMAGE BOOST
        // =================================================================

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.SourceDamage += DamageMultiplier - 1f;
        }

        // =================================================================
        // AI
        // =================================================================

        public override void AI()
        {
            AliveTime++;
            IntensityPulse = 0.7f + 0.3f * MathF.Sin(AliveTime * 0.1f);

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Lighting — brighter than normal beam
            Color lightCol = GoliathUtils.GetCosmicGradient(0.7f + IntensityPulse * 0.2f);
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * 0.6f);

            // Heavy flight particles
            SpawnFlightParticles();
        }

        private void SpawnFlightParticles()
        {
            if (Main.dedServ) return;

            // Beam sparks — every tick (denser than normal beam)
            if (AliveTime % 1 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(8f, 8f);
                Vector2 sparkVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1.5f, 3f)
                    + Main.rand.NextVector2Circular(1.2f, 1.2f);
                GoliathParticleHandler.Spawn(new BeamSparkParticle(
                    Projectile.Center + offset, sparkVel,
                    0.4f + IntensityPulse * 0.3f, 15 + Main.rand.Next(10)));
            }

            // Cosmic dust — every 2 ticks
            if (AliveTime % 2 == 0)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<GoliathDust>(), -Projectile.velocity.X * 0.1f, -Projectile.velocity.Y * 0.1f);
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 1.0f + IntensityPulse * 0.5f;
            }

            // Conductor glyphs — every 8 ticks
            if (AliveTime % 8 == 0)
            {
                Vector2 glyphVel = Main.rand.NextVector2Circular(1f, 1f);
                GoliathParticleHandler.Spawn(new ConductorGlyphParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), glyphVel,
                    0.5f, 30 + Main.rand.Next(15)));
            }

            // Music notes trailing — every 6 ticks
            if (AliveTime % 6 == 0)
            {
                Vector2 noteVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * 0.5f
                    + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, 0f));
                Color noteColor = GoliathUtils.GetCastGradient(Main.rand.NextFloat(0.3f, 0.8f));
                GoliathParticleHandler.Spawn(new MusicNoteParticle(
                    Projectile.Center, noteVel, noteColor,
                    0.5f + Main.rand.NextFloat(0.3f), 50 + Main.rand.Next(20)));
            }
        }

        // =================================================================
        // ON HIT — HEAL + DEBUFF + HEAVY VFX
        // =================================================================

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Heal owner 15 HP (devastating beam heals more)
            if (Projectile.owner == Main.myPlayer)
            {
                Player owner = Main.player[Projectile.owner];
                owner.Heal(HealAmount);
            }

            // Inflict Musical Dissonance
            target.AddBuff(ModContent.BuffType<MusicalDissonance>(), 480);

            // Heavy impact VFX
            SpawnDevastatingImpactVFX(target.Center);

            // Screen shake on hit
            if (Projectile.owner == Main.myPlayer)
            {
                try
                {
                    var shaker = Main.LocalPlayer.GetModPlayer<ScreenShakePlayer>();
                    shaker.AddShake(3f, 8);
                }
                catch { }
            }
        }

        private void SpawnDevastatingImpactVFX(Vector2 impactPos)
        {
            if (Main.dedServ) return;

            // Foundation VFX: GoliathRipple at devastating impact (enhanced intensity)
            if (Projectile.owner == Main.myPlayer)
            {
                Player owner = Main.player[Projectile.owner];
                GoliathPlayer gp = owner.GetModPlayer<GoliathPlayer>();
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), impactPos, Vector2.Zero,
                    ModContent.ProjectileType<GoliathRipple>(),
                    0, 0f, Projectile.owner,
                    ai0: gp.LunarPhaseMode, ai1: 1f); // ai0=phase, ai1=1 (devastating)
            }

            // Large impact bloom
            GoliathParticleHandler.Spawn(new ImpactBloomParticle(
                impactPos, GoliathUtils.SupermoonWhite, 1.2f, 20));

            // Secondary bloom — nebula
            GoliathParticleHandler.Spawn(new ImpactBloomParticle(
                impactPos, GoliathUtils.NebulaPurple * 0.6f, 1.8f, 25));

            // Radial spark burst — larger
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * (4f + Main.rand.NextFloat(3f));
                GoliathParticleHandler.Spawn(new BeamSparkParticle(
                    impactPos, vel, 0.4f + Main.rand.NextFloat(0.2f), 18 + Main.rand.Next(10)));
            }

            // Music notes cascade
            for (int i = 0; i < 5; i++)
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-4f, -1f));
                Color noteColor = GoliathUtils.GetCastGradient(Main.rand.NextFloat(0.3f, 1f));
                GoliathParticleHandler.Spawn(new MusicNoteParticle(
                    impactPos + Main.rand.NextVector2Circular(15f, 15f), noteVel,
                    noteColor, 0.5f + Main.rand.NextFloat(0.4f), 50 + Main.rand.Next(25)));
            }

            // Conductor glyphs burst
            for (int i = 0; i < 3; i++)
            {
                Vector2 glyphVel = Main.rand.NextVector2CircularEdge(2f, 2f);
                GoliathParticleHandler.Spawn(new ConductorGlyphParticle(
                    impactPos, glyphVel, 0.6f + Main.rand.NextFloat(0.3f), 35 + Main.rand.Next(15)));
            }

            // Heavy dust burst
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                int d = Dust.NewDust(impactPos - new Vector2(4), 8, 8,
                    ModContent.DustType<GoliathDust>(), dustVel.X, dustVel.Y);
                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 1.4f;
            }

            // Impact sound — deeper, more resonant
            SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.6f, Pitch = -0.3f }, impactPos);
        }

        // =================================================================
        // RENDERING
        // =================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            SpriteBatch sb = Main.spriteBatch;
            try
            {

            // End the active SpriteBatch before GPU primitive drawing
            Main.spriteBatch.End();
            try
            {
                DrawGlowTrail();
                DrawMainTrail();
            }
            finally
            {
                // Restore SpriteBatch to Terraria's expected state
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            DrawHeadGlow();

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

        private void DrawGlowTrail()
        {
            MiscShaderData glowShader = null;
            if (GameShaders.Misc.TryGetValue("MagnumOpus:GoliathDevastatingGlow", out var shader))
            {
                glowShader = shader;
                glowShader.UseColor(GoliathUtils.GetCastGradient(0.6f));
                glowShader.UseSecondaryColor(GoliathUtils.CosmicVoid);
                glowShader.UseOpacity(0.4f + IntensityPulse * 0.3f);
                glowShader.UseSaturation(IntensityPulse);
            }

            GoliathTrailRenderer.RenderTrail(Projectile.oldPos, new GoliathTrailSettings(
                widthFunction: (completion, _) =>
                {
                    float taper = 1f - completion;
                    return (BeamWidth * 3f + IntensityPulse * 20f) * taper * GoliathUtils.SineOut(1f - completion);
                },
                colorFunction: (completion, _) =>
                {
                    Color col = GoliathUtils.GetCastGradient(completion * 0.5f + 0.3f);
                    return col * (0.35f + IntensityPulse * 0.2f) * (1f - completion * 0.4f);
                },
                offsetFunction: (_, _) => Projectile.Size * 0.5f,
                smoothen: true,
                shader: glowShader
            ), TrailLength);
        }

        private void DrawMainTrail()
        {
            MiscShaderData mainShader = null;
            if (GameShaders.Misc.TryGetValue("MagnumOpus:GoliathDevastatingMain", out var shader))
            {
                mainShader = shader;
                mainShader.UseColor(GoliathUtils.SupermoonWhite);
                mainShader.UseSecondaryColor(GoliathUtils.FirstQuarterViolet);
                mainShader.UseOpacity(0.8f + IntensityPulse * 0.2f);
                mainShader.UseSaturation(IntensityPulse);
            }

            GoliathTrailRenderer.RenderTrail(Projectile.oldPos, new GoliathTrailSettings(
                widthFunction: (completion, _) =>
                {
                    float taper = 1f - completion;
                    return (BeamWidth * 1.5f + IntensityPulse * 10f) * taper * GoliathUtils.SineOut(1f - completion);
                },
                colorFunction: (completion, _) =>
                {
                    Color col = GoliathUtils.GetCastGradient(0.5f + completion * 0.4f);
                    return col * (0.85f + IntensityPulse * 0.15f) * (1f - completion * 0.2f);
                },
                offsetFunction: (_, _) => Projectile.Size * 0.5f,
                smoothen: true,
                shader: mainShader
            ), TrailLength);
        }

        private void DrawHeadGlow()
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D bloom = GoliathTextures.SoftRadialBloom;
            Vector2 origin = bloom.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Switch to Additive for bloom glow layers
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer glow — cast palette (reduced further for clarity)
            Color outerColor = GoliathUtils.WaxingGibbous with { A = 0 };
            float outerScale = 0.07f + IntensityPulse * 0.03f;
            sb.Draw(bloom, drawPos, null, outerColor * (0.22f + IntensityPulse * 0.1f), 0f, origin, outerScale, SpriteEffects.None, 0f);

            // Middle glow — ice blue
            Color midColor = GoliathUtils.FullMoonIceBlue with { A = 0 };
            float midScale = 0.04f + IntensityPulse * 0.02f;
            sb.Draw(bloom, drawPos, null, midColor * (0.22f + IntensityPulse * 0.1f), 0f, origin, midScale, SpriteEffects.None, 0f);

            // Inner core — supermoon white
            Color coreColor = GoliathUtils.SupermoonWhite with { A = 0 };
            float coreScale = 0.025f + IntensityPulse * 0.01f;
            sb.Draw(bloom, drawPos, null, coreColor * (0.35f + IntensityPulse * 0.15f), 0f, origin, coreScale, SpriteEffects.None, 0f);

            // Restore to AlphaBlend
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        // =================================================================
        // DEATH VFX
        // =================================================================

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Grand dissipation bloom
            GoliathParticleHandler.Spawn(new ImpactBloomParticle(
                Projectile.Center, GoliathUtils.SupermoonWhite, 1.0f, 25));
            GoliathParticleHandler.Spawn(new ImpactBloomParticle(
                Projectile.Center, GoliathUtils.NebulaPurple * 0.5f, 1.5f, 30));

            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f);
                GoliathParticleHandler.Spawn(new BeamSparkParticle(
                    Projectile.Center, vel, 0.35f, 18 + Main.rand.Next(10)));
            }

            // Final music note cascade
            for (int i = 0; i < 4; i++)
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -0.5f));
                GoliathParticleHandler.Spawn(new MusicNoteParticle(
                    Projectile.Center, noteVel, GoliathUtils.GetCastGradient(Main.rand.NextFloat()),
                    0.4f + Main.rand.NextFloat(0.3f), 45 + Main.rand.Next(20)));
            }

            SoundEngine.PlaySound(SoundID.Item60 with { Volume = 0.3f, Pitch = 0.4f }, Projectile.Center);
        }
    }
}
