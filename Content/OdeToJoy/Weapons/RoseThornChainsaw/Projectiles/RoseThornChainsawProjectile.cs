using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Projectiles
{
    // ═══════════════════════════════════════════════════
    //  CHAINSAW HOLDOUT — the spinning blade the player holds
    // ═══════════════════════════════════════════════════

    /// <summary>
    /// Held projectile for the Rose Thorn Chainsaw.
    /// Stays in front of the player, spinning, spawning ThornChainProjectiles
    /// and dense particle trails each frame.
    /// </summary>
    public class ChainsawHoldoutProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        /// <summary>Frame counter for thorn-chain spawn cadence.</summary>
        private int chainTimer;
        private const int ChainInterval = 12;

        /// <summary>Frame counter for venom mist cadence.</summary>
        private int mistTimer;

        /// <summary>Number of "teeth" drawn around the blade ring.</summary>
        private const int TeethCount = 8;

        /// <summary>Radius of the spinning tooth ring (pixels).</summary>
        private const float TeethRadius = 22f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
        }

        // ── HOLDOUT AI ──

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // Kill when player stops channeling
            if (!player.channel || player.dead || !player.active)
            {
                Projectile.Kill();
                return;
            }

            // Keep the item in use and force direction
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.heldProj = Projectile.whoAmI;

            // Aim toward cursor
            Vector2 aim = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            player.ChangeDir(aim.X >= 0f ? 1 : -1);

            // Position in front of the player at a fixed offset
            float holdDist = 50f;
            Projectile.Center = player.MountedCenter + aim * holdDist;
            Projectile.velocity = Vector2.Zero;

            // Spin!
            Projectile.rotation += 0.5f;

            // ── Spawn thorn chain projectile periodically ──
            chainTimer++;
            if (chainTimer >= ChainInterval)
            {
                chainTimer = 0;

                if (Main.myPlayer == Projectile.owner)
                {
                    float spread = MathHelper.ToRadians(Main.rand.NextFloat(-10f, 10f));
                    Vector2 chainDir = aim.RotatedBy(spread);
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        chainDir * 18f,
                        ModContent.ProjectileType<ThornChainProjectile>(),
                        Projectile.damage / 3,
                        Projectile.knockBack * 0.4f,
                        Projectile.owner);
                }
            }

            // ── Per-frame particles (client only) ──
            if (!Main.dedServ)
            {
                // ThornSpark at the "teeth" tip
                Vector2 teethTip = Projectile.Center + aim * TeethRadius;
                ThornParticleHandler.Spawn(new ThornSpark(
                    teethTip + Main.rand.NextVector2Circular(4f, 4f),
                    aim.RotatedByRandom(0.6f) * Main.rand.NextFloat(2f, 5f),
                    Color.Lerp(RoseThornChainsawUtils.VerdantGreen, RoseThornChainsawUtils.GoldenPollen, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.25f, 0.45f),
                    Main.rand.Next(10, 20)));

                // VenomMist every 3 frames
                mistTimer++;
                if (mistTimer >= 3)
                {
                    mistTimer = 0;
                    ThornParticleHandler.Spawn(new VenomMist(
                        Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                        -aim * 0.4f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                        Main.rand.NextFloat(0.3f, 0.5f),
                        Main.rand.Next(25, 40)));
                }

                // Occasional pollen sparkle
                if (Main.rand.NextBool(2))
                {
                    ThornParticleHandler.Spawn(new PollenSparkle(
                        Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                        Main.rand.NextVector2Circular(1.5f, 1.5f),
                        Main.rand.NextFloat(0.2f, 0.35f),
                        Main.rand.Next(15, 25)));
                }
            }

            Lighting.AddLight(Projectile.Center, RoseThornChainsawUtils.VerdantGreen.ToVector3() * 0.9f);
        }

        // ── COMBAT ──

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 300);
            target.AddBuff(BuffID.Venom, 180);

            // Crit bonus: extra thorn chain
            if (hit.Crit && Main.myPlayer == Projectile.owner)
            {
                Vector2 dir = (target.Center - Main.player[Projectile.owner].MountedCenter).SafeNormalize(Vector2.UnitX);
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    dir.RotatedByRandom(0.3f) * 14f,
                    ModContent.ProjectileType<ThornChainProjectile>(),
                    Projectile.damage / 4,
                    2f,
                    Projectile.owner);
            }

            // Hit VFX
            if (!Main.dedServ)
            {
                for (int i = 0; i < 6; i++)
                {
                    ThornParticleHandler.Spawn(new ThornSpark(
                        target.Center + Main.rand.NextVector2Circular(8f, 8f),
                        Main.rand.NextVector2Circular(4f, 4f),
                        Color.Lerp(RoseThornChainsawUtils.VerdantGreen, RoseThornChainsawUtils.RosePink, Main.rand.NextFloat()),
                        Main.rand.NextFloat(0.3f, 0.5f),
                        Main.rand.Next(10, 18)));
                }

                for (int i = 0; i < 3; i++)
                {
                    ThornParticleHandler.Spawn(new RosePetal(
                        target.Center + Main.rand.NextVector2Circular(12f, 12f),
                        Main.rand.NextVector2Circular(2f, 2f) - Vector2.UnitY,
                        RoseThornChainsawUtils.RosePink,
                        Main.rand.NextFloat(0.3f, 0.5f),
                        Main.rand.Next(30, 50)));
                }

                ThornParticleHandler.Spawn(new ThornBloom(
                    target.Center,
                    RoseThornChainsawUtils.VerdantGreen,
                    Main.rand.NextFloat(0.4f, 0.6f),
                    Main.rand.Next(12, 20)));
            }
        }

        // ── CUSTOM RENDERING ──

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D bloom = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 bloomOrigin = bloom.Size() / 2f;
            float time = (float)Main.GameUpdateCount / 60f;

            sb.End();

            // ═══ Layer 1: VerdantSlash — buzzing chainsaw aura ═══
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Effect slashShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyVerdantSlashShader);
            if (slashShader != null)
            {
                slashShader.Parameters["uTime"]?.SetValue(time * 3f);
                slashShader.Parameters["uColor"]?.SetValue(RoseThornChainsawUtils.VerdantGreen.ToVector3());
                slashShader.Parameters["uSecondaryColor"]?.SetValue(RoseThornChainsawUtils.GoldenPollen.ToVector3());
                slashShader.Parameters["uOpacity"]?.SetValue(0.4f);
                slashShader.Parameters["uIntensity"]?.SetValue(1.5f);
                slashShader.Parameters["uRadius"]?.SetValue(0.5f);
                slashShader.CurrentTechnique = slashShader.Techniques["VerdantSlashTechnique"];
                slashShader.CurrentTechnique.Passes[0].Apply();

                sb.Draw(bloom, drawPos, null, Color.White,
                    Projectile.rotation, bloomOrigin, 1.6f, SpriteEffects.None, 0f);
            }

            sb.End();

            // ═══ Layer 2: Additive glow + teeth ═══
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer verdant glow (large, faint)
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f;
            sb.Draw(bloom, drawPos, null,
                RoseThornChainsawUtils.Additive(RoseThornChainsawUtils.VerdantGreen, 0.35f * pulse),
                0f, bloomOrigin, 1.2f * pulse, SpriteEffects.None, 0f);

            // Inner golden core
            sb.Draw(bloom, drawPos, null,
                RoseThornChainsawUtils.Additive(RoseThornChainsawUtils.GoldenPollen, 0.5f),
                0f, bloomOrigin, 0.6f, SpriteEffects.None, 0f);

            // Spinning "blade teeth" — small bloom circles arranged in a ring
            for (int i = 0; i < TeethCount; i++)
            {
                float angle = Projectile.rotation + MathHelper.TwoPi * i / TeethCount;
                Vector2 toothPos = drawPos + angle.ToRotationVector2() * TeethRadius;

                // Each tooth is a small bright circle
                Color toothColor = RoseThornChainsawUtils.GetChainColor((float)i / TeethCount);
                sb.Draw(bloom, toothPos, null,
                    RoseThornChainsawUtils.Additive(toothColor, 0.8f),
                    angle, bloomOrigin, 0.25f, SpriteEffects.None, 0f);
            }

            // Hot white center pinpoint
            sb.Draw(bloom, drawPos, null,
                RoseThornChainsawUtils.Additive(Color.White, 0.6f),
                0f, bloomOrigin, 0.2f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    // ═══════════════════════════════════════════════════
    //  THORN CHAIN PROJECTILE — bouncy thorn shrapnel
    // ═══════════════════════════════════════════════════

    /// <summary>
    /// Gravity-affected thorn shard that bounces off tiles and pierces enemies.
    /// Spawns particle bursts on hit and on death.
    /// </summary>
    public class ThornChainProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        /// <summary>Ring buffer for trail rendering.</summary>
        private const int TrailLength = 8;
        private readonly Vector2[] trailPositions = new Vector2[TrailLength];
        private int trailIndex;
        private bool trailFilled;

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 6;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        // ── AI: gravity + spin ──

        public override void AI()
        {
            Projectile.velocity.Y += 0.15f;
            Projectile.rotation += Projectile.velocity.Length() * 0.05f;

            // Record trail position
            trailPositions[trailIndex] = Projectile.Center;
            trailIndex = (trailIndex + 1) % TrailLength;
            if (trailIndex == 0) trailFilled = true;

            // Trail sparks
            if (!Main.dedServ && Main.rand.NextBool(2))
            {
                ThornParticleHandler.Spawn(new ThornSpark(
                    Projectile.Center,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f),
                    Color.Lerp(RoseThornChainsawUtils.VerdantGreen, RoseThornChainsawUtils.GoldenPollen, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.15f, 0.3f),
                    Main.rand.Next(8, 15)));
            }

            Lighting.AddLight(Projectile.Center, RoseThornChainsawUtils.VerdantGreen.ToVector3() * 0.35f);
        }

        // ── BOUNCE ──

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.penetrate--;
            if (Projectile.penetrate <= 0)
            {
                Projectile.Kill();
                return false;
            }

            // Bounce physics
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X * 0.6f;
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y * 0.6f;

            Projectile.velocity.X *= 0.95f;

            // Bounce sparks
            if (!Main.dedServ)
            {
                SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.4f, Pitch = 0.5f }, Projectile.Center);

                for (int i = 0; i < 4; i++)
                {
                    ThornParticleHandler.Spawn(new ThornSpark(
                        Projectile.Center,
                        Main.rand.NextVector2Circular(3f, 3f),
                        RoseThornChainsawUtils.ThornGreen,
                        Main.rand.NextFloat(0.2f, 0.35f),
                        Main.rand.Next(8, 14)));
                }
            }

            return false;
        }

        // ── ENEMY HIT ──

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 120);

            if (!Main.dedServ)
            {
                // Rose petals
                for (int i = 0; i < 4; i++)
                {
                    ThornParticleHandler.Spawn(new RosePetal(
                        target.Center + Main.rand.NextVector2Circular(10f, 10f),
                        Main.rand.NextVector2Circular(2.5f, 2.5f) - Vector2.UnitY * 0.8f,
                        RoseThornChainsawUtils.RosePink,
                        Main.rand.NextFloat(0.25f, 0.4f),
                        Main.rand.Next(30, 50)));
                }

                // Pollen sparkle at impact
                ThornParticleHandler.Spawn(new PollenSparkle(
                    target.Center,
                    Main.rand.NextVector2Circular(1f, 1f),
                    Main.rand.NextFloat(0.3f, 0.45f),
                    Main.rand.Next(15, 25)));
            }
        }

        // ── DEATH ──

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // 3 ThornBloom particles
            for (int i = 0; i < 3; i++)
            {
                Color bloomColor = RoseThornChainsawUtils.MulticolorLerp(
                    Main.rand.NextFloat(),
                    RoseThornChainsawUtils.VerdantGreen,
                    RoseThornChainsawUtils.RosePink,
                    RoseThornChainsawUtils.GoldenPollen);

                ThornParticleHandler.Spawn(new ThornBloom(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    bloomColor,
                    Main.rand.NextFloat(0.3f, 0.55f),
                    Main.rand.Next(15, 25)));
            }

            // Final spark burst
            for (int i = 0; i < 5; i++)
            {
                ThornParticleHandler.Spawn(new ThornSpark(
                    Projectile.Center,
                    Main.rand.NextVector2Circular(4f, 4f),
                    RoseThornChainsawUtils.VerdantGreen,
                    Main.rand.NextFloat(0.2f, 0.35f),
                    Main.rand.Next(10, 18)));
            }

            SoundEngine.PlaySound(SoundID.Grass with { Volume = 0.5f, Pitch = 0.3f }, Projectile.Center);
        }

        // ── CUSTOM RENDERING ──

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D bloom = TextureAssets.Projectile[Type].Value;
            Vector2 bloomOrigin = bloom.Size() / 2f;
            float time = (float)Main.GameUpdateCount / 60f;

            sb.End();

            // ═══ Layer 1: TriumphantTrail — verdant energy trail wake ═══
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Effect trailShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyTriumphantTrailShader);
            int count = trailFilled ? TrailLength : trailIndex;
            if (trailShader != null && count > 2)
            {
                trailShader.Parameters["uTime"]?.SetValue(time);
                trailShader.Parameters["uColor"]?.SetValue(RoseThornChainsawUtils.VerdantGreen.ToVector3());
                trailShader.Parameters["uSecondaryColor"]?.SetValue(RoseThornChainsawUtils.ThornGreen.ToVector3());
                trailShader.Parameters["uOpacity"]?.SetValue(0.45f);
                trailShader.Parameters["uIntensity"]?.SetValue(1.2f);
                trailShader.CurrentTechnique = trailShader.Techniques["TriumphantTrailTechnique"];
                trailShader.CurrentTechnique.Passes[0].Apply();

                // Draw trail segments with shader
                for (int i = 1; i < count; i++)
                {
                    int curIdx = (trailIndex - count + i + TrailLength) % TrailLength;
                    Vector2 cur = trailPositions[curIdx] - Main.screenPosition;
                    float progress = (float)i / count;
                    sb.Draw(bloom, cur, null, Color.White * (progress * 0.6f),
                        0f, bloomOrigin, 0.2f * progress, SpriteEffects.None, 0f);
                }
            }

            sb.End();

            // ═══ Layer 2: Additive trail dots + connecting lines + core glow ═══
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // ── Trail ──
            for (int i = 1; i < count; i++)
            {
                // Read positions in chronological order
                int curIdx = (trailIndex - count + i + TrailLength) % TrailLength;
                int prevIdx = (curIdx - 1 + TrailLength) % TrailLength;
                Vector2 cur = trailPositions[curIdx] - Main.screenPosition;
                Vector2 prev = trailPositions[prevIdx] - Main.screenPosition;

                float progress = (float)i / count;
                float alpha = progress * 0.5f;
                Color trailColor = Color.Lerp(RoseThornChainsawUtils.DeepThorn,
                    RoseThornChainsawUtils.VerdantGreen, progress);

                // Small bloom dot for each trail segment
                sb.Draw(bloom, cur, null,
                    RoseThornChainsawUtils.Additive(trailColor, alpha),
                    0f, bloomOrigin, 0.15f * progress, SpriteEffects.None, 0f);

                // Thin connecting line via stretched pixel
                Vector2 diff = cur - prev;
                float len = diff.Length();
                if (len > 1f)
                {
                    Texture2D px = MagnumTextureRegistry.GetPointBloom();
                    if (px == null) continue;
                    sb.Draw(px, prev, new Rectangle(0, 0, 1, 1),
                        RoseThornChainsawUtils.Additive(trailColor, alpha * 0.5f),
                        diff.ToRotation(), new Vector2(0f, 0.5f),
                        new Vector2(len, 2f), SpriteEffects.None, 0f);
                }
            }

            // ── Core glow ──
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.15f;

            // Outer verdant glow
            sb.Draw(bloom, drawPos, null,
                RoseThornChainsawUtils.Additive(RoseThornChainsawUtils.VerdantGreen, 0.5f * pulse),
                0f, bloomOrigin, 0.55f * pulse, SpriteEffects.None, 0f);

            // Darker green core
            sb.Draw(bloom, drawPos, null,
                RoseThornChainsawUtils.Additive(RoseThornChainsawUtils.ThornGreen, 0.7f),
                0f, bloomOrigin, 0.25f, SpriteEffects.None, 0f);

            // White pinpoint
            sb.Draw(bloom, drawPos, null,
                RoseThornChainsawUtils.Additive(Color.White, 0.4f),
                0f, bloomOrigin, 0.1f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
