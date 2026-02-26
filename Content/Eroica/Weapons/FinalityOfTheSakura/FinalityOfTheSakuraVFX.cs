using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX.Screen;

namespace MagnumOpus.Content.Eroica.Weapons.FinalityOfTheSakura
{
    /// <summary>
    /// Static VFX helper for Finality of the Sakura — a post-Moon Lord summon weapon
    /// that calls forth the Sakura of Fate minion. Theme: "a final blossom before
    /// eternal night" — dark, dramatic, fatalistic.
    ///
    /// The visual language is that of a last stand: black flames consuming crimson
    /// petals, doom-lit halos, fading sakura beauty against an encroaching void.
    /// The minion fires projectiles of black and scarlet flame — beautiful and terminal.
    ///
    /// All shared VFX routed through EroicaVFXLibrary for canonical palette.
    /// </summary>
    public static class FinalityOfTheSakuraVFX
    {
        // ══════════════════════════════════════════════════════════════
        //  UNIQUE IDENTITY COLORS  (fate-specific accents)
        // ══════════════════════════════════════════════════════════════

        /// <summary>Absolute black of fate — the void that waits at the end.</summary>
        private static readonly Color FateBlack = new Color(20, 10, 15);

        /// <summary>Doom crimson — the last color the condemned see.</summary>
        private static readonly Color DoomCrimson = new Color(160, 30, 40);

        /// <summary>Fading sakura — the final petal before the dark.</summary>
        private static readonly Color SakuraFinale = new Color(255, 130, 160);

        /// <summary>Eternal white — the absolute light that precedes oblivion.</summary>
        private static readonly Color EternalWhite = new Color(245, 240, 235);

        /// <summary>Abyss flame — fire that burns without warmth, only ending.</summary>
        private static readonly Color AbyssFlame = new Color(100, 20, 30);

        /// <summary>Remnant gold — the last gleam of a hero's memory.</summary>
        private static readonly Color RemnantGold = new Color(200, 160, 80);

        /// <summary>Last petal — the delicate pink of something about to vanish.</summary>
        private static readonly Color LastPetal = new Color(255, 180, 200);

        /// <summary>Void smoke — the haze between life and what comes after.</summary>
        private static readonly Color VoidSmoke = new Color(40, 25, 35);

        // ══════════════════════════════════════════════════════════════
        //  1. SUMMON VFX  (called from Shoot — summoning the Sakura of Fate)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Dramatic summoning VFX: a converging crimson dust ring (16 points) collapses
        /// toward the summon point, dark smoke erupts outward, a heroic impact detonates,
        /// an EternalWhite bloom flare ignites, gradient halo rings cascade, 5 music notes
        /// carry the fatalistic hymn, god rays pierce the dark, and the screen shudders
        /// with both shake and distortion.
        /// The moment fate is invoked — the last blossom is called forth.
        /// </summary>
        public static void SummonExplosionVFX(Vector2 position)
        {
            // Converging crimson dust ring — 16 points spiraling inward like a collapsing fate
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 ringPos = position + angle.ToRotationVector2() * 90f;
                Vector2 toCenter = (position - ringPos).SafeNormalize(Vector2.Zero) * 4f;
                Color ringColor = Color.Lerp(DoomCrimson, FateBlack, (float)i / 16f);
                Dust d = Dust.NewDustPerfect(ringPos, DustID.Torch, toCenter, 0, ringColor, 1.4f);
                d.noGravity = true;
            }

