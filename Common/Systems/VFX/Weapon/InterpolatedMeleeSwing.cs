using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.DataStructures;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// CALAMITY-STYLE INTERPOLATED MELEE SWING SYSTEM
    /// 
    /// This system provides buttery-smooth melee weapon animations by:
    /// 1. Computing swing rotation using piecewise Bézier easing curves
    /// 2. Storing previous frame values for sub-frame interpolation
    /// 3. Rendering the weapon in a PlayerDrawLayer with interpolation
    /// 4. Hiding vanilla weapon rendering to prevent double-draw
    /// 
    /// The result is 144Hz+ smooth animations with zero frame-jumping.
    /// </summary>
    public class InterpolatedMeleeSwingPlayer : ModPlayer
    {
        #region Swing State
        
        // Current swing state
        public bool IsSwinging { get; private set; }
        public bool HideVanillaWeapon { get; private set; }
        public float CurrentRotation { get; private set; }
        public float PreviousRotation { get; private set; }
        public Vector2 CurrentWeaponPos { get; private set; }
        public Vector2 PreviousWeaponPos { get; private set; }
        public float CurrentScale { get; private set; } = 1f;
        public float PreviousScale { get; private set; } = 1f;
        public int SwingDirection { get; private set; } = 1;
        public string Theme { get; private set; } = "generic";
        public Item CurrentWeapon { get; private set; }
        
        // Raw progress values for interpolation
        private float _currentLinearProgress;
        private float _previousLinearProgress;
        private float _currentEasedProgress;
        private float _previousEasedProgress;
        
        // Swing arc parameters (calculated at swing start)
        private float _startAngle;
        private float _endAngle;
        private float _targetAngle; // Angle toward cursor at swing start
        
        // Track swing timing for smoother transitions
        private int _swingStartTick;
        private int _lastSwingItemType = -1;
        
        #endregion
        
        #region Exo Blade Style Curves
        
        /// <summary>
        /// Exo Blade-style piecewise swing curves.
        /// SlowStart → FastSnap → SmoothEnd creates premium feel.
        /// </summary>
        private static readonly CurveSegment[] ExoBladeSwingCurve = new CurveSegment[]
        {
            // 0% - 20%: Slow anticipation buildup
            new CurveSegment(EasingType.SineIn, 0f, 0f, 0.08f),
            // 20% - 35%: Accelerating into snap
            new CurveSegment(EasingType.PolyIn, 0.20f, 0.08f, 0.22f, 2),
            // 35% - 65%: FAST aggressive snap (the satisfying part)
            new CurveSegment(EasingType.Linear, 0.35f, 0.30f, 0.50f),
            // 65% - 85%: Deceleration
            new CurveSegment(EasingType.PolyOut, 0.65f, 0.80f, 0.15f, 2),
            // 85% - 100%: Gentle stop
            new CurveSegment(EasingType.SineOut, 0.85f, 0.95f, 0.05f)
        };
        
        private static float GetEasedProgress(float linearProgress)
        {
            return PiecewiseAnimation(MathHelper.Clamp(linearProgress, 0f, 0.9999f), ExoBladeSwingCurve);
        }
        
        #endregion
        
        public override void PreUpdate()
        {
            // Store previous frame values for interpolation
            PreviousRotation = CurrentRotation;
            PreviousWeaponPos = CurrentWeaponPos;
            PreviousScale = CurrentScale;
            _previousLinearProgress = _currentLinearProgress;
            _previousEasedProgress = _currentEasedProgress;
        }
        
        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            // Hide vanilla weapon rendering when we're doing interpolated drawing
            if (IsSwinging && HideVanillaWeapon)
            {
                PlayerDrawLayers.HeldItem.Hide();
            }
        }
        
        public override void PostUpdate()
        {
            Item heldItem = Player.HeldItem;
            
            // Check if we're swinging a MagnumOpus melee weapon
            bool shouldBeSwinging = Player.itemAnimation > 0 && IsMagnumMeleeWeapon(heldItem);
            
            if (shouldBeSwinging)
            {
                // Detect new swing start
                if (!IsSwinging || heldItem.type != _lastSwingItemType)
                {
                    InitializeSwing(heldItem);
                }
                
                IsSwinging = true;
                HideVanillaWeapon = true;
                CurrentWeapon = heldItem;
                _lastSwingItemType = heldItem.type;
                
                // Calculate current swing state
                UpdateSwingState();
            }
            else
            {
                // Swing ended
                if (IsSwinging)
                {
                    EndSwing();
                }
            }
        }
        
        private void InitializeSwing(Item item)
        {
            _swingStartTick = (int)Main.GameUpdateCount;
            SwingDirection = Player.direction;
            Theme = DetectTheme(item);
            
            // Calculate target angle (toward cursor at swing START)
            Vector2 toMouse = (Main.MouseWorld - Player.MountedCenter).SafeNormalize(Vector2.UnitX * Player.direction);
            _targetAngle = toMouse.ToRotation();
            
            // Calculate arc (weapon swings FROM above head TO below, WITH target in middle)
            float arcRadians = MathHelper.ToRadians(150f); // 150° total arc
            
            if (SwingDirection == 1) // Facing right
            {
                _startAngle = _targetAngle - arcRadians * 0.55f; // Wind up above
                _endAngle = _targetAngle + arcRadians * 0.45f;   // Follow through below
            }
            else // Facing left
            {
                _startAngle = _targetAngle + arcRadians * 0.55f;
                _endAngle = _targetAngle - arcRadians * 0.45f;
            }
            
            // Initialize rotation to start
            CurrentRotation = _startAngle + MathHelper.PiOver4;
            PreviousRotation = CurrentRotation;
            CurrentScale = 1f;
            PreviousScale = 1f;
            
            // Initialize position too
            Vector2 baseOffset = new Vector2(6f * SwingDirection, Player.gravDir * -2f);
            Vector2 bladeDir = (_startAngle).ToRotationVector2();
            CurrentWeaponPos = Player.MountedCenter + baseOffset + bladeDir * 8f;
            PreviousWeaponPos = CurrentWeaponPos;
        }
        
        private void UpdateSwingState()
        {
            // Calculate linear progress (0 to 1)
            _currentLinearProgress = 1f - (float)Player.itemAnimation / Player.itemAnimationMax;
            _currentLinearProgress = MathHelper.Clamp(_currentLinearProgress, 0f, 1f);
            
            // Apply Exo Blade easing
            _currentEasedProgress = GetEasedProgress(_currentLinearProgress);
            
            // Interpolate angle along the arc
            float currentAngle = MathHelper.Lerp(_startAngle, _endAngle, _currentEasedProgress);
            
            // Add whip snap bonus during fast phase
            float whipBonus = CalculateWhipBonus(_currentLinearProgress, _currentEasedProgress);
            currentAngle += whipBonus * SwingDirection;
            
            // Final rotation (add PiOver4 for proper sword orientation)
            CurrentRotation = currentAngle + MathHelper.PiOver4;
            
            // Handle gravity inversion
            if (Player.gravDir == -1f)
            {
                CurrentRotation = -CurrentRotation + MathHelper.Pi;
            }
            
            // Calculate weapon position
            Vector2 baseOffset = new Vector2(6f * SwingDirection, Player.gravDir * -2f);
            Vector2 bladeDir = (CurrentRotation - MathHelper.PiOver4).ToRotationVector2();
            
            // Stretch during fast phase
            float stretch = CalculateStretch(_currentLinearProgress);
            CurrentScale = 1f + stretch * 0.15f; // Subtle scale increase
            
            float reachDistance = 8f + stretch * 12f;
            CurrentWeaponPos = Player.MountedCenter + baseOffset + bladeDir * reachDistance;
            
            // Apply to player for hitbox purposes (even though we render separately)
            Player.itemRotation = CurrentRotation;
            Player.itemLocation = CurrentWeaponPos;
            Player.FlipItemLocationAndRotationForGravity();
            
            // Arm animation
            UpdateArmAnimation();
        }
        
        private float CalculateWhipBonus(float linearProgress, float easedProgress)
        {
            // During the fast snap phase (35%-65%), add extra rotational "snap"
            if (linearProgress < 0.35f || linearProgress > 0.65f)
                return 0f;
            
            // Calculate curve velocity (derivative of easing function)
            float delta = 0.01f;
            float prevEased = GetEasedProgress(Math.Max(0f, linearProgress - delta));
            float nextEased = GetEasedProgress(Math.Min(1f, linearProgress + delta));
            float velocity = (nextEased - prevEased) / (delta * 2f);
            
            // Whip bonus peaks at highest velocity
            float phaseProg = (linearProgress - 0.35f) / 0.30f;
            float peak = (float)Math.Sin(phaseProg * MathHelper.Pi);
            
            return velocity * peak * 0.08f;
        }
        
        private float CalculateStretch(float linearProgress)
        {
            // Stretch peaks at 40% through swing (during the snap)
            if (linearProgress < 0.15f || linearProgress > 0.85f)
                return 0f;
            
            float stretchPeak = 0.40f;
            float stretchProgress;
            
            if (linearProgress < stretchPeak)
            {
                stretchProgress = (linearProgress - 0.15f) / (stretchPeak - 0.15f);
                stretchProgress = (float)Math.Sin(stretchProgress * MathHelper.PiOver2); // Ease in
            }
            else
            {
                stretchProgress = 1f - (linearProgress - stretchPeak) / (0.85f - stretchPeak);
                stretchProgress = (float)Math.Sin(stretchProgress * MathHelper.PiOver2); // Ease out
            }
            
            return stretchProgress;
        }
        
        private void UpdateArmAnimation()
        {
            float armRotation = CurrentRotation;
            if (SwingDirection == -1)
                armRotation += MathHelper.Pi;
            
            var armStretch = Player.CompositeArmStretchAmount.Full;
            if (_currentLinearProgress < 0.2f || _currentLinearProgress > 0.8f)
                armStretch = Player.CompositeArmStretchAmount.ThreeQuarters;
            
            Player.SetCompositeArmFront(true, armStretch, armRotation - MathHelper.PiOver2);
        }
        
        private void EndSwing()
        {
            IsSwinging = false;
            HideVanillaWeapon = false;
            CurrentWeapon = null;
        }
        
        private bool IsMagnumMeleeWeapon(Item item)
        {
            if (item == null || item.IsAir) return false;
            if (!item.CountsAsClass(DamageClass.Melee)) return false;
            if (item.useStyle != ItemUseStyleID.Swing) return false;
            if (item.noUseGraphic) return false;
            if (item.ModItem?.Mod != ModContent.GetInstance<MagnumOpus>()) return false;
            return true;
        }
        
        private string DetectTheme(Item item)
        {
            if (item?.ModItem == null) return "generic";
            
            string fullName = item.ModItem.GetType().FullName ?? "";
            
            if (fullName.Contains("Eroica")) return "Eroica";
            if (fullName.Contains("LaCampanella")) return "LaCampanella";
            if (fullName.Contains("SwanLake")) return "SwanLake";
            if (fullName.Contains("MoonlightSonata")) return "MoonlightSonata";
            if (fullName.Contains("Enigma")) return "Enigma";
            if (fullName.Contains("Fate")) return "Fate";
            if (fullName.Contains("ClairDeLune")) return "ClairDeLune";
            if (fullName.Contains("DiesIrae")) return "DiesIrae";
            
            return "generic";
        }
        
        /// <summary>
        /// Gets the interpolated rotation for rendering (uses partialTicks for sub-frame smoothness).
        /// </summary>
        public float GetInterpolatedRotation()
        {
            float partialTicks = InterpolatedRenderer.PartialTicks;
            
            // Interpolate rotation handling wrap-around
            float diff = MathHelper.WrapAngle(CurrentRotation - PreviousRotation);
            return MathHelper.WrapAngle(PreviousRotation + diff * partialTicks);
        }
        
        /// <summary>
        /// Gets the interpolated position for rendering.
        /// </summary>
        public Vector2 GetInterpolatedPosition()
        {
            float partialTicks = InterpolatedRenderer.PartialTicks;
            return Vector2.Lerp(PreviousWeaponPos, CurrentWeaponPos, partialTicks);
        }
        
        /// <summary>
        /// Gets the interpolated scale for rendering.
        /// </summary>
        public float GetInterpolatedScale()
        {
            float partialTicks = InterpolatedRenderer.PartialTicks;
            return MathHelper.Lerp(PreviousScale, CurrentScale, partialTicks);
        }
        
        /// <summary>
        /// Gets the interpolated eased progress for VFX timing.
        /// </summary>
        public float GetInterpolatedProgress()
        {
            float partialTicks = InterpolatedRenderer.PartialTicks;
            return MathHelper.Lerp(_previousEasedProgress, _currentEasedProgress, partialTicks);
        }
    }
    
    /// <summary>
    /// Draw layer that renders the interpolated melee weapon with sub-frame smoothness.
    /// Draws BEFORE the vanilla HeldItem layer, which is hidden when we're actively swinging.
    /// </summary>
    public class InterpolatedMeleeDrawLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.HeldItem);
        
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            var player = drawInfo.drawPlayer;
            var swingPlayer = player.GetModPlayer<InterpolatedMeleeSwingPlayer>();
            return swingPlayer.IsSwinging && swingPlayer.HideVanillaWeapon;
        }
        
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            var player = drawInfo.drawPlayer;
            var swingPlayer = player.GetModPlayer<InterpolatedMeleeSwingPlayer>();
            
            if (!swingPlayer.IsSwinging || swingPlayer.CurrentWeapon == null)
                return;
            
            Item item = swingPlayer.CurrentWeapon;
            if (item == null || item.IsAir)
                return;
            
            // Get the weapon texture
            Main.instance.LoadItem(item.type);
            Texture2D texture = TextureAssets.Item[item.type].Value;
            if (texture == null)
                return;
            
            // Update partial ticks for this frame
            InterpolatedRenderer.UpdatePartialTicks();
            
            // Get INTERPOLATED values for buttery-smooth rendering
            float rotation = swingPlayer.GetInterpolatedRotation();
            float scale = swingPlayer.GetInterpolatedScale() * item.scale;
            
            // Handle animated items
            Rectangle? sourceRect = null;
            int frameWidth = texture.Width;
            int frameHeight = texture.Height;
            if (Main.itemAnimations[item.type] != null)
            {
                sourceRect = Main.itemAnimations[item.type].GetFrame(texture);
                frameWidth = sourceRect.Value.Width;
                frameHeight = sourceRect.Value.Height;
            }
            
            // Calculate draw position using drawInfo.Position (like working wing layers)
            // This properly accounts for player position offsets
            Vector2 playerDrawCenter = drawInfo.Position - Main.screenPosition + new Vector2(player.width / 2f, player.height / 2f);
            playerDrawCenter.Y += player.gfxOffY;
            
            // Calculate weapon offset from player center using the interpolated rotation
            Vector2 baseOffset = new Vector2(6f * swingPlayer.SwingDirection, player.gravDir * -2f);
            Vector2 bladeDir = (rotation - MathHelper.PiOver4).ToRotationVector2();
            float reachDistance = 8f + (scale - 1f) / 0.15f * 12f; // Reverse-calculate from scale
            Vector2 weaponOffset = baseOffset + bladeDir * reachDistance;
            
            Vector2 drawPos = (playerDrawCenter + weaponOffset).Floor();
            
            // Origin for held items: the pivot point (handle) is at bottom-left of the sprite
            // This is the point the weapon rotates around - typically where the player holds it
            // For swords swinging, we need the origin at the handle (bottom of sprite)
            Vector2 origin;
            SpriteEffects effects = SpriteEffects.None;
            
            // When facing right, origin is bottom-left (handle)
            // When facing left, we flip horizontally AND move origin to bottom-right
            if (swingPlayer.SwingDirection == -1)
            {
                effects = SpriteEffects.FlipHorizontally;
                origin = new Vector2(frameWidth, frameHeight); // Bottom-right for left-facing
            }
            else
            {
                origin = new Vector2(0, frameHeight); // Bottom-left for right-facing
            }
            
            // Handle gravity inversion
            if (player.gravDir == -1f)
            {
                effects |= SpriteEffects.FlipVertically;
                // Flip origin vertically - if it was at bottom, now at top
                origin.Y = frameHeight - origin.Y;
            }
            
            // Get lighting from world position
            Vector2 worldPos = playerDrawCenter + Main.screenPosition + weaponOffset;
            Color lightColor = Lighting.GetColor((int)(worldPos.X / 16f), (int)(worldPos.Y / 16f));
            
            // DEBUG: Spawn bright dust at the draw position to verify position calculation
            // DISABLED - debug code removed for production
            // if (Main.GameUpdateCount % 3 == 0)
            // {
            //     Dust debug = Dust.NewDustPerfect(worldPos, Terraria.ID.DustID.Torch, Vector2.Zero, 0, Color.Yellow, 2f);
            //     debug.noGravity = true;
            // }
            
            // Create draw data
            DrawData data = new DrawData(
                texture,
                drawPos,
                sourceRect,
                item.GetAlpha(lightColor),
                rotation,
                origin,
                scale,
                effects,
                0
            );
            
            drawInfo.DrawDataCache.Add(data);
        }
    }
}
