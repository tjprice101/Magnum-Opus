using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Buffs;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Projectiles;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver
{
    /// <summary>
    /// WRATH'S CLEAVER — Dies Irae Tier 8 Melee Greatsword.
    /// 4-phase wrath combo + Wrath Meter resource system.
    ///
    /// Combo Phases (musical movements of fury):
    ///   0 - Accusation:  Wide horizontal cleave, 2 crystallized flames
    ///   1 - Conviction:  Overhead slam, 4 spread flames + conviction smoke
    ///   2 - Execution:   270° spin, 6 ring flames + shockwave
    ///   3 - Damnation:   360° infernal eruption, 8 carpet flames + screen darken
    ///
    /// Wrath Meter (0–100):
    ///   +8 per hit, +12 per crit, +20 per kill. Decays 2/s when not swinging.
    ///   At 50+: swings leave lingering fire walls (2s)
    ///   At 100: auto-triggers Infernal Eruption (massive AoE)
    ///
    /// Held VFX: At 50+ Wrath, orbiting ember constellation + passive heat shimmer.
    /// </summary>
    public class WrathsCleaver : MeleeSwingItemBase
    {
        public override string Texture => "MagnumOpus/Content/DiesIrae/Weapons/WrathsCleaver/WrathsCleaver";

        private static readonly Color DiesIraeLore = new Color(200, 50, 30);

        // Wrath Meter state (per player via item instance)
        private float _wrathMeter;
        private int _wrathDecayTimer;

        protected override int SwingProjectileType
            => ModContent.ProjectileType<WrathsCleaverSwing>();

        protected override int ComboStepCount => 4;
        protected override int ComboResetDelay => 55;

        protected override Color GetLoreColor() => DiesIraeLore;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        protected override void SetWeaponDefaults()
        {
            Item.damage = 2200;
            Item.knockBack = 9f;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.crit = 20;
            Item.scale = 1.6f;
        }

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "4-phase wrath combo: Accusation, Conviction, Execution, Damnation"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Each swing spawns crystallized flames that persist as burning ground"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Hits build Wrath meter — at 50, swings leave lingering fire walls"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "At maximum Wrath, triggers Infernal Eruption marking all nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect5",
                "Marked enemies take 25% increased damage from all sources"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'The first blow of wrath is always the loudest — but the last shakes the earth itself.'")
            {
                OverrideColor = DiesIraeLore
            });
        }

        // ═══════════════════════════════════════════════════════════
        //  WRATH METER MANAGEMENT
        // ═══════════════════════════════════════════════════════════

        public float WrathMeter => _wrathMeter;

        public void AddWrath(float amount)
        {
            _wrathMeter = MathHelper.Clamp(_wrathMeter + amount, 0f, 100f);
            _wrathDecayTimer = 90; // Reset decay cooldown (1.5s)
        }

        public void ConsumeWrath(float amount)
        {
            _wrathMeter = MathHelper.Clamp(_wrathMeter - amount, 0f, 100f);
        }

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            // Decay wrath when not swinging
            _wrathDecayTimer--;
            if (_wrathDecayTimer <= 0)
            {
                _wrathMeter = MathHelper.Clamp(_wrathMeter - 2f / 60f, 0f, 100f);
            }

            // VFX at 50+ Wrath: orbiting ember constellation
            if (_wrathMeter >= 50f && Main.myPlayer == player.whoAmI)
            {
                float wrathNorm = (_wrathMeter - 50f) / 50f;
                float time = Main.GameUpdateCount * 0.04f;

                // Orbiting embers
                int orbitCount = 3 + (int)(wrathNorm * 3);
                if (Main.GameUpdateCount % 8 == 0)
                {
                    for (int i = 0; i < orbitCount; i++)
                    {
                        float angle = time * 1.5f + MathHelper.TwoPi * i / orbitCount;
                        float radius = 35f + wrathNorm * 15f;
                        Vector2 orbitPos = player.Center + angle.ToRotationVector2() * radius;
                        Vector2 vel = (angle + MathHelper.PiOver2).ToRotationVector2() * 0.5f;

                        Color col = Color.Lerp(DiesIraePalette.EmberOrange, DiesIraePalette.JudgmentGold, wrathNorm);
                        Dust d = Dust.NewDustPerfect(orbitPos, DustID.Torch, vel, 0, col, 0.7f + wrathNorm * 0.5f);
                        d.noGravity = true;
                        d.fadeIn = 0.8f;
                    }
                }

                // Passive heat aura damage at 80+
                if (_wrathMeter >= 80f && Main.GameUpdateCount % 15 == 0)
                {
                    float auraDmg = Item.damage * 0.03f;
                    float auraRadius = 96f + wrathNorm * 48f;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && !npc.dontTakeDamage
                            && npc.Distance(player.Center) < auraRadius)
                        {
                            player.ApplyDamageToNPC(npc, (int)auraDmg, 0f, 0, false);
                        }
                    }
                }

                Lighting.AddLight(player.Center, DiesIraePalette.InfernalRed.ToVector3() * (0.3f + wrathNorm * 0.4f));
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  PER-PHASE FLAME SPAWNING
        // ═══════════════════════════════════════════════════════════

        protected override void OnShoot(Player player, int projectileIndex)
        {
            int justUsedStep = (CurrentComboStep + ComboStepCount - 1) % ComboStepCount;
            Vector2 toCursor = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            int flameType = ModContent.ProjectileType<WrathCrystallizedFlame>();
            float wrathBonus = _wrathMeter >= 50f ? 1.2f : 1f;

            switch (justUsedStep)
            {
                case 0: // Accusation — 2 arcing flames
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 vel = toCursor.RotatedBy(i * 0.3f) * 10f * wrathBonus + new Vector2(0, -3f);
                        Projectile.NewProjectile(player.GetSource_ItemUse(Item),
                            player.Center, vel, flameType,
                            (int)(Item.damage * 0.4f), 4f, player.whoAmI);
                    }
                    break;

                case 1: // Conviction — 4 spread flames
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = toCursor.ToRotation() + MathHelper.ToRadians(-30 + i * 20);
                        Vector2 vel = angle.ToRotationVector2() * 11f * wrathBonus + new Vector2(0, -4f);
                        Projectile.NewProjectile(player.GetSource_ItemUse(Item),
                            player.Center, vel, flameType,
                            (int)(Item.damage * 0.35f), 3f, player.whoAmI);
                    }
                    SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.4f, Volume = 0.7f }, player.Center);
                    break;

                case 2: // Execution — 6 flames in ring
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi / 6f * i;
                        Vector2 vel = angle.ToRotationVector2() * 8f * wrathBonus;
                        Projectile.NewProjectile(player.GetSource_ItemUse(Item),
                            player.Center, vel, flameType,
                            (int)(Item.damage * 0.5f), 5f, player.whoAmI);
                    }
                    SoundEngine.PlaySound(SoundID.Item45 with { Pitch = -0.5f, Volume = 0.8f }, player.Center);
                    break;

                case 3: // Damnation — 8 carpet flames + eruption
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i;
                        Vector2 vel = angle.ToRotationVector2() * 6f * wrathBonus;
                        Projectile.NewProjectile(player.GetSource_ItemUse(Item),
                            player.Center, vel, flameType,
                            (int)(Item.damage * 0.6f), 6f, player.whoAmI, ai1: 1f);
                    }

                    // Eruption: mark all nearby enemies
                    if (_wrathMeter >= 80f)
                    {
                        float eruptionRadius = 320f;
                        for (int n = 0; n < Main.maxNPCs; n++)
                        {
                            NPC npc = Main.npc[n];
                            if (npc.active && !npc.friendly && !npc.dontTakeDamage
                                && npc.Distance(player.Center) < eruptionRadius)
                            {
                                npc.AddBuff(ModContent.BuffType<WrathMark>(), 300);
                            }
                        }
                        ConsumeWrath(50f);
                    }

                    SoundEngine.PlaySound(SoundID.Item45 with { Pitch = -0.7f, Volume = 1.0f }, player.Center);
                    SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.3f, Volume = 0.9f }, player.Center);
                    break;
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  WORLD / INVENTORY RENDERING
        // ═══════════════════════════════════════════════════════════

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + MathF.Sin(time * 2.2f) * 0.05f + MathF.Sin(time * 3.8f) * 0.03f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            DiesIraePalette.DrawItemBloom(spriteBatch, tex, pos, origin, rotation, scale, pulse);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, DiesIraePalette.InfernalRed.ToVector3() * 0.35f);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
            Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            float time = Main.GameUpdateCount * 0.04f;
            float pulse = 1f + MathF.Sin(time * 2f) * 0.06f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.UIScaleMatrix);

            float cycle = MathF.Sin(time * 0.7f) * 0.5f + 0.5f;
            Color glowColor = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold, cycle) * 0.24f;
            spriteBatch.Draw(tex, position, frame, glowColor with { A = 0 }, 0f, origin,
                scale * pulse * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.UIScaleMatrix);

            spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}