using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.CameraModifiers;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Particles;
using MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Projectiles
{
    // ═══════════════════════════════════════════════════════════
    // GardenerFuryProjectile — Custom rapier thrust projectile
    // Extends outward then retracts. Follows player, faces cursor.
    // ai[0] = combo stacks at time of fire (for rendering intensity)
    // Spawns RapierSparkParticle at tip each frame.
    // On hit: increments combo, spawns petals at 5+, celebration at 10 + crit.
    // ═══════════════════════════════════════════════════════════
    public class GardenerFuryProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;
        private static Asset<Texture2D> _softBloomTex;

        /// <summary>Maximum extension distance from player center in pixels.</summary>
        private const float MaxExtension = 100f;

        /// <summary>Total lifetime of the thrust animation in frames.</summary>
        private const int TotalDuration = 14;

        /// <summary>Frames spent extending outward.</summary>
        private const int ExtendFrames = 6;

        /// <summary>Frames spent holding at max extension.</summary>
        private const int HoldFrames = 2;

        /// <summary>Direction of thrust, stored at spawn.</summary>
        private Vector2 thrustDirection;

        /// <summary>Whether the thrust direction has been initialized.</summary>
        private bool initialized;

        /// <summary>Current frame of the thrust animation.</summary>
        private ref float Timer => ref Projectile.localAI[0];

        /// <summary>Combo stacks at time of fire (for visual intensity).</summary>
        private int ComboAtFire => (int)Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = TotalDuration + 5;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.alpha = 255; // invisible by default — we draw custom
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // Initialize thrust direction on first frame
            if (!initialized)
            {
                thrustDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Zero; // We position manually
                initialized = true;
            }

            // Keep player animation and held projectile
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            owner.ChangeDir(thrustDirection.X >= 0f ? 1 : -1);

            // Calculate extension based on phase
            Timer++;
            float timer = Timer;
            float extensionProgress;

            if (timer <= ExtendFrames)
            {
                // Phase 1: Extend — fast ease-out
                float t = timer / ExtendFrames;
                extensionProgress = 1f - (1f - t) * (1f - t); // ease-out quad
            }
            else if (timer <= ExtendFrames + HoldFrames)
            {
                // Phase 2: Hold at max extension
                extensionProgress = 1f;
            }
            else if (timer <= TotalDuration)
            {
                // Phase 3: Retract — ease-in
                float t = (timer - ExtendFrames - HoldFrames) / (TotalDuration - ExtendFrames - HoldFrames);
                extensionProgress = 1f - t * t; // ease-in quad
            }
            else
            {
                Projectile.Kill();
                return;
            }

            // Position projectile tip at extension distance from player
            Vector2 tipOffset = thrustDirection * MaxExtension * extensionProgress;
            Projectile.Center = owner.MountedCenter + tipOffset;
            Projectile.rotation = thrustDirection.ToRotation();

            // Spawn RapierSparkParticle at tip each frame
            if (!Main.dedServ && extensionProgress > 0.2f)
            {
                Vector2 sparkVel = thrustDirection.RotatedByRandom(0.6f) * Main.rand.NextFloat(1.5f, 4f);
                var spark = new RapierSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    sparkVel,
                    Color.Lerp(GardenersUtils.JubilantGold, GardenersUtils.SunlightWhite, Main.rand.NextFloat(0.3f)),
                    Main.rand.NextFloat(0.3f, 0.55f),
                    Main.rand.Next(8, 18));
                GardenersParticleHandler.SpawnParticle(spark);

                // Combo glow particle at higher stacks
                if (ComboAtFire >= 3 && Main.rand.NextBool(3))
                {
                    var glow = new ComboGlowParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                        Main.rand.NextVector2Circular(0.5f, 0.5f),
                        ComboAtFire,
                        Main.rand.NextFloat(0.4f, 0.7f),
                        Main.rand.Next(12, 22));
                    GardenersParticleHandler.SpawnParticle(glow);
                }
            }

            // Lighting at tip
            float lightIntensity = 0.4f + ComboAtFire * 0.06f;
            Lighting.AddLight(Projectile.Center, lightIntensity, lightIntensity * 0.85f, lightIntensity * 0.2f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            var comboPlayer = owner.GetModPlayer<ComboStackPlayer>();
            int stacks = comboPlayer.ComboStacks;

            // ── At 5+ stacks: spawn 1–5 SmallPetalProjectile (1/3 damage) ──
            if (stacks >= 5)
            {
                int petalCount = Main.rand.Next(1, 6); // 1–5 petals
                int petalDamage = Math.Max(1, Projectile.damage / 3);

                for (int i = 0; i < petalCount; i++)
                {
                    Vector2 petalVel = Main.rand.NextVector2CircularEdge(5f, 5f) * Main.rand.NextFloat(0.6f, 1.2f);
                    Projectile.NewProjectile(
                        Projectile.GetSource_OnHit(target),
                        target.Center + Main.rand.NextVector2Circular(8f, 8f),
                        petalVel,
                        ModContent.ProjectileType<SmallPetalProjectile>(),
                        petalDamage,
                        Projectile.knockBack * 0.3f,
                        owner.whoAmI);
                }

                // Petal burst VFX
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var petal = new PetalBurstParticle(
                            target.Center + Main.rand.NextVector2Circular(12f, 12f),
                            Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0f, -0.5f),
                            Main.rand.NextFloat(0.25f, 0.5f),
                            Main.rand.Next(30, 55));
                        GardenersParticleHandler.SpawnParticle(petal);
                    }
                }
            }

            // ── At 10 stacks + crit: Triumphant Celebration ──
            if (stacks >= 10 && hit.Crit)
            {
                // 8 JubilantPetalProjectile in radial burst (full damage)
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi / 8f * i;
                    Vector2 burstVel = angle.ToRotationVector2() * 7f;
                    Projectile.NewProjectile(
                        Projectile.GetSource_OnHit(target),
                        target.Center,
                        burstVel,
                        ModContent.ProjectileType<JubilantPetalProjectile>(),
                        Projectile.damage,
                        Projectile.knockBack * 0.5f,
                        owner.whoAmI);
                }

                // Screen shake for the celebration
                if (Main.myPlayer == owner.whoAmI)
                {
                    PunchCameraModifier shake = new PunchCameraModifier(
                        target.Center,
                        Main.rand.NextVector2CircularEdge(1f, 1f),
                        12f, 8f, 20, 1200f, "GardenersFuryCelebration");
                    Main.instance.CameraModifiers.Add(shake);
                }

                // Massive VFX burst
                if (!Main.dedServ)
                {
                    // Celebration bloom
                    var bloom = new GardenerBloomParticle(
                        target.Center, Vector2.Zero, 2.5f, 35);
                    GardenersParticleHandler.SpawnParticle(bloom);

                    // Shower of petals
                    for (int i = 0; i < 12; i++)
                    {
                        var petal = new PetalBurstParticle(
                            target.Center + Main.rand.NextVector2Circular(30f, 30f),
                            Main.rand.NextVector2Circular(4f, 4f) + new Vector2(0f, -2f),
                            Main.rand.NextFloat(0.3f, 0.6f),
                            Main.rand.Next(40, 70));
                        GardenersParticleHandler.SpawnParticle(petal);
                    }

                    // Music notes ascending
                    for (int i = 0; i < 8; i++)
                    {
                        var note = new FloralNoteParticle(
                            target.Center + Main.rand.NextVector2Circular(25f, 25f),
                            new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-3.5f, -1.5f)),
                            Main.rand.NextFloat(0.3f, 0.55f),
                            Main.rand.Next(40, 65));
                        GardenersParticleHandler.SpawnParticle(note);
                    }

                    // Ring of sparks
                    for (int i = 0; i < 10; i++)
                    {
                        float angle = MathHelper.TwoPi / 10f * i;
                        Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                        var spark = new RapierSparkParticle(
                            target.Center,
                            sparkVel,
                            GardenersUtils.JubilantGold,
                            Main.rand.NextFloat(0.5f, 0.8f),
                            Main.rand.Next(15, 28));
                        GardenersParticleHandler.SpawnParticle(spark);
                    }
                }

                // Reset stacks after celebration
                comboPlayer.ComboStacks = 0;
                comboPlayer.ComboTimer = 0;
            }

            // Standard on-hit VFX — sparks at impact
            if (!Main.dedServ)
            {
                for (int i = 0; i < 3; i++)
                {
                    var spark = new RapierSparkParticle(
                        target.Center + Main.rand.NextVector2Circular(8f, 8f),
                        Main.rand.NextVector2Circular(3f, 3f),
                        Color.Lerp(GardenersUtils.GoldenPetal, GardenersUtils.JubilantGold, Main.rand.NextFloat()),
                        Main.rand.NextFloat(0.3f, 0.5f),
                        Main.rand.Next(10, 20));
                    GardenersParticleHandler.SpawnParticle(spark);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            _bloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _softBloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);

            Texture2D bloom = _bloomTex.Value;
            Texture2D softBloom = _softBloomTex.Value;
            Vector2 bOrigin = bloom.Size() / 2f;
            Vector2 sOrigin = softBloom.Size() / 2f;
            Player owner = Main.player[Projectile.owner];

            float comboIntensity = Math.Clamp(ComboAtFire / 10f, 0f, 1f);
            float timer = Timer;
            float extensionProgress;

            if (timer <= ExtendFrames)
            {
                float t = timer / ExtendFrames;
                extensionProgress = 1f - (1f - t) * (1f - t);
            }
            else if (timer <= ExtendFrames + HoldFrames)
            {
                extensionProgress = 1f;
            }
            else if (timer <= TotalDuration)
            {
                float t = (timer - ExtendFrames - HoldFrames) / (TotalDuration - ExtendFrames - HoldFrames);
                extensionProgress = 1f - t * t;
            }
            else
            {
                return false;
            }

            if (extensionProgress < 0.05f)
                return false;

            SpriteBatch sb = Main.spriteBatch;
            Vector2 tipPos = Projectile.Center - Main.screenPosition;
            Vector2 basePos = owner.MountedCenter - Main.screenPosition;
            float rot = thrustDirection.ToRotation();
            float bladeLength = MaxExtension * extensionProgress;
            float time = (float)Main.GameUpdateCount / 60f;

            sb.End();

            Vector2 beamCenter = (tipPos + basePos) / 2f;
            float beamScaleX = bladeLength / bloom.Width * 1.1f;
            float beamScaleY = 0.14f + comboIntensity * 0.08f;

            // ═══ Layer 1: VerdantSlash shader — vine-entwined blade beam ═══
            Effect slashShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyVerdantSlashShader);

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (slashShader != null)
            {
                slashShader.Parameters["uTime"]?.SetValue(time);
                slashShader.Parameters["uColor"]?.SetValue(GardenersUtils.StemGreen.ToVector3());
                slashShader.Parameters["uSecondaryColor"]?.SetValue(GardenersUtils.JubilantGold.ToVector3());
                slashShader.Parameters["uOpacity"]?.SetValue(0.8f + comboIntensity * 0.2f);
                slashShader.Parameters["uIntensity"]?.SetValue(1.3f + comboIntensity * 0.5f);
                slashShader.Parameters["uComboProgress"]?.SetValue(comboIntensity);
                slashShader.CurrentTechnique = slashShader.Techniques["VerdantSlashTechnique"];
                slashShader.CurrentTechnique.Passes[0].Apply();

                sb.Draw(bloom, beamCenter, null, Color.White, rot, bOrigin,
                    new Vector2(beamScaleX, beamScaleY * 2.2f), SpriteEffects.None, 0f);
            }

            sb.End();

            // ═══ Layer 2: Additive glow layers ═══
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Stretched blade beam
            Color bladeColor = GardenersUtils.Additive(
                Color.Lerp(GardenersUtils.GoldenPetal, GardenersUtils.JubilantGold, comboIntensity),
                0.5f + comboIntensity * 0.3f);
            sb.Draw(bloom, beamCenter, null, bladeColor, rot, bOrigin,
                new Vector2(beamScaleX, beamScaleY), SpriteEffects.None, 0f);

            // Brighter inner core beam
            Color coreColor = GardenersUtils.Additive(GardenersUtils.SunlightWhite, 0.3f + comboIntensity * 0.25f);
            sb.Draw(bloom, beamCenter, null, coreColor, rot, bOrigin,
                new Vector2(beamScaleX * 0.7f, beamScaleY * 0.45f), SpriteEffects.None, 0f);

            // ═══ Layer 3: Bright tip glow with SoftRadialBloom ═══
            float tipPulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.15f;
            float tipGlowScale = (0.35f + comboIntensity * 0.25f) * tipPulse;

            Color tipColor = GardenersUtils.Additive(GardenersUtils.JubilantGold, 0.7f + comboIntensity * 0.2f);
            sb.Draw(softBloom, tipPos, null, tipColor, 0f, sOrigin, tipGlowScale, SpriteEffects.None, 0f);

            Color tipCore = GardenersUtils.Additive(GardenersUtils.SunlightWhite, 0.5f + comboIntensity * 0.3f);
            sb.Draw(bloom, tipPos, null, tipCore, 0f, bOrigin, tipGlowScale * 0.45f, SpriteEffects.None, 0f);

            // Rose accent at combo 5+
            if (comboIntensity >= 0.5f)
            {
                Color roseAccent = GardenersUtils.Additive(GardenersUtils.RoseBlush, (comboIntensity - 0.5f) * 0.5f);
                sb.Draw(softBloom, tipPos, null, roseAccent, 0f, sOrigin, tipGlowScale * 1.3f, SpriteEffects.None, 0f);
            }

            sb.End();
            GardenersUtils.BeginDefault(sb);

            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // SmallPetalProjectile — 12x12, pen 1, timeLeft 120, homes at 0.06 lerp
    // Applies Poisoned 120 on hit.
    // ═══════════════════════════════════════════════════════════
    public class SmallPetalProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // Home onto nearest NPC at 0.06 lerp
            NPC target = GardenersUtils.ClosestNPC(Projectile.Center, 600f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.06f);
            }

            // Maintain speed
            if (Projectile.velocity.Length() < 4f)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 4f;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Spawn drifting petal particles
            if (!Main.dedServ && Main.rand.NextBool(3))
            {
                var petal = new PetalBurstParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    Main.rand.NextFloat(0.15f, 0.25f),
                    Main.rand.Next(15, 30));
                GardenersParticleHandler.SpawnParticle(petal);
            }

            Lighting.AddLight(Projectile.Center, 0.3f, 0.25f, 0.1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 120);

            if (!Main.dedServ)
            {
                for (int i = 0; i < 3; i++)
                {
                    var spark = new RapierSparkParticle(
                        target.Center + Main.rand.NextVector2Circular(6f, 6f),
                        Main.rand.NextVector2Circular(2f, 2f),
                        GardenersUtils.RoseBlush,
                        Main.rand.NextFloat(0.2f, 0.35f),
                        Main.rand.Next(8, 16));
                    GardenersParticleHandler.SpawnParticle(spark);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            _bloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _bloomTex.Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            GardenersUtils.BeginAdditive(sb);

            float fade = Projectile.timeLeft / 120f;
            Color petalCol = GardenersUtils.Additive(
                Color.Lerp(GardenersUtils.RoseBlush, GardenersUtils.GoldenPetal, 0.4f), fade * 0.8f);
            sb.Draw(tex, drawPos, null, petalCol, Projectile.rotation, origin, 0.2f, SpriteEffects.None, 0f);

            Color coreCol = GardenersUtils.Additive(GardenersUtils.SunlightWhite, fade * 0.4f);
            sb.Draw(tex, drawPos, null, coreCol, Projectile.rotation, origin, 0.1f, SpriteEffects.None, 0f);

            sb.End();
            GardenersUtils.BeginDefault(sb);

            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // JubilantPetalProjectile — 20x20, pen -1, timeLeft 90, homes at 0.08 lerp
    // Applies Poisoned 180 + Confused 60 on hit.
    // ═══════════════════════════════════════════════════════════
    public class JubilantPetalProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> _bloomTex;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            // Home onto nearest NPC at 0.08 lerp
            NPC target = GardenersUtils.ClosestNPC(Projectile.Center, 800f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.08f);
            }

            // Maintain speed
            if (Projectile.velocity.Length() < 5f)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 5f;

            Projectile.rotation += 0.15f;

            // Rich VFX trail — petals + notes
            if (!Main.dedServ)
            {
                if (Main.rand.NextBool(2))
                {
                    var petal = new PetalBurstParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                        -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f),
                        Main.rand.NextFloat(0.2f, 0.35f),
                        Main.rand.Next(20, 40));
                    GardenersParticleHandler.SpawnParticle(petal);
                }

                if (Main.rand.NextBool(5))
                {
                    var note = new FloralNoteParticle(
                        Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                        new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2f, -0.5f)),
                        Main.rand.NextFloat(0.2f, 0.35f),
                        Main.rand.Next(25, 45));
                    GardenersParticleHandler.SpawnParticle(note);
                }
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.4f, 0.15f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 180);
            target.AddBuff(BuffID.Confused, 60);

            if (!Main.dedServ)
            {
                // Jubilant impact burst
                var bloom = new GardenerBloomParticle(
                    target.Center, Vector2.Zero, 1.2f, 20);
                GardenersParticleHandler.SpawnParticle(bloom);

                for (int i = 0; i < 5; i++)
                {
                    var spark = new RapierSparkParticle(
                        target.Center + Main.rand.NextVector2Circular(8f, 8f),
                        Main.rand.NextVector2Circular(4f, 4f),
                        Color.Lerp(GardenersUtils.JubilantGold, GardenersUtils.SunlightWhite, Main.rand.NextFloat(0.3f)),
                        Main.rand.NextFloat(0.35f, 0.6f),
                        Main.rand.Next(10, 22));
                    GardenersParticleHandler.SpawnParticle(spark);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            _bloomTex ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _bloomTex.Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = (float)Main.GameUpdateCount / 60f;

            SpriteBatch sb = Main.spriteBatch;
            sb.End();

            float fade = Projectile.timeLeft / 90f;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.25f) * 0.1f;

            // ═══ GardenBloom shader — petal-shaped pulsing glow ═══
            Effect bloomShader = ShaderLoader.GetShader(ShaderLoader.OdeToJoyGardenBloomShader);

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (bloomShader != null)
            {
                bloomShader.Parameters["uTime"]?.SetValue(time);
                bloomShader.Parameters["uColor"]?.SetValue(GardenersUtils.JubilantGold.ToVector3());
                bloomShader.Parameters["uSecondaryColor"]?.SetValue(GardenersUtils.RoseBlush.ToVector3());
                bloomShader.Parameters["uOpacity"]?.SetValue(fade * 0.6f);
                bloomShader.Parameters["uIntensity"]?.SetValue(1.3f);
                bloomShader.Parameters["uRadius"]?.SetValue(0.4f);
                bloomShader.Parameters["uPulseSpeed"]?.SetValue(4f);
                bloomShader.CurrentTechnique = bloomShader.Techniques["JubilantPulseTechnique"];
                bloomShader.CurrentTechnique.Passes[0].Apply();

                sb.Draw(tex, drawPos, null, Color.White, Projectile.rotation, origin,
                    0.55f * pulse, SpriteEffects.None, 0f);
            }

            // Outer golden glow
            Color outerCol = GardenersUtils.Additive(GardenersUtils.JubilantGold, fade * 0.65f);
            sb.Draw(tex, drawPos, null, outerCol, Projectile.rotation, origin, 0.38f * pulse, SpriteEffects.None, 0f);

            // Rose-blush mid layer
            Color midCol = GardenersUtils.Additive(GardenersUtils.RoseBlush, fade * 0.45f);
            sb.Draw(tex, drawPos, null, midCol, Projectile.rotation * 0.7f, origin, 0.26f * pulse, SpriteEffects.None, 0f);

            // Hot white core
            Color coreCol = GardenersUtils.Additive(GardenersUtils.SunlightWhite, fade * 0.5f);
            sb.Draw(tex, drawPos, null, coreCol, 0f, origin, 0.13f * pulse, SpriteEffects.None, 0f);

            sb.End();
            GardenersUtils.BeginDefault(sb);

            return false;
        }
    }
}
