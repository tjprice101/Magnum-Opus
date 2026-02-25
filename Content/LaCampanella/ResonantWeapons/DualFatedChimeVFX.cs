using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons
{
    /// <summary>
    /// VFX helper for the Dual-Fated Chime melee weapon.
    /// Handles hold-item ambient, world item bloom, swing frame VFX,
    /// combo impacts, Inferno Waltz special, and finisher effects.
    /// Call from DualFatedChime and DualFatedChimeSwing.
    /// </summary>
    public static class DualFatedChimeVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: orbiting fire motes, ember sparkles,
        /// ambient smoke, and periodic music notes.
        /// Call from HoldItem().
        /// </summary>
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // 3-point orbiting fire motes (cycling)
            for (int i = 0; i < 3; i++)
            {
                float angle = time * 0.04f + MathHelper.TwoPi * i / 3f;
                float radius = 18f + MathF.Sin(time * 0.06f + i) * 4f;
                Vector2 motePos = center + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(3))
                {
                    Color col = LaCampanellaPalette.PaletteLerp(LaCampanellaPalette.DualFatedChimeBlade,
                        0.3f + (float)i / 3f * 0.5f);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.Torch, Vector2.Zero, 0, col, 0.8f);
                    d.noGravity = true;
                    d.fadeIn = 0.6f;
                }
            }

            // Prismatic ember sparkles
            if (Main.rand.NextBool(4))
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(20f, 20f);
                Color sparkCol = LaCampanellaPalette.GetShimmer(time);
                Dust d = Dust.NewDustPerfect(sparkPos, DustID.Enchanted_Gold, Vector2.Zero, 0, sparkCol, 0.6f);
                d.noGravity = true;
            }

            // Ambient smoke
            LaCampanellaVFXLibrary.SpawnAmbientSmoke(center, 25f);

            // Periodic music notes
            if (Main.rand.NextBool(25))
                LaCampanellaVFXLibrary.SpawnMusicNotes(center, 1, 20f, 0.7f, 0.9f, 30);

            // Light pulse
            float pulse = 0.5f + MathF.Sin(time * 0.05f) * 0.15f;
            Lighting.AddLight(center, LaCampanellaPalette.InfernalOrange.ToVector3() * pulse);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        /// <summary>
        /// 4-layer bloom for DualFatedChime when lying in the world.
        /// Call from PreDrawInWorld.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            LaCampanellaPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  SWING FRAME VFX
        // =====================================================================

        /// <summary>
        /// Per-frame swing VFX for DualFatedChimeSwing projectile.
        /// Dense fire dust, ember scatter, smoke trail, music notes.
        /// </summary>
        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep, int timer)
        {
            if (Main.dedServ) return;

            // Dense fire dust at blade tip
            LaCampanellaVFXLibrary.SpawnSwingDust(tipPos, -swordDirection, DustID.Torch);

            // Bronze contrast sparkle
            LaCampanellaVFXLibrary.SpawnContrastSparkle(tipPos, -swordDirection);

            // Ember scatter intensifies with combo
            if (timer % (3 - Math.Min(comboStep, 2)) == 0)
                LaCampanellaVFXLibrary.SpawnEmberScatter(tipPos, 2 + comboStep, 2f);

            // Smoke trail (heavier on later combos)
            if (timer % (5 - comboStep) == 0)
                LaCampanellaVFXLibrary.SpawnHeavySmoke(tipPos, 1 + comboStep / 2, 0.4f, 1.5f, 30);

            // Periodic music notes
            if (timer % 5 == 0)
                LaCampanellaVFXLibrary.SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            // Dynamic light
            Color lightCol = LaCampanellaPalette.PaletteLerp(LaCampanellaPalette.DualFatedChimeBlade,
                0.4f + comboStep * 0.15f);
            Lighting.AddLight(tipPos, lightCol.ToVector3() * 0.6f);
        }

        // =====================================================================
        //  COMBO IMPACTS
        // =====================================================================

        /// <summary>
        /// On-hit impact VFX scaled by combo step.
        /// Phase 0: Bell strike, Phase 1: Toll sweep, Phase 2: Grand toll.
        /// </summary>
        public static void ComboImpact(Vector2 pos, int comboStep)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.MeleeImpact(pos, comboStep);

            // Extra bell chime on later combos
            if (comboStep >= 1)
                CustomParticles.LaCampanellaBellChime(pos, 8 + comboStep * 4);

            // Grand toll: heavy bloom + screen shake
            if (comboStep >= 2)
            {
                LaCampanellaVFXLibrary.DrawBloom(pos, 0.7f);
                MagnumScreenEffects.AddScreenShake(4f);
                LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 4, 0.8f, 3f, 50);
            }
        }

        // =====================================================================
        //  INFERNO WALTZ SPECIAL
        // =====================================================================

        /// <summary>
        /// Inferno Waltz activation burst — spinning flame dance VFX.
        /// Call when right-click special is triggered.
        /// </summary>
        public static void InfernoWaltzActivation(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Central fire flash
            LaCampanellaVFXLibrary.DrawBloom(pos, 1.0f);

            // Spinning flame ring (12 points)
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 dustPos = pos + angle.ToRotationVector2() * 40f;
                Vector2 vel = angle.ToRotationVector2() * 4f;
                Color col = LaCampanellaPalette.GetFireGradient((float)i / 12f);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, vel, 0, col, 1.8f);
                d.noGravity = true;
            }

            // Bell shockwave
            LaCampanellaVFXLibrary.BellShockwaveImpact(pos, 0.8f);

            // Heavy smoke burst
            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 8, 1.0f, 4f, 60);

            // Music notes scatter
            LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 8, 40f, 0.8f, 1.2f, 40);

            MagnumScreenEffects.AddScreenShake(6f);
        }

        /// <summary>
        /// Per-frame Inferno Waltz VFX during the spinning dance.
        /// </summary>
        public static void InfernoWaltzFrameVFX(Vector2 playerCenter, float spinAngle)
        {
            if (Main.dedServ) return;

            // Orbiting fire ring
            for (int i = 0; i < 4; i++)
            {
                float offset = MathHelper.TwoPi * i / 4f;
                Vector2 pos = playerCenter + (spinAngle + offset).ToRotationVector2() * 35f;
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, Vector2.Zero, 0, col, 1.4f);
                d.noGravity = true;
            }

            // Ember scatter
            LaCampanellaVFXLibrary.SpawnEmberScatter(playerCenter, 3, 3f);

            // Heavy smoke
            LaCampanellaVFXLibrary.SpawnHeavySmoke(playerCenter, 2, 0.6f, 2f, 40);

            Lighting.AddLight(playerCenter, LaCampanellaPalette.FlameYellow.ToVector3() * 0.8f);
        }
    }
}
