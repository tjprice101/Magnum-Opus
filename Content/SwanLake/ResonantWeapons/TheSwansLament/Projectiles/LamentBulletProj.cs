using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Particles;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Primitives;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Shaders;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Utilities;
using ReLogic.Content;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Projectiles
{
    /// <summary>
    /// Individual bullet fired by The Swan's Lament.
    /// Visually dark with intermittent prismatic flashes — grief punctuated by revelation.
    /// Shader-driven trail via LamentPrimitiveRenderer. On enemy kill, spawns
    /// a DestructionHaloProj and registers the kill for Lament's Echo.
    /// </summary>
    public class LamentBulletProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft";

        // Trail history
        private List<Vector2> _trailPositions = new List<Vector2>();
        private const int MaxTrailPoints = 18;
        private LamentPrimitiveRenderer _trailRenderer;

        // Prismatic flash timing — random per bullet for visual variety
        private float _flashOffset;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 2;
            Projectile.alpha = 0;
            Projectile.ignoreWater = true;
            _flashOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Record trail
            _trailPositions.Insert(0, Projectile.Center);
            if (_trailPositions.Count > MaxTrailPoints)
                _trailPositions.RemoveAt(_trailPositions.Count - 1);

            // Grief smoke — intermittent dark wisps
            if (Main.rand.NextBool(4))
            {
                Vector2 smokeVel = Projectile.velocity.RotatedByRandom(0.3f) * -0.15f;
                LamentParticleHandler.Spawn(new GriefSmoke(),
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    smokeVel, LamentUtils.MourningBlack, Main.rand.NextFloat(0.6f, 1.0f), 30);
            }

            // Prismatic flash — rare, brief, intense
            float flashChance = LamentUtils.GetGriefFlashIntensity(
                (float)(Main.GameUpdateCount * 0.02f + _flashOffset));
            if (flashChance > 0.7f && Main.rand.NextBool(3))
            {
                LamentParticleHandler.Spawn(new PrismaticFlashParticle(),
                    Projectile.Center, Vector2.Zero,
                    LamentUtils.CatharsisWhite, Main.rand.NextFloat(0.3f, 0.6f), 8);
            }

            // Embers
            if (Main.rand.NextBool(6))
            {
                Vector2 emberVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f)
                    + Main.rand.NextVector2Circular(1f, 1f);
                LamentParticleHandler.Spawn(new LamentEmberParticle(),
                    Projectile.Center, emberVel,
                    LamentUtils.GriefGrey, Main.rand.NextFloat(0.2f, 0.4f), 20);
            }

            Lighting.AddLight(Projectile.Center, LamentUtils.GriefGrey.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 300); // 5 seconds

            // Music note on hit — grief punctuated by melody
            SwanLakeVFXLibrary.SpawnMusicNotes(target.Center, 1, 12f, 0.5f, 0.7f, 20);
        }

        public override void OnKill(int timeLeft)
        {
            // Impact burst
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f, Volume = 0.5f }, Projectile.Center);

            // Dark smear + embers burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f);
                LamentParticleHandler.Spawn(new GriefSmoke(),
                    Projectile.Center, vel, LamentUtils.MourningBlack,
                    Main.rand.NextFloat(0.8f, 1.4f), 25);
            }
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(2f, 2f);
                LamentParticleHandler.Spawn(new LamentEmberParticle(),
                    Projectile.Center, vel, LamentUtils.GriefGrey,
                    Main.rand.NextFloat(0.2f, 0.5f), 18);
            }

            // If the bullet killed an enemy (check via GlobalNPC or flag),
            // spawn a Destruction Halo.
            // The kill registration happens in LamentGlobalNPC on enemy death.

            // Music notes burst from destruction — revelation through grief
            SwanLakeVFXLibrary.SpawnMusicNotes(Projectile.Center, 2, 15f, 0.6f, 0.9f, 25);
            SwanLakeVFXLibrary.SpawnFeatherDrift(Projectile.Center, 2, 12f, 0.2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var spriteBatch = Main.spriteBatch;

            // === LAYER 1: Shader-driven trail ===
            if (_trailPositions.Count >= 2)
            {
                _trailRenderer ??= new LamentPrimitiveRenderer(Main.graphics.GraphicsDevice);

                var settings = new LamentTrailSettings
                {
                    WidthFunction = p => MathHelper.Lerp(10f, 2f, p) * (1f - p * 0.4f),
                    ColorFunction = p =>
                    {
                        float flash = LamentUtils.GetGriefFlashIntensity(p + _flashOffset);
                        Color baseCol = Color.Lerp(LamentUtils.GriefGrey, LamentUtils.MourningBlack, p);
                        Color flashCol = Color.Lerp(baseCol, LamentUtils.RevelationGold, flash * 0.4f);
                        return flashCol * (1f - p);
                    },
                    ShaderKey = LamentShaderLoader.HasLamentBulletTrailShader ?
                        "MagnumOpus:LamentBulletTrail" : null,
                    TrailLength = 0.9f
                };

                LamentUtils.BeginAdditive(spriteBatch);
                _trailRenderer.DrawTrail(_trailPositions, settings);
                LamentUtils.RestoreSpriteBatch(spriteBatch);
            }

            // === LAYER 2: Core glow (4-layer VFX Asset Library bloom) ===
            Texture2D softRadial = null;
            Texture2D pointBloom = null;
            Texture2D starAccent = null;
            try
            {
                softRadial = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom",
                    AssetRequestMode.ImmediateLoad)?.Value;
                pointBloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                    AssetRequestMode.ImmediateLoad)?.Value;
                starAccent = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard",
                    AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }

            float corePulse = 0.85f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.15f + _flashOffset);
            Color coreColor = Color.Lerp(LamentUtils.GriefGrey, LamentUtils.CatharsisWhite,
                LamentUtils.GetGriefFlashIntensity((float)(Main.GameUpdateCount * 0.03f + _flashOffset)));
            Vector2 corePos = Projectile.Center - Main.screenPosition;

            // Layer 2a: Outer grief halo (SoftRadialBloom)
            if (softRadial != null)
            {
                var srOrigin = new Vector2(softRadial.Width, softRadial.Height) * 0.5f;
                Color outerGlow = Color.Lerp(LamentUtils.MourningBlack, LamentUtils.GriefGrey, 0.3f);
                spriteBatch.Draw(softRadial, corePos, null,
                    new Color(outerGlow.R, outerGlow.G, outerGlow.B, 0) * 0.25f * corePulse, 0f, srOrigin, 0.35f * corePulse,
                    SpriteEffects.None, 0f);

                // Layer 2b: Mid revelation glow
                spriteBatch.Draw(softRadial, corePos, null,
                    new Color(coreColor.R, coreColor.G, coreColor.B, 0) * 0.4f * corePulse, 0f, srOrigin, 0.2f * corePulse,
                    SpriteEffects.None, 0f);
            }

            // Layer 2c: Focused core (PointBloom)
            if (pointBloom != null)
            {
                var pbOrigin = new Vector2(pointBloom.Width, pointBloom.Height) * 0.5f;
                spriteBatch.Draw(pointBloom, corePos, null,
                    new Color(coreColor.R, coreColor.G, coreColor.B, 0) * 0.65f * corePulse, 0f, pbOrigin, 0.12f * corePulse,
                    SpriteEffects.None, 0f);
            }

            // Layer 2d: Revelation star flash during prismatic moments
            float flashVal = LamentUtils.GetGriefFlashIntensity((float)(Main.GameUpdateCount * 0.03f + _flashOffset));
            if (starAccent != null && flashVal > 0.4f)
            {
                var starOrigin = new Vector2(starAccent.Width, starAccent.Height) * 0.5f;
                float starRot = Main.GameUpdateCount * 0.1f + _flashOffset;
                spriteBatch.Draw(starAccent, corePos, null,
                    new Color(LamentUtils.RevelationGold.R, LamentUtils.RevelationGold.G, LamentUtils.RevelationGold.B, 0) * (flashVal - 0.4f) * 0.5f,
                    starRot, starOrigin, 0.18f * corePulse, SpriteEffects.None, 0f);
            }

            // === LAYER 3: Actual bullet sprite ===
            var tex = ModContent.Request<Texture2D>(Texture,
                ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            var origin = new Vector2(tex.Width, tex.Height) * 0.5f;
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null,
                LamentUtils.GriefGrey * 0.9f, Projectile.rotation, origin, 0.4f,
                SpriteEffects.None, 0f);

            return false;
        }
    }
}
