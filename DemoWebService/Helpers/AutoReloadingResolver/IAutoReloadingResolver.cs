using IP2Country;

namespace DemoWebService.Helpers
{
    public interface IAutoReloadingResolver : IIP2CountryBatchResolver
    {
        void Initialize();
    }
}