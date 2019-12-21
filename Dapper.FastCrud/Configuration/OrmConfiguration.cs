﻿// ReSharper disable once CheckNamespace (the namespace is intentionally not in sync with the file location) 
namespace Dapper.FastCrud
{
    using System;
    using System.Collections.Concurrent;
    using System.Runtime.CompilerServices;
    using Dapper.FastCrud.Configuration;
    using Dapper.FastCrud.EntityDescriptors;
    using Dapper.FastCrud.Mappings;
    using Dapper.FastCrud.SqlStatements;
    using Dapper.FastCrud.Validations;

    /// <summary>
    /// Sets up various FastCrud settings.
    /// </summary>
    public static class OrmConfiguration
    {
        private static volatile SqlDialect _currentDefaultDialect = SqlDialect.MsSql;
        private static volatile OrmConventions _currentOrmConventions = new OrmConventions();
        private static readonly ConcurrentDictionary<Type, EntityDescriptor> _entityDescriptorCache = new ConcurrentDictionary<Type, EntityDescriptor>();
        private static volatile SqlStatementOptions _defaultStatementOptions = new SqlStatementOptions();

        /// <summary>
        /// Clears all the recorded entity registrations and entity ORM mappings.
        /// </summary>
        public static void ClearEntityRegistrations()
        {
            _entityDescriptorCache.Clear();
        }

        /// <summary>
        /// Returns the default entity mapping for an entity.
        /// This was either previously set by you in a call to <see cref="SetDefaultEntityMapping{TEntity}"/> or it was auto-generated by the library.
        /// 
        /// You can use the returned mappings to create new temporary mappings for the query calls or to override the defaults.
        /// Once the mappings have been used in query calls, the instance will be frozen and it won't support further modifications, but you can always call <see cref="EntityMapping{TEntity}.Clone"/> to create a new instance.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        public static EntityMapping GetDefaultEntityMapping<TEntity>()
        {
            return GetEntityDescriptor<TEntity>().DefaultEntityMapping;
        }

        /// <summary>
        /// Registers a new entity. Please continue setting up property mappings and other entity options with the returned default entity mapping instance.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        public static EntityMapping<TEntity> RegisterEntity<TEntity>()
        {
            return SetDefaultEntityMapping<TEntity>(new EntityMapping<TEntity>()) as EntityMapping<TEntity>;
        }

        /// <summary>
        /// Registers a new entity. Please continue setting up property mappings and other entity options with the returned default entity mapping instance.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        public static TableValuedFunctionEntityMapping<TEntity> RegisterTableValuedFunctionEntity<TEntity>()
        {
            return SetDefaultEntityMapping<TEntity>(new TableValuedFunctionEntityMapping<TEntity>()) as TableValuedFunctionEntityMapping<TEntity>;
        }

        /// <summary>
        /// Sets the default entity type mapping for the entity type.
        /// This must be called before any query operations were made on the entity.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        public static EntityMapping SetDefaultEntityMapping<TEntity>(EntityMapping mappings)
        {
            Requires.NotNull(mappings, nameof(mappings));
            Requires.Argument(!mappings.IsFrozen,nameof(mappings),  "The entity mappings were frozen and can't be used as defaults. They must be cloned first.");
            GetEntityDescriptor<TEntity>().DefaultEntityMapping = mappings;
            return mappings;
        }


        /// <summary>
        /// Returns an SQL builder helpful for constructing verbatim SQL queries.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entityMapping">If NULL, de default entity mapping will be used.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ISqlBuilder GetSqlBuilder<TEntity>(EntityMapping entityMapping = null)
        {
            return GetEntityDescriptor<TEntity>().GetSqlStatements(entityMapping).SqlBuilder;
        }

        /// <summary>
        /// Gets or sets the default command options. 
        /// </summary>
        public static SqlStatementOptions DefaultSqlStatementOptions
        {
            get
            {
                return _defaultStatementOptions;
            }
            //set
            //{
            //    Requires.NotNull(value, nameof(DefaultSqlStatementOptions));
            //    _defaultStatementOptions = value;
            //}
        }

        /// <summary>
        /// Gets or sets the default dialect. 
        /// </summary>
        public static SqlDialect DefaultDialect
        {
            get
            {
                return _currentDefaultDialect;
            }
            set
            {
                _currentDefaultDialect = value;
            }
        }

        /// <summary>
        /// Gets or sets the conventions used by the library. Subclass <see cref="OrmConventions"/> to provide your own set of conventions.
        /// </summary>
        public static OrmConventions Conventions {
            get
            {
                return _currentOrmConventions;
            }
            set
            {
                Requires.NotNull(value, nameof(Conventions));
                _currentOrmConventions = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static EntityDescriptor<TEntity> GetEntityDescriptor<TEntity>()
        {
            var entityType = typeof(TEntity);

            return (EntityDescriptor<TEntity>)_entityDescriptorCache.GetOrAdd(entityType, cacheKey => new EntityDescriptor<TEntity>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ISqlStatements<TEntity> GetSqlStatements<TEntity>(EntityMapping entityMapping = null)
        {
            return (ISqlStatements<TEntity>)GetEntityDescriptor<TEntity>().GetSqlStatements(entityMapping);
        }
    }
}
