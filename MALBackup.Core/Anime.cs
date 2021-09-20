using System;
using System.Collections.Generic;
using System.Text.Json;

namespace MALBackup.Core
{
    public class Anime
    {
        public Status Status { get; set; }

        int _score;
        public int Score
        {
            get => _score;
            set
            {
                if( value is < 0 or > 10 )
                {
                    throw new ArgumentException( $"Score must between 0 and 10, value: {value}" );
                }
                _score = value;
            }
        }

        public int IsRewatching { get; set; }

        public int NumWatchedEpisodes { get; set; }

        public int NumEpisodes { get; set; }

        public int AiringStatus { get; set; }

        public int AnimeId { get; }

        public bool HasEpisodeVideo { get; set; }

        public bool HasPromotionVideo { get; set; }

        public bool HasVideo { get; set; }

        public bool IsAddedToList { get; set; }

        public string Tags { get; set; }

        public string Title { get; }

        /// <summary>
        /// URL of anime's trailer on MyAnimeList website.
        /// </summary>
        public string VideoUrl { get; set; }

        /// <summary>
        /// URL of anime's page on MyAnimeList website.
        /// </summary>
        public string AnimeUrl { get; }

        public string AnimeImagePath { get; set; }

        public MediaType MediaType { get; set; }

        /// <summary>
        /// Values : G, PG, PG-13, R, R+, Rx or null.
        /// <see href="https://myanimelist.net/forum/?topicid=16816">Info here.</see>
        /// </summary>
        public string RatingString { get; set; }

        public MALDate? StartDate { get; set; }

        public MALDate? FinishDate { get; set; }

        public int? DaysString { get; set; }

        public MALDate? AnimeStartDate { get; set; }

        public MALDate? AnimeEndDate { get; set; }

        public string Storage { get; set; }

        public Priority Priority { get; set; }

        public List<Studio> Studios { get; }

        public List<License> Licensors { get; }

        public List<Genre> Genres { get; }

        public List<Demographic> Demographics { get; }

        public string Season { get; }

        public Anime( int animeId, string animeTitle, string animeUrl, string season, List<Studio> studios, List<License> licensors, List<Genre> genres, List<Demographic> demographics )
        {
            AnimeId = animeId;
            Title = animeTitle;
            AnimeUrl = animeUrl;
            Season = season;
            Studios = studios;
            Licensors = licensors;
            Genres = genres;
            Demographics = demographics;
        }

        internal Anime( ref Utf8JsonReader reader )
        {
            if( reader.TokenType != JsonTokenType.StartObject )
            {
                throw new JsonException( "Invalid value, reader not start with object." );
            }

            while( reader.Read() /* Open property or end object */ && reader.TokenType != JsonTokenType.EndObject )
            {
                string propertyName = reader.GetString();
                reader.Read(); // Open value
                string date;

                switch( propertyName )
                {
                    case "status":
                        int status = reader.GetInt32();
                        Status = status switch
                        {
                            1 => Status.Watching,
                            2 => Status.Completed,
                            3 => Status.OnHold,
                            5 => Status.Dropped,
                            6 => Status.PlanToWatch,
                            _ => throw new JsonException( $"Not valid status: {status}")
                        };
                        break;

                    case "score":
                        Score = reader.GetInt32();
                        break;

                    case "is_rewatching":
                        IsRewatching = reader.GetInt32();
                        break;

                    case "anime_num_episodes":
                        NumEpisodes = reader.GetInt32();
                        break;

                    case "anime_airing_status":
                        AiringStatus = reader.GetInt32();
                        break;

                    case "num_watched_episodes":
                        NumWatchedEpisodes = reader.GetInt32();
                        break;

                    case "anime_id":
                        AnimeId = reader.GetInt32();
                        break;

                    case "has_episode_video":
                        HasEpisodeVideo = reader.GetBoolean();
                        break;

                    case "has_promotion_video":
                        HasPromotionVideo = reader.GetBoolean();
                        break;

                    case "has_video":
                        HasVideo = reader.GetBoolean();
                        break;

                    case "is_added_to_list":
                        IsAddedToList = reader.GetBoolean();
                        break;

                    case "tags":
                        Tags = reader.GetString();
                        break;

                    case "anime_title":
                        Title = reader.GetString();
                        break;

                    case "video_url":
                        VideoUrl = reader.GetString();
                        break;

                    case "anime_url":
                        AnimeUrl = reader.GetString();
                        break;

                    case "anime_image_path":
                        AnimeImagePath = reader.GetString();
                        break;

                    case "anime_media_type_string":
                        string mediaType = reader.GetString();
                        MediaType = mediaType switch
                        {
                            "Movie" => MediaType.Movie,
                            "Music" => MediaType.Music,
                            "OVA" => MediaType.OVA,
                            "ONA" => MediaType.ONA,
                            "Special" => MediaType.Special,
                            "TV" => MediaType.TV,
                            "Unknown" => MediaType.Unknown,
                            _ => throw new JsonException( $"Not valid anime_media_type_string: {mediaType}" )
                        };
                        break;

                    case "anime_mpaa_rating_string":
                        RatingString = reader.GetString();
                        break;

                    case "start_date_string":
                        date = reader.GetString();
                        StartDate = date is null ? null : new MALDate( date );
                        break;

                    case "finish_date_string":
                        date = reader.GetString();
                        FinishDate = date is null ? null : new MALDate( date );
                        break;

                    case "days_string":
                        DaysString = reader.TokenType == JsonTokenType.Null ? null : reader.GetInt32();
                        break;

                    case "anime_start_date_string":
                        date = reader.GetString();
                        AnimeStartDate = date is null ? null : new MALDate( date );
                        break;

                    case "anime_end_date_string":
                        date = reader.GetString();
                        AnimeEndDate = date is null ? null : new MALDate( date );
                        break;

                    case "storage_string":
                        Storage = reader.GetString();
                        break;

                    case "priority_string":
                        string priority = reader.GetString();
                        Priority = priority switch
                        {
                            "Low" => Priority.Low,
                            "Medium" => Priority.Medium,
                            "Hight" => Priority.Hight,
                            _ => throw new JsonException( $"Not valid priority_string value: {priority}" )
                        };
                        break;

                    case "anime_studios":
                        if( reader.TokenType == JsonTokenType.Null )
                        {
                            Studios = null;
                            break;
                        }

                        Studios = new List<Studio>();

                        while( reader.Read() /* Open start object or end array */ && reader.TokenType != JsonTokenType.EndArray )
                        {
                            Studios.Add( new Studio( ref reader ) );
                        }
                        break;

                    case "anime_licensors":
                        if( reader.TokenType == JsonTokenType.Null )
                        {
                            Licensors = null;
                            break;
                        }

                        Licensors = new List<License>();

                        while( reader.Read() /* Open start object or end array */ && reader.TokenType != JsonTokenType.EndArray )
                        {
                            Licensors.Add( new License( ref reader ) );
                        }
                        break;

                    case "genres":
                        if( reader.TokenType == JsonTokenType.Null )
                        {
                            Genres = null;
                            break;
                        }

                        Genres = new List<Genre>();

                        while( reader.Read() && reader.TokenType != JsonTokenType.EndArray )
                        {
                            Genres.Add( new Genre( ref reader ) );
                        }
                        break;

                    case "demographics":
                        if( reader.TokenType == JsonTokenType.Null )
                        {
                            Demographics = null;
                            break;
                        }

                        Demographics = new List<Demographic>();

                        while( reader.Read() && reader.TokenType != JsonTokenType.EndArray )
                        {
                            Demographics.Add( new Demographic( ref reader ) );
                        }
                        break;

                    case "anime_season":
                        Season = reader.GetString();
                        break;

                    default:
                        throw new JsonException( $"Not valid anime property name: {propertyName}" );
                }
            }
        }

