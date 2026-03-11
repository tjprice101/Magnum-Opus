using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Utilities.IncisorUtils;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Projectiles
{
    /// <summary>
    /// Staccato Note Shard 窶・fired in a 5-shard fan during Movement II (Allegretto).
    /// Tiny glowing eighth notes (笙ｪ) in silver-white that bounce off tiles up to 2 times.
    /// If 3+ shards hit the same enemy, they detonate in a small lunar burst.
    /// "The deceptively light middle section 窶・each note a percussive strike."
    /// </summary>
    public class StaccatoNoteProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar";

        private const int MaxBounces = 2;
        private int bounceCount = 0;

        // Track hits per NPC for detonation - stored in ai[0] as target NPC index
        public ref float TargetHitTracker => ref Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 150;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 1;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (bounceCount >= MaxBounces)
                return true;

            bounceCount++;
            SoundEngine.PlaySound(SoundID.Item56 with { Pitch = 0.5f + bounceCount * 0.2f, Volume = 0.3f },
                Projectile.Center);

            // Bounce physics
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X * 0.85f;
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y * 0.85f;

            // Bounce spark VFX
            if (!Main.dedServ)
            {
                for (int i = 0; i < 3; i++)
                {
                    Color sc = Color.Lerp(Color.White, new Color(200, 210, 255), Main.rand.NextFloat());
                    var spark = new ConstellationSparkParticle(
                        Projectile.Center, Main.rand.NextVector2Circular(3f, 3f) + Projectile.velocity * 0.2f,
                        false, Main.rand.Next(6, 12), Main.rand.NextFloat(0.06f, 0.12f),
                        sc, new Vector2(0.3f, 1.8f), quickShrink: true);
                    IncisorParticleHandler.SpawnParticle(spark);
                }
            }

            return false;
        }

        public override void AI()
        {
            Projectile.rotation += Projectile.velocity.Length() * 0.05f * Math.Sign(Projectile.velocity.X);

            // Gentle gravity
            Projectile.velocity.Y += 0.08f;

            // Moonlight glow
            Lighting.AddLight(Projectile.Center, new Vector3(0.35f, 0.38f, 0.55f) * 0.5f);

            // Tiny sparkle trail
            if (Main.rand.NextBool(5))
            {
                Color sparkColor = Color.Lerp(new Color(230, 235, 255), new Color(170, 140, 255),
                    Main.rand.NextFloat());
                var spark = new ConstellationSparkParticle(
                    Projectile.Center, -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    false, Main.rand.Next(8, 14), Main.rand.NextFloat(0.04f, 0.1f),
                    sparkColor, new Vector2(0.3f, 1.5f), quickShrink: true);
                IncisorParticleHandler.SpawnParticle(spark);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item25 with { Pitch = 0.6f, Volume = 0.35f }, target.Center);

            // Track staccato hits on this target for detonation
            // Use the global NPC staccato hit counter tracked in IncisorPlayer
            Player owner = Main.player[Projectile.owner];
            var ip = owner.GetModPlayer<Utilities.IncisorPlayer>();
            ip.RegisterStaccatoHit(target.whoAmI);

            // Check if 3+ staccato notes hit this enemy 窶・if so, lunar burst detonation
            if (ip.GetStaccatoHits(target.whoAmI) >= 3)
            {
                ip.ResetStaccatoHits(target.whoAmI);
                DetonateLunarBurst(target);
            }

            // Small impact sparkles
            if (!Main.dedServ)
            {
                for (int i = 0; i < 2; i++)
                {
                    Color sc = MulticolorLerp(Main.rand.NextFloat(), IncisorPalette);
                    var spark = new ConstellationSparkParticle(
                        target.Center, Main.rand.NextVector2Circular(4f, 4f),
                        true, Main.rand.Next(8, 14), Main.rand.NextFloat(0.08f, 0.15f),
                        sc, new Vector2(0.4f, 1.3f));
                    IncisorParticleHandler.SpawnParticle(spark);
                }
            }
        }

        private void DetonateLunarBurst(NPC target)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.6f }, target.Center);

            // Bonus damage explosion (50% of projectile damage)
            if (Main.myPlayer == Projectile.owner)
            {
                int burstDmg = (int)(Projectile.damage * 0.5f);
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center, Vector2.Zero,
                    ModContent.ProjectileType<LunarNova>(), burstDmg, 0f, Projectile.owner);
            }

            // VFX burst
            if (!Main.dedServ)
            {
                MoonlightVFXLibrary.SpawnMusicNotes(target.Center, count: 4, spread: 20f,
                    minScale: 0.6f, maxScale: 0.9f, lifetime: 35);

                for (int i = 0; i < 8; i++)
                {
                    Color sparkColor = MulticolorLerp(Main.rand.NextFloat(), IncisorPalette);
                    var spark = new ConstellationSparkParticle(
                        target.Center, Main.rand.NextVector2Circular(8f, 8f),
                        true, Main.rand.Next(14, 22), Main.rand.NextFloat(0.12f, 0.28f),
                        sparkColor, new Vector2(0.5f, 1.5f));
                    IncisorParticleHandler.SpawnParticle(spark);
                }

                for (int i = 0; i < 4; i++)
                {
                    var mote = new LunarMoteParticle(
                        target.Center + Main.rand.NextVector2Circular(15f, 15f),
                        Main.rand.NextVector2Circular(4f, 4f),
                        Main.rand.NextFloat(0.3f, 0.5f),
                        MulticolorLerp(Main.rand.NextFloat(), IncisorPalette),
                        Main.rand.Next(20, 30), 2.5f, 3f, hueShift: 0.012f);
                    IncisorParticleHandler.SpawnParticle(mote);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            var starTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar",
                AssetRequestMode.ImmediateLoad).Value;
            var bloomTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                AssetRequestMode.ImmediateLoad).Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 0.9f + 0.1f * MathF.Sin(Projectile.timeLeft * 0.2f);

            // Soft bloom glow
            Color glowColor = new Color(200, 210, 255) with { A = 0 };
            Main.spriteBatch.Draw(bloomTex, drawPos, null, glowColor * 0.3f,
                0f, bloomTex.Size() * 0.5f, 0.12f * pulse, SpriteEffects.None, 0f);

            // Note body 窶・silver-white
            Color bodyColor = Color.Lerp(Color.White, new Color(200, 210, 255), 0.3f) with { A = 0 };
            Main.spriteBatch.Draw(starTex, drawPos, null, bodyColor * 0.85f,
                Projectile.rotation, starTex.Size() * 0.5f, 0.12f * pulse,
                SpriteEffects.None, 0f);

            // Afterimage trail
            for (int i = 1; i < Math.Min(Projectile.oldPos.Length, 6); i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) break;
                float fade = 1f - i / 6f;
                Vector2 oldDraw = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                Main.spriteBatch.Draw(starTex, oldDraw, null, bodyColor * fade * 0.25f,
                    Projectile.oldRot[i], starTex.Size() * 0.5f, 0.08f * fade,
                    SpriteEffects.None, 0f);
            }

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

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;
            SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.8f, Volume = 0.2f }, Projectile.Center);
        }
    }
}
