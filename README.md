# Local Data Manager

`com.cosgunhalil.localdatamanager`

Generic local data saver and loader for Unity. Serialize your classes with
`JsonUtility`, write them to local storage, and read them back with a single
generic call.

## Installation

Unity Package Manager -> **+** -> **Install package from git URL...**:

```
https://github.com/cosgunhalil/LB_LocalDataManager.git
```

Or add it to `Packages/manifest.json` directly:

```json
{
  "dependencies": {
    "com.cosgunhalil.localdatamanager": "https://github.com/cosgunhalil/LB_LocalDataManager.git"
  }
}
```

Pin a release by appending a tag: `...LB_LocalDataManager.git#1.0.0`.

Requires Unity 2021.3 or newer.

## Usage

```csharp
using LB.LocalDataManager;

[Serializable]
public class PlayerData
{
    public int Id;
    public string Name;
}

LocalData.Save(new PlayerData { Id = 1, Name = "Halil" }, "PlayerData");

var playerData = LocalData.Load<PlayerData>("PlayerData");
```

`Load` hands back `default(T)` when there is no save. When "no save yet" and
"the save is damaged" need different handling, ask:

```csharp
PlayerData playerData;
if (LocalData.TryLoad("PlayerData", out playerData))
{
    Continue(playerData);
}
else if (LocalData.Exists("PlayerData"))
{
    ShowCorruptSaveDialog();     // the file is there but did not parse
}
else
{
    StartNewGame();
}
```

Or take a fallback and move on: `LocalData.Load("PlayerData", PlayerData.New())`.

`LocalDataSaver` and `LocalDataLoader` stay public if you would rather hold and
inject instances than call the static entry point.

Import the **Basic Usage** sample from the package's page in the Package Manager
window for a runnable version.

## API

| Member | Behaviour |
| --- | --- |
| `bool LocalData.Save<T>(T dataObject, string fileName)` | Serializes and writes, replacing any existing file. Returns `false` and logs an error when the write fails. |
| `T LocalData.Load<T>(string fileName)` | Reads and deserializes, or `default(T)` when there is nothing to load. |
| `T LocalData.Load<T>(string fileName, T fallback)` | Same, but returns `fallback` instead of `default(T)`. |
| `bool LocalData.TryLoad<T>(string fileName, out T value)` | `false` when the file is missing, unreadable, empty or does not parse. |
| `bool LocalData.Exists(string fileName)` | Whether the file has been saved. Says nothing about whether it still parses. |
| `bool LocalData.Delete(string fileName)` | `true` when a file was there and is now gone; `false` when there was nothing to delete. |
| `string LocalData.GetPath(string fileName)` | Full path a given file name resolves to. |
| `string LocalDataPath.RootDirectory` | Directory the files are written to. |

Every member above throws `ArgumentException` for an invalid file name. The same
operations exist on the injectable `LocalDataSaver.SaveData<T>` and
`LocalDataLoader.LoadData<T>` / `TryLoadData<T>`.

`Tools > Local Data Manager` in the editor menu opens or logs that directory.

## Where the files go

Everything is written under `Application.persistentDataPath`, in the editor and
in a player build alike — so what you see while testing is what ships, and
nothing lands in your project's `Assets/` folder.

Pass a **bare file name**: the directory and the `.txt` extension are added for
you, so `"PlayerData"` — not `"PlayerData.txt"`. Forward slashes make
subfolders, and missing folders are created for you:

```csharp
saver.SaveData(slot, "slots/autosave");   // <persistentDataPath>/slots/autosave.txt
```

A file name is rejected with an `ArgumentException` when it is empty, absolute
(`/save`, `C:/save`), navigates out of the data folder (`../save`), or contains
something that is not portable across platforms (`< > : " | ? *`, control
characters, a trailing dot or space). Unity's own platforms disagree about these,
so they fail the same way everywhere instead of only on Windows.

## Crash safety

