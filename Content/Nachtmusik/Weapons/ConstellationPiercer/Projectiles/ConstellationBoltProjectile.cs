using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Nachtmusik.Systems;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Nachtmusik.Weapons.ConstellationPiercer.Projectiles
{
    /// <summary>
    /// Constellation Bolt — a needle of compressed starlight that pierces 3 enemies
    /// and etches their positions into the sky as constellation marks.
    /// After 3 marks, the triangle detonates with visible golden constellation lines.
    /// </summary>
    public class ConstellationBoltProjectile : ModProjectile
    {
        // Thin 4-pointed star needle — looks like a compressed point of starlight
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar";

        private static Asset<Texture2D> _bloomTex;
        private static Asset<Texture2D> _starFlare;
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _starTex;
        private VertexStrip _strip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.timeLeft = 140;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Rotate along velocity (star needle points forward)
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Occasional stray constellation spark
            if (Main.rand.NextBool(6))
            {
                Color dustCol = Main.rand.NextBool(2) ? NachtmusikPalette.RadianceGold : NachtmusikPalette.CosmicPurple;
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.WhiteTorch,
                    -Projectile.velocity * 0.06f + Main.rand.NextVector2Circular(0.4f, 0.4f),
                    0, dustCol, Main.rand.NextFloat(0.35f, 0.7f));
                d.noGravity = true;
                d.fadeIn = 0.25f;
            }

            Lighting.AddLight(Projectile.Center, NachtmusikPalette.RadianceGold.ToVector3() * 0.45f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.Nachtmusik, ref _strip);

            // Draw the constellation star needle + flare cross on top as an additive overlay
            _starTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar", AssetRequestMode.ImmediateLoad);
            _starFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare", AssetRequestMode.ImmediateLoad);

            SpriteBatch sb = Main.spriteBatch;
            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                float pulse = 0.88f + 0.12f * MathF.Sin(Main.GameUpdateCount * 0.28f + Projectile.whoAmI * 1.9f);
                Vector2 headPos = Projectile.Center - Main.screenPosition;

                // Star flare cross (gold + white-blue offset)
                if (_starFlare?.Value != null)
                {
                    Texture2D flare = _starFlare.Value;
                    Vector2 flareOrigin = flare.Size() / 2f;
                    sb.Draw(flare, headPos, null,
                        (NachtmusikPalette.RadianceGold with { A = 0 }) * 0.55f * pulse,
                        Projectile.rotation, flareOrigin, 0.28f * Projectile.scale, SpriteEffects.None, 0f);
                    sb.Draw(flare, headPos, null,
                        (NachtmusikPalette.StarWhite with { A = 0 }) * 0.35f * pulse,
                        Projectile.rotation + MathHelper.PiOver4, flareOrigin, 0.16f * Projectile.scale, SpriteEffects.None, 0f);
                }

                // Star needle body
                if (_starTex?.Value != null)
                {
                    sb.Draw(_starTex.Value, headPos, null,
                        (NachtmusikPalette.TwinklingWhite with { A = 0 }) * 0.95f * pulse,
                        Projectile.rotation, _starTex.Value.Size() / 2f,
                        0.55f * Projectile.scale, SpriteEffects.None, 0f);
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 600);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);

            Player owner = Main.player[Projectile.owner];
            var combat = owner.GetModPlayer<NachtmusikCombatPlayer>();
            bool full = combat.AddConstellationMarker(target.Center);
            int markCount = combat.ConstellationMarkerCount;

            // Hit burst — scales with number of marks placed (visual feedback on mark count)
            int burstCount = 8 + markCount * 4;
            for (int i = 0; i < burstCount; i++)
            {
                float angle = MathHelper.TwoPi * i / burstCount;
                float speed = Main.rand.NextFloat(2.5f, 5.5f);
                Color col = i % 3 == 0 ? NachtmusikPalette.RadianceGold
                          : i % 3 == 1 ? NachtmusikPalette.StarlitBlue
                          :               NachtmusikPalette.StarGold;
                Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                    new Vector2(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed), 0, col, 0.85f);
                d.noGravity = true;
                d.fadeIn = 0.4f;
            }

            // Constellation mark indicator: compact star cluster at mark position
            for (int i = 0; i < markCount * 5; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(2f, 2f),
                    0, NachtmusikPalette.ConstellationBlue, Main.rand.NextFloat(0.5f, 1.1f));
                d.noGravity = true;
            }

            if (full)
            {
                Vector2[] marks = combat.ConstellationMarkers;

                // Spawn damage zones along the 3 triangle edges
                for (int i = 0; i < 3; i++)
                {
                    Vector2 a = marks[i];
                    Vector2 b = marks[(i + 1) % 3];
                    Vector2 midpoint = (a + b) / 2f;
                    float length = Vector2.Distance(a, b);
                    float radius = MathHelper.Clamp(length * 0.3f, 40f, 120f);

                    GenericDamageZone.SpawnZone(
                        Projectile.GetSource_FromThis(),
                        midpoint, Projectile.damage, Projectile.knockBack, Projectile.owner,
                        GenericDamageZone.FLAG_SLOW | GenericDamageZone.FLAG_SPAWN_CHILDREN,
                        radius, GenericHomingOrbChild.THEME_NACHTMUSIK, durationFrames: 60);
                }

                // === CONSTELLATION LINE VFX: dense particle trails along each edge ===
                for (int edge = 0; edge < 3; edge++)
                {
                    Vector2 a = marks[edge];
                    Vector2 b = marks[(edge + 1) % 3];
                    float dist = Vector2.Distance(a, b);
                    int steps = Math.Max((int)(dist / 10f), 4);

                    for (int s = 0; s <= steps; s++)
                    {
                        float t = (float)s / steps;
                        Vector2 pos = Vector2.Lerp(a, b, t);
                        // Line: gold/blue/violet alternating
                        Color lineCol = (s % 3 == 0) ? NachtmusikPalette.RadianceGold
                                      : (s % 3 == 1) ? NachtmusikPalette.StarlitBlue
                                      :                  NachtmusikPalette.Violet;
                        Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(2f, 2f),
                            DustID.WhiteTorch,
                            Main.rand.NextVector2Circular(0.6f, 0.6f),
                            0, lineCol, Main.rand.NextFloat(0.6f, 1f));
                        d.noGravity = true;
                        d.fadeIn = 0.3f;
                    }
                }

                // Grand star burst at each vertex
                for (int v = 0; v < 3; v++)
                {
                    for (int i = 0; i < 18; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 18f;
                        float speed = Main.rand.NextFloat(3f, 7f + v);
                        Color col = i % 3 == 0 ? NachtmusikPalette.StarGold
                                  : i % 3 == 1 ? NachtmusikPalette.StarlitBlue
                                  :               NachtmusikPalette.CosmicPurple;
                        Dust d = Dust.NewDustPerfect(marks[v], DustID.WhiteTorch,
                            new Vector2(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed),
                            0, col, 0.9f);
                        d.noGravity = true;
                        d.fadeIn = 0.5f;
                    }
                }

                combat.ResetConstellationMarkers();
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                float speed = Main.rand.NextFloat(2f, 4.5f);
                Color col = i % 2 == 0 ? NachtmusikPalette.RadianceGold : NachtmusikPalette.CosmicPurple;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    new Vector2(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed),
                    0, col, 0.6f);
                d.noGravity = true;
            }
        }
    }
}
