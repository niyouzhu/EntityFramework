// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ValueGeneratorConvention :
        IPrimaryKeyConvention,
        IKeyConvention,
        IKeyRemovedConvention,
        IForeignKeyConvention,
        IForeignKeyRemovedConvention,
        IPropertyAnnotationSetConvention,
        IBaseTypeConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            foreach (var property in relationshipBuilder.Metadata.Properties)
            {
                property.Builder.ValueGenerated(ValueGenerated.Never, ConfigurationSource.Convention);
                property.Builder.RequiresValueGenerator(GetRequiresValueGenerator(property), ConfigurationSource.Convention);
            }

            return relationshipBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Apply(InternalEntityTypeBuilder entityTypeBuilder, ForeignKey foreignKey)
        {
            foreach (var property in foreignKey.Properties)
            {
                property.Builder?.ValueGenerated(GetValueGenerated(property), ConfigurationSource.Convention);
                property.Builder?.RequiresValueGenerator(GetRequiresValueGenerator(property), ConfigurationSource.Convention);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalKeyBuilder Apply(InternalKeyBuilder keyBuilder)
        {
            foreach (var property in keyBuilder.Metadata.Properties)
            {
                property.Builder.RequiresValueGenerator(GetRequiresValueGenerator(property), ConfigurationSource.Convention);
            }

            return keyBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Apply(InternalEntityTypeBuilder entityTypeBuilder, Key key)
        {
            foreach (var property in key.Properties)
            {
                property.Builder?.RequiresValueGenerator(GetRequiresValueGenerator(property), ConfigurationSource.Convention);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalKeyBuilder keyBuilder, Key previousPrimaryKey)
        {
            if (previousPrimaryKey != null
                && previousPrimaryKey.Properties.Count == 1)
            {
                var property = previousPrimaryKey.Properties.First();
                property.Builder?.ValueGenerated(GetValueGenerated(property), ConfigurationSource.Convention);
            }

            foreach (var property in keyBuilder.Metadata.Properties)
            {
                property.Builder.ValueGenerated(GetValueGenerated(property), ConfigurationSource.Convention);
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Annotation Apply(InternalPropertyBuilder propertyBuilder, string name, Annotation annotation, Annotation oldAnnotation)
        {
            if (name == CoreAnnotationNames.ValueGeneratorFactoryAnnotation)
            {
                propertyBuilder.RequiresValueGenerator(GetRequiresValueGenerator(propertyBuilder.Metadata), ConfigurationSource.Convention);
            }

            return annotation;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
        {
            foreach (var property in entityTypeBuilder.Metadata.GetProperties())
            {
                property.Builder.ValueGenerated(GetValueGenerated(property), ConfigurationSource.Convention);
                property.Builder.RequiresValueGenerator(GetRequiresValueGenerator(property), ConfigurationSource.Convention);
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual ValueGenerated? GetValueGenerated([NotNull] Property property)
            => property.IsForeignKey()
                ? ValueGenerated.Never
                : (property.PrimaryKey?.Properties.Count == 1 && ShouldKeyPropertyBeStoreGenerated(property)
                    ? ValueGenerated.OnAdd
                    : (ValueGenerated?)null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual bool? GetRequiresValueGenerator([NotNull] Property property)
            => property.GetValueGeneratorFactory() != null
                ? true
                : property.IsForeignKey()
                    ? false
                    : property.IsKey()
                        ? true
                        : (bool?)null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property FindValueGeneratedOnAddProperty(
            [NotNull] IReadOnlyList<Property> properties, [NotNull] EntityType entityType)
        {
            Check.NotNull(properties, nameof(properties));
            Check.NotNull(entityType, nameof(entityType));

            if (entityType.FindPrimaryKey(properties) != null
                && properties.Count == 1)
            {
                var property = properties.First();
                if (!property.IsForeignKey())
                {
                    if (ShouldKeyPropertyBeStoreGenerated(property))
                    {
                        return property;
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///     Indicates whether the specified property should have the value generated by the store when not set.
        /// </summary>
        /// <param name="property"> The key property that might be store generated. </param>
        /// <returns> A value indicating whether the specified property should have the value generated by the sto </returns>
        protected virtual bool ShouldKeyPropertyBeStoreGenerated([NotNull] Property property) => false;
    }
}
