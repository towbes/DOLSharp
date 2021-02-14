/* Summon Buffbot Command - Created By Deathwish, with a BIG THANKS to geshi for his help!
 * Version 1.0 (13/07/2010) For the use of all Dol Members.
 * 
 * Please share any updates or changes.
 * 
 * This script will summon a Buffbot for the cost of 5bps, that lasts 30 seconds.
 * This script contains everything you need to run the script.
 * 
 * How to use: Place script into your scripts folder.
 * InGame Use: /bb to summon (5 bps is needed to summon or you cant summon the Buffbot)
 * 
 * To change Buffbots name guild etc see line 204.
 * (I am not the creator of the buffbot script i have added to this script,
 * its only there for people that dont have a working bb and to make life easier for people that cant use C#!)
 * 
 * Update V1.1 (27/07/10)
 * Now summon buffbot will load in rvr zones, also remove the loading up error
 * 
 * Updated V1.2 (02/08/10)
 * Added a timer for 30 sec so player cant abuse the script.
 * 
 * Added check to to prevent recasting of buffs on buffed player
 * Removed the Bounty Cost
*/

using DOL.Database;
using DOL.GS.Housing;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


#region

using System;
using System.Collections;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

#endregion

namespace DOL.GS.Commands
{
    [Cmd(
        "&tp",
        ePrivLevel.Player, // Set to player.
        "/tp - To Summon a Buffbot for the cost of 50bps")]
    public class SummontpCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        [ScriptLoadedEvent]
        public static void OnScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            Spell load;
            load = TpBotSpell;
        }

        #region Command Timer

        public const string SummonBuff = "SummonBuff";

        public void OnCommand(GameClient client, string[] args)
        {
            var player = client.Player;
            var buffTick = player.TempProperties.getProperty(SummonBuff, 0L);
            var changeTime = player.CurrentRegion.Time - buffTick;
            if (changeTime < 30000)
            {
                player.Out.SendMessage(
                    "You must wait " + ((30000 - changeTime) / 1000) + " more second to attempt to use this command!",
                    eChatType.CT_System, eChatLoc.CL_ChatWindow);
                return;
            }
            player.TempProperties.setProperty(SummonBuff, player.CurrentRegion.Time);

            #endregion Command timer

            #region Command spell Loader   
            if (client.Player.BountyPoints >= 0) // how many bps are need to summon the buffbot
            {

                var line = new SpellLine("TpBotCast", "TpBot Cast", "unknown", false);
                var spellHandler = ScriptMgr.CreateSpellHandler(client.Player, TpBotSpell, line);
                if (spellHandler != null)
                    spellHandler.StartSpell(client.Player);
                client.Player.RemoveBountyPoints(0); // removes the amount of bps from the player
                client.Player.Out.SendMessage("You have summoned a Teleporter!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                client.Player.SaveIntoDatabase(); // saves new amount of bps
                client.Player.Out.SendUpdatePlayer(); // updates players bp
            }

            #endregion command spell loader
            else client.Player.Out.SendMessage("You don't have enough Bounty Pounts to summon a Teleporter!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
        }

        #region Spell

        protected static Spell MTpBotSpell;

        public static Spell TpBotSpell
        {
            get
            {
                if (MTpBotSpell == null)
                {
                    var spell = new DBSpell { CastTime = 0, ClientEffect = 0, Duration = 15 };
                    spell.Description = "Summons a Teleporter to your location for " + spell.Duration + " seconds.";
                    spell.Name = "Teleport Spell";
                    spell.Type = "Summon A Teleporter";
                    spell.Range = 0;
                    spell.SpellID = 121233;
                    spell.Target = "Self";
                    spell.Value =TpBotTemplate.TemplateId;
                    MTpBotSpell = new Spell(spell, 2);
                    SkillBase.GetSpellList(GlobalSpellsLines.Item_Effects).Add(MTpBotSpell);
                }
                return MTpBotSpell;
            }
        }

        #endregion

        #region Npc

        protected static NpcTemplate MTpBotTemplate;

        public static NpcTemplate TpBotTemplate
        {
            get
            {
                if (MTpBotTemplate == null)
                {
					MTpBotTemplate = new NpcTemplate();
					MTpBotTemplate.Flags += (byte)GameNPC.eFlags.GHOST + (byte)GameNPC.eFlags.PEACE;
					MTpBotTemplate.Name = "Teleporter";
					MTpBotTemplate.ClassType = "DOL.GS.Scripts.SummonedTeleporter";
					MTpBotTemplate.Model = "50";
					MTpBotTemplate.TemplateId = 93050;
                    NpcTemplateMgr.AddTemplate(MTpBotTemplate);
                }
                return MTpBotTemplate;
            }
        }

        #endregion
    }
}

