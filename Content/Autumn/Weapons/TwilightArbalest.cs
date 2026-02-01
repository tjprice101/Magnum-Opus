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
    /// Twilight Arbalest - Autumn-themed ranged weapon (Post-Plantera tier)
    /// A heavy crossbow that fires bolts infused with autumn's fading light.
    /// - Twilight Bolt: Fires piercing bolts that leave decay trails (98 damage)
    /// - Fading Light: Bolts gain damage as they travel, up to +50%
    /// - Harvest Moon: Every 6th shot fires a large seeking moon bolt
    /// - Dusk's Embrace: Critical hits spawn homing leaf shards
    /// </summary>
    public class TwilightArbalest : ModItem
    {
        private static readonly Color TwilightPurple = new Color(120, 60, 140);
        private static readonly Color TwilightOrange = new Color(255, 120, 60);
        private static readonly Color AutumnGold = new Color(218, 165, 32);
        private static readonly Color MoonSilver = new Color(200, 200, 220);

        private int shotCount = 0;

        public override void SetDefaults()
        {
            Item.width = 56;
            Item.height = 28;
            Item.damage = 98;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.buyPrice(gold: 32);
            Item.rare = ItemRarityID.Lime;
            Item.UseSound = SoundID.Item102;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<TwilightBolt>();
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Arrow;
        }

        public override void HoldItem(Player player)
        {
            // Ambient twilight particles
            if (Main.rand.NextBool(12))
            {
                Vector2 auraPos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 auraVel = new Vector2(0, -Main.rand.NextFloat(0.5f, 1f));
                Color auraColor = Color.Lerp(TwilightPurple, TwilightOrange, Main.rand.NextFloat()) * 0.35f;
                var aura = new GenericGlowParticle(auraPos, auraVel, auraColor, 0.18f, 28, true);
                MagnumParticleHandler.SpawnParticle(aura);
            }

            // Floating autumn melody notes
            if (Main.rand.NextBool(12))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(38f, 38f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.2f, 0.5f));
                Color noteColor = Color.Lerp(TwilightOrange, TwilightPurple, Main.rand.NextFloat()) * 0.6f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.75f, 40);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.035f) * 0.08f + 0.35f;
            Lighting.AddLight(player.Center, TwilightPurple.ToVector3() * pulse);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Convert all arrows to twilight bolts
            type = ModContent.ProjectileType<TwilightBolt>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCount++;

            // Muzzle flash - layered bloom instead of halo
            CustomParticles.GenericFlare(position, TwilightOrange, 0.5f, 15);
            CustomParticles.GenericFlare(position, TwilightPurple * 0.4f, 0.35f, 12);
            
            // Music note on shot
            ThemedParticles.MusicNote(position, velocity * 0.1f, AutumnGold * 0.8f, 0.7f, 25);
            
            // Twilight wisp burst
            for (int wisp = 0; wisp < 4; wisp++)
            {
                float wispAngle = MathHelper.TwoPi * wisp / 4f;
                Vector2 wispPos = position + wispAngle.ToRotationVector2() * 10f;
                CustomParticles.GenericFlare(wispPos, TwilightPurple * 0.5f, 0.15f, 9);
            }

            // Particle burst on fire
            for (int i = 0; i < 5; i++)
            {
                Vector2 burstVel = velocity.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(2f, 5f) + Main.rand.NextVector2Circular(2f, 2f);
                Color burstColor = Color.Lerp(TwilightPurple, TwilightOrange, Main.rand.NextFloat()) * 0.5f;
                var burst = new GenericGlowParticle(position, burstVel, burstColor, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // Harvest Moon - every 6th shot
            if (shotCount >= 6)
            {
                shotCount = 0;

                // Spawn large moon bolt
                Projectile.NewProjectile(source, position, velocity * 0.9f, ModContent.ProjectileType<HarvestMoonBolt>(), (int)(damage * 1.75f), knockback * 1.5f, player.whoAmI);

                // Extra VFX - layered bloom instead of halo
                CustomParticles.GenericFlare(position, MoonSilver, 0.7f, 20);
                CustomParticles.GenericFlare(position, AutumnGold * 0.5f, 0.55f, 15);
                
                // Music note ring and burst for Harvest Moon
                ThemedParticles.MusicNoteRing(position, TwilightOrange, 40f, 6);
                ThemedParticles.MusicNoteBurst(position, MoonSilver, 5, 4f);
                
                // Harvest glyphs (Autumn theme)
                CustomParticles.GlyphBurst(position, TwilightPurple, 4, 3f);
                
                // Sparkle accents
                for (int sparkIdx = 0; sparkIdx < 4; sparkIdx++)
                {
                    var sparkle = new SparkleParticle(position + Main.rand.NextVector2Circular(12f, 12f),
                        Main.rand.NextVector2Circular(2f, 2f), AutumnGold * 0.5f, 0.2f, 16);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // Moon wisp burst
                for (int wisp = 0; wisp < 5; wisp++)
                {
                    float wispAngle = MathHelper.TwoPi * wisp / 5f;
                    Vector2 wispPos = position + wispAngle.ToRotationVector2() * 15f;
                    CustomParticles.GenericFlare(wispPos, AutumnGold * 0.6f, 0.22f, 12);
                }
            }

            // Normal bolt
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

            spriteBatch.Draw(texture, position, null, TwilightPurple * 0.25f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, null, TwilightOrange * 0.2f, rotation, origin, scale * 1.1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Lighting.AddLight(Item.Center, TwilightPurple.ToVector3() * 0.35f);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "TwilightBolt", "Fires piercing twilight bolts that leave decay trails") { OverrideColor = TwilightPurple });
            tooltips.Add(new TooltipLine(Mod, "FadingLight", "Bolts gain up to 50% bonus damage as they travel") { OverrideColor = TwilightOrange });
            tooltips.Add(new TooltipLine(Mod, "HarvestMoon", "Every 6th shot fires a large seeking harvest moon bolt") { OverrideColor = MoonSilver });
            tooltips.Add(new TooltipLine(Mod, "DusksEmbrace", "Critical hits spawn homing leaf shards") { OverrideColor = AutumnGold });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The last light before the long night'") { OverrideColor = Color.Lerp(TwilightPurple, TwilightOrange, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<HarvestBar>(), 16)
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 1)
                .AddIngredient(ItemID.SoulofFright, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
