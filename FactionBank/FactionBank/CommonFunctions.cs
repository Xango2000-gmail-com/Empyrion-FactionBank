using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eleon.Modding;

namespace FactionBank
{
    class CommonFunctions
    {
        internal class ItemStackX
        {
            internal int slotIdx;
            internal int id;
            internal long count;
            internal int ammo;
            internal int decay;
        }

        internal static void LogFile(string FileName, string FileData)
        {
            if (!File.Exists(MyEmpyrionMod.ModPath + FileName))
            {
                try
                {
                    using (FileStream fs = File.Create(MyEmpyrionMod.ModPath + FileName)) { }
                }
                catch
                {
                    File.WriteAllText(MyEmpyrionMod.ModPath + "Debug.txt", "File Doesnt Exist");
                }
            }
            File.AppendAllText(MyEmpyrionMod.ModPath + FileName, FileData + Environment.NewLine);
        }

        internal static void Debug(string Data)
        {
            if (MyEmpyrionMod.debug)
            {
                LogFile("Debug.txt", Data);
            }
        }

        internal static void Log(string Data)
        {
            LogFile("Log.txt", Data);
        }

        internal static void ERROR(string Data)
        {
            LogFile("ERROR.txt", Data);
        }

        internal static int SeqNrGenerator(int LastSeqNr)
        {
            bool Fail = false;
            int CurrentSeqNr = 2000;
            do
            {
                if (LastSeqNr > 65530)
                {
                    LastSeqNr = 2000;
                }
                CurrentSeqNr = LastSeqNr + 1;
                if (MyEmpyrionMod.SeqNrStorage.ContainsKey(CurrentSeqNr)) { Fail = true; }
            } while (Fail == true);
            return CurrentSeqNr;
        }

        internal static string ArrayConcatenate(int start, string[] array)
        {
            string message = "";
            for (int i = start; i < array.Length; i++)
            {
                message = message + "\r\n";
                message = message + array[i];
            }
            return message;
        }

        internal static string ArrayToString(int start, string[] array)
        {
            string message = "";
            for (int i = start; i < array.Length; i++)
            {
                message = message + " " + array[i];
            }
            return message;
        }

        public static void FileReader(ushort ThisSeqNr, string File)
        {
            //Checks for simple errors
            string[] Script1 = System.IO.File.ReadAllLines(File);
            for (int i = 0; i < Script1.Count(); ++i)
            {

            }
        }

        public static string ChatmessageHandler(string[] Chatmessage, string Selector)
        {
            List<string> Restring = new List<string>(Chatmessage);
            string Picked = "";
            if (Selector.Contains('*'))
            {
                if (Selector == "1*")
                {
                    Restring.Remove(Restring[0]);
                    Picked = string.Join(" ", Restring.ToArray());
                }
                else if (Selector == "2*")
                {
                    Restring.Remove(Restring[1]);
                    Restring.Remove(Restring[0]);
                    Picked = string.Join(" ", Restring.ToArray());
                }
                else if (Selector == "3*")
                {
                    Restring.Remove(Restring[2]);
                    Restring.Remove(Restring[1]);
                    Restring.Remove(Restring[0]);
                    Picked = string.Join(" ", Restring.ToArray());
                }
                else if (Selector == "4*")
                {
                    Restring.Remove(Restring[3]);
                    Restring.Remove(Restring[2]);
                    Restring.Remove(Restring[1]);
                    Restring.Remove(Restring[0]);
                    Picked = string.Join(" ", Restring.ToArray());
                }
                else if (Selector == "5*")
                {
                    Restring.Remove(Restring[4]);
                    Restring.Remove(Restring[3]);
                    Restring.Remove(Restring[2]);
                    Restring.Remove(Restring[1]);
                    Restring.Remove(Restring[0]);
                    Picked = string.Join(" ", Restring.ToArray());
                }
            }
            else
            {

            }
            return Picked;
        }

        public static Dictionary<string, string[]> CSVReader1(string File)
        {
            Dictionary<string, string[]> ItemDB = new Dictionary<string, string[]> { };
            string[] Line = System.IO.File.ReadAllLines(File);
            foreach (string Item in Line)
            {
                string[] itemArray = Item.Split(',');
                ItemDB.Add(itemArray[0], itemArray);
            }
            return ItemDB;
        }

