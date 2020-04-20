using System;
using Convey.Persistence.MongoDB.Builders;
using Convey.Persistence.MongoDB.Factories;
using Convey.Persistence.MongoDB.Initializers;
using Convey.Persistence.MongoDB.Repositories;
using Convey.Persistence.MongoDB.Seeders;
using Convey.Types;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Convey.Persistence.MongoDB
{
    public static class Extensions
    {
        // Helpful when dealing with integration testing
        private static bool _conventionsRegistered;
        private const string SectionName = "mongo";
        private const string RegistryName = "persistence.mongoDb";

        public static IConveyBuilder AddMongo(this IConveyBuilder builder, string sectionName = SectionName,
            Type seederType = null, bool registerConventions = true)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            var mongoOptions = builder.GetOptions<MongoDbOptions>(sectionName);
            return builder.AddMongo(mongoOptions, seederType, registerConventions);
        }

        public static IConveyBuilder AddMongo(this IConveyBuilder builder, Func<IMongoDbOptionsBuilder,
            IMongoDbOptionsBuilder> buildOptions, Type seederType = null, bool registerConventions = true)
        {
            var mongoOptions = buildOptions(new MongoDbOptionsBuilder()).Build();
            return builder.AddMongo(mongoOptions, seederType, registerConventions);
        }

        public static IConveyBuilder AddMongo(this IConveyBuilder builder, MongoDbOptions mongoOptions,
            Type seederType = null, bool registerConventions = true)
        {
            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            if (mongoOptions.SetRandomDatabaseSuffix)
            {
                var suffix = $"{Guid.NewGuid():N}";
                Console.WriteLine($"Setting a random MongoDB database suffix: '{suffix}'.");
                mongoOptions.Database = $"{mongoOptions.Database}_{suffix}";
            }

            builder.Services.AddSingleton(mongoOptions);
            builder.Services.AddSingleton<IMongoClient>(sp =>
            {
                var options = sp.GetService<MongoDbOptions>();
                return new MongoClient(options.ConnectionString);
            });
            builder.Services.AddTransient(sp =>
            {
                var options = sp.GetService<MongoDbOptions>();
                var client = sp.GetService<IMongoClient>();
                return client.GetDatabase(options.Database);
            });
            builder.Services.AddTransient<IMongoDbInitializer, MongoDbInitializer>();
            builder.Services.AddTransient<IMongoSessionFactory, MongoSessionFactory>();

            if (seederType is null)
            {
                builder.Services.AddTransient<IMongoDbSeeder, MongoDbSeeder>();
            }
            else
            {
                builder.Services.AddTransient(typeof(IMongoDbSeeder), seederType);
            }

            builder.AddInitializer<IMongoDbInitializer>();
            if (registerConventions && !_conventionsRegistered)
            {
                RegisterConventions();
            }

            return builder;
        }

        private static void RegisterConventions()
        {
            _conventionsRegistered = true;
            BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
            BsonSerializer.RegisterSerializer(typeof(decimal?),
                new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));
            ConventionRegistry.Register("convey", new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
            }, _ => true);
        }

        public static IConveyBuilder AddMongoRepository<TEntity, TIdentifiable>(this IConveyBuilder builder,
            string collectionName)
            where TEntity : IIdentifiable<TIdentifiable>
        {
            builder.Services.AddTransient<IMongoRepository<TEntity, TIdentifiable>>(sp =>
            {
                var database = sp.GetService<IMongoDatabase>();
                return new MongoRepository<TEntity, TIdentifiable>(database, collectionName);
            });

            return builder;
        }
    }
}