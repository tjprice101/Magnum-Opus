using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.EnemyDrops;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.DiesIrae.Enemies
{
    public class PyreCantor : ModNPC
    {
        public override string Texture => "MagnumOpus/Content/DiesIrae/Enemies/PyreCantor/PyreCantor";

        private static readonly Color PyreCrimson = new Color(180, 30, 20);
        private static readonly Color PyreEmber = new Color(240, 100, 30);

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
            NPC.damage = 185;
            NPC.defense = 78;
            NPC.lifeMax = 7000;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0.03f;
            NPC.value = Item.buyPrice(gold: 19);
            NPC.aiStyle = NPCAIStyleID.HoveringFighter;
            AIType = NPCID.Pixie;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.lavaImmune = true;
            NPC.scale = 0.65f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld,
                new FlavorTextBestiaryInfoElement(
                    "A cantor of the pyre, chanting hymns of wrath in tongues of fire. " +
                    "Its crimson robes billow with superheated air as it floats above lakes of lava.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (MoonlightSonataSystem.FateBossKilledOnce &&
                (spawnInfo.Player.ZoneUnderworldHeight || spawnInfo.Player.ZoneDungeon))
                return 0.045f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WrathEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            NPC.rotation = NPC.velocity.X * 0.015f;
            float flicker = Main.rand.NextFloat(0.9f, 1.1f);
            float pulse = 0.65f + 0.35f * MathF.Sin(Main.GlobalTimeWrappedHourly * 4f);
            Lighting.AddLight(NPC.Center, PyreCrimson.ToVector3() * 0.5f * pulse * flicker);

            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.CrimsonTorch, Main.rand.NextFloat(-1.2f, 1.2f), -1f, 60, default, 0.9f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(7))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.Torch, Main.rand.NextFloat(-0.8f, 0.8f), -0.8f, 80, default, 0.7f);
                dust.noGravity = true;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 30; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.Torch : DustID.CrimsonTorch;
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        dustType, Main.rand.NextFloat(-7f, 7f), Main.rand.NextFloat(-7f, 7f), 40, default, 1.4f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            float flicker = 0.85f + 0.15f * MathF.Sin(Main.GlobalTimeWrappedHourly * 5f);
            return Color.Lerp(drawColor, PyreEmber, 0.2f) * flicker;
        }
    }
}
