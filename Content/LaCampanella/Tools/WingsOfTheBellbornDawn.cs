using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using ReLogic.Content;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common;

namespace MagnumOpus.Content.LaCampanella.Tools
{
    /// <summary>
    /// Wings of the Bellborn Dawn - La Campanella wings with infernal bell aesthetics.
    /// Higher tier than Eroica, features smoky fire trails and bell chime effects.
    /// Uses 6x6 sprite sheet for flying animation.
    /// </summary>
    [AutoloadEquip(EquipType.Wings)]
    public class WingsOfTheBellbornDawn : ModItem
    {
        public override void SetStaticDefaults()
        {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(
                flyTime: 320, // Higher than Eroica (280)
                flySpeedOverride: 16f, // Faster than Eroica (14)
                accelerationMultiplier: 3.5f, // Higher than Eroica (3.2)
                hasHoldDownHoverFeatures: true
            );
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
            ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling = 1.2f;
            ascentWhenRising = 0.24f;
            maxCanAscendMultiplier = 1.4f;
            maxAscentMultiplier = 4.0f;
            constantAscend = 0.20f;
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<WingsOfTheBellbornDawnPlayer>().hasWingsEquipped = true;
            
            // Infernal bell particles when flying
            if (!hideVisual && player.velocity.Y != 0)
            {
                ThemedParticles.LaCampanellaAura(player.Center, 35f);
                
                // Occasional smoke particles
                if (Main.rand.NextBool(4))
                {
                    var smoke = new HeavySmokeParticle(
                        player.Center + Main.rand.NextVector2Circular(15f, 15f),
                        new Vector2(player.velocity.X * -0.1f, -1f),
                        Color.Black, Main.rand.Next(25, 40), 0.3f, 0.6f, 0.02f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<LaCampanellaResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfLaCampanella>(), 5)
                .AddIngredient(ItemID.SoulofFlight, 30)
                .AddIngredient(ItemID.SoulofFright, 20)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class WingsOfTheBellbornDawnPlayer : ModPlayer
    {
        public int wingFrame = 0;
        private int frameCounter = 0;
        private bool wasFlying = false;
        
        public bool hasWingsEquipped = false;
        private int dodgeCooldown = 0;
        private const int DodgeCooldownMax = 22;
        private const float DodgeSpeed = 30f;
        private bool isDodging = false;
        private int dodgeTimer = 0;
        private const int DodgeDuration = 8;
        private int lastDodgeDirection = 0;

        public override void ResetEffects()
        {
            hasWingsEquipped = false;
        }

        public override void PostUpdate()
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "WingsOfTheBellbornDawn", EquipType.Wings);
            bool hasWings = Player.wings == wingSlot && wingSlot > 0;

            if (!hasWings && !hasWingsEquipped)
            {
                wingFrame = 0;
                frameCounter = 0;
                wasFlying = false;
                return;
            }

            // Dodge cooldown tick
            if (dodgeCooldown > 0)
                dodgeCooldown--;
            
            // Handle dodge input - double-tap left/right like Shield of Cthulhu
            if (dodgeCooldown <= 0 && !isDodging)
            {
                if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[2] < 15)
                {
                    PerformDodge(-1);
                }
                else if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[3] < 15)
                {
                    PerformDodge(1);
                }
            }
            
            // Handle ongoing dodge
            if (isDodging)
            {
                dodgeTimer++;
                Player.immune = true;
                Player.immuneTime = 2;
                Player.immuneNoBlink = true;
                
                // Infernal trail with smoke
                ThemedParticles.DodgeTrail(Player.Center, Player.velocity, false);
                
                // Fire dust trail
                for (int i = 0; i < 3; i++)
                {
                    Dust trail = Dust.NewDustDirect(Player.position, Player.width, Player.height, 
                        DustID.Torch, -Player.velocity.X * 0.2f, -Player.velocity.Y * 0.2f, 100, default, 1.5f);
                    trail.noGravity = true;
                }
                
                if (dodgeTimer >= DodgeDuration)
                {
                    isDodging = false;
                    dodgeTimer = 0;
                    
                    // Bell chime burst on dodge end
                    ThemedParticles.LaCampanellaImpact(Player.Center, 1f);
                    SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f, Volume = 0.6f }, Player.Center);
                }
            }

            // Wing animation logic
            bool isFlying = Player.controlJump && Player.velocity.Y != 0 && !Player.mount.Active;
            bool isOnGround = Player.velocity.Y == 0;
            
            if (isFlying || isDodging)
            {
                frameCounter++;
                if (frameCounter >= 2)
                {
                    frameCounter = 0;
                    wingFrame++;
                    if (wingFrame >= 36) // 6x6 = 36 frames
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
            lastDodgeDirection = direction;
            
            Vector2 dodgeDir = new Vector2(direction, 0f);
            Player.velocity = dodgeDir * DodgeSpeed;
            
            SoundEngine.PlaySound(SoundID.Item74 with { Pitch = 0.2f, Volume = 0.8f }, Player.Center);
            
            // Infernal burst
            ThemedParticles.LaCampanellaImpact(Player.Center, 1.5f);
            ThemedParticles.TeleportBurst(Player.Center, false);
        }
        
        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "WingsOfTheBellbornDawn", EquipType.Wings);
            if (Player.wings == wingSlot && wingSlot > 0)
            {
                PlayerDrawLayers.Wings.Hide();
            }
        }
    }
    
    /// <summary>
    /// Custom draw layer for Wings of the Bellborn Dawn.
    /// Uses 6x6 sprite sheet for all animation.
    /// </summary>
    public class WingsOfTheBellbornDawnLayer : PlayerDrawLayer
    {
        private Asset<Texture2D> _animatedTexture;

        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.BackAcc);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            int wingSlot = EquipLoader.GetEquipSlot(Mod, "WingsOfTheBellbornDawn", EquipType.Wings);
            return drawInfo.drawPlayer.wings == wingSlot && wingSlot > 0 && !drawInfo.drawPlayer.dead && !drawInfo.drawPlayer.invis;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            var modPlayer = player.GetModPlayer<WingsOfTheBellbornDawnPlayer>();

            _animatedTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Content/LaCampanella/Tools/WingsOfTheBellbornDawn_Wings");
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
