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
            // Ambient decay particles
            if (Main.rand.NextBool(10))
            {
                Vector2 auraPos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                Vector2 auraVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(0.5f, 1.5f));
                Color auraColor = Color.Lerp(AutumnOrange, DecayPurple, Main.rand.NextFloat()) * 0.4f;
                var aura = new GenericGlowParticle(auraPos, auraVel, auraColor, 0.2f, 30, true);
                MagnumParticleHandler.SpawnParticle(aura);
            }

            // Falling leaf particles
            if (Main.rand.NextBool(15))
            {
                Vector2 leafPos = player.Center + new Vector2(Main.rand.NextFloat(-60f, 60f), -40f);
                Vector2 leafVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(1f, 3f));
                Color leafColor = Main.rand.NextBool() ? AutumnOrange : AutumnRed;
                var leaf = new GenericGlowParticle(leafPos, leafVel, leafColor * 0.5f, 0.22f, 45, true);
                MagnumParticleHandler.SpawnParticle(leaf);
            }

            // Floating autumn melody notes
            if (Main.rand.NextBool(12))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(38f, 38f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.2f, 0.5f));
                Color noteColor = Color.Lerp(AutumnOrange, AutumnBrown, Main.rand.NextFloat()) * 0.6f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.75f, 40);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 0.4f;
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

                // VFX burst - layered decay bloom instead of halo
                CustomParticles.GenericFlare(player.Center, AutumnGold, 0.9f, 22);
                CustomParticles.GenericFlare(player.Center, AutumnOrange, 0.7f, 18);
                CustomParticles.GenericFlare(player.Center, DecayPurple * 0.6f, 0.5f, 15);
                
                // Music note ring and burst for special ability
                ThemedParticles.MusicNoteRing(player.Center, AutumnOrange, 40f, 6);
                ThemedParticles.MusicNoteBurst(player.Center, AutumnBrown, 5, 4f);
                
                // Harvest glyphs (Autumn decay theme)
                CustomParticles.GlyphBurst(player.Center, AutumnBrown, 4, 3f);
                
                // Sparkle accents
                for (int sparkIdx = 0; sparkIdx < 4; sparkIdx++)
                {
                    var sparkle = new SparkleParticle(player.Center + Main.rand.NextVector2Circular(12f, 12f),
                        Main.rand.NextVector2Circular(2f, 2f), AutumnGold * 0.5f, 0.2f, 16);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // Decay wisp burst
                for (int wisp = 0; wisp < 6; wisp++)
                {
                    float wispAngle = MathHelper.TwoPi * wisp / 6f;
                    Vector2 wispPos = player.Center + wispAngle.ToRotationVector2() * 22f;
                    Color wispColor = Color.Lerp(AutumnOrange, DecayPurple, (float)wisp / 6f);
                    CustomParticles.GenericFlare(wispPos, wispColor * 0.75f, 0.28f, 14);
                }

                // Radial decay particles
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                    Color burstColor = Color.Lerp(AutumnOrange, DecayPurple, (float)i / 12f) * 0.7f;
                    var burst = new GenericGlowParticle(player.Center, burstVel, burstColor, 0.35f, 25, true);
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
            
            // Trail particles - decay wisps
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                Vector2 trailVel = player.velocity * 0.3f + Main.rand.NextVector2Circular(2f, 2f);
                Color trailColor = Color.Lerp(AutumnOrange, AutumnRed, Main.rand.NextFloat()) * 0.5f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.28f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Music notes - melancholy autumn melody
            if (Main.rand.NextBool(4))
            {
                Vector2 notePos = hitCenter + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 noteVel = (player.direction * Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 2.5f);
                Color noteColor = Color.Lerp(AutumnOrange, AutumnBrown, Main.rand.NextFloat());
                ThemedParticles.MusicNote(notePos, noteVel, noteColor * 0.8f, 0.75f, 28);
            }
            
            // Falling leaves along swing
            if (Main.rand.NextBool(5))
            {
                Vector2 leafPos = hitCenter + Main.rand.NextVector2Circular(15f, 15f);
                Vector2 leafVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(1f, 3f));
                Color leafColor = Main.rand.NextBool() ? AutumnOrange : AutumnRed;
                var leaf = new GenericGlowParticle(leafPos, leafVel, leafColor * 0.6f, 0.25f, 30, true);
                MagnumParticleHandler.SpawnParticle(leaf);
            }
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
                
                // Decay burst VFX - layered bloom instead of halo
                CustomParticles.GenericFlare(target.Center, DecayPurple, 0.65f, 18);
                CustomParticles.GenericFlare(target.Center, DecayPurple * 0.6f, 0.45f, 15);
                
                // Music note burst for decay proc
                ThemedParticles.MusicNoteBurst(target.Center, AutumnBrown, 4, 3f);
                
                // Decay glyphs (harvest theme)
                CustomParticles.GlyphBurst(target.Center, DecayPurple, 3, 2.5f);
                
                // Sparkle accents
                for (int sparkIdx = 0; sparkIdx < 3; sparkIdx++)
                {
                    var sparkle = new SparkleParticle(target.Center + Main.rand.NextVector2Circular(10f, 10f),
                        Main.rand.NextVector2Circular(2f, 2f), AutumnGold * 0.4f, 0.18f, 14);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // Decay wisps around impact
                for (int wisp = 0; wisp < 4; wisp++)
                {
                    float wispAngle = MathHelper.TwoPi * wisp / 4f;
                    Vector2 wispPos = target.Center + wispAngle.ToRotationVector2() * 15f;
                    CustomParticles.GenericFlare(wispPos, DecayPurple * 0.7f, 0.22f, 12);
                }

                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 decayVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                    Color decayColor = Color.Lerp(DecayPurple, AutumnBrown, Main.rand.NextFloat()) * 0.6f;
                    var decay = new GenericGlowParticle(target.Center, decayVel, decayColor, 0.28f, 20, true);
                    MagnumParticleHandler.SpawnParticle(decay);
                }
            }

            // Standard hit VFX
            CustomParticles.GenericFlare(target.Center, AutumnOrange, 0.5f, 15);
            
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                Color sparkColor = Color.Lerp(AutumnOrange, AutumnRed, Main.rand.NextFloat()) * 0.5f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Falling leaves on hit
            for (int i = 0; i < 3; i++)
            {
                Vector2 leafVel = new Vector2(Main.rand.NextFloat(-3f, 3f), -Main.rand.NextFloat(2f, 5f));
                Color leafColor = Main.rand.NextBool() ? AutumnOrange : AutumnRed;
                var leaf = new GenericGlowParticle(target.Center, leafVel, leafColor * 0.6f, 0.2f, 35, true);
                MagnumParticleHandler.SpawnParticle(leaf);
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

                // Death VFX - layered bloom instead of halo
                CustomParticles.GenericFlare(target.Center, AutumnGold, 0.7f, 20);
                CustomParticles.GenericFlare(target.Center, DecayPurple, 0.55f, 18);
                CustomParticles.GenericFlare(target.Center, DecayPurple * 0.6f, 0.4f, 14);
                
                // Music note ring for soul harvest
                ThemedParticles.MusicNoteRing(target.Center, AutumnOrange, 35f, 5);
                ThemedParticles.MusicNoteBurst(target.Center, AutumnGold, 4, 3.5f);
                
                // Harvest glyphs for soul reaping
                CustomParticles.GlyphBurst(target.Center, AutumnBrown, 5, 4f);
                
                // Sparkle accents
                for (int sparkIdx = 0; sparkIdx < 5; sparkIdx++)
                {
                    var sparkle = new SparkleParticle(target.Center + Main.rand.NextVector2Circular(15f, 15f),
                        Main.rand.NextVector2Circular(3f, 3f), AutumnGold * 0.6f, 0.22f, 18);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // Soul wisp burst
                for (int wisp = 0; wisp < 5; wisp++)
                {
                    float wispAngle = MathHelper.TwoPi * wisp / 5f;
                    Vector2 wispPos = target.Center + wispAngle.ToRotationVector2() * 18f;
                    CustomParticles.GenericFlare(wispPos, DecayPurple * 0.7f, 0.22f, 12);
                }

                // Soul release particles
                for (int i = 0; i < 10; i++)
                {
                    Vector2 soulVel = Main.rand.NextVector2Circular(6f, 6f) + new Vector2(0, -2f);
                    Color soulColor = Color.Lerp(AutumnGold, Color.White, Main.rand.NextFloat()) * 0.7f;
                    var soul = new GenericGlowParticle(target.Center, soulVel, soulColor, 0.3f, 30, true);
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
