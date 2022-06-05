# About 

Application for backup user's anime list from [MyAnimeList](https://myanimelist.net/).

**Warning**: It doesn't work like the "export" functionality of MyAnimeList website. It doesn't generate XML file.

I use [MyAnimeList API (beta ver.) (2)](https://myanimelist.net/apiconfig/references/api/v2) to get the user's anime list.

**Warning**: This application not work if the user anime list is in `Private` or `Friends Only` privacy. Change your anime list privacy [here](https://myanimelist.net/editprofile.php?go=listpreferences).

_Note: This application not use user credentials._

# How to use this application?

1. Clone the repository on your computer

```
> git clone https://github.com/RomainDGx/MALBackup.git
```

2. Compile the `MALBackup.App` project for your plateforme (_The project is developed on .Net Core 6.0, cross plateform framwork for C#_). Your can find dotnet build documentation [here](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build).

3. Run the application with the following arguments:
    
    - A MyAnimeList client id ;

    - A MyAnimeList valid user name ;

    - A Target folder.

Exemple (for Linux):

```
> MALBackup.App.dll CLIENT_ID USER_NAME /Backups/MyAnimeList
```

**Warning**: The application require a client id.

For create your client id, go to [the API section of your MyAnileList profile](https://myanimelist.net/apiconfig), or check [this forum](https://myanimelist.net/forum/?topicid=1850649).


## Output format

This program generate JSON file with the current datetime for file name. Format: yyyy-MM-dd-HH-mm-ss.json

Exemple :
```
2022-06-05-23-30-00.json
```

File content :
```typescript
// List of "All Anime" in the user anime list
[
  // An anime object
  {
    // Anime id
    "id": number,
    // Anime title
    "title": string,
    // Number of episodes of the anime
    "num_episodes": number,
    // Specific user watching informations
    "list_status": {
      // User watching status
      "status": string,
      // Number of episodes watched by the user
      "num_episodes_watched": number,
      // Number of times of the user has seen the anime
      "num_times_rewatched": number,
      // Number of episodes the user has seen during the rewatching
      "rewatched_value": number
    }
  },
  ...
]
```

For more informations, see the [MyAnimeList API (beta ver.) (2) documentation](https://myanimelist.net/apiconfig/references/api/v2#operation/users_user_id_animelist_get).
