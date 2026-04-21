using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.ClairDeLune;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.StarfallWhisper.Projectiles
{
    /// <summary>
    /// Starfall Whisper — Clair de Lune ranged. Delayed Replay mechanic.
    /// Orb fires straight. On hit, marks fracture point. 1.5s later, replay spawns at fracture traveling same direction (75% damage).
    /// If replay hits different enemy, creates own fracture (max 3 generations).
    /// Right-click: 5 orbs in spread.
    /// </summary>
    public class TemporalArrowProjectile : ModProjectile
    {
        public int Generation { get; set; } = 0;
        private VertexStrip _strip;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail VFX
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    -Projectile.velocity * 0.1f, 0, ClairDeLunePalette.PearlBlue, 0.7f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.AddClairDeLuneLight(Projectile.Center, 0.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // IncisorOrb shader beam trail + 5-layer palette-cycling bloom head
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.ClairDeLune, ref _strip);

            // Generation indicator: orbiting crimson dots — one dot per echo generation
            if (Generation > 0)
            {
                SpriteBatch sb = Main.spriteBatch;
                try
                {
                    Vector2 drawPos = Projectile.Center - Main.screenPosition;
                    float t = (float)Main.timeForVisualEffects;

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                        Main.DefaultSamplerState, DepthStencilState.None,
                        RasterizerState.CullCounterClockwise, null,
                        Main.GameViewMatrix.TransformationMatrix);

                    Texture2D bloom = ModContent.Request<Texture2D>(
                        "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    Vector2 origin = bloom.Size() / 2f;

                    for (int g = 0; g < Generation; g++)
                    {
                        float angle = t * 0.07f * 60f + g * MathHelper.TwoPi / 3f;
                        Vector2 dotPos = drawPos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 18f;
                        sb.Draw(bloom, dotPos, null,
                            (ClairDeLunePalette.TemporalCrimson with { A = 0 }) * 0.65f, 0f, origin,
                            0.22f, SpriteEffects.None, 0f);
                    }
                }
                catch { }
                finally
                {
                    try { sb.End(); } catch { }
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ClairDeLuneVFXLibrary.ProjectileImpact(target.Center, 1f);

            // Mark fracture point if generation < 3
            if (Generation < 3 && Main.myPlayer == Projectile.owner)
            {
                var combatPlayer = Main.player[Projectile.owner].GetModPlayer<ClairDeLuneCombatPlayer>();
                combatPlayer.FracturePoints.Add((target.Center, Generation + 1, (int)Main.GameUpdateCount));

                // Schedule replay after 1.5s (90 frames)
                Projectile.Kill();
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.SpawnRadialDustBurst(Projectile.Center, 6, 3f);
        }
    }
}
