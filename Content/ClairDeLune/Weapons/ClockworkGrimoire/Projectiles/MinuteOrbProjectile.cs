using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.ClairDeLune;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkGrimoire.Projectiles
{
    /// <summary>
    /// Minute Orb — One of 12 ticking orbs from ClockworkGrimoire's minute hand sweep.
    /// VoronoiCell-style countdown with 3-tick detonation and 4-ring burst.
    /// 3 render passes: (1) ClairDeLuneMoonlit MoonlitGlow ticking body,
    /// (2) ClairDeLunePearlGlow PearlShimmer tick-pulse overlay, (3) Multi-scale bloom + Voronoi facets.
    /// </summary>
    public class MinuteOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private VertexStrip _vertexStrip;
        private int _tickCount;
        private int _timer;
        private const int TickInterval = 25;
        private const int TotalTicks = 3;
        private const float DetonationRadius = 48f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 100;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 100;
        }

        public override void AI()
        {
            _timer++;

            // Slow to a stop
            Projectile.velocity *= 0.97f;
            Projectile.rotation += 0.05f;

            // Tick countdown
            if (_timer % TickInterval == 0 && _tickCount < TotalTicks)
            {
                _tickCount++;
                SoundEngine.PlaySound(SoundID.Item11 with { Pitch = 0.3f + _tickCount * 0.15f, Volume = 0.3f },
                    Projectile.Center);

                // Tick burst particles
                Color tickCol = Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.MoonbeamGold,
                    _tickCount / (float)TotalTicks);
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 vel = angle.ToRotationVector2() * (1.5f + _tickCount * 0.5f);
                    var spark = new GenericGlowParticle(Projectile.Center, vel,
                        tickCol with { A = 0 } * 0.4f, 0.04f, 6, true);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }

            // Detonate after final tick
            if (_tickCount >= TotalTicks && _timer >= TotalTicks * TickInterval + 4)
            {
                Detonate();
                Projectile.Kill();
                return;
            }

            if (Projectile.velocity.Length() < 0.3f)
                Projectile.velocity = Vector2.Zero;

            float tickProgress = (float)_tickCount / TotalTicks;
            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.SoftBlue.ToVector3() * (0.2f + tickProgress * 0.3f));
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity = Vector2.Zero;
            return false;
        }

        private void Detonate()
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.4f, Volume = 0.5f }, Projectile.Center);

            // 4-ring detonation burst
            for (int ring = 0; ring < 4; ring++)
            {
                float ringProgress = ring / 3f;
                Color ringCol = Color.Lerp(ClairDeLunePalette.MoonbeamGold, ClairDeLunePalette.PearlFrost, ringProgress);
                int points = 10 + ring * 2;
                for (int p = 0; p < points; p++)
                {
                    float angle = MathHelper.TwoPi * p / points + ring * 0.15f;
                    Vector2 vel = angle.ToRotationVector2() * (1.5f + ring * 1.2f);
                    var dot = new GenericGlowParticle(Projectile.Center, vel,
                        ringCol with { A = 0 } * (0.4f - ring * 0.07f), 0.05f, 8 + ring, true);
                    MagnumParticleHandler.SpawnParticle(dot);
                }
            }

            var flash = new BloomParticle(Projectile.Center, Vector2.Zero,
                ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.5f, 0.3f, 8);
            MagnumParticleHandler.SpawnParticle(flash);

            // AoE damage
            for (int n = 0; n < Main.maxNPCs; n++)
            {
                NPC npc = Main.npc[n];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (Vector2.Distance(Projectile.Center, npc.Center) < DetonationRadius)
                {
                    Player player = Main.player[Projectile.owner];
                    player.ApplyDamageToNPC(npc, Projectile.damage, Projectile.knockBack,
                        (npc.Center - Projectile.Center).X > 0 ? 1 : -1, false);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.ClairDeLune, ref _vertexStrip);

                // --- Ticking moonbeam halo accent ---
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                var glowTex = MagnumTextureRegistry.GetSoftGlow();
                Vector2 origin = glowTex.Size() / 2f;
                Vector2 pos = Projectile.Center - Main.screenPosition;
                float tick = 0.85f + 0.15f * (float)Math.Sin(Main.timeForVisualEffects * 0.1);
                Color gold = (ClairDeLunePalette.MoonbeamGold with { A = 0 }) * 0.5f * tick;
                sb.Draw(glowTex, pos, null, gold, 0f, origin, 0.04f, SpriteEffects.None, 0f);

                sb.End();
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
    }
}
