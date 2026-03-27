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
    public class ReverieDrifter : ModNPC
    {
        private static readonly Color LuneBlue = new Color(150, 200, 255);
        private static readonly Color LunePearl = new Color(220, 230, 250);

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
            NPC.damage = 210;
            NPC.defense = 88;
            NPC.lifeMax = 8500;
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.02f;
            NPC.value = Item.buyPrice(gold: 25);
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
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Sky,
                new FlavorTextBestiaryInfoElement(
                    "A drifter of moonlit reverie, its ethereal form woven from night mist and pearl white light. " +
                    "It floats through the upper atmosphere in a state of dreamlike calm.")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (MoonlightSonataSystem.FateBossKilledOnce &&
                (spawnInfo.Player.ZoneSkyHeight || (!Main.dayTime && spawnInfo.Player.ZoneOverworldHeight)))
                return 0.04f;

            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LuneEssence>(), 5, 1, 2));
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            NPC.rotation = NPC.velocity.X * 0.01f;
            float pulse = 0.6f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 1.5f);
            Lighting.AddLight(NPC.Center, LuneBlue.ToVector3() * 0.45f * pulse);

            if (Main.rand.NextBool(6))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.BlueTorch, Main.rand.NextFloat(-0.6f, 0.6f), -0.4f, 100, LunePearl, 0.6f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.WhiteTorch, Main.rand.NextFloat(-0.3f, 0.3f), -0.2f, 120, default, 0.4f);
                dust.noGravity = true;
                dust.velocity *= 0.15f;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 25; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.BlueTorch, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 60, LunePearl, 1.2f);
                }
                for (int i = 0; i < 12; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height,
                        DustID.WhiteTorch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 80, default, 1f);
                }
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            float transparency = 0.75f + 0.15f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2f);
            return Color.Lerp(drawColor, LunePearl, 0.3f) * transparency;
        }
    }
}