`SaveData` writes to a staging file and only then moves it over the target, so a
crash, a force-quit or an OS kill mid-save leaves the previous file untouched
rather than truncating it. It does not protect against a power cut at the exact
moment the filesystem commits the move.

## Serialization rules

By default `JsonUtility` does the serializing, so the saved type must be a
`[Serializable]` class or struct with **public fields** (or `[SerializeField]`
private ones) — properties are not serialized, `Dictionary` is not supported,
polymorphism is not preserved, and a collection or primitive cannot be the
top-level type.

## Swapping the serializer

Those rules come from the default serializer, not from the package. Implement
`IDataSerializer` to replace it — with Newtonsoft.Json, say, and dictionaries,
properties and polymorphism all start working:

```csharp
public class NewtonsoftSerializer : IDataSerializer
{
    public string Serialize<T>(T value) => JsonConvert.SerializeObject(value);
    public T Deserialize<T>(string data) => JsonConvert.DeserializeObject<T>(data);
}

// Once, during startup, before anything saves or loads:
LocalData.Serializer = new NewtonsoftSerializer();
```

Or pass one to an instance: `new LocalDataSaver(mySerializer)`. Setting
`LocalData.Serializer = null` restores the default.

Files carry no record of how they were written, so a serializer swap does not
migrate what is already on disk: existing saves stop loading unless the new
serializer happens to read the old format. Want indented, readable saves while
developing? `LocalData.Serializer = new JsonUtilitySerializer(prettyPrint: true)`.

## Encrypting saves

`IDataProcessor` is a text-to-text stage applied after serializing and before
deserializing — encryption, compression, whatever you need. The package ships
`AesDataProcessor` (AES-CBC, HMAC-SHA256, PBKDF2 key derivation, random salt and
IV per save):

```csharp
// Once, during startup:
LocalData.Processor = new AesDataProcessor("a passphrase from your game");
```

A wrong password or an edited file fails the integrity check, so `TryLoad`
returns `false` instead of handing back data that decrypted into nonsense.

**This is protection against casual save editing, not security.** Your password
ships inside the build and can be recovered from it. It stops a player editing
coins in a text editor; it does not stop a determined one, so never put anything
in a save file that you would not hand the player outright.

Switching the processor on or off does not convert existing files: a save
written in plain text will not load with a processor set, and vice versa.

## Schema versions and migrations

When a saved type changes shape, old files on players' devices do not. Implement
`IVersionedData` and the version is stamped into the file:

```csharp
[Serializable]
public class PlayerData : IVersionedData
{
    public int SchemaVersion { get { return 2; } }

    public int Id;
    public string DisplayName;   // called "Name" in version 1
}
```

Then register a migration, which rewrites the serialized text — the old shape
is exactly what no longer deserializes into the new type:

```csharp
public class RenameNameToDisplayName : IDataMigration
{
    public int FromVersion { get { return 1; } }
    public int ToVersion { get { return 2; } }

    public string Migrate(string data) => data.Replace("\"Name\":", "\"DisplayName\":");
}

// Once, during startup:
LocalData.Migrations.Register<PlayerData>(new RenameNameToDisplayName());
```

Migrations chain: a version 1 file with `1->2` and `2->3` registered runs through
both before loading. Files written before you adopted `IVersionedData` count as
version 0, so a `0->1` migration picks them up too. A file whose version has no
migration registered is loaded as it is — fields that did not change still
arrive, the rest stay at their defaults.

Types that do not implement `IVersionedData` are written exactly as before, with
no header.

## Running the tests

Tests ship with the package. To run them in a project that consumes it, add the
package to `testables` in `Packages/manifest.json`:

```json
{
  "testables": [
    "com.cosgunhalil.localdatamanager"
  ]
}
```

They then appear in **Window > General > Test Runner** under both EditMode and
PlayMode.

## License

MIT — see [LICENSE.md](LICENSE.md).
