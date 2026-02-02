using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using ReLogic.Content;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common;

namespace MagnumOpus.Content.DiesIrae.Tools
{
    /// <summary>
    /// Wings of Damnation - Dies Irae wings, the POST-NACHTMUSIK ULTIMATE tier wings.
    /// Features infernal fire trails, ember particles, and devastating flight.
    /// Superior to Nachtmusik's Serenade of Stars wings.
    /// Single-frame wings (no animation).
    /// </summary>
    [AutoloadEquip(EquipType.Wings)]
    public class WingsOfDamnation : ModItem
    {
        // Dies Irae colors
        private static readonly Color BloodRed = new Color(139, 0, 0);
        private static readonly Color EmberOrange = new Color(255, 69, 0);
        private static readonly Color CharredBlack = new Color(25, 20, 15);
        private static readonly Color Crimson = new Color(200, 30, 30);
        private static readonly Color HellfireGold = new Color(255, 180, 50);

        public override void SetStaticDefaults()
        {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(
                flyTime: 560, // POST-NACHTMUSIK ULTIMATE (higher than Nachtmusik's 480)
                flySpeedOverride: 28f, // POST-NACHTMUSIK ULTIMATE (higher than Nachtmusik's 24)
                accelerationMultiplier: 5.8f, // POST-NACHTMUSIK ULTIMATE (higher than Nachtmusik's 5.0)
                hasHoldDownHoverFeatures: true
            );
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 18);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FlightTime", "9.3 second flight time") { OverrideColor = HellfireGold });
            tooltips.Add(new TooltipLine(Mod, "FlightSpeed", "28 mph flight speed") { OverrideColor = EmberOrange });
            tooltips.Add(new TooltipLine(Mod, "Hover", "Allows hovering") { OverrideColor = Crimson });
            tooltips.Add(new TooltipLine(Mod, "DodgeEffect", "Double-tap to perform an infernal dash that damages enemies") { OverrideColor = BloodRed });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Wings forged in the flames of final judgment, carrying the condemned to their eternal fate'") { OverrideColor = Color.Lerp(BloodRed, EmberOrange, 0.5f) });
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
            ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling = 1.8f; // Higher than Nachtmusik
            ascentWhenRising = 0.36f;
            maxCanAscendMultiplier = 2.0f;
            maxAscentMultiplier = 5.8f;
            constantAscend = 0.32f;
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<WingsOfDamnationPlayer>().hasWingsEquipped = true;
            
