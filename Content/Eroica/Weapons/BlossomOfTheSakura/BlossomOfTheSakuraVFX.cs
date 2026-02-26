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
using MagnumOpus.Common.Systems.VFX.Screen;

namespace MagnumOpus.Content.Eroica.Weapons.BlossomOfTheSakura
{
    /// <summary>
    /// Static VFX helper for BlossomOfTheSakura — sakura-themed assault rifle that blooms with heat.
    /// Cool start with gentle petals, increasingly fiery with sustained fire.
    /// Game logic stays in item/projectile files; all VFX routed through EroicaVFXLibrary.
    /// </summary>
    public static class BlossomOfTheSakuraVFX
    {
        // === UNIQUE IDENTITY COLORS — heat-reactive assault rifle palette ===
        private static readonly Color CoolBarrel = new Color(180, 140, 160);
        private static readonly Color WarmBlush = new Color(220, 100, 120);
        private static readonly Color HotCrimson = new Color(240, 60, 50);
        private static readonly Color WhiteHotMuzzle = new Color(255, 245, 230);
        private static readonly Color MuzzleFlash = new Color(255, 200, 150);
        private static readonly Color CasingBrass = new Color(200, 170, 80);
        private static readonly Color TracerPink = new Color(255, 160, 180);
        private static readonly Color GunsmokePurple = new Color(120, 100, 130);

        // ══════════════════════════════════════════════════════════════
        //  1. HEAT-REACTIVE BARREL VFX  (called from HoldItem)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Progressive barrel heat: shimmer, glow, embers, smoke, lighting. 0=cold, 1=max.</summary>
        public static void BarrelHeatVFX(Vector2 gunBarrel, Vector2 gunBody, float heatProgress, int playerDirection)
        {
            if (heatProgress < 0.1f)
                return;

            // Shimmer particles rising from barrel (GenericGlowParticle heat haze)
            if (heatProgress > 0.2f && Main.rand.NextBool((int)(8 - heatProgress * 5)))
            {
                float shimmerT = (heatProgress - 0.2f) / 0.8f;
                Vector2 shimmerPos = gunBody + new Vector2(Main.rand.NextFloat(-15f, 25f) * playerDirection, 0f);
                Vector2 shimmerVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1f, 2.5f));
                Color shimmerColor = Color.Lerp(CoolBarrel * 0.3f, WarmBlush * 0.5f, shimmerT);
                MagnumParticleHandler.SpawnParticle(
                    new GenericGlowParticle(shimmerPos, shimmerVel, shimmerColor, 0.15f + shimmerT * 0.1f, 20, true));
            }

            // Barrel glow dust — Sakura -> Crimson -> Gold -> White-hot progression
            Color barrelColor;
            if (heatProgress < 0.4f)
                barrelColor = Color.Lerp(EroicaPalette.Sakura * 0.5f, HotCrimson, (heatProgress - 0.1f) / 0.3f);
            else if (heatProgress < 0.7f)
                barrelColor = Color.Lerp(HotCrimson, EroicaPalette.Gold, (heatProgress - 0.4f) / 0.3f);
            else
                barrelColor = Color.Lerp(EroicaPalette.Gold, WhiteHotMuzzle, (heatProgress - 0.7f) / 0.3f);

            float glowScale = 0.2f + heatProgress * 0.4f;
            Dust d = Dust.NewDustPerfect(gunBarrel, DustID.GoldFlame, Vector2.Zero, 0, barrelColor, glowScale * 3f);
            d.noGravity = true;

            if (heatProgress > 0.5f)
            {
                Vector2 midBarrel = Vector2.Lerp(gunBody, gunBarrel, 0.5f);
                Dust d2 = Dust.NewDustPerfect(midBarrel, DustID.GoldFlame, Vector2.Zero, 0, barrelColor * 0.7f, glowScale * 2f);
                d2.noGravity = true;
            }

