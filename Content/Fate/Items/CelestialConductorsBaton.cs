using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.Fate.Items
{
    /// <summary>
    /// Celestial Conductor's Baton - Summons Fate, The Warden of Universal Melodies
    /// Requires Moon Lord to be defeated.
    /// </summary>
    public class CelestialConductorsBaton : ModItem
    {
        // Placeholder texture until custom art is created
        public override string Texture => "Terraria/Images/Item_" + ItemID.CelestialSigil;
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 13; // After Moon Lord items
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 20;
            Item.rare = ItemRarityID.Red;
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = true;
            Item.value = Item.buyPrice(gold: 5);
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "SummonInfo", "Summons Fate, The Warden of Universal Melodies")
            {
                OverrideColor = new Color(255, 60, 80)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Warning", "Use at night")
            {
                OverrideColor = new Color(200, 200, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The celestial symphony awaits its conductor...'")
            {
                OverrideColor = new Color(180, 50, 100)
            });
        }

        public override bool CanUseItem(Player player)
        {
            // Prevent use if boss is already alive
            if (NPC.AnyNPCs(ModContent.NPCType<FateWardenOfMelodies>()))
                return false;
            
            // Must be nighttime
            if (Main.dayTime)
                return false;
            
            // Requires Moon Lord defeated
            if (!NPC.downedMoonlord)
            {
                Main.NewText("The celestial forces do not yet recognize you...", new Color(180, 50, 100));
                return false;
            }
            
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                SoundEngine.PlaySound(SoundID.Roar, player.position);

                int type = ModContent.NPCType<FateWardenOfMelodies>();

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Spawn above player
                    Vector2 spawnPos = player.Center + new Vector2(0, -600f);
                    NPC.NewNPC(player.GetSource_ItemUse(Item), (int)spawnPos.X, (int)spawnPos.Y, type);
                }
                else
                {
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
                }
                
                // Summon VFX
                CustomParticles.GenericFlare(player.Center, new Color(255, 60, 80), 1.5f, 30);
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    CustomParticles.HaloRing(player.Center, new Color(180, 50, 100), 0.3f + i * 0.1f, 15 + i * 2);
                }
                CustomParticles.GlyphBurst(player.Center, new Color(120, 30, 140), 8, 5f);
            }

            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonanceEnergies.FateResonantEnergy>(10)
                .AddIngredient(ItemID.LunarBar, 5)
                .AddIngredient(ItemID.FragmentSolar, 5)
                .AddIngredient(ItemID.FragmentNebula, 5)
                .AddIngredient(ItemID.FragmentStardust, 5)
                .AddIngredient(ItemID.FragmentVortex, 5)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }

        public override void PostUpdate()
        {
            // Celestial glow
            Lighting.AddLight(Item.Center, 0.9f, 0.3f, 0.45f);
            
            if (Main.rand.NextBool(15))
            {
                CustomParticles.GenericFlare(Item.Center, new Color(180, 50, 100) * 0.5f, 0.25f, 15);
            }
        }
    }
}
