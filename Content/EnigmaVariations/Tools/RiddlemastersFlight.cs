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
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common;

namespace MagnumOpus.Content.EnigmaVariations.Tools
{
    /// <summary>
    /// Riddlemaster's Flight - Enigma Variations wings with mysterious arcane aesthetics.
    /// Features eerie green flames, watching eyes, and arcane glyphs.
    /// Uses 6x6 sprite sheet for flying animation.
    /// </summary>
    [AutoloadEquip(EquipType.Wings)]
    public class RiddlemastersFlight : ModItem
    {
        public override void SetStaticDefaults()
        {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(
                flyTime: 360, // Higher than Swan Lake (340)
                flySpeedOverride: 18f, // Faster than Swan Lake (17)
                accelerationMultiplier: 3.8f, // Higher than Swan Lake (3.6)
                hasHoldDownHoverFeatures: true
            );
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 5);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Soar through the spaces between questions'") { OverrideColor = new Color(140, 60, 200) });
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
            ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling = 1.3f;
            ascentWhenRising = 0.26f;
            maxCanAscendMultiplier = 1.5f;
            maxAscentMultiplier = 4.4f;
            constantAscend = 0.22f;
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RiddlemastersFlightPlayer>().hasWingsEquipped = true;
            
            // Mysterious enigma particles when flying - performance optimized
            if (!hideVisual && player.velocity.Y != 0)
            {
                // EnigmaAura only every 20 frames
                if (Main.GameUpdateCount % 20 == 0)
                    ThemedParticles.EnigmaAura(player.Center, 35f);
                
                // Eerie green flame wisps - every 20 frames
                if (Main.GameUpdateCount % 20 == 0)
                {
                    Color greenFlame = ThemedParticles.EnigmaGreenFlame;
                    var glow = new GenericGlowParticle(
                        player.Center + Main.rand.NextVector2Circular(20f, 15f),
                        new Vector2(player.velocity.X * -0.05f, -1.5f),
                        greenFlame, 0.3f, 25, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                
                // Occasional glyph - every 50 frames
                if (Main.GameUpdateCount % 50 == 0)
                {
                    CustomParticles.Glyph(player.Center + Main.rand.NextVector2Circular(30f, 25f), 
                        ThemedParticles.EnigmaPurple, 0.25f, -1);
                }
                
                // Rare watching eye - every 120 frames
                if (Main.GameUpdateCount % 120 == 0)
                {
                    Vector2 eyePos = player.Center + Main.rand.NextVector2Circular(25f, 20f);
                    CustomParticles.EnigmaEyeGaze(eyePos, ThemedParticles.EnigmaGreenFlame * 0.7f, 0.3f, null);
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<EnigmaResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEnigma>(), 5)
                .AddIngredient(ItemID.SoulofFlight, 40)
                .AddIngredient(ItemID.SoulofSight, 20)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class RiddlemastersFlightPlayer : ModPlayer
    {
        public int wingFrame = 0;
        private int frameCounter = 0;
        private bool wasFlying = false;
        
        // Direction constants for doubleTapCardinalTimer array
        private const int DashDown = 0;
        private const int DashUp = 1;
        private const int DashLeft = 2;
        private const int DashRight = 3;
        
        public bool hasWingsEquipped = false;
        private int dodgeCooldown = 0;
        private const int DodgeCooldownMax = 18;
        private const float DodgeSpeed = 34f;
        private bool isDodging = false;
        private int dodgeTimer = 0;
        private const int DodgeDuration = 9;
        private int dashDir = -1;

        public override void ResetEffects()
        {
            hasWingsEquipped = false;
            
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "RiddlemastersFlight", EquipType.Wings);
            bool hasWings = Player.wings == wingSlot && wingSlot > 0;
            
            if (!hasWings && !hasWingsEquipped)
            {
                dashDir = -1;
                return;
            }
            
            if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[DashRight] < 15 && Player.doubleTapCardinalTimer[DashLeft] == 0)
            {
                dashDir = DashRight;
            }
            else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[DashLeft] < 15 && Player.doubleTapCardinalTimer[DashRight] == 0)
            {
                dashDir = DashLeft;
            }
            else
            {
                dashDir = -1;
            }
        }
        
        public override void PreUpdateMovement()
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "RiddlemastersFlight", EquipType.Wings);
            bool hasWings = Player.wings == wingSlot && wingSlot > 0;
            
            if (!hasWings && !hasWingsEquipped)
                return;
            
            if (CanDodge() && dashDir != -1 && dodgeCooldown <= 0)
            {
                int direction = dashDir == DashLeft ? -1 : 1;
                PerformDodge(direction);
            }
            