            // Dark smoke burst — 12 particles erupting as the void exhales
            for (int i = 0; i < 12; i++)
            {
                Vector2 smokePos = position + Main.rand.NextVector2Circular(15f, 15f);
                Vector2 smokeVel = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0, -1.5f);
                var smoke = new HeavySmokeParticle(smokePos, smokeVel,
                    VoidSmoke, Main.rand.Next(35, 55), 0.35f, 0.55f, 0.014f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // Heroic impact — the weight of finality
            EroicaVFXLibrary.HeroicImpact(position, 1.4f);

            // Bloom flare — EternalWhite, the blinding instant of summoning
            EroicaVFXLibrary.BloomFlare(position, EternalWhite, 0.9f, 22);

            // Gradient halo rings — doom crimson cascading to fate black
            for (int i = 0; i < 5; i++)
            {
                float progress = (float)i / 5f;
                Color haloColor = Color.Lerp(DoomCrimson, AbyssFlame, progress);
                float scale = 0.35f + i * 0.12f;
                var ring = new BloomRingParticle(position, Vector2.Zero, haloColor, scale, 28, 0.07f);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            // Music notes — 5 somber notes, the fatalistic overture
            EroicaVFXLibrary.SpawnMusicNotes(position, 5, 40f, 0.8f, 1.1f, 38);

            // God rays — 6 rays piercing the dark like the last light of dawn
            GodRaySystem.CreateBurst(position, DoomCrimson, 6, 90f, 35,
                GodRaySystem.GodRayStyle.Explosion, AbyssFlame);

            // Screen shake — the world acknowledges fate's arrival
            MagnumScreenEffects.AddScreenShake(5f);

            // Screen distortion — reality ripples at the summon point
            ScreenDistortionManager.TriggerRipple(position, DoomCrimson, 0.7f, 24);

            Lighting.AddLight(position, EternalWhite.ToVector3() * 1.6f);
        }

        // ══════════════════════════════════════════════════════════════
        //  2. MINION AMBIENT  (called from minion AI every frame)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Ambient aura for the Sakura of Fate minion: every 6 frames, 3 orbiting
        /// fate-black motes circle the minion alongside scarlet embers. Every 25 frames
        /// a somber music note drifts upward. Pulsing AbyssFlame lighting breathes
        /// with the rhythm of impending finality.
        /// The minion is never still — fate is always turning.
        /// </summary>
        public static void MinionAmbientAura(Vector2 minionCenter, int frameCounter)
        {
            // Orbiting fate-black motes and scarlet embers — every 6 frames
            if (frameCounter % 6 == 0)
            {
                float orbitAngle = frameCounter * 0.04f;
                for (int i = 0; i < 3; i++)
                {
                    float moteAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 22f + MathF.Sin(frameCounter * 0.025f + i) * 5f;
                    Vector2 motePos = minionCenter + moteAngle.ToRotationVector2() * radius;

                    // Fate-black mote
                    Dust mote = Dust.NewDustPerfect(motePos, DustID.Torch, Vector2.Zero, 0, FateBlack, 1.1f);
                    mote.noGravity = true;

                    // Scarlet ember trailing the mote
                    Vector2 emberPos = minionCenter + (moteAngle - 0.3f).ToRotationVector2() * (radius + 4f);
                    Dust ember = Dust.NewDustPerfect(emberPos, DustID.CrimsonTorch,
                        new Vector2(0, -0.4f), 0, DoomCrimson, 0.9f);
                    ember.noGravity = true;
                }
            }

            // Somber music note — every 25 frames, the fate-hymn murmurs
            if (frameCounter % 25 == 0)
            {
                EroicaVFXLibrary.SpawnMusicNotes(minionCenter + new Vector2(0f, -18f), 1, 12f, 0.65f, 0.85f, 32);
            }

            // Pulsing AbyssFlame lighting — the heartbeat of finality
            float pulse = 0.35f + MathF.Sin(frameCounter * 0.045f) * 0.12f;
            Lighting.AddLight(minionCenter, AbyssFlame.ToVector3() * pulse);
        }

        // ══════════════════════════════════════════════════════════════
        //  3. MINION ATTACK  (fire, trail, hit, death)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Minion fires a black flame projectile: dark bloom flash at the barrel,
        /// directional sparks in the firing direction, and AbyssFlame dust scattered
        /// around the fire point. Each shot is a sentence pronounced.
        /// </summary>
        public static void MinionFireAttackVFX(Vector2 firePos, Vector2 direction)
        {
            // Dark bloom flash — the muzzle of doom
            EroicaVFXLibrary.BloomFlare(firePos, AbyssFlame, 0.5f, 12);

            // Directional sparks — DoomCrimson embers in the firing direction
            Vector2 fireDir = direction.SafeNormalize(Vector2.UnitX);
            for (int i = 0; i < 6; i++)
            {
                float spreadAngle = Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 vel = fireDir.RotatedBy(spreadAngle) * Main.rand.NextFloat(3f, 6f);
                Color col = Color.Lerp(FateBlack, DoomCrimson, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(firePos, DustID.Torch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            // AbyssFlame dust scatter around the fire point
            for (int i = 0; i < 4; i++)
            {
                Dust abyss = Dust.NewDustPerfect(firePos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.CrimsonTorch, Main.rand.NextVector2Circular(1f, 1f), 0, AbyssFlame, 1.1f);
                abyss.noGravity = true;
            }

            Lighting.AddLight(firePos, DoomCrimson.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Per-frame flame trail for black flame projectiles: alternating FateBlack
        /// and DoomCrimson dust, AbyssFlame glow dust, and occasional SakuraFinale
        /// sparkles — fading petals caught in the dark fire.
        /// Each frame the flame writes another line of the requiem.
        /// </summary>
        public static void MinionFlameTrailVFX(Projectile proj)
        {
            // Alternating FateBlack / DoomCrimson dust trail
            Vector2 trailVel = -proj.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f);
            Color trailColor = Main.rand.NextBool() ? FateBlack : DoomCrimson;
            Dust trail = Dust.NewDustPerfect(proj.Center + Main.rand.NextVector2Circular(4f, 4f),
                DustID.Torch, trailVel, 0, trailColor, 1.3f);
            trail.noGravity = true;

            // AbyssFlame glow dust — the fire beneath the black
            if (Main.rand.NextBool(2))
            {
                Dust glow = Dust.NewDustPerfect(proj.Center, DustID.CrimsonTorch,
                    -proj.velocity * 0.08f, 0, AbyssFlame, 1.0f);
                glow.noGravity = true;
            }

            // Occasional SakuraFinale sparkle — a petal dying in the flame
            if (Main.rand.NextBool(5))
            {
                Dust sparkle = Dust.NewDustPerfect(proj.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.PinkFairy, -proj.velocity * 0.05f, 0, SakuraFinale, 1.1f);
                sparkle.noGravity = true;
            }

            Lighting.AddLight(proj.Center, AbyssFlame.ToVector3() * 0.45f);
        }

        /// <summary>
        /// Black flame hits an enemy: dark bloom detonates, a crimson dust burst
        /// radiates outward, halo rings cascade in AbyssFlame tones, and a single
        /// somber music note marks the strike. Each hit is a knell tolled.
        /// </summary>
        public static void MinionFlameHitVFX(Vector2 hitPos)
        {
            // Dark bloom — the impact flash is swallowed by shadow
            EroicaVFXLibrary.BloomFlare(hitPos, DoomCrimson, 0.45f, 16);

            // Crimson burst — radial dust ring
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 5f);
                Color col = Color.Lerp(FateBlack, DoomCrimson, (float)i / 10f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.CrimsonTorch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            // Halo rings — AbyssFlame cascade
            for (int i = 0; i < 2; i++)
            {
                Color ringCol = Color.Lerp(AbyssFlame, DoomCrimson, i / 2f);
                var ring = new BloomRingParticle(hitPos, Vector2.Zero, ringCol * 0.7f,
                    0.3f + i * 0.1f, 20, 0.06f);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            // Single music note — the bell tolls
            EroicaVFXLibrary.SpawnMusicNotes(hitPos, 1, 10f, 0.7f, 0.9f, 28);

            Lighting.AddLight(hitPos, DoomCrimson.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Black flame projectile expires: fading dark embers scatter and drift,
        /// a VoidSmoke puff dissolves into nothing, and a small bloom flickers out.
        /// The sentence ends — the flame returns to the abyss.
        /// </summary>
        public static void MinionFlameDeathVFX(Vector2 pos)
        {
            // Fading dark embers — drifting like ash from a funeral pyre
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2.5f, 2.5f) + new Vector2(0, -0.4f);
                Color col = Color.Lerp(FateBlack, AbyssFlame, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, Main.rand.NextFloat(0.7f, 1.1f));
                d.noGravity = true;
            }

            // VoidSmoke puff — the last breath of the flame
            for (int i = 0; i < 3; i++)
            {
                Dust smoke = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.Smoke, Main.rand.NextVector2Circular(0.8f, 0.8f), 140, VoidSmoke, 0.9f);
                smoke.noGravity = true;
            }

            // Small bloom — a dim flicker, then nothing
            EroicaVFXLibrary.BloomFlare(pos, AbyssFlame, 0.2f, 10);

            Lighting.AddLight(pos, AbyssFlame.ToVector3() * 0.3f);
        }

        // ══════════════════════════════════════════════════════════════
        //  4. PREDRAW  (minion bloom + flame projectile rendering)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// 4-layer bloom for the Sakura of Fate minion: FateBlack outer void,
        /// AbyssFlame dark fire, DoomCrimson heart, SakuraFinale fading petal core.
        /// Uses {A=0} premultiplied alpha — no SpriteBatch restart needed.
        /// The minion glows like a dying star wrapped in shadow.
        /// </summary>
        public static void DrawMinionBloom(SpriteBatch sb, Vector2 minionWorldPos, float scale)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            Vector2 drawPos = minionWorldPos - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 3.5f) * 0.08f;

            // Shader-enhanced dark flame aura pass
            {
                Texture2D shaderGlow = MagnumTextureRegistry.GetSoftGlow();
                EroicaShaderManager.BeginShaderAdditive(sb);
                EroicaShaderManager.ApplyFinalityDarkFlameAura(Main.GlobalTimeWrappedHourly);
                Vector2 glowOrigin = shaderGlow.Size() * 0.5f;
                sb.Draw(shaderGlow, drawPos, null, Color.White * 0.5f, 0f,
                    glowOrigin, scale * 1.8f * pulse, SpriteEffects.None, 0f);
                EroicaShaderManager.RestoreSpriteBatch(sb);
            }

            // Layer 1: Outer void — FateBlack halo
            sb.Draw(bloom, drawPos, null,
                (FateBlack with { A = 0 }) * 0.2f, 0f, origin,
                scale * 2.2f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Dark fire — AbyssFlame
            sb.Draw(bloom, drawPos, null,
                (AbyssFlame with { A = 0 }) * 0.35f, 0f, origin,
                scale * 1.5f * pulse, SpriteEffects.None, 0f);

            // Layer 3: Crimson heart — DoomCrimson
            sb.Draw(bloom, drawPos, null,
                (DoomCrimson with { A = 0 }) * 0.45f, 0f, origin,
                scale * 1.0f * pulse, SpriteEffects.None, 0f);

            // Layer 4: Fading petal core — SakuraFinale
            sb.Draw(bloom, drawPos, null,
                (SakuraFinale with { A = 0 }) * 0.3f, 0f, origin,
                scale * 0.5f * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Custom rendering for black flame projectiles: dark {A=0} trail fading from
        /// FateBlack to DoomCrimson, afterimage with inverted bloom (dark outer, crimson
        /// inner), and a SakuraFinale petal-bright core pulsing at the center.
        /// Returns false to suppress vanilla drawing.
        /// </summary>
        public static bool DrawBlackFlameProjectile(SpriteBatch sb, Projectile proj, ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[proj.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 projScreen = proj.Center - Main.screenPosition;

            // {A=0} dark bloom trail — FateBlack
            EroicaVFXLibrary.DrawProjectileTrail(sb, proj, FateBlack);

            // Shader-enhanced dark funeral trail pass
            {
                Texture2D shaderGlow = MagnumTextureRegistry.GetSoftGlow();
                EroicaShaderManager.BeginShaderAdditive(sb);
                EroicaShaderManager.ApplyFinalityDarkFuneralTrail(Main.GlobalTimeWrappedHourly);
                Vector2 glowOrigin = shaderGlow.Size() * 0.5f;
                for (int k = 0; k < proj.oldPos.Length; k++)
                {
                    if (proj.oldPos[k] == Vector2.Zero) continue;
                    Vector2 shaderPos = proj.oldPos[k] - Main.screenPosition + new Vector2(proj.width / 2f, proj.height / 2f);
                    float shaderProgress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;
                    sb.Draw(shaderGlow, shaderPos, null, Color.White * shaderProgress * 0.45f, proj.oldRot[k],
                        glowOrigin, proj.scale * (0.35f + shaderProgress * 0.55f), SpriteEffects.None, 0f);
                }
                EroicaShaderManager.RestoreSpriteBatch(sb);
            }

            // Afterimage trail — FateBlack to DoomCrimson gradient
            for (int k = 0; k < proj.oldPos.Length; k++)
            {
                if (proj.oldPos[k] == Vector2.Zero) continue;

                Vector2 drawPos = proj.oldPos[k] - Main.screenPosition + new Vector2(proj.width / 2f, proj.height / 2f);
                float progress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;
                Color trailColor = Color.Lerp(FateBlack, DoomCrimson, progress);
                trailColor = (trailColor * progress) with { A = 0 };
                float trailScale = proj.scale * (0.4f + progress * 0.6f);

                sb.Draw(texture, drawPos, null, trailColor, proj.oldRot[k], drawOrigin, trailScale, SpriteEffects.None, 0f);
            }

            // Inverted bloom stack — dark outer, crimson inner, sakura core
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.09f) * 0.1f + 1f;
            Color outerGlow = (FateBlack with { A = 0 }) * 0.35f;
            Color midGlow = (DoomCrimson with { A = 0 }) * 0.4f;
            Color innerGlow = (SakuraFinale with { A = 0 }) * 0.3f;

            sb.Draw(texture, projScreen, null, outerGlow, proj.rotation, drawOrigin,
                proj.scale * 1.3f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, midGlow, proj.rotation, drawOrigin,
                proj.scale * 1.15f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, innerGlow, proj.rotation, drawOrigin,
                proj.scale * 1.05f * pulse, SpriteEffects.None, 0f);

            // SakuraFinale core — the petal at the heart of the black flame
            Color coreColor = new Color(255, 200, 210, 200) * pulse;
            sb.Draw(texture, projScreen, null, coreColor, proj.rotation, drawOrigin, proj.scale, SpriteEffects.None, 0f);

            return false;
        }

        // ══════════════════════════════════════════════════════════════
        //  5. SPECIAL  (fate descent / dismissal)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Minion spawn transition VFX: a dark portal ring forms, particles converge
        /// inward from all directions toward the spawn point, and a cascade of music
        /// notes descends like a fatalistic overture. Fate arrives.
        /// </summary>
        public static void FateDescentVFX(Vector2 pos)
        {
            // Dark portal ring — FateBlack expanding halo
            var portalRing = new BloomRingParticle(pos, Vector2.Zero, FateBlack, 0.6f, 35, 0.05f);
            MagnumParticleHandler.SpawnParticle(portalRing);
            var innerPortal = new BloomRingParticle(pos, Vector2.Zero, DoomCrimson, 0.4f, 30, 0.06f);
            MagnumParticleHandler.SpawnParticle(innerPortal);

            // Converging particles — drawn from all directions toward the spawn point
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 startPos = pos + angle.ToRotationVector2() * 70f;
                Vector2 toCenter = (pos - startPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 5f);
                Color col = Color.Lerp(DoomCrimson, AbyssFlame, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(startPos, DustID.CrimsonTorch, toCenter, 0, col, 1.2f);
                d.noGravity = true;
            }

            // Music cascade — 4 notes descending, the overture of fate
            EroicaVFXLibrary.SpawnMusicNotes(pos, 4, 30f, 0.75f, 1.0f, 35);

            // Dark bloom at the center
            EroicaVFXLibrary.BloomFlare(pos, AbyssFlame, 0.6f, 20);

            Lighting.AddLight(pos, DoomCrimson.ToVector3() * 1.0f);
        }

        /// <summary>
        /// Minion despawn transition VFX: sakura petals scatter outward in a final
        /// breath, the bloom fades from crimson through to nothing, and spirit motes
        /// ascend skyward — the last petals returning to the void.
        /// Fate departs. The blossom withers.
        /// </summary>
        public static void FateDismissVFX(Vector2 pos)
        {
            // Petal scatter — sakura petals bursting outward in a final exhale
            EroicaVFXLibrary.SpawnSakuraPetals(pos, 8, 45f);

            // Fading bloom — DoomCrimson to nothing
            EroicaVFXLibrary.BloomFlare(pos, DoomCrimson, 0.5f, 25);
            EroicaVFXLibrary.BloomFlare(pos, SakuraFinale, 0.3f, 15);

            // Ascending spirit motes — fragments of the minion drifting upward
            for (int i = 0; i < 8; i++)
            {
                Vector2 motePos = pos + Main.rand.NextVector2Circular(20f, 10f);
                Vector2 moteVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -2f - Main.rand.NextFloat(1.5f));
                Color moteColor = Color.Lerp(LastPetal, EternalWhite, Main.rand.NextFloat());
                Dust mote = Dust.NewDustPerfect(motePos, DustID.PinkFairy, moteVel, 0, moteColor, 1.0f);
                mote.noGravity = true;
            }

            // Halo ring — one final ring expanding into silence
            var farewell = new BloomRingParticle(pos, Vector2.Zero, SakuraFinale * 0.5f, 0.4f, 30, 0.04f);
            MagnumParticleHandler.SpawnParticle(farewell);

            Lighting.AddLight(pos, SakuraFinale.ToVector3() * 0.6f);
        }

        // ══════════════════════════════════════════════════════════════
        //  6. HOLD  (called from HoldItem every frame)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Dark summoner aura while holding the weapon: DoomCrimson glow pulses around
        /// the player, AbyssFlame embers drift upward, occasional sakura petals fall
        /// like the last leaves of autumn, and faint music notes murmur the fatalistic
        /// hymn. The weapon remembers what it will cost.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.gameMenu) return;

            // DoomCrimson glow — pulsing summoner aura
            if (Main.rand.NextBool(10))
            {
                float angle = Main.GameUpdateCount * 0.025f;
                float radius = 28f + MathF.Sin(Main.GameUpdateCount * 0.03f) * 6f;
                Vector2 glowPos = player.Center + angle.ToRotationVector2() * radius;
                Dust glow = Dust.NewDustPerfect(glowPos, DustID.CrimsonTorch, Vector2.Zero, 0, DoomCrimson, 0.9f);
                glow.noGravity = true;
            }

            // AbyssFlame embers — drifting upward from around the player
            if (Main.rand.NextBool(14))
            {
                Vector2 emberPos = player.Center + Main.rand.NextVector2Circular(25f, 15f);
                Vector2 emberVel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -0.8f - Main.rand.NextFloat(0.6f));
                Color emberColor = Color.Lerp(AbyssFlame, FateBlack, Main.rand.NextFloat(0.4f));
                Dust ember = Dust.NewDustPerfect(emberPos, DustID.Torch, emberVel, 0, emberColor, 0.9f);
                ember.noGravity = true;
            }

            // Sakura petals — falling like the last leaves before eternal night
            if (Main.rand.NextBool(20))
            {
                EroicaVFXLibrary.SpawnSakuraPetals(
                    player.Center + Main.rand.NextVector2Circular(30f, 30f), 1, 15f);
            }

            // Music notes — every 18 frames, the fate-hymn whispers
            if (Main.GameUpdateCount % 18 == 0)
            {
                EroicaVFXLibrary.SpawnMusicNotes(player.Center, 1, 18f, 0.6f, 0.8f, 30);
            }

            // Pulsing DoomCrimson light — the weapon's somber presence
            float pulse = MathF.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 0.9f;
            Lighting.AddLight(player.Center, DoomCrimson.ToVector3() * pulse * 0.4f);
        }
    }
}
