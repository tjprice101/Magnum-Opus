using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.Fate.Enemies
{
    public class CelestialArbiter : ModNPC
    {
        private static readonly Color FateCrimson = new Color(180, 40, 80);
        private static readonly Color FateWhite = new Color(240, 220, 240);

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new()
            {
                Velocity = 1f
            };
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = bestiaryData;
        }

        public override void SetDefaults()
        {
            NPC.width = 130;
            NPC.height = 77;
            NPC.damage = 150;
            NPC.defense = 66;
            NPC.lifeMax = 4500;
            NPC.HitSound = SoundID.NPCHit36;
            NPC.DeathSound = SoundID.NPCDeath39;
            NPC.knockBackResist = 0.05f;
            NPC.value = Item.buyPrice(gold: 12);
            NPC.aiStyle = NPCAIStyleID.HoveringFighter;
            AIType = NPCID.Pixie;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.scale = 0.65f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCorruption,
                new FlavorTextBestiaryInfoElement(
                    "A celestial arbiter of fate, wreathed in dark pink energy and crimson starlight. " +
                    "Ancient glyphs orbit its form, humming with the weight of cosmic inevitability.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (NPC.downedMoonlord &&
                (spawnInfo.Player.ZoneCorrupt || spawnInfo.Player.ZoneCrimson))
                return 0.045f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FateEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            NPC.rotation = NPC.velocity.X * 0.02f;
            float pulse = 0.6f + 0.4f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(NPC.Center, FateCrimson.ToVector3() * 0.45f * pulse);

            if (Main.rand.NextBool(6))
            {
                Color[] fateColors = { new Color(180, 50, 100), new Color(255, 60, 80), Color.White };
                Color c = fateColors[Main.rand.Next(fateColors.Length)];
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.PinkTorch, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f), 80, c, 0.7f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(NPC.Center + Main.rand.NextVector2Circular(40, 40),
                    4, 4, DustID.Enchanted_Gold, 0f, 0f, 100, FateWhite, 0.4f);
                dust.noGravity = true;
                dust.velocity *= 0.1f;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 25; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.PinkTorch, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 40, default, 1.4f);
                }
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.Enchanted_Gold, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 60, Color.White, 0.8f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            float flicker = 0.85f + 0.15f * MathF.Sin(Main.GlobalTimeWrappedHourly * 4f);
            return Color.Lerp(drawColor, FateCrimson, 0.2f) * flicker;
        }
    }
}
