﻿using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;

namespace NzbDrone.Core.Datastore.Converters
{
    public class EmbeddedDocumentConverter<T> : SqlMapper.TypeHandler<T>
    {
        protected readonly JsonSerializerOptions SerializerSettings;

        public EmbeddedDocumentConverter()
        {
            var serializerSettings = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                IgnoreNullValues = false,
                PropertyNameCaseInsensitive = true,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            // serializerSettings.Converters.Add(new HttpUriConverter());
            serializerSettings.Converters.Add(new NoFlagsStringEnumConverter());
            serializerSettings.Converters.Add(new TimeSpanConverter());
            serializerSettings.Converters.Add(new UtcConverter());

            SerializerSettings = serializerSettings;
        }

        public EmbeddedDocumentConverter(params JsonConverter[] converters) : this()
        {
            foreach (var converter in converters)
            {
                SerializerSettings.Converters.Add(converter);
            }
        }

        public override void SetValue(IDbDataParameter parameter, T doc)
        {
            parameter.Value = JsonSerializer.Serialize(doc, SerializerSettings);
        }

        public override T Parse(object value)
        {
            return JsonSerializer.Deserialize<T>((string) value, SerializerSettings);
        }
    }
}
