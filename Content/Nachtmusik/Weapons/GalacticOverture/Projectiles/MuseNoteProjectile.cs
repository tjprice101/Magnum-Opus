using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Debuffs;

namespace MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture.Projectiles
{
    /// <summary>
    /// Muse's musical note attack projectile — fires from the Celestial Muse minion.
    /// Uses CursiveMusicNote texture, spins, leaves golden musical trail,
    /// explodes into music notes on kill.
    /// </summary>
    public class MuseNoteProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote";

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.2f;

            // Musical trail — golden glow particles
            if (Main.rand.NextBool(3))
            {
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    NachtmusikPalette.RadianceGold * 0.6f, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Musical notation trail — music notes + golden sparkle
            if (Main.rand.NextBool(5))
            {
                NachtmusikVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 1f, 0.7f, 0.7f, 25);

                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -0.9f);
                var sparkle = new SparkleParticle(Projectile.Center, noteVel * 0.5f, NachtmusikPalette.RadianceGold * 0.5f, 0.22f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, NachtmusikPalette.RadianceGold.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 240);

            // Musical impact flare
            CustomParticles.GenericFlare(target.Center, NachtmusikPalette.RadianceGold, 0.4f, 10);

            // Musical chord — burst of notes
            NachtmusikVFXLibrary.SpawnMusicNotes(target.Center, 4, 3f, 0.7f, 0.9f, 25);

            // Sparkle burst
            for (int i = 0; i < 3; i++)
            {
                var sparkle = new SparkleParticle(target.Center, Main.rand.NextVector2Circular(2.5f, 2.5f),
                    NachtmusikPalette.StarWhite * 0.6f, 0.22f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Golden burst
            for (int i = 0; i < 4; i++)
            {
                var burst = new GenericGlowParticle(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f),
                    NachtmusikPalette.RadianceGold * 0.6f, 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // Musical finale — burst of notes
            NachtmusikVFXLibrary.SpawnMusicNotes(Projectile.Center, 5, 3.5f, 0.7f, 0.9f, 25);

            // Finale sparkle cascade
            for (int i = 0; i < 4; i++)
            {
                var sparkle = new SparkleParticle(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f),
                    NachtmusikPalette.RadianceGold * 0.6f, 0.25f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Procedural Nachtmusik VFX — musical note rendering
            ProceduralProjectileVFX.DrawNachtmusikProjectile(Main.spriteBatch, Projectile, 0.25f);

            // Nachtmusik theme star flare accent
            NachtmusikShaderManager.BeginAdditive(Main.spriteBatch);
            NachtmusikVFXLibrary.DrawThemeStarFlare(Main.spriteBatch, Projectile.Center, 1f, 0.5f);
            NachtmusikShaderManager.RestoreSpriteBatch(Main.spriteBatch);

            return false;
        }
    }
}
