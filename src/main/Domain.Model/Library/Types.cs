using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;
using System.Threading;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Library
{
    public enum ExternalReferenceId
    {
        [Display(Name = "Direct Object:Dependency:Dependent:Unit")]
        DirectObject,
        [Display(Name = "Has^")]
        Has_Unit,
        [Display(Name = "Instantiates^Avatar~do")]
        InstantiatesAvatar,
        [Display(Name = "Instantiates^(Date Time Offset)~do")]
        InstantiatesDateTimeOffset,
        [Display(Name = "Instantiates^Idea~do")]
        InstantiatesIdea,
        [Display(Name = "Instantiates^Message~do")]
        InstantiatesMessage,
        [Display(Name = "Instantiates^(Receipt Info)~do")]
        InstantiatesReceiptInfo,
        [Display(Name = "Instantiates^(Receipt Status)~do")]
        InstantiatesReceiptStatus,
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
        [Display(Name = "Of~cs")]
        Of_Case,
        [Display(Name = "(Defines^((Have^Must~ax,(Avatar Instance)~do)^(Avatar^As~cs)~nm)~do)^")]
        ReceiptInfo_MustHaveAvatarInstance,
        [Display(Name = "(Defines^((Have^Must~ax,(Message Instance)~do)^(Message^As~cs)~nm)~do)^")]
        ReceiptInfo_MustHaveMessageInstance,
        [Display(Name = "(Defines^((Have^Must~ax,(Status)~do)^((Receipt Status)^As~cs)~nm)~do)^")]
        ReceiptInfo_MustHaveStatus,
        [Display(Name = "Read")]
        Read,
        [Display(Name = "Subordination:Literal:Expression")]
        Subordination,
        [Display(Name = "Unit")]
        Unit,
        [Display(Name = "Unread")]
        Unread
    }
}