namespace DOL.GS.Scripts
{
	public class SummonedTeleporter : GameTeleporter
	{


		public override bool AddToWorld()
		{
			switch (Realm)
			{
				case eRealm.Albion: Model = 10; break;
				case eRealm.Hibernia: Model = 307; break;
				case eRealm.Midgard: Model = 158; break;
				case eRealm.None: Model = 10; break;
			}

			GuildName = "Powered By Dawn Of Light";
			Realm = eRealm.None;
			return base.AddToWorld();
		}

		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public String DisplayTeleportDestinations(eRealm destRealm)
		{
			StringBuilder sRet = new StringBuilder("");

			switch (destRealm)
			{
				case eRealm.Albion:
					sRet.Append("Would you like to teleport to?\n[Camelot]\n[Albion Frontiers] or [Battlegrounds]\n[Albion Darkness Falls]\n");
					sRet.Append("[Albion Mainland]\n[Albion Dungeons]\n[Albion Shrouded Isles]\n[Albion Oceanus]\n");
					sRet.Append("[Housing]\n[Hibernia] or [Midgard]");
					break;
				case eRealm.Midgard:
					sRet.Append("Would you like to teleport to?\n[Jordheim]\n[Midgard Frontiers] or [Battlegrounds]\n[Midgard Darkness Falls]\n");
					sRet.Append("[Midgard Mainland]\n[Midgard Dungeons]\n[Midgard Shrouded Isles]\n[Midgard Oceanus]\n");
					sRet.Append("[Housing]\n[Albion] or [Hibernia]");
					break;
				case eRealm.Hibernia:
					sRet.Append("Would you like to teleport to?\n[Tir na Nog]\n[Hibernia Frontiers] or [Battlegrounds]\n[Hibernia Darkness Falls]\n");
					sRet.Append("[Hibernia Mainland]\n[Hibernia Dungeons]\n[Hibernia Shrouded Isles]\n[Hibernia Oceanus]\n");
					sRet.Append("[Housing]\n[Albion] or [Midgard]");
					break;
				default:
					log.Warn(String.Format("DisplayTeleportDestinations does not handle player realm [{0}]", destRealm.ToString()));
					break;
			}

			return sRet.ToString();
		}

		/// <summary>
		/// Player right-clicked the teleporter.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			String intro = DisplayTeleportDestinations(player.Realm);
			if (intro != null)
				SayTo(player, intro);

			if (!base.Interact(player))
				return false;

			return true;
		}

