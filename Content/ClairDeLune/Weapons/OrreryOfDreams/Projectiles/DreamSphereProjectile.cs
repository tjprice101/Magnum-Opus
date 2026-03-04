using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.OrreryOfDreams.Projectiles
{
    /// <summary>
    /// Orbiting Dream Sphere with CelestialOrbit.fx shader rendering.
    /// 3 render passes: (1) CelestialOrbitCore shader overlay, (2) Multi-scale bloom halo,
    /// (3) Orbit ring trail dots with PearlShimmer.
    /// ai[0] = sphere type (0=Inner, 1=Middle, 2=Outer), ai[1] = orbit direction.
    /// </summary>
    public class DreamSphereProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private float _orbitAngle;
        private int _fireTimer;
        private const int InnerFireRate = 20;
        private const int MiddleFireRate = 40;
        private const int OuterFireRate = 60;

        private float OrbitDistance => SphereType switch { 0 => 40f, 1 => 80f, 2 => 120f, _ => 80f };
        private float OrbitSpeed => SphereType switch
        {
            0 => MathHelper.ToRadians(3f), 1 => MathHelper.ToRadians(2f),
            2 => MathHelper.ToRadians(1f), _ => MathHelper.ToRadians(2f)
        };
        private int FireRate => SphereType switch { 0 => InnerFireRate, 1 => MiddleFireRate, 2 => OuterFireRate, _ => MiddleFireRate };
        private int SphereType => (int)Projectile.ai[0];
        private float OrbitDirection => Projectile.ai[1];

        // --- Shader + texture caching ---
        private static Effect _celestialOrbitShader;
        private static Effect _pearlShimmerShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _starFlare;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = false;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
            Projectile.hide = false;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead || !owner.HeldItem.active ||
                owner.HeldItem.type != ModContent.ItemType<OrreryOfDreams>())
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;
            _orbitAngle += OrbitSpeed * OrbitDirection;
            Vector2 orbitOffset = new Vector2(
                MathF.Cos(_orbitAngle) * OrbitDistance,
                MathF.Sin(_orbitAngle) * OrbitDistance * 0.6f
            );
            Projectile.Center = owner.Center + orbitOffset;

            int target = FindNearestEnemy(600f);
            _fireTimer++;
            if (_fireTimer >= FireRate && target != -1)
            {
                _fireTimer = 0;
                FireAtTarget(owner, target);
            }

            SpawnAmbientParticles();
        }

        private void FireAtTarget(Player owner, int targetIndex)
        {
            NPC target = Main.npc[targetIndex];
            Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            bool isNight = !Main.dayTime;
            float nightMult = isNight ? 1.15f : 1f;

            switch (SphereType)
            {
                case 0:
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                        direction * 14f, ModContent.ProjectileType<InnerSphereBoltProjectile>(),
                        (int)(Projectile.damage * nightMult), 2f, owner.whoAmI);
                    SoundEngine.PlaySound(SoundID.Item12 with { Pitch = 0.5f, Volume = 0.3f }, Projectile.Center);
                    break;
                case 1:
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                        direction * 8f, ModContent.ProjectileType<MiddleSphereOrbProjectile>(),
                        (int)(Projectile.damage * nightMult), 3f, owner.whoAmI);
                    SoundEngine.PlaySound(SoundID.Item28 with { Pitch = 0.3f, Volume = 0.35f }, Projectile.Center);
                    break;
                case 2:
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                        direction * 6f, ModContent.ProjectileType<OuterSphereBombProjectile>(),
                        (int)(Projectile.damage * nightMult), 5f, owner.whoAmI);
                    SoundEngine.PlaySound(SoundID.Item73 with { Pitch = -0.2f, Volume = 0.4f }, Projectile.Center);
                    break;
            }
        }

        private int FindNearestEnemy(float range)
        {
            int closest = -1;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist) { closestDist = dist; closest = i; }
            }
            return closest;
        }

        private void SpawnAmbientParticles()
        {
            if (Main.rand.NextBool(6))
            {
                Color color = SphereType switch
                {
                    0 => ClairDeLunePalette.PearlWhite, 1 => ClairDeLunePalette.SoftBlue,
                    2 => ClairDeLunePalette.NightMist, _ => ClairDeLunePalette.PearlBlue
                };
                var sparkle = new SparkleParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8, 8),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    color with { A = 0 } * 0.6f, 0.15f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        private void LoadTextures()
        {
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _starFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            Color coreColor = SphereType switch
            {
                0 => ClairDeLunePalette.PearlWhite, 1 => ClairDeLunePalette.SoftBlue,
                2 => ClairDeLunePalette.NightMist, _ => ClairDeLunePalette.PearlBlue
            };
            Color glowColor = SphereType switch
            {
                0 => ClairDeLunePalette.PearlBlue, 1 => ClairDeLunePalette.MidnightBlue,
                2 => ClairDeLunePalette.NightMist, _ => ClairDeLunePalette.SoftBlue
            };

            float pulse = 0.85f + 0.15f * MathF.Sin(Main.GameUpdateCount * 0.08f + SphereType * MathHelper.TwoPi / 3f);
            float baseScale = SphereType switch { 0 => 0.15f, 1 => 0.22f, 2 => 0.3f, _ => 0.2f };

            DrawCelestialOrbitCore(sb, matrix, coreColor, pulse);   // Pass 1: CelestialOrbit shader
            DrawBloomHalo(sb, matrix, coreColor, glowColor, pulse, baseScale); // Pass 2: Multi-scale bloom
            DrawOrbitRing(sb, matrix, glowColor);                   // Pass 3: Orbit ring trail

            ClairDeLuneVFXLibrary.DrawThemeAccents(sb, Projectile.Center, 0.5f, 0.3f);
            return false;
        }

        // ---- PASS 1: CelestialOrbitCore shader via SpriteBatch Effect ----
        private void DrawCelestialOrbitCore(SpriteBatch sb, Matrix matrix, Color coreColor, float pulse)
        {
            _celestialOrbitShader ??= ShaderLoader.CelestialOrbit;
            if (_celestialOrbitShader == null) return;

            sb.End();

            _celestialOrbitShader.Parameters["uColor"]?.SetValue(coreColor.ToVector4());
            _celestialOrbitShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _celestialOrbitShader.Parameters["uOpacity"]?.SetValue(0.7f * pulse);
            _celestialOrbitShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _celestialOrbitShader.Parameters["uIntensity"]?.SetValue(1.5f);
            _celestialOrbitShader.Parameters["uOverbrightMult"]?.SetValue(1.2f);
            _celestialOrbitShader.Parameters["uScrollSpeed"]?.SetValue(1f);
            _celestialOrbitShader.Parameters["uDistortionAmt"]?.SetValue(0.01f);
            _celestialOrbitShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _celestialOrbitShader.CurrentTechnique = _celestialOrbitShader.Techniques["CelestialOrbitCore"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _celestialOrbitShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float sphereSize = SphereType switch { 0 => 20f, 1 => 30f, 2 => 40f, _ => 28f };
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f,
                sphereSize / sc.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: Multi-scale bloom halo ----
        private void DrawBloomHalo(SpriteBatch sb, Matrix matrix, Color coreColor, Color glowColor, float pulse, float baseScale)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            Texture2D sf = _starFlare.Value;

            // Outer haze
            sb.Draw(srb, drawPos, null, glowColor with { A = 0 } * 0.2f * pulse,
                0f, srb.Size() * 0.5f, baseScale * 2.5f * (srb.Width / 64f), SpriteEffects.None, 0f);
            // Mid glow
            sb.Draw(srb, drawPos, null, coreColor with { A = 0 } * 0.4f * pulse,
                0f, srb.Size() * 0.5f, baseScale * 1.5f * (srb.Width / 64f), SpriteEffects.None, 0f);
            // Hot core
            sb.Draw(pb, drawPos, null, ClairDeLunePalette.WhiteHot with { A = 0 } * 0.6f * pulse,
                0f, pb.Size() * 0.5f, baseScale * 0.8f * (pb.Width / 64f), SpriteEffects.None, 0f);
            // Star flare accent
            sb.Draw(sf, drawPos, null, coreColor with { A = 0 } * 0.15f * pulse,
                Main.GlobalTimeWrappedHourly * (1f + SphereType * 0.5f), sf.Size() * 0.5f,
                baseScale * 1.2f * (sf.Width / 64f), SpriteEffects.None, 0f);
        }

        // ---- PASS 3: Orbit ring trail with PearlShimmer overlay ----
        private void DrawOrbitRing(SpriteBatch sb, Matrix matrix, Color glowColor)
        {
            _pearlShimmerShader ??= ShaderLoader.ClairDeLunePearlGlow;

            Vector2 ownerDraw = Main.player[Projectile.owner].Center - Main.screenPosition;
            Texture2D pb = _pointBloom.Value;
            Vector2 pbOrigin = pb.Size() * 0.5f;

            if (_pearlShimmerShader != null)
            {
                sb.End();

                _pearlShimmerShader.Parameters["uColor"]?.SetValue(glowColor.ToVector4());
                _pearlShimmerShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector4());
                _pearlShimmerShader.Parameters["uOpacity"]?.SetValue(0.25f);
                _pearlShimmerShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                _pearlShimmerShader.Parameters["uIntensity"]?.SetValue(1f);
                _pearlShimmerShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

                _pearlShimmerShader.CurrentTechnique = _pearlShimmerShader.Techniques["PearlShimmer"];

                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                    SamplerState.LinearWrap, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, _pearlShimmerShader, matrix);
            }

            // Draw orbit ring points
            for (int i = 0; i < 24; i++)
            {
                float a = _orbitAngle * 0.1f + i * MathHelper.TwoPi / 24f;
                Vector2 ringPt = ownerDraw + new Vector2(
                    MathF.Cos(a) * OrbitDistance,
                    MathF.Sin(a) * OrbitDistance * 0.6f);
                sb.Draw(pb, ringPt, null, glowColor with { A = 0 } * 0.06f,
                    0f, pbOrigin, 4f / pb.Width, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, matrix);
        }
    }
}
