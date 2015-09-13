using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SQLite;
using System.Collections.Generic;
using System.Collections;

namespace IMessageFinder
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			//verify that a phone number has been specified.
			if (args.Length != 1) {
				Error(
					"Exactly one arguement must be specified (phone number).");
				return;
			}

			//verify that the phone number pattern matches. If not, blow up.
			var regex = new Regex (@"\d{10}");
			if (!regex.IsMatch (args [0])) {
				Error (
					"The phone number must be specified with the format: 1234567890.");
				return;
			}
		
			//now lets check the OS and file path we need to work with.
			var system = "Unknown system type.";
			var backupDirRoot = string.Empty;

			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				//running on a mac
				system = "Mac OSX";
				var user = Environment.GetFolderPath (Environment.SpecialFolder.Personal);

				backupDirRoot = Path.Combine(user, 
					"Library/Application Support/MobileSync/Backup/");
			}
			else if(Environment.OSVersion.Platform == PlatformID.Win32NT){
				//running on windows
				system = "Windows";
				backupDirRoot = Path.Combine (
					Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData),
					@"Apple Computer\MobileSync\Backup\");
			}

			Info ("Determined OS is: {0}", system);

			try{
				//find the db file that we need to open.
				var di = new DirectoryInfo (backupDirRoot);
				var deviceBackupFolder = di.GetDirectories ().First ();
				const string iMesssageDB = "3d0d7e5fb2ce288813306e4d4636395e047a3d28";

				Info ("Found backup location: {0}", deviceBackupFolder.FullName);

				//get the sqlite file we need to work with.
				var iMessageDBFile = deviceBackupFolder.GetFiles ()
					.FirstOrDefault (f => f.Name == iMesssageDB);

				if (iMessageDBFile != null) {
					//this is the db file to work with.
					Info ("Opening iMessage database at: {0}", iMessageDBFile.FullName);

					//the output of all the records as text.
					var fileOutputResult = string.Empty;

					//now open it...
					using (var conn = new SQLiteConnection (iMessageDBFile.FullName)) {
						//the query to get the messages.
						var query = string.Format (
							" SELECT message.is_from_me as IsFromMe, message.text as Text" +
							" FROM message, handle" +
							" WHERE message.handle_id = handle.ROWID" +
							" AND handle.id = '+1{0}'" +
							" ORDER BY message.date;", args [0]);
						//run the sql.
						var data = conn.Query<SqlResult>(query, new object[]{});
						var sb = new StringBuilder ();
						foreach (var row in data) {
							if (!string.IsNullOrWhiteSpace (row.Text)) {
								if (row.IsFromMe) {
									sb.AppendLine ("[From Me] " + row.Text);
								} else {
									sb.AppendLine (string.Format("[From {0}] {1}", args[0], row.Text));
								}
							}
						}
						fileOutputResult = sb.ToString ();
					}

					//now write to file
					var outputFile = Path.Combine (
						Environment.GetFolderPath (Environment.SpecialFolder.Desktop),
						string.Format ("iMessageHistory-{0}.txt", args [0]));

					File.WriteAllText (outputFile, fileOutputResult);
					Success ("Wrote messages to file: {0}", outputFile);
					Exit ();
				} else {
					Error (
						"No iMessage database could be found. Please make sure you have backed up through iTunes and try again.");
				}
			}
			catch(DirectoryNotFoundException e){
				Error("No backup location could be determined. Please use iTunes to back up and try again.");
			}
			catch(FileNotFoundException e){
				Error ("Could not locate the messages database. Please try backing up the device again.");
			}
			catch(Exception e){
				Error (e.Message);
			}
		}

		static void Success(string format, params string[] data){
			Console.ResetColor ();

			Console.WriteLine ();
			Console.WriteLine (format, data);
		}
			
		static void Info(string format, params string[] data){
			Console.ForegroundColor = ConsoleColor.Yellow;

			Console.WriteLine ();
			Console.WriteLine (format, data);

			Console.ResetColor ();
		}

		static void Error(string message){
			Console.ForegroundColor = ConsoleColor.Red;

			Console.WriteLine ();
			Console.WriteLine (message);

			Console.ResetColor ();
			Exit ();
		}

		static void Exit(){
			Console.WriteLine ();
			Console.WriteLine ("Press any key to exit.");
			Console.ReadKey ();
		}
	}
	class SqlResult 
	{
		public bool IsFromMe {get;set;}
		public string Text {get;set;}
	}
}
