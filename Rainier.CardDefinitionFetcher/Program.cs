/*************************************************************************
* Rainier Card Definition Fetcher
* (c) 2022 Hastwell/Electrosheep Networks 
* 
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as published
* by the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Affero General Public License for more details.
* 
* You should have received a copy of the GNU Affero General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
**************************************************************************/

using Platform.Sdk;
using Platform.Sdk.Models.Account;
using Omukade.Tools.RainierCardDefinitionFetcher;
using Spectre.Console;
using static TPCI.PTCS.PTCSUtils;
using Omukade.Tools.RainierCardDefinitionFetcher.Model;
using Newtonsoft.Json;
using Omukade.AutoPAR;
using Omukade.AutoPAR.Rainier;

internal class Program
{
    const string ARG_FETCH_ITEMDB = "--fetch-itemdb";
    const string ARG_FETCH_CARDACTIONS = "--fetch-cardactions";
    const string ARG_FETCH_CARDDB = "--fetch-carddb";
    const string ARG_FETCH_CARDDEFINITIONS = "--fetch-carddefinitions";
    const string ARG_FETCH_CARDDEFINITIONS_SHORTER = "--fetch-carddefs";
    const string ARG_CARDDEFS_IGNORE_INVALID_CARDS_FILE = "--ignore-invalid-ids-file";
    const string ARG_FETCH_RULES = "--fetch-rules";
    const string ARG_FETCH_ALL_QUESTS = "--fetch-allquests";
    const string ARG_FETCH_CURRENT_QUESTS = "--fetch-currentquests";
    const string ARG_FETCH_AIDECKS = "--fetch-aidecks";
    const string ARG_FETCH_DECKVALIDATION = "--fetch-deckvalidation";
    const string ARG_OUTPUT_FOLDER = "--output-folder";
    const string ARG_HELP_LONG = "--help";
    const string ARG_HELP_SHORT = "-h";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    static internal string outputFolder;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private static void Main(string[] args)
    {
        // Load secrets
        SecretsConfig secrets = LoadSecrets();

        Console.WriteLine("Checking for Rainier updates...");
        UpdaterManifest updaterManifest = RainierFetcher.GetUpdateManifestAsync().Result;
        if(RainierFetcher.DoesNeedUpdate(updaterManifest))
        {
            Console.WriteLine($"Update detected; downloading...");
            RainierFetcher.DownloadUpdateFile(updaterManifest).Wait();
            Console.WriteLine("Update downloaded; extracting...");
            RainierFetcher.ExtractUpdateFile(deleteExistingUpdateFolder: true);
            Console.WriteLine("Update complete and extracted.");
        }
        else
        {
            Console.WriteLine("Rainier client is up-to-date");
        }

        Console.WriteLine("Initializing AutoPAR...");
        AssemblyLoadInterceptor.Initialize(RainierFetcher.UpdateDirectory);

        PostParMain(args, secrets);
    }

