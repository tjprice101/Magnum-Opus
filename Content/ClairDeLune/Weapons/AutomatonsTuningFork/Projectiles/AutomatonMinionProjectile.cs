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
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.ClairDeLune.Weapons.AutomatonsTuningFork.Projectiles
{
    /// <summary>
    /// Automaton Minion - shader-driven tuning fork summon that cycles 4 frequency notes.
    /// 3 render passes: (1) ResonanceFieldPulse body aura, (2) ClairDeLuneMoonlit ambient glow,
    /// (3) Bloom stacking for frequency indicator ring, prong tips, core, conductor buildup.
    /// Emits FrequencyZone projectiles and Conductor's Final Note every 30s.
    /// </summary>
    public class AutomatonMinionProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int _currentFrequency; // 0=A, 1=C, 2=E, 3=G
        private int _frequencyTimer;
        private int _conductorTimer;
        private const int FrequencyCycleDuration = 90;
        private const int ConductorCooldown = 1800;
        private const float ZoneRadius = 128f;
        private const float DetectionRange = 600f;

        private static readonly Color FreqAColor = ClairDeLunePalette.MoonbeamGold;
        private static readonly Color FreqCColor = ClairDeLunePalette.NightMist;
        private static readonly Color FreqEColor = ClairDeLunePalette.SoftBlue;
        private static readonly Color FreqGColor = ClairDeLunePalette.PearlBlue;

        // --- Shader + texture caching ---
        private static Effect _resonanceShader;
        private static Effect _moonlitShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _starFlare;
        private static Asset<Texture2D> _noiseTex;

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
                owner.ClearBuff(ModContent.BuffType<AutomatonsTuningForkBuff>());
                Projectile.Kill();
                return;
            }

            if (owner.HasBuff(ModContent.BuffType<AutomatonsTuningForkBuff>()))
                Projectile.timeLeft = 2;

            // Follow owner (hover above right side)
            Vector2 targetPos = owner.Center + new Vector2(owner.direction * 40f, -50f);
            Vector2 toTarget = targetPos - Projectile.Center;
            Projectile.velocity = toTarget * 0.1f;

            // Gentle hovering bob
            Projectile.position.Y += MathF.Sin(Main.GameUpdateCount * 0.05f) * 0.3f;

            // Cycle through frequencies
            _frequencyTimer++;
            if (_frequencyTimer >= FrequencyCycleDuration)
            {
                _frequencyTimer = 0;
                EmitFrequencyZone(owner);
                _currentFrequency = (_currentFrequency + 1) % 4;
            }

            // Conductor's Final Note
            _conductorTimer++;
            if (_conductorTimer >= ConductorCooldown)
            {
                _conductorTimer = 0;
                EmitConductorsNote(owner);
            }

            ApplyFrequencyEffects(owner);

            // Ambient resonance particles
            if (Main.rand.NextBool(5))
            {
                Color freqColor = GetFrequencyColor(_currentFrequency);
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(10, 18);

                var sparkle = new SparkleParticle(Projectile.Center + offset,
                    angle.ToRotationVector2() * 0.3f,
                    freqColor with { A = 0 } * 0.25f, 0.05f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Tuning fork vibration particles
            if (Main.rand.NextBool(8))
            {
                float vibAngle = Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vibVel = new Vector2(vibAngle, -1f) * 0.5f;
                var vib = new GenericGlowParticle(Projectile.Center + new Vector2(0, -10),
                    vibVel, ClairDeLunePalette.PearlWhite with { A = 0 } * 0.2f, 0.03f, 15);
                MagnumParticleHandler.SpawnParticle(vib);
            }

            Lighting.AddLight(Projectile.Center, GetFrequencyColor(_currentFrequency).ToVector3() * 0.2f);
        }

        private void EmitFrequencyZone(Player owner)
        {
            Vector2 zonePos;
            int target = FindTarget(owner);

            if ((_currentFrequency == 0 || _currentFrequency == 3) && target != -1)
                zonePos = Main.npc[target].Center;
            else
                zonePos = owner.Center;

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), zonePos, Vector2.Zero,
                ModContent.ProjectileType<FrequencyZoneProjectile>(),
                (int)(Projectile.damage * 0.4f), 0f, owner.whoAmI, _currentFrequency);

            SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.2f + _currentFrequency * 0.15f, Volume = 0.3f },
                Projectile.Center);

            Color emitColor = GetFrequencyColor(_currentFrequency);
            var flash = new BloomParticle(Projectile.Center, Vector2.Zero,
                emitColor with { A = 0 } * 0.4f, 0.2f, 10);
            MagnumParticleHandler.SpawnParticle(flash);
        }

        private void EmitConductorsNote(Player owner)
        {
            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.3f, Volume = 0.8f }, Projectile.Center);

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), owner.Center, Vector2.Zero,
                ModContent.ProjectileType<ConductorFinalNoteProjectile>(),
                (int)(Projectile.damage * 2f), 8f, owner.whoAmI);

            for (int i = 0; i < 4; i++)
            {
                Color color = GetFrequencyColor(i);
                float angle = i * MathHelper.PiOver2;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                var flash = new BloomParticle(Projectile.Center, vel,
                    color with { A = 0 } * 0.5f, 0.3f, 15);
                MagnumParticleHandler.SpawnParticle(flash);
            }
        }

        private void ApplyFrequencyEffects(Player owner)
        {
            float activeRadius = ZoneRadius * 0.5f;

            switch (_currentFrequency)
            {
                case 1:
                    if (Vector2.Distance(Projectile.Center, owner.Center) < activeRadius)
                        owner.statDefense += 10;
                    break;
                case 2:
                    if (Vector2.Distance(Projectile.Center, owner.Center) < activeRadius)
                    {
                        owner.moveSpeed += 0.15f;
                        owner.maxRunSpeed *= 1.15f;
                    }
                    break;
            }
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

        private Color GetFrequencyColor(int freq) => freq switch
        {
            0 => FreqAColor,
            1 => FreqCColor,
            2 => FreqEColor,
            3 => FreqGColor,
            _ => ClairDeLunePalette.PearlWhite
        };

        private void LoadTextures()
        {
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _starFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare", AssetRequestMode.ImmediateLoad);
            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/PerlinNoise", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            Color currentColor = GetFrequencyColor(_currentFrequency);
            float freqProgress = _frequencyTimer / (float)FrequencyCycleDuration;
            float pulse = 0.85f + 0.15f * MathF.Sin(Main.GameUpdateCount * 0.08f);

            DrawResonanceBodyAura(sb, matrix, currentColor, pulse);   // Pass 1
            DrawMoonlitAmbient(sb, matrix, currentColor, pulse);      // Pass 2
            DrawBloomComposite(sb, matrix, currentColor, pulse);      // Pass 3
            ClairDeLuneVFXLibrary.DrawThemeAccents(sb, Projectile.Center, 0.5f, 0.3f);
            return false;
        }

        // ---- PASS 1: ResonanceFieldPulse body aura via CDL shader ----
        private void DrawResonanceBodyAura(SpriteBatch sb, Matrix matrix, Color freqColor, float pulse)
        {
            _resonanceShader ??= ShaderLoader.ResonanceField;
            if (_resonanceShader == null) return;

            sb.End();

            _resonanceShader.Parameters["uColor"]?.SetValue(freqColor.ToVector4());
            _resonanceShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector4());
            _resonanceShader.Parameters["uOpacity"]?.SetValue(0.4f * pulse);
            _resonanceShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _resonanceShader.Parameters["uIntensity"]?.SetValue(1.0f);
            _resonanceShader.Parameters["uOverbrightMult"]?.SetValue(1.2f);
            _resonanceShader.Parameters["uScrollSpeed"]?.SetValue(2.0f);
            _resonanceShader.Parameters["uDistortionAmt"]?.SetValue(0.01f);
            _resonanceShader.Parameters["uHasSecondaryTex"]?.SetValue(_noiseTex != null);

            if (_noiseTex != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = _noiseTex.Value;
                _resonanceShader.Parameters["uSecondaryTexScale"]?.SetValue(2.0f);
            }

            _resonanceShader.CurrentTechnique = _resonanceShader.Techniques["ResonanceFieldPulse"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _resonanceShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float auraSize = 48f; // Small aura around minion body
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f,
                auraSize / sc.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: ClairDeLuneMoonlit ambient glow via CDL shader ----
        private void DrawMoonlitAmbient(SpriteBatch sb, Matrix matrix, Color freqColor, float pulse)
        {
            _moonlitShader ??= ShaderLoader.ClairDeLuneMoonlit;
            if (_moonlitShader == null) return;

            sb.End();

            _moonlitShader.Parameters["uColor"]?.SetValue(freqColor.ToVector4());
            _moonlitShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.DreamHaze.ToVector4());
            _moonlitShader.Parameters["uOpacity"]?.SetValue(0.25f * pulse);
            _moonlitShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _moonlitShader.Parameters["uIntensity"]?.SetValue(0.8f);
            _moonlitShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _moonlitShader.Parameters["uScrollSpeed"]?.SetValue(0.5f);
            _moonlitShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _moonlitShader.CurrentTechnique = _moonlitShader.Techniques["MoonlitGlow"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _moonlitShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float ambientSize = 64f;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f,
                ambientSize / sc.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Bloom composite (prongs, indicators, core, conductor buildup) ----
        private void DrawBloomComposite(SpriteBatch sb, Matrix matrix, Color freqColor, float pulse)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            Texture2D sf = _starFlare.Value;

            float vibration = MathF.Sin(Main.GameUpdateCount * 0.3f) * 2f;

            // Tuning fork prong tips
            Vector2 leftProng = drawPos + new Vector2(-4 + vibration * 0.3f, -8);
            Vector2 rightProng = drawPos + new Vector2(4 - vibration * 0.3f, -8);

            sb.Draw(pb, leftProng, null, freqColor with { A = 0 } * 0.35f * pulse,
                0f, pb.Size() * 0.5f, 6f / pb.Width, SpriteEffects.None, 0f);
            sb.Draw(pb, rightProng, null, freqColor with { A = 0 } * 0.35f * pulse,
                0f, pb.Size() * 0.5f, 6f / pb.Width, SpriteEffects.None, 0f);

            // Frequency indicator ring (4 colored stars)
            for (int i = 0; i < 4; i++)
            {
                float angle = i * MathHelper.PiOver2 + Main.GameUpdateCount * 0.02f;
                Vector2 dotPos = drawPos + angle.ToRotationVector2() * 16f;
                Color dotColor = GetFrequencyColor(i);
                float dotAlpha = i == _currentFrequency ? 0.5f : 0.15f;
                float dotScale = i == _currentFrequency ? 8f : 4f;

                sb.Draw(sf, dotPos, null, dotColor with { A = 0 } * dotAlpha,
                    angle, sf.Size() * 0.5f, dotScale / sf.Width, SpriteEffects.None, 0f);
            }

            // Body ambient halo
            sb.Draw(srb, drawPos, null, freqColor with { A = 0 } * 0.15f * pulse,
                0f, srb.Size() * 0.5f, 32f / srb.Width, SpriteEffects.None, 0f);

            // Core
            sb.Draw(pb, drawPos, null, ClairDeLunePalette.PearlWhite with { A = 0 } * 0.4f,
                0f, pb.Size() * 0.5f, 6f / pb.Width, SpriteEffects.None, 0f);

            // Conductor timer indicator (builds urgency near Final Note)
            float conductorProgress = _conductorTimer / (float)ConductorCooldown;
            if (conductorProgress > 0.8f)
            {
                float urgency = (conductorProgress - 0.8f) / 0.2f;
                float urgencyPulse = 0.7f + 0.3f * MathF.Sin(Main.GameUpdateCount * 0.15f * urgency);

                sb.Draw(srb, drawPos, null, ClairDeLunePalette.WhiteHot with { A = 0 } * 0.2f * urgency * urgencyPulse,
                    0f, srb.Size() * 0.5f, (28f + urgency * 12f) / srb.Width, SpriteEffects.None, 0f);

                // Star flare at prong tips intensifies
                sb.Draw(sf, leftProng, null, ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.25f * urgency,
                    Main.GameUpdateCount * 0.05f, sf.Size() * 0.5f, 10f / sf.Width, SpriteEffects.None, 0f);
                sb.Draw(sf, rightProng, null, ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.25f * urgency,
                    -Main.GameUpdateCount * 0.05f, sf.Size() * 0.5f, 10f / sf.Width, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullNone, null, matrix);
        }
    }
}
