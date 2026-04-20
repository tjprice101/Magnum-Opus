using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Generic configurable stationary damage zone for use across all themes.
    /// Configurable via ai[] and localAI[]:
    ///
    /// ai[0] = Mode flags (bitfield):
    ///         Bit 0 (1):   Pull enemies toward center
    ///         Bit 1 (2):   Slow enemies
    ///         Bit 2 (4):   Speed up ally projectiles passing through
    ///         Bit 3 (8):   Spawn children on expiry
    /// ai[1] = Radius in pixels (default 80 if 0)
    /// localAI[0] = Theme index (same as GenericHomingOrbChild)
    /// localAI[1] = Duration in frames (default 120 if 0)
    /// </summary>
    public class GenericDamageZone : ModProjectile
    {
        public const int FLAG_PULL = 1;
        public const int FLAG_SLOW = 2;
        public const int FLAG_SPEED_ALLIES = 4;
        public const int FLAG_SPAWN_CHILDREN = 8;

        private int _timer;
        private float _seed;
        private int _duration;
        private float _radius;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 500;
        }

        public override void SetDefaults()
        {
            Projectile.width = 160;
            Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.timeLeft = 600; // Will be overridden
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.hide = false;
        }

        public override void AI()
        {
            if (_timer == 0)
            {
                _seed = Main.rand.NextFloat(100f);
                _duration = (int)Projectile.localAI[1];
                if (_duration <= 0) _duration = 120;
                Projectile.timeLeft = _duration;

                _radius = Projectile.ai[1];
                if (_radius <= 0) _radius = 80f;

                Projectile.width = (int)(_radius * 2);
                Projectile.height = (int)(_radius * 2);
                Projectile.position = Projectile.Center - new Vector2(_radius, _radius);
            }

            _timer++;
            Projectile.velocity = Vector2.Zero;

            int flags = (int)Projectile.ai[0];
            float alpha = GetAlphaMultiplier();

            // Pull enemies
            if ((flags & FLAG_PULL) != 0)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < _radius * 1.5f && dist > 10f)
                    {
                        Vector2 pullDir = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                        npc.velocity += pullDir * 1.5f * alpha;
                    }
                }
            }

            // Slow enemies
            if ((flags & FLAG_SLOW) != 0)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < _radius)
                        npc.velocity *= 0.92f;
                }
            }

            // Speed up ally projectiles
            if ((flags & FLAG_SPEED_ALLIES) != 0)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    var proj = Main.projectile[i];
                    if (!proj.active || !proj.friendly || proj.whoAmI == Projectile.whoAmI) continue;
                    if (proj.type == Projectile.type) continue; // Don't affect other zones
                    float dist = Vector2.Distance(Projectile.Center, proj.Center);
                    if (dist < _radius)
                    {
                        float speedBoost = 1.005f; // Gentle per-frame boost
                        proj.velocity *= speedBoost;
                    }
                }
            }

            // Lighting
            Color themeColor = GetThemeColor();
            float pulse = 0.7f + 0.3f * MathF.Sin((float)Main.timeForVisualEffects * 0.05f + _seed);
            Lighting.AddLight(Projectile.Center, themeColor.ToVector3() * 0.4f * alpha * pulse);

            // Ambient dust
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float r = Main.rand.NextFloat(_radius * 0.9f);
                Vector2 dustPos = Projectile.Center + angle.ToRotationVector2() * r;
                Dust d = Dust.NewDustPerfect(dustPos, DustID.RainbowMk2,
                    new Vector2(0, -0.5f), 0, themeColor, 0.4f);
                d.noGravity = true;
                d.fadeIn = 0.3f;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Temporal Paradox: if a Forward (FLAG_SPEED_ALLIES) and Reverse (FLAG_SLOW) zone overlap,
            // enemies in the overlap area take 2x damage
            int myFlags = (int)Projectile.ai[0];
            bool isForward = (myFlags & FLAG_SPEED_ALLIES) != 0;
            bool isReverse = (myFlags & FLAG_SLOW) != 0;
            if (!isForward && !isReverse) return;

            int oppositeFlag = isForward ? FLAG_SLOW : FLAG_SPEED_ALLIES;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.whoAmI == Projectile.whoAmI || p.type != Projectile.type || p.owner != Projectile.owner) continue;
                int otherFlags = (int)p.ai[0];
                if ((otherFlags & oppositeFlag) == 0) continue;

                // Check if target is within both zones' radii
                float otherRadius = p.ai[1] > 0 ? p.ai[1] : 80f;
                if (Vector2.Distance(target.Center, p.Center) < otherRadius)
                {
                    modifiers.FinalDamage *= 2f;
                    return;
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            int flags = (int)Projectile.ai[0];

            // Spawn children on expiry
            if ((flags & FLAG_SPAWN_CHILDREN) != 0)
            {
                int themeIndex = (int)Projectile.localAI[0];
                for (int i = 0; i < 3; i++)
                {
                    float angle = MathHelper.TwoPi / 3f * i + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 vel = angle.ToRotationVector2() * 8f;

                    NPC target = FindClosestNPC(400f);
                    if (target != null)
                        vel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 10f
                              + Main.rand.NextVector2Circular(2f, 2f);

                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center, vel,
                        Projectile.damage, Projectile.knockBack, Projectile.owner,
                        homingStrength: 0.06f, behaviorFlags: 0, themeIndex: themeIndex);
                }
            }

            // Death burst
            if (!Main.dedServ)
            {
                Color col = GetThemeColor();
                for (int i = 0; i < 6; i++)
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(_radius * 0.5f, _radius * 0.5f),
                        DustID.RainbowMk2, Main.rand.NextVector2Circular(3f, 3f), 0, col, 0.5f);
                    d.noGravity = true;
                }
            }
        }

        private float GetAlphaMultiplier()
        {
            float fadeIn = MathHelper.Clamp(_timer / 15f, 0f, 1f);
            float fadeOut = Projectile.timeLeft < 30 ? Projectile.timeLeft / 30f : 1f;
            return fadeIn * fadeOut;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 closestPoint = new Vector2(
                MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right),
                MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
            return Vector2.Distance(Projectile.Center, closestPoint) <= _radius;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float alpha = GetAlphaMultiplier();
                Color col = GetThemeColor();
                float pulse = 0.85f + 0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.04f + _seed);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                Texture2D glow = ModContent.Request<Microsoft.Xna.Framework.Graphics.Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Vector2 origin = glow.Size() / 2f;

                // Outer glow
                float glowScale = _radius / (glow.Width * 0.5f) * 1.2f * pulse;
                sb.Draw(glow, drawPos, null, col * (0.15f * alpha), 0f, origin, glowScale, SpriteEffects.None, 0f);

                // Inner glow
                sb.Draw(glow, drawPos, null, col * (0.25f * alpha), 0f, origin, glowScale * 0.6f, SpriteEffects.None, 0f);

                // Core
                sb.Draw(glow, drawPos, null, Color.White * (0.3f * alpha), 0f, origin, glowScale * 0.3f, SpriteEffects.None, 0f);
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

        private NPC FindClosestNPC(float maxDist)
        {
            NPC closest = null;
            float closestDist = maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist) { closestDist = dist; closest = npc; }
            }
            return closest;
        }

        private Color GetThemeColor()
        {
            int themeIndex = (int)Projectile.localAI[0];
            var config = themeIndex switch
            {
                0 => IncisorOrbRenderer.Eroica,
                1 => IncisorOrbRenderer.LaCampanella,
                2 => IncisorOrbRenderer.Enigma,
                3 => IncisorOrbRenderer.SwanLake,
                4 => IncisorOrbRenderer.Fate,
                5 => IncisorOrbRenderer.Nachtmusik,
                6 => IncisorOrbRenderer.DiesIrae,
                7 => IncisorOrbRenderer.OdeToJoy,
                8 => IncisorOrbRenderer.ClairDeLune,
                9 => IncisorOrbRenderer.Spring,
                10 => IncisorOrbRenderer.Summer,
                11 => IncisorOrbRenderer.Autumn,
                12 => IncisorOrbRenderer.Winter,
                _ => IncisorOrbRenderer.DiesIrae,
            };
            Vector3 v = config.LightColor;
            return new Color(v.X, v.Y, v.Z);
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;

        // =====================================================================
        // STATIC HELPER for spawning themed zones
        // =====================================================================

        /// <summary>
        /// Spawns a themed damage zone at the given position.
        /// </summary>
        public static int SpawnZone(
            Terraria.DataStructures.IEntitySource source,
            Vector2 position, int damage, float knockback, int owner,
            int modeFlags, float radius, int themeIndex,
            int durationFrames = 120)
        {
            int projType = ModContent.ProjectileType<GenericDamageZone>();
            int idx = Terraria.Projectile.NewProjectile(source, position, Vector2.Zero, projType,
                damage, knockback, owner, ai0: modeFlags, ai1: radius);
            if (idx >= 0 && idx < Main.maxProjectiles)
            {
                Main.projectile[idx].localAI[0] = themeIndex;
                Main.projectile[idx].localAI[1] = durationFrames;
            }
            return idx;
        }
    }
}
