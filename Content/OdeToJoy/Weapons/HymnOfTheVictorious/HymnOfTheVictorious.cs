using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Utilities;
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

namespace MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious
{
    public class HymnOfTheVictorious : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 3100;
            Item.DamageType = DamageClass.Magic;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.crit = 15;
            Item.mana = 25;
            Item.shoot = ModContent.ProjectileType<HymnVictoriousSwing>();
            Item.shootSpeed = 16f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var hymnPlayer = player.HymnOfTheVictorious();
            int verse = hymnPlayer.currentVerse;
            bool hymnResonance = hymnPlayer.completedCycles >= 3;
            int bonusOrbs = hymnResonance ? 1 : 0;

            switch (verse)
            {
                case 0: // Exordium: 1 orb (+ bonus)
                    for (int i = 0; i < 1 + bonusOrbs; i++)
                    {
                        Vector2 vel = velocity.RotatedByRandom(MathHelper.ToRadians(3f * i));
                        Projectile.NewProjectile(source, position, vel, type, damage, knockback, player.whoAmI, ai0: 0f, ai1: 0f);
                    }
                    break;

                case 1: // Rising: 2 orbs at +/-5 degrees (+ bonus)
                    for (int i = 0; i < 2 + bonusOrbs; i++)
                    {
                        float angleOffset = (i < 2) ? ((i == 0 ? -1f : 1f) * MathHelper.ToRadians(5f)) : MathHelper.ToRadians(2f * (i - 1));
                        Projectile.NewProjectile(source, position, velocity.RotatedBy(angleOffset), type, damage, knockback, player.whoAmI, ai0: 1f, ai1: 0f);
                    }
                    break;

                case 2: // Apex: 3 orbs at +/-8 degrees and center (+ bonus)
                    for (int i = 0; i < 3 + bonusOrbs; i++)
                    {
                        float angleOffset = i switch
                        {
                            0 => MathHelper.ToRadians(-8f),
                            1 => 0f,
                            2 => MathHelper.ToRadians(8f),
                            _ => MathHelper.ToRadians(4f * (i - 2)),
                        };
                        Projectile.NewProjectile(source, position, velocity.RotatedBy(angleOffset), type, damage, knockback, player.whoAmI, ai0: 2f, ai1: 0f);
                    }
                    break;

                case 3: // Gloria: 1 special orb with ai[1]=1 flag (+ bonus normal)
                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai0: 3f, ai1: 1f);
                    for (int i = 0; i < bonusOrbs; i++)
                    {
                        Vector2 vel = velocity.RotatedByRandom(MathHelper.ToRadians(5f));
                        Projectile.NewProjectile(source, position, vel, type, damage, knockback, player.whoAmI, ai0: 3f, ai1: 0f);
                    }
                    break;
            }

            hymnPlayer.AddVerse();
            hymnPlayer.isActive = true;
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
            spriteBatch.Draw(tex, drawPos, null, OdeToJoyPalette.WarmAmber with { A = 0 } * pulse, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, OdeToJoyPalette.SunlightYellow with { A = 0 } * (pulse * 0.5f), rotation, origin, scale * 1.02f, SpriteEffects.None, 0f);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Cycles through 4 unique verse types: Exordium, Rising, Apex, and Gloria"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Completing the full cycle fires the devastating Complete Hymn"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Enemies hit by 3+ verse types within 5s gain Hymn Resonance (+25% magic damage taken)"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Encore: killing with Complete Hymn locks Gloria for repeated devastating cycles"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each verse is a victory. The final verse is annihilation.'")
            {
                OverrideColor = OdeToJoyPalette.LoreText
            });
        }
    }
}
