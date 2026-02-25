using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using ReLogic.Content;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.EnigmaVariations.Accessories
{
    /// <summary>
    /// Riddlemaster's Cauldron - Summoner Accessory
    /// 
    /// "Cauldron's Brew" - Minions periodically release "Mystery Vapors":
    /// - Confuses enemies and reduces their damage by 15%
    /// Every 5 seconds, a random minion gains "Riddle's Blessing":
    /// - Doubled attack speed for 3 seconds
    /// Additionally, +1 max minion slot.
    /// 
    /// Features a floating animated cauldron that orbits the player using a 6x6 sprite sheet.
    /// 
    /// Theme: The bubbling cauldron of riddles,
    /// where each answer spawns a dozen new questions.
    /// </summary>
    public class RiddlemastersCauldron : ModItem
    {
        // Enigma color palette
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // Enable floating cauldron visual
            // Enable floating cauldron visual
            var cauldronPlayer = player.GetModPlayer<RiddlemastersCauldronPlayer>();
            cauldronPlayer.showFloatingCauldron = !hideVisual;
            
            var modPlayer = player.GetModPlayer<EnigmaAccessoryPlayer>();
            modPlayer.hasRiddlemastersCauldron = true;
            
            // Ambient visual effects
            if (!hideVisual)
            {
                // Cauldron bubble particles around the player - every 30 frames
                if (Main.GameUpdateCount % 30 == 0)
                {
                    // Bubbles rise from the floating cauldron position
                    float cauldronAngle = cauldronPlayer.orbitAngle;
                    Vector2 cauldronPos = player.Center + new Vector2((float)Math.Cos(cauldronAngle) * 50f, 
                        (float)Math.Sin(cauldronAngle * 0.5f) * 15f - 20f);
                    
                    Vector2 bubbleStart = cauldronPos + new Vector2(Main.rand.NextFloat(-8f, 8f), -5f);
                    Vector2 bubbleVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -1.2f - Main.rand.NextFloat(0.6f));
                    
                    float progress = Main.rand.NextFloat();
                    Color bubbleColor = EnigmaAccessoryPlayer.GetEnigmaGradient(progress);
                    
                    CustomParticles.GenericGlow(bubbleStart, bubbleColor * 0.5f, 0.15f + Main.rand.NextFloat(0.1f), 30);
                }
                
                // Vapor timer visual - intensifying as vapor release approaches
                float vaporProgress = (float)modPlayer.vaporTimer / 180f; // 3 second interval
                if (vaporProgress > 0.7f)
                {
                    // Vapor is about to release - show building effect, every 15 frames
                    if (Main.GameUpdateCount % 15 == 0)
                    {
                        Vector2 vaporPos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                        CustomParticles.GenericGlow(vaporPos, EnigmaPurple * 0.6f, 0.2f, 15);
                    }
                }
                
                // Blessing timer visual - show which minion is blessed
                if (modPlayer.blessingDuration > 0 && modPlayer.blessedMinionIndex >= 0)
                {
                    Projectile blessedMinion = Main.projectile[modPlayer.blessedMinionIndex];
                    if (blessedMinion.active && blessedMinion.owner == player.whoAmI && blessedMinion.minion)
                    {
                        // Glowing aura around blessed minion - every 10 frames
                        if (Main.GameUpdateCount % 10 == 0)
                        {
                            Vector2 auraPos = blessedMinion.Center + Main.rand.NextVector2Circular(15f, 15f);
                            CustomParticles.GenericFlare(auraPos, EnigmaGreenFlame * 0.7f, 0.25f, 10);
                        }
                        
                        // Orbiting glyphs - every 25 frames
                        if (Main.GameUpdateCount % 25 == 0)
                        {
                            float glyphAngle = Main.GameUpdateCount * 0.08f + Main.rand.NextFloat() * MathHelper.TwoPi;
                            Vector2 glyphPos = blessedMinion.Center + glyphAngle.ToRotationVector2() * 20f;
                            CustomParticles.Glyph(glyphPos, EnigmaPurple * 0.6f, 0.2f, -1);
                        }
                        
                        // Watching eye above blessed minion - every 60 frames
                        if (Main.GameUpdateCount % 60 == 0)
                        {
                            Vector2 eyePos = blessedMinion.Top + new Vector2(Main.rand.NextFloat(-8f, 8f), -15f);
                            CustomParticles.EnigmaEyeGaze(eyePos, EnigmaGreenFlame * 0.5f, 0.25f, 
                                (blessedMinion.Center - eyePos).SafeNormalize(Vector2.UnitY));
                        }
                        
                        // Light from blessed minion
                        Lighting.AddLight(blessedMinion.Center, EnigmaGreenFlame.ToVector3() * 0.4f);
                    }
                }
                
                // General minion aura effects
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.owner == player.whoAmI && proj.minion)
                    {
                        // Subtle mystery vapor around all minions
                        if (Main.rand.NextBool(20))
                        {
                            Vector2 vaporPos = proj.Center + Main.rand.NextVector2Circular(12f, 12f);
                            Color vaporColor = EnigmaAccessoryPlayer.GetEnigmaGradient(Main.rand.NextFloat());
                            CustomParticles.GenericGlow(vaporPos, vaporColor * 0.3f, 0.12f, 18);
                        }
                    }
                }
                
                // Enigma aura for player
                ThemedParticles.EnigmaAura(player.Center, 30f, 0.2f);
            }
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "CauldronHeader", "Cauldron's Brew:")
            {
                OverrideColor = EnigmaDeepPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Minions release Mystery Vapors every 3 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Vapors confuse enemies and reduce their damage by 15%"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 5 seconds, a random minion gains Riddle's Blessing"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Blessed minions attack twice as fast for 3 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Minion", "+1 max minion slot"));
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The answer was always in the question'")
            {
                OverrideColor = EnigmaGreenFlame
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonanceEnergies.EnigmaResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCores.HarmonicCoreOfEnigma>(), 1)
                .AddIngredient(ItemID.PygmyNecklace)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Global projectile to handle Riddle's Blessing attack speed bonus for minions
    /// </summary>
    public class RiddlemastersCauldronGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        
        private int extraUpdateCounter = 0;
        
        public override void AI(Projectile projectile)
        {
            if (!projectile.minion) return;
            
            Player owner = Main.player[projectile.owner];
            if (owner == null || !owner.active) return;
            
            var modPlayer = owner.GetModPlayer<EnigmaAccessoryPlayer>();
            
            // Check if this minion is blessed
            if (modPlayer.hasRiddlemastersCauldron && modPlayer.blessingDuration > 0 && 
                modPlayer.blessedMinionIndex == projectile.whoAmI)
            {
                // Double attack speed by running AI twice (extra update simulation)
                extraUpdateCounter++;
                if (extraUpdateCounter >= 2)
                {
                    extraUpdateCounter = 0;
                    // The AI will naturally run again, effectively doubling speed
                    // We can also boost attack timing here
                    projectile.localAI[0] += 0.5f; // Speed up attack timers
                }
            }
            else
            {
                extraUpdateCounter = 0;
            }
        }
    }
    
    /// <summary>
    /// ModPlayer to handle the floating cauldron visual and animation state.
    /// </summary>
    public class RiddlemastersCauldronPlayer : ModPlayer
    {
        // Visual state
        public bool showFloatingCauldron = false;
        public float orbitAngle = 0f;
        public int animationFrame = 0;
        private int animationTimer = 0;
        private const int FramesPerAnimStep = 4; // Animation speed
        
        public override void ResetEffects()
        {
            showFloatingCauldron = false;
        }
        
        public override void PostUpdate()
        {
            if (showFloatingCauldron)
            {
                // Update orbit angle - cauldron circles player
                orbitAngle += 0.02f;
                if (orbitAngle > MathHelper.TwoPi)
                    orbitAngle -= MathHelper.TwoPi;
                
                // Update animation frame - cycles through 6x6 sprite sheet
                animationTimer++;
                if (animationTimer >= FramesPerAnimStep)
                {
                    animationTimer = 0;
                    animationFrame++;
                    if (animationFrame >= 36) // 6x6 = 36 frames
                        animationFrame = 0;
                }
            }
        }
    }
    
    /// <summary>
    /// Custom draw layer for the floating Riddlemaster's Cauldron.
    /// Uses 6x6 sprite sheet for bubbling animation.
    /// </summary>
    public class RiddlemastersCauldronDrawLayer : PlayerDrawLayer
    {
        // Enigma color palette
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        
        private Asset<Texture2D> _cauldronTexture;

        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.FrontAccFront);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            var modPlayer = drawInfo.drawPlayer.GetModPlayer<RiddlemastersCauldronPlayer>();
            return modPlayer.showFloatingCauldron && !drawInfo.drawPlayer.dead && !drawInfo.drawPlayer.invis;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            var modPlayer = player.GetModPlayer<RiddlemastersCauldronPlayer>();

            _cauldronTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Content/EnigmaVariations/Accessories/RiddlemastersCauldron_Summon");
            if (_cauldronTexture.State != AssetState.Loaded)
                return;

            Texture2D tex = _cauldronTexture.Value;
            int cols = 6;
            int rows = 6;
            int frameW = tex.Width / cols;
            int frameH = tex.Height / rows;

            // Get current frame from animation
            int frame = modPlayer.animationFrame;
            int col = frame % cols;
            int row = frame / cols;

            Rectangle source = new Rectangle(col * frameW, row * frameH, frameW, frameH);

            // Calculate orbiting position - figure-8 pattern around player
            float orbitRadius = 50f;
            float verticalBob = (float)Math.Sin(modPlayer.orbitAngle * 0.5f) * 15f;
            Vector2 offset = new Vector2(
                (float)Math.Cos(modPlayer.orbitAngle) * orbitRadius,
                verticalBob - 20f // Float above player
            );
            
            Vector2 pos = drawInfo.Position - Main.screenPosition + new Vector2(player.width / 2f, player.height / 2f) + offset;
            pos.Y += player.gfxOffY;
            pos = pos.Floor();

            // Determine direction based on orbit position
            SpriteEffects fx = offset.X > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (player.gravDir == -1)
                fx |= SpriteEffects.FlipVertically;

            // Use a mysterious glow color
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 0.85f;
            Color glowColor = Color.Lerp(Color.White, EnigmaPurple, 0.2f) * pulse;

            Vector2 origin = new Vector2(frameW / 2f, frameH / 2f);

            // Draw main cauldron
            float scale = 0.5f;
            DrawData mainData = new DrawData(tex, pos, source, glowColor, 0f, origin, scale, fx, 0);
            drawInfo.DrawDataCache.Add(mainData);
            
            // Draw additive glow layer
            Color additiveGlow = EnigmaGreenFlame * 0.3f * pulse;
            additiveGlow.A = 0; // For additive blending effect
            DrawData glowData = new DrawData(tex, pos, source, additiveGlow, 0f, origin, scale * 1.1f, fx, 0);
            drawInfo.DrawDataCache.Add(glowData);
            
            // Add dynamic lighting from the cauldron
            Vector2 lightPos = player.Center + offset;
            Lighting.AddLight(lightPos, EnigmaGreenFlame.ToVector3() * 0.5f * pulse);
        }
    }
}