		/// <summary>
		/// Talk to the teleporter.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public override bool WhisperReceive(GameLiving source, string text)
		{
			GamePlayer player = source as GamePlayer;
			if (player == null)
				return false;

			eRealm realmTarget = player.Realm;

			StringBuilder sRet = new StringBuilder();

			switch (text.ToUpper())
			{
				// Realm specific menus
				case "ALBION":
					SayTo(player, DisplayTeleportDestinations(eRealm.Albion));
					return true;
				case "MIDGARD":
					SayTo(player, DisplayTeleportDestinations(eRealm.Midgard));
					return true;
				case "HIBERNIA":
					SayTo(player, DisplayTeleportDestinations(eRealm.Hibernia));
					return true;

				case "ALBION FRONTIERS":
					sRet.Append("Where in the frontiers would you like to go?\n[Forest Sauvage]\n[Castle Sauvage]\n[Snowdonia Fortress]\n");
					sRet.Append("[Albion Agramon]");
					SayTo(player, sRet.ToString());
					return true;
				case "ALBION MAINLAND":
					sRet.Append("Where in Albion would you like to go?\n");
					if (!ServerProperties.Properties.DISABLE_TUTORIAL && player.Level <= 15)
						sRet.Append("[Holtham] (Levels 1-9)\n");
					sRet.Append("[Cotswold Village] (Levels 10-14)\n[Prydwen Keep] (Levels 15-19)\n");
					sRet.Append("[Caer Ulfwych] (Levels 20-24)\n[Campacorentin Station] (Levels 25-29)\n[Adribard's Retreat] (Levels 30-34)\n");
					sRet.Append("[Cornwall Station] (Levels 35+)\n[Swanton Keep] (Levels 35+)\n[Lyonesse] (Levels 45+)\n[Dartmoor] (Levels 45+) \n");
					sRet.Append("[Inconnu Crypt]");
					SayTo(player, sRet.ToString());
					return true;
				case "ALBION DUNGEONS":
					sRet.Append("Which dungeon would you like to teleport to?\n");
					sRet.Append("[Tomb of Mithra] (Levels 10-18)\n[Keltoi Fogou] (Levels 18-26)\n[Tepok's Mine] (Levels 26-34)\n");
					sRet.Append("[Catacombs of Cardova] (Levels 34-42)\n[Stonehenge Barrows] (Levels 42-50)\n");
					sRet.Append("[Krondon] (Levels 50+)\n[Avalon City] (Epic)\n[Caer Sidi] (Epic)");
					SayTo(player, sRet.ToString());
					return true;
				case "ALBION SHROUDED ISLES":
					sRet.Append("Where in Avalon would you like to go?\n");
					sRet.Append("[Caer Gothwaite]\n[Wearyall Village]\n[Fort Gwyntell]\n[Caer Diogel]");
					SayTo(player, sRet.ToString());
					return true;

				case "MIDGARD FRONTIERS":
					sRet.Append("Where in the frontiers would you like to go?\n[Uppland]\n[Svasud Faste]\n[Vindsaul Faste]\n");
					sRet.Append("[Midgard Agramon]");
					SayTo(player, sRet.ToString());
					return true;
				case "MIDGARD MAINLAND":
					sRet.Append("Where in Midgard would you like to go?\n");
					if (!ServerProperties.Properties.DISABLE_TUTORIAL && player.Level <= 15)
						sRet.Append("[Hafheim] (Levels 1-9)\n");
					sRet.Append("[Mularn] (Levels 10-14)\n[Fort Veldon] (Levels 15-19)\n");
					sRet.Append("[Audliten] (Levels 20-24)\n[Huginfell] (Levels 25-29)\n[Fort Atla] (Levels 30-34)\n");
					sRet.Append("[Gna Faste] (Levels 35+)\n[Vindsaul Faste] (Levels 35+)\n[Raumarik] (Levels 45+)\n[Malmohus] (Levels 45+)\n");
					sRet.Append("[Kobold Undercity]");
					SayTo(player, sRet.ToString());
					return true;
				case "MIDGARD DUNGEONS":
					sRet.Append("Which dungeon would you like to teleport to?\n");
					sRet.Append("[Nisse's Lair] (Levels 10-18)\n[Cursed Tomb] (Levels 18-26)\n[Vendo Caverns] (Levels 26-34)\n");
					sRet.Append("[Varulvhamn] (Levels 34-42)\n[Spindelhalla ] (Levels 42-50),\n");
					sRet.Append("[Iarnvidiur's Lair] (Levels 50+)\n[Trollheim] (Epic)\n[Tuscaren Glacier] (Epic)");
					SayTo(player, sRet.ToString());
					return true;
				case "MIDGARD SHROUDED ISLES":
					sRet.Append("Where in Aegir would you like to go?\n");
					sRet.Append("[Aegirhamn]\n[Bjarken]\n[Hagall]\n[Knarr]");
					SayTo(player, sRet.ToString());
					return true;

				case "HIBERNIA FRONTIERS":
					sRet.Append("Where in the frontiers would you like to go?\n[Cruachan Gorge]\n[Druim Ligen]\n[Druim Cain]\n");
					sRet.Append("[Hibernia Agramon]");
					SayTo(player, sRet.ToString());
					return true;
				case "HIBERNIA MAINLAND":
					sRet.Append("Where in Hibernia would you like to go?\n");
					if (!ServerProperties.Properties.DISABLE_TUTORIAL && player.Level <= 15)
						sRet.Append("[Fintain] (Levels 1-9)\n");
					sRet.Append("[Mag Mell] (Levels 10-14)\n[Tir na mBeo] (Levels 15-19)\n");
					sRet.Append("[Ardagh] (Levels 20-24)\n[Howth] (Levels 25-29)\n[Connla] (Levels 30-34)\n");
					sRet.Append("[Innis Carthaig] (Levels 35+)\n[Druim Cain] (Levels 35+)\n[Cursed Forest] (Levels 45+)\n[Sheeroe Hills] (Levels 45+)\n");
					sRet.Append("[Shar Labyrinth]");
					SayTo(player, sRet.ToString());
					return true;
				case "HIBERNIA DUNGEONS":
					sRet.Append("Which dungeon would you like to teleport to?\n");
					sRet.Append("[Muire Tomb] (Levels 10-18)\n[Spraggon Den] (Levels 18-26)\n[Koalinth Caverns] (Levels 26-34)\n");
					sRet.Append("[Treibh Caillte] (Levels 34-42)\n[Coruscating Mine] (Levels 42-50)\n");
					sRet.Append("[Tur Suil] (Levels 50+)\n[Fomor] (Epic)\n[Galladoria] (Epic)");
					SayTo(player, sRet.ToString());
					return true;

				case "HIBERNIA SHROUDED ISLES":
					sRet.Append("Where in Hy Brasil would you like to go?\n");
					sRet.Append("[Domnann]\n[Droighaid]\n[Aalid Feie]\n[Necht]");
					SayTo(player, sRet.ToString());
					return true;

				case "HOUSING":
					sRet.Append("\nI can send you to:\n Your [personal] house, if you have one,\n");
					sRet.Append("The housing [entrance],\nYour [guild] house,\nor your [Hearth] bind.");
					SayTo(player, sRet.ToString());
					return true;

				// DF locations
				case "ALBION DARKNESS FALLS":
					realmTarget = eRealm.Albion;
					text = "Darkness Falls";
					break;
				case "MIDGARD DARKNESS FALLS":
					realmTarget = eRealm.Midgard;
					text = "Darkness Falls";
					break;
				case "HIBERNIA DARKNESS FALLS":
					realmTarget = eRealm.Hibernia;
					text = "Darkness Falls";
					break;

				// Agramon
				case "ALBION AGRAMON":
					realmTarget = eRealm.Albion;
					text = "Agramon";
					break;
				case "MIDGARD AGRAMON":
					realmTarget = eRealm.Midgard;
					text = "Agramon";
					break;
				case "HIBERNIA AGRAMON":
					realmTarget = eRealm.Hibernia;
					text = "Agramon";
					break;

				// Albion destinations
				case "CAMELOT":
					SayTo(player, "The great city awaits!");
					realmTarget = eRealm.Albion;
					break;
				case "ALBION OCEANUS":
					if (player.Client.Account.PrivLevel < ServerProperties.Properties.ATLANTIS_TELEPORT_PLVL)
					{
						SayTo(player, "I'm sorry, but you are not authorized to enter Atlantis at this time.");
						return true;
					}
					SayTo(player, "You will soon arrive in the Haven of Oceanus.");
					realmTarget = eRealm.Albion;
					text = "Oceanus";
					break;
				// SI cities
				case "GOTHWAITE":
				case "DIOGEL":
				case "GWYNTELL":
				case "WEARYALL":
					SayTo(player, "The Shrouded Isles await you.");
					realmTarget = eRealm.Albion;
					break;
				// Mainland destinations
				case "COTSWOLD VILLAGE":
				case "PRYDWEN KEEP":
				case "CAER ULFWYCH":
				case "CAMPACORENTIN STATION":
				case "ADRIBARD'S RETREAT":
				case "CORNWALL STATION":
				case "SWANTON KEEP":
				case "LYONESSE":
				case "DARTMOOR":
				case "AVALON MARSH":
				// Dungeon destinations
				case "TOMB OF MITHRA":
				case "KELTOI FOGOU":
				case "TEPOK'S MINE":
				case "CATACOMBS OF CARDOVA":
				case "STONEHENGE BARROWS":
				case "KRONDON":
				case "AVALON CITY":
				case "CAER SIDI":
					sRet.Append("You shall soon arrive in ");
					sRet.Append(text);
					sRet.Append(".");
					SayTo(player, sRet.ToString());
					realmTarget = eRealm.Albion;
					break;
				case "FOREST SAUVAGE":
					SayTo(player, "Now to the Frontiers for the glory of the realm!");
					realmTarget = eRealm.Albion;
					break;
				case "CASTLE SAUVAGE":
				case "SNOWDONIA FORTRESS":
					sRet.Append(text);
					sRet.Append(" is what you seek, and ");
					sRet.Append(text);
					sRet.Append(" is what you shall find.");
					SayTo(player, sRet.ToString());
					realmTarget = eRealm.Albion;
					break;
				case "INCONNU CRYPT":
					//if (player.HasFinishedQuest(typeof(InconnuCrypt)) <= 0)
					//{
					//	SayTo(player, String.Format("I may only send those who know the way to this {0} {1}",
					//	                            "city. Seek out the path to the city and in future times I will aid you in",
					//	                            "this journey."));
					//	return;
					//}
					realmTarget = eRealm.Albion;
					break;
				case "HOLTHAM":
					if (ServerProperties.Properties.DISABLE_TUTORIAL)
						SayTo(player, "Sorry, this place is not available for now !");
					else if (player.Level > 15)
						SayTo(player, "Sorry, you are far too experienced to enjoy this place !");
					else
					{
						realmTarget = eRealm.Albion;
						break;
					}
					return true;

				// Midgard
				case "JORDHEIM":
					SayTo(player, "The great city awaits!");
					realmTarget = eRealm.Midgard;
					break;
				case "MIDGARD OCEANUS":
					if (player.Client.Account.PrivLevel < ServerProperties.Properties.ATLANTIS_TELEPORT_PLVL)
					{
						SayTo(player, "I'm sorry, but you are not authorized to enter Atlantis at this time.");
						return true;
					}
					SayTo(player, "You will soon arrive in the Haven of Oceanus.");
					realmTarget = eRealm.Midgard;
					text = "Oceanus";
					break;
				// SI cities
				case "AEGIRHAMN":
				case "BJARKEN":
				case "HAGALL":
				case "KNARR":
					SayTo(player, "The Shrouded Isles await you.");
					realmTarget = eRealm.Midgard;
					break;
				// Mainland destinations
				case "MULARN":
				case "FORT VELDON":
				case "AUDLITEN":
				case "HUGINFELL":
				case "FORT ATLA":
				case "GNA FASTE":
				case "RAUMARIK":
				case "MALMOHUS":
				case "GOTAR":
				// Dungeon destinations
				case "NISSE'S LAIR":
				case "CURSED TOMB":
				case "VENDO CAVERNS":
				case "VARULVHAMN":
				case "SPINDELHALLA":
				case "IARNVIDIUR'S LAIR":
				case "TROLLHEIM":
				case "TUSCAREN GLACIER":
					sRet.Append("You shall soon arrive in ");
					sRet.Append(text);
					sRet.Append(".");
					SayTo(player, sRet.ToString());
					realmTarget = eRealm.Midgard;
					break;
				case "KOBOLD UNDERCITY":
					//if (player.HasFinishedQuest(typeof(KoboldUndercity)) <= 0)
					//{
					//	SayTo(player, String.Format("I may only send those who know the way to this {0} {1}",
					//	                            "city. Seek out the path to the city and in future times I will aid you in",
					//	                            "this journey."));
					//	return;
					//}
					realmTarget = eRealm.Midgard;
					break;
				case "UPPLAND":
					SayTo(player, "Now to the Frontiers for the glory of the realm!");
					realmTarget = eRealm.Midgard;
					break;
				case "SVASUD FASTE":
				case "VINDSAUL FASTE":
					sRet.Append(text);
					sRet.Append(" is what you seek, and ");
					sRet.Append(text);
					sRet.Append(" is what you shall find.");
					SayTo(player, sRet.ToString());
					realmTarget = eRealm.Midgard;
					break;
				case "HAFHEIM":
					if (ServerProperties.Properties.DISABLE_TUTORIAL)
						SayTo(player, "Sorry, this place is not available for now !");
					else if (player.Level > 15)
						SayTo(player, "Sorry, you are far too experienced to enjoy this place !");
					else
					{
						realmTarget = eRealm.Midgard;
						break;
					}
					return true;

				// Hibernia
				case "TIR NA NOG":
					SayTo(player, "The great city awaits!");
					realmTarget = eRealm.Hibernia;
					break;
				case "HIBERNIA OCEANUS":
					if (player.Client.Account.PrivLevel < ServerProperties.Properties.ATLANTIS_TELEPORT_PLVL)
					{
						SayTo(player, "I'm sorry, but you are not authorized to enter Atlantis at this time.");
						return true;
					}
					SayTo(player, "You will soon arrive in the Haven of Oceanus.");
					realmTarget = eRealm.Hibernia;
					text = "Oceanus";
					break;
				// SI locations
				case "DOMNANN":
				case "NECHT":
				case "AALID FEIE":
				case "DROIGHAID":
					SayTo(player, "The Shrouded Isles await you.");
					realmTarget = eRealm.Hibernia;
					break;
				// Mainland locations
				case "MAG MELL":
				case "TIR NA MBEO":
				case "ARDAGH":
				case "HOWTH":
				case "CONNLA":
				case "INNIS CARTHAIG":
				case "CURSED FOREST":
				case "SHEEROE HILLS":
				case "SHANNON ESTUARY":
				// Dungeon destinations
				case "MUIRE TOMB":
				case "SPRAGGON DEN":
				case "KOALINTH CAVERNS":
				case "TREIBH CAILLTE":
				case "CORUSCATING MINE":
				case "TUR SUIL":
				case "FOMOR":
				case "GALLADORIA":
					sRet.Append("You shall soon arrive in ");
					sRet.Append(text);
					sRet.Append(".");
					SayTo(player, sRet.ToString());
					realmTarget = eRealm.Hibernia;
					break;
				case "CRUACHAN GORGE":
					SayTo(player, "Now to the Frontiers for the glory of the realm!");
					realmTarget = eRealm.Hibernia;
					break;
				case "DRUIM CAIN":
				case "DRUIM LIGEN":
					sRet.Append(text);
					sRet.Append(" is what you seek, and ");
					sRet.Append(text);
					sRet.Append(" is what you shall find.");
					SayTo(player, sRet.ToString());
					realmTarget = eRealm.Hibernia;
					break;
				case "SHAR LABYRINTH":
					//if (player.HasFinishedQuest(typeof(SharLabyrinth)) <= 0)
					//{
					//	SayTo(player, String.Format("I may only send those who know the way to this {0} {1}",
					//	                            "city. Seek out the path to the city and in future times I will aid you in",
					//	                            "this journey."));
					//	return;
					//}
					realmTarget = eRealm.Hibernia;
					break;
				case "FINTAIN":
					if (ServerProperties.Properties.DISABLE_TUTORIAL)
						SayTo(player, "Sorry, this place is not available for now !");
					else if (player.Level > 15)
						SayTo(player, "Sorry, you are far too experienced to enjoy this place !");
					else
					{
						text = "Fintain";
						realmTarget = eRealm.Hibernia;
						break;
					}
					return true;
				// All realms
				case "BATTLEGROUNDS":
					if (!ServerProperties.Properties.BG_ZONES_OPENED && player.Client.Account.PrivLevel == (uint)ePrivLevel.Player)
					{
						SayTo(player, ServerProperties.Properties.BG_ZONES_CLOSED_MESSAGE);
						return true;
					}

					SayTo(player, "I will teleport you to the appropriate battleground for your level and Realm Rank. If you exceed the Realm Rank for a battleground, you will not teleport. Please gain more experience to go to the next battleground.");
					break;
				case "ENTRANCE":
				case "PERSONAL":
				case "HEARTH":
					realmTarget = player.Realm;
					break;
				default:
					return base.WhisperReceive(source, text);
			} // switch (text.ToLower())

			// Find the teleport location in the database.
			Teleport port = GetTeleportLocation(player, text, realmTarget);
			if (port != null)
				OnTeleportSpell(player, port);
			else
				SayTo(player, "This destination is not yet supported.");

			return true;
		}

