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

namespace MagnumOpus.Content.ClairDeLune.Weapons.LunarPhylactery.Projectiles
{
    /// <summary>
    /// Moonlight Sentinel — crystal minion that orbits player and fires sustained beams.
    /// VoronoiCell-style faceted crystal body. Fires MoonlightBeamProjectile at enemies.
    /// 3 render passes: (1) SoulBeam.fx SoulBeamAura crystal body,
    /// (2) ClairDeLunePearlGlow.fx PearlShimmer facet overlay, (3) Multi-scale bloom + target indicator.
    /// </summary>
    public class MoonlightSentinelProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private float _orbitAngle;
        private int _fireTimer;
        private int _pulseTimer;
        private int _beamTarget = -1;
        private const int FireCooldown = 30;
        private const int PulseCooldown = 600;
        private const float OrbitRadius = 60f;
        private const float DetectionRange = 800f;

        // --- Shader + texture caching ---
        private static Effect _soulBeamShader;
        private static Effect _pearlGlowShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!owner.active || owner.dead)
            {
                owner.ClearBuff(ModContent.BuffType<LunarPhylacteryBuff>());
                Projectile.Kill();
                return;
            }

            if (owner.HasBuff(ModContent.BuffType<LunarPhylacteryBuff>()))
                Projectile.timeLeft = 2;

            _orbitAngle += MathHelper.ToRadians(2f);
            Vector2 targetPos = owner.Center + new Vector2(
                MathF.Cos(_orbitAngle) * OrbitRadius,
                MathF.Sin(_orbitAngle) * OrbitRadius * 0.5f - 40f
            );

            Vector2 toTarget = targetPos - Projectile.Center;
            Projectile.velocity = toTarget * 0.15f;

            float bob = MathF.Sin(Main.GameUpdateCount * 0.04f) * 2f;
            Projectile.position.Y += bob * 0.1f;

            _beamTarget = FindTarget(owner);

            _fireTimer++;
            if (_fireTimer >= FireCooldown && _beamTarget != -1)
            {
                _fireTimer = 0;
                FireBeam(owner);
            }

            _pulseTimer++;
            if (_pulseTimer >= PulseCooldown)
            {
                _pulseTimer = 0;
                int healAmount = Math.Max(1, (int)(owner.statLifeMax2 * 0.03f));
                owner.Heal(healAmount);

                SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.5f, Volume = 0.3f }, Projectile.Center);

                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 vel = angle.ToRotationVector2() * 2f;
                    var bloom = new BloomParticle(Projectile.Center, vel,
                        ClairDeLunePalette.PearlBlue with { A = 0 } * 0.4f,
                        0.12f, 20);
                    MagnumParticleHandler.SpawnParticle(bloom);
                }

                var healSparkle = new SparkleParticle(owner.Center + new Vector2(0, -20),
                    new Vector2(0, -1f), ClairDeLunePalette.PearlWhite with { A = 0 } * 0.5f,
                    0.15f, 25);
                MagnumParticleHandler.SpawnParticle(healSparkle);
            }

            if (Main.rand.NextBool(5))
            {
                Vector2 offset = Main.rand.NextVector2Circular(12, 12);
                var sparkle = new SparkleParticle(Projectile.Center + offset,
                    Main.rand.NextVector2Circular(0.3f, 0.3f) + new Vector2(0, -0.5f),
                    ClairDeLunePalette.PearlBlue with { A = 0 } * 0.3f,
                    0.06f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.PearlBlue.ToVector3() * 0.3f);
        }

        private int FindTarget(Player owner)
        {
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC target = Main.npc[owner.MinionAttackTargetNPC];
                if (target.active && !target.friendly && Vector2.Distance(Projectile.Center, target.Center) < DetectionRange)
                    return owner.MinionAttackTargetNPC;
            }

            int closest = -1;
            float closestDist = DetectionRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.CountsAsACritter) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist && Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                {
                    closestDist = dist;
                    closest = i;
                }
            }
            return closest;
        }

        private void FireBeam(Player owner)
        {
            if (_beamTarget == -1) return;

            Vector2 dir = (Main.npc[_beamTarget].Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, dir * 20f,
                ModContent.ProjectileType<MoonlightBeamProjectile>(),
                Projectile.damage, Projectile.knockBack, owner.whoAmI, _beamTarget);

            SoundEngine.PlaySound(SoundID.Item12 with { Pitch = 0.3f, Volume = 0.35f }, Projectile.Center);
        }

        private void LoadTextures()
        {
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            DrawSoulBeamBody(sb, matrix);        // Pass 1: SoulBeamAura crystal body
            DrawPearlFacetOverlay(sb, matrix);   // Pass 2: PearlShimmer facets
            DrawBloomCrystal(sb, matrix);        // Pass 3: Bloom + target indicator
            ClairDeLuneVFXLibrary.DrawThemeAccents(sb, Projectile.Center, 0.5f, 0.3f);
            return false;
        }

        // ---- PASS 1: SoulBeam.fx SoulBeamAura crystal body ----
        private void DrawSoulBeamBody(SpriteBatch sb, Matrix matrix)
        {
            _soulBeamShader ??= ShaderLoader.SoulBeam;
            if (_soulBeamShader == null) return;

            float pulse = 0.85f + 0.15f * MathF.Sin(Main.GameUpdateCount * 0.07f);

            Player owner = Main.player[Projectile.owner];
            float hpRatio = owner.statLife / (float)owner.statLifeMax2;

            sb.End();

            Color bodyColor = Color.Lerp(ClairDeLunePalette.TemporalCrimson, ClairDeLunePalette.PearlBlue, hpRatio);
            _soulBeamShader.Parameters["uColor"]?.SetValue(bodyColor.ToVector4());
            _soulBeamShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector4());
            _soulBeamShader.Parameters["uOpacity"]?.SetValue(0.5f * pulse);
            _soulBeamShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _soulBeamShader.Parameters["uIntensity"]?.SetValue(1f);
            _soulBeamShader.Parameters["uOverbrightMult"]?.SetValue(1.1f);
            _soulBeamShader.Parameters["uScrollSpeed"]?.SetValue(1f);
            _soulBeamShader.Parameters["uDistortionAmt"]?.SetValue(0.015f);
            _soulBeamShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _soulBeamShader.CurrentTechnique = _soulBeamShader.Techniques["SoulBeamAura"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _soulBeamShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float bodyScale = 28f / sc.Width;
            float bodyRot = Main.GameUpdateCount * 0.02f;
            sb.Draw(sc, drawPos, null, Color.White, bodyRot, sc.Size() * 0.5f, bodyScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: PearlShimmer facet overlay ----
        private void DrawPearlFacetOverlay(SpriteBatch sb, Matrix matrix)
        {
            _pearlGlowShader ??= ShaderLoader.ClairDeLunePearlGlow;
            if (_pearlGlowShader == null) return;

            sb.End();

            _pearlGlowShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector4());
            _pearlGlowShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.PearlBlue.ToVector4());
            _pearlGlowShader.Parameters["uOpacity"]?.SetValue(0.2f);
            _pearlGlowShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _pearlGlowShader.Parameters["uIntensity"]?.SetValue(0.7f);
            _pearlGlowShader.Parameters["uOverbrightMult"]?.SetValue(1f);
            _pearlGlowShader.Parameters["uScrollSpeed"]?.SetValue(1.2f);
            _pearlGlowShader.Parameters["uDistortionAmt"]?.SetValue(0.01f);
            _pearlGlowShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _pearlGlowShader.CurrentTechnique = _pearlGlowShader.Techniques["PearlShimmer"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _pearlGlowShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float bodyRot = Main.GameUpdateCount * 0.02f;

            // 4 facet quads to simulate crystal structure
            for (int i = 0; i < 4; i++)
            {
                float facetAngle = bodyRot + i * MathHelper.PiOver2;
                Vector2 facetOffset = facetAngle.ToRotationVector2() * 4f;
                sb.Draw(sc, drawPos + facetOffset, null, Color.White, facetAngle, sc.Size() * 0.5f,
                    12f / sc.Width, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom + target indicator ----
        private void DrawBloomCrystal(SpriteBatch sb, Matrix matrix)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            float pulse = 0.85f + 0.15f * MathF.Sin(Main.GameUpdateCount * 0.07f);

            Player owner = Main.player[Projectile.owner];
            float hpRatio = owner.statLife / (float)owner.statLifeMax2;
            Color hpColor = Color.Lerp(ClairDeLunePalette.TemporalCrimson, ClairDeLunePalette.PearlBlue, hpRatio);

            // HP-linked outer glow
            sb.Draw(srb, pos, null,
                hpColor with { A = 0 } * 0.15f * pulse, 0f, srb.Size() * 0.5f,
                22f / srb.Width, SpriteEffects.None, 0f);

            // PearlBlue mid
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.PearlBlue with { A = 0 } * 0.2f, 0f, srb.Size() * 0.5f,
                14f / srb.Width, SpriteEffects.None, 0f);

            // PearlWhite core
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.PearlWhite with { A = 0 } * 0.4f, 0f, srb.Size() * 0.5f,
                6f / srb.Width, SpriteEffects.None, 0f);

            // Beam target indicator
            if (_beamTarget != -1 && Main.npc[_beamTarget].active)
            {
                Vector2 targetDir = (Main.npc[_beamTarget].Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Vector2 indicatorPos = pos + targetDir * 18f;
                sb.Draw(srb, indicatorPos, null,
                    ClairDeLunePalette.WhiteHot with { A = 0 } * 0.2f, 0f, srb.Size() * 0.5f,
                    3f / srb.Width, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
