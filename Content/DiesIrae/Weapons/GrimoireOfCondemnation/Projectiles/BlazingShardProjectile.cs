using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation.Projectiles
{
    /// <summary>
    /// Condemnation Wave ? sweepable channeled beam projectile.
    /// Widens over channel time (80��120px). Every 7th cast: Page Turn = +30% damage.
    /// Kills leave "condemned" name power (+5% per name, max 10 = +50%).
    /// </summary>
    public class BlazingShardProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private float ChannelTimer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private float BeamAngle;
        private const float MaxChannelTime = 180f; // 3s
        private const float BeamRange = 600f; // 10 tiles
        private const float AimSpeed = 0.12f; // rad/frame

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 200;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // Stay centered on player while channeling
            Projectile.Center = owner.Center;
            ChannelTimer++;

            // Aim toward cursor
            if (Main.myPlayer == Projectile.owner)
            {
                float targetAngle = (Main.MouseWorld - owner.Center).ToRotation();
                BeamAngle = MathHelper.Lerp(BeamAngle, targetAngle, 0.15f);
                Projectile.rotation = BeamAngle;

                // Keep alive while mouse held
                if (owner.channel)
                    Projectile.timeLeft = 10;
            }

            // Update player animation
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            owner.direction = Math.Cos(BeamAngle) >= 0 ? 1 : -1;

            // Channel progress 0-1
            float progress = Math.Min(ChannelTimer / MaxChannelTime, 1f);

            // Beam-end position
            Vector2 beamDir = BeamAngle.ToRotationVector2();
            Vector2 beamEnd = owner.Center + beamDir * BeamRange;

            // Hit enemies along beam
            float beamWidth = 16f + 16f * progress; // Widens with channel
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;

                // Point-to-line distance check
                float dist = DistanceToLine(npc.Center, owner.Center, beamEnd);
                if (dist < beamWidth + npc.width / 2f)
                {
                    // Within beam line range check
                    float dot = Vector2.Dot(npc.Center - owner.Center, beamDir);
                    if (dot > 0 && dot < BeamRange)
                    {
                        npc.AddBuff(BuffID.OnFire3, 120);
                    }
                }
            }

            // Beam fire dust
            if (!Main.dedServ && ChannelTimer % 3 == 0)
            {
                float dustDist = Main.rand.NextFloat(BeamRange * 0.8f);
                Vector2 dustPos = owner.Center + beamDir * dustDist;
                Vector2 perp = new Vector2(-beamDir.Y, beamDir.X) * Main.rand.NextFloat(-beamWidth, beamWidth);
                Dust d = Dust.NewDustPerfect(dustPos + perp, DustID.Torch,
                    beamDir * 2f + Main.rand.NextVector2Circular(1f, 1f), 0, default, 1f + progress);
                d.noGravity = true;
            }

            // Lighting along beam
            for (int i = 0; i < 5; i++)
            {
                Vector2 lightPos = Vector2.Lerp(owner.Center, beamEnd, (float)i / 4f);
                Lighting.AddLight(lightPos, 0.8f * progress, 0.2f * progress, 0.05f);
            }
        }

        private float DistanceToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            float lineLen = line.Length();
            if (lineLen < 0.01f) return Vector2.Distance(point, lineStart);
            Vector2 lineDir = line / lineLen;
            float t = MathHelper.Clamp(Vector2.Dot(point - lineStart, lineDir), 0f, lineLen);
            Vector2 closest = lineStart + lineDir * t;
            return Vector2.Distance(point, closest);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Custom beam collision
            Player owner = Main.player[Projectile.owner];
            Vector2 beamDir = BeamAngle.ToRotationVector2();
            Vector2 beamEnd = owner.Center + beamDir * BeamRange;
            float progress = Math.Min(ChannelTimer / MaxChannelTime, 1f);
            float beamWidth = 16f + 16f * progress;

            float dist = DistanceToLine(targetHitbox.Center.ToVector2(), owner.Center, beamEnd);
            float dot = Vector2.Dot(targetHitbox.Center.ToVector2() - owner.Center, beamDir);
            return dist < beamWidth + targetHitbox.Width / 2f && dot > 0 && dot < BeamRange;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Player owner = Main.player[Projectile.owner];
            float progress = Math.Min(ChannelTimer / MaxChannelTime, 1f);
            Vector2 beamDir = BeamAngle.ToRotationVector2();
            Vector2 beamEnd = owner.Center + beamDir * BeamRange;

            // Draw condemnation beam
            GrimoireOfCondemnationUtils.DrawCondemnationBeam(sb, owner.Center, beamEnd, progress, ChannelTimer);

            // Dies Irae theme accent layer
            GrimoireOfCondemnationUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Dark Sermon Sigil ? ritual circle projectile (alt-fire).
    /// Builds over 3s of channeling, then detonates dealing massive damage inside circle.
    /// </summary>
    public class DarkSermonSigilProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private float BuildTimer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private const float BuildTime = 180f; // 3s to complete
        private const float CircleRadius = 160f; // 10 tiles

        public override void SetDefaults()
        {
            Projectile.width = 320;
            Projectile.height = 320;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.velocity = Vector2.Zero;
            BuildTimer++;

            float buildProgress = Math.Min(BuildTimer / BuildTime, 1f);

            // Keep alive while building
            if (BuildTimer < BuildTime)
            {
                Projectile.timeLeft = Math.Max(Projectile.timeLeft, 30);

                // Particle buildup during channeling
                if (!Main.dedServ && Main.rand.NextBool(3))
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float radius = CircleRadius * buildProgress;
                    Vector2 dustPos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch,
                        (Projectile.Center - dustPos).SafeNormalize(Vector2.Zero) * 1f, 0, default, 0.8f);
                    d.noGravity = true;
                }
            }
            else if (BuildTimer == (int)BuildTime)
            {
                // DETONATION!
                GrimoireOfCondemnationUtils.DoSermonDetonation(Projectile.Center, CircleRadius);

                // Damage all enemies inside circle
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                    {
                        if (Vector2.Distance(Projectile.Center, npc.Center) < CircleRadius)
                        {
                            npc.AddBuff(BuffID.OnFire3, 300);
                        }
                    }
                }

                Projectile.timeLeft = 30; // Fade out after detonation
            }

            // Lighting during build
            Lighting.AddLight(Projectile.Center, 0.6f * buildProgress, 0.15f * buildProgress, 0.05f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Only deal direct damage at detonation moment
            if (BuildTimer >= BuildTime - 5 && BuildTimer <= BuildTime + 5)
            {
                float dist = Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2());
                return dist < CircleRadius;
            }
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float buildProgress = Math.Min(BuildTimer / BuildTime, 1f);

            // Draw sermon circle
            GrimoireOfCondemnationUtils.DrawSermonCircle(sb, Projectile.Center, CircleRadius * buildProgress, buildProgress, BuildTimer);

            // Post-detonation fade glow
            if (BuildTimer > BuildTime)
            {
                float fadeProgress = 1f - (BuildTimer - BuildTime) / 30f;
                if (fadeProgress > 0)
                {
                    Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                    if (glow != null)
                    {
                        Vector2 origin = glow.Size() / 2f;
                        Vector2 pos = Projectile.Center - Main.screenPosition;
                        sb.Draw(glow, pos, null, DiesIraePalette.JudgmentGold * 0.4f * fadeProgress, 0f, origin,
                            MathHelper.Min(CircleRadius / glow.Width * 4f, 0.586f), SpriteEffects.None, 0f);
                    }
                }
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}