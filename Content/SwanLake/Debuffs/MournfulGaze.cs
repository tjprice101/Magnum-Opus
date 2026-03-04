using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.Debuffs
{
    /// <summary>
    /// Mournful Gaze — debuff from The Swan's Lament Destruction Halo.
    /// Enemies afflicted have -15% movement speed.
    /// Visual: faint white teardrop particles drifting downward.
    /// Duration: 300 frames (5 seconds).
    /// </summary>
    public class MournfulGaze : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<MournfulGazeNPC>().HasMournfulGaze = true;
        }
    }

    /// <summary>
    /// GlobalNPC handler for Mournful Gaze debuff effects.
    /// -15% movement speed and teardrop visuals.
    /// </summary>
    public class MournfulGazeNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool HasMournfulGaze { get; set; }

        public override void ResetEffects(NPC npc)
        {
            HasMournfulGaze = false;
        }

        public override void PostAI(NPC npc)
        {
            if (HasMournfulGaze)
            {
                // -15% movement speed
                npc.velocity *= 0.85f;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (!HasMournfulGaze) return;

            // Slightly desaturated tint
            drawColor = Color.Lerp(drawColor, new Color(180, 180, 200), 0.1f);

            // Teardrop particles drifting downward
            if (Main.rand.NextBool(10))
            {
                Vector2 offset = Main.rand.NextVector2Circular(npc.width * 0.3f, npc.height * 0.1f);
                Vector2 spawnPos = npc.Top + offset;
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), 1.2f + Main.rand.NextFloat(0.5f));
                Dust d = Dust.NewDustPerfect(spawnPos, DustID.WhiteTorch, vel, 0,
                    new Color(220, 220, 240), 0.6f);
                d.noGravity = false;
                d.fadeIn = 0.4f;
            }

            // Subtle white haze
            if (Main.rand.NextBool(16))
            {
                Vector2 pos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.4f, npc.height * 0.4f);
                Dust d = Dust.NewDustPerfect(pos, DustID.WhiteTorch, Vector2.Zero, 80,
                    new Color(200, 200, 220), 0.4f);
                d.noGravity = true;
            }
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!HasMournfulGaze) return;

            // Draw a subtle mournful mist above NPC
            float pulse = 0.5f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.08f);

            try
            {
                var glowTex = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;

                if (glowTex != null)
                {
                    Vector2 drawPos = npc.Top - screenPos + new Vector2(0, -8f);
                    Vector2 origin = new Vector2(glowTex.Width / 2f, glowTex.Height / 2f);

                    spriteBatch.Draw(glowTex, drawPos, null,
                        new Color(200, 200, 230, 0) * 0.15f * pulse, 0f, origin, 0.25f, SpriteEffects.None, 0f);
                }
            }
            catch { }
        }
    }
}
