using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner
{
    /// <summary>
    /// Nocturnal Executioner — Heavy cosmic greatsword with Execution mechanic.
    /// 3-phase cosmic combo: Shadow Cleave → Cosmic Divide → Stellar Execution.
    /// Execution Charge (0-100) builds through combat; at 50+ fires 5-blade fan,
    /// at 100 fires homing blades with massive cosmic explosion.
    /// Cosmic Presence aura damages nearby enemies at high charge.
    /// "At midnight, the executioner does not knock. The stars simply go dark."
    /// </summary>
    public class NocturnalExecutioner : MeleeSwingItemBase
    {
        protected override int SwingProjectileType => ModContent.ProjectileType<NocturnalExecutionerSwing>();
        protected override int ComboStepCount => 3;

        private int executionCharge;
        private int chargeDecayTimer;
        private const int ChargeDecayInterval = 30; // 0.5s between decay ticks

        public int ExecutionCharge
        {
            get => executionCharge;
            set => executionCharge = Math.Clamp(value, 0, 100);
        }

        protected override void SetWeaponDefaults()
        {
            Item.damage = 1850;
            Item.knockBack = 7.5f;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.scale = 1.5f;
            Item.crit = 18;
            Item.value = Terraria.Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item71 with { Pitch = -0.3f };
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                if (executionCharge < 50) return false;
                Item.useTime = 30;
                Item.useAnimation = 30;
            }
            else
            {
                Item.useTime = 16;
                Item.useAnimation = 16;
            }
            return base.CanUseItem(player);
        }

        protected override void OnShoot(Player player, int projectileIndex)
        {
            if (player.altFunctionUse == 2 && executionCharge >= 50)
            {
                bool isMaxCharge = executionCharge >= 100;
                float damageMult = isMaxCharge ? 3.5f : 2.5f;

                Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                var source = player.GetSource_ItemUse(Item);
                int dmg = (int)(Item.damage * damageMult * player.GetDamage(DamageClass.Melee).Multiplicative);

                // Fire 5 blades in fan pattern
                for (int i = -2; i <= 2; i++)
                {
                    float angle = dir.ToRotation() + MathHelper.ToRadians(i * 12f);
                    Vector2 vel = angle.ToRotationVector2() * 18f;
                    int projIndex = Projectile.NewProjectile(source, player.Center, vel,
                        ModContent.ProjectileType<ExecutionFanBlade>(),
                        dmg, Item.knockBack, player.whoAmI, ai0: isMaxCharge ? 1f : 0f);
                }

                NocturnalExecutionerVFX.ExecutionFanVFX(player.Center, isMaxCharge);

                if (isMaxCharge)
                {
                    // Brief screen darkening effect
                    MagnumScreenEffects.AddScreenShake(12f);
                }
                else
                {
                    MagnumScreenEffects.AddScreenShake(8f);
                }

                executionCharge = 0;
            }
        }

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            // Charge decay: -2 per second
            if (!player.ItemAnimationActive)
            {
                chargeDecayTimer++;
                if (chargeDecayTimer >= ChargeDecayInterval && executionCharge > 0)
                {
                    executionCharge--;
                    chargeDecayTimer = 0;
                }
            }
            else
            {
                chargeDecayTimer = 0;
            }

            // Cosmic Presence: High charge aura VFX + passive damage
            float chargeProgress = executionCharge / 100f;
            NocturnalExecutionerVFX.HoldItemVFX(player, chargeProgress);

            // Cosmic Presence — orbiting constellation particles at high charge
            if (executionCharge >= 50 && Main.rand.NextBool(3))
            {
                float angle = Main.GameUpdateCount * 0.06f;
                float radius = 30f + (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 8f;
                for (int i = 0; i < 3; i++)
                {
                    float a = angle + MathHelper.TwoPi * i / 3f;
                    Vector2 orbitPos = player.Center + a.ToRotationVector2() * radius;
                    Color orbitColor = Color.Lerp(NachtmusikPalette.DeepBlue, NachtmusikPalette.StarWhite, chargeProgress);
                    Dust d = Dust.NewDustPerfect(orbitPos, DustID.PurpleTorch, Vector2.Zero, 0, orbitColor, 0.8f + chargeProgress * 0.4f);
                    d.noGravity = true;
                }
            }

            // Cosmic Presence passive aura damage at high charge
            if (executionCharge >= 50 && Main.GameUpdateCount % 15 == 0)
            {
                float auraRadius = 128f; // 8 tiles
                int auraDamage = (int)(Item.damage * 0.05f * chargeProgress);
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.Distance(player.Center) < auraRadius)
                    {
                        if (Main.myPlayer == player.whoAmI)
                            player.ApplyDamageToNPC(npc, auraDamage, 0f, 0, false);
                    }
                }
            }

            Lighting.AddLight(player.Center, NachtmusikPalette.StarlitBlue.ToVector3() * (0.3f + chargeProgress * 0.5f));
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            float chargeProgress = executionCharge / 100f;

            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 1.8f) * 0.06f * (1f + chargeProgress);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Night void outer aura — scales with charge
            Color outerColor = Color.Lerp(NachtmusikPalette.DeepBlue, NachtmusikPalette.StarlitBlue, chargeProgress) with { A = 0 };
            spriteBatch.Draw(tex, pos, null, outerColor * (0.25f + chargeProgress * 0.2f),
                rotation, origin, scale * pulse * (1.25f + chargeProgress * 0.15f), SpriteEffects.None, 0f);

            // Cosmic blue mid glow
            Color midColor = NachtmusikPalette.StarlitBlue with { A = 0 };
            spriteBatch.Draw(tex, pos, null, midColor * (0.2f + chargeProgress * 0.15f),
                rotation, origin, scale * pulse * 1.12f, SpriteEffects.None, 0f);

            // Starlight silver core at high charge
            if (chargeProgress > 0.3f)
            {
                Color coreColor = NachtmusikPalette.StarWhite with { A = 0 };
                spriteBatch.Draw(tex, pos, null, coreColor * (chargeProgress * 0.2f),
                    rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return true;
        }

        protected override Color GetLoreColor() => new Color(100, 120, 200); // Starlight Indigo

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Devastating 3-phase cosmic combo with escalating power"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", $"Execution Charge: {executionCharge}/100 — builds on swing (+5), hit (+10), kill (+15)"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "50+ charge: Right-click fires 5 spectral blades in fan (2.5x damage)"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "100 charge: Blades home briefly, massive cosmic explosion (3.5x damage)"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "High charge pulses cosmic presence — enemies within 8 tiles take passive damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'At midnight, the executioner does not knock. The stars simply go dark.'")
            { OverrideColor = new Color(100, 120, 200) });
        }
    }
}
