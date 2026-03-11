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

namespace MagnumOpus.Content.ClairDeLune.Weapons.GearDrivenArbiter.Projectiles
{
    /// <summary>
    /// Arbiter Minion — clockwork construct that orbits and fires gear projectiles.
    /// Tracks judgment stacks per NPC. At 8 stacks, triggers Arbiter's Verdict.
    /// 3 render passes: (1) JudgmentMark.fx JudgmentMarkSigil clockwork body,
    /// (2) ClairDeLuneMoonlit.fx MoonlitGlow ambient wrap, (3) Multi-scale bloom + gear teeth.
    /// </summary>
    public class ArbiterMinionProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/GearDrivenArbiter/GearDrivenArbiterMinion";

        private float _orbitAngle;
        private int _fireTimer;
        private int _courtTimer;
        private const int FireCooldown = 25;
        private const int CourtBarrageCooldown = 480;
        private const float OrbitRadius = 70f;
        private const float DetectionRange = 700f;

        // --- Shader + texture caching ---
        private static Effect _judgmentShader;
        private static Effect _moonlitShader;
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
            Projectile.width = 28;
            Projectile.height = 28;
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
                owner.ClearBuff(ModContent.BuffType<GearDrivenArbiterBuff>());
                Projectile.Kill();
                return;
            }

            if (owner.HasBuff(ModContent.BuffType<GearDrivenArbiterBuff>()))
                Projectile.timeLeft = 2;

            int arbiterCount = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].owner == Projectile.owner &&
                    Main.projectile[i].type == Projectile.type)
                    arbiterCount++;
            }

            float orbiterIndex = 0;
            for (int i = 0; i < Projectile.whoAmI; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].owner == Projectile.owner &&
                    Main.projectile[i].type == Projectile.type)
                    orbiterIndex++;
            }

            float angleOffset = arbiterCount > 1 ? orbiterIndex * MathHelper.TwoPi / arbiterCount : 0f;
            _orbitAngle += MathHelper.ToRadians(1.8f);

            Vector2 targetPos = owner.Center + new Vector2(
                MathF.Cos(_orbitAngle + angleOffset) * OrbitRadius,
                MathF.Sin(_orbitAngle + angleOffset) * OrbitRadius * 0.6f - 30f
            );

            Vector2 toTarget = targetPos - Projectile.Center;
            Projectile.velocity = toTarget * 0.12f;

            int target = FindTarget(owner);

            _fireTimer++;
            if (_fireTimer >= FireCooldown && target != -1)
            {
                _fireTimer = 0;
                FireGear(owner, target);
            }

            _courtTimer++;
            if (_courtTimer >= CourtBarrageCooldown && arbiterCount >= 3 && target != -1)
            {
                _courtTimer = 0;

                int courtDmg = (int)(Projectile.damage * 1.3f);
                Vector2 dir = (Main.npc[target].Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                    dir * 12f, ModContent.ProjectileType<ArbiterGearProjectile>(),
                    courtDmg, Projectile.knockBack * 1.5f, owner.whoAmI, 1f);

                SoundEngine.PlaySound(SoundID.Item22 with { Pitch = 0.3f, Volume = 0.5f }, Projectile.Center);

                var courtFlash = new BloomParticle(Projectile.Center, Vector2.Zero,
                    ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.4f, 0.2f, 12);
                MagnumParticleHandler.SpawnParticle(courtFlash);
            }

            if (Main.rand.NextBool(6))
            {
                float sparkAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = sparkAngle.ToRotationVector2() * Main.rand.NextFloat(8, 14);
                var spark = new GlowSparkParticle(Projectile.Center + offset,
                    sparkAngle.ToRotationVector2() * 0.3f,
                    ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.25f,
                    0.04f, 15);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.ClockworkBrass.ToVector3() * 0.2f);
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
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = i;
                }
            }
            return closest;
        }

        private void FireGear(Player owner, int target)
        {
            Vector2 dir = (Main.npc[target].Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                dir * 10f, ModContent.ProjectileType<ArbiterGearProjectile>(),
                Projectile.damage, Projectile.knockBack, owner.whoAmI, 0f);

            SoundEngine.PlaySound(SoundID.Item23 with { Pitch = 0.2f, Volume = 0.3f }, Projectile.Center);
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
            try
            {
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            // ── MINION SPRITE: Draw base PNG sprite ──
            Texture2D minionTex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 minionOrigin = minionTex.Size() / 2f;
            sb.Draw(minionTex, drawPos, null, lightColor * Projectile.Opacity, Projectile.rotation, minionOrigin, Projectile.scale, SpriteEffects.None, 0f);

            DrawJudgmentBody(sb, matrix);        // Pass 1: JudgmentMarkSigil body
            DrawMoonlitAmbient(sb, matrix);      // Pass 2: MoonlitGlow ambient
            DrawBloomGearTeeth(sb, matrix);      // Pass 3: Bloom + gear teeth
            ClairDeLuneVFXLibrary.DrawThemeAccents(sb, Projectile.Center, 0.5f, 0.3f);
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

        // ---- PASS 1: JudgmentMark.fx JudgmentMarkSigil body ----
        private void DrawJudgmentBody(SpriteBatch sb, Matrix matrix)
        {
            _judgmentShader ??= ShaderLoader.JudgmentMark;
            if (_judgmentShader == null) return;

            sb.End();

            _judgmentShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.ClockworkBrass.ToVector4());
            _judgmentShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _judgmentShader.Parameters["uOpacity"]?.SetValue(0.5f);
            _judgmentShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _judgmentShader.Parameters["uIntensity"]?.SetValue(1f);
            _judgmentShader.Parameters["uOverbrightMult"]?.SetValue(1f);
            _judgmentShader.Parameters["uScrollSpeed"]?.SetValue(1.5f);
            _judgmentShader.Parameters["uDistortionAmt"]?.SetValue(0.01f);
            _judgmentShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _judgmentShader.CurrentTechnique = _judgmentShader.Techniques["JudgmentMarkSigil"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _judgmentShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float bodyScale = 30f / sc.Width;
            float bodyRot = Main.GameUpdateCount * 0.03f;
            sb.Draw(sc, drawPos, null, Color.White, bodyRot, sc.Size() * 0.5f, bodyScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: ClairDeLuneMoonlit.fx MoonlitGlow ambient ----
        private void DrawMoonlitAmbient(SpriteBatch sb, Matrix matrix)
        {
            _moonlitShader ??= ShaderLoader.ClairDeLuneMoonlit;
            if (_moonlitShader == null) return;

            sb.End();

            _moonlitShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector4());
            _moonlitShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.PearlBlue.ToVector4());
            _moonlitShader.Parameters["uOpacity"]?.SetValue(0.15f);
            _moonlitShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _moonlitShader.Parameters["uIntensity"]?.SetValue(0.6f);
            _moonlitShader.Parameters["uOverbrightMult"]?.SetValue(1f);
            _moonlitShader.Parameters["uScrollSpeed"]?.SetValue(0.8f);
            _moonlitShader.Parameters["uDistortionAmt"]?.SetValue(0.005f);
            _moonlitShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _moonlitShader.CurrentTechnique = _moonlitShader.Techniques["MoonlitGlow"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _moonlitShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, 40f / sc.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom + gear teeth ----
        private void DrawBloomGearTeeth(SpriteBatch sb, Matrix matrix)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            float gearRot = Main.GameUpdateCount * 0.03f;

            // 12 gear teeth
            for (int i = 0; i < 12; i++)
            {
                float angle = gearRot + i * MathHelper.TwoPi / 12f;
                Vector2 toothPos = pos + angle.ToRotationVector2() * 12f;
                sb.Draw(srb, toothPos, null,
                    ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.25f, 0f, srb.Size() * 0.5f,
                    3f / srb.Width, SpriteEffects.None, 0f);
            }

            // MoonbeamGold outer glow
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.2f, 0f, srb.Size() * 0.5f,
                18f / srb.Width, SpriteEffects.None, 0f);

            // ClockworkBrass mechanism
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.3f, 0f, srb.Size() * 0.5f,
                10f / srb.Width, SpriteEffects.None, 0f);

            // PearlWhite core
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.PearlWhite with { A = 0 } * 0.35f, 0f, srb.Size() * 0.5f,
                4f / srb.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
