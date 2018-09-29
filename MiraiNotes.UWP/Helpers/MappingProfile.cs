﻿using AutoMapper;
using MiraiNotes.Data.Models;
using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.Models.API;
using MiraiNotes.UWP.ViewModels;

namespace MiraiNotes.UWP.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<GoogleTaskModel, TaskItemViewModel>();
            CreateMap<TaskItemViewModel, GoogleTaskModel>();

            CreateMap<GoogleTaskListModel, TaskListItemViewModel>();
            CreateMap<TaskListItemViewModel, GoogleTaskListModel>();

            CreateMap<GoogleTaskListModel, ItemModel>()
                .ForMember(d => d.ItemID, opt => opt.MapFrom(s => s.TaskListID))
                .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Title));

            CreateMap<GoogleTaskModel, ItemModel>()
                .ForMember(d => d.ItemID, opt => opt.MapFrom(s => s.TaskID))
                .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Title));

            CreateMap<TaskListItemViewModel, ItemModel>()
                .ForMember(d => d.ItemID, opt => opt.MapFrom(s => s.TaskListID))
                .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Title));

            CreateMap<TaskItemViewModel, ItemModel>()
                .ForMember(d => d.ItemID, opt => opt.MapFrom(s => s.TaskID))
                .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Title));

            CreateMap<TaskItemViewModel, TaskItemViewModel>();

            #region Database mappings
            CreateMap<GoogleTaskList, TaskListItemViewModel>()
                .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
                .ForMember(d => d.TaskListID, opt => opt.MapFrom(s => s.GoogleTaskListID))
                .ForMember(d => d.UpdatedAt, opt => opt.MapFrom(s => s.UpdatedAt));

            CreateMap<GoogleTaskList, ItemModel>()
                .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Title))
                .ForMember(d => d.ItemID, opt => opt.MapFrom(s => s.GoogleTaskListID));

            CreateMap<GoogleTask, TaskItemViewModel>()
                .ForMember(d => d.CompletedOn, opt => opt.MapFrom(s => s.CompletedOn))
                .ForMember(d => d.TaskID, opt => opt.MapFrom(s => s.GoogleTaskID))
                .ForMember(d => d.IsDeleted, opt => opt.MapFrom(s => s.IsDeleted))
                .ForMember(d => d.IsHidden, opt => opt.MapFrom(s => s.IsHidden))
                .ForMember(d => d.Notes, opt => opt.MapFrom(s => s.Notes))
                .ForMember(d => d.ParentTask, opt => opt.MapFrom(s => s.ParentTask))
                .ForMember(d => d.Position, opt => opt.MapFrom(s => s.Position))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status))
                .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
                .ForMember(d => d.ToBeCompletedOn, opt => opt.MapFrom(s => s.ToBeCompletedOn))
                .ForMember(d => d.UpdatedAt, opt => opt.MapFrom(s => s.UpdatedAt));

            CreateMap<GoogleTask, ItemModel>()
            .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.ItemID, opt => opt.MapFrom(s => s.GoogleTaskID));

            CreateMap<GoogleUser, GoogleUserViewModel>()
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email))
                .ForMember(d => d.Fullname, opt => opt.MapFrom(s => s.Fullname))
                .ForMember(d => d.PictureUrl, opt => opt.MapFrom(s => s.PictureUrl))
                .ForMember(d => d.UserID, opt => opt.MapFrom(s => s.GoogleUserID));
            #endregion
        }
    }
}
