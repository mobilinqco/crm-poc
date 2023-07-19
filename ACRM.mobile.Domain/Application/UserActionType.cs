using System;
namespace ACRM.mobile.Domain.Application
{
    public enum UserActionType
    {
        SyncFull, SyncConfig, SyncResource, SyncData, SyncIncremental,
        RecordLists, Search, ShowRecord, Menu, Button, NewOrEdit, RecordSelector,
        NotImplemented, Calendar, WebContentView, SerialEntryListing, OpenURL, ImageView,
        DocumentView, ClientReport, QuestionnaireEdit, NavigateBack, NavigateHome,
        Query, RecordSwitch, ModifyRecord
    }
}
