using AutoMapper;
using AutoMapper.Configuration;
using GalaSoft.MvvmLight.Ioc;
using System.ComponentModel;

namespace MiraiNotes.UWP.Helpers
{
    public class MapperProvider
    {
        private readonly ISimpleIoc _container;

        public MapperProvider(ISimpleIoc container)
        {
            _container = container;
        }

        public IMapper GetMapper()
        {
            var mce = new MapperConfigurationExpression();
            mce.ConstructServicesUsing(_container.GetInstance);

            mce.AddProfiles(typeof(MappingProfile).Assembly);

            var mc = new MapperConfiguration(mce);
            mc.AssertConfigurationIsValid();

            IMapper m = new Mapper(mc, t => _container.GetInstance(t));

            return m;
        }
    }
}
