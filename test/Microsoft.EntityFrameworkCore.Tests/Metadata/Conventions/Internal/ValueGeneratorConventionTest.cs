// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Metadata.Conventions.Internal
{
    public class ValueGeneratorConventionTest
    {
        protected class SampleEntity
        {
            public int Id { get; set; }
            public int Number { get; set; }
            public string Name { get; set; }
        }

        protected class ReferencedEntity
        {
            public int Id { get; set; }
            public int SampleEntityId { get; set; }
        }

        protected enum Eenom
        {
            E,
            Nom
        }

        #region RequiresValueGenerator

        [Fact]
        public void RequiresValueGenerator_flag_is_set_for_key_properties_that_use_value_generation()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id", "Name" };

            entityBuilder.Property(properties[0], ConfigurationSource.Convention)
                .ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.True(keyProperties[0].RequiresValueGenerator);
            Assert.True(keyProperties[1].RequiresValueGenerator);

            Assert.Equal(ValueGenerated.OnAdd, keyProperties[0].ValueGenerated);
            Assert.Equal(ValueGenerated.Never, keyProperties[1].ValueGenerated);
        }

        [Fact]
        public void RequiresValueGenerator_flag_is_not_set_for_foreign_key()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "SampleEntityId" };

            referencedEntityBuilder.Property(properties[0], ConfigurationSource.Convention);

            referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Explicit);

            var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.False(keyProperties[0].RequiresValueGenerator);
            Assert.Equal(ValueGenerated.Never, keyProperties[0].ValueGenerated);
        }

        [Fact]
        public void RequiresValueGenerator_flag_is_set_for_property_which_are_not_part_of_any_foreign_key()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "SampleEntityId" };
            referencedEntityBuilder.Property(properties[0], ConfigurationSource.Convention);

            referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

            var keyBuilder = referencedEntityBuilder.PrimaryKey(new List<string> { "Id", "SampleEntityId" }, ConfigurationSource.Convention);

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.True(keyProperties[0].RequiresValueGenerator);
            Assert.False(keyProperties[1].RequiresValueGenerator);
            Assert.Equal(ValueGenerated.Never, keyProperties[0].ValueGenerated);
            Assert.Equal(ValueGenerated.Never, keyProperties[1].ValueGenerated);
        }

        [Fact]
        public void RequiresValueGenerator_flag_is_not_set_for_properties_which_are_part_of_a_foreign_key()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id", "SampleEntityId" };

            referencedEntityBuilder.Property(properties[0], ConfigurationSource.Convention)
                .ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);

            referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Explicit);

            var keyBuilder = referencedEntityBuilder.PrimaryKey(new List<string> { "SampleEntityId" }, ConfigurationSource.Convention);

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.False(keyProperties[0].RequiresValueGenerator);
            Assert.Equal(ValueGenerated.Never, keyProperties[0].ValueGenerated);
        }

        [Fact]
        public void KeyConvention_does_not_override_RequiresValueGenerator_when_configured_explicitly()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id" };

            entityBuilder.Property(properties[0], ConfigurationSource.Convention)
                .ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);

            entityBuilder.Property("Id", typeof(int), ConfigurationSource.Convention)
                .RequiresValueGenerator(false, ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.False(keyProperties[0].RequiresValueGenerator);
        }

        [Fact]
        public void RequiresValueGenerator_flag_is_turned_off_when_foreign_key_is_added()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "SampleEntityId" };

            referencedEntityBuilder.Property(properties[0], ConfigurationSource.Convention);

            var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.True(keyProperties[0].RequiresValueGenerator);

            referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

            Assert.False(keyProperties[0].RequiresValueGenerator);
            Assert.Equal(ValueGenerated.Never, keyProperties[0].ValueGenerated);
        }

        [Fact]
        public void RequiresValueGenerator_flag_is_set_when_foreign_key_is_removed()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "SampleEntityId" };

            referencedEntityBuilder.Property(properties[0], ConfigurationSource.Convention);

            var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.True(keyProperties[0].RequiresValueGenerator);

            var relationshipBuilder = referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

            Assert.False(keyProperties[0].RequiresValueGenerator);

            referencedEntityBuilder.RemoveForeignKey(relationshipBuilder.Metadata, ConfigurationSource.Convention);

            Assert.True(keyProperties[0].RequiresValueGenerator);
        }

        #endregion

        protected virtual InternalModelBuilder CreateInternalModelBuilder()
            => TestHelpers.Instance.CreateConventionBuilder().GetInfrastructure();
    }
}
