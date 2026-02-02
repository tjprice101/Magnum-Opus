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
    /// Harvest Reaper - Autumn-themed melee weapon (Post-Plantera tier)
    /// A massive scythe channeling autumn's decay.
    /// - Reaping Strike: Large sweeping arc with decay particles (145 damage)
    /// - Soul Harvest: Kills generate soul wisps that heal player
    /// - Autumn's Decay: Every 5th hit applies stacking decay debuff
    /// - Twilight Slash: Every 8th swing unleashes a massive crescent wave
    /// </summary>
    public class HarvestReaper : ModItem
    {
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnBrown = new Color(139, 90, 43);
        private static readonly Color AutumnRed = new Color(178, 34, 34);
        private static readonly Color AutumnGold = new Color(218, 165, 32);
        private static readonly Color DecayPurple = new Color(100, 50, 120);

        private int swingCount = 0;
        private int hitCount = 0;

        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.damage = 145;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7f;
            Item.value = Item.buyPrice(gold: 35);
            Item.rare = ItemRarityID.Lime;
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<DecayCrescentWave>();
            Item.shootSpeed = 12f;
        }

        public override void HoldItem(Player player)
        {
            // Sparse decay particles - EARLY GAME: subtle
            if (Main.rand.NextBool(25))
            {
                Vector2 auraPos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 auraVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -Main.rand.NextFloat(0.4f, 1.0f));
                Color auraColor = Color.Lerp(AutumnOrange, DecayPurple, Main.rand.NextFloat()) * 0.3f;
                var aura = new GenericGlowParticle(auraPos, auraVel, auraColor, 0.15f, 25, true);
                MagnumParticleHandler.SpawnParticle(aura);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.08f + 0.35f;
            Lighting.AddLight(player.Center, AutumnOrange.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            swingCount++;

            // Swing VFX - decay particles along arc
            float arcAngle = velocity.ToRotation();
            for (int i = 0; i < 8; i++)
            {
                float angle = arcAngle + MathHelper.ToRadians(-40f + i * 10f);
                float dist = Main.rand.NextFloat(40f, 80f);
                Vector2 particlePos = player.Center + angle.ToRotationVector2() * dist;
                Vector2 particleVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color particleColor = Color.Lerp(AutumnOrange, AutumnBrown, Main.rand.NextFloat()) * 0.6f;
                var particle = new GenericGlowParticle(particlePos, particleVel, particleColor, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Twilight Slash - every 8th swing
            if (swingCount >= 8)
            {
                swingCount = 0;

                // Spawn crescent wave
                Projectile.NewProjectile(source, player.Center, velocity * 1.5f, type, (int)(damage * 1.6f), knockback, player.whoAmI);

                // Simple Twilight Slash VFX - EARLY GAME
                CustomParticles.GenericFlare(player.Center, AutumnGold, 0.5f, 15);
                CustomParticles.HaloRing(player.Center, DecayPurple * 0.4f, 0.35f, 12);
                
                // Small radial burst
                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.TwoPi * i / 5f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 6f);
                    Color burstColor = Color.Lerp(AutumnOrange, DecayPurple, (float)i / 5f) * 0.5f;
                    var burst = new GenericGlowParticle(player.Center, burstVel, burstColor, 0.25f, 18, true);
                    MagnumParticleHandler.SpawnParticle(burst);
                }
            }

            return false;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Vector2 hitCenter = hitbox.Center.ToVector2();
            
            // === SPECTACULAR SWING SYSTEM - MID TIER (3-4 arcs with decay swirls) ===
            SpectacularMeleeSwing.OnSwing(player, hitbox, AutumnOrange, DecayPurple, 
                SpectacularMeleeSwing.SwingTier.Mid, SpectacularMeleeSwing.WeaponTheme.Autumn);
            
            // === IRIDESCENT WINGSPAN-STYLE VFX (Autumn Theme) ===
            
            // HEAVY DUST TRAILS - orange/brown autumn gradient (2+ per frame)
            for (int d = 0; d < 2; d++)
            {
                Vector2 dustPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                Dust dust1 = Dust.NewDustPerfect(dustPos, DustID.Torch, player.velocity * 0.3f + Main.rand.NextVector2Circular(2f, 2f), 0, AutumnOrange, 1.1f);
                dust1.noGravity = true;
                dust1.fadeIn = 1.4f;
                
                Dust dust2 = Dust.NewDustPerfect(dustPos + Main.rand.NextVector2Circular(5f, 5f), DustID.GoldCoin, player.velocity * 0.2f, 0, AutumnGold, 0.9f);
                dust2.noGravity = true;
                dust2.fadeIn = 1.3f;
            }
            
            // CONTRASTING SPARKLES - harvest gold sparkles (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                var sparkle = new SparkleParticle(sparklePos, player.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f), AutumnGold, 0.5f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // AUTUMN SHIMMER TRAILS - cycling orange to brown hues via hslToRgb (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Vector2 shimmerPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                // Autumn hues: 0.08-0.12 (orange-gold range)
                float hue = Main.rand.NextFloat(0.08f, 0.12f);
                Color shimmerColor = Main.hslToRgb(hue, 0.9f, 0.65f);
                var shimmer = new GenericGlowParticle(shimmerPos, player.velocity * 0.25f + Main.rand.NextVector2Circular(1.5f, 1.5f), shimmerColor, 0.35f, 22, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }
            
            // PEARLESCENT HARVEST EFFECTS - color shifting amber (1-in-4)
            if (Main.rand.NextBool(4))
            {
                Vector2 pearlPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                float colorShift = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.5f + 0.5f;
                Color pearlColor = Color.Lerp(AutumnOrange, AutumnGold, colorShift) * 0.7f;
                var pearl = new GenericGlowParticle(pearlPos, player.velocity * 0.2f + new Vector2(0, -0.8f), pearlColor, 0.32f, 20, true);
                MagnumParticleHandler.SpawnParticle(pearl);
            }
            
            // FREQUENT FLARES - autumn glow flares (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Vector2 flarePos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                Color flareColor = Color.Lerp(AutumnOrange, AutumnRed, Main.rand.NextFloat());
                CustomParticles.GenericFlare(flarePos, flareColor, Main.rand.NextFloat(0.25f, 0.4f), 14);
            }
            
            // FALLING LEAF PARTICLES - autumn leaf drift (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Vector2 leafPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                Vector2 leafVel = player.velocity * 0.1f + new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-0.5f, 1f));
                Color leafColor = Main.rand.NextBool() ? AutumnOrange : AutumnRed;
                var leaf = new GenericGlowParticle(leafPos, leafVel, leafColor * 0.65f, 0.28f, 30, true);
                MagnumParticleHandler.SpawnParticle(leaf);
            }
            
            // MUSIC NOTES - harvest melody (1-in-6)
            if (Main.rand.NextBool(6))
            {
                Vector2 notePos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                Vector2 noteVel = player.velocity * 0.1f + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
                ThemedParticles.MusicNote(notePos, noteVel, AutumnGold, 0.85f, 28);
            }
            
            // PULSING LIGHT
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f + 0.55f;
            Lighting.AddLight(hitCenter, AutumnOrange.ToVector3() * pulse);
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            hitCount++;

            // Autumn's Decay - every 5th hit
            if (hitCount >= 5)
            {
                hitCount = 0;
                // Apply stacking decay (using vanilla Ichor as proxy for armor reduction)
                target.AddBuff(BuffID.Ichor, 300);
                
                // === SEEKING AUTUMN DECAY CRYSTALS ===
                SeekingCrystalHelper.SpawnAutumnCrystals(
                    player.GetSource_OnHit(target), target.Center, (target.Center - player.Center).SafeNormalize(Vector2.Zero) * 4f, 
                    (int)(damageDone * 0.4f), hit.Knockback, player.whoAmI, 5);
                
                // DECAY SPECIAL VFX
                CustomParticles.GenericFlare(target.Center, DecayPurple, 0.6f, 18);
                CustomParticles.HaloRing(target.Center, DecayPurple * 0.6f, 0.45f, 16);
                
                // Decay burst
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 decayVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                    Color decayColor = Color.Lerp(DecayPurple, AutumnBrown, Main.rand.NextFloat()) * 0.6f;
                    var decay = new GenericGlowParticle(target.Center, decayVel, decayColor, 0.3f, 20, true);
                    MagnumParticleHandler.SpawnParticle(decay);
                }
            }

            // === IRIDESCENT WINGSPAN-STYLE IMPACT VFX ===
            
            // GRADIENT HALO RINGS (4 stacked, outer to inner)
            for (int h = 0; h < 4; h++)
            {
                float progress = h / 4f;
                Color haloColor = Color.Lerp(AutumnOrange, DecayPurple, progress);
                float haloScale = 0.45f - h * 0.08f;
                int haloLife = 16 - h * 2;
                CustomParticles.HaloRing(target.Center, haloColor * (0.7f - progress * 0.2f), haloScale, haloLife);
            }
            
            // AUTUMN SHIMMER FLARES - radial burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 flarePos = target.Center + angle.ToRotationVector2() * Main.rand.NextFloat(8f, 20f);
                float hue = Main.rand.NextFloat(0.06f, 0.14f); // Orange-amber range
                Color flareColor = Main.hslToRgb(hue, 0.85f, 0.6f);
                CustomParticles.GenericFlare(flarePos, flareColor, Main.rand.NextFloat(0.25f, 0.4f), 14);
            }
            
            // RADIAL DUST BURST
            for (int d = 0; d < 14; d++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustPerfect(target.Center, Main.rand.NextBool() ? DustID.Torch : DustID.GoldCoin, dustVel, 0, Color.White, 1.1f);
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }
            
            // HARVEST GOLD SPARKLES
            for (int s = 0; s < 5; s++)
            {
                Vector2 sparklePos = target.Center + Main.rand.NextVector2Circular(15f, 15f);
                var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(3f, 3f), AutumnGold, 0.45f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // FALLING LEAVES ON HIT
            for (int i = 0; i < 6; i++)
            {
                Vector2 leafVel = new Vector2(Main.rand.NextFloat(-4f, 4f), -Main.rand.NextFloat(2f, 5f));
                Color leafColor = Main.rand.NextBool() ? AutumnOrange : AutumnRed;
                var leaf = new GenericGlowParticle(target.Center, leafVel, leafColor * 0.7f, 0.28f, 35, true);
                MagnumParticleHandler.SpawnParticle(leaf);
            }
            
            // MUSIC NOTES BURST
            for (int n = 0; n < 5; n++)
            {
                float angle = MathHelper.TwoPi * n / 5f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color noteColor = Color.Lerp(AutumnOrange, AutumnGold, Main.rand.NextFloat());
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.75f, 25);
            }

            // Soul Harvest - kills generate healing wisps
            bool killed = target.life <= 0;
            if (killed)
            {
                // Spawn soul wisp that homes to player
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(
                        player.GetSource_OnHit(target),
                        target.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<SoulWisp>(),
                        0,
                        0,
                        player.whoAmI
                    );
                }

                // Simple Soul Harvest VFX - EARLY GAME
                CustomParticles.GenericFlare(target.Center, AutumnGold, 0.5f, 16);
                CustomParticles.HaloRing(target.Center, DecayPurple * 0.4f, 0.35f, 12);

                // Soul release particles
                for (int i = 0; i < 5; i++)
                {
                    Vector2 soulVel = Main.rand.NextVector2Circular(4f, 4f) + new Vector2(0, -1.5f);
                    Color soulColor = Color.Lerp(AutumnGold, Color.White, Main.rand.NextFloat()) * 0.5f;
                    var soul = new GenericGlowParticle(target.Center, soulVel, soulColor, 0.22f, 22, true);
                    MagnumParticleHandler.SpawnParticle(soul);
                }
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.08f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, AutumnOrange * 0.3f, rotation, origin, scale * pulse * 1.25f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, DecayPurple * 0.22f, rotation, origin, scale * 1.12f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, AutumnRed * 0.18f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, AutumnOrange.ToVector3() * 0.4f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ReapingStrike", "Massive sweeping strikes with decay particles") { OverrideColor = AutumnOrange });
            tooltips.Add(new TooltipLine(Mod, "SoulHarvest", "Kills generate soul wisps that heal you") { OverrideColor = AutumnGold });
            tooltips.Add(new TooltipLine(Mod, "AutumnsDecay", "Every 5th hit applies armor-reducing decay") { OverrideColor = DecayPurple });
            tooltips.Add(new TooltipLine(Mod, "TwilightSlash", "Every 8th swing unleashes a devastating crescent wave") { OverrideColor = AutumnRed });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The harvest claims all in due time'") { OverrideColor = Color.Lerp(AutumnOrange, AutumnBrown, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<HarvestBar>(), 18)
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofFright, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
