using ei8.Cortex.Chat.Nucleus.Domain.Model.Messages;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Library
{
    public struct TagValues
    {
        public struct Core
        {
            public const string Has_Unit = "Has^";
            public const string Unit = "Unit";
            public const string Subordination = "Subordination:Literal:Expression";
            public const string Of_Case = "Of~cs";
            public const string NominalModifier = "Nominal Modifier:Dependency:Dependent:Unit";
            public const string DirectObject = "Direct Object:Dependency:Dependent:Unit";
        }

        public struct Avatar
        {
            public const string Instantiates = "Instantiates^Avatar~do";
        }
        
        public struct ReceiptInfo
        {
            public const string Instantiates = "Instantiates^(Receipt Info)~do";

            public struct Properties
            {
                public const string MustHaveMessageInstance = "(Defines^((Have^Must~ax,(Message Instance)~do)^(Message^As~cs)~nm)~do)^";
                public const string MustHaveAvatarInstance = "(Defines^((Have^Must~ax,(Avatar Instance)~do)^(Avatar^As~cs)~nm)~do)^";
                public const string MustHaveStatus = "(Defines^((Have^Must~ax,(Status)~do)^((Receipt Status)^As~cs)~nm)~do)^";
            }
        }
        
        public struct Message
        {
            public const string Instantiates = "Instantiates^Message~do";

            public struct Properties
            {
                public const string MustHaveContent = "(Defines^((Have^Must~ax,(Content)~do)^(Avatar^Idea~cs)~nm)~do)^";
                public const string MustHaveAuthor = "(Defines^((Have^Must~ax,(Author Instance)~do)^(Avatar^As~cs)~nm)~do)^";
                public const string MustHaveCreationTimestamp = "(Defines^((Have^Must~ax,(CreationTimestamp)~do)^(DateTimeOffset^As~cs)~nm)~do)^";
                public const string MustHaveLastModificationTimestamp = "(Defines^((Have^Must~ax,(LastModificationTimestamp)~do)^(DateTimeOffset^As~cs)~nm)~do)^";
            }
        }
    }
}
