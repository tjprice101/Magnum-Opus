using System;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.ClairDeLune;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.RequiemOfTime.Projectiles
{
    /// <summary>
    /// Requiem of Time — Dual-Zone Caster magic projectile.
    /// Left-click fires orb spawning Forward Zone (+30% projectile speed, accelerates allies).
    /// Right-click: Reverse Zone (slow -40% enemy movement).
    /// Overlapping zones: 2x damage (Temporal Paradox).
    /// </summary>
    public class TimeFreezeSlashProjectile : ModProjectile
    {
        private bool IsReverseZone { get; set; } = false;
        private VertexStrip _strip;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail VFX — temporal crimson for reverse zone, pearl blue for forward
            if (Main.rand.NextBool(2))
            {
                Color trailColor = IsReverseZone ? ClairDeLunePalette.TemporalCrimson : ClairDeLunePalette.PearlBlue;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    -Projectile.velocity * 0.15f, 0, trailColor, 0.8f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center,
                IsReverseZone ? ClairDeLunePalette.TemporalCrimson.ToVector3() * 0.6f : ClairDeLunePalette.PearlBlue.ToVector3() * 0.6f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // IncisorOrb shader beam trail + 5-layer palette-cycling bloom head
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.ClairDeLune, ref _strip);

            // Reverse zone crimson indicator: outer ring distinguishes zone type
            if (IsReverseZone)
            {
                SpriteBatch sb = Main.spriteBatch;
                try
                {
                    Vector2 drawPos = Projectile.Center - Main.screenPosition;
                    float t = (float)Main.timeForVisualEffects;
                    float pulse = 0.75f + 0.25f * MathF.Sin(t * 0.18f);

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                        Main.DefaultSamplerState, DepthStencilState.None,
                        RasterizerState.CullCounterClockwise, null,
                        Main.GameViewMatrix.TransformationMatrix);

                    Texture2D bloom = ModContent.Request<Texture2D>(
                        "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    Vector2 origin = bloom.Size() / 2f;

                    sb.Draw(bloom, drawPos, null,
                        (ClairDeLunePalette.TemporalCrimson with { A = 0 }) * 0.55f * pulse, 0f, origin,
                        Projectile.scale * 1.8f * pulse, SpriteEffects.None, 0f);
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
            ClairDeLuneVFXLibrary.ProjectileImpact(target.Center, 1.0f);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.SpawnRadialDustBurst(Projectile.Center, 8, 4f);

            // Spawn zone (duration 90 frames = 1.5s)
            if (Main.myPlayer == Projectile.owner)
            {
                var combatPlayer = Main.player[Projectile.owner].GetModPlayer<ClairDeLuneCombatPlayer>();

                if (IsReverseZone)
                {
                    combatPlayer.HasReverseZone = true;
                    combatPlayer.ReverseZoneCenter = Projectile.Center;
                    combatPlayer.ReverseZoneTimer = 90;
                }
                else
                {
                    combatPlayer.HasForwardZone = true;
                    combatPlayer.ForwardZoneCenter = Projectile.Center;
                    combatPlayer.ForwardZoneTimer = 90;
                }
            }
        }
    }
}
