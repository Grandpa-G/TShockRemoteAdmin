/*
 * 
 * Welcome to this template plugin.
 * Level: All Levels
 * Purpose: To get a working model for new plugins to be built off.  This plugin will
 * compile immediately, all you have to do is rename TemplatePlugin to reflect 
 * the purpose of the plugin.
 * 
 * You may need to delete the references to TerrariaServer and TShockAPI.  They 
 * could be pointing to my current folder.  Just remove them and then right-click the
 * references folder, go to browse go to the dll folder, and select both.
 * 
 */

using System;
using Terraria;
using TShockAPI;
using TerrariaApi.Server;
using System.Reflection;
using HttpServer;
using System.Collections;
using System.Collections.Generic;
using TShockAPI.DB;
using Rests;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Data;
using TShockAPI.ServerSideCharacters;

namespace extraAdminREST
{
    [ApiVersion(1, 16)]
    public class ExtraAdminREST : TerrariaPlugin
    {
        public static string SavePath = "tshock";
        public static IDbConnection DB;
  
        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }
        public override string Author
        {
            get { return "Grandpa-G"; }
        }
        public override string Name
        {
            get { return "ExtraAdminREST"; }
        }

        public override string Description
        {
            get { return "Extra Admin Rest Api commands"; }
        }

        public static ConfigFile Config;

        public static ServerSideConfig ServerSideCharacterConfig;

        public override void Initialize()
        {
              TShock.RestApi.Register(new SecureRestCommand("/AdminREST/version", AdminRest, "AdminREST.allow"));
            TShock.RestApi.Register(new RestCommand("/AdminREST/getPlayerData", getPlayerData));
            TShock.RestApi.Register(new RestCommand("/AdminREST/GroupList", GroupList));
            TShock.RestApi.Register(new RestCommand("/AdminREST/PlayerList", PlayerList));
            TShock.RestApi.Register(new RestCommand("/AdminREST/BanList", BanListV2));
            TShock.RestApi.Register(new RestCommand("/AdminREST/WorldInfo", WorldInfo));
            TShock.RestApi.Register(new RestCommand("/AdminREST/GroupInfo", GroupInfo));
            TShock.RestApi.Register(new RestCommand("/AdminREST/serverInfo", serverInfo));
            TShock.RestApi.Register(new RestCommand("/AdminREST/getLog", getLog));
            TShock.RestApi.Register(new RestCommand("/AdminREST/updateMOTD", updateMOTD));
            TShock.RestApi.Register(new RestCommand("/AdminREST/getConfig", getConfig));
            TShock.RestApi.Register(new RestCommand("/AdminREST/getConfigDescription", getConfigDescription));
            TShock.RestApi.Register(new RestCommand("/AdminREST/updateConfig", updateConfig));
            TShock.RestApi.Register(new RestCommand("/AdminREST/searchusers", searchUsers));
            TShock.RestApi.Register(new RestCommand("/AdminREST/updateInventory", updateInventory));
            TShock.RestApi.Register(new RestCommand("/AdminREST/userlist", UserList));
            TShock.RestApi.Register(new RestCommand("/AdminREST/getInventory", getInventory));
 
            FileTools.SetupConfig();
        }

