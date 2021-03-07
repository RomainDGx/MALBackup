using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

namespace MALBackup.Core
{
    public class AnimeList : List<Anime>
    {
        public AnimeList() : base()
        {
        }

        public AnimeList( Utf8JsonReader reader ) : this()
        {
            Concat( reader );
        }

        public void Concat( Utf8JsonReader reader )
        {
            reader.Read(); // Open start array

            if( reader.TokenType != JsonTokenType.StartArray )
            {
                throw new JsonException( "Invalid value, reader not start with array." );
            }

            while( reader.Read() /* Open object or end array */ && reader.TokenType != JsonTokenType.EndArray )
            {
                Add( new Anime( ref reader ) );
            }
        }

        public void Save( Utf8JsonWriter writer )
        {
            writer.WriteStartArray();
            ForEach( anime => anime.Save( writer ) );
            writer.WriteEndArray();
        }
    }
}
