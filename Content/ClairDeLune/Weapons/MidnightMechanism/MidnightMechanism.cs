using System;
using System.Collections.Generic;
using MagnumOpus.Content.ClairDeLune;
using MagnumOpus.Content.ClairDeLune.Weapons.MidnightMechanism.Projectiles;
using MagnumOpus.Content.ClairDeLune.Weapons.MidnightMechanism.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.MidnightMechanism
{
    public class MidnightMechanism : ModItem
    {
        /// <summary>Tracks shots fired at Phase 5 for Midnight Strike trigger.</summary>
        private int _phase5ShotCounter;

        public override void SetDefaults()
        {
            Item.width = 72;
            Item.height = 36;
            Item.damage = 2900;
            Item.DamageType = DamageClass.Ranged;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<MechanismBulletProjectile>();
            Item.shootSpeed = 22f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 16;
        }

        public override float UseSpeedMultiplier(Player player)
        {
            var mp = player.GetModPlayer<MidnightMechanismPlayer>();
            int heat = mp.mechanismHeat;

            // 5-phase fire rate scaling
            if (heat >= 80) return 3.0f;  // Phase 5: very fast
            if (heat >= 60) return 2.4f;  // Phase 4
            if (heat >= 40) return 1.8f;  // Phase 3
            if (heat >= 20) return 1.4f;  // Phase 2
            return 1.0f;                   // Phase 1: base speed
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var mp = player.GetModPlayer<MidnightMechanismPlayer>();
            mp.isActive = true;
            mp.AddHeat(1);

            int heat = mp.mechanismHeat;

            // Determine phase (0-4) and spread
            int phase;
            float spreadDegrees;
            if (heat >= 80) { phase = 4; spreadDegrees = 10f; }
            else if (heat >= 60) { phase = 3; spreadDegrees = 7f; }
            else if (heat >= 40) { phase = 2; spreadDegrees = 5f; }
            else if (heat >= 20) { phase = 1; spreadDegrees = 3f; }
            else { phase = 0; spreadDegrees = 0f; }

            // Apply spread
            if (spreadDegrees > 0f)
            {
                float spreadRad = MathHelper.ToRadians(spreadDegrees);
                float offset = Main.rand.NextFloat(-spreadRad, spreadRad);
                velocity = velocity.RotatedBy(offset);
            }

            // Midnight Strike: every 12th shot at Phase 5
            float midnightStrikeFlag = 0f;
            int finalDamage = damage;
            if (phase == 4)
            {
                _phase5ShotCounter++;
                if (_phase5ShotCounter >= 12)
                {
                    _phase5ShotCounter = 0;
                    midnightStrikeFlag = 1f;
                    finalDamage = damage * 10; // 10x damage
                }
            }
            else
            {
                _phase5ShotCounter = 0; // Reset if we drop out of Phase 5
            }

            Projectile.NewProjectile(source, player.MountedCenter, velocity, type,
                finalDamage, knockback, player.whoAmI,
                ai0: phase, ai1: midnightStrikeFlag);

            return false;
        }

        public override void HoldItem(Player player)
        {
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Color col = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.WhiteTorch,
                    new Vector2(0, -0.8f) + Main.rand.NextVector2Circular(0.4f, 0.4f), 0, col, 0.5f);
                d.noGravity = true;
            }

            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(player.Center, ClairDeLunePalette.SoftBlue.ToVector3() * 0.35f * pulse);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);

            spriteBatch.Draw(tex, drawPos, null, ClairDeLunePalette.SoftBlue with { A = 0 } * pulse,
                rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, ClairDeLunePalette.PearlWhite with { A = 0 } * (pulse * 0.7f),
                rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "5-phase gatling spin-up accelerates from 3 to 24 shots per second"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Higher phases add bullet spread and enemy-tracking homing"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At Phase 5, every 12th shot triggers a Midnight Strike at 10x damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Stopping fire decays heat — sustain fire to maintain overdrive"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The clock does not care if you are ready. Midnight comes regardless.'")
            {
                OverrideColor = ClairDeLunePalette.LoreText
            });
        }
    }
}