    private static void PostParMain(string[] args, SecretsConfig secrets)
    {
        Console.WriteLine("Rainer Card Definition Fetcher");

        if(args.Length == 0 || args.Contains(ARG_HELP_SHORT) || args.Contains(ARG_HELP_LONG))
        {
            ShowHelpText();
            return;
        }

        // Parse output folder if needed
        outputFolder = GetOutputFolder(args);

        Console.WriteLine("Logging in...");
        TokenData tokenData = AccessHelper.GetTokenForUsernameAndPassword(secrets.username, secrets.password);
        Console.WriteLine("Logged in Successfully");

        // Access Key is hardcoded in Client.Setup
        const string ACCESS_KEY = "421d8904-0236-4ab4-94f5-a8a84aeb3f7b";

        // DEVICE_ID is derived from, in preference:
        // * UnityEngine.SystemInfo.deviceUniqueIdentifier
        // * PlayerPrefs.GetString("GameVersionInfo:Identifier")
        // * Guid.NewGuid()
        string DEVICE_ID = Guid.NewGuid().ToString(); //"1047b8069bcaa0358004cb88aad57f5cc7dc4759";

        // CLIENT_ID is derived from, in perference:
        // * tokenData.id_token
        // * If PlayerPrefs.GetInt("prefs-random-user") > 0, Guid.NewGuid()
        // * UnityEngine.SystemInfo.deviceUniqueIdentifier
        // * PlayerPrefs.GetString("GameVersionInfo:Identifier")
        // * Guid.NewGuid()
        string CLIENT_ID = tokenData.id_token ?? Guid.NewGuid().ToString(); // "6a9d54403b2ba18414990995b57b2632";

        Client client = new ClientBuilder()
            .WithStage(Stages.PROD)
            .WithAccessKey(ACCESS_KEY)
            .WithDeviceInfo(deviceId: DEVICE_ID, deviceType: null, "Windows")
            .Create(clientId: CLIENT_ID);

        client.RegisterAsync().Wait();
        client.MakeSyncCall<string, string, TokenResponse>(client.AuthAsync, tokenData.access_token, "PJWT");

        bool anyArgWasSpecified = false;
        if (args.Contains(ARG_FETCH_ITEMDB))
        {
            anyArgWasSpecified = true;
            Fetchers.FetchAndSaveItemDatabase(client);
        }

        if (args.Contains(ARG_FETCH_CARDACTIONS))
        {
            anyArgWasSpecified = true;
            Fetchers.FetchAndSaveCardActions(client);
        }

        if (args.Contains(ARG_FETCH_CARDDB))
        {
            anyArgWasSpecified = true;
            Fetchers.FetchAndSaveCardDatabase(client);
        }

        if (args.Contains(ARG_FETCH_CARDDEFINITIONS) || args.Contains(ARG_FETCH_CARDDEFINITIONS_SHORTER))
        {
            anyArgWasSpecified = true;
            Fetchers.FetchAndSaveCardDefinitions(client, leveragePreviousInvalidCardIds: !args.Contains(ARG_CARDDEFS_IGNORE_INVALID_CARDS_FILE));
        }

        if (args.Contains(ARG_FETCH_RULES))
        {
            anyArgWasSpecified = true;
            Fetchers.FetchAndSaveAllGamemodeData(client);
        }

        if (args.Contains(ARG_FETCH_AIDECKS))
        {
            anyArgWasSpecified = true;
            for (int i = 0; i < 5; i++)
            {
                Fetchers.FetchAndSaveAiCustomizationData(client);
            }
        }

        if(args.Contains(ARG_FETCH_ALL_QUESTS))
        {
            anyArgWasSpecified = true;
            Fetchers.FetchQuestData(client);
        }

        if(args.Contains(ARG_FETCH_DECKVALIDATION))
        {
            anyArgWasSpecified = true;
            Fetchers.FetchAndSaveDeckValidationRules(client);
        }

        if(!anyArgWasSpecified)
        {
            Console.WriteLine("No fetch argument was specified.");
            ShowHelpText();
        }
    }

