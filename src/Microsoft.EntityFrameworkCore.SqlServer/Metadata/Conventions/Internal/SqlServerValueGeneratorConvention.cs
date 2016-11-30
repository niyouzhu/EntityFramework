// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class SqlServerValueGeneratorConvention : RelationalValueGeneratorConvention
    {
        public SqlServerValueGeneratorConvention([NotNull] IRelationalAnnotationProvider annotationProvider)
            : base(annotationProvider)
        {
        }

        public override Annotation Apply(InternalPropertyBuilder propertyBuilder, string name, Annotation annotation, Annotation oldAnnotation)
        {
            if (name == SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy)
            {
                propertyBuilder.ValueGenerated(GetValueGenerated(propertyBuilder.Metadata), ConfigurationSource.Convention);
                propertyBuilder.RequiresValueGenerator(GetRequiresValueGenerator(propertyBuilder.Metadata), ConfigurationSource.Convention);
                return annotation;
            }

            return base.Apply(propertyBuilder, name, annotation, oldAnnotation);
        }

        protected override ValueGenerated? GetValueGenerated(Property property)
        {
            var valueGenerated = base.GetValueGenerated(property);
            if (valueGenerated != null)
            {
                return valueGenerated;
            }

            var valueGenerationStrategy = property.SqlServer().GetSqlServerValueGenerationStrategy(fallbackToModel: false);
            return valueGenerationStrategy == SqlServerValueGenerationStrategy.IdentityColumn
                ? ValueGenerated.OnAdd
                : valueGenerationStrategy == SqlServerValueGenerationStrategy.SequenceHiLo
                    ? ValueGenerated.Never
                    : (ValueGenerated?)null;
        }

        protected override bool? GetRequiresValueGenerator(Property property)
        {
            var requiresValueGenerator = base.GetRequiresValueGenerator(property);
            if (requiresValueGenerator != null)
            {
                return requiresValueGenerator;
            }

            var valueGenerationStrategy = property.SqlServer().GetSqlServerValueGenerationStrategy(fallbackToModel: false);
            return valueGenerationStrategy == SqlServerValueGenerationStrategy.SequenceHiLo
                ? true
                : (bool?)null;
        }
    }
}
