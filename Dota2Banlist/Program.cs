using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Dota2BanlistCore;
using Dota2BanlistCore.Db;
using Ninject;
using MoreLinq;

namespace Dota2Banlist
{
    static class Program
    {
        public class MainApp
        {

        }

        [STAThread]
        static void Main()
        {
            //var kernal = new StandardKernel();
            //kernal.Bind<Dota2BanlistCore.Db.BanlistDbContext>().ToConstant(new Dota2BanlistCore.Db.BanlistDbContext());

            var matches = new List<Match>();
            using (var server = new ServerLogMatchProvider(new FileInfo(@"E:\Steam\steamapps\common\dota 2 beta\dota\server_log.txt")))
            {
                server.MatchesFound += (sender, args) => matches.AddRange(args.Matches);
                server.ReadLatestEntries();
            }


            Process.GetCurrentProcess().WaitForExit();
        }


        static void SteamIdsToDatabase()
        {
            var dbPath = Path.Combine(Environment.CurrentDirectory, "test.db");

            using (var conn = new SQLiteConnection("Data Source=" + dbPath))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = CreateTableQuery;
                    cmd.ExecuteNonQuery();
                }
            }

            var matches = new List<Match>();
            using (var server = new ServerLogMatchProvider(new FileInfo(@"E:\Steam\steamapps\common\dota 2 beta\dota\server_log.txt")))
            {
                server.MatchesFound += (sender, args) => matches.AddRange(args.Matches);
                server.ReadLatestEntries();
            }

            var context = new BanlistDbContext(new SQLiteConnection("Data Source=" + dbPath));
            context.Configuration.AutoDetectChangesEnabled = false;

            var steamIds = matches
                .SelectMany(m => m.SteamIds)
                .DistinctBy(s => s.ToEncodedId())
                .Select(s => new SteamIds { SteamIdEncoded = s.ToEncodedId(), SteamId_Y = s.Y, SteamId_Z = s.Z })
                .ToList();

            foreach (var sid in steamIds)
                context.SteamIds.Add(sid);

            for (int i = 0; i < matches.Count; i++)
            {
                var m = new Matches { LobbyId = matches[i].LobbyId };
                context.Matches.Add(m);

                for (int o = 0; o < matches[i].SteamIds.Count; o++)
                {
                    var enc = matches[i].SteamIds[o].ToEncodedId();
                    var id = steamIds.Single(s => s.SteamIdEncoded == enc);

                    context.MatchPlayers.Add(new MatchPlayers { Match = m, PlayerIndex = o, SteamId = id });
                }
            }

            context.SaveChanges();
        }

        const string CreateTableQuery = @"
    
	DROP TABLE if exists [Matches];
    
	DROP TABLE if exists [SteamIds];
    
	DROP TABLE if exists [MatchPlayers];

CREATE TABLE [Matches] (
    [Id] integer PRIMARY KEY AUTOINCREMENT  NOT NULL ,
    [LobbyId] integer   NOT NULL 
);
CREATE TABLE [SteamIds] (
    [Id] integer PRIMARY KEY AUTOINCREMENT  NOT NULL ,
    [SteamId_Y] integer   NOT NULL ,
    [SteamId_Z] integer   NOT NULL ,
    [SteamIdEncoded] integer UNIQUE  NOT NULL ,
    [CurrentName] nvarchar(2147483647)   NULL 
);
CREATE TABLE [MatchPlayers] (
    [Id] integer PRIMARY KEY AUTOINCREMENT  NOT NULL ,
    [PlayerIndex] integer   NOT NULL ,
    [Match_Id] integer   NOT NULL ,
    [SteamId_Id] integer   NOT NULL 
			
		,CONSTRAINT [FK_MatchPlayersMatches]
    		FOREIGN KEY ([Match_Id])
    		REFERENCES [Matches] ([Id])					
    		
						
		,CONSTRAINT [FK_MatchPlayersSteamIds]
    		FOREIGN KEY ([SteamId_Id])
    		REFERENCES [SteamIds] ([Id])					
    		
			);";
    }
}
