﻿namespace MiraiNotes.UWP.Models
{
    public enum MessageType
    {
        OPEN_PANE = 0,
        NAVIGATIONVIEW_SELECTION_CHANGED = 1,

        TASK_LIST_ADDED = 100,
        NEW_TASK = 200,
        TASK_SAVED = 201,
        TASK_DELETED = 202,
        TASK_DELETED_FROM_CONTENT_FRAME = 203,
        SUBTASK_DELETED = 204,
        TASK_STATUS_CHANGED = 205,

        SHOW_CONTENT_FRAME_PROGRESS_RING = 301,
        SHOW_PANE_FRAME_PROGRESS_RING = 302,
    }
}
