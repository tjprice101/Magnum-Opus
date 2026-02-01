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

            // Cherry blossom petal trail - dense and beautiful
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustPos = hitCenter + Main.rand.NextVector2Circular(hitbox.Width / 2, hitbox.Height / 2);
                Vector2 dustVel = player.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f);
                Color petalColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat());
                
                var petal = new GenericGlowParticle(dustPos, dustVel, petalColor, 0.35f, 30, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }

            // Sparkle accents
            if (Main.rand.NextBool(3))
            {
                Vector2 sparklePos = hitCenter + Main.rand.NextVector2Circular(20f, 20f);
                CustomParticles.GenericFlare(sparklePos, SpringWhite, 0.3f, 12);
            }            
            // Music notes in swing trail
            if (Main.rand.NextBool(3))
            {
                Vector2 notePos = hitCenter + Main.rand.NextVector2Circular(8f, 8f);
                Vector2 noteVel = (player.direction * Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 2.5f);
                Color noteColor = Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat());
                ThemedParticles.MusicNote(notePos, noteVel, noteColor * 0.85f, 0.75f, 30);
                
                // Sparkle companion
                var sparkle = new SparkleParticle(notePos, noteVel * 0.5f, SpringWhite * 0.5f, 0.2f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            // Light green nature energy trails
            if (Main.rand.NextBool(4))
            {
                Vector2 trailPos = hitCenter + Main.rand.NextVector2Circular(15f, 15f);
                var greenTrail = new GenericGlowParticle(trailPos, -player.velocity * 0.1f, SpringGreen * 0.6f, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(greenTrail);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            hitCounter++;

            // Impact VFX - petal burst with sparkles
            CustomParticles.GenericFlare(target.Center, SpringPink, 0.6f, 18);
            CustomParticles.GenericFlare(target.Center, SpringWhite * 0.7f, 0.4f, 12);
            
            // Sparkle accents instead of halo
            for (int s = 0; s < 4; s++)
            {
                Vector2 sparkleOffset = Main.rand.NextVector2Circular(15f, 15f);
                CustomParticles.GenericFlare(target.Center + sparkleOffset, CherryBlossom * 0.8f, 0.2f, 10);
            }
            
            // Scatter petals on hit
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 petalVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color petalColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat());
                var petal = new GenericGlowParticle(target.Center, petalVel, petalColor, 0.3f, 25, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }
            
            // Music notes spiral outward on impact
            for (int i = 0; i < 4; i++)
            {
                float noteAngle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 3.5f);
                Color noteColor = Color.Lerp(SpringPink, SpringGreen, (float)i / 4f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.7f, 32);
            }
            
            // Sparkle accents on impact
            for (int i = 0; i < 2; i++)
            {
                var sparkle = new SparkleParticle(target.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(2f, 2f), SpringWhite * 0.6f, 0.22f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Renewal Strike: Every 5th hit heals 8 HP
            if (hitCounter >= 5)
            {
                hitCounter = 0;
                player.Heal(8);
                
                // Healing VFX - layered bloom instead of halo
                CustomParticles.GenericFlare(player.Center, Color.White, 0.9f, 22);
                CustomParticles.GenericFlare(player.Center, SpringGreen, 0.8f, 25);
                CustomParticles.GenericFlare(player.Center, SpringGreen * 0.6f, 0.5f, 18);
                
                // Healing sparkle ring
                for (int s = 0; s < 6; s++)
                {
                    float sparkleAngle = MathHelper.TwoPi * s / 6f;
                    Vector2 sparklePos = player.Center + sparkleAngle.ToRotationVector2() * 20f;
                    CustomParticles.GenericFlare(sparklePos, SpringGreen * 0.9f, 0.3f, 15);
                }
                
                // Green healing particles rising
                for (int i = 0; i < 8; i++)
                {
                    Vector2 healPos = player.Center + Main.rand.NextVector2Circular(20f, 20f);
                    Vector2 healVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(2f, 4f));
                    var healParticle = new GenericGlowParticle(healPos, healVel, SpringGreen, 0.4f, 30, true);
                    MagnumParticleHandler.SpawnParticle(healParticle);
                }
                
                // Healing music note ring
                ThemedParticles.MusicNoteRing(player.Center, SpringGreen, 35f, 6);
                
                CombatText.NewText(player.Hitbox, SpringGreen, "Renewal!");
            }

            // Spring Bloom: Critical hits cause flower burst AoE
            if (hit.Crit)
            {
                // === SEEKING SPRING BLOSSOM CRYSTALS ===
                SeekingCrystalHelper.SpawnSpringCrystals(
                    player.GetSource_OnHit(target), target.Center, (target.Center - player.Center).SafeNormalize(Vector2.Zero) * 4f, 
                    (int)(damageDone * 0.4f), hit.Knockback, player.whoAmI, 4);
                
                // Massive petal explosion
                CustomParticles.GenericFlare(target.Center, Color.White, 1.0f, 20);
                CustomParticles.GenericFlare(target.Center, SpringPink, 0.8f, 18);
                
                // Cascading bloom flares instead of halo rings
                for (int ring = 0; ring < 4; ring++)
                {
                    Color ringColor = Color.Lerp(SpringPink, SpringGreen, ring / 4f);
                    float ringScale = 0.5f + ring * 0.15f;
                    CustomParticles.GenericFlare(target.Center, ringColor * 0.7f, ringScale, 15 + ring * 3);
                    
                    // Scattered sparkles at ring radius
                    for (int s = 0; s < 3; s++)
                    {
                        float sparkAngle = MathHelper.TwoPi * s / 3f + ring * 0.5f;
                        Vector2 sparkPos = target.Center + sparkAngle.ToRotationVector2() * (25f + ring * 12f);
                        CustomParticles.GenericFlare(sparkPos, ringColor * 0.8f, 0.25f, 12 + ring * 2);
                    }
                }
                
                // Petal explosion burst
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                    Color burstColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat());
                    var burst = new GenericGlowParticle(target.Center, burstVel, burstColor, 0.4f, 30, true);
                    MagnumParticleHandler.SpawnParticle(burst);
                }
                
                // Music note ring and burst for Spring Bloom
                ThemedParticles.MusicNoteRing(target.Center, SpringPink, 50f, 8);
                ThemedParticles.MusicNoteBurst(target.Center, SpringGreen, 6, 5f);
                
                // Sparkle starburst
                for (int i = 0; i < 8; i++)
                {
                    var sparkle = new SparkleParticle(target.Center, (MathHelper.TwoPi * i / 8f).ToRotationVector2() * 4f,
                        SpringWhite * 0.7f, 0.28f, 22);
                    MagnumParticleHandler.SpawnParticle(sparkle);
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
