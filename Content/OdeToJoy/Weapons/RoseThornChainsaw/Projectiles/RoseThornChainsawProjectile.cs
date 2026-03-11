using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Buffs;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Dusts;

namespace MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Projectiles
{
    /// <summary>
    /// Chainsaw Holdout — continuous held projectile.
    /// Renders a spinning thorn ribbon around the weapon,
    /// flings thorn shrapnel periodically, and erupts in Petal Storm.
    /// </summary>
    public class ChainsawHoldoutProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int holdTimer;
        private int thornFlingTimer;
        private float revSpeed; // 0 → 1 (max RPM)
        private float petalStormMeter; // 0 → 1 (erupts at 1)
        private float ribbonAngle;
        private bool hasFiredPetalStorm;

        private const int ThornFlingInterval = 30; // 0.5s
        private const int PetalStormChargeFrames = 240; // 4s
        private const float MaxRevTime = 120f; // 2s to max rev

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

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // Channel check
            if (!owner.channel || owner.dead || !owner.active)
            {
                Projectile.Kill();
                return;
            }

            holdTimer++;
            thornFlingTimer++;
            owner.heldProj = Projectile.whoAmI;

            // Aim toward mouse
            Vector2 aimDir = (Main.MouseWorld - owner.MountedCenter).SafeNormalize(Vector2.UnitX);
            Projectile.Center = owner.MountedCenter + aimDir * 40f;
            Projectile.rotation = aimDir.ToRotation();
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

            // Direction
            owner.direction = Math.Sign(aimDir.X) != 0 ? Math.Sign(aimDir.X) : 1;
            owner.itemTime = 2;
            owner.itemAnimation = 2;

            // Rev up
            revSpeed = MathHelper.Clamp(holdTimer / MaxRevTime, 0f, 1f);

            // Petal Storm meter
            petalStormMeter = MathHelper.Clamp(holdTimer / (float)PetalStormChargeFrames, 0f, 1f);

            // Petal Storm eruption
            if (petalStormMeter >= 1f && !hasFiredPetalStorm)
            {
                hasFiredPetalStorm = true;
                FirePetalStorm(owner);
            }

            // Thorn fling every 0.5s
            if (thornFlingTimer >= ThornFlingInterval)
            {
                thornFlingTimer = 0;
                FlingThorns(owner);
            }

            // Ribbon spin (faster with rev)
            ribbonAngle += 0.3f + revSpeed * 0.4f;

            // Damage scales with rev
            float revBonus = revSpeed * 0.4f; // up to +40%
            Projectile.damage = (int)(owner.HeldItem.damage * (1f + revBonus));

            // VFX: continuous particles
            SpawnChainsawParticles(owner, aimDir);

