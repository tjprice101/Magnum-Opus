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
    /// Solstice Tome - Summer-themed magic tome (Post-Mechs tier)
    /// Channels the power of the summer solstice to devastate foes.
    /// - Solar Barrage: Fires rapid solar orbs that explode on impact
    /// - Sunbeam Charge: Hold to charge a devastating beam attack
    /// - Solstice Blessing: Every 10 kills grants temporary damage boost
    /// - Radiant Aura: Provides light and minor health regen while held
    /// </summary>
    public class SolsticeTome : ModItem
    {
        private int chargeTime = 0;
        private int killCount = 0;
        private int blessingTimer = 0;
        private bool isCharging = false;

        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);
        private static readonly Color SunRed = new Color(255, 100, 50);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 34;
            Item.damage = 95;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 8;
            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.buyPrice(gold: 25);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<SolarOrbProjectile>();
            Item.shootSpeed = 14f;
            Item.noMelee = true;
            Item.channel = true;
            Item.staff[Type] = true;
        }

        public override void HoldItem(Player player)
        {
            // Radiant Aura: Light and minor health regen
            player.lifeRegen += 2;
            
            // Blessing timer decay
            if (blessingTimer > 0)
            {
                blessingTimer--;
                player.GetDamage(DamageClass.Magic) += 0.15f; // +15% damage during blessing
            }

            // Ethereal solar aura
            if (Main.rand.NextBool(6))
            {
                float angle = Main.GameUpdateCount * 0.03f;
                for (int i = 0; i < 2; i++)
                {
                    float orbAngle = angle + MathHelper.Pi * i;
                    float radius = 35f + (float)Math.Sin(Main.GameUpdateCount * 0.05f + i) * 8f;
                    Vector2 orbPos = player.Center + orbAngle.ToRotationVector2() * radius;
                    Color orbColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat());
                    CustomParticles.GenericFlare(orbPos, orbColor * 0.55f, 0.28f, 14);
                }
            }

            // Rising light motes
            if (Main.rand.NextBool(10))
            {
                Vector2 motePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 moteVel = new Vector2(0, -Main.rand.NextFloat(0.8f, 2f));
                var mote = new GenericGlowParticle(motePos, moteVel, SunGold * 0.4f, 0.22f, 28, true);
                MagnumParticleHandler.SpawnParticle(mote);
            }

            // Floating summer melody notes
            if (Main.rand.NextBool(12))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(38f, 38f);
                Vector2 noteVel = new Vector2(0, -Main.rand.NextFloat(0.3f, 0.7f));
                Color noteColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat()) * 0.6f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.75f, 40);
            }

            // Charging effect
            if (player.channel && player.CheckMana(Item.mana, false))
            {
                if (!isCharging)
                {
                    isCharging = true;
                    chargeTime = 0;
                }
                chargeTime++;
                
                // Charge particles converging
                if (chargeTime > 20 && chargeTime % 5 == 0)
                {
                    float chargeProgress = Math.Min(1f, (chargeTime - 20) / 60f);
                    int particleCount = (int)(4 + chargeProgress * 8);
                    
                    for (int i = 0; i < particleCount; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float dist = 80f * (1f - chargeProgress * 0.5f);
                        Vector2 particlePos = player.Center + angle.ToRotationVector2() * dist;
                        Vector2 particleVel = (player.Center - particlePos).SafeNormalize(Vector2.Zero) * (4f + chargeProgress * 3f);
                        Color particleColor = Color.Lerp(SunGold, SunWhite, chargeProgress);
                        var particle = new GenericGlowParticle(particlePos, particleVel, particleColor * 0.7f, 0.3f + chargeProgress * 0.2f, 15, true);
                        MagnumParticleHandler.SpawnParticle(particle);
                    }
                    
                    // Core glow
                    CustomParticles.GenericFlare(player.Center, SunGold * (0.3f + chargeProgress * 0.5f), 0.4f + chargeProgress * 0.4f, 8);
                }
            }
            else if (isCharging)
            {
                // Release charged attack
                if (chargeTime >= 80) // Fully charged
                {
                    ReleaseChargedBeam(player);
                }
                isCharging = false;
                chargeTime = 0;
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.12f + 0.6f;
            Lighting.AddLight(player.Center, SunGold.ToVector3() * pulse);
        }

        private void ReleaseChargedBeam(Player player)
        {
            if (Main.myPlayer != player.whoAmI) return;
            
            Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            Vector2 spawnPos = player.Center + direction * 30f;
            
            // Spawn sunbeam
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), spawnPos, direction * 20f,
                ModContent.ProjectileType<SunbeamProjectile>(), Item.damage * 3, Item.knockBack * 2f, player.whoAmI);
            
            // Release VFX - layered solar bloom instead of halo
            CustomParticles.GenericFlare(spawnPos, Color.White, 1.2f, 25);
            CustomParticles.GenericFlare(spawnPos, SunGold, 0.95f, 22);
            CustomParticles.GenericFlare(spawnPos, SunOrange * 0.7f, 0.7f, 18);
            
            // Solar corona burst
            for (int ray = 0; ray < 6; ray++)
            {
                float rayAngle = MathHelper.TwoPi * ray / 6f;
                Vector2 rayPos = spawnPos + rayAngle.ToRotationVector2() * 22f;
                CustomParticles.GenericFlare(rayPos, SunGold * 0.85f, 0.32f, 14);
            }
            
            // Radial burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color burstColor = Color.Lerp(SunGold, SunWhite, (float)i / 12f);
                var burst = new GenericGlowParticle(spawnPos, burstVel, burstColor * 0.75f, 0.4f, 22, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // Music note ring and burst for Sunbeam Charge
            ThemedParticles.MusicNoteRing(spawnPos, SunGold, 40f, 6);
            ThemedParticles.MusicNoteBurst(spawnPos, SunOrange, 5, 4f);

            // Sparkle accents
            for (int i = 0; i < 4; i++)
            {
                var sparkle = new SparkleParticle(spawnPos + Main.rand.NextVector2Circular(12f, 12f),
                    Main.rand.NextVector2Circular(2f, 2f), SunWhite * 0.5f, 0.2f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Mana cost
            player.CheckMana(Item.mana * 5, true);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (isCharging && chargeTime > 20) return false; // Don't fire while charging

            // Solar Barrage: Rapid solar orbs
            Vector2 offset = Main.rand.NextVector2Circular(3f, 3f);
            Projectile.NewProjectile(source, position, velocity + offset, type, damage, knockback, player.whoAmI);

            // Fire VFX
            CustomParticles.GenericFlare(position, SunGold, 0.45f, 12);

            // Music note on cast
            ThemedParticles.MusicNote(position, velocity * 0.1f, SunGold * 0.8f, 0.7f, 25);
            
            // Sparkles
            for (int i = 0; i < 3; i++)
            {
                Vector2 sparklePos = position + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 sparkleVel = velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f);
                var sparkle = new GenericGlowParticle(sparklePos, sparkleVel, SunOrange * 0.6f, 0.25f, 15, true);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            return false;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Solstice Blessing: Track kills
            if (target.life <= 0)
            {
                killCount++;
                if (killCount >= 10)
                {
                    killCount = 0;
                    blessingTimer = 300; // 5 seconds of bonus damage
                    
                    // Blessing activation VFX - layered bloom instead of halo
                    CustomParticles.GenericFlare(player.Center, SunWhite, 0.9f, 22);
                    CustomParticles.GenericFlare(player.Center, SunGold, 0.7f, 18);
                    CustomParticles.GenericFlare(player.Center, SunGold * 0.6f, 0.5f, 15);
                    
                    // Blessing sparkle ring
                    for (int ray = 0; ray < 8; ray++)
                    {
                        float rayAngle = MathHelper.TwoPi * ray / 8f;
                        Vector2 rayPos = player.Center + rayAngle.ToRotationVector2() * 25f;
                        CustomParticles.GenericFlare(rayPos, SunGold * 0.8f, 0.28f, 14);
                    }
                    
                    // Burst particles
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f;
                        Vector2 burstVel = angle.ToRotationVector2() * 5f;
                        var burst = new GenericGlowParticle(player.Center, burstVel, SunGold * 0.8f, 0.35f, 25, true);
                        MagnumParticleHandler.SpawnParticle(burst);
                    }
                }
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.12f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, SunGold * 0.35f, rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SunOrange * 0.28f, rotation, origin, scale * pulse * 1.18f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, SunWhite * 0.22f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, SunGold.ToVector3() * 0.5f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "SolarBarrage", "Fires rapid solar orbs that explode on impact") { OverrideColor = SunOrange });
            tooltips.Add(new TooltipLine(Mod, "SunbeamCharge", "Hold to charge a devastating sunbeam attack") { OverrideColor = SunGold });
            tooltips.Add(new TooltipLine(Mod, "SolsticeBlessing", "Every 10 kills grants +15% damage for 5 seconds") { OverrideColor = SunWhite });
            tooltips.Add(new TooltipLine(Mod, "RadiantAura", "Provides +2 life regeneration while held") { OverrideColor = Color.Lerp(SunGold, SunWhite, 0.5f) });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The wisdom of endless summer days bound within pages of light'") { OverrideColor = Color.Lerp(SunGold, SunOrange, 0.5f) });
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