            // Infernal fire particles when flying
            if (!hideVisual && player.velocity.Y != 0)
            {
                // Blood-red to orange gradient flames
                if (Main.rand.NextBool(2))
                {
                    float progress = Main.rand.NextFloat();
                    Color flameColor = Color.Lerp(BloodRed, EmberOrange, progress);
                    
                    var glow = new GenericGlowParticle(
                        player.Center + Main.rand.NextVector2Circular(25f, 20f),
                        new Vector2(player.velocity.X * -0.05f, -2f),
                        flameColor * 0.8f, 0.45f, 30, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                
                // Ember sparks
                if (Main.rand.NextBool(3))
                {
                    Vector2 emberPos = player.Center + Main.rand.NextVector2Circular(30f, 25f);
                    CustomParticles.GenericFlare(emberPos, HellfireGold * 0.7f, 0.3f, 18);
                }
                
                // Heavy smoke trailing behind
                if (Main.rand.NextBool(3))
                {
                    var smoke = new HeavySmokeParticle(
                        player.Center + Main.rand.NextVector2Circular(20f, 15f),
                        new Vector2(player.velocity.X * -0.1f, Main.rand.NextFloat(0.5f, 1.5f)),
                        CharredBlack * 0.6f,
                        Main.rand.Next(35, 55), 0.35f, 0.5f, 0.018f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
                
                // Crimson fire wisps
                if (Main.rand.NextBool(4))
                {
                    Vector2 wispPos = player.Center + Main.rand.NextVector2Circular(35f, 30f);
                    var wisp = new GenericGlowParticle(wispPos, 
                        new Vector2(player.velocity.X * -0.08f, Main.rand.NextFloat(-0.5f, 0.5f)),
                        Crimson * 0.6f, 0.25f, 35, true);
                    MagnumParticleHandler.SpawnParticle(wisp);
                }
                
                // Music notes with fire effect
                if (Main.rand.NextBool(8))
                {
                    Vector2 notePos = player.Center + new Vector2(Main.rand.NextFloat(-20f, 20f), 15f);
                    ThemedParticles.MusicNote(notePos, new Vector2(player.velocity.X * -0.1f, -0.5f), EmberOrange * 0.7f, 0.6f, 28);
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 30)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 3)
                .AddIngredient(ItemID.SoulofFlight, 80)
                .AddIngredient(ItemID.LunarBar, 25)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class WingsOfDamnationPlayer : ModPlayer
    {
        // Dies Irae colors
        private static readonly Color BloodRed = new Color(139, 0, 0);
        private static readonly Color EmberOrange = new Color(255, 69, 0);
        private static readonly Color CharredBlack = new Color(25, 20, 15);
        private static readonly Color Crimson = new Color(200, 30, 30);
        private static readonly Color HellfireGold = new Color(255, 180, 50);

        public bool hasWingsEquipped = false;
        private int dashCooldown = 0;
        private int doubleTapTimer = 0;
        private int lastDirection = 0;

        public override void ResetEffects()
        {
            hasWingsEquipped = false;
        }

        public override void PreUpdate()
        {
            if (dashCooldown > 0)
                dashCooldown--;

            // Double-tap dash detection
            if (hasWingsEquipped && dashCooldown <= 0)
            {
                int direction = 0;
                if (Player.controlLeft && !Player.controlRight)
                    direction = -1;
                else if (Player.controlRight && !Player.controlLeft)
                    direction = 1;

                if (direction != 0)
                {
                    if (doubleTapTimer > 0 && direction == lastDirection)
                    {
                        // Perform infernal dash
                        PerformInfernalDash(direction);
                        doubleTapTimer = 0;
                        dashCooldown = 40; // Shorter cooldown than Nachtmusik (45)
                    }
                    else if (direction != lastDirection || doubleTapTimer <= 0)
                    {
                        doubleTapTimer = 15; // Window for double-tap
                        lastDirection = direction;
                    }
                }

                if (doubleTapTimer > 0)
                    doubleTapTimer--;
            }
        }

        private void PerformInfernalDash(int direction)
        {
            // Dash velocity - faster than Nachtmusik
            Player.velocity.X = direction * 32f;
            
            // Sound effect - deeper, more menacing
            SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.3f }, Player.Center);
            
            // Damage enemies in path
            Rectangle dashHitbox = new Rectangle(
                (int)Player.position.X - 50,
                (int)Player.position.Y - 30,
                Player.width + 100,
                Player.height + 60);
            
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.Hitbox.Intersects(dashHitbox))
                {
                    Player.ApplyDamageToNPC(npc, 500, 8f, direction, false);
                }
            }
            
            // VFX burst - more intense than Nachtmusik
            for (int i = 0; i < 35; i++)
            {
                float progress = i / 35f;
                Color dashColor = Color.Lerp(BloodRed, EmberOrange, progress);
                
                Vector2 pos = Player.Center + Main.rand.NextVector2Circular(35f, 25f);
                Vector2 vel = new Vector2(-direction * Main.rand.NextFloat(3f, 8f), Main.rand.NextFloat(-3f, 3f));
                
                var particle = new GenericGlowParticle(pos, vel, dashColor * 0.9f, 0.55f, 35, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            // Heavy smoke burst
            for (int i = 0; i < 12; i++)
            {
                var smoke = new HeavySmokeParticle(
                    Player.Center + Main.rand.NextVector2Circular(30f, 20f),
                    new Vector2(-direction * Main.rand.NextFloat(2f, 5f), Main.rand.NextFloat(-1f, 1f)),
                    CharredBlack * 0.7f,
                    Main.rand.Next(40, 60), 0.4f, 0.6f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Fire burst ring
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 firePos = Player.Center + angle.ToRotationVector2() * 30f;
                Color fireColor = Color.Lerp(Crimson, HellfireGold, i / 16f);
                CustomParticles.GenericFlare(firePos, fireColor, 0.45f, 20);
            }
            
            // Central hellfire flash
            CustomParticles.GenericFlare(Player.Center, HellfireGold, 1.0f, 22);
            CustomParticles.HaloRing(Player.Center, EmberOrange, 0.7f, 18);
            CustomParticles.HaloRing(Player.Center, BloodRed, 0.5f, 15);
        }
    }
}
