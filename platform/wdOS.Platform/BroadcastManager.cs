using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform
{
    public static class BroadcastManager
    {
        public const uint MaxBroadcastSize = 1024;
        public static string BroadcastPath = "0:/PrivateSystem/broadcasts.bin";
        public static bool SaveDatabase;
        private static List<Broadcast> allBroadcasts;
        private static bool initialized = false;
        public static void Initialize()
        {
            if (!initialized)
            {
                allBroadcasts = new List<Broadcast>();
                LoadBroadcasts();
                initialized = true;
            }
        }
        public static void SendBroadcast(User to, string subject, string message)
        {
            allBroadcasts.Add(new()
            {
                Message = message,
                Subject = subject,
                Sender = UserManager.CurrentUser,
                Sendee = to,
                SendTime = DateTime.Now,
            });
            PlatformManager.Log($"sent broadcast from {UserManager.CurrentUser.Username} to {to.Username}", "braodcastmanager");
        }
        public static Broadcast[] GetAvailableBroadcasts()
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
        public static void LoadBroadcasts() 
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
        public static void SaveBroadcasts()
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
    public struct Broadcast
    {
        public string Subject;
        public string Message;
        public DateTime SendTime;
        public User Sender;
        public User Sendee;
        public Broadcast(byte[] bytes)
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
        public byte[] ToBytes()
        {
            byte[] bytes = new byte[BroadcastManager.MaxBroadcastSize];
            BinaryWriter bw = new(new MemoryStream(bytes));
            bw.Write((byte)Subject.Length);
            bw.Write((byte)Message.Length);
            bw.Write((byte)Sender.Username.Length);
            bw.Write((byte)Sendee.Username.Length);
            bw.Write(Subject.ToCharArray());
            bw.Write(Message.ToCharArray());
            bw.Write(Sender.Username.ToCharArray());
            bw.Write(Sendee.Username.ToCharArray());
            bw.Write(SendTime.Ticks);
            return bytes[0..new((int)bw.BaseStream.Position)];
        }
    }
}