            // Ember sparks at high heat (>0.6)
            if (heatProgress > 0.6f && Main.rand.NextBool((int)(10 - heatProgress * 6)))
            {
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f));
                Color sparkColor = Color.Lerp(EroicaPalette.Gold, WhiteHotMuzzle, Main.rand.NextFloat(0.3f));
                MagnumParticleHandler.SpawnParticle(
                    new GenericGlowParticle(gunBarrel + Main.rand.NextVector2Circular(5f, 5f), sparkVel, sparkColor, 0.12f, 15, true));
            }

            // Smoke wisps at extreme heat (>0.7) — stylized purple gunsmoke
            if (heatProgress > 0.7f && Main.rand.NextBool(12))
            {
                Vector2 smokePos = gunBarrel + new Vector2(Main.rand.NextFloat(-5f, 5f), -5f);
                MagnumParticleHandler.SpawnParticle(
                    new HeavySmokeParticle(smokePos, new Vector2(0, -0.8f), GunsmokePurple * 0.4f, Main.rand.Next(20, 35), 0.15f, 0.3f, 0.01f, false));
            }

            // Dynamic lighting scaling with heat
            Lighting.AddLight(gunBarrel, Color.Lerp(CoolBarrel, barrelColor, heatProgress).ToVector3() * (0.25f + heatProgress * 0.5f));
        }

        // ══════════════════════════════════════════════════════════════
        //  2. MUZZLE FLASH VFX  (called from Shoot each shot)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Heat-reactive muzzle flash: bloom, halos, sparks, casings, petals, smoke.</summary>
        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 velocity, float heatProgress)
        {
            // Bloom flare color shifts with heat
            Color flashColor;
            if (heatProgress < 0.3f)
                flashColor = Color.Lerp(EroicaPalette.Sakura, WarmBlush, heatProgress / 0.3f);
            else if (heatProgress < 0.6f)
                flashColor = Color.Lerp(WarmBlush, EroicaPalette.Gold, (heatProgress - 0.3f) / 0.3f);
            else
                flashColor = Color.Lerp(EroicaPalette.Gold, WhiteHotMuzzle, (heatProgress - 0.6f) / 0.4f);

            EroicaVFXLibrary.BloomFlare(muzzlePos, flashColor, 0.4f + heatProgress * 0.3f, 8);

            // Gradient halo rings at high heat
            if (heatProgress > 0.4f)
                EroicaVFXLibrary.SpawnGradientHaloRings(muzzlePos, 1 + (int)(heatProgress * 2), 0.2f + heatProgress * 0.15f);

            // Directional spark spray in firing direction
            Vector2 fireDir = velocity.SafeNormalize(Vector2.UnitX);
            EroicaVFXLibrary.SpawnDirectionalSparks(muzzlePos, fireDir, 3 + (int)(heatProgress * 5), 5f + heatProgress * 3f);

            // Casing eject particles (CasingBrass, affected by gravity)
            Vector2 ejectDir = new Vector2(-fireDir.Y, fireDir.X);
            for (int i = 0; i < 2; i++)
            {
                Vector2 casingVel = ejectDir * Main.rand.NextFloat(2f, 4f) + new Vector2(0f, -Main.rand.NextFloat(1f, 3f));
                Dust casing = Dust.NewDustPerfect(muzzlePos, DustID.Copper, casingVel, 0, CasingBrass, 0.8f);
                casing.noGravity = false;
            }

            // Sakura petal scatter
            if (Main.rand.NextBool(6))
                EroicaVFXLibrary.SpawnSakuraPetals(muzzlePos, 1, 15f);

            // Music notes at high heat
            if (heatProgress > 0.5f && Main.rand.NextBool(4))
                EroicaVFXLibrary.SpawnMusicNotes(muzzlePos, 2, 18f);

            // Spent shell smoke puff
            if (Main.rand.NextBool(3))
                MagnumParticleHandler.SpawnParticle(
                    new HeavySmokeParticle(muzzlePos, fireDir * 1.5f, GunsmokePurple * 0.3f, 15, 0.1f, 0.25f, 0.02f, false));

            Lighting.AddLight(muzzlePos, MuzzleFlash.ToVector3() * (0.6f + heatProgress * 0.4f));
        }

        // ══════════════════════════════════════════════════════════════
        //  3. BULLET PROJECTILE VFX  (AI / OnHitNPC / OnKill)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Per-frame: sakura tracer trail, heat-reactive intensity, petal sparkle, lighting.</summary>
        public static void BulletTrailVFX(Projectile proj, float heatProgress)
        {
            // Sakura tracer dust trail — denser at high heat
            if (Main.rand.NextBool(heatProgress > 0.5f ? 1 : 2))
            {
                Vector2 trailVel = -proj.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color trailColor = Color.Lerp(TracerPink, EroicaPalette.Gold, heatProgress * 0.6f);
                Dust t = Dust.NewDustPerfect(proj.Center + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.PinkTorch, trailVel, 0, trailColor, 1.0f + heatProgress * 0.6f);
                t.noGravity = true;
            }

            // Occasional petal sparkle
            if (Main.rand.NextBool(5))
            {
                Dust s = Dust.NewDustPerfect(proj.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.Enchanted_Pink, -proj.velocity * Main.rand.NextFloat(0.05f, 0.15f), 0, TracerPink, 1.1f);
                s.noGravity = true;
            }

            Lighting.AddLight(proj.Center, TracerPink.ToVector3() * (0.3f + heatProgress * 0.4f));
        }

        /// <summary>On-hit: heat-scaled explosion, petal scatter, impact dust, music note.</summary>
        public static void BulletHitVFX(Vector2 hitPos, float heatProgress)
        {
            EroicaVFXLibrary.HeroicImpact(hitPos, 0.5f + heatProgress * 0.4f);
            EroicaVFXLibrary.SpawnSakuraPetals(hitPos, 2 + (int)(heatProgress * 3), 20f);

            for (int i = 0; i < 5 + (int)(heatProgress * 8); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color col = Color.Lerp(TracerPink, EroicaPalette.Gold, Main.rand.NextFloat() * heatProgress);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.Torch, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            if (heatProgress > 0.4f && Main.rand.NextBool(3))
                EroicaVFXLibrary.SpawnMusicNotes(hitPos, 1, 12f, 0.7f, 0.9f, 25);

            Lighting.AddLight(hitPos, EroicaPalette.Sakura.ToVector3() * (0.6f + heatProgress * 0.5f));
        }

        /// <summary>Expiry: small petal dissipation when bullet fades.</summary>
        public static void BulletDeathVFX(Vector2 pos)
        {
            EroicaVFXLibrary.SpawnSakuraPetals(pos, 2, 12f);
            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustPerfect(pos, DustID.PinkFairy, Main.rand.NextVector2Circular(2f, 2f), 0, TracerPink, 0.9f);
                d.noGravity = true;
            }
            Lighting.AddLight(pos, TracerPink.ToVector3() * 0.3f);
        }

        // ══════════════════════════════════════════════════════════════
        //  4. BULLET PREDRAW  (called from Projectile.PreDraw)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Tracer rendering: {A=0} trail with TracerPink->MuzzleFlash gradient,
        /// heat-reactive bloom intensity, afterimage with sakura tint, pulsing core.
        /// </summary>
        public static bool DrawBulletProjectile(SpriteBatch sb, Projectile proj, float heatProgress, ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[proj.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 projScreen = proj.Center - Main.screenPosition;

            // {A=0} bloom trail behind the bullet
            EroicaVFXLibrary.DrawProjectileTrail(sb, proj, TracerPink);

            // Shader-enhanced tracer trail pass
            {
                Texture2D shaderGlow = MagnumTextureRegistry.GetSoftGlow();
                EroicaShaderManager.BeginShaderAdditive(sb);
                EroicaShaderManager.ApplyBlossomTracerTrail(Main.GlobalTimeWrappedHourly, heatProgress);
                Vector2 glowOrigin = shaderGlow.Size() * 0.5f;
                for (int k = 0; k < proj.oldPos.Length; k++)
                {
                    if (proj.oldPos[k] == Vector2.Zero) continue;
                    Vector2 shaderPos = proj.oldPos[k] - Main.screenPosition + new Vector2(proj.width / 2f, proj.height / 2f);
                    float shaderProgress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;
                    sb.Draw(shaderGlow, shaderPos, null, Color.White * shaderProgress * (0.4f + heatProgress * 0.3f), proj.oldRot[k],
                        glowOrigin, proj.scale * (0.3f + shaderProgress * 0.5f), SpriteEffects.None, 0f);
                }
                EroicaShaderManager.RestoreSpriteBatch(sb);
            }

            // Afterimage trail with sakura-tinted {A=0} additive colors
            for (int k = 0; k < proj.oldPos.Length; k++)
            {
                if (proj.oldPos[k] == Vector2.Zero) continue;

                Vector2 drawPos = proj.oldPos[k] - Main.screenPosition + new Vector2(proj.width / 2f, proj.height / 2f);
                float progress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;
                Color trailColor = (Color.Lerp(TracerPink, MuzzleFlash, progress * heatProgress) * progress) with { A = 0 };
                float scale = proj.scale * (0.4f + progress * 0.6f);

                sb.Draw(texture, drawPos, null, trailColor, proj.oldRot[k], drawOrigin, scale, SpriteEffects.None, 0f);
            }

            // Heat-reactive bloom stack — hotter = more intense
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.06f + 1f;
            float bloomOpacity = 0.25f + heatProgress * 0.3f;

            sb.Draw(texture, projScreen, null, (TracerPink with { A = 0 }) * bloomOpacity * 0.5f,
                proj.rotation, drawOrigin, proj.scale * 1.3f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, (MuzzleFlash with { A = 0 }) * bloomOpacity * 0.4f,
                proj.rotation, drawOrigin, proj.scale * 1.15f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, (WhiteHotMuzzle with { A = 0 }) * bloomOpacity * 0.3f,
                proj.rotation, drawOrigin, proj.scale * 1.05f * pulse, SpriteEffects.None, 0f);

            // Main sprite with warm tint
            sb.Draw(texture, projScreen, null, new Color(255, 240, 230, 220),
                proj.rotation, drawOrigin, proj.scale, SpriteEffects.None, 0f);

            return false;
        }

        // ══════════════════════════════════════════════════════════════
        //  5. OVERHEAT EFFECTS  (called when heat reaches maximum)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Max heat: steam explosion, bloom burst, radial embers, music chord, screen trauma.</summary>
        public static void OverheatPulse(Vector2 gunPos)
        {
            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.4f, Volume = 0.6f }, gunPos);

            // Dramatic steam explosion
            for (int i = 0; i < 8; i++)
            {
                Vector2 steamVel = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0f, -2f);
                MagnumParticleHandler.SpawnParticle(
                    new HeavySmokeParticle(gunPos, steamVel, GunsmokePurple * 0.5f, Main.rand.Next(25, 40), 0.2f, 0.4f, 0.02f, false));
            }

            // Wide bloom burst + radial ember spray
            EroicaVFXLibrary.BloomBurst(gunPos, 1.2f);
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Dust d = Dust.NewDustPerfect(gunPos, DustID.Torch, vel, 0,
                    Color.Lerp(HotCrimson, EroicaPalette.Gold, Main.rand.NextFloat()), 1.6f);
                d.noGravity = true;
            }

            // Music note chord + sakura scatter
            EroicaVFXLibrary.SpawnMusicNotes(gunPos, 5, 30f, 0.8f, 1.1f, 35);
            EroicaVFXLibrary.SpawnSakuraPetals(gunPos, 6, 40f);

            MagnumScreenEffects.AddScreenShake(3f);
            Lighting.AddLight(gunPos, WhiteHotMuzzle.ToVector3() * 1.5f);
        }

        /// <summary>Rising distortion particles simulating heat shimmer (subtle low-opacity glow).</summary>
        public static void HeatMirage(Vector2 gunBarrel, float heatProgress)
        {
            if (heatProgress < 0.3f)
                return;

            float mirageStrength = (heatProgress - 0.3f) / 0.7f;
            for (int i = 0; i < 1 + (int)(mirageStrength * 2); i++)
            {
                Vector2 miragePos = gunBarrel + new Vector2(Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-8f, 0f));
                Vector2 mirageVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.5f, 1.5f));
                Color mirageColor = Color.Lerp(CoolBarrel, WarmBlush, mirageStrength) * (0.08f + mirageStrength * 0.07f);
                MagnumParticleHandler.SpawnParticle(
                    new GenericGlowParticle(miragePos, mirageVel, mirageColor, 0.2f + mirageStrength * 0.15f, 25, false));
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  6. AMBIENT HOLD VFX  (called from HoldItem every frame)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Sakura petal drift, heat-reactive embers, music notes, dynamic lighting.</summary>
        public static void HoldItemVFX(Player player, float heatProgress)
        {
            // Sakura petal drift — always present, the weapon's identity
            if (Main.rand.NextBool(8))
            {
                Vector2 petalPos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                EroicaVFXLibrary.SpawnSakuraPetals(petalPos, 1, 20f);

                // At high heat, petals catch fire
                if (heatProgress > 0.5f && Main.rand.NextBool(3))
                    MagnumParticleHandler.SpawnParticle(
                        new GenericGlowParticle(petalPos, Vector2.Zero, HotCrimson * 0.5f, 0.15f, 12, true));
            }

            // Heat-reactive ember proximity
            if (heatProgress > 0.4f && Main.rand.NextBool((int)(12 - heatProgress * 8)))
            {
                Vector2 emberPos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Color emberColor = Color.Lerp(WarmBlush, EroicaPalette.Gold, Main.rand.NextFloat() * heatProgress);
                Dust e = Dust.NewDustPerfect(emberPos, DustID.Torch,
                    new Vector2(0f, -Main.rand.NextFloat(0.5f, 1.5f)), 0, emberColor, 0.9f);
                e.noGravity = true;
            }

            // Music notes at high heat — the gun sings when hot
            if (heatProgress > 0.3f && Main.rand.NextBool(20))
                EroicaVFXLibrary.SpawnMusicNotes(player.Center + new Vector2(40f * player.direction, -2f), 1, 15f);

            // Dynamic lighting — warm aura around the player
            if (heatProgress > 0.15f)
                Lighting.AddLight(player.Center,
                    Color.Lerp(CoolBarrel, EroicaPalette.Gold, heatProgress).ToVector3() * (0.2f + heatProgress * 0.3f));
        }
    }
}
