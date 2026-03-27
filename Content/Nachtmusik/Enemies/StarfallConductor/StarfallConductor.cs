using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.EnemyDrops;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Nachtmusik.Enemies
{
    public class StarfallConductor : ModNPC
    {
        private static readonly Color NachtIndigo = new Color(100, 120, 200);
        private static readonly Color StarSilver = new Color(200, 220, 255);

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
            NPC.damage = 165;
            NPC.defense = 72;
            NPC.lifeMax = 5500;
            NPC.HitSound = SoundID.NPCHit36;
            NPC.DeathSound = SoundID.NPCDeath39;
            NPC.knockBackResist = 0.04f;
            NPC.value = Item.buyPrice(gold: 15);
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
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement(
                    "A conductor of falling stars, its indigo robes trailing cosmic silver. " +
                    "Starlight streams from its baton as it orchestrates the nocturnal sky.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (MoonlightSonataSystem.FateBossKilledOnce &&
                !Main.dayTime &&
                spawnInfo.Player.ZoneOverworldHeight)
                return 0.05f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<NachtmusikEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            NPC.rotation = NPC.velocity.X * 0.015f;
            float pulse = 0.6f + 0.35f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2.5f);
            Lighting.AddLight(NPC.Center, NachtIndigo.ToVector3() * 0.45f * pulse);

            if (Main.rand.NextBool(6))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.BlueTorch, Main.rand.NextFloat(-1f, 1f), -0.8f, 80, default, 0.7f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Enchanted_Gold, Main.rand.NextFloat(-0.5f, 0.5f), -0.5f, 50, StarSilver, 0.5f);
                dust.noGravity = true;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 25; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.BlueTorch, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 40, default, 1.3f);
                }
                for (int i = 0; i < 12; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.Enchanted_Gold, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 50, StarSilver, 0.9f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Color.Lerp(drawColor, NachtIndigo, 0.2f);
        }
    }
}
