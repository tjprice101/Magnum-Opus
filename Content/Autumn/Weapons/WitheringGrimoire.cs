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
            // Ambient decay aura
            if (Main.rand.NextBool(10))
            {
                Vector2 auraPos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 auraVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(0.5f, 1.5f));
                Color auraColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.35f;
                var aura = new GenericGlowParticle(auraPos, auraVel, auraColor, 0.2f, 28, true);
                MagnumParticleHandler.SpawnParticle(aura);
            }

            // Floating autumn melody notes
            if (Main.rand.NextBool(12))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(38f, 38f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.2f, 0.5f));
                Color noteColor = Color.Lerp(AutumnOrange, new Color(139, 90, 43), Main.rand.NextFloat()) * 0.6f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.75f, 40);
            }

            // Charging logic
            if (player.channel && player.CheckMana(Item, -1, true, false))
            {
                isCharging = true;
                chargeTimer = Math.Min(chargeTimer + 1, MaxCharge);

                // Charge VFX
                float chargeProgress = (float)chargeTimer / MaxCharge;
                
                if (chargeTimer % 5 == 0)
                {
                    // Converging particles
                    float radius = 80f * (1f - chargeProgress * 0.6f);
                    int particleCount = 4 + (int)(chargeProgress * 6);
                    for (int i = 0; i < particleCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / particleCount + Main.GameUpdateCount * 0.05f;
                        Vector2 pos = player.Center + angle.ToRotationVector2() * radius;
                        Vector2 vel = (player.Center - pos).SafeNormalize(Vector2.Zero) * 2f;
                        Color color = Color.Lerp(DecayPurple, DeathGreen, chargeProgress);
                        var particle = new GenericGlowParticle(pos, vel, color * 0.6f, 0.25f + chargeProgress * 0.2f, 18, true);
                        MagnumParticleHandler.SpawnParticle(particle);
                    }
                }

                // Growing center glow
                CustomParticles.GenericFlare(player.Center, DecayPurple * chargeProgress * 0.4f, 0.25f + chargeProgress * 0.3f, 8);
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

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 0.4f;
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

            // Release VFX - layered bloom instead of halo
            CustomParticles.GenericFlare(player.Center, DeathGreen, 0.8f, 22);
            CustomParticles.GenericFlare(player.Center, DecayPurple * 0.7f, 0.65f, 18);
            CustomParticles.GenericFlare(player.Center, WitherBrown * 0.5f, 0.45f, 15);
            
            // Music note ring and burst for Autumn's Wrath
            ThemedParticles.MusicNoteRing(player.Center, AutumnOrange, 45f, 7);
            ThemedParticles.MusicNoteBurst(player.Center, new Color(139, 90, 43), 6, 5f);
            
            // Withering glyphs (Autumn decay theme)
            CustomParticles.GlyphBurst(player.Center, DecayPurple, 5, 4f);
            CustomParticles.GlyphBurst(player.Center, WitherBrown, 3, 2.5f);
            
            // Sparkle accents
            for (int sparkIdx = 0; sparkIdx < 6; sparkIdx++)
            {
                var sparkle = new SparkleParticle(player.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(3f, 3f), new Color(218, 165, 32) * 0.5f, 0.22f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Decay wisp burst
            for (int wisp = 0; wisp < 6; wisp++)
            {
                float wispAngle = MathHelper.TwoPi * wisp / 6f;
                Vector2 wispPos = player.Center + wispAngle.ToRotationVector2() * 22f;
                Color wispColor = Color.Lerp(DecayPurple, WitherBrown, (float)wisp / 6f);
                CustomParticles.GenericFlare(wispPos, wispColor * 0.7f, 0.28f, 14);
            }

            // Radial decay burst
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                Color burstColor = Color.Lerp(DecayPurple, DeathGreen, (float)i / 14f) * 0.6f;
                var burst = new GenericGlowParticle(player.Center, burstVel, burstColor, 0.32f, 24, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            player.statMana -= Item.mana * 2; // Extra mana cost for charged attack
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Only fire normal bolts if not charging
            if (isCharging && chargeTimer > 15) return false;

            // Muzzle VFX - layered bloom instead of halo
            CustomParticles.GenericFlare(position, DecayPurple, 0.45f, 15);
            CustomParticles.GenericFlare(position, DeathGreen * 0.4f, 0.3f, 12);
            
            // Music note on cast
            ThemedParticles.MusicNote(position, velocity * 0.1f, new Color(218, 165, 32) * 0.8f, 0.7f, 25);
            
            // Decay wisp burst
            for (int wisp = 0; wisp < 4; wisp++)
            {
                float wispAngle = MathHelper.TwoPi * wisp / 4f;
                Vector2 wispPos = position + wispAngle.ToRotationVector2() * 10f;
                CustomParticles.GenericFlare(wispPos, DeathGreen * 0.5f, 0.15f, 9);
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 burstVel = velocity.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(2f, 4f) + Main.rand.NextVector2Circular(2f, 2f);
                Color burstColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.45f;
                var burst = new GenericGlowParticle(position, burstVel, burstColor, 0.2f, 15, true);
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
