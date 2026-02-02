using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.Winter.Projectiles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Winter.Weapons
{
    /// <summary>
    /// Glacial Executioner - Winter-themed melee weapon (Post-Golem tier)
    /// A massive frozen greataxe that channels winter's wrath.
    /// - Frozen Cleave: Devastating swings that leave ice trails (195 damage)
    /// - Absolute Zero: Every hit has 25% chance to freeze enemies solid
    /// - Avalanche Strike: Every 6th swing creates a cascading ice wave
    /// - Permafrost: Frozen enemies take 30% more damage from all sources
    /// </summary>
    public class GlacialExecutioner : ModItem
    {
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color DeepBlue = new Color(60, 100, 180);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);

        private int swingCount = 0;

        public override void SetDefaults()
        {
            Item.width = 72;
            Item.height = 72;
            Item.damage = 195;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 32;
            Item.useAnimation = 32;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.value = Item.buyPrice(gold: 45);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<AvalancheWave>();
            Item.shootSpeed = 14f;
        }

        public override void HoldItem(Player player)
        {
            // Subtle frost particles
            if (Main.rand.NextBool(25))
            {
                Vector2 auraPos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 auraVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.3f, 0.3f));
                Color auraColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()) * 0.35f;
                var aura = new GenericGlowParticle(auraPos, auraVel, auraColor, 0.18f, 25, true);
                MagnumParticleHandler.SpawnParticle(aura);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.08f + 0.35f;
            Lighting.AddLight(player.Center, IceBlue.ToVector3() * pulse);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            swingCount++;

            // Ice trail along swing arc
            float arcAngle = velocity.ToRotation();
            for (int i = 0; i < 10; i++)
            {
                float angle = arcAngle + MathHelper.ToRadians(-45f + i * 10f);
                float dist = Main.rand.NextFloat(45f, 90f);
                Vector2 particlePos = player.Center + angle.ToRotationVector2() * dist;
                Vector2 particleVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color particleColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat()) * 0.6f;
                var particle = new GenericGlowParticle(particlePos, particleVel, particleColor, 0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Avalanche Strike - every 6th swing
            if (swingCount >= 6)
            {
                swingCount = 0;

                // Spawn avalanche wave
                Projectile.NewProjectile(source, player.Center, velocity * 1.2f, type, (int)(damage * 1.5f), knockback, player.whoAmI);

                // VFX burst
                CustomParticles.GenericFlare(player.Center, FrostWhite, 0.7f, 18);
                CustomParticles.HaloRing(player.Center, IceBlue, 0.4f, 15);
                
                // Simple ice burst
                for (int i = 0; i < 5; i++)
                {
                    Vector2 burstVel = Main.rand.NextVector2Circular(6f, 6f);
                    Color burstColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()) * 0.5f;
                    var burst = new GenericGlowParticle(player.Center, burstVel, burstColor, 0.25f, 18, true);
                    MagnumParticleHandler.SpawnParticle(burst);
                }
            }

            return false;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Vector2 hitCenter = hitbox.Center.ToVector2();
            
            // === SPECTACULAR SWING SYSTEM - HIGH TIER (5-6 arcs with frost crystals) ===
            SpectacularMeleeSwing.OnSwing(player, hitbox, IceBlue, CrystalCyan, 
                SpectacularMeleeSwing.SwingTier.High, SpectacularMeleeSwing.WeaponTheme.Winter);
            
            // === IRIDESCENT WINGSPAN-STYLE HEAVY DUST TRAILS ===
            // Heavy ice blue dust trail #1
            float trailProgress1 = Main.rand.NextFloat();
            Color iceGradient = Color.Lerp(IceBlue, CrystalCyan, trailProgress1);
            Dust heavyIce = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 
                DustID.IceTorch, player.velocity.X * 0.3f, player.velocity.Y * 0.3f, 100, iceGradient, 1.5f);
            heavyIce.noGravity = true;
            heavyIce.fadeIn = 1.4f;
            heavyIce.velocity = heavyIce.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(1.2f, 1.8f);
            
            // Heavy frost white dust trail #2
            float trailProgress2 = Main.rand.NextFloat();
            Color frostGradient = Color.Lerp(FrostWhite, IceBlue, trailProgress2);
            Dust heavyFrost = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 
                DustID.Frost, player.velocity.X * 0.25f, player.velocity.Y * 0.25f, 80, frostGradient, 1.4f);
            heavyFrost.noGravity = true;
            heavyFrost.fadeIn = 1.3f;
            heavyFrost.velocity = heavyFrost.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(1.0f, 1.6f);
            
            // === CONTRASTING CRYSTAL SPARKLES (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                CustomParticles.PrismaticSparkle(sparklePos, CrystalCyan, 0.35f);
                
                Dust crystalDust = Dust.NewDustDirect(sparklePos, 1, 1, DustID.BlueCrystalShard, 0f, 0f, 100, default, 0.9f);
                crystalDust.noGravity = true;
                crystalDust.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }
            
            // === FROST SHIMMER TRAIL (Main.hslToRgb in cyan-blue range) (1-in-3) ===
            if (Main.rand.NextBool(3))
            {
                // Frost shimmer - cycling through cyan to blue hues (0.5-0.65 hue range)
                float frostHue = 0.5f + (Main.GameUpdateCount * 0.015f % 0.15f);
                Color shimmerColor = Main.hslToRgb(frostHue, 0.85f, 0.8f);
                Vector2 shimmerPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                CustomParticles.GenericFlare(shimmerPos, shimmerColor, 0.4f, 12);
            }
            
            // === PEARLESCENT ICE EFFECTS (1-in-4) ===
            if (Main.rand.NextBool(4))
            {
                Vector2 pearlPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                float pearlShift = (Main.GameUpdateCount * 0.02f) % 1f;
                Color pearlColor = Color.Lerp(Color.Lerp(CrystalCyan, FrostWhite, pearlShift), 
                    IceBlue, (float)Math.Sin(pearlShift * MathHelper.TwoPi) * 0.3f + 0.3f);
                CustomParticles.GenericFlare(pearlPos, pearlColor * 0.85f, 0.32f, 15);
            }
            
            // === FREQUENT FLARES (1-in-2) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 flarePos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                Color flareColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat());
                CustomParticles.GenericFlare(flarePos, flareColor, 0.35f, 10);
            }
            
            // Frost trail particles - crystalline shards
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                Vector2 trailVel = player.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f);
                Color trailColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()) * 0.6f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.35f, 22, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // === MUSIC NOTES (1-in-6) ===
            if (Main.rand.NextBool(6))
            {
                Vector2 notePos = hitCenter + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 noteVel = (player.direction * Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(1.5f, 3f);
                Color noteColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat());
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.85f, 32);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Absolute Zero: 25% freeze chance
            if (Main.rand.NextFloat() < 0.25f)
            {
                // Freeze effect (Frozen debuff)
                target.AddBuff(BuffID.Frozen, 90);
                
                // === SEEKING WINTER FROST CRYSTALS ===
                SeekingCrystalHelper.SpawnWinterCrystals(
                    player.GetSource_OnHit(target), target.Center, (target.Center - player.Center).SafeNormalize(Vector2.Zero) * 5f, 
                    (int)(damageDone * 0.35f), hit.Knockback, player.whoAmI, 5);
                
                // Freeze VFX - enhanced
                CustomParticles.GenericFlare(target.Center, CrystalCyan, 0.7f, 20);
                CustomParticles.GenericFlare(target.Center, FrostWhite, 0.55f, 16);
            }

            // Always apply Frostburn
            target.AddBuff(BuffID.Frostburn2, 240);

            // === IRIDESCENT WINGSPAN-STYLE GRADIENT HALO RINGS (4 stacked) ===
            CustomParticles.HaloRing(target.Center, DeepBlue, 0.55f, 16);
            CustomParticles.HaloRing(target.Center, IceBlue, 0.45f, 14);
            CustomParticles.HaloRing(target.Center, CrystalCyan, 0.35f, 12);
            CustomParticles.HaloRing(target.Center, FrostWhite * 0.9f, 0.25f, 10);
            
            // Impact flare
            CustomParticles.GenericFlare(target.Center, IceBlue, 0.6f, 18);
            
            // === FROST SHIMMER FLARE BURST (10 flares) ===
            for (int i = 0; i < 10; i++)
            {
                float frostHue = 0.5f + Main.rand.NextFloat(0.15f);
                Color shimmerColor = Main.hslToRgb(frostHue, 0.85f, 0.8f);
                Vector2 flarePos = target.Center + Main.rand.NextVector2Circular(22f, 22f);
                CustomParticles.GenericFlare(flarePos, shimmerColor, 0.42f, 16);
            }
            
            // === RADIAL DUST EXPLOSION (16 dust particles) ===
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                
                Dust iceDust = Dust.NewDustDirect(target.Center, 1, 1, DustID.IceTorch, dustVel.X, dustVel.Y, 100, default, 1.4f);
                iceDust.noGravity = true;
                iceDust.fadeIn = 1.2f;
            }
            
            // === CONTRASTING CRYSTAL SPARKLES (6 sparkles) ===
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparklePos = target.Center + Main.rand.NextVector2Circular(28f, 28f);
                CustomParticles.PrismaticSparkle(sparklePos, CrystalCyan, 0.38f);
                
                Dust crystalDust = Dust.NewDustDirect(sparklePos, 1, 1, DustID.BlueCrystalShard, 0f, 0f, 80, default, 1.0f);
                crystalDust.noGravity = true;
            }
            
            // Ice shard burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                Color sparkColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()) * 0.6f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.32f, 20, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // === MUSIC NOTES BURST (6 notes) ===
            for (int i = 0; i < 6; i++)
            {
                float noteAngle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 4f);
                Color noteColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat());
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.9f, 32);
            }
        }

        public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            // Permafrost: Frozen enemies take 30% more damage
            if (target.HasBuff(BuffID.Frozen))
            {
                modifiers.FinalDamage *= 1.3f;
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.045f) * 0.1f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, position, null, IceBlue * 0.35f, rotation, origin, scale * pulse * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, CrystalCyan * 0.25f, rotation, origin, scale * 1.15f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, FrostWhite * 0.2f, rotation, origin, scale * 1.05f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, IceBlue.ToVector3() * 0.5f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FrozenCleave", "Devastating swings that leave trails of frost") { OverrideColor = IceBlue });
            tooltips.Add(new TooltipLine(Mod, "AbsoluteZero", "25% chance to freeze enemies solid on hit") { OverrideColor = CrystalCyan });
            tooltips.Add(new TooltipLine(Mod, "AvalancheStrike", "Every 6th swing unleashes a cascading ice wave") { OverrideColor = FrostWhite });
            tooltips.Add(new TooltipLine(Mod, "Permafrost", "Frozen enemies take 30% bonus damage") { OverrideColor = DeepBlue });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cold embrace of eternal winter'") { OverrideColor = Color.Lerp(IceBlue, FrostWhite, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<PermafrostBar>(), 20)
                .AddIngredient(ModContent.ItemType<WinterResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofMight, 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
