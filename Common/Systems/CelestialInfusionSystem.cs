using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Audio;
using Terraria.UI;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Manages the Celestial Weapon Infusion system.
    /// Weapons can be infused with the Seed of Universal Melodies to transform
    /// them into Celestial variants with new attacks and enhanced stats.
    /// </summary>
    public class CelestialInfusionSystem : ModSystem
    {
        // Static registry of weapons that can be infused
        public static HashSet<int> InfusableWeapons { get; private set; } = new HashSet<int>();
        
        // Registry of transformed weapon types (base type -> celestial type)
        public static Dictionary<int, int> CelestialTransformations { get; private set; } = new Dictionary<int, int>();

        public override void PostSetupContent()
        {
            // Register all Magnum Opus weapons as infusable
            // This will be populated when celestial variants are created
            RegisterInfusableWeapons();
        }

        private void RegisterInfusableWeapons()
        {
            // This method will be called to register weapons
            // For now, it sets up the infrastructure
            // Individual weapon mods will register themselves
        }

        /// <summary>
        /// Registers a weapon as infusable with the Seed of Universal Melodies
        /// </summary>
        public static void RegisterInfusableWeapon(int baseWeaponType, int celestialWeaponType = -1)
        {
            InfusableWeapons.Add(baseWeaponType);
            
            if (celestialWeaponType > 0)
            {
                CelestialTransformations[baseWeaponType] = celestialWeaponType;
            }
        }

        /// <summary>
        /// Checks if a weapon can be infused
        /// </summary>
        public static bool CanBeInfused(Item item)
        {
            if (item == null || item.IsAir) return false;
            
            // Check if weapon is from this mod and is a weapon
            if (item.ModItem == null || item.ModItem.Mod.Name != "MagnumOpus") return false;
            if (!item.DamageType.CountsAsClass(DamageClass.Generic) && item.damage <= 0) return false;
            
            // Check if already infused
            if (item.TryGetGlobalItem<CelestialInfusionGlobalItem>(out var globalItem))
            {
                if (globalItem.IsCelestialInfused) return false;
            }
            
            return true;
        }

        /// <summary>
        /// Attempts to infuse a weapon with celestial power
        /// </summary>
        public static bool TryInfuseWeapon(Item weapon, Item seed, Player player)
        {
            if (!CanBeInfused(weapon)) return false;
            if (seed.type != ModContent.ItemType<Content.Items.SeedOfUniversalMelodies>()) return false;
            
            // Apply infusion
            if (weapon.TryGetGlobalItem<CelestialInfusionGlobalItem>(out var globalItem))
            {
                globalItem.IsCelestialInfused = true;
                globalItem.InfusionTime = Main.GameUpdateCount;
                
                // Apply stat boosts
                ApplyCelestialBoosts(weapon);
                
                // Visual and audio feedback
                CreateInfusionEffects(player.Center);
                
                // Consume one seed
                seed.stack--;
                if (seed.stack <= 0)
                    seed.TurnToAir();
                
                return true;
            }
            
            return false;
        }

        private static void ApplyCelestialBoosts(Item weapon)
        {
            // Base stat improvements for celestial weapons
            // +15% damage, +10% speed, +10% crit
            // These are applied through the global item
        }

        private static void CreateInfusionEffects(Vector2 position)
        {
            SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.5f }, position);
            SoundEngine.PlaySound(SoundID.Item29, position);
            
            // Musical note explosion
            for (int i = 0; i < 50; i++)
            {
                int dustType = Main.rand.Next(5) switch
                {
                    0 => DustID.GoldFlame,
                    1 => DustID.PurpleTorch,
                    2 => DustID.IceTorch,
                    3 => DustID.PinkTorch,
                    _ => DustID.SilverCoin
                };
                
                Vector2 vel = Main.rand.NextVector2Circular(10f, 10f);
                Dust note = Dust.NewDustDirect(position, 1, 1, dustType, vel.X, vel.Y, 100, default, 1.5f);
                note.noGravity = true;
            }
            
            // Clockwork gears
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Dust gear = Dust.NewDustDirect(position, 1, 1, DustID.Copper, vel.X, vel.Y, 100, default, 1.2f);
                gear.noGravity = true;
            }
            
            // Lightning sparks
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(12f, 12f);
                Dust spark = Dust.NewDustDirect(position, 1, 1, DustID.Electric, vel.X, vel.Y, 0, default, 1f);
                spark.noGravity = true;
            }
        }
    }

    /// <summary>
    /// Global item that tracks celestial infusion state for weapons
    /// </summary>
    public class CelestialInfusionGlobalItem : GlobalItem
    {
        public bool IsCelestialInfused { get; set; } = false;
        public uint InfusionTime { get; set; } = 0;

        public override bool InstancePerEntity => true;

        public override GlobalItem Clone(Item from, Item to)
        {
            var clone = (CelestialInfusionGlobalItem)base.Clone(from, to);
            clone.IsCelestialInfused = IsCelestialInfused;
            clone.InfusionTime = InfusionTime;
            return clone;
        }

        public override void SaveData(Item item, TagCompound tag)
        {
            if (IsCelestialInfused)
            {
                tag["CelestialInfused"] = true;
                tag["InfusionTime"] = InfusionTime;
            }
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            IsCelestialInfused = tag.GetBool("CelestialInfused");
            InfusionTime = tag.Get<uint>("InfusionTime");
        }

        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            // Only apply to weapons from this mod
            return entity.ModItem != null && entity.ModItem.Mod.Name == "MagnumOpus" && entity.damage > 0;
        }

        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            if (IsCelestialInfused)
            {
                // +15% damage for celestial weapons
                damage *= 1.15f;
            }
        }

        public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
        {
            if (IsCelestialInfused)
            {
                // +10% crit for celestial weapons
                crit += 10f;
            }
        }

        public override float UseSpeedMultiplier(Item item, Player player)
        {
            if (IsCelestialInfused)
            {
                // +10% attack speed for celestial weapons
                return 1.1f;
            }
            return 1f;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (IsCelestialInfused)
            {
                // Add celestial indicator to tooltip
                int index = tooltips.FindIndex(t => t.Name == "ItemName");
                if (index >= 0)
                {
                    tooltips[index].Text = "[c/FFD700:✦] " + tooltips[index].Text + " [c/FFD700:✦]";
                }
                
                // Add celestial bonus info
                tooltips.Add(new TooltipLine(item.ModItem.Mod, "CelestialBonus", "[c/00FFFF:Celestially Infused]"));
                tooltips.Add(new TooltipLine(item.ModItem.Mod, "CelestialDamage", "[c/FF6600:+15% damage]"));
                tooltips.Add(new TooltipLine(item.ModItem.Mod, "CelestialSpeed", "[c/FFD700:+10% attack speed]"));
                tooltips.Add(new TooltipLine(item.ModItem.Mod, "CelestialCrit", "[c/FF00FF:+10% critical strike chance]"));
            }
        }

        public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (IsCelestialInfused)
            {
                // Draw celestial glow behind item in inventory
                Texture2D texture = Terraria.GameContent.TextureAssets.Item[item.type].Value;
                
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.2f + 0.8f;
                
                // Rainbow glow
                for (int i = 0; i < 4; i++)
                {
                    float angle = Main.GameUpdateCount * 0.03f + i * MathHelper.PiOver2;
                    Color glowColor = Main.hslToRgb((i / 4f + Main.GameUpdateCount * 0.01f) % 1f, 1f, 0.5f) * 0.3f * pulse;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 2f;
                    
                    spriteBatch.Draw(texture, position + offset, frame, glowColor, 0f, origin, scale, SpriteEffects.None, 0f);
                }
            }
            return true;
        }

        public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (IsCelestialInfused)
            {
                Texture2D texture = Terraria.GameContent.TextureAssets.Item[item.type].Value;
                Vector2 drawPos = item.position - Main.screenPosition + new Vector2(item.width / 2f, item.height / 2f);
                
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f + 0.85f;
                
                // Rainbow glow in world
                for (int i = 0; i < 4; i++)
                {
                    float angle = Main.GameUpdateCount * 0.02f + i * MathHelper.PiOver2;
                    Color glowColor = Main.hslToRgb((i / 4f + Main.GameUpdateCount * 0.008f) % 1f, 1f, 0.6f) * 0.4f;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f;
                    
                    spriteBatch.Draw(texture, drawPos + offset, null, glowColor, rotation, texture.Size() / 2f, scale * pulse, SpriteEffects.None, 0f);
                }
                
                // Occasional particle effects
                if (Main.rand.NextBool(30))
                {
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.PurpleTorch;
                    Dust spark = Dust.NewDustDirect(item.position, item.width, item.height, dustType, 0f, -1f, 100, default, 0.8f);
                    spark.noGravity = true;
                }
            }
            return true;
        }

        public override void PostUpdate(Item item)
        {
            if (IsCelestialInfused)
            {
                // Celestial weapons glow and emit particles when dropped
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.3f + 0.7f;
                Lighting.AddLight(item.Center, 0.5f * pulse, 0.4f * pulse, 0.6f * pulse);
            }
        }
    }

    /// <summary>
    /// Player class for handling celestial weapon interactions
    /// </summary>
    public class CelestialInfusionPlayer : ModPlayer
    {
        // Track if player has infused a weapon this session (for achievements, etc.)
        public int WeaponsInfusedThisSession { get; set; } = 0;

        public override void PostUpdate()
        {
            // Check for right-click infusion interaction
            // This would be handled through UI or item use
        }
    }
}
