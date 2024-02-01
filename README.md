# Rainier Card Definition Fetcher
![logo](logo.png)

This is a small helper program used for both Omukade development and fetching card and rule data used to run the Omukade family of TCGL servers.
**You need this to run an Omukade server.**

## Requirements
* [.NET 6 Runtime or SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) for your platform
* A set of current TCGL assemblies and updated by PAR
* Supports Windows x64 and Linux x64 + ARM64
* For developing, Visual Studio 2022 (any edition) with C#, and [Procedual Assembly Rewriter](https://github.com/Hastwell/Omukade.ProcedualAssemblyRewriter)

## Usage

Before running this command, populate the file `secrets.json` in the app's directory with your Pokemon Trainer's Club account. This account is used to fetch
data from the TCGL servers. eg:
```json
{"username": "mysigninname", "password": "abc123"}
```

**The account used must have previously logged into TCGL** as this app cannot deal with any of the first-signin "authorize TCGL to access your account" stuff.

This application uses AutoPAR to load the TCGL binaries. Although they are usually automatically fetched without issue, if issues do arise, one of the following can be used to supply the needed TCGL binaries:
* Windows Only, Recommended: Install Pokemon TCG Live. It will be auto-detected by AutoPAR and used for this application.
* Add the setting `autopar-search-folder` with the location of your TCGL install directory to secrets.json. Backslashes and quotes must be escaped (`\\` and `\"` respectively).
* Copy the TCGL assemblies from your TCGL install directory (`C:\Install\Folder\Pokémon Trading Card Game Live\Pokemon TCG Live_Data\Managed`) to the folder `autopar` under the app's folder.
  Alternatively, the config setting `autopar-search-folder` can be used to set any other name for this directory if prefered. *You must manually update this folder whenever the game updates!*

### Arguments
```
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
--output-folder (/foo/bar)  Writes all fetched data to this folder. By default, the current working directory is used for output.
                            Directories will be created under this folder with all retrieved information.

Other arguments:
-h / --help             This help text. Will also appear automatically if run with no fetch arguments, or no arguments at all.
```

## Compiling

### Rainier Dependencies with AutoPAR
Before building this project, you'll need to run ManualPAR (part of the [Procedual Assembly Rewriter](https://github.com/Hastwell/Omukade.ProcedualAssemblyRewriter)) against the TCGL assemblies to produce a version
with public members that can be accessed by this tool.

These assemblies need to be located in:
```
[your sources folder]
|- Rainier-Assemblies
|  |- 1.3.11.156349.20221208_0543_PAR (or whatever version is latest)
|
|- Rainier.CardDefinitionFetcher
|  |- Rainier.CardDefinitionFetcher.sln
```

### Building
* Use Visual Studio 2022 or later, build the project.
* With the .NET 6 SDK, `dotnet build Rainier.CardDefinitionFetcher.sln`

## License
This software is licensed under the terms of the [GNU AGPL v3.0](https://www.gnu.org/licenses/agpl-3.0.en.html)