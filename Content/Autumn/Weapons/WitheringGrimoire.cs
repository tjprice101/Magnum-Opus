using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Autumn.Projectiles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Autumn.Weapons
{
    /// <summary>
    /// Withering Grimoire - Autumn-themed magic weapon (Post-Plantera tier)
    /// An ancient tome containing the essence of autumn's decay.
    /// - Decay Bolt: Fires piercing bolts that wither enemies (125 damage)
    /// - Entropic Field: Creates damaging decay zones on impact
    /// - Autumn's Wrath: Charge attack unleashes a wave of withering energy
    /// - Life Drain: 5% of damage dealt heals the player
    /// </summary>
    public class WitheringGrimoire : ModItem
    {
        private static readonly Color DecayPurple = new Color(100, 50, 120);
        private static readonly Color WitherBrown = new Color(90, 60, 40);
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color DeathGreen = new Color(80, 120, 60);

        private int chargeTimer = 0;
        private const int MaxCharge = 90;
        private bool isCharging = false;

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 36;
            Item.damage = 125;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 14;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.buyPrice(gold: 38);
            Item.rare = ItemRarityID.Lime;
            Item.UseSound = SoundID.Item103;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<DecayBoltProjectile>();
            Item.shootSpeed = 14f;
            Item.channel = true;
        }

        public override void HoldItem(Player player)
        {
            // === CALAMITY-STANDARD HEAVY DUST TRAILS ===
            // Heavy decay dust trail #1
            float trailProgress1 = Main.rand.NextFloat();
            Color purpleGradient = Color.Lerp(DecayPurple, DeathGreen, trailProgress1);
            Dust heavyDecay = Dust.NewDustDirect(player.position, player.width, player.height, 
                DustID.PurpleTorch, player.velocity.X * 0.3f, player.velocity.Y * 0.3f, 100, purpleGradient, 1.5f);
            heavyDecay.noGravity = true;
            heavyDecay.fadeIn = 1.4f;
            heavyDecay.velocity = heavyDecay.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(1.2f, 1.8f);
            
            // Heavy green dust trail #2
            float trailProgress2 = Main.rand.NextFloat();
            Color greenGradient = Color.Lerp(DeathGreen, AutumnOrange, trailProgress2);
            Dust heavyGreen = Dust.NewDustDirect(player.position, player.width, player.height, 
                DustID.CursedTorch, player.velocity.X * 0.25f, player.velocity.Y * 0.25f, 80, greenGradient, 1.4f);
            heavyGreen.noGravity = true;
            heavyGreen.fadeIn = 1.3f;
            heavyGreen.velocity = heavyGreen.velocity.RotatedByRandom(0.25f) * Main.rand.NextFloat(1.1f, 1.6f);
            
            // === CONTRASTING SPARKLES (every 1-in-2 frames) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(28f, 28f);
                Color sparkleColor = Main.rand.NextBool() ? DecayPurple : DeathGreen;
                CustomParticles.PrismaticSparkle(sparklePos, sparkleColor, 0.26f);
            }
            
            // === ORBITING DECAY GLYPHS ===
            if (Main.rand.NextBool(8))
            {
                float orbitAngle = Main.GameUpdateCount * 0.035f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / 4f;
                    Vector2 orbitPos = player.Center + angle.ToRotationVector2() * (32f + Main.rand.NextFloat(6f));
                    Color orbitColor = Color.Lerp(DecayPurple, DeathGreen, (float)i / 4f);
                    CustomParticles.GenericFlare(orbitPos, orbitColor * 0.55f, 0.2f, 14);
                }
            }
            
            // === MUSIC NOTES - the withering melody ===
            if (Main.rand.NextBool(15))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(22f, 22f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), -1f);
                Color noteColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat());
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.85f, 26);
            }

            // Charging logic
            if (player.channel && player.CheckMana(Item, -1, true, false))
            {
                isCharging = true;
                chargeTimer = Math.Min(chargeTimer + 1, MaxCharge);

                // === SPECTACULAR CHARGE VFX ===
                float chargeProgress = (float)chargeTimer / MaxCharge;
                
                if (chargeTimer % 4 == 0)
                {
                    // Converging particles - more intense spiral
                    float radius = 100f * (1f - chargeProgress * 0.7f);
                    int particleCount = 6 + (int)(chargeProgress * 10);
                    for (int i = 0; i < particleCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / particleCount + Main.GameUpdateCount * 0.06f;
                        Vector2 pos = player.Center + angle.ToRotationVector2() * radius;
                        Vector2 vel = (player.Center - pos).SafeNormalize(Vector2.Zero) * (2.5f + chargeProgress * 2f);
                        Color color = Color.Lerp(DecayPurple, DeathGreen, (float)i / particleCount);
                        var particle = new GenericGlowParticle(pos, vel, color * 0.75f, 0.3f + chargeProgress * 0.25f, 20, true);
                        MagnumParticleHandler.SpawnParticle(particle);
                    }
                    
                    // Heavy dust spiral
                    for (int d = 0; d < 2; d++)
                    {
                        float dustAngle = Main.GameUpdateCount * 0.08f + d * MathHelper.Pi;
                        Vector2 dustPos = player.Center + dustAngle.ToRotationVector2() * radius * 0.8f;
                        Vector2 dustVel = (player.Center - dustPos).SafeNormalize(Vector2.Zero) * 3f;
                        Dust spiral = Dust.NewDustPerfect(dustPos, DustID.CursedTorch, dustVel, 100, DeathGreen, 1.4f);
                        spiral.noGravity = true;
                        spiral.fadeIn = 1.2f;
                    }
                }

                // Growing center glow with layers
                CustomParticles.GenericFlare(player.Center, Color.White * chargeProgress * 0.4f, 0.2f + chargeProgress * 0.35f, 6);
                CustomParticles.GenericFlare(player.Center, DecayPurple * chargeProgress * 0.6f, 0.3f + chargeProgress * 0.4f, 8);
                CustomParticles.GenericFlare(player.Center, DeathGreen * chargeProgress * 0.5f, 0.25f + chargeProgress * 0.3f, 10);
            }
            else if (isCharging)
            {
                // Release charge attack
                if (chargeTimer >= MaxCharge * 0.6f)
                {
                    ReleaseChargeAttack(player);
                }
                isCharging = false;
                chargeTimer = 0;
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.15f + 0.6f;
            Lighting.AddLight(player.Center, DecayPurple.ToVector3() * pulse);
        }

        private void ReleaseChargeAttack(Player player)
        {
            if (Main.myPlayer != player.whoAmI) return;

            float chargeProgress = (float)chargeTimer / MaxCharge;
            Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitY);

            // Spawn withering wave
            int damage = (int)(Item.damage * (1f + chargeProgress * 0.8f)); // Up to +80% damage
            Projectile.NewProjectile(
                player.GetSource_ItemUse(Item),
                player.Center,
                direction * 12f,
                ModContent.ProjectileType<WitheringWave>(),
                damage,
                Item.knockBack,
                player.whoAmI
            );

            // === SPECTACULAR AUTUMN WRATH VFX ===
            // Multi-layer flare cascade
            CustomParticles.GenericFlare(player.Center, Color.White, 0.75f, 18);
            CustomParticles.GenericFlare(player.Center, DeathGreen, 0.6f, 22);
            CustomParticles.GenericFlare(player.Center, DecayPurple, 0.5f, 25);
            
            // Gradient halo rings - Purple → Green → Orange
            for (int ring = 0; ring < 6; ring++)
            {
                float progress = (float)ring / 6f;
                Color ringColor;
                if (progress < 0.5f)
                    ringColor = Color.Lerp(DecayPurple, DeathGreen, progress * 2f);
                else
                    ringColor = Color.Lerp(DeathGreen, AutumnOrange, (progress - 0.5f) * 2f);
                CustomParticles.HaloRing(player.Center, ringColor * 0.8f, 0.35f + ring * 0.12f, 14 + ring * 4);
            }
            
            // Heavy radial decay burst with dust
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                float progress = (float)i / 12f;
                Color burstColor = Color.Lerp(DecayPurple, DeathGreen, progress);
                
                var burst = new GenericGlowParticle(player.Center, burstVel, burstColor * 0.65f, 0.32f, 22, true);
                MagnumParticleHandler.SpawnParticle(burst);
                
                // Heavy dust
                Dust decay = Dust.NewDustPerfect(player.Center, DustID.CursedTorch, burstVel * 0.8f, 100, burstColor, 1.4f);
                decay.noGravity = true;
                decay.fadeIn = 1.3f;
            }
            
            // Contrasting sparkle ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 sparklePos = player.Center + angle.ToRotationVector2() * 25f;
                Color sparkleColor = i % 2 == 0 ? DecayPurple : DeathGreen;
                CustomParticles.PrismaticSparkle(sparklePos, sparkleColor, 0.35f);
            }
            
            // Music note starburst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color noteColor = Color.Lerp(DecayPurple, AutumnOrange, Main.rand.NextFloat());
                ThemedParticles.MusicNote(player.Center, noteVel, noteColor, 0.9f, 28);
            }

            player.statMana -= Item.mana * 2; // Extra mana cost for charged attack
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Only fire normal bolts if not charging
            if (isCharging && chargeTimer > 15) return false;

            // Simple muzzle VFX - EARLY GAME
            CustomParticles.GenericFlare(position, DecayPurple * 0.5f, 0.3f, 12);

            for (int i = 0; i < 3; i++)
            {
                Vector2 burstVel = velocity.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(1.5f, 3f) + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color burstColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.35f;
                var burst = new GenericGlowParticle(position, burstVel, burstColor, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // Fire main bolt
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.08f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, DecayPurple * 0.3f, rotation, origin, scale * pulse * 1.25f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, DeathGreen * 0.22f, rotation, origin, scale * 1.12f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, WitherBrown * 0.18f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, DecayPurple.ToVector3() * 0.4f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DecayBolt", "Fires piercing decay bolts that wither enemies") { OverrideColor = DecayPurple });
            tooltips.Add(new TooltipLine(Mod, "EntropicField", "Bolts create damaging decay zones on impact") { OverrideColor = DeathGreen });
            tooltips.Add(new TooltipLine(Mod, "AutumnsWrath", "Hold to charge, release a devastating withering wave") { OverrideColor = AutumnOrange });
            tooltips.Add(new TooltipLine(Mod, "LifeDrain", "5% of damage dealt heals you") { OverrideColor = WitherBrown });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'All things must wither and return to the earth'") { OverrideColor = Color.Lerp(DecayPurple, DeathGreen, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<HarvestBar>(), 16)
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofSight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
