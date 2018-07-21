using AutoMapper;
using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.Models.API;

namespace MiraiNotes.UWP.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<GoogleTaskModel, TaskModel>();
            CreateMap<TaskModel, GoogleTaskModel>();

            CreateMap<GoogleTaskListModel, ItemModel>()
                .ForMember(d => d.ItemID, opt => opt.MapFrom(s => s.TaskListID))
                .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Title));

            CreateMap<GoogleTaskModel, ItemModel>()
                .ForMember(d => d.ItemID, opt => opt.MapFrom(s => s.TaskID))
                .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Title));

            CreateMap<GoogleTaskListModel, TaskListModel>();
            CreateMap<TaskListModel, GoogleTaskListModel>();
        }
    }
}
