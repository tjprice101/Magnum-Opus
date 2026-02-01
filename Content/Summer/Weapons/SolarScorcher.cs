using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Summer.Projectiles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Summer.Weapons
{
    /// <summary>
    /// Solar Scorcher - Summer-themed flamethrower (Post-Mechs tier)
    /// Unleashes streams of concentrated solar fire.
    /// - Searing Spray: Continuous solar flame stream (68 damage/tick)
    /// - Heatwave: Every 2 seconds creates expanding heat pulse
    /// - Solar Buildup: Continuous fire increases damage over time (up to +30%)
    /// - Mirage Shield: Standing still creates heat barrier that damages nearby enemies
    /// </summary>
    public class SolarScorcher : ModItem
    {
        private int fireTimer = 0;
        private int heatwaveTimer = 0;
        private float damageBonus = 0f;
        private int idleTimer = 0;

        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);
        private static readonly Color SunRed = new Color(255, 100, 50);

        public override void SetDefaults()
        {
            Item.width = 54;
            Item.height = 24;
            Item.damage = 68;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 1f;
            Item.value = Item.buyPrice(gold: 25);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item34;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<SolarFlameStream>();
            Item.shootSpeed = 16f;
            Item.noMelee = true;
            Item.useAmmo = AmmoID.Gel;
            Item.channel = true;
        }

        public override void HoldItem(Player player)
        {
            // Track if player is standing still for Mirage Shield
            if (Math.Abs(player.velocity.X) < 0.5f && Math.Abs(player.velocity.Y) < 0.5f)
            {
                idleTimer++;
                
                // Mirage Shield: After 60 frames of standing still
                if (idleTimer >= 60)
                {
                    // Visual heat barrier
                    if (Main.rand.NextBool(4))
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float radius = 50f + Main.rand.NextFloat(20f);
                        Vector2 heatPos = player.Center + angle.ToRotationVector2() * radius;
                        Vector2 heatVel = (angle + MathHelper.PiOver2).ToRotationVector2() * 2f;
                        Color heatColor = Color.Lerp(SunOrange, SunRed, Main.rand.NextFloat()) * 0.5f;
                        var heat = new GenericGlowParticle(heatPos, heatVel, heatColor, 0.28f, 20, true);
                        MagnumParticleHandler.SpawnParticle(heat);
                    }

                    // Damage nearby enemies
                    if (idleTimer % 30 == 0)
                    {
                        foreach (NPC npc in Main.npc)
                        {
                            if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                            float dist = Vector2.Distance(player.Center, npc.Center);
                            if (dist < 80f)
                            {
                                int shieldDamage = (int)(Item.damage * 0.3f);
                                npc.SimpleStrikeNPC(shieldDamage, 0, false, 0f, DamageClass.Ranged);
                                npc.AddBuff(BuffID.OnFire3, 60);
                                
                                CustomParticles.GenericFlare(npc.Center, SunOrange, 0.4f, 12);
                            }
                        }
                    }
                }
            }
            else
            {
                idleTimer = 0;
            }

            // Decay damage bonus when not firing
            if (!player.channel)
            {
                fireTimer = 0;
                damageBonus = Math.Max(0f, damageBonus - 0.005f);
            }

            // Ambient heat aura
            if (Main.rand.NextBool(8))
            {
                Vector2 auraPos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 auraVel = new Vector2(0, -Main.rand.NextFloat(0.5f, 1.5f));
                var aura = new GenericGlowParticle(auraPos, auraVel, SunOrange * 0.35f, 0.22f, 25, true);
                MagnumParticleHandler.SpawnParticle(aura);
            }

            // Floating summer melody notes
            if (Main.rand.NextBool(12))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(38f, 38f);
                Vector2 noteVel = new Vector2(0, -Main.rand.NextFloat(0.3f, 0.7f));
                Color noteColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat()) * 0.6f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.75f, 40);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 0.5f;
            Lighting.AddLight(player.Center, SunOrange.ToVector3() * pulse);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Solar Buildup: Increase damage based on continuous fire time
            fireTimer++;
            damageBonus = Math.Min(0.30f, fireTimer / 300f); // Max +30% after 5 seconds of fire
            damage = (int)(damage * (1f + damageBonus));

            // Heatwave timer
            heatwaveTimer++;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spray spread
            float spread = MathHelper.ToRadians(Main.rand.NextFloat(-8f, 8f));
            velocity = velocity.RotatedBy(spread);

            // Muzzle flash
            CustomParticles.GenericFlare(position, SunOrange, 0.45f, 10);

            // Music note on shot
            ThemedParticles.MusicNote(position, velocity * 0.1f, SunGold * 0.8f, 0.7f, 25);

            // Fire stream projectile
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            // Heatwave: Every 2 seconds
            if (heatwaveTimer >= 120)
            {
                heatwaveTimer = 0;
                
                // Spawn heatwave pulse
                Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<HeatwavePulse>(), damage / 2, 0f, player.whoAmI);
                
                // VFX - layered heat bloom instead of halo
                CustomParticles.GenericFlare(player.Center, SunGold, 0.7f, 18);
                CustomParticles.GenericFlare(player.Center, SunOrange, 0.55f, 15);
                CustomParticles.GenericFlare(player.Center, SunOrange * 0.6f, 0.4f, 12);
                
                // Heatwave pulse burst
                for (int ray = 0; ray < 8; ray++)
                {
                    float rayAngle = MathHelper.TwoPi * ray / 8f;
                    Vector2 rayPos = player.Center + rayAngle.ToRotationVector2() * 20f;
                    CustomParticles.GenericFlare(rayPos, SunOrange * 0.75f, 0.25f, 12);
                }

                // Music note ring and burst for Heatwave
                ThemedParticles.MusicNoteRing(player.Center, SunGold, 40f, 6);
                ThemedParticles.MusicNoteBurst(player.Center, SunOrange, 5, 4f);

                // Sparkle accents
                for (int i = 0; i < 4; i++)
                {
                    var sparkle = new SparkleParticle(player.Center + Main.rand.NextVector2Circular(12f, 12f),
                        Main.rand.NextVector2Circular(2f, 2f), SunWhite * 0.5f, 0.2f, 16);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }

            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, SunOrange * 0.35f, rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SunRed * 0.28f, rotation, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SunGold * 0.22f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, SunOrange.ToVector3() * 0.5f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "SearingSpray", "Sprays continuous solar flames") { OverrideColor = SunOrange });
            tooltips.Add(new TooltipLine(Mod, "Heatwave", "Every 2 seconds creates an expanding heat pulse") { OverrideColor = SunGold });
            tooltips.Add(new TooltipLine(Mod, "SolarBuildup", "Continuous fire increases damage up to 30%") { OverrideColor = SunRed });
            tooltips.Add(new TooltipLine(Mod, "MirageShield", "Standing still creates a heat barrier that damages nearby foes") { OverrideColor = SunWhite });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Harness the scorching breath of summer's peak'") { OverrideColor = Color.Lerp(SunOrange, SunRed, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SolsticeBar>(), 16)
                .AddIngredient(ModContent.ItemType<SummerResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofMight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
