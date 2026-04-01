using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.EnemyDrops;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.ClairDeLune.Enemies
{
    public class NocturneShade : ModNPC
    {
        public override string Texture => "MagnumOpus/Content/ClairDeLune/Enemies/NocturneShade/NocturneShade";

        private static readonly Color NightMist = new Color(100, 140, 200);
        private static readonly Color MoonGlow = new Color(180, 210, 255);

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
            NPC.damage = 230;
            NPC.defense = 92;
            NPC.lifeMax = 10000;
            NPC.HitSound = SoundID.NPCHit36;
            NPC.DeathSound = SoundID.NPCDeath39;
            NPC.knockBackResist = 0.01f;
            NPC.value = Item.buyPrice(gold: 28);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            AIType = NPCID.Herpling;
            NPC.scale = 0.65f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Sky,
                new FlavorTextBestiaryInfoElement(
                    "A shade of the nocturne, its silhouette cut from the fabric of night itself. " +
                    "Soft blue luminescence traces its edges like moonlight on still water.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (MoonlightSonataSystem.FateBossKilledOnce &&
                (spawnInfo.Player.ZoneSkyHeight || (!Main.dayTime && spawnInfo.Player.ZoneOverworldHeight)))
                return 0.035f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LuneEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            float pulse = 0.5f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 1.8f);
            Lighting.AddLight(NPC.Center, NightMist.ToVector3() * 0.35f * pulse);

            if (Main.rand.NextBool(7))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.BlueTorch, Main.rand.NextFloat(-0.7f, 0.7f), -0.5f, 120, default, 0.6f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(15))
            {
                Dust dust = Dust.NewDustDirect(NPC.Center + Main.rand.NextVector2Circular(NPC.width / 2, NPC.height / 2),
                    4, 4, DustID.DungeonSpirit, 0f, 0f, 150, MoonGlow, 0.4f);
                dust.noGravity = true;
                dust.velocity *= 0.1f;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 28; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.BlueTorch, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 60, default, 1.3f);
                }
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.DungeonSpirit, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 80, MoonGlow, 0.9f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Color.Lerp(drawColor, NightMist, 0.2f);
        }
    }
}
