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

`JsonUtility` does the serializing, so the saved type must be a `[Serializable]`
class or struct with **public fields** (or `[SerializeField]` private ones) —
properties are not serialized, `Dictionary` is not supported, polymorphism is
not preserved, and a collection or primitive cannot be the top-level type.

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
