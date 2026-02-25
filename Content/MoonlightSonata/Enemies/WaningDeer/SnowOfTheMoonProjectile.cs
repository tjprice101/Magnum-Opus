using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX.Core;

namespace MagnumOpus.Content.MoonlightSonata.Enemies
{
    /// <summary>
    /// Snow of the Moon - An icy projectile fired by Waning Deer.
    /// Purple and light blue particles trail behind it.
    /// </summary>
    public class SnowOfTheMoonProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Enemies/WaningDeer/SnowOfTheMoon";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 280;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 50;
            Projectile.light = 0.4f;
            Projectile.coldDamage = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.12f;

            // Slight gravity
            Projectile.velocity.Y += 0.03f;
            if (Projectile.velocity.Y > 10f)
                Projectile.velocity.Y = 10f;

            // Palette-based lighting
            Lighting.AddLight(Projectile.Center, MoonlightVFXLibrary.IceBlue.ToVector3() * 0.5f);

            // Trail particles — palette colors
            if (Main.rand.NextBool(2))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Color dustColor = Main.rand.NextBool() ? MoonlightVFXLibrary.DarkPurple : MoonlightVFXLibrary.IceBlue;
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, dustColor, 1.1f);
                trail.noGravity = true;
                trail.velocity *= 0.2f;
            }

            // Snow particles
            if (Main.rand.NextBool(4))
            {
                Dust snow = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Snow, 0f, 0f, 100, default, 0.8f);
                snow.noGravity = true;
                snow.velocity = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
            }

            // Sparkle particles
            if (Main.rand.NextBool(6))
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.BlueFairy, 0f, 0f, 0, default, 0.7f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }

            // Music notes — sparse for enemy projectile
            if (Main.rand.NextBool(12))
            {
                MoonlightVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 8f, 0.6f, 0.75f, 25);
            }

            // Unified enemy projectile trail VFX
            MoonlightEnemyVFX.EnemyProjectileTrail(Projectile);
        }

        public override void OnKill(int timeLeft)
        {
            // Snow projectile death — themed impact
            MoonlightVFXLibrary.ProjectileImpact(Projectile.Center, 0.5f);
            CustomParticles.HaloRing(Projectile.Center, MoonlightVFXLibrary.IceBlue, 0.25f, 14);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);

            // Unified enemy projectile impact VFX
            MoonlightEnemyVFX.EnemyProjectileImpact(Projectile.Center, 0.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // === Trail with {A=0} bloom ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) break;

                Vector2 trailDrawPos = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = 1f - progress;
                Color trailColor = Color.Lerp(MoonlightVFXLibrary.IceBlue, MoonlightVFXLibrary.DarkPurple, progress) with { A = 0 };
                trailColor *= trailAlpha * 0.45f;
                float trailScale = Projectile.scale * (0.4f + 0.6f * trailAlpha);

                Main.EntitySpriteDraw(texture, trailDrawPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0);
            }

            // === 3-layer {A=0} bloom stack ===
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.25f + 0.75f;

            // Layer 1: Outer purple aura
            Color outerGlow = (MoonlightVFXLibrary.DarkPurple with { A = 0 }) * 0.25f * pulse;
            Main.EntitySpriteDraw(texture, drawPos, null, outerGlow, Projectile.rotation, origin, Projectile.scale * 1.15f, SpriteEffects.None, 0);

            // Layer 2: Mid ice blue
            Color midGlow = (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.35f * pulse;
            Main.EntitySpriteDraw(texture, drawPos, null, midGlow, Projectile.rotation, origin, Projectile.scale * 1.08f, SpriteEffects.None, 0);

            // Layer 3: Inner white core
            Color innerGlow = (MoonlightVFXLibrary.MoonWhite with { A = 0 }) * 0.20f * pulse;
            Main.EntitySpriteDraw(texture, drawPos, null, innerGlow, Projectile.rotation, origin, Projectile.scale * 1.03f, SpriteEffects.None, 0);

            return true;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 180);
        }
    }
}
