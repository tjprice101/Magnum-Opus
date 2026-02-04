using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.ClairDeLune.Projectiles;
using MagnumOpus.Content.ClairDeLune.ResonanceEnergies;
using MagnumOpus.Content.ClairDeLune.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common;

namespace MagnumOpus.Content.ClairDeLune.Tools
{
    /// <summary>
    /// Chronologist's Excavator - SUPREME FINAL BOSS pickaxe
    /// Must exceed Fate's Pickaxe (550 pick power)
    /// Features temporal particle effects with clockwork, crystals, and lightning.
    /// </summary>
    public class ChronologistsExcavator : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.LunarHamaxeNebula;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 280; // SUPREME (Fate: 220)
            Item.DamageType = DamageClass.Melee;
            Item.width = 48;
            Item.height = 48;
            Item.useTime = 2;
            Item.useAnimation = 3; // Slightly faster than Fate
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 10f;
            Item.value = Item.sellPrice(platinum: 1, gold: 20);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item15 with { Pitch = -0.3f, Volume = 0.9f };
            Item.autoReuse = true;
            Item.useTurn = true;

            // SUPREME pick power - exceeds Fate (550)
            Item.pick = 600;
            
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "PickPower", "600% pickaxe power") { OverrideColor = ClairDeLuneColors.Crystal });
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Mines blocks in a 3x3 area") { OverrideColor = ClairDeLuneColors.Brass });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "15% chance to duplicate ores mined") { OverrideColor = ClairDeLuneColors.GearGold });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The flow of time bends around its crystalline edge, revealing what was buried for eons'") 
            { 
                OverrideColor = Color.Lerp(ClairDeLuneColors.Crystal, ClairDeLuneColors.ElectricBlue, 0.5f) 
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 25)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 18)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 3)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Crystal shard particles
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.BlueCrystalShard, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, 
                    ClairDeLuneColors.Crystal, 1.4f);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
            }

            // Electric blue sparks
            if (Main.rand.NextBool(3))
            {
                var glow = new GenericGlowParticle(
                    new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height)),
                    Main.rand.NextVector2Circular(2f, 2f),
                    ClairDeLuneColors.ElectricBlue * 0.8f, 0.35f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Clockwork gear sparks
            if (Main.rand.NextBool(5))
            {
                Vector2 gearPos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                ClairDeLuneVFX.SpawnClockworkGear(gearPos, Main.rand.NextVector2Circular(1.5f, 1.5f), false, 0.3f);
            }

            // Brass dust
            if (Main.rand.NextBool(4))
            {
                Dust brassDust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Copper, 0f, 0f, 100, ClairDeLuneColors.Brass, 1.1f);
                brassDust.noGravity = true;
            }

            // Lightning spark (rare)
            if (Main.rand.NextBool(8))
            {
                CustomParticles.GenericFlare(
                    new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height)),
                    ClairDeLuneColors.LightningPurple, 0.3f, 12);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Temporal impact VFX
            ClairDeLuneVFX.TemporalImpact(target.Center, 0.5f);
            
            // Apply time-related debuffs
            if (Main.rand.NextBool(4))
            {
                target.AddBuff(BuffID.Slow, 120);
            }
        }

        // 3x3 mining area
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            hitbox.X -= 16;
            hitbox.Y -= 16;
            hitbox.Width += 32;
            hitbox.Height += 32;
        }
    }

    /// <summary>
    /// Temporal Cleaver - SUPREME FINAL BOSS axe
    /// Exceeds previous axe powers
    /// </summary>
    public class TemporalCleaver : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.LunarHamaxeSolar;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 260;
            Item.DamageType = DamageClass.Melee;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 3;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(platinum: 1);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item1 with { Pitch = -0.2f };
            Item.autoReuse = true;
            Item.useTurn = true;

            // SUPREME axe power
            Item.axe = 70; // Divide by 5 for actual axe power (350%)
            
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "AxePower", "350% axe power") { OverrideColor = ClairDeLuneColors.Brass });
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Trees fall instantly") { OverrideColor = ClairDeLuneColors.GearGold });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "10% chance to drop double wood") { OverrideColor = ClairDeLuneColors.Crystal });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each swing cleaves through centuries of growth in an instant'") 
            { 
                OverrideColor = ClairDeLuneColors.MoonlightSilver 
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 22)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 2)
                .AddIngredient(ItemID.LunarBar, 12)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Brass/gear particles
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Copper, player.velocity.X * 0.15f, player.velocity.Y * 0.15f, 100, 
                    ClairDeLuneColors.GearGold, 1.3f);
                dust.noGravity = true;
                dust.velocity *= 1.2f;
            }

            // Clockwork sparks
            if (Main.rand.NextBool(4))
            {
                var spark = new GenericGlowParticle(
                    new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height)),
                    Main.rand.NextVector2Circular(2f, 2f),
                    ClairDeLuneColors.Brass * 0.9f, 0.3f, 15, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            ClairDeLuneVFX.TemporalImpact(target.Center, 0.4f);
        }
    }

    /// <summary>
    /// Clockwork Excavation Drill - SUPREME FINAL BOSS combined drill
    /// Both pickaxe and axe in one
    /// </summary>
    public class ClockworkExcavationDrill : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.SolarFlareDrill;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 320;
            Item.DamageType = DamageClass.Melee;
            Item.width = 52;
            Item.height = 26;
            Item.useTime = 2;
            Item.useAnimation = 3;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item23;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<ClockworkDrillProjectile>();
            Item.shootSpeed = 32f;

            // SUPREME combined tool
            Item.pick = 580;
            Item.axe = 65; // 325%
            
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "PickPower", "580% pickaxe power") { OverrideColor = ClairDeLuneColors.Crystal });
            tooltips.Add(new TooltipLine(Mod, "AxePower", "325% axe power") { OverrideColor = ClairDeLuneColors.Brass });
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Channeled drilling with extreme speed") { OverrideColor = ClairDeLuneColors.GearGold });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Projects forward to mine at range") { OverrideColor = ClairDeLuneColors.ElectricBlue });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Gears spin through the fabric of reality, tearing apart stone and time alike'") 
            { 
                OverrideColor = Color.Lerp(ClairDeLuneColors.Brass, ClairDeLuneColors.LightningPurple, 0.4f) 
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 30)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 4)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddIngredient(ItemID.Cog, 50)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class ClockworkDrillProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SolarFlareDrill;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 38;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            if (!player.channel || player.dead || !player.active)
            {
                Projectile.Kill();
                return;
            }

            // Keep projectile in front of player
            Vector2 toMouse = Main.MouseWorld - player.Center;
            toMouse.Normalize();
            Projectile.Center = player.Center + toMouse * 45f;
            Projectile.rotation = toMouse.ToRotation() + MathHelper.PiOver4;
            
            // Visual effects while drilling
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 8, 8,
                    DustID.Copper, toMouse.X * 2f, toMouse.Y * 2f, 100, 
                    ClairDeLuneColors.Brass, 1.0f);
                dust.noGravity = true;
            }
            
            // Electric sparks
            if (Main.rand.NextBool(4))
            {
                var spark = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    toMouse * 2f + Main.rand.NextVector2Circular(1f, 1f),
                    ClairDeLuneColors.ElectricBlue * 0.7f, 0.25f, 12, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Clockwork gear sparks (rare)
            if (Main.rand.NextBool(12))
            {
                ClairDeLuneVFX.SpawnClockworkGear(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    toMouse * 1.5f, false, 0.25f);
            }

            // Player direction
            player.ChangeDir(Math.Sign(toMouse.X) != 0 ? Math.Sign(toMouse.X) : 1);
            Projectile.spriteDirection = player.direction;
            
            // Keep player animation locked
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ClairDeLuneVFX.TemporalImpact(target.Center, 0.3f);
            
            // Small lightning burst on hit
            if (Main.rand.NextBool(3))
            {
                ClairDeLuneVFX.LightningStrikeExplosion(target.Center, 0.3f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw with glow
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Glow layer
            Main.EntitySpriteDraw(texture, drawPos, null, ClairDeLuneColors.ElectricBlue * 0.3f, Projectile.rotation, origin, Projectile.scale * 1.15f, SpriteEffects.None, 0);
            // Main sprite
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            
            return false;
        }
    }

    /// <summary>
    /// Hammer of Broken Hours - SUPREME FINAL BOSS hammer
    /// Extreme hammer power
    /// </summary>
    public class HammerOfBrokenHours : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.LunarHamaxeStardust;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 300;
            Item.DamageType = DamageClass.Melee;
            Item.width = 54;
            Item.height = 54;
            Item.useTime = 4;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 15f;
            Item.value = Item.sellPrice(platinum: 1, gold: 10);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item1 with { Pitch = -0.4f, Volume = 1.1f };
            Item.autoReuse = true;
            Item.useTurn = true;

            // SUPREME hammer power
            Item.hammer = 200; // Extremely high

            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HammerPower", "200% hammer power") { OverrideColor = ClairDeLuneColors.Brass });
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Destroys walls in massive area") { OverrideColor = ClairDeLuneColors.GearGold });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Struck enemies are frozen in time briefly") { OverrideColor = ClairDeLuneColors.Crystal });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each impact shatters not just walls, but the very moments that held them together'") 
            { 
                OverrideColor = ClairDeLuneColors.MoonlightSilver 
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 22)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 2)
                .AddIngredient(ItemID.LunarBar, 12)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Heavy impact particles
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Smoke, 0f, 0f, 100, ClairDeLuneColors.DarkGray, 1.5f);
                dust.noGravity = false;
            }

            // Brass gear fragments
            if (Main.rand.NextBool(3))
            {
                var fragment = new GenericGlowParticle(
                    new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height)),
                    new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-2f, 1f)),
                    ClairDeLuneColors.Brass * 0.85f, 0.35f, 20, false);
                MagnumParticleHandler.SpawnParticle(fragment);
            }

            // Crystal shatter sparks
            if (Main.rand.NextBool(5))
            {
                CustomParticles.GenericFlare(
                    new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height)),
                    ClairDeLuneColors.Crystal, 0.35f, 14);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Massive impact VFX
            ClairDeLuneVFX.TemporalImpact(target.Center, 0.7f);
            ClairDeLuneVFX.ClockworkGearCascade(target.Center, 8, 5f, 0.5f);
            
            // Freeze enemy briefly
            target.AddBuff(BuffID.Slow, 90);
            target.AddBuff(BuffID.Frostburn2, 120);
            
            // Screen shake on hit
            if (Main.netMode != NetmodeID.Server)
            {
                MagnumScreenEffects.AddScreenShake(4f);
            }
        }

        // Larger hitbox for wall destruction
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            hitbox.X -= 24;
            hitbox.Y -= 24;
            hitbox.Width += 48;
            hitbox.Height += 48;
        }
    }
}
