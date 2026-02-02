using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Spring.Projectiles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Spring.Weapons
{
    /// <summary>
    /// Blossom's Edge - Spring-themed melee sword (Post-WoF tier)
    /// A delicate blade that blooms with every swing, scattering cherry blossom petals.
    /// - Petal Trail: Swings leave behind a trail of damaging cherry blossom petals
    /// - Renewal Strike: Every 5th hit heals the player for 8 HP
    /// - Spring Bloom: Critical hits cause flowers to burst from enemies, dealing 50% damage in AoE
    /// - Vernal Vigor: Increased attack speed during daytime
    /// </summary>
    public class BlossomsEdge : ModItem
    {
        private int hitCounter = 0;
        
        // Spring colors - pink/white/light green
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringWhite = new Color(255, 250, 250);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color CherryBlossom = new Color(255, 183, 197);

        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 72;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.LightRed; // Post-WoF tier
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<BlossomPetal>();
            Item.shootSpeed = 8f;
            Item.scale = 1.1f;
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            // Vernal Vigor: +12% damage during daytime
            if (Main.dayTime)
            {
                damage += 0.12f;
            }
        }

        public override float UseSpeedMultiplier(Player player)
        {
            // Vernal Vigor: +15% attack speed during daytime
            return Main.dayTime ? 1.15f : 1f;
        }

        public override void HoldItem(Player player)
        {
            // Ambient petal particles while holding
            if (Main.rand.NextBool(8))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-0.5f, 0.5f));
                Color petalColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat());
                
                var petal = new GenericGlowParticle(pos, vel, petalColor * 0.8f, 0.3f, 40, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }

            // Orbiting flower petals
            if (Main.rand.NextBool(12))
            {
                float angle = Main.GameUpdateCount * 0.03f;
                for (int i = 0; i < 3; i++)
                {
                    float petalAngle = angle + MathHelper.TwoPi * i / 3f;
                    float radius = 30f + (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 8f;
                    Vector2 petalPos = player.Center + petalAngle.ToRotationVector2() * radius;
                    CustomParticles.GenericFlare(petalPos, CherryBlossom * 0.7f, 0.25f, 15);
                }
            }
            
            // Spring melody - floating music notes
            if (Main.rand.NextBool(14))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                Vector2 noteVel = new Vector2(0, -Main.rand.NextFloat(0.3f, 0.8f));
                Color noteColor = Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat()) * 0.7f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.75f, 45);
                
                // Accompanying sparkle
                var sparkle = new SparkleParticle(notePos, noteVel * 0.5f, SpringWhite * 0.4f, 0.18f, 25);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Soft spring lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 0.6f;
            Lighting.AddLight(player.Center, SpringPink.ToVector3() * pulse * 0.5f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Scatter 2-3 petals per swing
            int petalCount = Main.rand.Next(2, 4);
            for (int i = 0; i < petalCount; i++)
            {
                Vector2 perturbedVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(25));
                perturbedVelocity *= Main.rand.NextFloat(0.8f, 1.2f);
                Projectile.NewProjectile(source, position, perturbedVelocity, type, damage / 3, knockback * 0.5f, player.whoAmI);
            }
            return false;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Vector2 hitCenter = hitbox.Center.ToVector2();
            
            // === SPECTACULAR SWING SYSTEM - BASIC TIER (1-2 arcs) ===
            SpectacularMeleeSwing.OnSwing(player, hitbox, SpringPink, SpringGreen, 
                SpectacularMeleeSwing.SwingTier.Basic, SpectacularMeleeSwing.WeaponTheme.Spring);

            // === IRIDESCENT WINGSPAN-STYLE HEAVY DUST TRAILS ===
            // Heavy pink dust trail #1
            float trailProgress1 = Main.rand.NextFloat();
            Color pinkGradient = Color.Lerp(SpringPink, SpringWhite, trailProgress1);
            Dust heavyPink = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 
                DustID.PinkFairy, player.velocity.X * 0.3f, player.velocity.Y * 0.3f, 100, pinkGradient, 1.4f);
            heavyPink.noGravity = true;
            heavyPink.fadeIn = 1.4f;
            heavyPink.velocity = heavyPink.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(1.2f, 1.8f);
            
            // Heavy green dust trail #2
            float trailProgress2 = Main.rand.NextFloat();
            Color greenGradient = Color.Lerp(SpringGreen, SpringWhite, trailProgress2);
            Dust heavyGreen = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 
                DustID.GreenFairy, player.velocity.X * 0.25f, player.velocity.Y * 0.25f, 80, greenGradient, 1.3f);
            heavyGreen.noGravity = true;
            heavyGreen.fadeIn = 1.3f;
            heavyGreen.velocity = heavyGreen.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(1.0f, 1.6f);
            
            // === CONTRASTING WHITE SPARKLES (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                CustomParticles.PrismaticSparkle(sparklePos, SpringWhite, 0.3f);
                
                Dust whiteDust = Dust.NewDustDirect(sparklePos, 1, 1, DustID.WhiteTorch, 0f, 0f, 100, default, 0.8f);
                whiteDust.noGravity = true;
                whiteDust.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }
            
            // === SPRING SHIMMER TRAIL (Main.hslToRgb in pink-green range) (1-in-3) ===
            if (Main.rand.NextBool(3))
            {
                // Spring shimmer - cycling through pink to green hues (0.85-1.0 and 0.25-0.35 hue range)
                float springHue = Main.rand.NextBool() ? (0.85f + (Main.GameUpdateCount * 0.015f % 0.15f)) : (0.25f + (Main.GameUpdateCount * 0.015f % 0.1f));
                springHue = springHue % 1f;
                Color shimmerColor = Main.hslToRgb(springHue, 0.8f, 0.75f);
                Vector2 shimmerPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                CustomParticles.GenericFlare(shimmerPos, shimmerColor, 0.35f, 12);
            }
            
            // === PEARLESCENT BLOSSOM EFFECTS (1-in-4) ===
            if (Main.rand.NextBool(4))
            {
                Vector2 pearlPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                float pearlShift = (Main.GameUpdateCount * 0.02f) % 1f;
                Color pearlColor = Color.Lerp(Color.Lerp(SpringPink, SpringWhite, pearlShift), 
                    SpringGreen, (float)Math.Sin(pearlShift * MathHelper.TwoPi) * 0.3f + 0.3f);
                CustomParticles.GenericFlare(pearlPos, pearlColor * 0.8f, 0.28f, 15);
            }
            
            // === FREQUENT FLARES (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 flarePos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                Color flareColor = Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat());
                CustomParticles.GenericFlare(flarePos, flareColor, 0.32f, 10);
            }

            // Cherry blossom petal trail - dense and beautiful
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustPos = hitCenter + Main.rand.NextVector2Circular(hitbox.Width / 2, hitbox.Height / 2);
                Vector2 dustVel = player.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f);
                Color petalColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat());
                
                var petal = new GenericGlowParticle(dustPos, dustVel, petalColor, 0.35f, 30, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }

            // === MUSIC NOTES (1-in-6) ===
            if (Main.rand.NextBool(6))
            {
                Vector2 notePos = hitCenter + Main.rand.NextVector2Circular(8f, 8f);
                Vector2 noteVel = (player.direction * Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 2.5f);
                Color noteColor = Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat());
                ThemedParticles.MusicNote(notePos, noteVel, noteColor * 0.9f, 0.8f, 32);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            hitCounter++;

            // === IRIDESCENT WINGSPAN-STYLE GRADIENT HALO RINGS (3 stacked) ===
            CustomParticles.HaloRing(target.Center, SpringPink, 0.45f, 14);
            CustomParticles.HaloRing(target.Center, SpringWhite, 0.35f, 12);
            CustomParticles.HaloRing(target.Center, SpringGreen, 0.25f, 10);
            
            // Impact VFX - petal burst with flares
            CustomParticles.GenericFlare(target.Center, SpringPink, 0.6f, 18);
            CustomParticles.GenericFlare(target.Center, SpringWhite * 0.8f, 0.45f, 14);
            
            // === SPRING SHIMMER FLARE BURST (8 flares) ===
            for (int i = 0; i < 8; i++)
            {
                float springHue = Main.rand.NextBool() ? (0.9f + Main.rand.NextFloat(0.1f)) % 1f : 0.28f + Main.rand.NextFloat(0.07f);
                Color shimmerColor = Main.hslToRgb(springHue, 0.8f, 0.8f);
                Vector2 flarePos = target.Center + Main.rand.NextVector2Circular(18f, 18f);
                CustomParticles.GenericFlare(flarePos, shimmerColor, 0.35f, 14);
            }
            
            // === RADIAL DUST EXPLOSION (12 dust particles) ===
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                
                Dust pinkDust = Dust.NewDustDirect(target.Center, 1, 1, DustID.PinkFairy, dustVel.X, dustVel.Y, 100, default, 1.3f);
                pinkDust.noGravity = true;
                pinkDust.fadeIn = 1.2f;
            }
            
            // === CONTRASTING WHITE SPARKLES (4 sparkles) ===
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparklePos = target.Center + Main.rand.NextVector2Circular(22f, 22f);
                CustomParticles.PrismaticSparkle(sparklePos, SpringWhite, 0.3f);
            }
            
            // Scatter petals on hit
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 petalVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color petalColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat());
                var petal = new GenericGlowParticle(target.Center, petalVel, petalColor, 0.35f, 28, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }
            
            // === MUSIC NOTES BURST (4 notes) ===
            for (int i = 0; i < 4; i++)
            {
                float noteAngle = MathHelper.TwoPi * i / 4f;
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 3.5f);
                Color noteColor = Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat());
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.85f, 30);
            }
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(2f, 2f);
                ThemedParticles.MusicNote(target.Center, noteVel, SpringPink * 0.8f, 0.65f, 25);
            }

            // Renewal Strike: Every 5th hit heals 8 HP
            if (hitCounter >= 5)
            {
                hitCounter = 0;
                player.Heal(8);
                
                // Simple healing VFX - EARLY GAME
                CustomParticles.GenericFlare(player.Center, SpringGreen, 0.5f, 18);
                
                // Few rising particles
                for (int i = 0; i < 3; i++)
                {
                    Vector2 healPos = player.Center + Main.rand.NextVector2Circular(15f, 15f);
                    Vector2 healVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1.5f, 2.5f));
                    var healParticle = new GenericGlowParticle(healPos, healVel, SpringGreen * 0.7f, 0.25f, 22, true);
                    MagnumParticleHandler.SpawnParticle(healParticle);
                }
                
                CombatText.NewText(player.Hitbox, SpringGreen, "Renewal!");
            }

            // Spring Bloom: Critical hits cause flower burst AoE
            if (hit.Crit)
            {
                // === SEEKING SPRING BLOSSOM CRYSTALS ===
                SeekingCrystalHelper.SpawnSpringCrystals(
                    player.GetSource_OnHit(target), target.Center, (target.Center - player.Center).SafeNormalize(Vector2.Zero) * 4f, 
                    (int)(damageDone * 0.4f), hit.Knockback, player.whoAmI, 4);
                
                // Simple crit flash - EARLY GAME
                CustomParticles.GenericFlare(target.Center, SpringPink, 0.6f, 16);
                CustomParticles.HaloRing(target.Center, SpringGreen * 0.5f, 0.35f, 14);
                
                // Modest petal burst
                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.TwoPi * i / 5f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                    Color burstColor = Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat()) * 0.6f;
                    var burst = new GenericGlowParticle(target.Center, burstVel, burstColor, 0.25f, 20, true);
                    MagnumParticleHandler.SpawnParticle(burst);
                }

                // Deal AoE damage (50% of hit damage)
                int aoeDamage = damageDone / 2;
                float aoeRadius = 100f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && 
                        Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                    {
                        npc.SimpleStrikeNPC(aoeDamage, hit.HitDirection, false, 0f, DamageClass.Melee);
                        
                        // Mini petal burst on AoE targets
                        for (int j = 0; j < 4; j++)
                        {
                            Vector2 miniVel = Main.rand.NextVector2Circular(3f, 3f);
                            var miniPetal = new GenericGlowParticle(npc.Center, miniVel, SpringPink, 0.25f, 20, true);
                            MagnumParticleHandler.SpawnParticle(miniPetal);
                        }
                    }
                }
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.12f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer pink glow
            spriteBatch.Draw(texture, position, null, SpringPink * 0.4f, rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);
            // Middle white glow
            spriteBatch.Draw(texture, position, null, SpringWhite * 0.3f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            // Inner green accent
            spriteBatch.Draw(texture, position, null, SpringGreen * 0.25f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, SpringPink.ToVector3() * 0.5f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "PetalTrail", "Swings scatter damaging cherry blossom petals") { OverrideColor = SpringPink });
            tooltips.Add(new TooltipLine(Mod, "RenewalStrike", "Every 5th hit heals you for 8 HP") { OverrideColor = SpringGreen });
            tooltips.Add(new TooltipLine(Mod, "SpringBloom", "Critical hits cause flowers to burst, dealing 50% damage in area") { OverrideColor = SpringPink });
            tooltips.Add(new TooltipLine(Mod, "VernalVigor", "Increased damage and attack speed during daytime") { OverrideColor = new Color(255, 220, 100) });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Where the blade touches, spring eternally blooms'") { OverrideColor = Color.Lerp(SpringPink, SpringGreen, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<VernalBar>(), 12)
                .AddIngredient(ModContent.ItemType<SpringResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
