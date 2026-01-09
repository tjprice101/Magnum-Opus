using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.Projectiles;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Weapons
{
    /// <summary>
    /// Eternal Moon - A heavy sword that sends out waves of purple energy when swung.
    /// Fires multiple projectiles and occasionally a massive beam.
    /// Applies Musical Dissonance debuff on hit.
    /// </summary>
    public class EternalMoon : ModItem
    {
        private int swingCounter = 0;

        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.damage = 300; // Balanced: ~1000 DPS (300 × 60/18)
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 18; // Faster swing
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7f;
            Item.value = Item.buyPrice(gold: 25);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<EternalMoonWave>();
            Item.shootSpeed = 14f;
            Item.maxStack = 1;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Purple particle trail when swinging
            if (Main.rand.NextBool(3))
            {
                Vector2 hitCenter = new Vector2(hitbox.X + hitbox.Width / 2, hitbox.Y + hitbox.Height / 2);
                ThemedParticles.MoonlightTrail(hitCenter, player.velocity * 0.3f);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Musical Dissonance debuff
            target.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 180); // 3 seconds
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            swingCounter++;
            
            // Fire 3 waves in a spread pattern
            float spreadAngle = MathHelper.ToRadians(15f);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 spreadVel = velocity.RotatedBy(spreadAngle * i);
                Projectile.NewProjectile(source, position, spreadVel, type, damage, knockback, player.whoAmI);
            }
            
            // Every 4th swing, fire 3 spinning star projectiles in quick succession
            if (swingCounter >= 4)
            {
                swingCounter = 0;
                
                // Fire 3 stars back-to-back with slight delay using ai[1] as spawn delay
                Vector2 beamVel = velocity.SafeNormalize(Vector2.UnitX) * 16f;
                for (int i = 0; i < 3; i++)
                {
                    // Slight spread and position offset for visual interest
                    float spreadOffset = MathHelper.ToRadians(5f * (i - 1)); // -5, 0, +5 degrees
                    Vector2 starVel = beamVel.RotatedBy(spreadOffset);
                    Vector2 startPos = position + velocity.SafeNormalize(Vector2.Zero) * (i * 8); // Stagger starting positions
                    
                    Projectile proj = Projectile.NewProjectileDirect(source, startPos, starVel, 
                        ModContent.ProjectileType<EternalMoonBeam>(), (int)(damage * 1.5f), knockback * 2f, player.whoAmI);
                    
                    // Use ai[1] to delay each star's movement slightly (they'll catch up visually)
                    proj.ai[1] = i * 3; // Frame delay for staggered launch effect
                }
                
                // Visual and audio feedback for stars
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122, position);
                
                // Burst of particles
                ThemedParticles.MoonlightBloomBurst(position, 1.5f);
                ThemedParticles.MoonlightSparkles(position, 10, 25f);
                
                // Musical notes burst!
                ThemedParticles.MoonlightMusicNotes(position, 8, 30f);
                ThemedParticles.MoonlightClef(position, Main.rand.NextBool(), 1.2f);
            }
            
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 30)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 5)
                .AddIngredient(ModContent.ItemType<Enemies.ShardsOfMoonlitTempo>(), 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
