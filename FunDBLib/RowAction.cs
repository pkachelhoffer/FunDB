using System;

namespace FunDBLib
{
    internal struct RowAction
    {
        public EnumRowActionType RowActionType { get; set; }

        public RowAction(EnumRowActionType rowActionType)
        {
            RowActionType = rowActionType;
        }
    }
}