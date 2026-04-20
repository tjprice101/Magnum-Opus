using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory
{
    public class AnthemOfGlory : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.damage = 2800;
            Item.DamageType = DamageClass.Magic;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item21;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.mana = 15;
            Item.crit = 12;
            Item.shoot = ModContent.ProjectileType<AnthemGloryProjectile>();
            Item.shootSpeed = 12f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var anthemPlayer = player.AnthemOfGlory();
            anthemPlayer.isActive = true;
            anthemPlayer.activeTimer = 120;

            float channelProgress = anthemPlayer.GetChannelProgress();

            // Damage scaling: 1x to 2x based on channel progress
            int scaledDamage = (int)(damage * (1f + channelProgress));

            // Pass channel progress via ai[0]
            Projectile.NewProjectile(source, position, velocity, type, scaledDamage, knockback, player.whoAmI, ai0: channelProgress);

            // Every 120 frames of channeling: fire an extra Glory Note
            if (anthemPlayer.channelFrames > 0 && anthemPlayer.channelFrames % 120 == 0)
            {
                try
                {
                    Vector2 noteVel = velocity.RotatedByRandom(MathHelper.ToRadians(15f)) * 0.8f;
                    GenericHomingOrbChild.SpawnChild(
                        source, position, noteVel,
                        damage / 2, knockback, player.whoAmI,
                        homingStrength: 0.08f, behaviorFlags: 0,
                        themeIndex: GenericHomingOrbChild.THEME_ODETOJOY,
                        scaleMult: 0.8f, timeLeft: 120);
                }
                catch { }
            }

            // Victory Fanfare: 3+ kills during active channel triggers reset to 1.5x base
            if (anthemPlayer.channelKills >= 3 && !anthemPlayer.victoryFanfareActive)
            {
                anthemPlayer.victoryFanfareTimer = 180; // 3 seconds of fanfare buff
                anthemPlayer.channelKills = 0;
                anthemPlayer.channelFrames = (int)(300f * 0.5f); // Reset channel to 50% (1.5x base)

                // Fanfare sound
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, player.Center);
            }

            return false;
        }

        public override void HoldItem(Player player)
        {
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Color col = OdeToJoyPalette.GetGardenGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.GreenTorch,
                    new Vector2(0, -0.8f) + Main.rand.NextVector2Circular(0.4f, 0.4f), 0, col, 0.5f);
                d.noGravity = true;
            }

            float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(player.Center, new Vector3(0.4f, 0.35f, 0.15f) * pulse);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.2f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Item[Type].Value;
            Vector2 drawPos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height);
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
            spriteBatch.Draw(tex, drawPos, null, OdeToJoyPalette.GoldenPollen with { A = 0 } * pulse, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, OdeToJoyPalette.LeafGreen with { A = 0 } * (pulse * 0.6f), rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Hold to channel a continuous golden beam that grows in power"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Crescendo: beam damage and width scale from 1x to 2x over 5 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Spawns homing Glory Notes every 2 seconds while channeling"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Victory Fanfare: 3 kills during a channel triggers a golden shockwave"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Let every note ring with triumph — for this is the anthem that crowns the victorious'")
            {
                OverrideColor = OdeToJoyPalette.LoreText
            });
        }
    }
}
