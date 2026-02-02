using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.Spring.Weapons;
using MagnumOpus.Content.Summer.Weapons;
using MagnumOpus.Content.Autumn.Weapons;
using MagnumOpus.Content.Winter.Weapons;

namespace MagnumOpus.Content.Seasons.Weapons
{
    /// <summary>
    /// Concerto of Seasons - Ultimate Vivaldi Magic Weapon
    /// Post-Moon Lord magic tome that cycles through seasonal spells
    /// 
    /// MECHANICS:
    /// - Left Click: Seasonal Spell - Cycles through Spring ↁESummer ↁEAutumn ↁEWinter spells
    /// - Right Click: Grand Crescendo - Channels all four seasons simultaneously for massive damage
    /// - Each season has a unique spell pattern and debuff combination
    /// 
    /// SEASONAL SPELLS:
    /// - Spring Verse: Blooming petal storm with homing petals
    /// - Summer Movement: Solar flare barrage with burning pillars
    /// - Autumn Passage: Decaying orbs that drain life
    /// - Winter Finale: Blizzard burst with freeze chance
    /// 
    /// CRAFTING: All 4 seasonal bars + all 4 resonant energies + Nebula Fragments @ Lunar Crafting Station
    /// </summary>
    public class ConcertoOfSeasons : ModItem
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SummerGold = new Color(255, 215, 0);
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color WinterBlue = new Color(150, 220, 255);

        private int seasonIndex = 0;
        private int chargeTime = 0;
        private bool isCharging = false;