        private object PlayerList(RestRequestArgs args)
        {
            var playerList = new ArrayList();
            foreach (TSPlayer tsPlayer in TShock.Players.Where(p => null != p))
            {
                var p = PlayerFilter(tsPlayer, args.Parameters);
                if (null != p)
                    playerList.Add(p);
            }
            return new RestObject() { { "players", playerList } };
        }

        
        private object WorldInfo(RestRequestArgs args)
        {
      				var msg = new TShockAPI.Net.WorldInfoMsg
				{
					Time = (int)Main.time,
					DayTime = Main.dayTime,
					MoonPhase = (byte)Main.moonPhase,
					BloodMoon = Main.bloodMoon,
					Eclipse = Main.eclipse,
					MaxTilesX = (short)Main.maxTilesX,
					MaxTilesY = (short)Main.maxTilesY,
					SpawnX = (short)Main.spawnTileX,
					SpawnY = (short)Main.spawnTileY,
					WorldSurface = (short)Main.worldSurface,
					RockLayer = (short)Main.rockLayer,
					//Sending a fake world id causes the client to not be able to find a stored spawnx/y.
					//This fixes the bed spawn point bug. With a fake world id it wont be able to find the bed spawn.
					WorldID = Main.worldID,
					MoonType = (byte)Main.moonType,
					TreeX0 = Main.treeX[0],
					TreeX1 = Main.treeX[1],
					TreeX2 = Main.treeX[2],
					TreeStyle0 = (byte)Main.treeStyle[0],
					TreeStyle1 = (byte)Main.treeStyle[1],
					TreeStyle2 = (byte)Main.treeStyle[2],
					TreeStyle3 = (byte)Main.treeStyle[3],
					CaveBackX0 = Main.caveBackX[0],
					CaveBackX1 = Main.caveBackX[1],
					CaveBackX2 = Main.caveBackX[2],
					CaveBackStyle0 = (byte)Main.caveBackStyle[0],
					CaveBackStyle1 = (byte)Main.caveBackStyle[1],
					CaveBackStyle2 = (byte)Main.caveBackStyle[2],
					CaveBackStyle3 = (byte)Main.caveBackStyle[3],
					SetBG0 = (byte)WorldGen.treeBG,
					SetBG1 = (byte)WorldGen.corruptBG,
					SetBG2 = (byte)WorldGen.jungleBG,
					SetBG3 = (byte)WorldGen.snowBG,
					SetBG4 = (byte)WorldGen.hallowBG,
					SetBG5 = (byte)WorldGen.crimsonBG,
					SetBG6 = (byte)WorldGen.desertBG,
					SetBG7 = (byte)WorldGen.oceanBG,
					IceBackStyle = (byte)Main.iceBackStyle,
					JungleBackStyle = (byte)Main.jungleBackStyle,
					HellBackStyle = (byte)Main.hellBackStyle,
					WindSpeed = Main.windSpeed,
					NumberOfClouds = (byte)Main.numClouds,
                    BossFlags = (WorldGen.shadowOrbSmashed ? TShockAPI.Net.BossFlags.OrbSmashed : TShockAPI.Net.BossFlags.None) |
                                (NPC.downedBoss1 ? TShockAPI.Net.BossFlags.DownedBoss1 : TShockAPI.Net.BossFlags.None) |
                                (NPC.downedBoss2 ? TShockAPI.Net.BossFlags.DownedBoss2 : TShockAPI.Net.BossFlags.None) |
                                (NPC.downedBoss3 ? TShockAPI.Net.BossFlags.DownedBoss3 : TShockAPI.Net.BossFlags.None) |
                                (Main.hardMode ? TShockAPI.Net.BossFlags.HardMode : TShockAPI.Net.BossFlags.None) |
                                (NPC.downedClown ? TShockAPI.Net.BossFlags.DownedClown : TShockAPI.Net.BossFlags.None) |
                                (Main.ServerSideCharacter ? TShockAPI.Net.BossFlags.ServerSideCharacter : TShockAPI.Net.BossFlags.None) |
                                (NPC.downedPlantBoss ? TShockAPI.Net.BossFlags.DownedPlantBoss : TShockAPI.Net.BossFlags.None),
                    BossFlags2 = (NPC.downedMechBoss1 ? TShockAPI.Net.BossFlags2.DownedMechBoss1 : TShockAPI.Net.BossFlags2.None) |
                                 (NPC.downedMechBoss2 ? TShockAPI.Net.BossFlags2.DownedMechBoss2 : TShockAPI.Net.BossFlags2.None) |
                                 (NPC.downedMechBoss3 ? TShockAPI.Net.BossFlags2.DownedMechBoss3 : TShockAPI.Net.BossFlags2.None) |
                                 (NPC.downedMechBossAny ? TShockAPI.Net.BossFlags2.DownedMechBossAny : TShockAPI.Net.BossFlags2.None) |
                                 (Main.cloudBGActive == 1f ? TShockAPI.Net.BossFlags2.CloudBg : TShockAPI.Net.BossFlags2.None) |
                                 (WorldGen.crimson ? TShockAPI.Net.BossFlags2.Crimson : TShockAPI.Net.BossFlags2.None) |
                                 (Main.pumpkinMoon ? TShockAPI.Net.BossFlags2.PumpkinMoon : TShockAPI.Net.BossFlags2.None),
					Rain = Main.maxRaining,
					WorldName = TShock.Config.UseServerName ? TShock.Config.ServerName : Main.worldName
                };
                    Console.Write(msg.BloodMoon);
                    Console.Write(msg.DayTime);
                    return new RestObject() { { "bloodmoon", msg.BloodMoon } };

        }

