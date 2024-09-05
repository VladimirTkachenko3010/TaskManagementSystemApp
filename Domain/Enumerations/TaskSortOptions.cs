using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enumerations
{
    public enum TaskSortOptions
    {
        [EnumMember(Value = "DueDate")]
        DueDate,

        [EnumMember(Value = "Priority")]
        Priority
    }
}
