using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Particles;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Shaders;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Utilities;
using ReLogic.Content;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Projectiles
{
    /// <summary>
    /// Destruction Halo — spawned on enemy death by LamentGlobalNPC.
    /// An expanding ring of prismatic revelation that deals AoE damage.
    /// Mostly dark, with sudden flashes of white-gold as the ring expands.
    /// </summary>
    public class DestructionHaloProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/HardCircleMask";

        private float _currentRadius;
        private const float MaxRadius = 140f;
        private const int ExpandDuration = 35;

        // The halo hits each NPC only once
        private bool[] _hitNPC;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = ExpandDuration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // hit each NPC once
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (_hitNPC == null)
                _hitNPC = new bool[Main.maxNPCs];

            float progress = 1f - ((float)Projectile.timeLeft / ExpandDuration);
            _currentRadius = MathHelper.Lerp(0f, MaxRadius, EaseOutQuart(progress));

            // Expand the hitbox to match the ring radius
            int diameter = (int)(_currentRadius * 2f);
            Projectile.position = Projectile.Center - new Vector2(diameter / 2f);
            Projectile.width = diameter;
            Projectile.height = diameter;

            // VFX: concentric destruction rings
            if (Main.GameUpdateCount % 3 == 0)
            {
                LamentParticleHandler.Spawn(new DestructionRingParticle(),
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Vector2.Zero,
                    Color.Lerp(LamentUtils.GriefGrey, LamentUtils.RevelationGold, progress),
                    _currentRadius / MaxRadius * 1.5f, 20);
            }

            // Prismatic flash sparks around the ring edge
            if (Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 ringEdge = Projectile.Center + angle.ToRotationVector2() * _currentRadius;
                float flashVal = LamentUtils.GetGriefFlashIntensity(progress);

                if (flashVal > 0.3f)
                {
                    LamentParticleHandler.Spawn(new PrismaticFlashParticle(),
                        ringEdge, Main.rand.NextVector2Circular(0.5f, 0.5f),
                        LamentUtils.CatharsisWhite,
                        Main.rand.NextFloat(0.3f, 0.7f), 10);
                }
                else
                {
                    LamentParticleHandler.Spawn(new LamentEmberParticle(),
                        ringEdge, angle.ToRotationVector2() * Main.rand.NextFloat(0.5f, 2f),
                        LamentUtils.MourningBlack,
                        Main.rand.NextFloat(0.2f, 0.4f), 15);
                }
            }

            // Dynamic lighting
            float lightPulse = 0.3f + 0.5f * LamentUtils.GetGriefFlashIntensity(progress);
            Lighting.AddLight(Projectile.Center,
                LamentUtils.CatharsisWhite.ToVector3() * lightPulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 480); // 8 seconds on halo hits
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Ring-shaped collision: only hits at the ring's edge, not the center
            float dist = Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2());
            float ringThickness = 30f;
            return dist >= (_currentRadius - ringThickness) && dist <= (_currentRadius + ringThickness);
        }

        public override void OnKill(int timeLeft)
        {
            // Final revelation flash
            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.4f, Volume = 0.6f }, Projectile.Center);

            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f);
                LamentParticleHandler.Spawn(new PrismaticFlashParticle(),
                    Projectile.Center + vel * Main.rand.NextFloat(MaxRadius * 0.5f, MaxRadius),
                    vel * 0.5f, LamentUtils.RevelationGold,
                    Main.rand.NextFloat(0.4f, 0.8f), 12);
            }

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * MaxRadius;
                LamentParticleHandler.Spawn(new GriefSmoke(),
                    pos, angle.ToRotationVector2() * 2f,
                    LamentUtils.MourningBlack, 1.2f, 35);
            }

            // Revelation music note cascade — the swan's final song
            SwanLakeVFXLibrary.SpawnMusicNotes(Projectile.Center, 5, MaxRadius * 0.6f, 0.7f, 1.1f, 30);

            // Feather scatter from the destruction ring edge
            SwanLakeVFXLibrary.SpawnFeatherBurst(Projectile.Center, 4, 0.3f);

            // Prismatic sparkle cascade
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(Projectile.Center, 6, MaxRadius * 0.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var spriteBatch = Main.spriteBatch;
            float progress = 1f - ((float)Projectile.timeLeft / ExpandDuration);

            var tex = ModContent.Request<Texture2D>(Texture,
                ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            var origin = new Vector2(tex.Width, tex.Height) * 0.5f;

            float opacity = progress < 0.3f ? progress / 0.3f : (1f - progress) / 0.7f;
            opacity = MathHelper.Clamp(opacity, 0f, 1f);

            float flashIntensity = LamentUtils.GetGriefFlashIntensity(progress);

            // === LAYER 1: Bloom backdrop (additive) ===
            Texture2D softRadial = null;
            try
            {
                softRadial = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom",
                    AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }

            LamentUtils.BeginAdditive(spriteBatch);

            // Bloom glow backdrop behind ring
            if (softRadial != null)
            {
                var srOrigin = new Vector2(softRadial.Width, softRadial.Height) * 0.5f;
                float bloomScale = (_currentRadius * 2.5f) / softRadial.Width;
                Color bloomColor = Color.Lerp(LamentUtils.GriefGrey, LamentUtils.RevelationGold, flashIntensity);
                spriteBatch.Draw(softRadial, Projectile.Center - Main.screenPosition, null,
                    new Color(bloomColor.R, bloomColor.G, bloomColor.B, 0) * opacity * 0.2f, 0f, srOrigin, bloomScale,
                    SpriteEffects.None, 0f);
            }

            // === LAYER 2: Outer ring glow ===

            // Outer halo
            float outerScale = (_currentRadius * 2f) / tex.Width;
            Color outerColor = Color.Lerp(LamentUtils.GriefGrey, LamentUtils.RevelationGold, flashIntensity);
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null,
                outerColor * opacity * 0.4f, 0f, origin, outerScale,
                SpriteEffects.None, 0f);

            // Inner hollow (slightly smaller, darker to create ring effect)
            float innerScale = outerScale * 0.82f;
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null,
                LamentUtils.MourningBlack * opacity * 0.6f, 0f, origin, innerScale,
                SpriteEffects.None, 0f);

            // Flash overlay during prismatic moments
            if (flashIntensity > 0.4f)
            {
                spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null,
                    LamentUtils.CatharsisWhite * (flashIntensity - 0.4f) * opacity * 0.5f,
                    0f, origin, outerScale * 1.05f, SpriteEffects.None, 0f);
            }

            LamentUtils.RestoreSpriteBatch(spriteBatch);

            return false;
        }

        private static float EaseOutQuart(float t)
        {
            t = 1f - t;
            return 1f - t * t * t * t;
        }
    }
}
