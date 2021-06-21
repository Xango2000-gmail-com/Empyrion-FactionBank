using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Eleon.Modding;
//using ProtoBuf;
using YamlDotNet.Serialization;
using Eleon;

namespace FactionBank
{
    public class MyEmpyrionMod : ModInterface, IMod
    {
        public static string ModShortName = "FactionBank";
        public static string ModVersion = ModShortName + " v0.0.11 made by Xango2000 (3209)";
        public static string ModPath = "..\\Content\\Mods\\" + ModShortName + "\\";
        internal static bool debug = false;
        internal static IModApi modApi;

        internal static Dictionary<int, Storage.StorableData> SeqNrStorage = new Dictionary<int, Storage.StorableData> { };
        public int thisSeqNr = 2000;
        internal static SetupYaml.Root SetupYamlData = new SetupYaml.Root { };
        public ItemStack[] blankItemStack = new ItemStack[] { };
        public Dictionary<int, PlayerInfo> OpenFB = new Dictionary<int, PlayerInfo> { };
        public Dictionary<ulong, Inventory> DictInvData = new Dictionary<ulong, Inventory> { };

        List<string> OnlinePlayers = new List<string> { };
        internal static int Expiration = 1628312399;
        bool LiteVersion = false;
        bool Disable = false;

        //########################################################################################################################################################
        //################################################ This is where the actual Empyrion Modding API stuff Begins ############################################
        //########################################################################################################################################################
        public void Game_Start(ModGameAPI gameAPI)
        {
            Storage.GameAPI = gameAPI;
            if (!Directory.GetCurrentDirectory().EndsWith("DedicatedServer"))
            {
                ModPath = "Content\\Mods\\" + ModShortName + "\\";
            }
            if (debug) { File.WriteAllText(ModPath + "ERROR.txt", ""); }
            if (debug) { File.WriteAllText(ModPath + "debug.txt", ""); }
            SetupYaml.Setup();
            CommonFunctions.Log("--------------------" + CommonFunctions.TimeStamp() + "----------------------------");
            ulong Tick = Storage.GameAPI.Game_GetTickTime();
            CommonFunctions.Debug("Startup Tick = " + Tick);

        }

