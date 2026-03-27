using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Bosses;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.OdeToJoy.SummonItems
{
    /// <summary>
    /// Score of Ode to Joy — Summon item for Ode to Joy, Chromatic Rose Conductor.
    /// Used on the Surface after defeating Dies Irae.
    /// </summary>
    public class ScoreOfOdeToJoy : ModItem
    {
        private static readonly Color RosePink = new Color(255, 105, 180);
        private static readonly Color GoldenPollen = new Color(255, 200, 50);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 18;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 20;
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = true;
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.value = Item.sellPrice(gold: 5);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons Ode to Joy, Chromatic Rose Conductor"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Use on the Surface during daytime"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The final movement begins — let joy be unconfined'")
            {
                OverrideColor = GoldenPollen
            });
        }

        public override bool CanUseItem(Player player)
        {
            // Must be on the surface during daytime
            if (!Main.dayTime || player.ZoneUnderworldHeight || player.ZoneSkyHeight)
            {
                if (Main.netMode != NetmodeID.Server)
                    Main.NewText("The Conductor only appears under the jubilant sun...", RosePink);
                return false;
            }

            // Check that boss isn't already alive
            return !NPC.AnyNPCs(ModContent.NPCType<OdeToJoyChromaticRoseConductor>());
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.4f }, player.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<OdeToJoyChromaticRoseConductor>());
                }
                else
                {
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent,
                        number: player.whoAmI,
                        number2: ModContent.NPCType<OdeToJoyChromaticRoseConductor>());
                }

                // Summon VFX — rose petals and golden sparkles
                for (int i = 0; i < 30; i++)
                {
                    Vector2 pos = player.Center + Main.rand.NextVector2Circular(60f, 60f);
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-4f, -1f));
                    Color color = Color.Lerp(RosePink, GoldenPollen, Main.rand.NextFloat());
                    CustomParticles.GenericFlare(pos, color, 0.4f, 25);
                }
            }

            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 1)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 10)
                .AddIngredient(ItemID.SoulofLight, 10)
                .AddIngredient(ItemID.LifeFruit, 5)
                .AddTile<FatesCosmicAnvilTile>()
                .Register();
        }
    }
}
