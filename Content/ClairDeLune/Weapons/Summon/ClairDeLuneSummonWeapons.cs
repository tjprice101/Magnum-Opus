using MagnumOpus.Common.Systems;
using MagnumOpus.Common;
using MagnumOpus.Content.ClairDeLune.HarmonicCores;
using MagnumOpus.Content.ClairDeLune.Projectiles;
using MagnumOpus.Content.ClairDeLune.ResonanceEnergies;
using MagnumOpus.Content.ClairDeLune.Weapons.Summon;
using MagnumOpus.Content.Fate.CraftingStations;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Summon
{
    public class LunarPhylactery : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 18;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<LunarPhylacteryMinionProjectile>();
            Item.buffType = ModContent.BuffType<LunarPhylacteryBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 26)
            .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 20)
            .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 3)
            .AddIngredient(ItemID.LunarBar, 20)
            .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
            .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a crystalline soul vessel to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "Fires temporal beams that pierce multiple enemies"));
            tooltips.Add(new TooltipLine(Mod, "Mechanic", "Killing enemies empowers the phylactery (+5% damage per soul, max 50%)"));
            tooltips.Add(new TooltipLine(Mod, "Synergy", "Multiple phylacteries share souls for massive damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A vessel for souls lost to time'")
            {
                OverrideColor = ClairDeLuneColors.Crystal
            });
        }
    }

    public class LunarPhylacteryBuff : ModBuff
    {
        // Preserved separately
    }

    public class LunarPhylacteryMinionProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 42;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            Projectile.timeLeft = 2;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead) { Projectile.Kill(); return; }
            Projectile.timeLeft = 2;
        }

        public override bool PreDraw(ref Color lightColor) => false;

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;
    }

    public class PhylacteryBeamProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 80;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 2;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 120);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class GearDrivenArbiter : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 22;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<GearDrivenArbiterMinionProjectile>();
            Item.buffType = ModContent.BuffType<GearDrivenArbiterBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 28)
            .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 22)
            .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 4)
            .AddIngredient(ItemID.LunarBar, 22)
            .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
            .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a clockwork arbiter to judge your foes"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "Fires gear projectiles that mark enemies"));
            tooltips.Add(new TooltipLine(Mod, "Mechanic", "Marked enemies take +15% damage from all sources"));
            tooltips.Add(new TooltipLine(Mod, "Synergy", "Multiple arbiters spread marks faster"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The gears of justice turn without mercy'")
            {
                OverrideColor = ClairDeLuneColors.Brass
            });
        }
    }

    public class GearDrivenArbiterBuff : ModBuff
    {
        // Preserved separately
    }

    public class GearDrivenArbiterMinionProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            Projectile.timeLeft = 2;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead) { Projectile.Kill(); return; }
            Projectile.timeLeft = 2;
        }

        public override bool PreDraw(ref Color lightColor) => false;

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;
    }

    public class ArbiterGearProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<TemporalJudgmentDebuff>(), 300);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class TemporalJudgmentDebuff : ModBuff
    {
        // Preserved separately
    }

    public class TemporalJudgmentGlobalNPC : GlobalNPC
    {
        // Gutted
    }

    public class AutomatonsTuningFork : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 32;
            Item.useAnimation = 32;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<AutomatonsTuningForkMinionProjectile>();
            Item.buffType = ModContent.BuffType<AutomatonsTuningForkBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 24)
            .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 18)
            .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 3)
            .AddIngredient(ItemID.LunarBar, 18)
            .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
            .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a resonating automaton to harmonize your summons"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "Pulses with temporal energy, damaging nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Mechanic", "Creates a resonance field - all summons within deal +25% damage"));
            tooltips.Add(new TooltipLine(Mod, "Synergy", "Multiple automatons stack resonance up to +50% bonus"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The perfect pitch of destruction'")
            {
                OverrideColor = ClairDeLuneColors.MoonlightSilver
            });
        }
    }

    public class AutomatonsTuningForkBuff : ModBuff
    {
        // Preserved separately
    }

    public class AutomatonsTuningForkMinionProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 44;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            Projectile.timeLeft = 2;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead) { Projectile.Kill(); return; }
            Projectile.timeLeft = 2;
        }

        public override bool PreDraw(ref Color lightColor) => false;

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;
    }

    public class ResonanceFieldGlobalProjectile : GlobalProjectile
    {
        // Gutted
    }
}