		protected Teleport GetTeleportLocation(GamePlayer player, string text, eRealm realm)
		{
			// Battlegrounds are specials, as the teleport location depends on
			// the level of the player, so let's deal with that first.
			if (text.ToLower() == "battlegrounds")
			{
				if (!ServerProperties.Properties.BG_ZONES_OPENED && player.Client.Account.PrivLevel == (uint)ePrivLevel.Player)
				{
					SayTo(player, ServerProperties.Properties.BG_ZONES_CLOSED_MESSAGE);
				}
				else
				{
					AbstractGameKeep portalKeep = GameServer.KeepManager.GetBGPK(player);
					if (portalKeep != null)
					{
						Teleport teleport = new Teleport();
						teleport.TeleportID = "battlegrounds";
						teleport.Realm = (byte)portalKeep.Realm;
						teleport.RegionID = portalKeep.Region;
						teleport.X = portalKeep.X;
						teleport.Y = portalKeep.Y;
						teleport.Z = portalKeep.Z;
						teleport.Heading = 0;
						return teleport;
					}
					else
					{
						if (player.Client.Account.PrivLevel > (uint)ePrivLevel.Player)
						{
							player.Out.SendMessage("No portal keep found.", eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
						}
						return null;
					}
				}
			}

			// Another special case is personal house, as there is no location
			// that will work for every player.
			if (text.ToLower() == "personal")
			{
				House house = HouseMgr.GetHouseByPlayer(player);

				if (house == null)
				{
					text = "entrance";  // Fall through, port to housing entrance.
				}
				else
				{
					IGameLocation location = house.OutdoorJumpPoint;
					Teleport teleport = new Teleport();
					teleport.TeleportID = "personal";
					teleport.Realm = (int)player.Realm;
					teleport.RegionID = location.RegionID;
					teleport.X = location.X;
					teleport.Y = location.Y;
					teleport.Z = location.Z;
					teleport.Heading = location.Heading;
					return teleport;
				}
			}

			// Yet another special case the port to the 'hearth' what means
			// that the player will be ported to the defined house bindstone
			if (text.ToLower() == "hearth")
			{
				// Check if player has set a house bind
				if (!(player.BindHouseRegion > 0))
				{
					SayTo(player, "Sorry, you haven't set any house bind point yet.");
					return null;
				}

				// Check if the house at the player's house bind location still exists
				ArrayList houses = (ArrayList)HouseMgr.GetHousesCloseToSpot((ushort)player.
					BindHouseRegion, player.BindHouseXpos, player.
					BindHouseYpos, 700);
				if (houses.Count == 0)
				{
					SayTo(player, "I'm afraid I can't teleport you to your hearth since the house at your " +
						"house bind location has been torn down.");
					return null;
				}

				// Check if the house at the player's house bind location contains a bind stone
				House targetHouse = (House)houses[0];
				IDictionary<uint, DBHouseHookpointItem> hookpointItems = targetHouse.HousepointItems;
				Boolean hasBindstone = false;

				foreach (KeyValuePair<uint, DBHouseHookpointItem> targetHouseItem in hookpointItems)
				{
					if (((GameObject)targetHouseItem.Value.GameObject).GetName(0, false).ToLower().EndsWith("bindstone"))
					{
						hasBindstone = true;
						break;
					}
				}

				if (!hasBindstone)
				{
					SayTo(player, "I'm sorry to tell that the bindstone of your current house bind location " +
						"has been removed, so I'm not able to teleport you there.");
					return null;
				}

				// Check if the player has the permission to bind at the house bind stone
				if (!targetHouse.CanBindInHouse(player))
				{
					SayTo(player, "You're no longer allowed to bind at the house bindstone you've previously " +
						"chosen, hence I'm not allowed to teleport you there.");
					return null;
				}

				Teleport teleport = new Teleport();
				teleport.TeleportID = "hearth";
				teleport.Realm = (int)player.Realm;
				teleport.RegionID = player.BindHouseRegion;
				teleport.X = player.BindHouseXpos;
				teleport.Y = player.BindHouseYpos;
				teleport.Z = player.BindHouseZpos;
				teleport.Heading = player.BindHouseHeading;
				return teleport;
			}

			if (text.ToLower() == "guild")
			{
				House house = HouseMgr.GetGuildHouseByPlayer(player);

				if (house == null)
				{
					return null;  // no teleport when guild house not found
				}
				else
				{
					IGameLocation location = house.OutdoorJumpPoint;
					Teleport teleport = new Teleport();
					teleport.TeleportID = "guild house";
					teleport.Realm = (int)player.Realm;
					teleport.RegionID = location.RegionID;
					teleport.X = location.X;
					teleport.Y = location.Y;
					teleport.Z = location.Z;
					teleport.Heading = location.Heading;
					return teleport;
				}
			}

			// Find the teleport location in the database.
			return WorldMgr.GetTeleportLocation(realm, String.Format(":{0}", text));
		}
	}
}

