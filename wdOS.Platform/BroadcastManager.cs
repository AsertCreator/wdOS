using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform
{
    internal static class BroadcastManager
    {
        internal const uint MaxBroadcastSize = 1024;
        internal static string BroadcastPath = "0:/PrivateSystem/broadcasts.bin";
        internal static bool SaveDatabase;
        private static List<Broadcast> allBroadcasts;
        private static bool initialized = false;
        internal static void Initialize()
        {
            if (!initialized)
            {
                allBroadcasts = new List<Broadcast>();
                initialized = true;
            }
        }
        internal static void SendBroadcast(UserManager.User to, string subject, string message)
        {
            allBroadcasts.Add(new()
            {
                Message = message,
                Subject = subject,
                Sender = UserManager.CurrentUser,
                Sendee = to,
                SendTime = DateTime.Now,
            });
            PlatformManager.Log($"sent broadcast from {UserManager.CurrentUser.UserName} to {to.UserName}", "braodcastmanager");
        }
        internal static Broadcast[] GetAvailableBroadcasts()
        {
            if (UserManager.CurrentUser.IsRoot) return allBroadcasts.ToArray();

            List<Broadcast> availableBroadcasts = new();
            for (int i = 0; i < allBroadcasts.Count; i++)
            {
                var broadcast = allBroadcasts[i];
                if (broadcast.Sendee == UserManager.CurrentUser ||
                    broadcast.Sender == UserManager.CurrentUser || 
                    broadcast.Sendee == UserManager.EveryoneUser)
                    availableBroadcasts.Add(broadcast);
            }

            return availableBroadcasts.ToArray();
        }
        internal static void LoadBroadcasts() 
        {
            if (!FileSystemManager.FileExists(BroadcastPath))
            {
                SendBroadcast(UserManager.EveryoneUser, "Broadcast Database",
                    "Broadcast Manager notifies you, that it wasn't able to load Broadcast Database, " +
                    "please save your work for PC maintainer to repair it");
                return;
            }
            var entries = ConfigurationManager.LoadConfig(FileSystemManager.ReadBytesFile(BroadcastPath));
            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                allBroadcasts.Add(new Broadcast(entry.ContentBytes));
            }
            PlatformManager.Log("loaded all broadcasts!", "braodcastmanager");
        }
        internal static void SaveBroadcasts()
        {
            List<ConfigurationTableEntry> entries = new();
            for (int i = 0; i < allBroadcasts.Count; i++)
            {
                var broadcast = allBroadcasts[i];
                entries.Add(new()
                {
                    ContentBytes = broadcast.ToBytes(),
                    ContentType = 2,
                    Name = "BroadcastContents"
                });
            }
            File.WriteAllBytes(BroadcastPath, ConfigurationManager.SaveConfig(entries.ToArray()));
            PlatformManager.Log("saved all broadcasts!", "braodcastmanager");
        }
    }
    internal struct Broadcast
    {
        internal string Subject;
        internal string Message;
        internal DateTime SendTime;
        internal UserManager.User Sender;
        internal UserManager.User Sendee;
        internal Broadcast(byte[] bytes)
        {
            BinaryReader br = new(new MemoryStream(bytes));
            var subjectlength = br.ReadByte();
            var messagelength = br.ReadByte();
            var senderlength = br.ReadByte();
            var sendeelength = br.ReadByte();
            Subject = new string(br.ReadChars(subjectlength));
            Message = new string(br.ReadChars(messagelength));
            Sender = UserManager.FindByName(new string(br.ReadChars(senderlength)));
            Sendee = UserManager.FindByName(new string(br.ReadChars(sendeelength)));
            SendTime = new DateTime(br.ReadInt64());
        }
        internal byte[] ToBytes()
        {
            byte[] bytes = new byte[BroadcastManager.MaxBroadcastSize];
            BinaryWriter bw = new(new MemoryStream(bytes));
            bw.Write((byte)Subject.Length);
            bw.Write((byte)Message.Length);
            bw.Write((byte)Sender.UserName.Length);
            bw.Write((byte)Sendee.UserName.Length);
            bw.Write(Subject.ToCharArray());
            bw.Write(Message.ToCharArray());
            bw.Write(Sender.UserName.ToCharArray());
            bw.Write(Sendee.UserName.ToCharArray());
            bw.Write(SendTime.Ticks);
            return bytes[0..new((int)bw.BaseStream.Position)];
        }
    }
}