        public void Game_Event(CmdId cmdId, ushort seqNr, object data)
        {
            try
            {
                switch (cmdId)
                {
                    case CmdId.Event_ChatMessage:
                        //Triggered when player says something in-game
                        ChatInfo Received_ChatInfo = (ChatInfo)data;
                        /*
                        if (!Disable)
                        {

                            string msg = Received_ChatInfo.msg.ToLower();
                            if (msg == SetupYamlData.ReinitializeCommand) //Reinitialize
                            {
                                SetupYaml.Setup();
                            }
                            else if (msg == "/mods" || msg == "!mods")
                            {
                                //API.Chat("Player", Received_ChatInfo.playerId, ModVersion);
                                API.ServerTell(Received_ChatInfo.playerId, ModShortName, ModVersion, true);
                            }
                            else if (msg == SetupYamlData.Command.ToLower())
                            {
                                try
                                {
                                    Storage.StorableData function = new Storage.StorableData
                                    {
                                        function = "FactionBank",
                                        Match = Convert.ToString(Received_ChatInfo.playerId),
                                        Requested = "PlayerInfo",
                                        ChatInfo = Received_ChatInfo
                                    };
                                    API.PlayerInfo(Received_ChatInfo.playerId, function);
                                }
                                catch
                                {
                                    CommonFunctions.Debug("FactionBank Fail: at ChatInfo");
                                }
                            }
                        }
                        */
                        break;


                    case CmdId.Event_Player_Connected:
                        //Triggered when a player logs on
                        Id Received_PlayerConnected = (Id)data;
                        string SteamID = modApi.Application.GetPlayerDataFor(Received_PlayerConnected.id).Value.SteamId;
                        if (!OnlinePlayers.Contains(SteamID))
                        {
                            OnlinePlayers.Add(SteamID);
                        }
                        if (OnlinePlayers.Count() > 10 && LiteVersion)
                        {
                            Disable = true;
                        }
                        ItemStack[] Backpack;
                        ItemStack[] Toolbar;
                        int edit = 0;
                        CommonFunctions.Debug("0");
                        if (File.Exists(ModPath + "Disconnected\\" + Received_PlayerConnected.id + "Backpack.txt"))
                        {
                            CommonFunctions.Debug("1");
                            Backpack = CommonFunctions.ReadItemStacks("", "Disconnected\\" + Received_PlayerConnected.id + "Backpack.txt");
                            edit = edit + 1;
                            File.Delete(ModPath + "Disconnected\\" + Received_PlayerConnected.id + "Backpack.txt");
                        }
                        else
                        {
                            Backpack = new ItemStack[] { };
                        }
                        if (File.Exists(ModPath + "Disconnected\\" + Received_PlayerConnected.id + "Toolbar.txt"))
                        {
                            CommonFunctions.Debug("2");
                            Toolbar = CommonFunctions.ReadItemStacks("", "Disconnected\\" + Received_PlayerConnected.id + "Toolbar.txt");
                            edit = edit + 1;
                            File.Delete(ModPath + "Disconnected\\" + Received_PlayerConnected.id + "Toolbar.txt");
                        }
                        else
                        {
                            Toolbar = new ItemStack[] { };
                        }
                        if (edit != 0)
                        {
                            API.PlayerInventorySet(Received_PlayerConnected.id, Backpack, Toolbar);
                            /*
                            Inventory InvData = new Inventory()
                            {
                                playerId = Received_PlayerConnected.id,
                                bag = Backpack,
                                toolbelt = Toolbar
                            };
                            ulong Tick = Storage.GameAPI.Game_GetTickTime();
                            DictInvData.Add(Tick, InvData);
                            CommonFunctions.Debug("Tick = " + Tick);
                            */
                        }
                        break;


                    case CmdId.Event_Player_Disconnected:
                        //Triggered when a player logs off
                        Id Received_PlayerDisconnected = (Id)data;
                        foreach (PlayerInfo player in OpenFB.Values)
                        {
                            if( Received_PlayerDisconnected.id == player.entityId)
                            {
                                OpenFB.Remove(player.factionId);
                                CommonFunctions.WriteItemStacks("", "Disconnected\\" + player.entityId + "Backpack.txt", player.bag, "false", 2000000000, false);
                                CommonFunctions.WriteItemStacks("", "Disconnected\\" + player.entityId + "Toolbar.txt", player.toolbar, "false", 2000000000, false);
                            }
                        }
                        break;


                    case CmdId.Event_Player_ChangedPlayfield:
                        //Triggered when a player changes playfield
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_ChangePlayfield, (ushort)CurrentSeqNr, new IdPlayfieldPositionRotation( [PlayerID], [Playfield Name], [PVector3 position], [PVector3 Rotation] ));
                        IdPlayfield Received_PlayerChangedPlayfield = (IdPlayfield)data;
                        break;


                    case CmdId.Event_Playfield_Loaded:
                        //Triggered when a player goes to a playfield that isnt currently loaded in memory
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Load_Playfield, (ushort)CurrentSeqNr, new PlayfieldLoad( [float nSecs], [string nPlayfield], [int nProcessId] ));
                        PlayfieldLoad Received_PlayfieldLoaded = (PlayfieldLoad)data;
                        break;


                    case CmdId.Event_Playfield_Unloaded:
                        //Triggered when there are no players left in a playfield
                        PlayfieldLoad Received_PlayfieldUnLoaded = (PlayfieldLoad)data;
                        break;


                    case CmdId.Event_Faction_Changed:
                        //Triggered when an Entity (player too?) changes faction
                        FactionChangeInfo Received_FactionChange = (FactionChangeInfo)data;
                        break;


                    case CmdId.Event_Statistics:
                        //Triggered on various game events like: Player Death, Entity Power on/off, Remove/Add Core
                        StatisticsParam Received_EventStatistics = (StatisticsParam)data;
                        break;


                    case CmdId.Event_Player_DisconnectedWaiting:
                        //Triggered When a player is having trouble logging into the server
                        Id Received_PlayerDisconnectedWaiting = (Id)data;
                        break;


                    case CmdId.Event_TraderNPCItemSold:
                        //Triggered when a player buys an item from a trader
                        TraderNPCItemSoldInfo Received_TraderNPCItemSold = (TraderNPCItemSoldInfo)data;
                        break;


                    case CmdId.Event_Player_List:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_List, (ushort)CurrentSeqNr, null));
                        IdList Received_PlayerList = (IdList)data;
                        break;


