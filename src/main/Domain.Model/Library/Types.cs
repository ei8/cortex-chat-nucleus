using System.ComponentModel.DataAnnotations;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Library
{
    public enum ExternalReferenceKey
    {
        [Display(Name = "Direct Object:Dependency:Dependent:Unit")]
        DirectObject,
        [Display(Name = "Has")]
        Has,
        [Display(Name = "Instantiates")]
        Instantiates,
        [Display(Name = "Idea")]
        Idea,
        [Display(Name = "Nominal Modifier:Dependency:Dependent:Unit")]
        NominalModifier,
        [Display(Name = "Of")]
        Of,
        [Display(Name = "Case")]
        Case,
        [Display(Name = "Simple:Literal:Expression")]
        Simple,
        [Display(Name = "Subordination:Literal:Expression")]
        Subordination,
        [Display(Name = "Coordination:Literal:Expression")]
        Coordination,
        [Display(Name = "Unit")]
        Unit
    }
}