        public static ItemStack[] ReadItemStacks(string FolderPath, string FileName)
        {
            string RealFile = MyEmpyrionMod.ModPath + FolderPath + FileName;
            string[] bagLines = File.ReadAllLines(RealFile);
            List<string> Group1 = bagLines.ToList();
            /* Disabled for this mod
            if (File.Exists(MyEmpyrionMod.ModPath + FolderPath + "Cache.txt"))
            {
                try
                {
                    string[] CacheLines = File.ReadAllLines(MyEmpyrionMod.ModPath + FolderPath + "Cache.txt");
                    List<string> Group2 = CacheLines.ToList();
                    foreach (string GroupItem in Group2)
                    {
                        Group1.Add(GroupItem);
                    }
                }
                catch
                {
                    Debug("cache.txt file read error");
                }
            }*/
            bagLines = Group1.ToArray();
            //int itemStackSize = bagLines.Count();
            List<ItemStack> ItemStackList = new List<ItemStack> { };
            List<ItemStack> ReCacheList = new List<ItemStack> { };
            int LineCounter = 0;
            /*
            bool RunConversion = false;
            if (Directory.Exists(MyEmpyrionMod.ModPath + "BlocksConfig"))
            {
                if (File.Exists(MyEmpyrionMod.ModPath + "BlocksConfig\\Converted.yaml"))
                {
                    if (File.GetLastWriteTimeUtc(MyEmpyrionMod.ModPath + FolderPath + FileName) < File.GetLastWriteTimeUtc(MyEmpyrionMod.ModPath + FolderPath + "BlocksConfig\\Converted.yaml"))
                    {
                        RunConversion = true;
                    }
                }
            }
            */
            foreach (string Line in bagLines)
            {
                if (Line.Contains(','))
                {
                    string[] SplitLine = Line.Split(',');
                    ItemStack NewEntry = new ItemStack
                    {
                        slotIdx = Convert.ToByte(LineCounter),
                        id = int.Parse(SplitLine[1]),
                        count = int.Parse(SplitLine[2]),
                        decay = int.Parse(SplitLine[3]),
                        ammo = int.Parse(SplitLine[4])
                    };
                    ItemStackList.Add(NewEntry);
                }
                LineCounter = LineCounter + 1;
            }
            ItemStack[] itStack = ItemStackList.ToArray();
            try
            {
                if (ReCacheList.Count() != 0)
                {
                    ItemStack[] ReCache = ReCacheList.ToArray();
                    //WriteItemStacksSimple(FolderPath, "Cache.txt", ReCache);
                }
                else
                {
                   //File.WriteAllText(MyEmpyrionMod.ModPath + FolderPath + "Cache.txt", "");
                }
            }
            catch
            {
                Debug("Failed writing cache file");
            }
            return itStack;
        }

        public static void WriteItemStacksSimple(string FolderPath, string FileName, ItemStack[] ItemStacks)
        {
            File.WriteAllText(MyEmpyrionMod.ModPath + FolderPath + FileName, "");
            foreach (ItemStack item in ItemStacks)
            {
                LogFile(FolderPath + FileName, item.slotIdx + "," + item.id + "," + item.count + "," + item.decay + "," + item.ammo);
            }
        }

