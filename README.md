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

// Save
var saver = new LocalDataSaver();
saver.SaveData(new PlayerData { Id = 1, Name = "Halil" }, "PlayerData");

// Load
var loader = new LocalDataLoader();
var playerData = loader.LoadData<PlayerData>("PlayerData");
```

Import the **Basic Usage** sample from the package's page in the Package Manager
window for a runnable version.

## API

| Member | Behaviour |
| --- | --- |
| `bool LocalDataSaver.SaveData<T>(T dataObject, string fileName)` | Serializes and writes, overwriting any existing file. Returns `false` and logs an error when the write fails. |
| `T LocalDataLoader.LoadData<T>(string fileName)` | Reads and deserializes. Returns `default(T)` when the file is missing, unreadable or empty. |
| `string LocalDataLoader.ReadDataFromPath(string path)` | Raw file contents, or `null` when the file cannot be read. |
| `string LocalDataPath.GetPathFor(string fileName)` | Full path a given file name resolves to. |
| `string LocalDataPath.RootDirectory` | Directory the files are written to. |

`Tools > Local Data Manager` in the editor menu opens or logs that directory.

## Where the files go

Pass a **bare file name**: the directory and the `.txt` extension are added for
you, so `"PlayerData"` — not `"PlayerData.txt"`.

| Context | Directory |
| --- | --- |
| Editor | `Application.dataPath` (your project's `Assets/` folder) |
| Player build | `Application.persistentDataPath` |

Editor and player therefore do not share saved data, and files saved while in the
editor show up as assets in your project.

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
