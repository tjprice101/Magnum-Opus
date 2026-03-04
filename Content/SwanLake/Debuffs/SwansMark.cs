using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.Debuffs
{
    /// <summary>
    /// Swan's Mark — debuff from Call of the Black Swan.
    /// Marked enemies have -10 defense. Visual: black feather stuck to enemy.
    /// Duration: 300 frames (5 seconds).
    /// </summary>
    public class SwansMark : ModBuff
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
            npc.GetGlobalNPC<SwansMarkNPC>().HasSwansMark = true;
        }
    }

    /// <summary>
    /// GlobalNPC handler for Swan's Mark debuff effects.
    /// -10 defense and black feather visual.
    /// </summary>
    public class SwansMarkNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool HasSwansMark { get; set; }
        private float _featherTimer;

        public override void ResetEffects(NPC npc)
        {
            HasSwansMark = false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (HasSwansMark)
            {
                // -10 defense applied in ModifyHitByProjectile/Item
            }
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (HasSwansMark)
            {
                modifiers.Defense.Flat -= 10;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (!HasSwansMark) return;

            // Darken the enemy sprite slightly
            drawColor = Color.Lerp(drawColor, new Color(40, 40, 50), 0.15f);

            _featherTimer += 0.05f;

            // Spawn dark feather particles periodically
            if (Main.rand.NextBool(12))
            {
                Vector2 offset = Main.rand.NextVector2Circular(npc.width * 0.4f, npc.height * 0.4f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.8f - Main.rand.NextFloat(0.5f));
                Dust d = Dust.NewDustPerfect(npc.Center + offset, DustID.Shadowflame, vel, 0,
                    new Color(15, 15, 25), 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Subtle prismatic edge glint
            if (Main.rand.NextBool(20))
            {
                float hue = Main.rand.NextFloat();
                Color prismatic = Main.hslToRgb(hue, 0.9f, 0.8f);
                Vector2 pos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.5f, npc.height * 0.5f);
                Dust d = Dust.NewDustPerfect(pos, DustID.RainbowTorch, Vector2.Zero, 0, prismatic, 0.4f);
                d.noGravity = true;
            }
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!HasSwansMark) return;

            // Draw a subtle dark mark indicator above the NPC
            float pulse = 0.6f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.1f);

            try
            {
                var markTex = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;

                if (markTex != null)
                {
                    Vector2 drawPos = npc.Top - screenPos + new Vector2(0, -12f);
                    Vector2 origin = new Vector2(markTex.Width / 2f, markTex.Height / 2f);

                    spriteBatch.Draw(markTex, drawPos, null,
                        new Color(15, 15, 25, 0) * 0.4f * pulse, 0f, origin, 0.2f, SpriteEffects.None, 0f);
                    spriteBatch.Draw(markTex, drawPos, null,
                        new Color(200, 180, 255, 0) * 0.2f * pulse, 0f, origin, 0.35f, SpriteEffects.None, 0f);
                }
            }
            catch { }
        }
    }
}
