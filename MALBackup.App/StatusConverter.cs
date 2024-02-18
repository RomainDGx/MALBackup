using MALBackup.Model;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MALBackup.App
{
    internal class StatusConverter : JsonConverter<Status>
    {
        public override Status Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
        {
            if( reader.TokenType != JsonTokenType.String )
            {
                throw new InvalidDataException( "Value should be a string." );
            }
            string? status = reader.GetString();
            return status switch
            {
                "watching" => Status.Watching,
                "completed" => Status.Completed,
                "on_hold" => Status.OnHold,
                "dropped" => Status.Dropped,
                "plan_to_watch" => Status.PlanToWatch,
                _ => throw new InvalidDataException( $"Invalid status type '{status}'." )
            };
        }

        public override void Write( Utf8JsonWriter writer, Status value, JsonSerializerOptions options )
        {
            writer.WriteStringValue( value switch
            {
                Status.Watching => "watching",
                Status.Completed => "completed",
                Status.OnHold => "on_hold",
                Status.Dropped => "dropped",
                Status.PlanToWatch => "plan_to_watch",
                _ => throw new InvalidDataException( $"Unknown status type '{value}'." ),
            } );
        }
    }
}