            if (isDodging)
            {
                dodgeTimer++;
                Player.immune = true;
                Player.immuneTime = 2;
                Player.immuneNoBlink = true;
                
                ThemedParticles.EnigmaTrail(Player.Center, Player.velocity);
                
                for (int i = 0; i < 2; i++)
                {
                    Dust trail = Dust.NewDustDirect(Player.position, Player.width, Player.height, 
                        DustID.GreenTorch, -Player.velocity.X * 0.2f, -Player.velocity.Y * 0.2f, 100, default, 1.4f);
                    trail.noGravity = true;
                }
                
                if (dodgeTimer >= DodgeDuration)
                {
                    isDodging = false;
                    dodgeTimer = 0;
                    
                    ThemedParticles.EnigmaImpact(Player.Center, 1f);
                    CustomParticles.GlyphBurst(Player.Center, ThemedParticles.EnigmaPurple, 6, 4f);
                }
            }
            
            if (dodgeCooldown > 0)
                dodgeCooldown--;
        }
        
        private bool CanDodge()
        {
            return (hasWingsEquipped || Player.wings == EquipLoader.GetEquipSlot(Mod, "RiddlemastersFlight", EquipType.Wings))
                && Player.dashType == DashID.None
                && !Player.setSolar
                && !Player.mount.Active;
        }

        public override void PostUpdate()
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "RiddlemastersFlight", EquipType.Wings);
            bool hasWings = Player.wings == wingSlot && wingSlot > 0;

            if (!hasWings && !hasWingsEquipped)
            {
                wingFrame = 0;
                frameCounter = 0;
                wasFlying = false;
                return;
            }

            bool isFlying = Player.controlJump && Player.velocity.Y != 0 && !Player.mount.Active;
            bool isOnGround = Player.velocity.Y == 0;
            
            if (isFlying || isDodging)
            {
                frameCounter++;
                if (frameCounter >= 2)
                {
                    frameCounter = 0;
                    wingFrame++;
                    if (wingFrame >= 36)
                        wingFrame = 0;
                }
                wasFlying = true;
            }
            else if (isOnGround)
            {
                wingFrame = 0;
                frameCounter = 0;
                wasFlying = false;
            }
            else if (wasFlying && !isFlying)
            {
                wingFrame = 0;
                frameCounter = 0;
            }
        }

        private void PerformDodge(int direction)
        {
            isDodging = true;
            dodgeTimer = 0;
            dodgeCooldown = DodgeCooldownMax;
            
            Vector2 dodgeVelocity = new Vector2(direction, 0f);
            Player.velocity = dodgeVelocity * DodgeSpeed;
            
            SoundEngine.PlaySound(SoundID.Item103 with { Pitch = -0.2f, Volume = 0.7f }, Player.Center);
            
            ThemedParticles.EnigmaImpact(Player.Center, 1.5f);
            CustomParticles.GlyphCircle(Player.Center, ThemedParticles.EnigmaGreenFlame, 6, 40f, 0.03f);
            CustomParticles.EnigmaEyeFormation(Player.Center, ThemedParticles.EnigmaPurple * 0.8f, 3, 35f);
        }
        
        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "RiddlemastersFlight", EquipType.Wings);
            if (Player.wings == wingSlot && wingSlot > 0)
            {
                PlayerDrawLayers.Wings.Hide();
            }
        }
    }
    
    /// <summary>
    /// Custom draw layer for Riddlemaster's Flight.
    /// Uses 6x6 sprite sheet for all animation.
    /// </summary>
    public class RiddlemastersFlightLayer : PlayerDrawLayer
    {
        private Asset<Texture2D> _animatedTexture;

        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.BackAcc);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "RiddlemastersFlight", EquipType.Wings);
            return drawInfo.drawPlayer.wings == wingSlot && wingSlot > 0 && !drawInfo.drawPlayer.dead && !drawInfo.drawPlayer.invis;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            var modPlayer = player.GetModPlayer<RiddlemastersFlightPlayer>();

            _animatedTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Content/EnigmaVariations/Tools/RiddlemastersFlight_Wings");
            if (_animatedTexture.State != AssetState.Loaded)
                return;

            Texture2D tex = _animatedTexture.Value;
            int cols = 6;
            int rows = 6;
            int frameW = tex.Width / cols;
            int frameH = tex.Height / rows;

            int frame = modPlayer.wingFrame;
            int col = frame % cols;
            int row = frame / cols;

            Rectangle source = new Rectangle(col * frameW, row * frameH, frameW, frameH);

            Vector2 pos = drawInfo.Position - Main.screenPosition + new Vector2(player.width / 2f, player.height / 2f);
            pos.Y += player.gfxOffY;
            pos = pos.Floor();

            SpriteEffects fx = player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (player.gravDir == -1)
                fx |= SpriteEffects.FlipVertically;

            Color color = drawInfo.colorArmorBody;

            Vector2 origin = new Vector2(source.Width / 2f, source.Height / 2f);

            DrawData data = new DrawData(tex, pos, source, color, player.bodyRotation, origin, 0.339f, fx, 0);
            drawInfo.DrawDataCache.Add(data);
        }
    }
}
