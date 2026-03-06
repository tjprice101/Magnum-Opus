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
using MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance
{
    /// <summary>
    /// Twilight Severance — Ultra-fast Nachtmusik katana with Dimension Sever ultimate.
    /// 3-phase lightning combo: diagonal slash → reverse slash → horizontal cut.
    /// TwilightCharge (0-100) builds through swings (+5), decays at -3/s.
    /// Every 3rd slash fires perpendicular blade waves.
    /// Alt fire at 100 charge: Dimension Sever — 5 TwilightSlashProjectile in fan at 3x damage.
    /// "Between light and dark, the blade finds every truth."
    /// </summary>
    public class TwilightSeverance : MeleeSwingItemBase
    {
        protected override int SwingProjectileType => ModContent.ProjectileType<TwilightSeveranceSwing>();
        protected override int ComboStepCount => 3;

        // Nachtmusik palette
        private static readonly Color NightVoid = new Color(10, 10, 30);
        private static readonly Color DeepIndigo = new Color(40, 30, 100);
        private static readonly Color CosmicBlue = new Color(60, 80, 180);
        private static readonly Color StarlightSilver = new Color(180, 200, 230);
        private static readonly Color MoonPearl = new Color(220, 225, 245);
        private static readonly Color StellarWhite = new Color(240, 245, 255);

        private int twilightCharge;
        private int chargeDecayTimer;
        private int swingCounter; // Tracks total swings for perpendicular blade wave
        private const int ChargeDecayInterval = 20; // ~3/s at 60fps

        public int TwilightCharge
        {
            get => twilightCharge;
            set => twilightCharge = Math.Clamp(value, 0, 100);
        }

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        protected override void SetWeaponDefaults()
        {
            Item.damage = 1450;
            Item.knockBack = 4f;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.scale = 1.3f;
            Item.crit = 25;
            Item.value = Terraria.Item.sellPrice(gold: 40);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item71 with { Pitch = 0.2f, Volume = 0.8f };
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                if (twilightCharge < 100) return false;
                Item.useTime = 14;
                Item.useAnimation = 14;
            }
            else
            {
                Item.useTime = 8;
                Item.useAnimation = 8;
            }
            return base.CanUseItem(player);
        }

        protected override void OnShoot(Player player, int projectileIndex)
        {
            // Build charge on every swing
            TwilightCharge += 5;
            swingCounter++;

            // Alt fire: Dimension Sever at full charge
            if (player.altFunctionUse == 2 && twilightCharge >= 100)
            {
                Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                var source = player.GetSource_ItemUse(Item);
                int dmg = (int)(Item.damage * 3f * player.GetDamage(DamageClass.Melee).Multiplicative);

                // Fire 5 blades in fan pattern
                for (int i = -2; i <= 2; i++)
                {
                    float angle = dir.ToRotation() + MathHelper.ToRadians(i * 10f);
                    Vector2 vel = angle.ToRotationVector2() * 20f;
                    Projectile.NewProjectile(source, player.Center, vel,
                        ModContent.ProjectileType<TwilightSlashProjectile>(),
                        dmg, Item.knockBack * 1.5f, player.whoAmI, ai0: 1f); // ai0=1 → Dimension Sever variant
                }

                TwilightSeveranceVFX.DimensionSeverVFX(player.Center);
                MagnumScreenEffects.AddScreenShake(10f);
                SoundEngine.PlaySound(SoundID.Item162 with { Pitch = -0.4f, Volume = 1.1f }, player.Center);

                twilightCharge = 0;
                swingCounter = 0;
                return;
            }

            // Every 3rd slash fires perpendicular blade waves
            if (swingCounter % 3 == 0 && twilightCharge >= 30)
            {
                Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                var source = player.GetSource_ItemUse(Item);
                int dmg = (int)(Item.damage * 0.4f * player.GetDamage(DamageClass.Melee).Multiplicative);

                for (int side = -1; side <= 1; side += 2)
                {
                    Vector2 perpVel = dir.RotatedBy(MathHelper.PiOver2 * side) * 14f;
                    Projectile.NewProjectile(source, player.Center, perpVel,
                        ModContent.ProjectileType<TwilightSlashProjectile>(),
                        dmg, Item.knockBack * 0.5f, player.whoAmI, ai0: 0f); // ai0=0 → normal variant
                }

                TwilightSeveranceVFX.PerpendicularSlashVFX(player.Center);
            }
        }

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            // Charge decay: -3 per second (~every 20 frames)
            if (!player.ItemAnimationActive)
            {
                chargeDecayTimer++;
                if (chargeDecayTimer >= ChargeDecayInterval && twilightCharge > 0)
                {
                    twilightCharge--;
                    chargeDecayTimer = 0;
                }
            }
            else
            {
                chargeDecayTimer = 0;
            }

            float chargeProgress = twilightCharge / 100f;

            // 50+ charge: +15% movement speed
            if (twilightCharge >= 50)
            {
                player.moveSpeed += 0.15f;
            }

            // Charge aura VFX
            TwilightSeveranceVFX.HoldItemVFX(player, chargeProgress);

            Lighting.AddLight(player.Center, NachtmusikPalette.StarlitBlue.ToVector3() * (0.2f + chargeProgress * 0.4f));
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Vector2 pos = Item.Center - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            float chargeProgress = twilightCharge / 100f;

            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 2.2f) * 0.04f * (1f + chargeProgress);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Indigo outer shimmer — scales with charge
            Color outerColor = Color.Lerp(DeepIndigo, CosmicBlue, chargeProgress) with { A = 0 };
            spriteBatch.Draw(tex, pos, null, outerColor * (0.2f + chargeProgress * 0.15f),
                rotation, origin, scale * pulse * (1.18f + chargeProgress * 0.1f), SpriteEffects.None, 0f);

            // Silver mid glow — sharp katana shimmer
            Color midColor = StarlightSilver with { A = 0 };
            spriteBatch.Draw(tex, pos, null, midColor * (0.15f + chargeProgress * 0.12f),
                rotation, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);

            // Stellar core at high charge
            if (chargeProgress > 0.4f)
            {
                Color coreColor = StellarWhite with { A = 0 };
                spriteBatch.Draw(tex, pos, null, coreColor * (chargeProgress * 0.15f),
                    rotation, origin, scale * pulse * 1.03f, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return true;
        }

        protected override Color GetLoreColor() => new Color(100, 120, 200);

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Ultra-fast 3-phase katana combo with dimensional blade waves"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", $"Twilight Charge: {twilightCharge}/100 — builds on swing (+5), decays over time"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 3rd slash fires perpendicular blade waves (0.4x damage)"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "50+ charge grants 15% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "100 charge: Right-click unleashes Dimension Sever — 5 blade fan at 3x damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Between light and dark, the blade finds every truth.'")
            { OverrideColor = new Color(100, 120, 200) });
        }
    }
}
