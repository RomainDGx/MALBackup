# About 

Little application for backup user's anime list from MyAnimeList.

**Warning**: It doesn't work like the "export" functionality of MyAnimeList website, it doesn't generate XML file.

Method: Use load anime request for MyAnimeList server endpoint, [request is here](https://github.com/RomainDGx/MALBackup/blob/main/MALBackup.App/Program.cs#L34).

# How to use this program?

1. Clone the program on your computer

```
> git clone https://github.com/RomainDGx/MALBackup.git
```

2. Compile program for your OS (_program was developed on .Net Core 5.0, cross plateform framwork for C#_).

3. Run programm with arguments:
    
    - MyAnimeList user name

    - Anime status code

    - Target folder

**Warning**: Program not run if user not exists or if user anime list is private.

| Status    | Code |
|-----------|:----:|
| Watching  | 1    |
| Completed | 2    |
| On Hold   | 3    |
| Dropped   | 5    |
| All       | 7    |

Exemple :

```
> MALBackup.App.exe Username 7 C:\MALBackup
```

## Output format

This program generate json file with the current datetime for file name. Format: yyyy-MM-dd-HH-mm-ss.json

```
2021-03-07-11-30-00.json
```

File content :
```typescript
[
  {
    "status": number,
    "num_watched_episodes": number,
    "anime_title": string,
    "anime_num_episodes": number,
    "anime_id": number,
  },
  { ... }
]
```