#region Summon 

namespace DOL.GS.Spells
{
    [SpellHandler("Summon A Teleporter")]
    public class SummonTeleportSpellHandler : SpellHandler
    {
        protected GameNPC Npc;

        public SummonTeleportSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
        }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            var template = NpcTemplateMgr.GetTemplate((int)Spell.Value);

            base.ApplyEffectOnTarget(target, effectiveness);

            if (template.ClassType == "")
                Npc = new GameNPC();
            else
            {
                try
                {
                    Npc = new GameNPC();
                    Npc = (GameNPC)Assembly.GetAssembly(typeof(GameServer)).CreateInstance(template.ClassType, false);
                }
                catch (Exception e)
                {
                }
                if (Npc == null)
                {
                    try
                    {
                        Npc = (GameNPC)Assembly.GetExecutingAssembly().CreateInstance(template.ClassType, false);
                    }
                    catch (Exception e)
                    {
                    }
                }
                if (Npc == null)
                {
                    MessageToCaster("There was an error creating an instance of " + template.ClassType + "!",
                        eChatType.CT_System);
                    return;
                }
                Npc.LoadTemplate(template);
            }

            int x, y;
            Caster.GetSpotFromHeading(64, out x, out y);
            Npc.X = x;
            Npc.Y = y;
            Npc.Z = Caster.Z;
            Npc.CurrentRegion = Caster.CurrentRegion;
            Npc.Heading = (ushort)((Caster.Heading + 2048) % 4096);
            Npc.Realm = Caster.Realm;
            Npc.CurrentSpeed = 0;
            Npc.Level = Caster.Level;
            Npc.Name = Caster.Name + " Teleporter " + "";
            Npc.SetOwnBrain(new BlankBrain());
            Npc.AddToWorld();
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (Npc != null)
                Npc.Delete();
            return base.OnEffectExpires(effect, noMessages);
        }

        public override bool IsOverwritable(GameSpellEffect compare)
        {
            return false;
        }
    }
}

#endregion