        private object BanListV2(RestRequestArgs args)
        {
            var banList = new ArrayList();
            foreach (var ban in TShock.Bans.GetBans())
            {
                banList.Add(
                    new Dictionary<string, string>
					{
						{"name", null == ban.Name ? "" : ban.Name},
						{"ip", null == ban.IP ? "" : ban.IP},
						{"reason", null == ban.Reason ? "" : ban.Reason},
						{"banninguser", null == ban.BanningUser? "" : ban.BanningUser},
						{"date", null == ban.Date ? "" : ban.Date},
						{"expiration", null == ban.Expiration ? "" : ban.Expiration},
					}
                );
            }

            return new RestObject() { { "bans", banList } };
        }

        private object GroupList(RestRequestArgs args)
        {
            var groups = new ArrayList();
            foreach (TShockAPI.Group group in TShock.Groups)
            {
                groups.Add(new Dictionary<string, object> { { "name", group.Name }, { "parent", group.ParentName }, { "chatcolor", group.ChatColor }, {"prefix", group.Prefix}, {"suffix", group.Suffix} });
            }
            return new RestObject() { { "groups", groups } };
        }


        private object GroupInfo(RestRequestArgs args)
        {
  		var ret = GroupFind(args.Parameters);
			if (ret is RestObject)
				return ret;

            TShockAPI.Group group = (TShockAPI.Group)ret;
            return new RestObject() {
				{"name", group.Name},
				{"parent", group.ParentName},
				{"chatcolor", string.Format("{0},{1},{2}", group.R, group.G, group.B)},
				{"permissions", group.permissions},
				{"prefix", group.Prefix},
				{"suffix", group.Suffix},
				{"negatedpermissions", group.negatedpermissions},
				{"totalpermissions", group.TotalPermissions}
			};
        }


        private object getLog(RestRequestArgs args)
       {
            if (string.IsNullOrWhiteSpace(args.Parameters["count"]))
                return RestMissingParam("count");
            String lineCount = args.Parameters["count"];
 
            var directory = new DirectoryInfo(TShock.Config.LogPath);

            String searchPattern = @"(19|20)\d\d[-](0[1-9]|1[012])[-](0[1-9]|[12][0-9]|3[01]).*.log";

            var log = Directory.GetFiles(TShock.Config.LogPath).Where(path => Regex.Match(path, searchPattern).Success).Last();
            String logFile = Path.GetFullPath(log);

            FileStream logFileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader logFileReader = new StreamReader(logFileStream);

            int limit = System.Convert.ToInt32(lineCount);
            var buffor = new Queue<string>(limit);
            while (!logFileReader.EndOfStream)
            {
                string line = logFileReader.ReadLine();
                if (buffor.Count >= limit)
                    buffor.Dequeue();
                buffor.Enqueue(line);
            }