        public static void WriteItemStacks(string FolderPath, string FileName, ItemStack[] ItemStacks, string SuperStack, int MaxSuperStackSize, bool Cache)
        {
            //Dictionary<int, List<ItemStackX>> SuperStackerRoundOne = new Dictionary<int, List<ItemStackX>> { }; //item.id, List<ItemStacks>
            //Dictionary<int, List<ItemStackX>> SuperStackerRoundTwo = new Dictionary<int, List<ItemStackX>> { };
            Dictionary<int, Dictionary<string, int>> Round1 = new Dictionary<int, Dictionary<string, int>> { }; //Written 03092021
            if (!Directory.Exists(MyEmpyrionMod.ModPath + FolderPath))
            {
                Directory.CreateDirectory(MyEmpyrionMod.ModPath + FolderPath);
            }
            File.WriteAllText(MyEmpyrionMod.ModPath + FolderPath + FileName, "");
            int StackCounter = 0;
            if (SuperStack.ToLower() == "superstack")
            {
                //if Settings.Stack = SuperStack
                int MaxSuperstack = 2000000000;
                if (MaxSuperStackSize < MaxSuperstack + 1)
                {
                    MaxSuperstack = MaxSuperStackSize;
                }
                LogFile("Debug.txt", "##### Round 1 #####");
                //Round 1: Superstack Everything
                foreach(ItemStack Stack in ItemStacks)
                {
                    if (Round1.Keys.Contains(Stack.id))
                    {
                        if (Round1[Stack.id].Keys.Contains(Stack.ammo +"," + Stack.decay))
                        {
                            Round1[Stack.id][Stack.ammo + "," + Stack.decay] = Round1[Stack.id][Stack.ammo + "," + Stack.decay] + Stack.count;
                        }
                        else
                        {
                            Round1[Stack.id][Stack.ammo + "," + Stack.decay] = Stack.count;
                        }
                    }
                    else
                    {
                        Dictionary<string, int> NewStack = new Dictionary<string, int>
                        {
                            {(Stack.ammo + "," + Stack.decay), Stack.count }
                        };
                        Round1[Stack.id] = NewStack;
                    }
                }
                foreach (int ItemID in Round1.Keys)
                {
                    foreach (string AmmoDecay in Round1[ItemID].Keys)
                    {
                        int Ammo = int.Parse(AmmoDecay.Split(',')[0]);
                        int Decay = int.Parse(AmmoDecay.Split(',')[1]);
                        if (StackCounter < 50)
                        {
                            LogFile(FolderPath + FileName, StackCounter + "," + ItemID + "," + Round1[ItemID][AmmoDecay] + "," + Decay + "," + Ammo);
                        }
                        else
                        {
                            if (Cache)
                            {
                                LogFile(FolderPath + "Cache.txt", StackCounter + "," + ItemID + "," + Round1[ItemID][AmmoDecay] + "," + Decay + "," + Ammo);
                            }
                        }
                        StackCounter++;
                    }
                }
            }
            else
            {
                //if Settings.stack = false
                Dictionary<int, List<ItemStackX>> SuperStackerRoundOne = new Dictionary<int, List<ItemStackX>> { }; //item.id, List<ItemStacks>
                foreach (ItemStack stack in ItemStacks)
                {
                    List<ItemStackX> ListItemStackX2 = new List<ItemStackX> { };
                    if (SuperStackerRoundOne.Keys.Contains(stack.id))
                    {
                        ListItemStackX2 = SuperStackerRoundOne[stack.id];
                    }
                    ItemStackX NewItemStack = new ItemStackX
                    {
                        count = stack.count,
                        id = stack.id,
                        ammo = stack.ammo,
                        decay = stack.decay,
                        slotIdx = stack.slotIdx
                    };
                    ListItemStackX2.Add(NewItemStack);
                    SuperStackerRoundOne[stack.id] = ListItemStackX2;
                }
                foreach (List<ItemStackX> SuperStacker1 in SuperStackerRoundOne.Values)
                {
                    foreach (ItemStackX ItemStackOne in SuperStacker1)
                    {
                        if (StackCounter < 50)
                        {
                            LogFile(FolderPath + FileName, ItemStackOne.slotIdx + "," + ItemStackOne.id + "," + ItemStackOne.count + "," + ItemStackOne.decay + "," + ItemStackOne.ammo);
                        }
                        else
                        {
                            if (Cache)
                            {
                                LogFile(FolderPath + "Cache.txt", ItemStackOne.slotIdx + "," + ItemStackOne.id + "," + ItemStackOne.count + "," + ItemStackOne.decay + "," + ItemStackOne.ammo);
                            }
                        }
                        StackCounter++;
                    }
                }
            }
            // Round 3: Write To File
        }

        public static string SplitChat2(string ChatMessage)
        {
            string[] splitted = ChatMessage.Split(new[] { ' ' }, 2);
            string message = splitted[1];
            return message;
        }

        public static string UnixTimeStamp()
        {
            string time = Convert.ToString((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            return time;
        }

        public static string TimeStamp()
        {
            //string Timestamp = CommonFunctions.UnixTimeStamp();
            DateTime Timestamp2 = CommonFunctions.UnixTimeStampToDateTime(Convert.ToDouble(CommonFunctions.UnixTimeStamp()));
            return Timestamp2.ToString("yyyy/MM/dd HH:mm:ss ffff");
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static byte[] ConvertToByteArray(string input)
        {
            Char[] CharArray = input.ToCharArray();
            List<byte> ListByte = new List<byte> { };
            foreach (char Character in CharArray)
            {
                ListByte.Add(Convert.ToByte(Character));
            }
            byte[] data = ListByte.ToArray();
            return data;
        }

        public static string ConvertByteArrayToString(byte[] input)
        {
            string output = "";
            foreach (char character in input)
            {
                output = output + character.ToString();
            }
            return output;
        }

    }
}