            // Lighting
            Lighting.AddLight(Projectile.Center,
                ChainsawTextures.RadiantAmber.ToVector3() * (0.3f + revSpeed * 0.3f));
        }

        private void SpawnChainsawParticles(Player owner, Vector2 aimDir)
        {
            // Thorn debris flying off — custom chainsaw sparks
            if (holdTimer % 3 == 0)
            {
                float angle = ribbonAngle + Main.rand.NextFloat(-0.5f, 0.5f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f * (1f + revSpeed));
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    ModContent.DustType<ChainsawSparkDust>(), vel,
                    Scale: Main.rand.NextFloat(0.3f, 0.7f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }

            // Rev trail sparks at high RPM — hot grind sparks
            if (revSpeed > 0.5f && holdTimer % 2 == 0)
            {
                Vector2 sparkPos = Projectile.Center + aimDir * 25f
                    + Main.rand.NextVector2Circular(5f, 5f);
                Vector2 vel = aimDir.RotatedByRandom(0.8f) * Main.rand.NextFloat(3f, 6f);
                Dust dust = Dust.NewDustPerfect(sparkPos,
                    ModContent.DustType<ChainsawSparkDust>(), vel,
                    Scale: Main.rand.NextFloat(0.2f, 0.5f));
                dust.noGravity = true;
            }

            // RhythmicPulse at max rev
            if (revSpeed >= 1f && holdTimer == (int)MaxRevTime)
            {
                OdeToJoyVFXLibrary.RhythmicPulse(Projectile.Center, 0.8f, OdeToJoyPalette.GoldenPollen);
            }
        }

        private void FlingThorns(Player owner)
        {
            // Find 2 closest enemies within 8 tiles (128px)
            int flinged = 0;
            for (int i = 0; i < Main.maxNPCs && flinged < 2; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (Vector2.Distance(owner.Center, npc.Center) > 128f) continue;

                Vector2 dir = (npc.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.NewProjectile(
                    owner.GetSource_ItemUse(owner.HeldItem),
                    Projectile.Center, dir * 12f,
                    ModContent.ProjectileType<ThornShrapnelProjectile>(),
                    (int)(Projectile.damage * 0.3f), Projectile.knockBack * 0.3f,
                    owner.whoAmI);
                flinged++;
            }
        }

        private void FirePetalStorm(Player owner)
        {
            SoundEngine.PlaySound(SoundID.Item119 with { Pitch = 0.2f, Volume = 0.8f },
                Projectile.Center);

            // 12 petal projectiles in all directions
            int petalCount = 12;
            for (int i = 0; i < petalCount; i++)
            {
                float angle = MathHelper.TwoPi / petalCount * i;
                Vector2 vel = angle.ToRotationVector2() * 10f;
                Projectile.NewProjectile(
                    owner.GetSource_ItemUse(owner.HeldItem),
                    Projectile.Center, vel,
                    ModContent.ProjectileType<PetalStormProjectile>(),
                    (int)(Projectile.damage * 0.6f), Projectile.knockBack * 0.5f,
                    owner.whoAmI);
            }

            // Burst VFX: spiral petal + spark particles
            for (int i = 0; i < 55; i++)
            {
                float angle = MathHelper.TwoPi / 55f * i;
                float speed = Main.rand.NextFloat(3f, 8f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                vel += angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * speed * 0.3f;

                // Alternate between petal chips and hot sparks
                int dustType = (i % 3 == 0)
                    ? ModContent.DustType<ChainsawSparkDust>()
                    : ModContent.DustType<RosePetalChipDust>();

                Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, vel,
                    Scale: Main.rand.NextFloat(0.5f, 1f));
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }

            // Petal Storm screen effects
            OdeToJoyVFXLibrary.ScreenShake(10f, 20);
            OdeToJoyVFXLibrary.ScreenFlash(OdeToJoyPalette.PetalPink, 1.2f);
            OdeToJoyVFXLibrary.HarmonicPulseRing(Projectile.Center, 1.5f, 16, OdeToJoyPalette.GoldenPollen);
            OdeToJoyVFXLibrary.CelebrationBurst(Projectile.Center, 1.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<RoseThornBleedDebuff>(), 180);

            // Contact impact sparks — grinding chainsaw sparks
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero).RotatedByRandom(0.5f)
                    * Main.rand.NextFloat(2f, 5f);
                Dust dust = Dust.NewDustPerfect(target.Center,
                    ModContent.DustType<ChainsawSparkDust>(), vel,
                    Scale: Main.rand.NextFloat(0.4f, 0.7f));
                dust.noGravity = true;
            }
            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(target.Center, 3, 4f, 0.5f);

            // Crit: bonus shrapnel
            if (hit.Crit)
            {
                Player owner = Main.player[Projectile.owner];
                Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.NewProjectile(
                    owner.GetSource_ItemUse(owner.HeldItem),
                    Projectile.Center, dir * 10f,
                    ModContent.ProjectileType<ThornShrapnelProjectile>(),
                    (int)(Projectile.damage * 0.2f), 2f, owner.whoAmI);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Player owner = Main.player[Projectile.owner];
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: VerdantSlash shader — spinning thorn aura ──
            Effect slashShader = OdeToJoyShaders.VerdantSlash;
            if (slashShader != null)
            {
                OdeToJoyShaders.SetSlashParams(slashShader, time + ribbonAngle * 0.5f,
                    ChainsawTextures.RadiantAmber, ChainsawTextures.BloomGold,
                    (0.4f + revSpeed * 0.4f), 2f + revSpeed, revSpeed);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, slashShader, "VerdantSlashTechnique");
                Texture2D softGlow = ChainsawTextures.SoftGlow.Value;
                Vector2 glowOrigin = softGlow.Size() / 2f;
                sb.Draw(softGlow, drawPos, null, Color.White, ribbonAngle, glowOrigin,
                    0.15f + revSpeed * 0.1f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            // Spinning ribbon thorn fragments
            Texture2D trailTex = ChainsawTextures.OJTrail.Value;
            int ribbonSegments = 6;
            for (int i = 0; i < ribbonSegments; i++)
            {
                float segAngle = ribbonAngle + MathHelper.TwoPi / ribbonSegments * i;
                float dist = 20f + revSpeed * 10f;
                Vector2 segPos = drawPos + segAngle.ToRotationVector2() * dist;

                Color segColor = Color.Lerp(ChainsawTextures.RoseShadow, ChainsawTextures.RadiantAmber,
                    (float)Math.Sin(segAngle * 2f + holdTimer * 0.1f) * 0.5f + 0.5f);

                Texture2D thornTex = ChainsawTextures.OJThornFragment.Value;
                Vector2 thornOrigin = thornTex.Size() / 2f;
                sb.Draw(thornTex, segPos, null,
                    segColor * (0.45f + revSpeed * 0.35f),
                    segAngle + MathHelper.PiOver2, thornOrigin,
                    0.35f + revSpeed * 0.1f, SpriteEffects.None, 0f);
            }

            // Core glow at chainsaw tip
            {
                Texture2D softGlow2 = ChainsawTextures.SoftGlow.Value;
                Vector2 glowOrigin2 = softGlow2.Size() / 2f;
                sb.Draw(softGlow2, drawPos, null,
                    ChainsawTextures.RadiantAmber * (0.25f + revSpeed * 0.25f),
                    0f, glowOrigin2, 0.08f + revSpeed * 0.04f, SpriteEffects.None, 0f);
                sb.Draw(softGlow2, drawPos, null,
                    ChainsawTextures.PureJoyWhite * (0.15f + revSpeed * 0.15f),
                    0f, glowOrigin2, 0.03f, SpriteEffects.None, 0f);
            }

            // Petal storm meter indicator
            if (petalStormMeter > 0.1f && !hasFiredPetalStorm)
            {
                Texture2D ringTex = ChainsawTextures.OJPowerEffectRing.Value;
                Vector2 ringOrigin = ringTex.Size() / 2f;
                float ringScale = petalStormMeter * 0.4f;
                Color ringColor = Color.Lerp(ChainsawTextures.PetalPink,
                    ChainsawTextures.JubilantLight, petalStormMeter);
                sb.Draw(ringTex, drawPos, null,
                    ringColor * petalStormMeter * 0.35f,
                    holdTimer * 0.02f, ringOrigin, ringScale,
                    SpriteEffects.None, 0f);
            }

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
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
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                int dustType = (i % 2 == 0)
                    ? ModContent.DustType<ChainsawSparkDust>()
                    : ModContent.DustType<RosePetalChipDust>();
                Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, vel,
                    Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.noGravity = true;
            }
            OdeToJoyVFXLibrary.RhythmicPulse(Projectile.Center, 0.6f, OdeToJoyPalette.PetalPink);
        }
    }

    /// <summary>
    /// Thorn Shrapnel — flung from chainsaw at nearby enemies.
    /// Rose Shadow trailing sparks, embeds on contact with bleed.
    /// </summary>
    public class ThornShrapnelProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int timer;
        private VertexStrip _vertexStrip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity.Y += 0.12f; // slight gravity

            // Rose Shadow trail — grinding sparks
            if (timer % 2 == 0)
            {
                Vector2 vel = -Projectile.velocity * 0.05f;
                Dust dust = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<ChainsawSparkDust>(), vel,
                    Scale: Main.rand.NextFloat(0.2f, 0.4f));
                dust.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center,
                ChainsawTextures.PetalPink.ToVector3() * 0.15f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<RoseThornBleedDebuff>(), 180);

            for (int i = 0; i < 4; i++)
            {
                Dust dust = Dust.NewDustPerfect(target.Center,
                    ModContent.DustType<ChainsawSparkDust>(),
                    Main.rand.NextVector2Circular(3f, 3f),
                    Scale: Main.rand.NextFloat(0.3f, 0.5f));
                dust.noGravity = true;
            }
            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(target.Center, 3, 4f, 0.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _vertexStrip);

                // Thorn shrapnel: rose-thorn directional streak
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;
                    float rot = Projectile.velocity.ToRotation();

                    sb.Draw(glow, drawPos, null,
                        (OdeToJoyPalette.RosePink with { A = 0 }) * 0.2f,
                        rot, origin, new Vector2(0.05f, 0.02f), SpriteEffects.None, 0f);
                }

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

    /// <summary>
    /// Petal Storm Projectile — razor petal from 360° eruption.
    /// Travels outward 12 tiles with petal sprite and pink trail.
    /// </summary>
    public class PetalStormProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int timer;
        private VertexStrip _vertexStrip;
        private const int MaxLifetime = 60;

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
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 3;
            Projectile.timeLeft = MaxLifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            timer++;
            Projectile.rotation += 0.2f;

            // Petal trail — shredded rose chips
            if (timer % 2 == 0)
            {
                Vector2 vel = -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<RosePetalChipDust>(), vel,
                    Scale: Main.rand.NextFloat(0.3f, 0.5f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }

            Lighting.AddLight(Projectile.Center,
                ChainsawTextures.PetalPink.ToVector3() * 0.2f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustPerfect(target.Center,
                    ModContent.DustType<RosePetalChipDust>(),
                    Main.rand.NextVector2Circular(3f, 3f),
                    Scale: Main.rand.NextFloat(0.4f, 0.7f));
                dust.noGravity = true;
            }
            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(target.Center, 4, 5f, 0.6f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _vertexStrip);

                // Petal storm: pink petal shimmer
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;

                    sb.Draw(glow, drawPos, null,
                        (OdeToJoyPalette.PetalPink with { A = 0 }) * 0.16f,
                        Projectile.rotation, origin, 0.035f, SpriteEffects.None, 0f);
                }

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

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<RosePetalChipDust>(),
                    Main.rand.NextVector2Circular(2f, 2f),
                    Scale: Main.rand.NextFloat(0.3f, 0.5f));
                dust.noGravity = true;
            }
            OdeToJoyVFXLibrary.SpawnTriumphantStarburst(Projectile.Center);
        }
    }

    /// <summary>
    /// ThornChainProjectile — kept for backward compatibility.
    /// Bouncing thorn chain that ricochets off terrain.
    /// </summary>
    public class ThornChainProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int timer;
        private VertexStrip _vertexStrip;

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
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 6;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Bounce
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X;
            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y;

            // Bounce particles — sparks on ricochet
            for (int i = 0; i < 3; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<ChainsawSparkDust>(),
                    Main.rand.NextVector2Circular(2f, 2f),
                    Scale: Main.rand.NextFloat(0.3f, 0.5f));
                dust.noGravity = true;
            }
            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(Projectile.Center, 2, 3f, 0.4f);

            Projectile.penetrate--;
            return false;
        }

        public override void AI()
        {
            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (timer % 3 == 0)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<ChainsawSparkDust>(),
                    -Projectile.velocity * 0.05f,
                    Scale: Main.rand.NextFloat(0.2f, 0.35f));
                dust.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<RoseThornBleedDebuff>(), 120);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _vertexStrip);

                // Thorn chain: amber bounce streak
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;
                    float rot = Projectile.velocity.ToRotation();

                    sb.Draw(glow, drawPos, null,
                        (OdeToJoyPalette.WarmAmber with { A = 0 }) * 0.18f,
                        rot, origin, new Vector2(0.05f, 0.02f), SpriteEffects.None, 0f);
                }

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