    private static SecretsConfig LoadSecrets()
    {
        const string SECRETS_FILE = "secrets.json";
        const string SECRETS_EXAMPLE_FILE = "secrets.example.json";
        const string SECRETS_SAMPLE_USERNAME = "myname";
        const string SECRETS_SAMPLE_PASSWORD = "mypassword";

        const string SECRETS_EXAMPLE_CONTENTS = "{\"username\": \"myname\",\""+SECRETS_SAMPLE_USERNAME+"\": \""+SECRETS_SAMPLE_PASSWORD+"\"}";
        if (!File.Exists(SECRETS_FILE))
        {
            Console.Error.WriteLine("Secrets file (" + SECRETS_FILE + ") not found. Use " + SECRETS_EXAMPLE_FILE + " as a starting point.");

            if (!File.Exists(SECRETS_EXAMPLE_FILE))
            {
                Console.Error.WriteLine("(The example secrets file did not already exist; one has been created)");
                File.WriteAllText(SECRETS_EXAMPLE_FILE, SECRETS_EXAMPLE_CONTENTS);
            }

            Environment.Exit(1);
        }

        SecretsConfig secrets;
        try
        {
            secrets = JsonConvert.DeserializeObject<SecretsConfig>(File.ReadAllText(SECRETS_FILE));
        }
        catch(Exception e)
        {
            Console.Error.WriteLine($"There was a problem loading the secrets file - {e.Message} ({e.GetType().FullName})");
            Console.Error.WriteLine("The secrets file may be corrupted. Correct it and try again.");
            throw;
        }

        if(string.IsNullOrEmpty(secrets.username) || secrets.username == SECRETS_SAMPLE_USERNAME || string.IsNullOrEmpty(secrets.password) || secrets.password == SECRETS_SAMPLE_PASSWORD)
        {
            Console.Error.WriteLine("The secrets file appears to have either the example username/password, or they're not set.");
            Console.Error.WriteLine("Add the credentials for a valid Pokemon Trainers Club account to the secrets file.");
            Environment.Exit(1);
        }

        return secrets;
    }

    private static string GetOutputFolder(string[] args)
    {
        string? outputFolder;
        if (args.Contains(ARG_OUTPUT_FOLDER))
        {
            outputFolder = args.SkipWhile(arg => arg != ARG_OUTPUT_FOLDER).Skip(1).FirstOrDefault();
            if (outputFolder == null)
            {
                string err = ARG_OUTPUT_FOLDER + " specified, but no output folder provided";
                Console.Error.WriteLine(err);
                Environment.Exit(1);
                throw new ArgumentException(err);
            }
            else if (outputFolder.StartsWith("--"))
            {
                string err = ARG_OUTPUT_FOLDER + " specified, but output folder looks like another argument";
                Console.Error.WriteLine(err);
                Environment.Exit(1);
                throw new ArgumentException(err);
            }

            if (!Directory.Exists(outputFolder))
            {
                try
                {
                    Directory.CreateDirectory(outputFolder);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Couldn't create output directory because:");
                    AnsiConsole.WriteException(e);
                    Environment.Exit(1);
                    throw new ArgumentException("Couldn't create output directory");
                }
            }
        }
        else
        {
            outputFolder = RainierSharedDataHelper.GetSharedDataDirectory();
        }

        return outputFolder;
    }

    private static void ShowHelpText()
    {
        Console.WriteLine
        (
"""
Fetch arguments (as many as desired can be specified):
--fetch-itemdb          Fetches the database of items.
--fetch-cardactions     Fetches the localized list of card actions.
--fetch-carddb          Fetches the list of cards for display in-game (not implementations, see --fetch-carddefinitions)
--fetch-carddefinitions / --fetch-carddefs Fetches all card implementations.
--ignore-invalid-ids-file   By default, --fetch-carddefinitions will create a file of known-bad card IDs that can be skipped
                            for significantly faster performance on subsequent executions (minutes vs hours).
                            These entries may become stale as skipped cards become implemented; --fetch-carddefinitions should
                            probably be run monthly with this flag.

                            If the invalid IDs file doesn't already exist, this flag will have no effect.

--fetch-rules           Fetches the game rules.
--fetch-aidecks         Fetches a selection of decks used by the AI.

Omukade Cheyenne servers typically only need the results of --fetch-carddefinitions --fetch-rules

Output arguments:
--output-folder (/foo/bar)  DEPRECATED: Writes all fetched data to this folder. By default, the Common Omukade Datastore is used for output.
                            Directories will be created under this folder with all retrieved information.

Other arguments:
-h / --help             This help text. Will also appear automatically if run with no fetch arguments, or no arguments at all.
"""
        );
    }
}