        internal void Save( Utf8JsonWriter writer )
        {
            writer.WriteStartObject();
            writer.WriteNumber( "status", (int)Status );
            writer.WriteNumber( "score", _score );
            writer.WriteString( "tags", Tags );
            writer.WriteNumber( "is_rewatching", IsRewatching );
            writer.WriteNumber( "num_watched_episodes", NumWatchedEpisodes );
            writer.WriteString( "anime_title", Title );
            writer.WriteNumber( "anime_num_episodes", NumEpisodes );
            writer.WriteNumber( "anime_airing_status", AiringStatus );
            writer.WriteNumber( "anime_id", AnimeId );
            if( Studios is null ) writer.WriteNull( "anime_studios" );
            else
            {
                writer.WriteStartArray( "anime_studios" );
                Studios.ForEach( studio => studio.Save( writer ) );
                writer.WriteEndArray();
            }
            if( Licensors is null ) writer.WriteNull( "anime_licensors" );
            else
            {
                writer.WriteStartArray( "anime_licensors" );
                Licensors.ForEach( licensor => licensor.Save( writer ) );
                writer.WriteEndArray();
            }
            if( Genres is null ) writer.WriteNull( "genres" );
            else
            {
                writer.WriteStartArray( "genres" );
                Genres.ForEach( genre => genre.Save( writer ) );
                writer.WriteEndArray();
            }
            if( Demographics is null ) writer.WriteNull( "demographics" );
            else
            {
                writer.WriteStartArray( "demographics" );
                Demographics.ForEach( demographic => demographic.Save( writer ) );
                writer.WriteEndArray();
            }
            writer.WriteString( "anime_season", Season );
            writer.WriteBoolean( "has_episode_video", HasEpisodeVideo );
            writer.WriteBoolean( "has_promotion_video", HasPromotionVideo );
            writer.WriteBoolean( "has_video", HasVideo );
            writer.WriteString( "video_url", VideoUrl );
            writer.WriteString( "anime_url", AnimeUrl );
            writer.WriteString( "anime_image_path", AnimeImagePath );
            writer.WriteBoolean( "is_added_to_list", IsAddedToList );
            writer.WriteString( "anime_media_type_string", MediaType.ToString( "g" ) );
            writer.WriteString( "anime_mpaa_rating_string", RatingString );
            writer.WriteString( "start_date_string", StartDate?.ToString() );
            writer.WriteString( "finish_date_string", FinishDate?.ToString() );
            writer.WriteString( "anime_start_date_string", AnimeStartDate?.ToString() );
            writer.WriteString( "anime_end_date_string", AnimeEndDate?.ToString() );
            if( DaysString is null ) writer.WriteNull( "days_string" );
            else writer.WriteNumber( "days_string", (int)DaysString );
            writer.WriteString( "storage_string", Storage );
            writer.WriteString( "priority_string", Priority.ToString( "g" ) );
            writer.WriteEndObject();
        }
    }
}
