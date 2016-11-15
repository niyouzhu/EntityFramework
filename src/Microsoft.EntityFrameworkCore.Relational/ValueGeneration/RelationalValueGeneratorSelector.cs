// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    public class RelationalValueGeneratorSelector : ValueGeneratorSelector
    {
        public RelationalValueGeneratorSelector(
            [NotNull] IValueGeneratorCache cache,
            [NotNull] IRelationalAnnotationProvider relationalExtensions)
            : base(cache)
        {
            Check.NotNull(relationalExtensions, nameof(relationalExtensions));

            RelationalExtensions = relationalExtensions;
        }

        protected virtual IRelationalAnnotationProvider RelationalExtensions { get; }
    }
}
