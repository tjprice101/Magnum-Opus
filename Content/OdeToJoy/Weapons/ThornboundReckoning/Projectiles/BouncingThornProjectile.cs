using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.OdeToJoy.Systems;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Projectiles
{
    /// <summary>
    /// Bouncing Thorn — Thornbound Reckoning orb projectile.
    /// Bounces off tiles and enemies, each bounce increases damage by +20%.
    /// ai[0] = Max bounces remaining
    /// ai[1] = Current damage multiplier (starts at 1.0)
    /// </summary>
    public class BouncingThornProjectile : ModProjectile
    {
        private int BounceCount => (int)Projectile.ai[0];
        private float DamageMultiplier
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private float _pulseTimer;
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
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1; // Bounces reduce this manually
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            // Initialize damage multiplier
            if (DamageMultiplier < 0.01f)
                DamageMultiplier = 1f;

            _pulseTimer += 0.1f;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Gentle homing toward nearest enemy
            NPC target = FindClosestNPC(400f);
            if (target != null)
            {
                Vector2 idealDir = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX));
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealDir * Projectile.velocity.Length(), 0.04f);
            }

            // Thorn trail dust
            if (Main.rand.NextBool(3))
            {
                float bounceProgress = 1f - (BounceCount / 3f);
                Color dustCol = Color.Lerp(OdeToJoyPalette.LeafGreen, OdeToJoyPalette.GoldenPollen, bounceProgress);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.GreenTorch, -Projectile.velocity * 0.15f, 0, dustCol, 0.7f + bounceProgress * 0.4f);
                d.noGravity = true;
            }

            // Petal sparkles
            if (Main.rand.NextBool(6))
            {
                Dust petal = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch,
                    Main.rand.NextVector2Circular(1f, 1f), 100, OdeToJoyPalette.RosePink, 0.5f);
                petal.noGravity = true;
            }

            // Lighting - increases with damage multiplier
            float intensity = 0.3f + (DamageMultiplier - 1f) * 0.1f;
            Color lightCol = Color.Lerp(OdeToJoyPalette.LeafGreen, OdeToJoyPalette.GoldenPollen, (DamageMultiplier - 1f) / 0.6f);
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * intensity);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return HandleBounce(oldVelocity, null);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Check thorn convergence
            Player owner = Main.player[Projectile.owner];
            var combat = owner.GetModPlayer<OdeToJoyCombatPlayer>();
            bool convergence = combat.TrackThornConvergence(target.whoAmI);

            if (convergence)
            {
                // Bonus convergence VFX
                SoundEngine.PlaySound(SoundID.Item73 with { Pitch = 0.2f, Volume = 0.6f }, target.Center);
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                    Color col = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.WhiteBloom, Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.GoldFlame, vel, 0, col, 1.1f);
                    d.noGravity = true;
                }
            }

            // Bounce off enemy
            HandleBounce(Projectile.velocity, target);

            // Impact VFX
            OdeToJoyVFXLibrary.SpawnRadialDustBurst(target.Center, 8, 4f);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Apply damage multiplier from bounces
            modifiers.FinalDamage *= DamageMultiplier;

            // Check thorn convergence for bonus
            Player owner = Main.player[Projectile.owner];
            var combat = owner.GetModPlayer<OdeToJoyCombatPlayer>();
            if (combat.ThornConvergenceHits >= 2 && combat.ThornConvergenceTargetWhoAmI == target.whoAmI)
            {
                modifiers.FinalDamage *= 1.25f; // 25% convergence bonus
            }
        }

        private bool HandleBounce(Vector2 oldVelocity, NPC hitNPC)
        {
            if (BounceCount <= 0)
                return true; // Kill projectile

            // Decrement bounce count
            Projectile.ai[0]--;

            // Increase damage by 20% (compounding)
            DamageMultiplier *= 1.2f;

            // Bounce VFX
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.3f + (3 - BounceCount) * 0.15f, Volume = 0.5f }, Projectile.Center);
            for (int i = 0; i < 6; i++)
            {
                Color col = Color.Lerp(OdeToJoyPalette.LeafGreen, OdeToJoyPalette.GoldenPollen, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch,
                    Main.rand.NextVector2Circular(4f, 4f), 0, col, 0.9f);
                d.noGravity = true;
            }

            if (hitNPC != null)
            {
                // Bounce off enemy - reflect away from them
                Vector2 awayDir = (Projectile.Center - hitNPC.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = awayDir * oldVelocity.Length() * 0.9f;
                // Add some randomness
                Projectile.velocity = Projectile.velocity.RotatedByRandom(0.3f);
            }
            else
            {
                // Bounce off tile
                if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                    Projectile.velocity.X = -oldVelocity.X;
                if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                    Projectile.velocity.Y = -oldVelocity.Y;
            }

            return false; // Don't kill projectile
        }

        private NPC FindClosestNPC(float maxDist)
        {
            NPC closest = null;
            float closestDist = maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Death burst
            for (int i = 0; i < 6; i++)
            {
                Color col = Color.Lerp(OdeToJoyPalette.LeafGreen, OdeToJoyPalette.GoldenPollen, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch,
                    Main.rand.NextVector2Circular(3f, 3f), 0, col, 0.7f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.OdeToJoy, ref _strip);

            // Bounce progress overlay: golden ring brightens with each bounce
            float bounceProgress = 1f - (BounceCount / 3f);
            if (bounceProgress > 0.01f)
            {
                SpriteBatch sb = Main.spriteBatch;
                try
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                        Main.DefaultSamplerState, DepthStencilState.None,
                        RasterizerState.CullCounterClockwise, null,
                        Main.GameViewMatrix.TransformationMatrix);

                    Texture2D glow = ModContent.Request<Texture2D>(
                        "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    Vector2 origin = glow.Size() / 2f;
                    Vector2 drawPos = Projectile.Center - Main.screenPosition;
                    float pulse = 0.9f + 0.1f * MathF.Sin(_pulseTimer * 2f);

                    Color bounceCol = Color.Lerp(OdeToJoyPalette.BudGreen, OdeToJoyPalette.GoldenPollen, bounceProgress);
                    float bounceRing = (0.32f + bounceProgress * 0.15f) * pulse * Projectile.scale;
                    sb.Draw(glow, drawPos, null, (bounceCol with { A = 0 }) * (0.30f + bounceProgress * 0.25f),
                        _pulseTimer * 0.5f, origin, bounceRing, SpriteEffects.None, 0f);
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
    }
}
