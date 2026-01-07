using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using MagnumOpus.Content.Eroica.Minions;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Eroica.ResonantWeapons
{
    /// <summary>
    /// Finality of the Sakura - A powerful summoner weapon with rainbow rarity.
    /// Summons the Sakura of Fate, a spectral guardian that fires black and red flames.
    /// The staff itself is lit aflame in black and deep scarlet particles.
    /// </summary>
    public class FinalityOfTheSakura : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.damage = 280;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.buyPrice(platinum: 1);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = null; // Custom sound
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<SakuraOfFate>();
            Item.buffType = ModContent.BuffType<SakuraOfFateBuff>();
            Item.maxStack = 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Apply the buff
            player.AddBuff(Item.buffType, 18000);
            
            // Spawn position at mouse
            position = Main.MouseWorld;
            
            // Epic summoning effects - dark vortex with black and red flames (NO PURPLE)
            // Outer ring - black smoke spiral
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                Vector2 dustPos = position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 70f;
                Vector2 dustVel = (position - dustPos).SafeNormalize(Vector2.Zero) * 5f;
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Smoke, dustVel, 200, Color.Black, 2.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.5f;
            }
            
            // Inner ring - deep crimson/scarlet
            for (int i = 0; i < 35; i++)
            {
                float angle = MathHelper.TwoPi * i / 35f;
                Vector2 dustPos = position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 50f;
                Vector2 dustVel = (position - dustPos).SafeNormalize(Vector2.Zero) * 4f;
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.CrimsonTorch, dustVel, 100, default, 2f);
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }
            
            // Central black smoke burst
            for (int i = 0; i < 30; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(8f, 8f);
                Dust dust = Dust.NewDustDirect(position, 1, 1, DustID.Smoke, dustVel.X, dustVel.Y, 200, Color.Black, 2.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }
            
            // Deep red fire accents
            for (int i = 0; i < 25; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustDirect(position, 1, 1, DustID.CrimsonTorch, dustVel.X, dustVel.Y, 100, default, 1.8f);
                dust.noGravity = true;
            }
            
            // Additional red flames
            for (int i = 0; i < 20; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f);
                Dust dust = Dust.NewDustDirect(position, 1, 1, DustID.Torch, dustVel.X, dustVel.Y, 100, new Color(200, 50, 50), 2f);
                dust.noGravity = true;
            }
            
            // Black smoke wisps
            for (int i = 0; i < 15; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustDirect(position, 1, 1, DustID.Smoke, dustVel.X, dustVel.Y, 200, Color.Black, 1.5f);
                dust.noGravity = true;
            }
            
            // Dark, ominous summoning sounds
            SoundEngine.PlaySound(SoundID.Item119 with { Volume = 0.9f, Pitch = -0.4f }, position);
            SoundEngine.PlaySound(SoundID.Item73 with { Volume = 0.7f, Pitch = -0.3f }, position);
            SoundEngine.PlaySound(SoundID.NPCDeath6 with { Volume = 0.4f, Pitch = 0.5f }, position);
            
            // Spawn the Sakura of Fate
            Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            // Staff is constantly lit aflame in black and deep scarlet particles - NO PURPLE
            if (Main.rand.NextBool(2))
            {
                // Position around the held item
                Vector2 itemCenter = player.itemLocation + new Vector2(Item.width * 0.5f * player.direction, -Item.height * 0.3f);
                
                // Black smoke (no purple)
                if (Main.rand.NextBool(2))
                {
                    Dust shadow = Dust.NewDustDirect(itemCenter + Main.rand.NextVector2Circular(8f, 8f), 1, 1, 
                        DustID.Smoke, 0f, -2f, 150, Color.Black, 1.3f);
                    shadow.noGravity = true;
                    shadow.velocity *= 0.5f;
                }
                
                // Deep crimson/scarlet
                if (Main.rand.NextBool(2))
                {
                    Dust crimson = Dust.NewDustDirect(itemCenter + Main.rand.NextVector2Circular(8f, 8f), 1, 1, 
                        DustID.CrimsonTorch, 0f, -2f, 100, default, 1.3f);
                    crimson.noGravity = true;
                    crimson.velocity *= 0.5f;
                }
            }
            
            // Occasional ember
            if (Main.rand.NextBool(6))
            {
                Vector2 itemCenter = player.itemLocation + new Vector2(Item.width * 0.5f * player.direction, -Item.height * 0.3f);
                Dust ember = Dust.NewDustDirect(itemCenter, 1, 1, DustID.Torch, 
                    Main.rand.NextFloat(-0.5f, 0.5f), -3f, 100, new Color(180, 30, 30), 1f);
                ember.noGravity = true;
            }
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MinionInfo", "Summons a Sakura of Fate to fight for you")
            {
                OverrideColor = new Color(180, 60, 80)
            });
            
            tooltips.Add(new TooltipLine(Mod, "FlameInfo", "The Sakura fires black and scarlet flames at enemies")
            {
                OverrideColor = new Color(120, 40, 60)
            });
            
            tooltips.Add(new TooltipLine(Mod, "FateInfo", "'A final blossom before eternal night'")
            {
                OverrideColor = new Color(100, 100, 100)
            });
        }
    }
}
