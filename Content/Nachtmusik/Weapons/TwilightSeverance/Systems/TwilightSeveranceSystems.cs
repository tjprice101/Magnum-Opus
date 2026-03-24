using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Projectiles;
using MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Systems
{
    public sealed class TwilightSeveranceMarkedNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int MarkStacks;
        public int MarkTimer;

        public override void PostAI(NPC npc)
        {
            if (MarkStacks <= 0)
                return;

            MarkTimer--;
            if (MarkTimer <= 0)
            {
                MarkStacks = 0;
                MarkTimer = 0;
            }
        }

        public void AddMark(int frames, int amount)
        {
            MarkStacks = Utils.Clamp(MarkStacks + amount, 0, 3);
            MarkTimer = Utils.Clamp(frames, 1, 600);
        }

        public int ConsumeAll()
        {
            int consumed = MarkStacks;
            MarkStacks = 0;
            MarkTimer = 0;
            return consumed;
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (MarkStacks <= 0 || Main.dedServ)
                return;

            DrawMarkStars(npc, spriteBatch, screenPos);
        }

        private void DrawMarkStars(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos)
        {
            Texture2D starTex = MagnumTextureRegistry.GetStar4Soft();
            if (starTex == null)
                return;

            Vector2 origin = starTex.Size() * 0.5f;
            float t = (float)Main.timeForVisualEffects;
            float pulse = 0.8f + 0.2f * MathHelper.Clamp((float)System.Math.Sin(t * 3.5f), 0f, 1f);
            float baseY = npc.position.Y - 16f - screenPos.Y;
            float centerX = npc.Center.X - screenPos.X;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float baseScale = 0.12f + MarkStacks * 0.03f;
            float opacity = 0.4f + MarkStacks * 0.15f;
            Color starColor = Color.Lerp(
                NachtmusikPalette.StarlitBlue,
                NachtmusikPalette.MoonlitSilver,
                (MarkStacks - 1) / 2f) * opacity;
            starColor.A = 0;

            if (MarkStacks == 1)
            {
                float rot = t * 0.8f;
                spriteBatch.Draw(starTex, new Vector2(centerX, baseY), null, starColor * pulse,
                    rot, origin, baseScale * pulse, SpriteEffects.None, 0f);
            }
            else if (MarkStacks == 2)
            {
                for (int i = -1; i <= 1; i += 2)
                {
                    float rot = t * 0.8f + i * 0.4f;
                    Vector2 pos = new Vector2(centerX + i * 7f, baseY);
                    spriteBatch.Draw(starTex, pos, null, starColor * pulse,
                        rot, origin, baseScale * pulse, SpriteEffects.None, 0f);
                }
            }
            else
            {
                float triRadius = 8f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = MathHelper.TwoPi / 3f * i - MathHelper.PiOver2;
                    Vector2 offset = new Vector2(
                        (float)System.Math.Cos(angle) * triRadius,
                        (float)System.Math.Sin(angle) * triRadius * 0.6f);
                    float rot = t * 1.0f + i * 0.7f;
                    spriteBatch.Draw(starTex, new Vector2(centerX + offset.X, baseY + offset.Y), null,
                        starColor * pulse, rot, origin, baseScale * pulse, SpriteEffects.None, 0f);
                }
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }

    public static class TwilightSeveranceCombat
    {
        public static void ApplyMark(NPC target, int baseDurationFrames)
        {
            if (target == null || !target.active || target.friendly)
                return;

            TwilightSeveranceMarkedNPC mark = target.GetGlobalNPC<TwilightSeveranceMarkedNPC>();
            mark.AddMark(baseDurationFrames, 1);
        }

        public static bool HasMarkedTargets(Player player, float radius)
        {
            Vector2 center = player.Center;
            float r2 = radius * radius;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;

                if (Vector2.DistanceSquared(center, npc.Center) > r2)
                    continue;

                if (npc.GetGlobalNPC<TwilightSeveranceMarkedNPC>().MarkStacks > 0)
                    return true;
            }

            return false;
        }

        public static int ConsumeMarkedTargets(Player player, Vector2 focusPosition, float radius,
            int baseDamage, float knockback, IEntitySource source)
        {
            float r2 = radius * radius;
            int consumedTotal = 0;

            for (int pass = 3; pass >= 1; pass--)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage)
                        continue;

                    if (Vector2.DistanceSquared(focusPosition, npc.Center) > r2)
                        continue;

                    TwilightSeveranceMarkedNPC mark = npc.GetGlobalNPC<TwilightSeveranceMarkedNPC>();
                    if (mark.MarkStacks != pass)
                        continue;

                    int consumed = mark.ConsumeAll();
                    if (consumed <= 0)
                        continue;

                    consumedTotal += consumed;

                    Vector2 dir = (npc.Center - player.Center).SafeNormalize(Vector2.UnitX);
                    int strikeDamage = (int)(baseDamage * (0.34f + consumed * 0.22f));
                    Projectile.NewProjectile(source, player.Center, dir * 22f,
                        ModContent.ProjectileType<TwilightSlashProjectile>(),
                        strikeDamage, knockback, player.whoAmI, ai0: consumed, ai1: npc.whoAmI);

                    TwilightSeveranceVFX.ConstellationBreakTargetVFX(npc.Center, consumed);
                }
            }

            return consumedTotal;
        }
    }
}
