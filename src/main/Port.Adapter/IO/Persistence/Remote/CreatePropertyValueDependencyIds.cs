using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote
{
    public class CreatePropertyValueDependencyIds
    {
        public CreatePropertyValueDependencyIds(
            Guid unit,
            Guid subordination,
            Guid of_Case,
            Guid nominalModifier,
            Guid directObject,
            Guid has_Unit
            )
        {
            AssertionConcern.AssertArgumentValid(g => g != Guid.Empty, unit, $"Value cannot be equal to '{Guid.Empty}'", nameof(unit));
            AssertionConcern.AssertArgumentValid(g => g != Guid.Empty, subordination, $"Value cannot be equal to '{Guid.Empty}'", nameof(subordination));
            AssertionConcern.AssertArgumentValid(g => g != Guid.Empty, of_Case, $"Value cannot be equal to '{Guid.Empty}'", nameof(of_Case));
            AssertionConcern.AssertArgumentValid(g => g != Guid.Empty, nominalModifier, $"Value cannot be equal to '{Guid.Empty}'", nameof(nominalModifier));
            AssertionConcern.AssertArgumentValid(g => g != Guid.Empty, directObject, $"Value cannot be equal to '{Guid.Empty}'", nameof(directObject));
            AssertionConcern.AssertArgumentValid(g => g != Guid.Empty, has_Unit, $"Value cannot be equal to '{Guid.Empty}'", nameof(has_Unit));

            this.Unit = unit;
            this.Subordination = subordination;
            this.Of_Case = of_Case;
            this.NominalModifier = nominalModifier;
            this.DirectObject = directObject;
            this.Has_Unit = has_Unit;
        }
        public Guid Unit { get; private set; }
        public Guid Subordination { get; private set; }
        public Guid Of_Case { get; private set; }
        public Guid NominalModifier { get; private set; }
        public Guid DirectObject { get; private set; }
        public Guid Has_Unit { get; private set; }
    }
}