                    case CmdId.Event_Player_Info:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CurrentSeqNr, new Id( [playerID] ));
                        PlayerInfo Received_PlayerInfo = (PlayerInfo)data;
                        if (SeqNrStorage.Keys.Contains(seqNr))
                        {
                            Storage.StorableData RetrievedData = SeqNrStorage[seqNr];
                            if (RetrievedData.Requested == "PlayerInfo" && RetrievedData.function == "FactionBank" && Convert.ToString(Received_PlayerInfo.entityId) == RetrievedData.Match)
                            {
                                try
                                {
                                    SeqNrStorage.Remove(seqNr);
                                    RetrievedData.function = "FactionBank";
                                    RetrievedData.Match = Convert.ToString(Received_PlayerInfo.entityId);
                                    RetrievedData.Requested = "ItemExchange";
                                    RetrievedData.TriggerPlayer = Received_PlayerInfo;
                                    if ( OpenFB.Keys.Contains(Received_PlayerInfo.factionId))
                                    {
                                        API.Alert(Received_PlayerInfo.entityId, "FactionBank is curently in use by " + OpenFB[Received_PlayerInfo.factionId].playerName, "Yellow", 10);
                                        CommonFunctions.Log("Faction " + Received_PlayerInfo.factionId + " Bank already open by " + OpenFB[Received_PlayerInfo.factionId].playerName + " Cannot be opened by " + Received_PlayerInfo.playerName);
                                    }
                                    else if ( Received_PlayerInfo.factionGroup == 0) // Faction Group = In Faction
                                    {
                                        if (File.Exists(ModPath + "Factions\\" + RetrievedData.TriggerPlayer.factionId + ".txt"))
                                        {
                                            ItemStack[] BankInfo = CommonFunctions.ReadItemStacks("", "Factions\\" + RetrievedData.TriggerPlayer.factionId + ".txt");
                                            OpenFB.Add(Received_PlayerInfo.factionId, Received_PlayerInfo);
                                            API.OpenItemExchange(Received_PlayerInfo.entityId, "Faction Bank", "Shared with Faction, Only one player may access at a time", "Close", BankInfo, RetrievedData);
                                            CommonFunctions.Log("Faction " + Received_PlayerInfo.factionId + " Bank Opened by " + Received_PlayerInfo.playerName);
                                        }
                                        else
                                        {
                                            ItemStack[] BankInfo = new ItemStack[] { };
                                            OpenFB.Add(Received_PlayerInfo.factionId, Received_PlayerInfo);
                                            API.OpenItemExchange(Received_PlayerInfo.entityId, "Faction Bank", "Shared with Faction, Only one player may access at a time", "Close", BankInfo, RetrievedData);
                                            CommonFunctions.Log("Faction " + Received_PlayerInfo.factionId + " Bank Created and Opened by " + Received_PlayerInfo.playerName);
                                        }
                                    }
                                    else if (Received_PlayerInfo.factionGroup == 1)
                                    {
                                        //API.Chat("Player", Received_PlayerInfo.entityId, "FactionBank Error: You must be in a faction to use FactionBank");
                                        API.ServerTell(Received_PlayerInfo.entityId, ModShortName, "FactionBank Error: You must be in a faction to use FactionBank", true);
                                    }
                                    else
                                    {
                                        //API.Chat("Player", Received_PlayerInfo.entityId, "FactionBank Error: I don't know what you just did, but something went wrong.");
                                        API.ServerTell(Received_PlayerInfo.entityId, ModShortName, "FactionBank Error: I don't know what you just did, but something went wrong.", true);
                                    }
                                }
                                catch
                                {
                                    CommonFunctions.Debug("FactionBank Fail: at PlayerInfo");
                                }
                            }
                        }
                        break;


                    case CmdId.Event_Player_Inventory:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_GetInventory, (ushort)CurrentSeqNr, new Id( [playerID] ));
                        Inventory Received_PlayerInventory = (Inventory)data;
                        break;


                    case CmdId.Event_Player_ItemExchange:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CurrentSeqNr, new ItemExchangeInfo( [id], [title], [description], [buttontext], [ItemStack[]] ));
                        ItemExchangeInfo Received_ItemExchangeInfo = (ItemExchangeInfo)data;
                        if (SeqNrStorage.Keys.Contains(seqNr))
                        {
                            Storage.StorableData RetrievedData = SeqNrStorage[seqNr];
                            if (RetrievedData.Requested == "ItemExchange" && RetrievedData.function == "FactionBank" && Convert.ToString(Received_ItemExchangeInfo.id) == RetrievedData.Match)
                            {
                                OpenFB.Remove(RetrievedData.TriggerPlayer.factionId);
                                CommonFunctions.Log("Faction " + RetrievedData.TriggerPlayer.factionId + " Bank Closed by " + RetrievedData.TriggerPlayer.playerName);
                                try
                                {
                                    if ( SetupYamlData.Stack.ToLower() == "true")
                                    {
                                        //WriteItemStacks(string FolderPath, string FileName, ItemStack[] ItemStacks, string SuperStack, int MaxSuperStackSize)
                                        CommonFunctions.WriteItemStacks("", "Factions\\" + RetrievedData.TriggerPlayer.factionId + ".txt", Received_ItemExchangeInfo.items, "true", 2000000000, false);
                                    }
                                    else if (SetupYamlData.Stack.ToLower() == "false")
                                    {
                                        CommonFunctions.WriteItemStacks("", "Factions\\" + RetrievedData.TriggerPlayer.factionId + ".txt", Received_ItemExchangeInfo.items, "false", 2000000000, false);
                                    }
                                    else
                                    {
                                        CommonFunctions.WriteItemStacks("", "Factions\\" + RetrievedData.TriggerPlayer.factionId + ".txt", Received_ItemExchangeInfo.items, "superstack", SetupYamlData.MaxSuperStack, false);
                                    }
                                }
                                catch
                                {
                                    CommonFunctions.ERROR("FactionBank Fail: at ItemExchange");
                                }
                            }
                        }


                        break;


                    case CmdId.Event_DialogButtonIndex:
                        //All of This is a Guess
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_ShowDialog_SinglePlayer, (ushort)CurrentSeqNr, new IdMsgPrio( [int nId], [string nMsg], [byte nPrio], [float nTime] )); //for Prio: 0=Red, 1=Yellow, 2=Blue
                        //Save/Pos = 0, Close/Cancel/Neg = 1
                        IdAndIntValue Received_DialogButtonIndex = (IdAndIntValue)data;
                        break;


                    case CmdId.Event_Player_Credits:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_Credits, (ushort)CurrentSeqNr, new Id( [PlayerID] ));
                        IdCredits Received_PlayerCredits = (IdCredits)data;
                        break;


                    case CmdId.Event_Player_GetAndRemoveInventory:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_GetAndRemoveInventory, (ushort)CurrentSeqNr, new Id( [playerID] ));
                        Inventory Received_PlayerGetRemoveInventory = (Inventory)data;
                        break;


                    case CmdId.Event_Playfield_List:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Playfield_List, (ushort)CurrentSeqNr, null));
                        PlayfieldList Received_PlayfieldList = (PlayfieldList)data;
                        break;


                    case CmdId.Event_Playfield_Stats:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Playfield_Stats, (ushort)CurrentSeqNr, new PString( [Playfield Name] ));
                        PlayfieldStats Received_PlayfieldStats = (PlayfieldStats)data;
                        break;


                    case CmdId.Event_Playfield_Entity_List:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Playfield_Entity_List, (ushort)CurrentSeqNr, new PString( [Playfield Name] ));
                        PlayfieldEntityList Received_PlayfieldEntityList = (PlayfieldEntityList)data;
                        break;


                    case CmdId.Event_Dedi_Stats:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Dedi_Stats, (ushort)CurrentSeqNr, null));
                        DediStats Received_DediStats = (DediStats)data;
                        break;


                    case CmdId.Event_GlobalStructure_List:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_GlobalStructure_List, (ushort)CurrentSeqNr, null));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_GlobalStructure_Update, (ushort)CurrentSeqNr, new PString( [Playfield Name] ));
                        GlobalStructureList Received_GlobalStructureList = (GlobalStructureList)data;
                        break;


                    case CmdId.Event_Entity_PosAndRot:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_PosAndRot, (ushort)CurrentSeqNr, new Id( [EntityID] ));
                        IdPositionRotation Received_EntityPosRot = (IdPositionRotation)data;
                        break;


                    case CmdId.Event_Get_Factions:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Get_Factions, (ushort)CurrentSeqNr, new Id( [int] )); //Requests all factions from a certain Id onwards. If you want all factions use Id 1.
                        FactionInfoList Received_FactionInfoList = (FactionInfoList)data;
                        break;


                    case CmdId.Event_NewEntityId:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_NewEntityId, (ushort)CurrentSeqNr, null));
                        Id Request_NewEntityId = (Id)data;
                        break;


                    case CmdId.Event_Structure_BlockStatistics:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Structure_BlockStatistics, (ushort)CurrentSeqNr, new Id( [EntityID] ));
                        IdStructureBlockInfo Received_StructureBlockStatistics = (IdStructureBlockInfo)data;
                        break;


                    case CmdId.Event_AlliancesAll:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_AlliancesAll, (ushort)CurrentSeqNr, null));
                        AlliancesTable Received_AlliancesAll = (AlliancesTable)data;
                        break;


                    case CmdId.Event_AlliancesFaction:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_AlliancesFaction, (ushort)CurrentSeqNr, new AlliancesFaction( [int nFaction1Id], [int nFaction2Id], [bool nIsAllied] ));
                        AlliancesFaction Received_AlliancesFaction = (AlliancesFaction)data;
                        break;


                    case CmdId.Event_BannedPlayers:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_GetBannedPlayers, (ushort)CurrentSeqNr, null ));
                        BannedPlayerData Received_BannedPlayers = (BannedPlayerData)data;
                        break;


                    case CmdId.Event_GameEvent:
                        //Triggered by PDA Events
                        GameEventData Received_GameEvent = (GameEventData)data;
                        break;


                    case CmdId.Event_Ok:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_SetInventory, (ushort)CurrentSeqNr, new Inventory(){ [changes to be made] });
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_AddItem, (ushort)CurrentSeqNr, new IdItemStack(){ [changes to be made] });
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_SetCredits, (ushort)CurrentSeqNr, new IdCredits( [PlayerID], [Double] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_AddCredits, (ushort)CurrentSeqNr, new IdCredits( [PlayerID], [+/- Double] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Blueprint_Finish, (ushort)CurrentSeqNr, new Id( [PlayerID] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Blueprint_Resources, (ushort)CurrentSeqNr, new BlueprintResources( [PlayerID], [List<ItemStack>], [bool ReplaceExisting?] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_Teleport, (ushort)CurrentSeqNr, new IdPositionRotation( [EntityId OR PlayerID], [Pvector3 Position], [Pvector3 Rotation] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_ChangePlayfield , (ushort)CurrentSeqNr, new IdPlayfieldPositionRotation( [EntityId OR PlayerID], [Playfield],  [Pvector3 Position], [Pvector3 Rotation] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_Destroy, (ushort)CurrentSeqNr, new Id( [EntityID] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_Destroy2, (ushort)CurrentSeqNr, new IdPlayfield( [EntityID], [Playfield] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_SetName, (ushort)CurrentSeqNr, new Id( [EntityID] )); Wait, what? This one doesn't make sense. This is what the Wiki says though.
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_Spawn, (ushort)CurrentSeqNr, new EntitySpawnInfo()); Doesn't make sense to me.
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Structure_Touch, (ushort)CurrentSeqNr, new Id( [EntityID] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CurrentSeqNr, new IdMsgPrio( [int nId], [string nMsg], [byte nPrio], [float nTime] )); //for Prio: 0=Red, 1=Yellow, 2=Blue
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_InGameMessage_Faction, (ushort)CurrentSeqNr, new IdMsgPrio( [int nId], [string nMsg], [byte nPrio], [float nTime] )); //for Prio: 0=Red, 1=Yellow, 2=Blue
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CurrentSeqNr, new IdMsgPrio( [int nId], [string nMsg], [byte nPrio], [float nTime] )); //for Prio: 0=Red, 1=Yellow, 2=Blue
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CurrentSeqNr, new PString( [Telnet Command] ));

                        //uh? Not Listed in Wiki... Received_ = ()data;
                        break;


                    case CmdId.Event_Error:
                        //Triggered when there is an error coming from the API
                        ErrorInfo Received_ErrorInfo = (ErrorInfo)data;
                        if (SeqNrStorage.Keys.Contains(seqNr))
                        {
                            CommonFunctions.LogFile("Debug.txt", "API Error:");
                            CommonFunctions.LogFile("Debug.txt", "ErrorType: " + Received_ErrorInfo.errorType);
                            CommonFunctions.LogFile("Debug.txt", "");
                        }
                        break;


                    case CmdId.Event_PdaStateChange:
                        //Triggered by PDA: chapter activated/deactivated/completed
                        PdaStateInfo Received_PdaStateChange = (PdaStateInfo)data;
                        break;


                    case CmdId.Event_ConsoleCommand:
                        //Triggered when a player uses a Console Command in-game
                        ConsoleCommandInfo Received_ConsoleCommandInfo = (ConsoleCommandInfo)data;
                        break;


                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                CommonFunctions.LogFile("ERROR.txt", "Message: " + ex.Message);
                CommonFunctions.LogFile("ERROR.txt", "Data: " + ex.Data);
                CommonFunctions.LogFile("ERROR.txt", "HelpLink: " + ex.HelpLink);
                CommonFunctions.LogFile("ERROR.txt", "InnerException: " + ex.InnerException);
                CommonFunctions.LogFile("ERROR.txt", "Source: " + ex.Source);
                CommonFunctions.LogFile("ERROR.txt", "StackTrace: " + ex.StackTrace);
                CommonFunctions.LogFile("ERROR.txt", "TargetSite: " + ex.TargetSite);
                CommonFunctions.LogFile("ERROR.txt", "");
            }
        }
        public void Game_Update()
        {
            //Triggered whenever Empyrion experiences "Downtime", roughly 75-100 times per second
        }
        public void Game_Exit()
        {
            //Triggered when the server is Shutting down. Does NOT pause the shutdown.
        }

        public void Init(IModApi modAPI)
        {
            modApi = modAPI;
            if (modApi.Application.Mode == ApplicationMode.DedicatedServer)
            {
                modApi.Application.ChatMessageSent += Application_ChatMessageSent;
                modApi.Network.RegisterReceiverForPlayfieldPackets(PlayfieldDataReceiver);
            }
        }

        private void PlayfieldDataReceiver(string sender, string playfieldName, byte[] data)
        {
            if(sender == "SubscriptionVerifier")
            {
                string IncommingData = CommonFunctions.ConvertByteArrayToString(data);
                if ( IncommingData.StartsWith("Expiration "))
                {
                    int NewExpiration = int.Parse(IncommingData.Split(' ')[1]);
                    Expiration = NewExpiration;
                }
            }
        }

        private void Application_ChatMessageSent(MessageData chatMsgData)
        {
            if (!Disable)
            {

                string msg = chatMsgData.Text.ToLower();
                if (msg == "/factionbank reinit" || msg == "/fb reinit") //Reinitialize
                {
                    SetupYaml.Setup();
                    API.ServerTell(chatMsgData.SenderEntityId, ModShortName, "Reinitialized", true);
                }
                else if (msg == "/mods" || msg == "!mods")
                {
                    //API.Chat("Player", Received_ChatInfo.playerId, ModVersion);
                    API.ServerTell(chatMsgData.SenderEntityId, ModShortName, ModVersion, true);
                }
                else if (msg == SetupYamlData.Command.ToLower())
                {
                    bool open = true;
                    try
                    {
                        if (chatMsgData.Channel == Eleon.MsgChannel.Global && SetupYamlData.BlockedChannels.Contains("Global"))
                        {
                            open = false;
                            API.ServerTell(chatMsgData.SenderEntityId, ModShortName, "This mod cannot be used in the Global chat channel", true);
                        }
                        else if (chatMsgData.Channel == Eleon.MsgChannel.Faction && SetupYamlData.BlockedChannels.Contains("Faction"))
                        {
                            open = false;
                            API.ServerTell(chatMsgData.SenderEntityId, ModShortName, "This mod cannot be used in the Faction chat channel", true);
                        }
                        else if (chatMsgData.Channel == Eleon.MsgChannel.Alliance && SetupYamlData.BlockedChannels.Contains("Alliance"))
                        {
                            open = false;
                            API.ServerTell(chatMsgData.SenderEntityId, ModShortName, "This mod cannot be used in the Alliance chat channel", true);
                        }
                        else if (chatMsgData.Channel == Eleon.MsgChannel.SinglePlayer && SetupYamlData.BlockedChannels.Contains("Private"))
                        {
                            open = false;
                            API.ServerTell(chatMsgData.SenderEntityId, ModShortName, "This mod cannot be used in the Private chat channel", true);
                        }
                        else if (chatMsgData.Channel == Eleon.MsgChannel.Server && SetupYamlData.BlockedChannels.Contains("Server"))
                        {
                            open = false;
                            API.ServerTell(chatMsgData.SenderEntityId, ModShortName, "This mod cannot be used in the Server chat channel", true);
                        }
                    }
                    catch { }

                    try
                    {
                        if (open)
                        {
                            ChatInfo newChatInfo = new ChatInfo
                            {
                                msg = chatMsgData.Text,
                                playerId = chatMsgData.SenderEntityId
                            };
                            Storage.StorableData function = new Storage.StorableData
                            {
                                function = "FactionBank",
                                Match = Convert.ToString(chatMsgData.SenderEntityId),
                                Requested = "PlayerInfo",
                                ChatInfo = newChatInfo
                            };
                            API.PlayerInfo(chatMsgData.SenderEntityId, function);
                        }
                    }
                    catch
                    {
                        CommonFunctions.Debug("FactionBank Fail: at ChatInfo");
                    }
                }
            }
        }

        public void Shutdown()
        {
        }
    }
}