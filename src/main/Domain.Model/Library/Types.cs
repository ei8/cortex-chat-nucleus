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
        [Display(Name = "(Defines^((Have^Must~ax,(Author Instance)~do)^(Avatar^As~cs)~nm)~do)^")]
        Message_MustHaveAuthor,
        [Display(Name = "(Defines^((Have^Must~ax,(Content)~do)^(Avatar^Idea~cs)~nm)~do)^")]
        Message_MustHaveContent,
        [Display(Name = "(Defines^((Have^Must~ax,(CreationTimestamp)~do)^(DateTimeOffset^As~cs)~nm)~do)^")]
        Message_MustHaveCreationTimestamp,
        [Display(Name = "(Defines^((Have^Must~ax,(LastModificationTimestamp)~do)^(DateTimeOffset^As~cs)~nm)~do)^")]
        Message_MustHaveLastModificationTimestamp,
        [Display(Name = "Nominal Modifier:Dependency:Dependent:Unit")]
        NominalModifier,
        [Display(Name = "Of")]
        Of,
        [Display(Name = "Case")]
        Case,
        [Display(Name = "(Defines^((Have^Must~ax,(Avatar Instance)~do)^(Avatar^As~cs)~nm)~do)^")]
        ReceiptInfo_MustHaveAvatarInstance,
        [Display(Name = "(Defines^((Have^Must~ax,(Message Instance)~do)^(Message^As~cs)~nm)~do)^")]
        ReceiptInfo_MustHaveMessageInstance,
        [Display(Name = "(Defines^((Have^Must~ax,(Status)~do)^((Receipt Status)^As~cs)~nm)~do)^")]
        ReceiptInfo_MustHaveStatus,
        [Display(Name = "Read")]
        Read,
        [Display(Name = "Simple:Literal:Expression")]
        Simple,
        [Display(Name = "Subordination:Literal:Expression")]
        Subordination,
        [Display(Name = "Coordination:Literal:Expression")]
        Coordination,
        [Display(Name = "Unit")]
        Unit,
        [Display(Name = "Unread")]
        Unread
    }
}