            string[] LogLinesEnd = buffor.ToArray();
            // Clean up
            logFileReader.Close();
            logFileStream.Close();

            return new RestObject() { { "file", logFile }, { "log", LogLinesEnd } };

        }
        #region GiveAll+
        public static void ShowColor(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /giveall <item name/id> [item amount] [prefix id/name]");
                return;
            }

            int amountParamIndex = -1;

            string itemNameOrId;
            if (amountParamIndex == -1)
                itemNameOrId = string.Join(" ", args.Parameters);
            else
                itemNameOrId = string.Join(" ", args.Parameters.Take(amountParamIndex));


            foreach (TSPlayer plr in TShock.Players)
            {
                if (plr != null)
                {
                    TSPlayer.All.SendSuccessMessage("{0} hair {1} skin {2}.", args.Player.Name, plr.TPlayer.hairColor, plr.TPlayer.skinColor);
                }
            }
        }
        #endregion

       private object getPlayerData(RestRequestArgs args)
        {
            var ret = PlayerFind(args.Parameters);
            if (ret is RestObject)
                return ret;

            TSPlayer player = (TSPlayer)ret;
            var inventory = player.TPlayer.inventory.Where(p => p.active).ToList();
            var equipment = player.TPlayer.armor.Where(p => p.active).ToList();
            var dyes = player.TPlayer.dye.Where(p => p.active).ToList();
            return new RestObject()
			{
				{"index", player.Index},
				{"nickname", player.Name},
				{"username", null == player.UserAccountName ? "" : player.UserAccountName},
				{"group", player.Group.Name},
				{"inventory", string.Join(", ", inventory.Select(p => (p.name + ":" + p.stack)))},
				{"armor", string.Join(", ", equipment.Select(p => (p.netID + ":" + p.prefix)))},
				{"haircolor", player.TPlayer.hairColor},
				{"skincolor", player.TPlayer.skinColor},
				{"pantsColor", player.TPlayer.pantsColor},
				{"shirtColor", player.TPlayer.shirtColor},
				{"underShirtColor", player.TPlayer.underShirtColor},
				{"eyeColor", player.TPlayer.eyeColor},
				{"shoeColor", player.TPlayer.shoeColor},
				{"dyes", string.Join(", ", dyes.Select(p => (p.name)))},
				{"buffs", string.Join(", ", player.TPlayer.buffType)}
			};
        }

       public static RestObject serverInfo(RestRequestArgs args)
        {

            return new RestObject() { { "worldID", Main.worldID } };

        }

       public static RestObject getConfigDescription(RestRequestArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Parameters["configFile"]))
                return new RestObject("400") { Response = "No config type given" };
             String configFile = args.Parameters["configFile"];
            if (configFile == null)
            {
                return new RestObject("400") { Response = "No config type given" };
            }
            DumpDescriptions((configFile.Equals("config.json")));

             return new RestObject() { { "description", ConfigDescription } };

        }


       public static RestObject getConfig(RestRequestArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Parameters["config"]))
                return new RestObject("400") { Response = "No config given" };
            String config = args.Parameters["config"];
            if (config == null)
            {
                //               Console.WriteLine(parameters["config"]);
                return new RestObject("400") { Response = "No config given" };
            }

            String configFilePath = Path.Combine(TShock.SavePath, config);

            if (!File.Exists(configFilePath))
            {
                return new RestObject("400") { Response = "Invalid file path" };
            }

            using (var sr = new StreamReader(configFilePath))
            {
                if (config.Equals("config.json"))
                {
                    var cf = JsonConvert.DeserializeObject<ConfigFile>(sr.ReadToEnd());
                    return new RestObject() { { "config", cf } };
                }
                else
                {
                    var cf = JsonConvert.DeserializeObject<ServerSideConfig>(sr.ReadToEnd());
                    return new RestObject() { { "config", cf } };
                }
            }
        }


       public static RestObject updateConfig(RestRequestArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Parameters["config"]))
                return new RestObject("400") { Response = "No config given" };
            String config = args.Parameters["config"];
            if (config == null)
            {
                return new RestObject("400") { Response = "No config given" };
            }

            if (string.IsNullOrWhiteSpace(args.Parameters["configFile"]))
                return RestMissingParam("configFile");
            String configFile = args.Parameters["configFile"];
            if (configFile == null)
            {
                //               Console.WriteLine(parameters["config"]);
                return new RestObject("400") { Response = "No config file given" };
            }

            JObject json = JObject.Parse(config);

            Dictionary<string, dynamic> configList = new Dictionary<string, dynamic> { };

            String configFilePath = Path.Combine(TShock.SavePath, configFile);
            if (File.Exists(configFilePath))
            {
                using (var sw = new StreamWriter(configFilePath))
                {
                    string j = JsonConvert.SerializeObject(json, Formatting.Indented);
                    sw.Write(j);
                    sw.Flush();
                    sw.Close();
                }
                //              File.WriteAllText(configFilePath, config);
            }
            else
            {
                Console.WriteLine(configFilePath);
                return new RestObject("400") { Response = "Invalid file path" };
            }
            return new RestObject() { Response = "config.json saved." };

        }


       public static RestObject updateMOTD(RestRequestArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Parameters["motd"]))
                return new RestObject("400") { Response = "No text given" };
            String motd = Convert.ToString(args.Parameters["motd"]);
            if (motd == null)
            {
                return new RestObject("400") { Response = "No text given" };
            }

            String f = Path.Combine(TShock.SavePath, "motd.txt");
  //          Console.WriteLine(f);

            if (File.Exists(f))
            {
                File.WriteAllText(f, motd);
            }
            return new RestObject()
			{
				 { "motd", motd }
			};
        }


       private object getInventory(RestRequestArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Parameters["account"]))
                return new RestObject("400") { Response = "Found no matches." };
            int account = Convert.ToInt32(args.Parameters["account"]);
            if (account <= 0)
            {
                 return new RestObject("400") { Response = "Found no matches." };
            }

            List<SSCInventory> inventory = GetSSCInventory(account);

            return new RestObject { { "inventory", inventory } };
        }

       public static RestObject updateInventory(RestRequestArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Parameters["account"]))
                return new RestObject("400") { Response = "Invalid account." };
            int account = Convert.ToInt32(args.Parameters["account"]);
            if (account <= 0)
            {
                return new RestObject("400") { Response = "Invalid account." };
            }

            if (string.IsNullOrWhiteSpace(args.Parameters["inventory"]))
                return RestMissingParam("inventory");
            string inventory = Convert.ToString(args.Parameters["inventory"]);
            if (inventory == null)
            {
                  return new RestObject("400") { Response = "Invalid inventory." };
            }
            bool ok = ModifySSCInventory(account, inventory);
            if (ok)
                return new RestObject { { "update", inventory } };
            else
                return new RestObject("400") { Response = "Update failure." };

        }

       public static RestObject AdminRest(RestRequestArgs args)
        {
            String dbType;
            if (TShock.DB.GetSqlType() == SqlType.Sqlite)
                dbType = "SQLite";
            else
                dbType = "MySQL";
            return new RestObject()
			{
				 { "version", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
				{ "db", dbType }
			};

        }

       public static RestObject searchUsers(RestRequestArgs args)
        {

             string searchString = args.Parameters["where"];
 
            if (searchString == null)
                return RestError("Missing or empty search string - /AdminREST/searchusers where=<where clause>~");

            List<UserList> userList = FindUsers(searchString, true);
            return new RestObject() { { "Users", userList } };

        }
        public static RestObject UserList(RestRequestArgs args)
        {

            var ret = UserFind(args.Parameters);
            if (ret is RestObject)
                return (RestObject)ret;

            User user = (User)ret;

            return new RestObject()
			{
				{"id", user.ID},
				{"username", user.Name},
				{"ip", user.KnownIps},
				{"uuid", user.UUID},
				{"registered", user.Registered},
				{"lastaccessed", user.LastAccessed},
				{"group", user.Group}
			};

        }

       protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }

        public ExtraAdminREST(Main game)
            : base(game)
        {
            Order = 1;
        }

        #region Utility Methods

        private static RestObject RestError(string message, string status = "400")
        {
            return new RestObject(status) { Error = message };
        }

        private RestObject RestResponse(string message, string status = "200")
        {
            return new RestObject(status) { Response = message };
        }

        private static RestObject RestMissingParam(string var)
        {
            return RestError("Missing or empty " + var + " parameter");
        }

        private static RestObject RestMissingParam(params string[] vars)
        {
            return RestMissingParam(string.Join(", ", vars));
        }

        private RestObject RestInvalidParam(string var)
        {
            return RestError("Missing or invalid " + var + " parameter");
        }

        private bool GetBool(string val, bool def)
        {
            bool ret;
            return bool.TryParse(val, out ret) ? ret : def;
        }

        private static object PlayerFind(IParameterCollection parameters)
        {
            string name = parameters["player"];
            if (string.IsNullOrWhiteSpace(name))
                return RestMissingParam("player");

            var found = TShock.Utils.FindPlayer(name);
            switch (found.Count)
            {
                case 1:
                    return found[0];
                case 0:
                    return RestError("Player " + name + " was not found");
                default:
                    return RestError("Player " + name + " matches " + found.Count + " players");
            }
        }

        private static object UserFind(IParameterCollection parameters)
        {
            string name = parameters["user"];
            if (string.IsNullOrWhiteSpace(name))
                return RestMissingParam("user");

            User user;
            string type = parameters["type"];
            try
            {
                switch (type)
                {
                    case null:
                    case "name":
                        type = "name";
                        user = TShock.Users.GetUserByName(name);
                        break;
                    case "id":
                        user = TShock.Users.GetUserByID(Convert.ToInt32(name));
                        break;
                    default:
                        return RestError("Invalid Type: '" + type + "'");
                }
            }
            catch (Exception e)
            {
                return RestError(e.Message);
            }

            if (null == user)
                return RestError(String.Format("User {0} '{1}' doesn't exist", type, name));

            return user;
        }

        private object BanFind(IParameterCollection parameters)
        {
            string name = parameters["ban"];
            if (string.IsNullOrWhiteSpace(name))
                return RestMissingParam("ban");

            string type = parameters["type"];
            if (string.IsNullOrWhiteSpace(type))
                return RestMissingParam("type");

            Ban ban;
            switch (type)
            {
                case "ip":
                    ban = TShock.Bans.GetBanByIp(name);
                    break;
                case "name":
                    ban = TShock.Bans.GetBanByName(name, GetBool(parameters["caseinsensitive"], true));
                    break;
                default:

                    return RestError("Invalid Type: '" + type + "'");
            }

            if (null == ban)
                return RestError("Ban " + type + " '" + name + "' doesn't exist");

            return ban;
        }

        private object GroupFind(IParameterCollection parameters)
        {
            var name = parameters["group"];
            if (string.IsNullOrWhiteSpace(name))
                return RestMissingParam("group");

            var group = TShock.Groups.GetGroupByName(name);
            if (null == group)
                return RestError("Group '" + name + "' doesn't exist");

            return group;
        }

        private Dictionary<string, object> PlayerFilter(TSPlayer tsPlayer, IParameterCollection parameters, bool viewips = false)
        {
            var player = new Dictionary<string, object>
				{
					{"nickname", tsPlayer.Name},
					{"index", tsPlayer.Index},
					{"username", tsPlayer.UserAccountName ?? ""},
					{"group", tsPlayer.Group.Name},
					{"active", tsPlayer.Active},
					{"state", tsPlayer.State},
					{"team", tsPlayer.Team},
				};

            if (viewips)
            {
                player.Add("ip", tsPlayer.IP);
            }
            foreach (IParameter filter in parameters)
            {
                if (player.ContainsKey(filter.Name) && !player[filter.Name].Equals(filter.Value))
                    return null;
            }
            return player;
        }

        #endregion

        /// <summary>
        /// Gets a list of Users from db.
        /// </summary>
        private static List<UserList> FindUsers(string search, bool casesensitive = true)
        {
            UserList rec;
            String sql;
            List<UserList> UserList = new List<UserList>();
            try
            {
                sql = "SELECT * FROM Users " + search + " order by UserName";
 
                 using (var reader = TShock.DB.QueryReader(sql))
                {
                    while (reader.Read())
                    {
                        rec = new UserList(reader.Get<Int32>("Id"), reader.Get<string>("UserName"), reader.Get<string>("UserGroup"), reader.Get<string>("Registered"), reader.Get<string>("LastAccessed"), reader.Get<string>("KnownIPs"));
                        UserList.Add(rec);
                    }
                    return UserList;
                }
            } 
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
            return null;
        }

        /// <summary>
        /// Gets a list of SSCInventoryLog entries. 
        /// </summary>
        public List<SSCInventory> GetSSCInventory(int account)
        {
            SSCInventory rec; 
            String sql;

            List<SSCInventory> SSCInventorylist = new List<SSCInventory>();
            try
            {
                using (var reader = TShock.DB.QueryReader("SELECT * FROM tsCharacter where account =@0", account))
                {
                    if (reader.Read())
                    {
                        rec = new SSCInventory(reader.Get<Int32>("Account"), reader.Get<string>("Inventory"),
                           reader.Get<Int32>("hair"), reader.Get<Int32>("hairdye"), (Color)TShock.Utils.DecodeColor(reader.Get<Int32>("haircolor")),
                           (Color)TShock.Utils.DecodeColor(reader.Get<Int32>("pantscolor")), (Color)TShock.Utils.DecodeColor(reader.Get<Int32>("shirtcolor")), (Color)TShock.Utils.DecodeColor(reader.Get<Int32>("undershirtcolor")), (Color)TShock.Utils.DecodeColor(reader.Get<Int32>("shoecolor")),
                           (Color)TShock.Utils.DecodeColor(reader.Get<Int32>("skincolor")), (Color)TShock.Utils.DecodeColor(reader.Get<Int32>("eyecolor"))
                           );
                         SSCInventorylist.Add(rec);
                        return SSCInventorylist;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
            return null;
        }

        /// <summary>
        /// Gets a list of SSCInventoryLog entries.
        /// </summary>
        public static bool ModifySSCInventory(int account, string inventory)
        {
            try
            {
                using (var reader = TShock.DB.QueryReader("UPDATE tsCharacter set Inventory = @1 where account =@0", account, inventory))
                {
                    try
                    {
                        //                       using (TShock.DB.QueryReader("COMMIT"))
                        {
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                        Console.WriteLine(ex.StackTrace);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                Console.WriteLine(ex.StackTrace);
                return false;
            }
            return true;
        }




        /// <summary>
        /// Dumps all configuration options to a text file in Markdown format
        /// </summary>
        //       List<String[]> ConfigDescription = new List<String[]>();
        public static Dictionary<string, object> ConfigDescription = new Dictionary<string, object> { };

        public static void DumpDescriptions(bool SSCconfigFile)
        {
            ConfigDescription.Clear();

            if (SSCconfigFile)
            {
                var defaults = new ConfigFile();
                foreach (var field in defaults.GetType().GetFields().OrderBy(f => f.Name))
                {
                    if (field.IsStatic)
                        continue;

                    var name = field.Name;
                    var type = field.FieldType.Name;

                    var descattr =
                        field.GetCustomAttributes(false).FirstOrDefault(o => o is DescriptionAttribute) as DescriptionAttribute;
                    string desc = descattr != null && !string.IsNullOrWhiteSpace(descattr.Description) ? descattr.Description : "None";

                    var def = field.GetValue(defaults);
                    Dictionary<string, string> definition = new Dictionary<string, string> { };
                    definition.Add("type", type);
                    //               desc = desc.Replace("\"", "");
                    definition.Add("definition", desc);

                    definition.Add("default", def.ToString());
                    ConfigDescription.Add(name, definition);
                }
            }
            else
            {
                var defaults = new ServerSideConfig();
                foreach (var field in defaults.GetType().GetFields().OrderBy(f => f.Name))
                {
                    if (field.IsStatic)
                        continue;

                    var name = field.Name;
                    var type = field.FieldType.Name;

                    var descattr =
                        field.GetCustomAttributes(false).FirstOrDefault(o => o is DescriptionAttribute) as DescriptionAttribute;
                    string desc = descattr != null && !string.IsNullOrWhiteSpace(descattr.Description) ? descattr.Description : "None";

                    var def = field.GetValue(defaults);
                    Dictionary<string, string> definition = new Dictionary<string, string> { };
                    definition.Add("type", type);
                    //               desc = desc.Replace("\"", "");
                    definition.Add("definition", desc);

                    definition.Add("default", def.ToString());
                    ConfigDescription.Add(name, definition);
                }

            }
        }
    }

    public class SSCInventory
    {
        public Int32 Id { get; set; }
        public string Inventory { get; set; }
        public Int32 Hair { get; set; }
        public Int32 HairDye { get; set; }
        public Color HairColor { get; set; }
        public Color PantsColor { get; set; }
        public Color ShirtColor { get; set; }
        public Color UnderShirtColor { get; set; }
        public Color ShoeColor { get; set; }
        public Color SkinColor { get; set; }
        public Color EyeColor { get; set; }

        public SSCInventory(Int32 id, string inventory, Int32 hair, Int32 hairdye, Color haircolor,
            Color pantscolor, Color shirtcolor, Color undershirtcolor, Color shoecolor, Color skincolor, Color eyecolor)
        {
            Id = id;
            Inventory = inventory;
            Hair = hair;
            HairDye = hairdye;
            HairColor = haircolor;
            PantsColor = pantscolor;
            ShirtColor = shirtcolor;
            UnderShirtColor = undershirtcolor;
            ShoeColor = haircolor;
            SkinColor = skincolor;
            EyeColor = eyecolor;
        }

        public SSCInventory()
        {
            Id = 0;
            Inventory = string.Empty;
            Hair = 0;
            HairDye = 0;
            HairColor = new Color(0,0,0);
            PantsColor = new Color(0, 0, 0);
            ShirtColor = new Color(0, 0, 0);
            UnderShirtColor = new Color(0, 0, 0);
            ShoeColor = new Color(0, 0, 0);
            SkinColor = new Color(0, 0, 0);
            EyeColor = new Color(0, 0, 0);

        }
    }
    public class UserList
    {
        public Int32 Id { get; set; }

        public string UserName { get; set; }

        public string UserGroup { get; set; }
        public string Registered { get; set; }

        public string LastAccessed { get; set; }

        public string KnownIPs { get; set; }

        public UserList(Int32 id, string username, string usergroup, string registered, string lastaccessed, string knownIPs)
        {
            Id = id;
            UserName = username;
            UserGroup = usergroup;
            Registered = registered;
            LastAccessed = lastaccessed;
            KnownIPs = knownIPs;
        }

        public UserList()
        {
            Id = 0;
            UserName = string.Empty;
            UserGroup = string.Empty;
            Registered = string.Empty;
            LastAccessed = string.Empty;
            KnownIPs = string.Empty;
        }
    }
}
