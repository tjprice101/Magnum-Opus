using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;

namespace MagnumOpus.Content.LaCampanella.HarmonicCores
{
    public class HarmonicCoreOfLaCampanella : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.scale = 1.25f; // Display 25% larger
            Item.maxStack = 1;
            Item.value = Item.sellPrice(gold: 35);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 4 Harmonic Core]")
            {
                OverrideColor = new Color(255, 180, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "HarmonicCore", "Equip in the Harmonic Core UI (opens with inventory)")
            {
                OverrideColor = new Color(255, 180, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer1", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "DamageBonus", "+10% All Damage")
            {
                OverrideColor = new Color(120, 200, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer2", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "UniqueHeader", "◆ Bell's Resonance")
            {
                OverrideColor = new Color(255, 180, 100)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect1", "  Every hit echoes with a delayed bell chime")
            {
                OverrideColor = new Color(255, 210, 150)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect2", "  The chime deals 25% of the original damage")
            {
                OverrideColor = new Color(255, 210, 150)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer3", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The bell tolls for thee'")
            {
                OverrideColor = new Color(180, 140, 100)
            });
        }

        public override void PostUpdate()
        {
            // Golden bell glow
            Lighting.AddLight(Item.Center, 0.6f, 0.5f, 0.2f);
            
            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GoldFlame, 0f, -0.5f, 100, default, 1.2f);
                dust.noGravity = true;
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Use the larger _World texture when item is dropped on the ground
            Texture2D worldTexture = ModContent.Request<Texture2D>("MagnumOpus/Content/LaCampanella/HarmonicCores/HarmonicCoreOfLaCampanella_World", AssetRequestMode.ImmediateLoad).Value;
            
            Vector2 drawPosition = Item.Center - Main.screenPosition;
            Vector2 origin = worldTexture.Size() / 2f;
            
            // Pulsing golden glow effect
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f + 0.9f;
            Color glowColor = new Color(255, 200, 100) * 0.4f * pulse;
            
            // Draw glow layers (additive blending for glow)
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer golden glow
            spriteBatch.Draw(worldTexture, drawPosition, null, glowColor, rotation, origin, scale * 1.15f * pulse, SpriteEffects.None, 0f);
            // Inner warm glow
            spriteBatch.Draw(worldTexture, drawPosition, null, new Color(255, 150, 50) * 0.3f * pulse, rotation, origin, scale * 1.08f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw the actual item
            spriteBatch.Draw(worldTexture, drawPosition, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);
            
            return false; // Don't draw the default texture
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfLaCampanella>(25)
                .AddIngredient<LaCampanellaResonantEnergy>(25)
                .AddTile(ModContent.TileType<Content.MoonlightSonata.CraftingStations.MoonlightAnvilTile>())
                .Register();
        }
    }
    
    public class ResonantCoreOfLaCampanella : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 99;
            Item.value = Item.sellPrice(gold: 8);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 0.5f, 0.4f, 0.2f);
        }
    }
}
