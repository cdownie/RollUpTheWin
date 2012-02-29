using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manatee;
using System.IO;
using System.Text.RegularExpressions;

namespace VidPub.Tasks {
    class Program {
        static Migrator _development;
        static Migrator _test;
        static Migrator _production;
        //this will allow you to sync a test DB with your dev DB by running the same migrations
        //there
        static bool _syncTestDB = false;

        static List<string> _args;

        static void Main(string[] args) {
            var migrationDir = LocateMigrations();

            _args = args.ToList();
            _development = new Migrator(migrationDir, "development");
            _test = new Migrator(migrationDir, "test",silent:true);
            _production = new Migrator(migrationDir, "production");
            SayHello();
        }
        static string GetNameStub() {
            var nextMigration = _development.LastVersion+1;
            if (nextMigration < 10) {
                return string.Format("00{0}", nextMigration);
            } else if (nextMigration < 100) {
                return string.Format("0{0}", nextMigration);
            } else {
                return string.Format("{0}", nextMigration);
            }
        }
        static void Generate(string name) {
            //drop the name to lower, add _'s, and an extension
            var splits = name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            var fileName = name.ToLower().Replace(" ", "_") + ".js";
            var template = Templates.Blank;
            
            //timestamp it
            var formattedName = string.Format("{0}_{1}", GetNameStub(), fileName);
            if (fileName.StartsWith("create")) {
                template = Templates.CreateTable;
                var tableName = name.Replace("create_", "").Replace("Create_","");
                template = template.Replace("my_table", tableName);
            } else if (fileName.StartsWith("add")) {
                template = Templates.AddColumn;

            } else if (fileName.StartsWith("index")) {
                template = Templates.AddIndex;

            } else if (fileName.StartsWith("fk") || fileName.StartsWith("foreign_key")) {
                template = Templates.FK;

            }
            var migrationPath = Path.Combine(LocateMigrations(),formattedName);
            using (var stream = new FileStream(migrationPath, FileMode.Create)) {
                var chars = template.ToCharArray();
                var bits = new ASCIIEncoding().GetBytes(chars);
                stream.Write(bits,0,bits.Length);
            }
        }
        static int WhichVersion(string command) {
            int result = -1;
            var stems = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < stems.Length; i++) {
                var bit = stems[i];
                if (bit == "/v")
                    int.TryParse(stems[i + 1], out result);
            }

            return result;
        }
        static bool ShouldExecute(string command) {
            var stems = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < stems.Length; i++) {
                var bit = stems[i];
                if (bit == "/p")
                    return false;
            }

            return true ;
        }
        static void DecideWhatToDo(string command){
            int version = WhichVersion(command);
            bool shouldExecute = ShouldExecute(command);
            if(command.StartsWith("up")){
                if (version < 0)
                    version = _development.LastVersion;
                //roll it to the top
                _development.Migrate(version, execute: shouldExecute);
                if(_syncTestDB)
                    _test.Migrate(version, execute: shouldExecute);

            }else if(command.StartsWith("g") || command.StartsWith("c")){
                Console.WriteLine("Generating a Migration...");
                var name = command.Replace("g ", "").Replace("c ", "");
                Generate(name);

            }else if(command.StartsWith("down") || command.StartsWith("back") || command.StartsWith("rollback")){
                //go back one if the version isn't specified
                if (version < 0)
                    version = _development.CurrentVersion - 1;
                _development.Migrate(version, execute: shouldExecute);
                if (_syncTestDB)
                    _test.Migrate(version, execute: shouldExecute);
            } else if (command.StartsWith("migrate")) {
                if (version < 0)
                    version = _development.LastVersion;
                _development.Migrate(version, execute: shouldExecute);
                if (_syncTestDB)
                    _test.Migrate(version, execute: shouldExecute);
            } else if (command.StartsWith("exit") || command.StartsWith("quit")) {
                Environment.Exit(1);
                return;
            }else if(command.StartsWith("list")){
                ListMigrations();
            } else if (command.StartsWith("push")) {
                //send it to production
                _production.Migrate();
            }else{
                HelpEmOut();
            }

            Console.WriteLine("Done! What next?");
            AskOrStepNextArg();
        }
        static void ListMigrations(){
            var migrationDir = new DirectoryInfo(LocateMigrations());
            var files = migrationDir.GetFiles();
            var reg = new Regex("\\d");
            var counter = 1;
            var currentVersion = _development.CurrentVersion;
            foreach (var file in files)
	        {
                var wasRun = "-";
                if (counter > currentVersion)
                    wasRun = "+";
                var fileName = Path.GetFileNameWithoutExtension(file.Name);
                var migName = reg.Replace(fileName, "").Trim().Replace("_", " ");
                Console.WriteLine("{0} {1}",wasRun,migName);
                counter++;
	        }
        }
        static void HelpEmOut(){
            Console.WriteLine("You can say 'up', 'down', or 'migrate' with some arguments. Those arguments are:");
            Console.WriteLine(" ... /v - this is the version number to go up or down to. To wipe our your DB, /v 0");
            Console.WriteLine(" ... /p - Print out the commands only");
            Console.WriteLine(" ... 'back' or rollback goes back a single version");
            Console.WriteLine(" ... 'up' will run every migration not run");
            Console.WriteLine(" ... 'exit' or 'quit' will... well you know.");
            Console.WriteLine(" ... 'list' will roll out a list of all the migrations");
            Console.WriteLine(" ... 'create', 'generate', or just 'c' or 'g' stub out a template for you and stick it in your migrations directory");
            Console.WriteLine(" ... 'push' will send your database changes up to your Production box");
            Console.WriteLine(" ...  When you generate a migration, be sure to use good naming - I'll do my best to stup it out for you.");
            Console.WriteLine(" ...  - for instance, if you use a name like 'create_mytable' - I'll know to stub out create syntax for you");
            Console.WriteLine(" ...  - the trick is to start the name with the operation. If you do, I'll do the right thing");
            Console.WriteLine(" ...  - create_ will do a create table, add_ will add a column, index_ will create an index template, fk_ will create an FK template");
            Console.WriteLine(" ...  - finally - if you name it with _'s, I'll do my best to figure out a table, column, or index name");
            Console.WriteLine("----------------------------------------------------------------------------------");
            Console.WriteLine("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
            AskOrStepNextArg();
       }
        static void SayHello() {
            Console.WriteLine("Manatee - Migrations for .NET");
            Console.WriteLine("Current DB Version: {0}",_development.CurrentVersion);
            Console.WriteLine("You have {0} migrations with {1} un-run. Type 'list' to see more details", _development.LastVersion, _development.LastVersion - _development.CurrentVersion);
            Console.WriteLine(">> (type 'h' or 'help' for assistance)");
            AskOrStepNextArg();
        }



        static void AskOrStepNextArg()
        {
            string command = string.Empty;
            if (_args.Any())
            {
                command = _args[0];
                Console.WriteLine(command);
                _args.RemoveAt(0);
            }

            if (string.IsNullOrWhiteSpace(command))
            {
                command = Console.ReadLine();
            }
            DecideWhatToDo(command);
        }


        static string LocateMigrations() {
            //this is the bin/release or bin/debug
            var binDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            //get the root - go up two levels
            var rootDirectory = binDirectory.Parent.Parent;
            //return the Migrations directory
            return Path.Combine(rootDirectory.FullName, "Migrations");

        }
    }
}