        public override void SetDefaults()
        {
            Item.damage = 220;
            Item.DamageType = DamageClass.Magic;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 35);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.SpringVerseProjectile>();
            Item.shootSpeed = 16f;
            Item.mana = 14;
            Item.channel = true;
            Item.noUseGraphic = false;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Right click - Grand Crescendo (channeled)
                Item.useTime = 10;
                Item.useAnimation = 10;
                Item.mana = 8;
            }
            else
            {
                // Left click - Seasonal Spell
                Item.useTime = 18;
                Item.useAnimation = 18;
                Item.mana = 14;
            }

            return base.CanUseItem(player);
        }

        public override void HoldItem(Player player)
        {
            // Handle channeling for Grand Crescendo
            if (player.altFunctionUse == 2 && player.channel && player.CheckMana(Item, -1, true))
            {
                if (!isCharging)
                {
                    isCharging = true;
                    chargeTime = 0;
                }

                chargeTime++;

                // Charging VFX - all seasons converge with music notes
                if (chargeTime % 5 == 0)
                {
                    float angle = Main.GameUpdateCount * 0.05f;
                    for (int i = 0; i < 4; i++)
                    {
                        float seasonAngle = angle + MathHelper.PiOver2 * i;
                        float radius = 60f - (chargeTime % 60) * 0.5f;
                        Vector2 particlePos = player.Center + seasonAngle.ToRotationVector2() * radius;
                        Color seasonColor = i switch { 0 => SpringPink, 1 => SummerGold, 2 => AutumnOrange, _ => WinterBlue };
                        var particle = new GenericGlowParticle(particlePos, (player.Center - particlePos).SafeNormalize(Vector2.Zero) * 2f, seasonColor * 0.5f, 0.28f, 18, true);
                        MagnumParticleHandler.SpawnParticle(particle);
                        
                        // ☁ESPARKLE at charge points
                        var sparkle = new SparkleParticle(particlePos, Vector2.Zero, seasonColor * 0.6f, 0.22f, 12);
                        MagnumParticleHandler.SpawnParticle(sparkle);
                    }
                }
                
                // ☁EMUSICAL NOTATION - Notes spiral during charge! - VISIBLE SCALE 0.72f+
                if (chargeTime % 12 == 0)
                {
                    float noteAngle = chargeTime * 0.1f;
                    Color noteColor = ((chargeTime / 15) % 4) switch { 0 => SpringPink, 1 => SummerGold, 2 => AutumnOrange, _ => WinterBlue };
                    Vector2 notePos = player.Center + noteAngle.ToRotationVector2() * 35f;
                    Vector2 noteVel = new Vector2(0, -1.5f);
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.72f, 30);
                }

                // Release Grand Crescendo at full charge
                if (chargeTime >= 60 && chargeTime % 15 == 0 && Main.myPlayer == player.whoAmI)
                {
                    Vector2 mousePos = Main.MouseWorld;
                    Vector2 direction = (mousePos - player.Center).SafeNormalize(Vector2.Zero);

                    // Fire all four seasons simultaneously
                    for (int season = 0; season < 4; season++)
                    {
                        float spread = MathHelper.ToRadians(20f * (season - 1.5f));
                        Vector2 velocity = direction.RotatedBy(spread) * 18f;

                        int projType = season switch
                        {
                            0 => ModContent.ProjectileType<Projectiles.SpringVerseProjectile>(),
                            1 => ModContent.ProjectileType<Projectiles.SummerMovementProjectile>(),
                            2 => ModContent.ProjectileType<Projectiles.AutumnPassageProjectile>(),
                            _ => ModContent.ProjectileType<Projectiles.WinterFinaleProjectile>()
                        };

                        Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, velocity, projType,
                            (int)(Item.damage * player.GetDamage(DamageClass.Magic).Additive * 1.5f), Item.knockBack, player.whoAmI);
                    }

                    // Crescendo VFX burst with music notes
                    CustomParticles.GenericFlare(player.Center, Color.White, 0.9f, 22);
                    CustomParticles.HaloRing(player.Center, Color.White * 0.5f, 0.45f, 18);
                    
                    // Musical crescendo notes - visible scale 0.72f+
                    for (int n = 0; n < 4; n++)
                    {
                        float noteAngle = MathHelper.TwoPi * n / 4f;
                        Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 3.5f);
                        Color noteColor = n switch { 0 => SpringPink, 1 => SummerGold, 2 => AutumnOrange, _ => WinterBlue };
                        ThemedParticles.MusicNote(player.Center, noteVel, noteColor, 0.72f, 30);
                    }
                    
                    for (int i = 0; i < 4; i++)
                    {
                        Color burstColor = i switch { 0 => SpringPink, 1 => SummerGold, 2 => AutumnOrange, _ => WinterBlue };
                        float burstAngle = MathHelper.PiOver2 * i + Main.rand.NextFloat(-0.2f, 0.2f);
                        CustomParticles.GenericFlare(player.Center + burstAngle.ToRotationVector2() * 20f, burstColor, 0.5f, 15);
                    }

                    SoundEngine.PlaySound(SoundID.Item29, player.Center);
                }

                Lighting.AddLight(player.Center, Color.Lerp(SpringPink, WinterBlue, (chargeTime % 60) / 60f).ToVector3() * 0.5f);
            }
            else
            {
                isCharging = false;
                chargeTime = 0;
            }

            // Ambient seasonal particles when held
            if (Main.rand.NextBool(18))
            {
                Color seasonColor = seasonIndex switch { 0 => SpringPink, 1 => SummerGold, 2 => AutumnOrange, _ => WinterBlue };
                Vector2 particlePos = player.Center + new Vector2(player.direction * 18f, -6f) + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 particleVel = new Vector2(0, Main.rand.NextFloat(-1.5f, -0.4f));
                var particle = new GenericGlowParticle(particlePos, particleVel, seasonColor * 0.3f, 0.18f, 16, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            // Musical notation - visible scale 0.7f+
            if (Main.rand.NextBool(25))
            {
                Color noteColor = seasonIndex switch { 0 => SpringPink, 1 => SummerGold, 2 => AutumnOrange, _ => WinterBlue };
                Vector2 notePos = player.Center + new Vector2(player.direction * 22f, -8f) + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 noteVel = new Vector2(0, -0.8f);
                ThemedParticles.MusicNote(notePos, noteVel, noteColor * 0.8f, 0.7f, 24);
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Right click is handled in HoldItem for channeling
                return false;
            }

            // Left click - Seasonal Spell
            int projType = seasonIndex switch
            {
                0 => ModContent.ProjectileType<Projectiles.SpringVerseProjectile>(),
                1 => ModContent.ProjectileType<Projectiles.SummerMovementProjectile>(),
                2 => ModContent.ProjectileType<Projectiles.AutumnPassageProjectile>(),
                _ => ModContent.ProjectileType<Projectiles.WinterFinaleProjectile>()
            };

            Color seasonColor = seasonIndex switch { 0 => SpringPink, 1 => SummerGold, 2 => AutumnOrange, _ => WinterBlue };

            // Fire main projectile
            Projectile.NewProjectile(source, position, velocity, projType, damage, knockback, player.whoAmI);

            // Additional projectiles based on season
            if (seasonIndex == 0) // Spring - Spread petals
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 spreadVel = velocity.RotatedBy(MathHelper.ToRadians(15f * (i - 1)));
                    Projectile.NewProjectile(source, position, spreadVel * 0.9f, projType, damage / 2, knockback * 0.5f, player.whoAmI);
                }
            }
            else if (seasonIndex == 1) // Summer - Piercing rays
            {
                Vector2 ray1 = velocity.RotatedBy(MathHelper.ToRadians(-10f));
                Vector2 ray2 = velocity.RotatedBy(MathHelper.ToRadians(10f));
                Projectile.NewProjectile(source, position, ray1, projType, (int)(damage * 0.7f), knockback * 0.7f, player.whoAmI);
                Projectile.NewProjectile(source, position, ray2, projType, (int)(damage * 0.7f), knockback * 0.7f, player.whoAmI);
            }

            // Muzzle flash with layered VFX
            CustomParticles.GenericFlare(position, Color.White, 0.7f, 20);
            CustomParticles.GenericFlare(position, seasonColor, 0.6f, 18);
            CustomParticles.HaloRing(position, seasonColor * 0.6f, 0.4f, 16);

            // ☁EMUSICAL NOTATION - Notes on cast! - VISIBLE SCALE 0.7f+
            for (int n = 0; n < 2; n++)
            {
                float noteAngle = velocity.ToRotation() + MathHelper.ToRadians(Main.rand.NextFloat(-40f, 40f));
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                ThemedParticles.MusicNote(position, noteVel, seasonColor, 0.7f, 30);
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.8f) * Main.rand.NextFloat(3f, 6f);
                var burst = new GenericGlowParticle(position, burstVel, seasonColor * 0.5f, 0.25f, 16, true);
                MagnumParticleHandler.SpawnParticle(burst);
                
                // ☁ESPARKLE accents
                if (i % 2 == 0)
                {
                    var sparkle = new SparkleParticle(position, burstVel * 0.6f, seasonColor * 0.6f, 0.2f, 14);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }

            SoundEngine.PlaySound(SoundID.Item43, position);

            // Cycle season
            seasonIndex = (seasonIndex + 1) % 4;

            return false;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            Color seasonColor = seasonIndex switch { 0 => SpringPink, 1 => SummerGold, 2 => AutumnOrange, _ => WinterBlue };
            string seasonName = seasonIndex switch { 0 => "Spring", 1 => "Summer", 2 => "Autumn", _ => "Winter" };

            tooltips.Add(new TooltipLine(Mod, "Season", $"Current Season: {seasonName}") { OverrideColor = seasonColor });
            tooltips.Add(new TooltipLine(Mod, "LeftClick", "Left click cycles through seasonal spells"));
            tooltips.Add(new TooltipLine(Mod, "RightClick", "Hold right click to channel Grand Crescendo - fires all four seasons"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The four movements of nature, conducted as one symphony'") { OverrideColor = new Color(180, 150, 255) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                // Combine the 4 lower-tier seasonal magic weapons
                .AddIngredient(ModContent.ItemType<VernalScepter>(), 1)
                .AddIngredient(ModContent.ItemType<SolsticeTome>(), 1)
                .AddIngredient(ModContent.ItemType<WitheringGrimoire>(), 1)
                .AddIngredient(ModContent.ItemType<PermafrostCodex>(), 1)
                // Plus 10 of each Seasonal Resonant Energy
                .AddIngredient(ModContent.ItemType<SpringResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<SummerResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<WinterResonantEnergy>(), 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
