using System.Text.Json;

namespace MALBackup.Core
{
    public struct Studio
    {
        public readonly int Id { get; }

        public readonly string Name { get; }

        internal Studio( ref Utf8JsonReader reader )
        {
            Id = default;
            Name = default;

            if( reader.TokenType != JsonTokenType.StartObject )
            {
                throw new JsonException( "Invalid value, reader not start with object." );
            }

            while( reader.Read() /* Open property or end object */ && reader.TokenType != JsonTokenType.EndObject )
            {
                string propertyName = reader.GetString();
                reader.Read(); // Open value

                switch( propertyName )
                {
                    case "id":
                        Id = reader.GetInt32();
                        break;

                    case "name":
                        Name = reader.GetString();
                        break;

                    default:
                        throw new JsonException( $"Invalid property name: {propertyName}" );
                }
            }
        }

        internal void Save( Utf8JsonWriter writer )
        {
            writer.WriteStartObject();
            writer.WriteNumber( "id", Id );
            writer.WriteString( "name", Name );
            writer.WriteEndObject();
        }
    }
}
