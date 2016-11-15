// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Tests.Metadata.Conventions.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Metadata.Conventions.Internal
{
    public class RelationalValueGeneratorConventionTest : ValueGeneratorConventionTest
    {
        [Fact]
        public void Identity_is_set_for_primary_key()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Id" }, ConfigurationSource.Convention);

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.True(property.RequiresValueGenerator);
        }

        [Fact]
        public void Identity_is_not_set_for_non_primary_key()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.HasKey(new List<string> { "Number" }, ConfigurationSource.Convention);

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
            Assert.True(property.RequiresValueGenerator);
        }

        [Fact]
        public void Identity_not_set_when_composite_primary_key()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Id", "Number" }, ConfigurationSource.Convention);

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.Equal(ValueGenerated.Never, keyProperties[0].ValueGenerated);
            Assert.Equal(ValueGenerated.Never, keyProperties[1].ValueGenerated);
            Assert.True(keyProperties[0].RequiresValueGenerator);
            Assert.True(keyProperties[1].RequiresValueGenerator);
        }

        [Fact]
        public void Identity_set_when_primary_key_property_is_string()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Name" }, ConfigurationSource.Convention);

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
            Assert.True(property.RequiresValueGenerator);
        }

        [Fact]
        public void Identity_not_set_when_primary_key_property_is_byte_array()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityBuilder.Property("binaryKey", typeof(byte[]), ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.PrimaryKey(new[] { "binaryKey" }, ConfigurationSource.Convention);

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
            Assert.True(property.RequiresValueGenerator);
        }

        [Fact]
        public void Identity_not_set_when_primary_key_property_is_enum()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityBuilder.Property("enumKey", typeof(Eenom), ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.PrimaryKey(new[] { "enumKey" }, ConfigurationSource.Convention);

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
            Assert.True(property.RequiresValueGenerator);
        }

        [Fact]
        public void Identity_is_recomputed_when_primary_key_is_changed()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var idProperty = entityBuilder.Property("Id", typeof(int), ConfigurationSource.Convention).Metadata;
            var numberProperty = entityBuilder.Property("Number", typeof(int), ConfigurationSource.Convention).Metadata;

            Assert.Same(idProperty, entityBuilder.Metadata.FindProperty("Id"));
            Assert.Same(numberProperty, entityBuilder.Metadata.FindProperty("Number"));

            Assert.Equal(ValueGenerated.OnAdd, idProperty.ValueGenerated);
            Assert.Equal(ValueGenerated.Never, numberProperty.ValueGenerated);

            entityBuilder.PrimaryKey(new List<string> { "Number" }, ConfigurationSource.Convention);

            Assert.Same(idProperty, entityBuilder.Metadata.FindProperty("Id"));
            Assert.Same(numberProperty, entityBuilder.Metadata.FindProperty("Number"));

            Assert.Equal(ValueGenerated.Never, ((IProperty)idProperty).ValueGenerated);
            Assert.Equal(ValueGenerated.OnAdd, ((IProperty)numberProperty).ValueGenerated);
        }

        [Fact]
        public void Convention_does_not_override_None_when_configured_explicitly()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            entityBuilder.Property("Id", typeof(int), ConfigurationSource.Convention)
                .ValueGenerated(ValueGenerated.Never, ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Id" }, ConfigurationSource.Convention);

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
            Assert.True(property.RequiresValueGenerator);
        }

        [Fact]
        public void Identity_is_removed_when_foreign_key_is_added()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id" };
            var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.True(property.RequiresValueGenerator);

            referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Explicit);

            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
            Assert.False(property.RequiresValueGenerator);
        }

        [Fact]
        public void Identity_is_added_when_foreign_key_is_removed_and_key_is_primary_key()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id" };
            var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.True(property.RequiresValueGenerator);

            var relationshipBuilder = referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Explicit);

            Assert.Equal(ValueGenerated.Never, ((IProperty)property).ValueGenerated);
            Assert.False(property.RequiresValueGenerator);

            referencedEntityBuilder.RemoveForeignKey(relationshipBuilder.Metadata, ConfigurationSource.Explicit);

            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.True(property.RequiresValueGenerator);
        }

        protected override InternalModelBuilder CreateInternalModelBuilder()
            => RelationalTestHelpers.Instance.CreateConventionBuilder().GetInfrastructure();
    }
}
