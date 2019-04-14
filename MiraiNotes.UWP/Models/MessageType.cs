﻿namespace MiraiNotes.UWP.Models
{
    public enum MessageType
    {
        OPEN_PANE = 0,
        NAVIGATIONVIEW_SELECTION_CHANGED = 1,

        TASK_LIST_ADDED = 100,
        NEW_TASK = 200,
        TASK_SAVED = 201,
        TASK_DELETED_FROM_PANE_FRAME = 202,
        TASK_DELETED_FROM_CONTENT_FRAME = 203,
        SUBTASK_DELETED_FROM_PANE_FRAME = 204,
        TASK_STATUS_CHANGED_FROM_PANE_FRAME = 205,
        TASK_STATUS_CHANGED_FROM_CONTENT_FRAME = 206,
        ON_FULL_SYNC = 207,
        DEFAULT_TASK_SORT_ORDER_CHANGED = 208,
        DEFAULT_TASK_LIST_SORT_ORDER_CHANGED = 209,
        CURRENT_USER_CHANGED = 210,

        SHOW_CONTENT_FRAME_PROGRESS_RING = 301,
        SHOW_PANE_FRAME_PROGRESS_RING = 302,
        SHOW_MAIN_PROGRESS_BAR = 303,
        SHOW_IN_APP_NOTIFICATION = 304
    }
}
