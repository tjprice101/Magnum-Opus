using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.OdeToJoy.Systems;

namespace MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Projectiles
{
    /// <summary>
    /// Hymn of the Victorious — Four-Verse Cycle magic orb launcher.
    /// Cycles: Exordium (1 orb, 8f) → Rising (2 orbs, 14f) → Apex (3 orbs, 18f, homing) → Gloria (1 pierce, accel)
    /// Resonance stacks increase orb count per verse.
    /// </summary>
    public class HymnVictoriousSwing : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private int _versePhase = 0; // 0=Exordium, 1=Rising, 2=Apex, 3=Gloria
        private float _verseTimer = 0f;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600; // Long lifetime for cycling
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            var combatPlayer = owner.GetModPlayer<OdeToJoyCombatPlayer>();

            // Advance timer and check for verse transitions
            _verseTimer += 1f;

            // Verse durations: Exordium 8f, Rising 14f, Apex 18f, Gloria 12f (accelerating)
            int[] verseDurations = { 8, 14, 18, 12 };
            if (_verseTimer >= verseDurations[_versePhase])
            {
                _verseTimer = 0f;

                // Fire the verse sequence based on current phase
                FireVerse(owner, combatPlayer);

                // Advance to next verse
                _versePhase = (_versePhase + 1) % 4;
                if (_versePhase == 0)
                {
                    // Completed a cycle — increment resonance
                    combatPlayer.AdvanceHymnVerse();
                }
            }

            // Light and VFX
            Color verseColor = GetVerseColor(_versePhase);
            OdeToJoyVFXLibrary.AddOdeToJoyLight(owner.MountedCenter, 0.5f);

            // Idle music note shimmer
            if (_verseTimer % 3 == 0)
            {
                OdeToJoyVFXLibrary.SpawnMusicNotes(owner.MountedCenter, 1, 8f, 0.6f, 0.8f, 20);
            }
        }

        private void FireVerse(Player owner, OdeToJoyCombatPlayer combatPlayer)
        {
            var source = Projectile.GetSource_FromThis();
            int orbCount = 0;
            float speed = 0f;
            float homing = 0f;
            int flags = GenericHomingOrbChild.FLAG_ACCELERATE;
            float scale = 1f;

            // Resonance bonus: +1 orb per verse if resonance >= 3
            int resonanceBonus = (combatPlayer.HymnResonanceStacks >= 3) ? 1 : 0;

            switch (_versePhase)
            {
                case 0: // Exordium: 1 orb, 8f speed
                    orbCount = 1 + resonanceBonus;
                    speed = 8f;
                    homing = 0f;
                    break;

                case 1: // Rising: 2 orbs, 14f speed
                    orbCount = 2 + resonanceBonus;
                    speed = 14f;
                    homing = 0f;
                    break;

                case 2: // Apex: 3 orbs, 18f speed, 0.12 homing
                    orbCount = 3 + resonanceBonus;
                    speed = 18f;
                    homing = 0.12f;
                    break;

                case 3: // Gloria: 1 pierce orb, accelerating 8→24f, 2x scale
                    orbCount = 1;
                    speed = 8f;
                    homing = 0.06f;
                    scale = 2f;
                    flags |= GenericHomingOrbChild.FLAG_PIERCE;
                    break;
            }

            // Find target or use mouse direction
            NPC target = FindClosestNPC(500f);
            Vector2 direction = target != null
                ? (target.Center - owner.MountedCenter).SafeNormalize(Vector2.UnitX)
                : (Main.MouseWorld - owner.MountedCenter).SafeNormalize(Vector2.UnitX);

            // Spread orbs in a fan pattern
            for (int i = 0; i < orbCount; i++)
            {
                float spreadAngle = (orbCount > 1)
                    ? MathHelper.Lerp(-0.3f, 0.3f, (float)i / (orbCount - 1))
                    : 0f;

                Vector2 orbVel = direction.RotatedBy(spreadAngle) * speed;

                GenericHomingOrbChild.SpawnChild(
                    source,
                    owner.MountedCenter, orbVel,
                    Projectile.damage, Projectile.knockBack, owner.whoAmI,
                    homingStrength: homing,
                    behaviorFlags: flags,
                    themeIndex: GenericHomingOrbChild.THEME_ODETOJOY,
                    scaleMult: scale,
                    timeLeft: 120);
            }

            // Impact VFX
            OdeToJoyVFXLibrary.SpawnMusicNotes(owner.MountedCenter, orbCount + 2, 15f, 0.7f, 1f, 25);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                Vector2 drawPos = owner.MountedCenter - Main.screenPosition;
                float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3.5f);
                Color verseCol = GetVerseColor(_versePhase);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                Texture2D glow = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Vector2 origin = glow.Size() / 2f;

                // Verse aura: outer ring in verse color, white-hot core
                sb.Draw(glow, drawPos, null, (verseCol with { A = 0 }) * 0.35f * pulse,
                    0f, origin, 0.60f, SpriteEffects.None, 0f);
                sb.Draw(glow, drawPos, null, (OdeToJoyPalette.GoldenPollen with { A = 0 }) * 0.30f * pulse,
                    0f, origin, 0.30f, SpriteEffects.None, 0f);
                sb.Draw(glow, drawPos, null, (OdeToJoyPalette.WhiteBloom with { A = 0 }) * 0.45f,
                    0f, origin, 0.10f, SpriteEffects.None, 0f);
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        private Color GetVerseColor(int verseIndex)
        {
            return verseIndex switch
            {
                0 => Color.Lerp(OdeToJoyPalette.LeafGreen, OdeToJoyPalette.GoldenPollen, 0.3f),
                1 => Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.RosePink, 0.4f),
                2 => OdeToJoyPalette.SunlightYellow,
                3 => Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.WhiteBloom, 0.6f),
                _ => OdeToJoyPalette.GoldenPollen,
            };
        }

        private NPC FindClosestNPC(float maxDist)
        {
            NPC closest = null;
            float closestDist = maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Main.player[Projectile.owner].MountedCenter, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        public override void OnKill(int timeLeft)
        {
            OdeToJoyVFXLibrary.FinisherSlam(Main.player[Projectile.owner].MountedCenter, 0.6f);
        }
    }
}
